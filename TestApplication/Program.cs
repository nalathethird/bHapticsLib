using bHapticsLib;
using System;
using System.IO;
using System.Threading;

namespace TestApplication
{
    public class Program
    {
        private static byte[] TestPacket = new byte[bHapticsManager.MaxMotorsPerDotPoint] { 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static HapticPattern testFeedback;
        private static HapticPattern testFeedbackSwapped;
        private static bHapticsConnection Connection;

        private static void Main()
        {
            // Show menu
            Console.WriteLine("=== bHapticsLib Test Application ===");
            Console.WriteLine();
            Console.WriteLine("Choose test mode:");
            Console.WriteLine("1. Interactive Test (original)");
            Console.WriteLine("2. Data Debug Test (see sent/received data)");
            Console.WriteLine("3. Event System Test");
            Console.WriteLine("4. WebSocket Data Inspector (ALL data types & structures)");
            Console.WriteLine("5. Raw WebSocket Logger (see JSON structures)");
            Console.WriteLine("6. ALL DEVICE TEST (vibrate every connected device)");
            Console.WriteLine("7. COMPLETE DATA DUMP (EVERY WebSocket field & value)");
            Console.WriteLine("8. MessagePack Integration Test (NEW - test high-performance MessagePack input)");
            Console.WriteLine();
            Console.Write("Enter choice (1-8): ");
            
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "2":
                    DataDebugTest.Run();
                    return;
                
                case "3":
                    EventSystemTest.RunTest();
                    return;

                case "4":
                    WebSocketDataInspector.Run();
                    return;

                case "5":
                    RawWebSocketLogger.Run();
                    return;

                case "6":
                    AllDeviceTest.Run();
                    return;

                case "7":
                    CompleteDataDump.Run();
                    return;

                case "8":
                    MessagePackTest.Run();
                    return;

                case "1":
                default:
                    RunInteractiveTest();
                    return;
            }
        }

        private static void RunInteractiveTest()
        {
            string testFeedbackPath = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "testfeedback.tact");
            testFeedback = HapticPattern.LoadFromFile("testfeedback", testFeedbackPath);

            for (int i = 0; i < 30; i++)
                testFeedbackSwapped = HapticPattern.LoadSwappedFromFile($"testFeedbackSwapped{i}", testFeedbackPath);

            Console.WriteLine("Initializing...");
            
            bHapticsManager.Connect("bHapticsLib", "TestApplication", maxRetries: 0);

            Thread.Sleep(1000);

            Connection = new bHapticsConnection("bHapticsLib2", "AdditionalConnection", maxRetries: 0);
            Connection.BeginInit();

            Console.WriteLine(Connection.Status);

            Console.WriteLine();

            Console.WriteLine($"Press 0 for {nameof(bHapticsManager.Status)}");
            Console.WriteLine();

            Console.WriteLine($"Press 2 for {nameof(bHapticsManager.GetConnectedDeviceCount)}()");
            Console.WriteLine($"Press 3 for {nameof(bHapticsManager.IsAnyDevicesConnected)}()");
            Console.WriteLine();

            Console.WriteLine($"Press 4 for {nameof(bHapticsManager.IsPlayingAny)}()");
            Console.WriteLine($"Press 5 for {nameof(bHapticsManager.IsAnyMotorActive)}({nameof(PositionID)}.{nameof(PositionID.Vest)})");
            Console.WriteLine($"Press 6 for {nameof(bHapticsManager.IsAnyMotorActive)}({nameof(PositionID)}.{nameof(PositionID.VestFront)})");
            Console.WriteLine($"Press 7 for {nameof(bHapticsManager.IsAnyMotorActive)}({nameof(PositionID)}.{nameof(PositionID.VestBack)})");
            Console.WriteLine();

            Console.WriteLine($"Press I for {nameof(bHapticsManager.GetDeviceStatus)}({nameof(PositionID)}.{nameof(PositionID.Vest)})");
            Console.WriteLine($"Press O for {nameof(bHapticsManager.GetDeviceStatus)}({nameof(PositionID)}.{nameof(PositionID.VestFront)})");
            Console.WriteLine($"Press P for {nameof(bHapticsManager.GetDeviceStatus)}({nameof(PositionID)}.{nameof(PositionID.VestBack)})");
            Console.WriteLine();

            Console.WriteLine($"Press N for {nameof(testFeedbackSwapped)}.{nameof(testFeedbackSwapped.Play)}()");
            Console.WriteLine();

            Console.WriteLine($"Press M for {nameof(testFeedbackSwapped)}.{nameof(testFeedbackSwapped.IsRegistered)}()");
            Console.WriteLine();

            Console.WriteLine($"Press J for {nameof(testFeedbackSwapped)}.{nameof(testFeedbackSwapped.IsPlaying)}()");
            Console.WriteLine();

            Console.WriteLine($"Press NUMPAD-1 for {nameof(bHapticsManager.IsDeviceConnected)}({nameof(PositionID)}.{nameof(PositionID.Vest)})");
            Console.WriteLine();

            Console.WriteLine($"Press NUMPAD-2 for {nameof(testFeedback)}.{nameof(testFeedback.Play)}()");
            Console.WriteLine();

            Console.WriteLine($"Press NUMPAD-3 for {nameof(testFeedback)}.{nameof(testFeedback.IsRegistered)}()");
            Console.WriteLine();

            Console.WriteLine($"Press NUMPAD-4 for {nameof(testFeedback)}.{nameof(testFeedback.IsPlaying)}()");
            Console.WriteLine();

            Console.WriteLine($"Press NUMPAD-5 for {nameof(bHapticsManager.Play)}(\"testPlayFront\", 1000, {nameof(PositionID)}.{nameof(PositionID.HandLeft)}, {nameof(TestPacket)} )");
            Console.WriteLine($"Press NUMPAD-6 for {nameof(bHapticsManager.Play)}(\"testPlayBack\", 1000, {nameof(PositionID)}.{nameof(PositionID.ArmRight)}, {nameof(TestPacket)} )");
            Console.WriteLine();

            Console.WriteLine($"Press NUMPAD-7 for {nameof(bHapticsManager.PlayMirrored)}(\"testPlayFront\", 1000, {nameof(PositionID)}.{nameof(PositionID.HandLeft)}, {nameof(TestPacket)}, {nameof(MirrorDirection)}.{nameof(MirrorDirection.Both)} )");
            Console.WriteLine($"Press NUMPAD-8 for {nameof(bHapticsManager.PlayMirrored)}(\"testPlayBack\", 1000, {nameof(PositionID)}.{nameof(PositionID.ArmRight)}, {nameof(TestPacket)}, {nameof(MirrorDirection)}.{nameof(MirrorDirection.Both)}  )");
            Console.WriteLine();

            // NEW: Add PlayMotors test
            Console.WriteLine($"Press NUMPAD-9 for {nameof(bHapticsManager.PlayMotors)}(\"motorTest\", 1000, {nameof(PositionID)}.{nameof(PositionID.Vest)}, byte[20] with motors 0,5,10 active)");
            Console.WriteLine();

            Console.WriteLine("Press Enter to Disconnect.");
            Console.WriteLine();

            while (true)
            {
                if (KeyCheck())
                    break;
                else
                    Thread.Sleep(1);
            }

            if (!Connection.EndInit() || !bHapticsManager.Disconnect())
                Console.WriteLine("Failed to Disconnect!");
        }

        static bool KeyCheck()
        {
            if (!Console.KeyAvailable)
                return false;

            ConsoleKeyInfo key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    return true;


                case ConsoleKey.D0:
                    Console.WriteLine($"{nameof(bHapticsManager.Status)}: {bHapticsManager.Status}");
                    goto default;


                case ConsoleKey.D2:
                    Console.WriteLine($"{nameof(bHapticsManager.GetConnectedDeviceCount)}(): {bHapticsManager.GetConnectedDeviceCount()}");
                    goto default;
                case ConsoleKey.D3:
                    Console.WriteLine($"{nameof(bHapticsManager.IsAnyDevicesConnected)}(): {bHapticsManager.IsAnyDevicesConnected()}");
                    goto default;


                case ConsoleKey.D4:
                    Console.WriteLine($"{nameof(bHapticsManager.IsPlayingAny)}(): {bHapticsManager.IsPlayingAny()}");
                    goto default;
                case ConsoleKey.D5:
                    Console.WriteLine($"{nameof(bHapticsManager.IsAnyMotorActive)}({nameof(PositionID)}.{nameof(PositionID.Vest)}): {bHapticsManager.IsAnyMotorActive(PositionID.Vest)}");
                    goto default;
                case ConsoleKey.D6:
                    Console.WriteLine($"{nameof(bHapticsManager.IsAnyMotorActive)}({nameof(PositionID)}.{nameof(PositionID.VestFront)}): {bHapticsManager.IsAnyMotorActive(PositionID.VestFront)}");
                    goto default;
                case ConsoleKey.D7:
                    Console.WriteLine($"{nameof(bHapticsManager.IsAnyMotorActive)}({nameof(PositionID)}.{nameof(PositionID.VestBack)}): {bHapticsManager.IsAnyMotorActive(PositionID.VestBack)}");
                    goto default;


                case ConsoleKey.I:
                    Console.WriteLine($"{nameof(bHapticsManager.GetDeviceStatus)}({nameof(PositionID)}.{nameof(PositionID.HandLeft)}): {bHapticsManager.GetDeviceStatus(PositionID.Vest).ToArrayString()}");
                    goto default;
                case ConsoleKey.O:
                    Console.WriteLine($"{nameof(bHapticsManager.GetDeviceStatus)}({nameof(PositionID)}.{nameof(PositionID.VestFront)}): {bHapticsManager.GetDeviceStatus(PositionID.VestFront).ToArrayString()}");
                    goto default;
                case ConsoleKey.P:
                    Console.WriteLine($"{nameof(bHapticsManager.GetDeviceStatus)}({nameof(PositionID)}.{nameof(PositionID.VestBack)}): {bHapticsManager.GetDeviceStatus(PositionID.VestBack).ToArrayString()}");
                    goto default;


                case ConsoleKey.N:
                    testFeedbackSwapped.Play();
                    goto default;

                case ConsoleKey.M:
                    Console.WriteLine($"{nameof(testFeedbackSwapped)}.{nameof(testFeedbackSwapped.IsRegistered)}(): {testFeedbackSwapped.IsRegistered()}");
                    goto default;

                case ConsoleKey.J:
                    Console.WriteLine($"{nameof(testFeedbackSwapped)}.{nameof(testFeedbackSwapped.IsPlaying)}(): {testFeedbackSwapped.IsPlaying()}");
                    goto default;


                case ConsoleKey.NumPad1:
                    Console.WriteLine($"{nameof(bHapticsManager.IsDeviceConnected)}({nameof(PositionID)}.{nameof(PositionID.HandLeft)}): {bHapticsManager.IsDeviceConnected(PositionID.HandLeft)}");
                    goto default;


                case ConsoleKey.NumPad2:
                    testFeedback.Play();
                    goto default;


                case ConsoleKey.NumPad3:
                    Console.WriteLine($"{nameof(testFeedback)}.{nameof(testFeedback.IsRegistered)}(): {testFeedback.IsRegistered()}");
                    goto default;


                case ConsoleKey.NumPad4:
                    Console.WriteLine($"{nameof(testFeedback)}.{nameof(testFeedback.IsPlaying)}(): {testFeedback.IsPlaying()}");
                    goto default;


                case ConsoleKey.NumPad5:
                    bHapticsManager.Play("testPlayFront", 1000, PositionID.HandLeft, TestPacket);
                   goto default;
                case ConsoleKey.NumPad6:
                    bHapticsManager.Play("testPlayBack", 1000, PositionID.ArmRight, TestPacket);
                    goto default;


                case ConsoleKey.NumPad7:
                    bHapticsManager.PlayMirrored("testPlayFrontMirrored", 1000, PositionID.HandLeft, TestPacket, MirrorDirection.Both);
                    goto default;
                case ConsoleKey.NumPad8:
                    bHapticsManager.PlayMirrored("testPlayBackMirrored", 1000, PositionID.ArmRight, TestPacket, MirrorDirection.Both);
                    goto default;

                // NEW: PlayMotors test
                case ConsoleKey.NumPad9:
                    byte[] motors = new byte[20];
                    motors[0] = 100;
                    motors[5] = 80;
                    motors[10] = 60;
                    bHapticsManager.PlayMotors("motorTest", 1000, PositionID.Vest, motors);
                    Console.WriteLine("PlayMotors called: motors[0]=100, motors[5]=80, motors[10]=60");
                    goto default;


                default:
                    return false;
            }
        }
    }

    internal static class Extensions
    {
        internal static string ToArrayString<T>(this T[] arr) where T : IComparable<T>
        {
            if (arr == null)
                return "{ }";
            int count = arr.Length;
            if (count <= 0)
                return "{ }";
            string returnval = "{";
            for (int i = 0; i < count; i++)
            {
                returnval += $" {arr[i]}";
                if (i < count - 1)
                    returnval += ",";
            }
            returnval += " }";
            return returnval;
        }
    }
}