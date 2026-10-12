using System;
using UnityEngine;

namespace WheelOfFortune.Data
{
    [Serializable]
    public class WheelRewardPoolEntry
    {
        [Tooltip("Reward item definition (visuals and identity)")]
        [SerializeField] private RewardItemSO rewardItem;

        [Tooltip("Base amount specific to this wheel tier")]
        [SerializeField] private int baseAmount = 100;

        [Tooltip("Relative spawn weight in the pool when rolling wheel slices")]
        [SerializeField] private int weight = 10;

        public RewardItemSO RewardItem => rewardItem;
        public int BaseAmount => baseAmount;
        public int Weight => Mathf.Max(1, weight);

        public WheelRewardPoolEntry()
        {
        }

        public WheelRewardPoolEntry(RewardItemSO item, int amount, int spawnWeight = 10)
        {
            rewardItem = item;
            baseAmount = amount;
            weight = spawnWeight;
        }

        public void SetEntry(RewardItemSO item, int amount, int spawnWeight)
        {
            rewardItem = item;
            baseAmount = amount;
            weight = spawnWeight;
        }
    }
}
