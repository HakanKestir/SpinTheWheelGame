using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Core;
using WheelOfFortune.Data;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Modals
{
    public class UIExitRewardCardItemView : UIAutoBinder
    {
        [Header("Card UI Elements")]
        [SerializeField] private Image ui_image_card_bg;
        [SerializeField] private Image ui_image_card_frame;
        [SerializeField] private Image ui_image_reward_icon;
        [SerializeField] private TextMeshProUGUI ui_text_amount_value;
        [SerializeField] private TextMeshProUGUI ui_text_name_value;

        protected override void AutoBindReferences()
        {
            if (ui_image_card_bg == null) ui_image_card_bg = FindImageByName("ui_image_card_bg");
            if (ui_image_card_frame == null) ui_image_card_frame = FindImageByName("ui_image_card_frame");
            if (ui_image_reward_icon == null) ui_image_reward_icon = FindImageByName("ui_image_reward_icon");
            if (ui_text_amount_value == null) ui_text_amount_value = FindTextByName("ui_text_amount_value");
            if (ui_text_name_value == null) ui_text_name_value = FindTextByName("ui_text_name_value");
        }

        protected override void OptimizeRaycastAndMaskable()
        {
            base.OptimizeRaycastAndMaskable();

            // Reward cards inside horizontal scroll rect must not intercept drag events
            if (ui_image_card_bg != null) { ui_image_card_bg.raycastTarget = false; ui_image_card_bg.maskable = true; }
            if (ui_image_card_frame != null) { ui_image_card_frame.raycastTarget = false; ui_image_card_frame.maskable = true; }
            if (ui_image_reward_icon != null) { ui_image_reward_icon.raycastTarget = false; ui_image_reward_icon.maskable = true; }
            if (ui_text_amount_value != null) { ui_text_amount_value.raycastTarget = false; ui_text_amount_value.maskable = true; }
            if (ui_text_name_value != null) { ui_text_name_value.raycastTarget = false; ui_text_name_value.maskable = true; }
        }

        protected override void RegisterButtonCallbacks() { }
        protected override void UnregisterButtonCallbacks() { }

        public void SetReward(CollectedRewardEntry entry)
        {
            if (entry == null || entry.Item == null) return;

            if (ui_image_reward_icon != null)
            {
                ui_image_reward_icon.sprite = entry.Item.Icon;
                ui_image_reward_icon.enabled = (entry.Item.Icon != null);
                ui_image_reward_icon.preserveAspect = true;
                ui_image_reward_icon.maskable = true;
                ui_image_reward_icon.raycastTarget = false;
            }

            if (ui_text_amount_value != null)
            {
                if (entry.Item.RewardType == RewardType.Cash)
                {
                    ui_text_amount_value.text = $"+${entry.TotalAmount:N0}";
                    ui_text_amount_value.color = new Color(0.2f, 0.95f, 0.35f, 1f); // Vibrant emerald
                }
                else if (entry.Item.RewardType == RewardType.Gold)
                {
                    ui_text_amount_value.text = $"+{entry.TotalAmount:N0}";
                    ui_text_amount_value.color = new Color(1f, 0.85f, 0.15f, 1f); // Radiant gold
                }
                else
                {
                    ui_text_amount_value.text = $"x{entry.TotalAmount:N0}";
                    ui_text_amount_value.color = Color.white;
                }
                ui_text_amount_value.maskable = true;
                ui_text_amount_value.raycastTarget = false;
            }

            if (ui_text_name_value != null)
            {
                ui_text_name_value.text = entry.Item.DisplayName;
                ui_text_name_value.maskable = true;
                ui_text_name_value.raycastTarget = false;
            }

            if (ui_image_card_bg != null)
            {
                ui_image_card_bg.maskable = true;
                ui_image_card_bg.raycastTarget = false;
            }

            if (ui_image_card_frame != null)
            {
                ui_image_card_frame.maskable = true;
                ui_image_card_frame.raycastTarget = false;
            }
        }

        public void SetReferences(Image bg, Image frame, Image icon, TextMeshProUGUI amount, TextMeshProUGUI nameTxt)
        {
            ui_image_card_bg = bg;
            ui_image_card_frame = frame;
            ui_image_reward_icon = icon;
            ui_text_amount_value = amount;
            ui_text_name_value = nameTxt;

            if (ui_image_card_bg != null) { ui_image_card_bg.raycastTarget = false; ui_image_card_bg.maskable = true; }
            if (ui_image_card_frame != null) { ui_image_card_frame.raycastTarget = false; ui_image_card_frame.maskable = true; }
            if (ui_image_reward_icon != null) { ui_image_reward_icon.raycastTarget = false; ui_image_reward_icon.maskable = true; }
            if (ui_text_amount_value != null) { ui_text_amount_value.raycastTarget = false; ui_text_amount_value.maskable = true; }
            if (ui_text_name_value != null) { ui_text_name_value.raycastTarget = false; ui_text_name_value.maskable = true; }
        }
    }
}
