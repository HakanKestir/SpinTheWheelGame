using System;
using UnityEngine;
using WheelOfFortune.Data;

namespace WheelOfFortune.Core
{
    public class ZoneProgressionManager : MonoBehaviour
    {
        [SerializeField] private ZoneConfigSO zoneConfig;
        [SerializeField] private int currentZone = 1;

        private WheelRuntimeModel activeWheelModel;

        public ZoneConfigSO Config => zoneConfig;
        public int CurrentZone => currentZone;
        public bool IsSafeZone => zoneConfig != null && zoneConfig.IsSafeZone(currentZone);
        public bool IsSuperZone => zoneConfig != null && zoneConfig.IsSuperZone(currentZone);
        public bool CanLeave => zoneConfig != null && zoneConfig.CanLeaveAtZone(currentZone);
        public int NextSafeZone => zoneConfig != null ? zoneConfig.GetNextSafeZone(currentZone) : 5;
        public int NextSuperZone => zoneConfig != null ? zoneConfig.GetNextSuperZone(currentZone) : 30;
        public WheelConfigSO ActiveWheelConfig => zoneConfig != null ? zoneConfig.GetWheelConfigForZone(currentZone) : null;
        public IWheelModel ActiveWheelModel => activeWheelModel != null ? activeWheelModel : (IWheelModel)ActiveWheelConfig;
        public float CurrentMultiplier => zoneConfig != null ? zoneConfig.GetRewardMultiplierForZone(currentZone) : 1f;

        public void Initialize(ZoneConfigSO config, int startingZone = 1)
        {
            zoneConfig = config;
            currentZone = startingZone;
            GenerateWheelModel();
            EventBus.Publish(currentZone);
        }

        public void AdvanceZone()
        {
            currentZone++;
            GenerateWheelModel();
            EventBus.Publish(currentZone);
        }

        public void ResetToStart()
        {
            currentZone = 1;
            GenerateWheelModel();
            EventBus.Publish(currentZone);
        }

        public void RefreshCurrentZoneWheel()
        {
            GenerateWheelModel();
            EventBus.Publish(currentZone);
        }

        private void GenerateWheelModel()
        {
            if (zoneConfig != null)
            {
                activeWheelModel = zoneConfig.CreateWheelModelForZone(currentZone);
            }
        }
    }
}
