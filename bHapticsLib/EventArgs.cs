using System;

namespace bHapticsLib
{
    /// <summary>
    /// Event arguments for device status changes
    /// </summary>
    public class DeviceStatusChangedEventArgs : EventArgs
    {
        /// <summary>Device position that changed</summary>
        public PositionID Position { get; set; }

        /// <summary>True if device connected, false if disconnected</summary>
        public bool IsConnected { get; set; }

        /// <summary>Timestamp when the change was detected</summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Creates a new DeviceStatusChangedEventArgs
        /// </summary>
        /// <param name="position">Device position</param>
        /// <param name="isConnected">Connection status</param>
        public DeviceStatusChangedEventArgs(PositionID position, bool isConnected)
        {
            Position = position;
            IsConnected = isConnected;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>String representation of the event</summary>
        public override string ToString()
        {
            string status = IsConnected ? "Connected" : "Disconnected";
            return $"[{Timestamp:HH:mm:ss}] {Position} {status}";
        }
    }

    /// <summary>
    /// Event arguments for connection state changes
    /// </summary>
    public class ConnectionStatusChangedEventArgs : EventArgs
    {
        /// <summary>Previous connection status</summary>
        public bHapticsStatus PreviousStatus { get; set; }

        /// <summary>New connection status</summary>
        public bHapticsStatus NewStatus { get; set; }

        /// <summary>Timestamp when the change occurred</summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Creates a new ConnectionStatusChangedEventArgs
        /// </summary>
        /// <param name="previousStatus">Previous status</param>
        /// <param name="newStatus">New status</param>
        public ConnectionStatusChangedEventArgs(bHapticsStatus previousStatus, bHapticsStatus newStatus)
        {
            PreviousStatus = previousStatus;
            NewStatus = newStatus;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>String representation of the event</summary>
        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {PreviousStatus} ? {NewStatus}";
        }
    }
}
