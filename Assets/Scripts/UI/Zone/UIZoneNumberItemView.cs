using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Zone
{
    public class UIZoneNumberItemView : UIAutoBinder
    {
        [Header("Zone Item UI Elements (Names ending with _value)")]
        [SerializeField] private Image ui_image_zone_cell_bg;
        [SerializeField] private TextMeshProUGUI ui_text_zone_number_value;

        [Header("Color Schemes - Past / Upcoming")]
        [SerializeField] private Color upcomingNormalColor = Color.white;
        [SerializeField] private Color upcomingSafeColor = new Color(0.15f, 1f, 0.25f, 1f); // Vibrant Neon Green (like 15 in reference)
        [SerializeField] private Color upcomingSuperColor = new Color(1f, 0.85f, 0.2f, 1f); // Gold
        [SerializeField] private Color pastNormalColor = new Color(0.55f, 0.55f, 0.55f, 0.45f); // Dimmed gray (like 6, 7, 8, 9 in reference)
        [SerializeField] private Color pastSafeColor = new Color(0.2f, 0.75f, 0.2f, 0.85f); // Visible green (like 5, 10 in reference)
        [SerializeField] private Color pastSuperColor = new Color(0.7f, 0.55f, 0.15f, 0.85f); // Muted gold

        protected override void AutoBindReferences()
        {
            if (ui_image_zone_cell_bg == null)
            {
                ui_image_zone_cell_bg = FindImageByName("ui_image_zone_cell_bg");
            }
            if (ui_text_zone_number_value == null)
            {
                ui_text_zone_number_value = FindTextByName("ui_text_zone_number_value");
            }
        }

        protected override void RegisterButtonCallbacks() { }
        protected override void UnregisterButtonCallbacks() { }

        public void SetZone(int zoneNumber, bool isCurrent, bool isPast, bool isSafe, bool isSuper)
        {
            if (ui_image_zone_cell_bg != null && ui_image_zone_cell_bg.gameObject.activeSelf)
            {
                ui_image_zone_cell_bg.gameObject.SetActive(false);
            }

            if (ui_text_zone_number_value != null)
            {
                if (zoneNumber <= 0 || isCurrent)
                {
                    // Current zone is displayed by the slider badge in the center
                    ui_text_zone_number_value.text = string.Empty;
                }
                else
                {
                    ui_text_zone_number_value.text = zoneNumber.ToString();

                    if (isPast)
                    {
                        if (isSuper) ui_text_zone_number_value.color = pastSuperColor;
                        else if (isSafe) ui_text_zone_number_value.color = pastSafeColor;
                        else ui_text_zone_number_value.color = pastNormalColor;
                    }
                    else
                    {
                        if (isSuper) ui_text_zone_number_value.color = upcomingSuperColor;
                        else if (isSafe) ui_text_zone_number_value.color = upcomingSafeColor;
                        else ui_text_zone_number_value.color = upcomingNormalColor;
                    }

                    ui_text_zone_number_value.fontStyle = (isSafe || isSuper) ? FontStyles.Bold : FontStyles.Normal;
                    ui_text_zone_number_value.fontSize = 20;
                }
            }
        }

        public void SetZone(int zoneNumber, bool isCurrent, bool isSafe, bool isSuper, Sprite currentSprite, Sprite normalSprite, Sprite superSprite)
        {
            SetZone(zoneNumber, isCurrent, false, isSafe, isSuper);
        }
    }
}
