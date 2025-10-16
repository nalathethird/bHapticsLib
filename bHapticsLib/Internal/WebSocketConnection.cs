using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using bHapticsLib.Internal.Models.Connection;
using bHapticsLib.Internal.SimpleJSON;

namespace bHapticsLib.Internal
{
    internal class WebSocketConnection : IDisposable
    {
        private readonly bHapticsConnection _parent;
        private ClientWebSocket _socket;
        private CancellationTokenSource _cancellationSource;
        private Task _receiveTask;
        private Task _reconnectTask;
        
        private volatile bool _isConnected;
        private volatile bool _isDisposed;
        private int _retryCount;
        private Exception _lastError;
        private string _connectionLog;
        
        private const int RetryDelayMs = 3000;
        private const int ReceiveBufferSize = 8192;
        private const int ConnectionTimeoutMs = 10000;
        
        internal bool FirstTry { get; set; }
        internal PlayerResponse LastResponse { get; private set; }

        internal WebSocketConnection(bHapticsConnection parent)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _cancellationSource = new CancellationTokenSource();
            FirstTry = true;
            _connectionLog = "[WebSocketConnection] Created\n";
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _connectionLog += "[Dispose] Starting cleanup\n";
            _isDisposed = true;
            
            try
            {
                _cancellationSource?.Cancel();
                _connectionLog += "[Dispose] Cancellation requested\n";
            }
            catch (Exception ex)
            {
                _connectionLog += $"[Dispose] Cancel error: {ex.Message}\n";
            }

            try
            {
                if (_socket?.State == WebSocketState.Open)
                {
                    _connectionLog += "[Dispose] Closing WebSocket\n";
                    Task closeTask = _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing", CancellationToken.None);
                    if (!closeTask.Wait(5000))
                    {
                        _connectionLog += "[Dispose] Close timeout\n";
                    }
                }
            }
            catch (Exception ex)
            {
                _connectionLog += $"[Dispose] Close error: {ex.Message}\n";
            }

            try
            {
                _socket?.Dispose();
            }
            catch { }

            try
            {
                _cancellationSource?.Dispose();
            }
            catch { }

            _isConnected = false;
            LastResponse = null;
            _connectionLog += "[Dispose] Complete\n";
        }

        internal void TryConnect()
        {
            _connectionLog += "[TryConnect] Called\n";
            
            if (_isDisposed)
            {
                _connectionLog += "[TryConnect] Aborted - disposed\n";
                return;
            }

            try
            {
                _connectionLog += "[TryConnect] Starting connection task\n";
                
                Task connectTask = Task.Run(async () => await ConnectAsync());
                
                _connectionLog += "[TryConnect] Waiting for connection...\n";
                
                if (!connectTask.Wait(ConnectionTimeoutMs))
                {
                    _lastError = new TimeoutException($"Connection to bHaptics Player timed out after {ConnectionTimeoutMs}ms");
                    _connectionLog += $"[TryConnect] TIMEOUT after {ConnectionTimeoutMs}ms\n";
                    _isConnected = false;
                }
                else
                {
                    _connectionLog += $"[TryConnect] Task completed. Connected={_isConnected}\n";
                }
            }
            catch (Exception ex)
            {
                _lastError = ex;
                _isConnected = false;
                _connectionLog += $"[TryConnect] EXCEPTION: {ex.GetType().Name}: {ex.Message}\n";
            }
        }

        private async Task ConnectAsync()
        {
            _connectionLog += "[ConnectAsync] Starting\n";
            
            if (_isDisposed || _cancellationSource.IsCancellationRequested)
            {
                _connectionLog += "[ConnectAsync] Aborted - disposed or cancelled\n";
                return;
            }

            try
            {
                if (_socket != null)
                {
                    try
                    {
                        _connectionLog += "[ConnectAsync] Disposing old socket\n";
                        _socket.Dispose();
                    }
                    catch { }
                    _socket = null;
                }

                _connectionLog += "[ConnectAsync] Creating ClientWebSocket\n";
                _socket = new ClientWebSocket();
                _socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);

                string url = $"ws://{_parent.IPAddress}:{bHapticsConnection.Port}/{bHapticsConnection.Endpoint}?app_id={_parent.ID}&app_name={_parent.Name}";
                Uri uri = new Uri(url);
                
                _connectionLog += $"[ConnectAsync] URL: {url}\n";
                _connectionLog += "[ConnectAsync] Calling ConnectAsync...\n";

                await _socket.ConnectAsync(uri, _cancellationSource.Token).ConfigureAwait(false);

                _connectionLog += $"[ConnectAsync] ConnectAsync completed. State={_socket.State}\n";

                if (_socket.State == WebSocketState.Open)
                {
                    _isConnected = true;
                    _retryCount = 0;
                    _lastError = null;
                    
                    _connectionLog += "[ConnectAsync] CONNECTED!\n";
                    
                    _parent.QueueRegisterCache();

                    _connectionLog += "[ConnectAsync] Starting receive loop\n";
                    _receiveTask = Task.Run(async () => await ReceiveLoopAsync());

                    if (_parent.TryToReconnect)
                    {
                        _connectionLog += "[ConnectAsync] Starting reconnect monitor\n";
                        _reconnectTask = Task.Run(async () => await MonitorConnectionAsync());
                    }
                }
                else
                {
                    _isConnected = false;
                    _lastError = new InvalidOperationException($"WebSocket in unexpected state: {_socket.State}");
                    _connectionLog += $"[ConnectAsync] Unexpected state: {_socket.State}\n";
                }
            }
            catch (Exception ex)
            {
                _isConnected = false;
                _lastError = ex;
                _connectionLog += $"[ConnectAsync] EXCEPTION: {ex.GetType().Name}\n";
                _connectionLog += $"[ConnectAsync] Message: {ex.Message}\n";
                if (ex.InnerException != null)
                {
                    _connectionLog += $"[ConnectAsync] Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}\n";
                }
            }
        }

        private async Task ReceiveLoopAsync()
        {
            byte[] buffer = new byte[ReceiveBufferSize];
            StringBuilder messageBuilder = new StringBuilder();

            try
            {
                _connectionLog += "[ReceiveLoop] Started\n";
                
                while (_socket != null && _socket.State == WebSocketState.Open && !_cancellationSource.IsCancellationRequested)
                {
                    messageBuilder.Clear();
                    WebSocketReceiveResult result;

                    try
                    {
                        do
                        {
                            result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationSource.Token)
                                .ConfigureAwait(false);

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                _connectionLog += "[ReceiveLoop] Close message received\n";
                                await HandleClosedConnection();
                                return;
                            }

                            if (result.MessageType == WebSocketMessageType.Text)
                            {
                                string chunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                                messageBuilder.Append(chunk);
                            }
                        }
                        while (!result.EndOfMessage);

                        if (messageBuilder.Length > 0)
                            ProcessReceivedMessage(messageBuilder.ToString());
                    }
                    catch (OperationCanceledException)
                    {
                        _connectionLog += "[ReceiveLoop] Cancelled\n";
                        break;
                    }
                    catch (WebSocketException ex)
                    {
                        _connectionLog += $"[ReceiveLoop] WebSocket error: {ex.Message}\n";
                        await HandleClosedConnection();
                        break;
                    }
                }
                
                _connectionLog += "[ReceiveLoop] Exited\n";
            }
            catch (Exception ex)
            {
                _lastError = ex;
                _connectionLog += $"[ReceiveLoop] Exception: {ex.GetType().Name}: {ex.Message}\n";
                await HandleClosedConnection();
            }
        }

        private void ProcessReceivedMessage(string message)
        {
            try
            {
                if (LastResponse == null)
                    LastResponse = new PlayerResponse();

                JSONNode node = JSON.Parse(message);
                if (node?.IsObject == true)
                    LastResponse.m_Dict = node.AsObject.m_Dict;
            }
            catch (Exception ex)
            {
                _lastError = ex;
            }
        }

        private async Task HandleClosedConnection()
        {
            _connectionLog += "[HandleClosed] Connection closed\n";
            _isConnected = false;
            LastResponse = null;

            try
            {
                if (_socket?.State == WebSocketState.Open || _socket?.State == WebSocketState.CloseReceived)
                {
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None)
                        .ConfigureAwait(false);
                }
            }
            catch { }
        }

        private async Task MonitorConnectionAsync()
        {
            _connectionLog += "[Monitor] Started\n";
            
            while (!_isDisposed && !_cancellationSource.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(RetryDelayMs, _cancellationSource.Token).ConfigureAwait(false);

                    if (!IsConnected() && _parent.TryToReconnect && !_isDisposed)
                    {
                        if (_socket?.State == WebSocketState.Connecting || _socket?.State == WebSocketState.CloseSent)
                            continue;

                        if (_parent.MaxRetries > 0)
                        {
                            if (_retryCount >= _parent.MaxRetries)
                            {
                                _connectionLog += "[Monitor] Max retries reached\n";
                                _parent.EndInit();
                                break;
                            }
                            _retryCount++;
                        }

                        _connectionLog += $"[Monitor] Retry attempt {_retryCount}\n";
                        await ConnectAsync().ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    _connectionLog += "[Monitor] Cancelled\n";
                    break;
                }
                catch (Exception ex)
                {
                    _lastError = ex;
                    _connectionLog += $"[Monitor] Exception: {ex.Message}\n";
                }
            }
            
            _connectionLog += "[Monitor] Exited\n";
        }

        internal bool IsConnected()
        {
            return _isConnected && 
                   _socket != null && 
                   _socket.State == WebSocketState.Open && 
                   !_isDisposed;
        }

        internal string GetLastError()
        {
            return _lastError?.ToString() ?? "No error information available";
        }

        internal string GetConnectionLog()
        {
            return _connectionLog ?? "No log available";
        }

        internal void Send(JSONObject jsonNode)
        {
            if (jsonNode == null)
                return;
            Send(jsonNode.ToString());
        }

        internal void Send(string message)
        {
            if (string.IsNullOrEmpty(message) || !IsConnected())
                return;

            Task.Run(async () =>
            {
                try
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(message);
                    ArraySegment<byte> segment = new ArraySegment<byte>(bytes);

                    await _socket.SendAsync(segment, WebSocketMessageType.Text, true, _cancellationSource.Token)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _lastError = ex;
                    _connectionLog += $"[Send] Error: {ex.Message}\n";
                }
            });
        }
    }
}