# bHapticsLib (Modernized)

> **Modernized Fork** of [bHapticsLib](https://github.com/HerpDerpinstine/bHapticsLib) - Optimized for .NET Standard 2.1 and .NET 9

An Open-Source .NET Library for [bHaptics](https://www.bhaptics.com) Support with native WebSocket implementation.

[![.NET](https://img.shields.io/badge/.NET-Standard%202.1%20%7C%20.NET%209-512BD4?style=flat&logo=.net)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.md)

**Special Thanks to [bHaptics](https://www.bhaptics.com) for making the bHaptics Gear and supporting the community!**

---

## What's New in This Fork (v2.0.0)

### **Modernization**
- **Native WebSockets** - Uses `System.Net.WebSockets.ClientWebSocket` (no external dependencies unlike the main branch!)
- **Full Async/Await** - Modern Task-based asynchronous patterns
- **Simplified Targets** - Only .NET Standard 2.1 and .NET 9 (removed legacy frameworks due to security issues and legacy)
- **Better Performance** - Improved connection handling and message buffering

### **New Features**
- **Event System** - Real-time device and connection state notifications
- **Battery Monitoring** - Query and monitor device battery levels
- **Debug Logging** - Comprehensive connection diagnostics
- **Resonite/FrooxEngine Ready** - Tested and optimized for modern VR applications

---

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [Device IDs](#device-ids--positionid-reference)
- [Connection Management](#connection-management)
- [Device Queries](#device-queries)
- [Battery Monitoring](#battery-monitoring)
- [Event System](#event-system)
- [Pattern Playback](#pattern-playback)
- [Pattern Registration](#pattern-registration)
- [Manual Feedback](#manual-feedback)
- [API Reference](#complete-api-reference)
- [Documentation](#documentation)
- [Credits](#credits)

---

## Installation

### **NuGet Package** *(Coming Soon)*
```bash
dotnet add package bHapticsLib
```

### **Manual Installation**
1. Download the latest release from [Releases](https://github.com/nalathethird/bHapticsLib/releases)
2. Add `bHapticsLib.dll` as a reference to your project
3. Start coding!

### **Build from Source**
```bash
git clone https://github.com/nalathethird/bHapticsLib.git
cd bHapticsLib
dotnet build -c Release
```

---

## Quick Start

```csharp
using bHapticsLib;

// Connect to bHaptics Player
bHapticsManager.Connect("MyApp", "My Application");

// Check connection
if (bHapticsManager.Status == bHapticsStatus.Connected)
{
    Console.WriteLine("Connected to bHaptics Player!");
    
    // Check connected devices
    int deviceCount = bHapticsManager.GetConnectedDeviceCount();
    Console.WriteLine($"Connected devices: {deviceCount}");
    
    // Play simple haptic feedback
    bHapticsManager.Play("test", 1000, PositionID.Vest, 
        new DotPoint[] { new DotPoint(0, 100) });
}

// Subscribe to events
bHapticsManager.DeviceStatusChanged += (sender, e) =>
{
    Console.WriteLine($"{e.Position} {(e.IsConnected ? "connected" : "disconnected")}");
};

// Cleanup
bHapticsManager.Disconnect();
```

---

## Device IDs & PositionID Reference

Each bHaptics device has a **fixed, permanent ID** that never changes:

| Device Name | PositionID | Integer ID | Description |
|-------------|-----------|------------|-------------|
| **TactSuit (X40/X16)** | `PositionID.Vest` | `3` | Full vest (front + back combined) |
| **Tactal** | `PositionID.Head` | `4` | Face mask / head device |
| **Tactosy Hand (Left)** | `PositionID.HandLeft` | `6` | Left hand/wrist device |
| **Tactosy Hand (Right)** | `PositionID.HandRight` | `7` | Right hand/wrist device |
| **Tactosy Foot (Left)** | `PositionID.FootLeft` | `8` | Left foot device |
| **Tactosy Foot (Right)** | `PositionID.FootRight` | `9` | Right foot device |
| **Tactosy2 Arm (Left)** | `PositionID.ArmLeft` | `10` | Left arm device |
| **Tactosy2 Arm (Right)** | `PositionID.ArmRight` | `11` | Right arm device |
| **Vest Front** | `PositionID.VestFront` | `201` | Front of vest only |
| **Vest Back** | `PositionID.VestBack` | `202` | Back of vest only |
| **TactGlove (Left)** | `PositionID.GloveLeft` | `203` | Left glove |
| **TactGlove (Right)** | `PositionID.GloveRight` | `204` | Right glove |

**Example:**
```csharp
// Get device ID as integer
int vestID = (int)PositionID.Vest;  // Always 3

// Get device name
string name = PositionID.Vest.ToString();  // "Vest"

// Use in battery queries
int? battery = bHapticsManager.GetBatteryLevel(PositionID.Vest);  // Query vest battery
```

---

## Connection Management

### **Connect to bHaptics Player**
```csharp
// Simple connection
bHapticsManager.Connect("MyApp", "My Application");

// With auto-reconnect
bHapticsManager.Connect("MyApp", "My Application", 
    tryToReconnect: true, 
    maxRetries: 10);
```

### **Check Connection Status**
```csharp
bHapticsStatus status = bHapticsManager.Status;
// Disconnected, Connecting, or Connected

if (status == bHapticsStatus.Connected)
    Console.WriteLine("Connected!");
```

### **Connection Events**
```csharp
bHapticsManager.ConnectionEstablished += (sender, e) =>
{
    Console.WriteLine($"Connected! ({e.PreviousStatus} -> {e.NewStatus})");
};

bHapticsManager.ConnectionLost += (sender, e) =>
{
    Console.WriteLine($"Disconnected! ({e.PreviousStatus} -> {e.NewStatus})");
};

bHapticsManager.StatusChanged += (sender, e) =>
{
    Console.WriteLine($"Status: {e.PreviousStatus} -> {e.NewStatus}");
};
```

### **Disconnect**
```csharp
bHapticsManager.Disconnect();
```

### **Debug Connection Issues**
```csharp
// Get last error
string error = bHapticsManager.GetLastError();
Console.WriteLine($"Error: {error}");

// Get detailed connection log
string log = bHapticsManager.GetConnectionLog();
Console.WriteLine(log);
```

---

## Device Queries

### **Device Count**
```csharp
int count = bHapticsManager.GetConnectedDeviceCount();
bool anyDevices = bHapticsManager.IsAnyDevicesConnected();
```

### **Check Device Connection**
```csharp
bool vestConnected = bHapticsManager.IsDeviceConnected(PositionID.Vest);
bool headConnected = bHapticsManager.IsDeviceConnected(PositionID.Head);

if (vestConnected)
    Console.WriteLine("TactSuit is connected!");
```

### **Device Status (Motor Values)**
```csharp
int[] motorValues = bHapticsManager.GetDeviceStatus(PositionID.Vest);
// Returns array of intensity values (0-100) for each motor

bool anyMotorActive = bHapticsManager.IsAnyMotorActive(PositionID.Vest);
```

### **Device Events**
```csharp
bHapticsManager.DeviceStatusChanged += (sender, e) =>
{
    if (e.IsConnected)
        Console.WriteLine($"{e.Position} connected");
    else
        Console.WriteLine($"{e.Position} disconnected");
};
```

---

## Battery Monitoring

### **Query Battery Level**
```csharp
// Get battery for specific device
int? vestBattery = bHapticsManager.GetBatteryLevel(PositionID.Vest);
int? headBattery = bHapticsManager.GetBatteryLevel(PositionID.Head);

if (vestBattery.HasValue)
    Console.WriteLine($"Vest battery: {vestBattery.Value}%");
else
    Console.WriteLine("Battery info not available");
```

### **Battery Change Events**
```csharp
bHapticsManager.BatteryLevelChanged += (sender, e) =>
{
    Console.WriteLine($"{e.Position} battery: {e.BatteryLevel}%");
    
    // Low battery warning
    if (e.BatteryLevel.HasValue && e.BatteryLevel.Value < 20)
        Console.WriteLine($"LOW BATTERY: {e.Position}");
};
```

### **Monitor All Device Batteries**
```csharp
var devices = new[] 
{ 
    PositionID.Vest, PositionID.Head,
    PositionID.HandLeft, PositionID.HandRight,
    PositionID.ArmLeft, PositionID.ArmRight 
};

foreach (var device in devices)
{
    if (bHapticsManager.IsDeviceConnected(device))
    {
        int deviceID = (int)device;  // Get integer ID
        int? battery = bHapticsManager.GetBatteryLevel(device);
        
        Console.WriteLine($"Device {deviceID} ({device}): {battery?.ToString() ?? "N/A"}%");
    }
}
```

**See [BATTERY_LEVEL_GUIDE.md](BATTERY_LEVEL_GUIDE.md) for advanced battery monitoring patterns.**

---

## Event System

### **Available Events**
```csharp
// Device connection/disconnection
bHapticsManager.DeviceStatusChanged += (sender, e) => { };

// Connection established
bHapticsManager.ConnectionEstablished += (sender, e) => { };

// Connection lost
bHapticsManager.ConnectionLost += (sender, e) => { };

// Any status change
bHapticsManager.StatusChanged += (sender, e) => { };

// Battery level changes
bHapticsManager.BatteryLevelChanged += (sender, e) => { };
```

### **Event Args**
```csharp
// DeviceStatusChangedEventArgs
public PositionID Position { get; }
public bool IsConnected { get; }
public DateTime Timestamp { get; }

// ConnectionStatusChangedEventArgs
public bHapticsStatus PreviousStatus { get; }
public bHapticsStatus NewStatus { get; }
public DateTime Timestamp { get; }

// BatteryLevelChangedEventArgs
public PositionID Position { get; }
public int? BatteryLevel { get; }
public int? PreviousBatteryLevel { get; }
public DateTime Timestamp { get; }
```

### **Thread Safety Warning**
**All events fire on a background thread!** For UI updates:
```csharp
bHapticsManager.DeviceStatusChanged += (s, e) =>
{
    // Dispatch to UI thread
    Dispatcher.Invoke(() => UpdateUI(e.Position, e.IsConnected));
};
```

**See [EVENT_SYSTEM_GUIDE.md](EVENT_SYSTEM_GUIDE.md) for comprehensive event examples.**

---

## Pattern Playback

### **Load and Play .tact Files**
```csharp
// Load pattern from file
HapticPattern pattern = HapticPattern.LoadFromFile("myPattern", "path/to/pattern.tact");

// Play pattern
pattern.Play();

// Play with options
pattern.Play(
    scaleOption: new ScaleOption(intensity: 0.8f, duration: 1.0f),
    rotationOption: new RotationOption(offsetAngleX: 45f, offsetY: 0f)
);

// Check if playing
bool isPlaying = pattern.IsPlaying();

// Stop pattern
pattern.Stop();
```

### **Play Registered Patterns**
```csharp
// Register pattern first
bHapticsManager.RegisterPatternFromFile("myPattern", "path/to/pattern.tact");

// Play registered pattern
bHapticsManager.PlayRegistered("myPattern");

// With options
bHapticsManager.PlayRegistered("myPattern",
    scaleOption: new ScaleOption(0.8f, 1.0f));
```

### **Pattern Swapping (Mirror Left/Right)**
```csharp
// Load swapped pattern (mirrors left/right)
HapticPattern swapped = HapticPattern.LoadSwappedFromFile("swapped", "path/to/pattern.tact");
swapped.Play();
```

---

## Pattern Registration

### **Register from File**
```csharp
bHapticsManager.RegisterPatternFromFile("impact", "patterns/impact.tact");
```

### **Register from JSON**
```csharp
string json = File.ReadAllText("pattern.tact");
bHapticsManager.RegisterPatternFromJson("impact", json);
```

### **Register Swapped Pattern**
```csharp
bHapticsManager.RegisterPatternSwappedFromFile("impactSwapped", "patterns/impact.tact");
```

### **Check if Registered**
```csharp
bool isRegistered = bHapticsManager.IsPatternRegistered("impact");
```

---

## Manual Feedback

### **Using DotPoint (Specific Motors)**
```csharp
// Activate motor 0 at 100% intensity for 1 second
bHapticsManager.Play("test", 1000, PositionID.Vest,
    new DotPoint[] 
    {
        new DotPoint(0, 100),  // Motor 0 at 100%
        new DotPoint(5, 80),   // Motor 5 at 80%
        new DotPoint(10, 60)   // Motor 10 at 60%
    });
```

### **Using Byte/Int Arrays**
```csharp
// 20 motors (max for vest)
byte[] motorArray = new byte[20] 
{
    100, 80, 60, 40, 20,  // Top row
    100, 80, 60, 40, 20,  // Second row
    100, 80, 60, 40, 20,  // Third row
    100, 80, 60, 40, 20   // Bottom row
};

bHapticsManager.Play("pattern", 1000, PositionID.Vest, motorArray);
```

### **Mirrored Playback**
```csharp
bHapticsManager.PlayMirrored("test", 1000, PositionID.Vest,
    new DotPoint[] { new DotPoint(0, 100) },
    MirrorDirection.Both);  // Mirror horizontally and vertically
```

### **PathPoint (Animated Paths)**
```csharp
bHapticsManager.Play("sweep", 2000, PositionID.Vest,
    new PathPoint[]
    {
        new PathPoint(0.0f, 0.0f, 100, 3),   // Start top-left
        new PathPoint(1.0f, 0.0f, 100, 3),   // Move to top-right
        new PathPoint(1.0f, 1.0f, 100, 3),   // Move to bottom-right
        new PathPoint(0.0f, 1.0f, 100, 3)    // Move to bottom-left
    });
```

---

## Playback Control

### **Check if Playing**
```csharp
bool playing = bHapticsManager.IsPlaying("patternKey");
bool anyPlaying = bHapticsManager.IsPlayingAny();
```

### **Stop Playback**
```csharp
bHapticsManager.StopPlaying("patternKey");  // Stop specific pattern
bHapticsManager.StopPlayingAll();           // Stop all patterns
```

---

## Complete API Reference

### **Connection**
- `bool Connect(string id, string name, bool tryToReconnect = true, int maxRetries = 5)`
- `bool Disconnect()`
- `bHapticsStatus Status { get; }`
- `string GetLastError()`
- `string GetConnectionLog()`

### **Device Queries**
- `int GetConnectedDeviceCount()`
- `bool IsAnyDevicesConnected()`
- `bool IsDeviceConnected(PositionID type)`
- `int[] GetDeviceStatus(PositionID type)`
- `bool IsAnyMotorActive(PositionID type)`
- `int? GetBatteryLevel(PositionID type)` **NEW!**

### **Pattern Management**
- `bool IsPatternRegistered(string key)`
- `void RegisterPatternFromFile(string key, string tactFilePath)`
- `void RegisterPatternFromJson(string key, string tactFileJson)`
- `void RegisterPatternSwappedFromFile(string key, string tactFilePath)`
- `void RegisterPatternSwappedFromJson(string key, string tactFileJson)`

### **Playback**
- `void Play(string key, int durationMillis, PositionID position, DotPoint[] dotPoints)`
- `void Play(string key, int durationMillis, PositionID position, byte[] dotPoints)`
- `void Play(string key, int durationMillis, PositionID position, PathPoint[] pathPoints)`
- `void PlayMirrored(string key, int durationMillis, PositionID position, DotPoint[] dotPoints, MirrorDirection direction)`
- `void PlayRegistered(string key)`
- `void PlayRegistered(string key, ScaleOption scaleOption, RotationOption rotationOption)`

### **Playback Control**
- `bool IsPlaying(string key)`
- `bool IsPlayingAny()`
- `void StopPlaying(string key)`
- `void StopPlayingAll()`

### **Events**
- `event EventHandler<DeviceStatusChangedEventArgs> DeviceStatusChanged` **NEW!**
- `event EventHandler<ConnectionStatusChangedEventArgs> ConnectionEstablished` **NEW!**
- `event EventHandler<ConnectionStatusChangedEventArgs> ConnectionLost` **NEW!**
- `event EventHandler<ConnectionStatusChangedEventArgs> StatusChanged` **NEW!**
- `event EventHandler<BatteryLevelChangedEventArgs> BatteryLevelChanged` **NEW!**

### **Constants**
- `const int MaxIntensityInInt = 500`
- `const byte MaxIntensityInByte = 200`
- `const int MaxMotorsPerDotPoint = 20`
- `const int MaxMotorsPerPathPoint = 3`

---

## Documentation

### **Getting Started**
- [Quick Start Guide](QUICK_START.md) - Get up and running fast
- [TestApplication Guide](TESTAPPLICATION_GUIDE.md) - Interactive test harness

### **Modernization**
- [Modernization Notes](MODERNIZATION_NOTES.md) - Technical details of changes
- [Changelog](CHANGELOG.md) - Version history
- [Rebuild Instructions](REBUILD_INSTRUCTIONS.md) - How to build the library

### **Advanced Features**
- [Event System Guide](EVENT_SYSTEM_GUIDE.md) - Comprehensive event documentation
- [Event Quick Reference](EVENT_QUICK_REFERENCE.md) - Copy-paste examples
- [Battery Level Guide](BATTERY_LEVEL_GUIDE.md) - Battery monitoring patterns
- [Battery Quick Reference](BATTERY_QUICK_REFERENCE.md) - Battery examples

### **Debugging**
- [Connection Debug Guide](CONNECTION_DEBUG_GUIDE.md) - Troubleshooting connections
- [Critical Debug Guide](CRITICAL_DEBUG_GUIDE.md) - Advanced diagnostics

---

## Use Cases

### **VR Applications (Resonite, VRChat, Unity)**
```csharp
// Real-time haptic feedback in VR
public class VRHaptics : MonoBehaviour
{
    void Start()
    {
        bHapticsManager.Connect("VRApp", "My VR Application");
        bHapticsManager.DeviceStatusChanged += OnDeviceChanged;
    }

    void OnCollision(Collision collision)
    {
        // Play impact on vest
        bHapticsManager.Play("impact", 500, PositionID.Vest,
            new DotPoint[] { new DotPoint(0, 100) });
    }
}
```

### **Game Development**
```csharp
// Damage feedback system
public void TakeDamage(int damage, Vector3 direction)
{
    int intensity = Mathf.Clamp(damage * 10, 0, 100);
    bHapticsManager.Play("damage", 800, PositionID.Vest,
        new DotPoint[] { new DotPoint(GetMotorFromDirection(direction), intensity) });
}
```

### **Health/Fitness Apps**
```csharp
// Heart rate training feedback
public void OnHeartRateZone(int zone)
{
    if (zone > 4)  // High intensity zone
        bHapticsManager.Play("alert", 1000, PositionID.Vest,
            new DotPoint[] { new DotPoint(0, 100) });
}
```

---

## Building from Source

### **Prerequisites**
- .NET 9 SDK
- Visual Studio 2022 or VS Code (optional)

### **Build Commands**
```bash
# Clone repository
git clone https://github.com/nalathethird/bHapticsLib.git
cd bHapticsLib

# Restore packages
dotnet restore

# Build
dotnet build -c Release

# Output location
Output\Release\netstandard2.1\bHapticsLib.dll
Output\Release\net9.0\bHapticsLib.dll
```

### **Run Tests**
```bash
dotnet run --project TestApplication
```

---

## Credits

### **Original Library**
Created by **Herp Derpinstine**
- Original Repository: https://github.com/HerpDerpinstine/bHapticsLib
- Full C# implementation of bHaptics SDK
- Pattern system and protocol design

### **This Fork (v2.0.0+)**
Modernized by **nalathethird** for .NET 9 and Resonite/FrooxEngine
- Repository: https://github.com/nalathethird/bHapticsLib
- Native WebSocket implementation
- Event system and battery monitoring
- Modern async/await patterns

### **Third-Party Libraries**
- [SimpleJSON](https://github.com/Bunny83/SimpleJSON) by Bunny83 - MIT License
- [System.Net.WebSockets.Client](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.clientwebsocket) - Microsoft, MIT License

### **Special Thanks**
- **bHaptics** for creating amazing haptic hardware and supporting the community
- **Herp Derpinstine** for the excellent original implementation

**See [CREDITS.md](CREDITS.md) for detailed attribution.**

---

## License

MIT License - See [LICENSE.md](LICENSE.md) for full details.

This fork maintains the same MIT License as the original project.

**You are free to:**
- Use commercially
- Modify and redistribute
- Use in closed-source projects
- Create derivative works

**You must:**
- Include original copyright notice
- Include license text

---

## Disclaimer

**[bHaptics](https://www.bhaptics.com) is not liable for any issues or problems that may arise from using this library.**

This is a community-driven project created and developed by passionate users and content creators.

**For questions or issues:**
- [GitHub Issues](https://github.com/nalathethird/bHapticsLib/issues) for this fork
- [Original Repository Issues](https://github.com/HerpDerpinstine/bHapticsLib/issues) for base library
- [bHaptics Discord](https://discord.gg/JDw423Wskf) for general support

---

## Links

- **This Fork**: https://github.com/nalathethird/bHapticsLib
- **Forked From**: https://github.com/HerpDerpinstine/bHapticsLib
- **bHaptics Website**: https://www.bhaptics.com
- **bHaptics Designer**: https://designer.bhaptics.com
- **bHaptics Discord**: https://discord.gg/JDw423Wskf

---

## Why This Fork?

The original bHapticsLib is excellent but targets a wide range of .NET frameworks including very old ones. This fork:

- **Removes legacy framework support** - Focus on modern .NET
- **Uses native WebSockets** - No external dependencies
- **Full async/await** - Modern C# patterns throughout
- **Event-driven architecture** - Real-time notifications
- **Battery monitoring** - Track device battery levels
- **Better diagnostics** - Comprehensive logging and debugging

Perfect for modern applications that don't need .NET Framework 3.5 support!

---

## Comparison

| Feature | Original | This Fork |
|---------|----------|-----------|
| **Target Frameworks** | .NET 3.5 - .NET 9 | .NET Std 2.1, .NET 9 |
| **WebSocket Library** | WebSocketDotNet | Native ClientWebSocket |
| **Async Support** | Partial | Full async/await |
| **Event System** | No | Yes |
| **Battery Monitoring** | No | Yes |
| **Debug Logging** | Basic | Comprehensive |
| **External Dependencies** | 1 | 0 |

---