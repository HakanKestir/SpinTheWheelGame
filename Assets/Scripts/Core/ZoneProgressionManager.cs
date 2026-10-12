using System;
using UnityEngine;
using WheelOfFortune.Data;

namespace WheelOfFortune.Core
{
    public class ZoneProgressionManager : MonoBehaviour
    {
        [SerializeField] private ZoneConfigSO zoneConfig;
        [SerializeField] private int currentZone = 1;

        public event Action<int> OnZoneChanged;

        public ZoneConfigSO Config => zoneConfig;
        public int CurrentZone => currentZone;
        public bool IsSafeZone => zoneConfig != null && zoneConfig.IsSafeZone(currentZone);
        public bool IsSuperZone => zoneConfig != null && zoneConfig.IsSuperZone(currentZone);
        public bool CanLeave => zoneConfig != null && zoneConfig.CanLeaveAtZone(currentZone);
        public int NextSafeZone => zoneConfig != null ? zoneConfig.GetNextSafeZone(currentZone) : 5;
        public int NextSuperZone => zoneConfig != null ? zoneConfig.GetNextSuperZone(currentZone) : 30;
        public WheelConfigSO ActiveWheelConfig => zoneConfig != null ? zoneConfig.GetWheelConfigForZone(currentZone) : null;
        public int CurrentMultiplier => zoneConfig != null ? zoneConfig.GetRewardMultiplierForZone(currentZone) : 1;

        public void Initialize(ZoneConfigSO config, int startingZone = 1)
        {
            zoneConfig = config;
            currentZone = startingZone;
            ActiveWheelConfig?.RollSlicesForZone(currentZone);
            OnZoneChanged?.Invoke(currentZone);
        }

        public void AdvanceZone()
        {
            currentZone++;
            ActiveWheelConfig?.RollSlicesForZone(currentZone);
            OnZoneChanged?.Invoke(currentZone);
        }

        public void ResetToStart()
        {
            currentZone = 1;
            ActiveWheelConfig?.RollSlicesForZone(currentZone);
            OnZoneChanged?.Invoke(currentZone);
        }
    }
}
