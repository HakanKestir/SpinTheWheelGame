using UnityEngine;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "NewRewardItem", menuName = "WheelOfFortune/Wheel/Reward Item", order = 1)]
    public class RewardItemSO : ScriptableObject
    {
        [Header("Identification")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private RewardType rewardType;

        [Header("Visuals")]
        [SerializeField] private Sprite icon;

        [Header("Values")]
        [SerializeField] private int baseAmount = 1;
        [SerializeField] private bool isBomb;

        public string Id => id;
        public string DisplayName => displayName;
        public RewardType RewardType => rewardType;
        public Sprite Icon => icon;
        public int BaseAmount => baseAmount;
        public bool IsBomb => isBomb;

        public void Initialize(string newId, string newDisplayName, RewardType newType, Sprite newIcon, int newAmount, bool bomb = false)
        {
            id = newId;
            displayName = newDisplayName;
            rewardType = newType;
            icon = newIcon;
            baseAmount = newAmount;
            isBomb = bomb;
        }
    }
}
