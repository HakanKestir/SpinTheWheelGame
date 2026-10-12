using System;
using System.Collections.Generic;
using UnityEngine;
using WheelOfFortune.Data;

namespace WheelOfFortune.Core
{
    [Serializable]
    public class CollectedRewardEntry
    {
        public RewardItemSO Item;
        public int TotalAmount;

        public CollectedRewardEntry(RewardItemSO item, int amount)
        {
            Item = item;
            TotalAmount = amount;
        }
    }

    public class PlayerInventoryManager : MonoBehaviour
    {
        [Header("Permanent Wallet")]
        [SerializeField] private int permanentCash = 250000;
        [SerializeField] private int permanentGold = 100;

        private readonly List<CollectedRewardEntry> currentRunRewards = new();
        private readonly Dictionary<string, CollectedRewardEntry> runRewardLookup = new();
        private readonly List<CollectedRewardEntry> permanentVaultItems = new();
        private readonly Dictionary<string, CollectedRewardEntry> permanentVaultLookup = new();

        public event Action<RewardItemSO, int> OnRewardAddedToRun;
        public event Action OnRunRewardsCleared;
        public event Action<IReadOnlyList<CollectedRewardEntry>> OnRunRewardsBanked;
        public event Action<int, int> OnWalletBalanceChanged;

        public int PermanentCash => permanentCash;
        public int PermanentGold => permanentGold;
        public IReadOnlyList<CollectedRewardEntry> CurrentRunRewards => currentRunRewards;
        public IReadOnlyList<CollectedRewardEntry> PermanentVaultItems => permanentVaultItems;

        public void Initialize(int startingCash, int startingGold)
        {
            permanentCash = startingCash;
            permanentGold = startingGold;
            currentRunRewards.Clear();
            runRewardLookup.Clear();
            permanentVaultItems.Clear();
            permanentVaultLookup.Clear();
            OnWalletBalanceChanged?.Invoke(permanentCash, permanentGold);
        }

        public void AddReward(RewardItemSO item, int amount)
        {
            if (item == null || item.IsBomb || amount <= 0) return;

            if (runRewardLookup.TryGetValue(item.Id, out var existing))
            {
                existing.TotalAmount += amount;
            }
            else
            {
                var newEntry = new CollectedRewardEntry(item, amount);
                currentRunRewards.Add(newEntry);
                runRewardLookup[item.Id] = newEntry;
            }

            OnRewardAddedToRun?.Invoke(item, amount);
        }

        public void ClearRunRewards()
        {
            currentRunRewards.Clear();
            runRewardLookup.Clear();
            OnRunRewardsCleared?.Invoke();
        }

        public int GetPendingRunCash()
        {
            int total = 0;
            foreach (var entry in currentRunRewards)
            {
                if (entry != null && entry.Item != null && entry.Item.RewardType == RewardType.Cash)
                {
                    total += entry.TotalAmount;
                }
            }
            return total;
        }

        public int GetPendingRunGold()
        {
            int total = 0;
            foreach (var entry in currentRunRewards)
            {
                if (entry != null && entry.Item != null && entry.Item.RewardType == RewardType.Gold)
                {
                    total += entry.TotalAmount;
                }
            }
            return total;
        }

        public void BankRewardsToWallet()
        {
            // Bank cash, gold, and all non-currency rewards into permanent player vault
            foreach (var entry in currentRunRewards)
            {
                if (entry == null || entry.Item == null) continue;

                if (entry.Item.RewardType == RewardType.Cash)
                {
                    permanentCash += entry.TotalAmount;
                }
                else if (entry.Item.RewardType == RewardType.Gold)
                {
                    permanentGold += entry.TotalAmount;
                }
                else
                {
                    if (permanentVaultLookup.TryGetValue(entry.Item.Id, out var existing))
                    {
                        existing.TotalAmount += entry.TotalAmount;
                    }
                    else
                    {
                        var vaultEntry = new CollectedRewardEntry(entry.Item, entry.TotalAmount);
                        permanentVaultItems.Add(vaultEntry);
                        permanentVaultLookup[entry.Item.Id] = vaultEntry;
                    }
                }
            }

            var snapshot = new List<CollectedRewardEntry>(currentRunRewards);
            currentRunRewards.Clear();
            runRewardLookup.Clear();

            OnWalletBalanceChanged?.Invoke(permanentCash, permanentGold);
            OnRunRewardsBanked?.Invoke(snapshot);
            OnRunRewardsCleared?.Invoke();
        }

        public bool CanAffordGold(int cost)
        {
            return permanentGold >= cost;
        }

        public bool TrySpendGold(int cost)
        {
            if (CanAffordGold(cost))
            {
                permanentGold -= cost;
                OnWalletBalanceChanged?.Invoke(permanentCash, permanentGold);
                return true;
            }
            return false;
        }

        public void AddGold(int amount)
        {
            permanentGold += Mathf.Max(0, amount);
            OnWalletBalanceChanged?.Invoke(permanentCash, permanentGold);
        }

        public void AddCash(int amount)
        {
            permanentCash += Mathf.Max(0, amount);
            OnWalletBalanceChanged?.Invoke(permanentCash, permanentGold);
        }
    }
}
