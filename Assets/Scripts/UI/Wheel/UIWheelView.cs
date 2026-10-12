using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Core;
using WheelOfFortune.Data;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Wheel
{
    public class UIWheelView : UIAutoBinder
    {
        [Header("Wheel Card Frame & Titles")]
        [SerializeField] private Image ui_image_wheel_card_bg;
        [SerializeField] private Image ui_image_wheel_card_frame;
        [SerializeField] private TextMeshProUGUI ui_text_spin_type_title_value;
        [SerializeField] private TextMeshProUGUI ui_text_wheel_subtitle_value;

        [Header("Wheel UI Elements")]
        [SerializeField] private Image ui_image_wheel_base;
        [SerializeField] private Image ui_image_wheel_indicator;
        [SerializeField] private Button ui_button_spin;
        [SerializeField] private UISpinButtonAnimator spinButtonAnimator;
        [SerializeField] private TextMeshProUGUI ui_text_spin_label_value;
        [SerializeField] private RectTransform ui_transform_wheel_anim_container;

        [Header("Slice Item Prefab (Reusable Slice)")]
        [SerializeField] private UIWheelSliceItemView sliceItemPrefab;

        [Header("Slice Views (8 slices)")]
        [SerializeField] private UIWheelSliceItemView[] sliceViews = new UIWheelSliceItemView[WheelConfigSO.SliceCount];

        public UIWheelSliceItemView SliceItemPrefab => sliceItemPrefab;
        public void SetSliceItemPrefab(UIWheelSliceItemView prefab) => sliceItemPrefab = prefab;

        [Header("Dependencies")]
        [SerializeField] private GameFlowManager gameFlowManager;
        [SerializeField] private ZoneProgressionManager zoneProgressionManager;

        public Image WheelCardBgImage => ui_image_wheel_card_bg;
        public Image WheelCardFrameImage => ui_image_wheel_card_frame;
        public TextMeshProUGUI SpinTypeTitleText => ui_text_spin_type_title_value;
        public TextMeshProUGUI WheelSubtitleText => ui_text_wheel_subtitle_value;
        public Image WheelBaseImage => ui_image_wheel_base;
        public Image WheelIndicatorImage => ui_image_wheel_indicator;
        public Button SpinButton => ui_button_spin;

        protected override void AutoBindReferences()
        {
            if (ui_image_wheel_card_bg == null)
            {
                ui_image_wheel_card_bg = FindImageByName("ui_image_wheel_card_bg");
            }
            if (ui_image_wheel_card_frame == null)
            {
                ui_image_wheel_card_frame = FindImageByName("ui_image_wheel_card_frame");
            }
            if (ui_text_spin_type_title_value == null)
            {
                ui_text_spin_type_title_value = FindTextByName("ui_text_spin_type_title_value");
            }
            if (ui_text_wheel_subtitle_value == null)
            {
                ui_text_wheel_subtitle_value = FindTextByName("ui_text_wheel_subtitle_value");
            }

            if (ui_image_wheel_base == null)
            {
                ui_image_wheel_base = FindImageByName("ui_image_wheel_base");
            }
            if (ui_image_wheel_indicator == null)
            {
                ui_image_wheel_indicator = FindImageByName("ui_image_wheel_indicator");
            }
            if (ui_button_spin == null)
            {
                ui_button_spin = FindButtonByName("ui_button_spin");
            }
            if (spinButtonAnimator == null && ui_button_spin != null)
            {
                spinButtonAnimator = ui_button_spin.GetComponent<UISpinButtonAnimator>();
                if (spinButtonAnimator == null)
                {
                    spinButtonAnimator = ui_button_spin.gameObject.AddComponent<UISpinButtonAnimator>();
                }
            }
            if (ui_text_spin_label_value == null)
            {
                ui_text_spin_label_value = FindTextByName("ui_text_spin_label_value");
                if (ui_text_spin_label_value == null)
                {
                    ui_text_spin_label_value = FindTextByName("ui_text_spin");
                }
            }
            if (ui_transform_wheel_anim_container == null)
            {
                var t = FindChildComponent<RectTransform>("ui_transform_wheel_anim_container");
                if (t != null) ui_transform_wheel_anim_container = t;
            }

            if (sliceViews == null || sliceViews.Length != WheelConfigSO.SliceCount)
            {
                sliceViews = new UIWheelSliceItemView[WheelConfigSO.SliceCount];
            }
            for (int i = 0; i < WheelConfigSO.SliceCount; i++)
            {
                if (sliceViews[i] == null)
                {
                    sliceViews[i] = FindChildComponent<UIWheelSliceItemView>($"ui_slice_item_{i}");
                }
            }

            if (gameFlowManager == null)
            {
                gameFlowManager = FindFirstObjectByType<GameFlowManager>();
            }
            if (zoneProgressionManager == null)
            {
                zoneProgressionManager = FindFirstObjectByType<ZoneProgressionManager>();
            }
        }

        protected override void RegisterButtonCallbacks()
        {
            if (ui_button_spin != null)
            {
                ui_button_spin.onClick.AddListener(OnSpinButtonClicked);
            }
            if (gameFlowManager != null)
            {
                gameFlowManager.OnStateChanged += OnGameStateChanged;
            }
            if (zoneProgressionManager != null)
            {
                zoneProgressionManager.OnZoneChanged += OnZoneChanged;
            }
        }

        protected override void UnregisterButtonCallbacks()
        {
            if (ui_button_spin != null)
            {
                ui_button_spin.onClick.RemoveListener(OnSpinButtonClicked);
            }
            if (gameFlowManager != null)
            {
                gameFlowManager.OnStateChanged -= OnGameStateChanged;
            }
            if (zoneProgressionManager != null)
            {
                zoneProgressionManager.OnZoneChanged -= OnZoneChanged;
            }
        }

        private void Start()
        {
            if (zoneProgressionManager != null)
            {
                UpdateWheelDisplay(zoneProgressionManager.ActiveWheelConfig, zoneProgressionManager.CurrentMultiplier);
            }
        }

        private void OnSpinButtonClicked()
        {
            if (gameFlowManager != null)
            {
                gameFlowManager.RequestSpin();
            }
        }

        private void OnGameStateChanged(GameState newState)
        {
            bool canSpin = (newState == GameState.ReadyToSpin);
            if (ui_button_spin != null)
            {
                ui_button_spin.interactable = canSpin;
            }
            if (spinButtonAnimator != null)
            {
                spinButtonAnimator.SetInteractable(canSpin);
            }
        }

        private void OnZoneChanged(int newZone)
        {
            if (zoneProgressionManager != null)
            {
                UpdateWheelDisplay(zoneProgressionManager.ActiveWheelConfig, zoneProgressionManager.CurrentMultiplier);
            }
        }

        public void UpdateWheelDisplay(WheelConfigSO config, int zoneMultiplier)
        {
            if (config == null) return;

            if (ui_image_wheel_base != null && config.WheelBaseSprite != null)
            {
                ui_image_wheel_base.sprite = config.WheelBaseSprite;
                ui_image_wheel_base.preserveAspect = true;
            }

            if (ui_image_wheel_indicator != null && config.IndicatorSprite != null)
            {
                ui_image_wheel_indicator.sprite = config.IndicatorSprite;
                ui_image_wheel_indicator.preserveAspect = true;
            }

            if (sliceViews != null)
            {
                for (int i = 0; i < sliceViews.Length; i++)
                {
                    if (sliceViews[i] != null)
                    {
                        var sliceConfig = config.GetSlice(i);
                        sliceViews[i].SetData(sliceConfig, zoneMultiplier);
                    }
                }
            }

            // Update Spin Type Title (Top)
            if (ui_text_spin_type_title_value != null)
            {
                switch (config.TierType)
                {
                    case WheelTierType.Gold:
                        ui_text_spin_type_title_value.text = "GOLDEN SPIN";
                        ui_text_spin_type_title_value.color = new Color(1f, 0.85f, 0.12f, 1f); // Radiant Gold
                        break;
                    case WheelTierType.Silver:
                        ui_text_spin_type_title_value.text = "SILVER SPIN";
                        ui_text_spin_type_title_value.color = new Color(0.2f, 0.95f, 0.3f, 1f); // Vibrant Safe Green
                        break;
                    case WheelTierType.Bronze:
                    default:
                        ui_text_spin_type_title_value.text = "BRONZE SPIN";
                        ui_text_spin_type_title_value.color = new Color(0.95f, 0.65f, 0.35f, 1f); // Bronze Warm Orange
                        break;
                }
            }

            // Update Wheel Subtitle (Bottom)
            if (ui_text_wheel_subtitle_value != null)
            {
                switch (config.TierType)
                {
                    case WheelTierType.Gold:
                        ui_text_wheel_subtitle_value.text = "Up To x10 Rewards";
                        ui_text_wheel_subtitle_value.color = new Color(1f, 0.82f, 0.15f, 1f); // Radiant Gold
                        break;
                    case WheelTierType.Silver:
                        ui_text_wheel_subtitle_value.text = "Safe Zone - No Bombs!";
                        ui_text_wheel_subtitle_value.color = new Color(0.2f, 0.95f, 0.3f, 1f); // Vibrant Neon Green
                        break;
                    case WheelTierType.Bronze:
                    default:
                        int mult = Mathf.Max(1, zoneMultiplier);
                        ui_text_wheel_subtitle_value.text = mult > 1 ? $"Up To x{mult} Rewards" : "Spin to Win!";
                        ui_text_wheel_subtitle_value.color = new Color(0.92f, 0.92f, 0.92f, 0.9f); // Crisp Silver White
                        break;
                }
            }
        }
    }
}
