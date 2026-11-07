using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Data
{
    /// <summary>
    /// Represents the common interface for wheel configurations and runtime wheel models.
    /// Provides slice access, tier metadata, and random slice selection without mutating assets.
    /// </summary>
    public interface IWheelModel
    {
        WheelTierType TierType { get; }
        Sprite WheelBaseSprite { get; }
        Sprite IndicatorSprite { get; }
        int SliceCount { get; }
        IReadOnlyList<WheelSliceConfig> Slices { get; }
        WheelSliceConfig GetSlice(int index);
        int SelectRandomSliceIndex();
    }
}
