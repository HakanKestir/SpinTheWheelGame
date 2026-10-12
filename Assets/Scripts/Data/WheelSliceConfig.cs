using System;
using UnityEngine;

namespace WheelOfFortune.Data
{
    [Serializable]
    public class WheelSliceConfig
    {
        [SerializeField] private RewardItemSO rewardItem;
        [SerializeField] private int overrideAmount;
        [SerializeField] private int weight = 1;

        public RewardItemSO RewardItem => rewardItem;
        public int Weight => Mathf.Max(1, weight);

        public int GetAmount(int zoneMultiplier = 1)
        {
            if (rewardItem == null) return 0;
            if (rewardItem.IsBomb) return 0;
            
            int baseVal = overrideAmount > 0 ? overrideAmount : rewardItem.BaseAmount;
            return baseVal * Mathf.Max(1, zoneMultiplier);
        }

        public bool IsBomb => rewardItem != null && rewardItem.IsBomb;
        public Sprite Icon => rewardItem != null ? rewardItem.Icon : null;
        public string DisplayName => rewardItem != null ? rewardItem.DisplayName : string.Empty;

        public void SetConfig(RewardItemSO item, int amount = 0, int sliceWeight = 1)
        {
            rewardItem = item;
            overrideAmount = amount;
            weight = sliceWeight;
        }
    }
}
