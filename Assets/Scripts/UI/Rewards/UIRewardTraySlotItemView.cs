using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Core;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Rewards
{
    public class UIRewardTraySlotItemView : UIAutoBinder
    {
        [Header("Slot UI Elements (Names ending with _value)")]
        [SerializeField] private Image ui_image_reward_icon;
        [SerializeField] private TextMeshProUGUI ui_text_reward_amount_value;
        [SerializeField] private RectTransform ui_transform_punch_target;

        private string currentItemId;

        public string CurrentItemId => currentItemId;

        protected override void AutoBindReferences()
        {
            if (ui_image_reward_icon == null)
            {
                ui_image_reward_icon = FindImageByName("ui_image_reward_icon");
            }
            if (ui_text_reward_amount_value == null)
            {
                ui_text_reward_amount_value = FindTextByName("ui_text_reward_amount_value");
            }
            if (ui_transform_punch_target == null)
            {
                var t = FindChildComponent<RectTransform>("ui_transform_punch_target");
                if (t != null) ui_transform_punch_target = t;
            }
        }

        protected override void RegisterButtonCallbacks() { }
        protected override void UnregisterButtonCallbacks() { }

        public void SetReward(CollectedRewardEntry entry, bool playAnimation = false)
        {
            if (entry == null || entry.Item == null) return;

            currentItemId = entry.Item.Id;

            if (ui_image_reward_icon != null)
            {
                ui_image_reward_icon.sprite = entry.Item.Icon;
                ui_image_reward_icon.preserveAspect = true;
            }

            if (ui_text_reward_amount_value != null)
            {
                ui_text_reward_amount_value.text = entry.TotalAmount.ToString();
            }

            if (playAnimation && ui_transform_punch_target != null)
            {
                ui_transform_punch_target.DOKill(true);
                ui_transform_punch_target.localScale = Vector3.one;
                ui_transform_punch_target.DOPunchScale(Vector3.one * 0.25f, 0.35f, 5, 0.5f);
            }
        }
    }
}
