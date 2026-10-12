using UnityEngine;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "NewZoneConfig", menuName = "WheelOfFortune/Wheel/Zone Config", order = 3)]
    public class ZoneConfigSO : ScriptableObject
    {
        [Header("Zone Intervals")]
        [SerializeField] private int safeZoneInterval = 5;
        [SerializeField] private int superZoneInterval = 30;

        [Header("Wheel Configurations")]
        [SerializeField] private WheelConfigSO bronzeWheelConfig;
        [SerializeField] private WheelConfigSO silverWheelConfig;
        [SerializeField] private WheelConfigSO goldenWheelConfig;

        [Header("Exit Rules")]
        [Tooltip("When enabled, the player can only choose to leave at safe zones or super zones. When disabled, player can leave at any zone before spinning.")]
        [SerializeField] private bool restrictExitToSafeZones = true;

        [Header("Multiplier Settings")]
        [Tooltip("Toggle to enable or disable reward multiplication as player progresses")]
        [SerializeField] private bool enableRewardMultiplication = true;

        [Tooltip("Number of zones before the reward multiplier increments by +1 (e.g. 4 means multiplier increases every 4 zones)")]
        [SerializeField] private int multiplierIncrementInterval = 4;

        [Header("Super Zone Target Preview")]
        [SerializeField] private RewardItemSO superZonePreviewReward;

        [Header("Economy & Revive")]
        [SerializeField] private int reviveGoldCost = 25;
        [SerializeField] private int initialCash = 1000;
        [SerializeField] private int initialGold = 50;

        public int SafeZoneInterval => safeZoneInterval;
        public int SuperZoneInterval => superZoneInterval;
        public WheelConfigSO BronzeWheelConfig => bronzeWheelConfig;
        public WheelConfigSO SilverWheelConfig => silverWheelConfig;
        public WheelConfigSO GoldenWheelConfig => goldenWheelConfig;
        public bool RestrictExitToSafeZones => restrictExitToSafeZones;
        public bool EnableRewardMultiplication => enableRewardMultiplication;
        public int MultiplierIncrementInterval => multiplierIncrementInterval;
        public RewardItemSO SuperZonePreviewReward => superZonePreviewReward;
        public int ReviveGoldCost => reviveGoldCost;
        public int InitialCash => initialCash;
        public int InitialGold => initialGold;

        public bool IsSuperZone(int zone)
        {
            if (zone <= 0) return false;
            return zone % superZoneInterval == 0;
        }

        public bool IsSafeZone(int zone)
        {
            if (zone <= 0) return false;
            return zone % safeZoneInterval == 0 && !IsSuperZone(zone);
        }

        public bool CanLeaveAtZone(int zone)
        {
            // If restricted, player can only leave when at safe zone or super zone
            if (!restrictExitToSafeZones) return true;
            return IsSafeZone(zone) || IsSuperZone(zone);
        }

        public WheelConfigSO GetWheelConfigForZone(int zone)
        {
            if (IsSuperZone(zone))
            {
                return goldenWheelConfig;
            }
            if (IsSafeZone(zone))
            {
                return silverWheelConfig;
            }
            return bronzeWheelConfig;
        }

        public int GetNextSafeZone(int currentZone)
        {
            int next = ((currentZone / safeZoneInterval) + 1) * safeZoneInterval;
            while (IsSuperZone(next))
            {
                next += safeZoneInterval;
            }
            return next;
        }

        public int GetNextSuperZone(int currentZone)
        {
            return ((currentZone / superZoneInterval) + 1) * superZoneInterval;
        }

        public int GetRewardMultiplierForZone(int zone)
        {
            if (!enableRewardMultiplication || multiplierIncrementInterval <= 0)
            {
                return 1;
            }
            return 1 + Mathf.Max(0, zone - 1) / multiplierIncrementInterval;
        }

        public void Setup(WheelConfigSO bronze, WheelConfigSO silver, WheelConfigSO gold, RewardItemSO superReward)
        {
            bronzeWheelConfig = bronze;
            silverWheelConfig = silver;
            goldenWheelConfig = gold;
            superZonePreviewReward = superReward;
        }

        public void SetMultiplicationSettings(bool enableMultiplication, int interval)
        {
            enableRewardMultiplication = enableMultiplication;
            multiplierIncrementInterval = Mathf.Max(1, interval);
        }

        public void SetExitRestriction(bool restrict)
        {
            restrictExitToSafeZones = restrict;
        }
    }
}
