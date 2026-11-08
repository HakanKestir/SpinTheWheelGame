using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "NewWheelConfig", menuName = "WheelOfFortune/Wheel/Wheel Config", order = 2)]
    public class WheelConfigSO : ScriptableObject, IWheelModel
    {
        public const int SliceCount = 8;

        [Header("Wheel Tier")]
        [SerializeField] private WheelTierType tierType;

        [Header("Sprites")]
        [SerializeField] private Sprite wheelBaseSprite;
        [SerializeField] private Sprite indicatorSprite;

        [Header("Bomb Item (Used if tier spawns bombs)")]
        [SerializeField] private RewardItemSO bombRewardItem;

        [Header("Reward Pool (Available rewards for this wheel)")]
        [Tooltip("Configurable pool of rewards with tier-specific base amount and spawn weights")]
        [SerializeField] private List<WheelRewardPoolEntry> rewardPool = new List<WheelRewardPoolEntry>();

        [Header("Active Slices (Populated dynamically per zone)")]
        [SerializeField] private WheelSliceConfig[] slices = new WheelSliceConfig[SliceCount];

        public WheelTierType TierType => tierType;
        public Sprite WheelBaseSprite => wheelBaseSprite;
        public Sprite IndicatorSprite => indicatorSprite;
        public RewardItemSO BombRewardItem => bombRewardItem;
        public IReadOnlyList<WheelRewardPoolEntry> RewardPool => rewardPool;
        public IReadOnlyList<WheelSliceConfig> Slices => slices;
        int IWheelModel.SliceCount => SliceCount;

        private void OnValidate()
        {
            EnsureSliceArray();
        }

        private void EnsureSliceArray()
        {
            if (slices == null || slices.Length != SliceCount)
            {
                var newSlices = new WheelSliceConfig[SliceCount];
                if (slices != null)
                {
                    for (int i = 0; i < Mathf.Min(slices.Length, SliceCount); i++)
                    {
                        newSlices[i] = slices[i];
                    }
                }
                for (int i = 0; i < SliceCount; i++)
                {
                    if (newSlices[i] == null)
                    {
                        newSlices[i] = new WheelSliceConfig();
                    }
                }
                slices = newSlices;
            }
        }

        /// <summary>
        /// Dynamically creates a pure runtime wheel model for the specified zone.
        /// Slice amounts stay at their base values; the zone multiplier is applied once
        /// when the wheel is displayed and when a reward is paid out.
        /// </summary>
        public WheelRuntimeModel CreateRuntimeModelForZone(int zone)
        {
            if (rewardPool == null || rewardPool.Count == 0)
            {
                var fallbackSlices = new List<WheelSliceConfig>();
                if (slices != null)
                {
                    for (int i = 0; i < slices.Length; i++)
                    {
                        var s = new WheelSliceConfig();
                        if (slices[i] != null) s.SetConfig(slices[i].RewardItem, slices[i].GetAmount(1), slices[i].Weight);
                        fallbackSlices.Add(s);
                    }
                }
                return new WheelRuntimeModel(tierType, wheelBaseSprite, indicatorSprite, fallbackSlices, zone);
            }

            var nonBombCandidates = new List<WheelRewardPoolEntry>();
            for (int i = 0; i < rewardPool.Count; i++)
            {
                var entry = rewardPool[i];
                if (entry != null && entry.RewardItem != null && !entry.RewardItem.IsBomb)
                {
                    nonBombCandidates.Add(entry);
                }
            }

            if (nonBombCandidates.Count == 0)
            {
                return new WheelRuntimeModel(tierType, wheelBaseSprite, indicatorSprite, new List<WheelSliceConfig>(), zone);
            }

            bool includeBomb = (tierType == WheelTierType.Bronze);
            int bombIndex = includeBomb ? Random.Range(0, SliceCount) : -1;

            int neededRewardCount = includeBomb ? SliceCount - 1 : SliceCount;
            var selectedEntries = PickDistinctWeightedEntries(nonBombCandidates, neededRewardCount);

            var runtimeSlices = new List<WheelSliceConfig>(SliceCount);
            int rewardPickIndex = 0;

            for (int i = 0; i < SliceCount; i++)
            {
                var slice = new WheelSliceConfig();

                if (i == bombIndex)
                {
                    RewardItemSO bombItem = bombRewardItem;
                    if (bombItem == null)
                    {
                        var bombEntry = rewardPool.Find(e => e != null && e.RewardItem != null && e.RewardItem.IsBomb);
                        if (bombEntry != null) bombItem = bombEntry.RewardItem;
                    }
                    slice.SetConfig(bombItem, 0, 10);
                }
                else
                {
                    if (rewardPickIndex < selectedEntries.Count)
                    {
                        var entry = selectedEntries[rewardPickIndex++];
                        slice.SetConfig(entry.RewardItem, entry.BaseAmount, entry.Weight);
                    }
                }

                runtimeSlices.Add(slice);
            }

            return new WheelRuntimeModel(tierType, wheelBaseSprite, indicatorSprite, runtimeSlices, zone);
        }

        /// <summary>
        /// Dynamically rolls 8 slices for the current zone from this wheel's reward pool.
        /// Bronze wheel contains 1 bomb + 7 distinct rewards.
        /// Silver and Gold wheels contain 8 distinct non-bomb rewards.
        /// </summary>
        public void RollSlicesForZone(int zone)
        {
            EnsureSliceArray();
            var model = CreateRuntimeModelForZone(zone);
            for (int i = 0; i < Mathf.Min(slices.Length, model.SliceCount); i++)
            {
                var s = model.GetSlice(i);
                if (slices[i] == null) slices[i] = new WheelSliceConfig();
                if (s != null)
                {
                    slices[i].SetConfig(s.RewardItem, s.GetAmount(1), s.Weight);
                }
            }
        }

        private List<WheelRewardPoolEntry> PickDistinctWeightedEntries(List<WheelRewardPoolEntry> pool, int count)
        {
            var result = new List<WheelRewardPoolEntry>();
            var available = new List<WheelRewardPoolEntry>(pool);

            for (int step = 0; step < count; step++)
            {
                if (available.Count == 0)
                {
                    available.AddRange(pool);
                }

                int totalWeight = 0;
                for (int i = 0; i < available.Count; i++)
                {
                    totalWeight += available[i].Weight;
                }

                int roll = Random.Range(0, Mathf.Max(1, totalWeight));
                int currentSum = 0;
                int chosenIndex = 0;

                for (int i = 0; i < available.Count; i++)
                {
                    currentSum += available[i].Weight;
                    if (roll < currentSum)
                    {
                        chosenIndex = i;
                        break;
                    }
                }

                result.Add(available[chosenIndex]);
                available.RemoveAt(chosenIndex);
            }

            return result;
        }

        public WheelSliceConfig GetSlice(int index)
        {
            if (slices == null || slices.Length == 0) return null;
            index = (index % SliceCount + SliceCount) % SliceCount;
            return slices[index];
        }

        public int SelectRandomSliceIndex()
        {
            if (slices == null || slices.Length == 0) return 0;

            int totalWeight = 0;
            for (int i = 0; i < slices.Length; i++)
            {
                totalWeight += slices[i] != null ? slices[i].Weight : 1;
            }

            int randomValue = Random.Range(0, totalWeight);
            int currentSum = 0;
            for (int i = 0; i < slices.Length; i++)
            {
                currentSum += slices[i] != null ? slices[i].Weight : 1;
                if (randomValue < currentSum)
                {
                    return i;
                }
            }

            return 0;
        }

        public void Setup(WheelTierType tier, Sprite baseSprite, Sprite indSprite, WheelSliceConfig[] sliceConfigs)
        {
            tierType = tier;
            wheelBaseSprite = baseSprite;
            indicatorSprite = indSprite;
            slices = sliceConfigs;
        }

        public void SetRewardPool(List<WheelRewardPoolEntry> pool, RewardItemSO bombItem = null)
        {
            rewardPool = pool ?? new List<WheelRewardPoolEntry>();
            if (bombItem != null) bombRewardItem = bombItem;
        }
    }
}
