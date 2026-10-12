using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Core;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Zone
{
    public class UIZoneBadgesView : UIAutoBinder
    {
        [Header("Super Zone Badge Elements")]
        [SerializeField] private TextMeshProUGUI ui_text_super_zone_value;
        [SerializeField] private Image ui_image_super_zone_reward_preview;

        [Header("Safe Zone Badge Elements")]
        [SerializeField] private TextMeshProUGUI ui_text_safe_zone_value;
        [SerializeField] private Image ui_image_safe_zone_icon;

        [Header("Dependencies")]
        [SerializeField] private ZoneProgressionManager zoneProgressionManager;

        protected override void AutoBindReferences()
        {
            if (ui_text_super_zone_value == null)
            {
                ui_text_super_zone_value = FindTextByName("ui_text_super_zone_value");
            }
            if (ui_image_super_zone_reward_preview == null)
            {
                ui_image_super_zone_reward_preview = FindImageByName("ui_image_super_zone_reward_preview");
            }
            if (ui_text_safe_zone_value == null)
            {
                ui_text_safe_zone_value = FindTextByName("ui_text_safe_zone_value");
            }
            if (ui_image_safe_zone_icon == null)
            {
                ui_image_safe_zone_icon = FindImageByName("ui_image_safe_zone_icon");
            }

            if (zoneProgressionManager == null)
            {
                zoneProgressionManager = FindFirstObjectByType<ZoneProgressionManager>();
            }
        }

        protected override void RegisterButtonCallbacks()
        {
            if (zoneProgressionManager != null)
            {
                zoneProgressionManager.OnZoneChanged += OnZoneChanged;
            }
        }

        protected override void UnregisterButtonCallbacks()
        {
            if (zoneProgressionManager != null)
            {
                zoneProgressionManager.OnZoneChanged -= OnZoneChanged;
            }
        }

        private void Start()
        {
            UpdateBadges();
        }

        private void OnZoneChanged(int newZone)
        {
            UpdateBadges();
        }

        public void UpdateBadges()
        {
            if (zoneProgressionManager == null) return;

            int nextSafe = zoneProgressionManager.NextSafeZone;
            int nextSuper = zoneProgressionManager.NextSuperZone;

            if (ui_text_safe_zone_value != null)
            {
                ui_text_safe_zone_value.text = nextSafe.ToString();
            }

            if (ui_text_super_zone_value != null)
            {
                ui_text_super_zone_value.text = nextSuper.ToString();
            }

            if (ui_image_super_zone_reward_preview != null && zoneProgressionManager.Config.SuperZonePreviewReward != null)
            {
                ui_image_super_zone_reward_preview.sprite = zoneProgressionManager.Config.SuperZonePreviewReward.Icon;
                ui_image_super_zone_reward_preview.preserveAspect = true;
            }
        }
    }
}
