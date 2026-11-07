using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Data
{
    /// <summary>
    /// Pure runtime model holding dynamically rolled slices for a specific zone.
    /// Eliminates runtime mutation of shared ScriptableObject assets.
    /// </summary>
    public class WheelRuntimeModel : IWheelModel
    {
        private readonly WheelTierType tierType;
        private readonly Sprite wheelBaseSprite;
        private readonly Sprite indicatorSprite;
        private readonly List<WheelSliceConfig> slices;
        private readonly int zone;

        public WheelTierType TierType => tierType;
        public Sprite WheelBaseSprite => wheelBaseSprite;
        public Sprite IndicatorSprite => indicatorSprite;
        public IReadOnlyList<WheelSliceConfig> Slices => slices;
        public int SliceCount => slices != null ? slices.Count : 0;
        public int Zone => zone;

        public WheelRuntimeModel(WheelTierType tier, Sprite baseSprite, Sprite indicator, List<WheelSliceConfig> sliceConfigs, int zoneNumber)
        {
            tierType = tier;
            wheelBaseSprite = baseSprite;
            indicatorSprite = indicator;
            slices = sliceConfigs ?? new List<WheelSliceConfig>();
            zone = zoneNumber;
        }

        public WheelSliceConfig GetSlice(int index)
        {
            if (slices == null || slices.Count == 0) return null;
            int count = slices.Count;
            index = (index % count + count) % count;
            return slices[index];
        }

        public int SelectRandomSliceIndex()
        {
            if (slices == null || slices.Count == 0) return 0;

            int totalWeight = 0;
            for (int i = 0; i < slices.Count; i++)
            {
                totalWeight += slices[i] != null ? slices[i].Weight : 1;
            }

            int randomValue = Random.Range(0, Mathf.Max(1, totalWeight));
            int currentSum = 0;
            for (int i = 0; i < slices.Count; i++)
            {
                currentSum += slices[i] != null ? slices[i].Weight : 1;
                if (randomValue < currentSum)
                {
                    return i;
                }
            }

            return 0;
        }
    }
}
