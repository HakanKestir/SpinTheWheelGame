using System;
using System.Collections.Generic;
using WheelOfFortune.Data;

namespace WheelOfFortune.Core
{
    /// <summary>
    /// Typed event messages published on the <see cref="EventBus"/>.
    /// Keeping all event messages in one file makes the bus contract discoverable
    /// and easy to extend.
    /// </summary>

    /// <summary>Published when a reward is added to the current run.</summary>
    public readonly struct RewardAddedEvent
    {
        public RewardItemSO Item { get; }
        public int Amount { get; }

        public RewardAddedEvent(RewardItemSO item, int amount)
        {
            Item = item;
            Amount = amount;
        }
    }

    /// <summary>Published when the current run rewards are cleared.</summary>
    public readonly struct RunRewardsClearedEvent { }

    /// <summary>Published when run rewards are restored from a snapshot.</summary>
    public readonly struct RunRewardsRestoredEvent { }

    /// <summary>Published when run rewards are banked to the wallet.</summary>
    public readonly struct RunRewardsBankedEvent
    {
        public IReadOnlyList<CollectedRewardEntry> BankedRewards { get; }

        public RunRewardsBankedEvent(IReadOnlyList<CollectedRewardEntry> bankedRewards)
        {
            BankedRewards = bankedRewards;
        }
    }

    /// <summary>Published when the permanent wallet balance changes.</summary>
    public readonly struct WalletBalanceChangedEvent
    {
        public int Cash { get; }
        public int Gold { get; }

        public WalletBalanceChangedEvent(int cash, int gold)
        {
            Cash = cash;
            Gold = gold;
        }
    }
}
