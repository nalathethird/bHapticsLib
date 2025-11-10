using bHapticsLib.Internal.SimpleJSON;
using MessagePack;

namespace bHapticsLib
{
    /// <summary>Haptic Point for Dot Mode (supports both MessagePack and JSON serialization)</summary>
    [MessagePackObject]
    public class DotPoint
    {
        internal JSONObject node = new JSONObject();

        /// <summary>Haptic Point for Dot Mode</summary>
        /// <param name="index">Index of Haptic Motor</param>
        /// <param name="intensity">Point Intensity</param>
        [SerializationConstructor]
        public DotPoint(int index = 0, int intensity = 50)
        {
            Index = index;
            Intensity = intensity;
        }

        /// <value>Index of Haptic Motor</value>
        [Key(0)]
        public int Index
        {
            get => node["index"].AsInt;
            set => node["index"] = value.Clamp(0, bHapticsManager.MaxMotorsPerDotPoint);
        }

        /// <value>Point Intensity</value>
        [Key(1)]
        public int Intensity
        {
            get => node["intensity"].AsInt;
            set => node["intensity"] = value.Clamp(0, bHapticsManager.MaxIntensityInInt);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
            => $"{nameof(DotPoint)} ( {nameof(Index)}: {Index}, {nameof(Intensity)}: {Intensity} )";
    }
}