using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Data;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Wheel
{
    public class UIWheelSliceItemView : UIAutoBinder
    {
        [Header("Slice UI Elements (Names ending with _value)")]
        [SerializeField] private Image ui_image_slice_icon;
        [SerializeField] private TextMeshProUGUI ui_text_slice_amount_value;

        public Image SliceIcon => ui_image_slice_icon;
        public TextMeshProUGUI SliceAmountValue => ui_text_slice_amount_value;

        protected override void AutoBindReferences()
        {
            if (ui_image_slice_icon == null)
            {
                ui_image_slice_icon = FindImageByName("ui_image_slice_icon");
            }
            if (ui_text_slice_amount_value == null)
            {
                ui_text_slice_amount_value = FindTextByName("ui_text_slice_amount_value");
            }
        }

        protected override void RegisterButtonCallbacks() { }
        protected override void UnregisterButtonCallbacks() { }

        public void SetData(WheelSliceConfig slice, int zoneMultiplier)
        {
            if (slice == null || slice.RewardItem == null)
            {
                if (ui_image_slice_icon != null) ui_image_slice_icon.gameObject.SetActive(false);
                if (ui_text_slice_amount_value != null) ui_text_slice_amount_value.text = string.Empty;
                return;
            }

            if (ui_image_slice_icon != null)
            {
                ui_image_slice_icon.gameObject.SetActive(true);
                ui_image_slice_icon.sprite = slice.Icon;
                ui_image_slice_icon.preserveAspect = true;
            }

            if (ui_text_slice_amount_value != null)
            {
                if (slice.IsBomb)
                {
                    ui_text_slice_amount_value.text = string.Empty;
                }
                else
                {
                    int amount = slice.GetAmount(zoneMultiplier);
                    ui_text_slice_amount_value.text = $"x{amount}";
                }
            }
        }
    }
}
