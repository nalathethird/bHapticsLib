using bHapticsLib.Internal.SimpleJSON;
using MessagePack;

namespace bHapticsLib
{
    /// <summary>Haptic Point for Path Mode (supports both MessagePack and JSON serialization)</summary>
    [MessagePackObject]
    public class PathPoint
    {
        internal JSONObject node = new JSONObject();

        /// <summary>Haptic Point for Path Mode</summary>
        /// <param name="x">X Axis of Point Position</param>
        /// <param name="y">Y Axis of Point Position</param>
        /// <param name="intensity">Point Intensity</param>
        /// <param name="motorCount">Point Motor Count</param>
        [SerializationConstructor]
        public PathPoint(float x = 0, float y = 0, int intensity = 50, int motorCount = 3)
        {
            X = x;
            Y = y;
            Intensity = intensity;
            MotorCount = motorCount;
        }

        /// <value>X Axis of Point Position</value>
        [Key(0)]
        public float X
        {
            get => node["x"].AsFloat;
            set => node["x"] = value;
        }

        /// <value>Y Axis of Point Position</value>
        [Key(1)]
        public float Y
        {
            get => node["y"].AsFloat;
            set => node["y"] = value;
        }

        /// <value>Point Intensity</value>
        [Key(2)]
        public int Intensity
        {
            get => node["intensity"].AsInt;
            set => node["intensity"] = value.Clamp(0, bHapticsManager.MaxIntensityInInt);
        }

        /// <value>Point Motor Count</value>
        [Key(3)]
        public int MotorCount
        {
            get => node["motorCount"].AsInt;
            set => node["motorCount"] = value.Clamp(0, bHapticsManager.MaxMotorsPerPathPoint);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
            => $"{nameof(PathPoint)} ( {nameof(X)}: {X}, {nameof(Y)}: {Y}, {nameof(MotorCount)}: {MotorCount}, {nameof(Intensity)}: {Intensity} )";
    }
}