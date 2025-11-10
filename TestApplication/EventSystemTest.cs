using System;
using System.Threading;
using bHapticsLib;

namespace TestApplication
{
    /// <summary>
    /// Example test for the bHapticsLib event system
    /// </summary>
    public class EventSystemTest
    {
        private static int deviceEventCount = 0;
        private static int connectionEventCount = 0;

        public static void RunTest()
        {
            Console.WriteLine("=== bHapticsLib Event System Test ===\n");

            // Subscribe to all events
            bHapticsManager.DeviceStatusChanged += OnDeviceStatusChanged;
            bHapticsManager.ConnectionEstablished += OnConnectionEstablished;
            bHapticsManager.ConnectionLost += OnConnectionLost;
            bHapticsManager.StatusChanged += OnStatusChanged;

            Console.WriteLine("Subscribed to all events");
            Console.WriteLine("Connecting to bHaptics Player...\n");

            // Connect
            bool connected = bHapticsManager.Connect("EventTest", "Event System Test", 
                tryToReconnect: true, maxRetries: 5);

            Console.WriteLine($"Connect() returned: {connected}");
            Console.WriteLine($"Initial Status: {bHapticsManager.Status}\n");

            // Wait for events
            Console.WriteLine("Waiting for connection and device events...");
            Console.WriteLine("(Try connecting/disconnecting devices in bHaptics Player)\n");

            // Run for 30 seconds
            for (int i = 0; i < 30; i++)
            {
                Thread.Sleep(1000);
                
                if (i % 5 == 0 && i > 0)
                {
                    Console.WriteLine($"[{i}s] Status: {bHapticsManager.Status}, " +
                                    $"Device Events: {deviceEventCount}, " +
                                    $"Connection Events: {connectionEventCount}");
                }
            }

            Console.WriteLine("\n=== Test Complete ===");
            Console.WriteLine($"Total Device Status Changes: {deviceEventCount}");
            Console.WriteLine($"Total Connection Events: {connectionEventCount}");

            // Cleanup
            Console.WriteLine("\nUnsubscribing and disconnecting...");
            bHapticsManager.DeviceStatusChanged -= OnDeviceStatusChanged;
            bHapticsManager.ConnectionEstablished -= OnConnectionEstablished;
            bHapticsManager.ConnectionLost -= OnConnectionLost;
            bHapticsManager.StatusChanged -= OnStatusChanged;

            bHapticsManager.Disconnect();
            Console.WriteLine("Disconnected.\n");
        }

        private static void OnDeviceStatusChanged(object sender, DeviceStatusChangedEventArgs e)
        {
            deviceEventCount++;
            string status = e.IsConnected ? "? CONNECTED" : "? DISCONNECTED";
            Console.WriteLine($"[DEVICE] {e.Position} {status} at {e.Timestamp:HH:mm:ss.fff}");
        }

        private static void OnConnectionEstablished(object sender, ConnectionStatusChangedEventArgs e)
        {
            connectionEventCount++;
            Console.WriteLine($"[CONNECTION] ? ESTABLISHED: {e.PreviousStatus} ? {e.NewStatus} at {e.Timestamp:HH:mm:ss.fff}");
        }

        private static void OnConnectionLost(object sender, ConnectionStatusChangedEventArgs e)
        {
            connectionEventCount++;
            Console.WriteLine($"[CONNECTION] ? LOST: {e.PreviousStatus} ? {e.NewStatus} at {e.Timestamp:HH:mm:ss.fff}");
        }

        private static void OnStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            Console.WriteLine($"[STATUS] {e.PreviousStatus} ? {e.NewStatus} at {e.Timestamp:HH:mm:ss.fff}");
        }
    }
}
