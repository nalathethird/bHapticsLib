using bHapticsLib.Internal.SimpleJSON;
using MessagePack;

namespace bHapticsLib
{
    /// <summary>Rotational Option for Haptic Patterns (supports both MessagePack and JSON serialization)</summary>
    [MessagePackObject]
    public class RotationOption
    {
        internal JSONObject node = new JSONObject();

        /// <summary>Rotational Option for Haptic Patterns</summary>
        /// <param name="offsetAngleX">Rotation Angle X Axis</param>
        /// <param name="offsetY">Rotation Y Axis</param>
        [SerializationConstructor]
        public RotationOption(float offsetAngleX = 0, float offsetY = 0)
        {
            OffsetAngleX = offsetAngleX;
            OffsetY = offsetY;
        }

        /// <value>Rotation Angle X Axis</value>
        [Key(0)]
        public float OffsetAngleX
        {
            get => node["offsetAngleX"].AsFloat;
            set => node["offsetAngleX"] = value;
        }

        /// <value>Rotation Y Axis</value>
        [Key(1)]
        public float OffsetY
        {
            get => node["offsetY"].AsFloat;
            set => node["offsetY"] = value;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
            => $"{nameof(RotationOption)} ( {nameof(OffsetAngleX)}: {OffsetAngleX}, {nameof(OffsetY)}: {OffsetY} )";
    }
}