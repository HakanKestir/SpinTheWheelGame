using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "RewardDatabase", menuName = "WheelOfFortune/Wheel/Reward Database", order = 0)]
    public class RewardDatabaseSO : ScriptableObject
    {
        [Header("Master Reward List (All Game Rewards)")]
        [Tooltip("Comprehensive list of all distinct reward items in the game without tier-specific base amounts")]
        [SerializeField] private List<RewardItemSO> allRewards = new List<RewardItemSO>();

        public IReadOnlyList<RewardItemSO> AllRewards => allRewards;

        public RewardItemSO GetRewardById(string id)
        {
            if (string.IsNullOrEmpty(id) || allRewards == null) return null;
            return allRewards.Find(r => r != null && r.Id == id);
        }

        public RewardItemSO GetBombItem()
        {
            if (allRewards == null) return null;
            return allRewards.Find(r => r != null && r.IsBomb);
        }

        public List<RewardItemSO> GetNonBombRewards()
        {
            var list = new List<RewardItemSO>();
            if (allRewards != null)
            {
                for (int i = 0; i < allRewards.Count; i++)
                {
                    if (allRewards[i] != null && !allRewards[i].IsBomb)
                    {
                        list.Add(allRewards[i]);
                    }
                }
            }
            return list;
        }

        public void SetAllRewards(List<RewardItemSO> rewards)
        {
            allRewards = rewards ?? new List<RewardItemSO>();
        }
    }
}
