using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Core;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Zone
{
    public class UIZoneTrackerView : UIAutoBinder
    {
        [Header("Zone Bar Elements")]
        [SerializeField] private Image ui_image_zone_frame;
        [SerializeField] private Image ui_image_zone_bg;
        [SerializeField] private RectTransform ui_transform_zone_items_container;

        [Header("Current Zone Marker (Background + Frame + Black Number)")]
        [SerializeField] private RectTransform ui_transform_zone_slider;
        [SerializeField] private Image ui_image_slider_bg;
        [SerializeField] private Image ui_image_slider_frame;
        [SerializeField] private TextMeshProUGUI ui_text_slider_number_value;

        [Header("Marker Sprites")]
        [SerializeField] private Sprite sliderBgSprite;
        [SerializeField] private Sprite sliderFrameSprite;

        [Header("Zone Item Prefab (Reusable Slot)")]
        [SerializeField] private UIZoneNumberItemView zoneItemPrefab;

        [Header("Zone Items (13 slots: 6 past, 1 center, 6 upcoming)")]
        [SerializeField] private List<UIZoneNumberItemView> itemViews = new();

        public UIZoneNumberItemView ZoneItemPrefab => zoneItemPrefab;
        public void SetZoneItemPrefab(UIZoneNumberItemView prefab) => zoneItemPrefab = prefab;

        [Header("Dependencies")]
        [SerializeField] private ZoneProgressionManager zoneProgressionManager;

        private Coroutine slideCoroutine;
        private int lastZone = 1;
        private Vector2 containerDefaultPos;
        private bool isInitialized = false;

        public Image SliderBgImage => ui_image_slider_bg;
        public Image SliderFrameImage => ui_image_slider_frame;
        public TextMeshProUGUI SliderNumberText => ui_text_slider_number_value;

        public bool IsSafeZone(int zone)
        {
            if (zone <= 0) return false;
            if (zoneProgressionManager != null && zoneProgressionManager.Config != null)
            {
                return zoneProgressionManager.Config.IsSafeZone(zone);
            }
            // Default rules: every 5th zone is safe, every 30th is super
            return (zone % 5 == 0) && (zone % 30 != 0);
        }

        public bool IsSuperZone(int zone)
        {
            if (zone <= 0) return false;
            if (zoneProgressionManager != null && zoneProgressionManager.Config != null)
            {
                return zoneProgressionManager.Config.IsSuperZone(zone);
            }
            return (zone % 30 == 0);
        }

        protected override void AutoBindReferences()
        {
            if (ui_image_zone_frame == null)
            {
                ui_image_zone_frame = FindImageByName("ui_image_zone_frame");
            }
            if (ui_image_zone_bg == null)
            {
                ui_image_zone_bg = FindImageByName("ui_image_zone_bg");
            }
            if (ui_transform_zone_items_container == null)
            {
                var t = FindChildComponent<RectTransform>("ui_transform_zone_items_container");
                if (t != null) ui_transform_zone_items_container = t;
            }

            if (ui_transform_zone_slider == null)
            {
                var t = FindChildComponent<RectTransform>("ui_image_zone_slider");
                if (t != null) ui_transform_zone_slider = t;
            }
            if (ui_image_slider_bg == null)
            {
                ui_image_slider_bg = FindImageByName("ui_image_slider_bg");
                if (ui_image_slider_bg == null)
                {
                    ui_image_slider_bg = FindImageByName("ui_image_zone_slider");
                }
            }
            if (ui_image_slider_frame == null)
            {
                ui_image_slider_frame = FindImageByName("ui_image_slider_frame");
            }
            if (ui_text_slider_number_value == null)
            {
                ui_text_slider_number_value = FindTextByName("ui_text_slider_number_value");
            }

            if (itemViews.Count == 0 && ui_transform_zone_items_container != null)
            {
                var found = ui_transform_zone_items_container.GetComponentsInChildren<UIZoneNumberItemView>(true);
                itemViews.AddRange(found);
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
            if (ui_transform_zone_items_container != null)
            {
                containerDefaultPos = ui_transform_zone_items_container.anchoredPosition;
            }

            EnsureSliderSetup();

            if (zoneProgressionManager != null)
            {
                lastZone = zoneProgressionManager.CurrentZone;
                RefreshZoneDisplay(lastZone);
            }
            else
            {
                RefreshZoneDisplay(1);
            }

            isInitialized = true;
        }

        private void EnsureSliderSetup()
        {
            if (ui_transform_zone_slider != null)
            {
                ui_transform_zone_slider.anchorMin = new Vector2(0.5f, 0.5f);
                ui_transform_zone_slider.anchorMax = new Vector2(0.5f, 0.5f);
                ui_transform_zone_slider.pivot = new Vector2(0.5f, 0.5f);
                ui_transform_zone_slider.anchoredPosition = Vector2.zero;
                ui_transform_zone_slider.sizeDelta = new Vector2(52f, 58f);

                // 1. Permanently remove triangle notch if it exists
                var notch = ui_transform_zone_slider.Find("ui_text_slider_notch");
                if (notch != null)
                {
                    notch.gameObject.SetActive(false);
                    if (Application.isEditor && !Application.isPlaying)
                    {
                        DestroyImmediate(notch.gameObject);
                    }
                    else
                    {
                        Destroy(notch.gameObject);
                    }
                }

                // 2. Resolve background image (pure white base texture)
                if (ui_image_slider_bg == null)
                {
                    var bgTrans = ui_transform_zone_slider.Find("ui_image_slider_bg");
                    if (bgTrans != null)
                    {
                        ui_image_slider_bg = bgTrans.GetComponent<Image>();
                    }
                    else
                    {
                        // Check if root slider has an Image component
                        var rootImg = ui_transform_zone_slider.GetComponent<Image>();
                        if (rootImg != null)
                        {
                            ui_image_slider_bg = rootImg;
                        }
                    }
                }

                if (ui_image_slider_bg != null)
                {
                    if (sliderBgSprite != null)
                    {
                        ui_image_slider_bg.sprite = sliderBgSprite;
                    }
                    ui_image_slider_bg.type = Image.Type.Sliced;
                }

                // 3. Resolve border frame image
                if (ui_image_slider_frame == null)
                {
                    var frameTrans = ui_transform_zone_slider.Find("ui_image_slider_frame");
                    if (frameTrans != null)
                    {
                        ui_image_slider_frame = frameTrans.GetComponent<Image>();
                    }
                    else
                    {
                        // Dynamically create frame child if missing
                        var frameGO = new GameObject("ui_image_slider_frame", typeof(RectTransform));
                        frameGO.transform.SetParent(ui_transform_zone_slider, false);
                        var rt = frameGO.GetComponent<RectTransform>();
                        rt.anchorMin = Vector2.zero;
                        rt.anchorMax = Vector2.one;
                        rt.offsetMin = Vector2.zero;
                        rt.offsetMax = Vector2.zero;
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        ui_image_slider_frame = frameGO.AddComponent<Image>();
                        if (sliderFrameSprite != null) ui_image_slider_frame.sprite = sliderFrameSprite;
                        ui_image_slider_frame.type = Image.Type.Sliced;
                        ui_image_slider_frame.color = new Color(0.92f, 0.92f, 0.92f, 1f);
                        ui_image_slider_frame.raycastTarget = false;
                        ui_image_slider_frame.maskable = false;
                    }
                }

                // 4. Ensure hierarchy order: bg behind frame, number text in front
                if (ui_image_slider_bg != null && ui_image_slider_bg.transform != ui_transform_zone_slider)
                {
                    ui_image_slider_bg.transform.SetAsFirstSibling();
                }
                if (ui_image_slider_frame != null)
                {
                    ui_image_slider_frame.transform.SetSiblingIndex(1);
                }
                if (ui_text_slider_number_value != null)
                {
                    ui_text_slider_number_value.transform.SetAsLastSibling();
                }

                // Ensure slider itself renders above the zone bar slots and frame
                ui_transform_zone_slider.SetAsLastSibling();
            }
        }

        private void OnZoneChanged(int newZone)
        {
            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
                slideCoroutine = null;
            }

            // Smooth conveyor slide to the left when progressing +1
            if (isInitialized && newZone == lastZone + 1 && gameObject.activeInHierarchy)
            {
                slideCoroutine = StartCoroutine(SlideZoneTrackRoutine(lastZone, newZone));
            }
            else
            {
                lastZone = newZone;
                RefreshZoneDisplay(newZone);
            }
        }

        private IEnumerator SlideZoneTrackRoutine(int fromZone, int toZone)
        {
            // Calculate step width
            float stepX = 51.4f;
            int centerIndex = itemViews.Count / 2;
            if (itemViews.Count > centerIndex + 1 && itemViews[centerIndex] != null && itemViews[centerIndex + 1] != null)
            {
                stepX = itemViews[centerIndex + 1].GetComponent<RectTransform>().anchoredPosition.x -
                        itemViews[centerIndex].GetComponent<RectTransform>().anchoredPosition.x;
            }
            if (stepX <= 0.1f)
            {
                float totalWidth = ui_transform_zone_items_container != null ? ui_transform_zone_items_container.rect.width : 668f;
                stepX = totalWidth / Mathf.Max(1, itemViews.Count);
            }

            float duration = 0.35f;
            float elapsed = 0f;

            Vector2 startPos = containerDefaultPos;
            Vector2 targetPos = containerDefaultPos + new Vector2(-stepX, 0f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Ease-out cubic
                float easeT = 1f - Mathf.Pow(1f - t, 3f);

                if (ui_transform_zone_items_container != null)
                {
                    ui_transform_zone_items_container.anchoredPosition = Vector2.Lerp(startPos, targetPos, easeT);
                }

                if (ui_transform_zone_slider != null)
                {
                    float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.12f;
                    ui_transform_zone_slider.localScale = new Vector3(scale, scale, 1f);
                }

                yield return null;
            }

            lastZone = toZone;
            RefreshZoneDisplay(toZone);

            if (ui_transform_zone_items_container != null)
            {
                ui_transform_zone_items_container.anchoredPosition = containerDefaultPos;
            }

            if (ui_transform_zone_slider != null)
            {
                ui_transform_zone_slider.localScale = Vector3.one;
            }

            slideCoroutine = null;
        }

        public void RefreshZoneDisplay(int currentZone)
        {
            if (itemViews == null || itemViews.Count == 0) return;

            int centerIndex = itemViews.Count / 2;

            for (int i = 0; i < itemViews.Count; i++)
            {
                int zoneNum = currentZone + (i - centerIndex);
                bool isCurrent = (i == centerIndex);
                bool isPast = (zoneNum < currentZone);
                bool isSafe = IsSafeZone(zoneNum);
                bool isSuper = IsSuperZone(zoneNum);

                itemViews[i].SetZone(zoneNum, isCurrent, isPast, isSafe, isSuper);
            }

            // Update current zone marker (background + frame + number)
            if (ui_text_slider_number_value != null)
            {
                ui_text_slider_number_value.text = currentZone.ToString();
            }

            bool currentIsSuper = IsSuperZone(currentZone);
            bool currentIsSafe = IsSafeZone(currentZone);

            if (ui_image_slider_bg != null)
            {
                if (currentIsSuper)
                {
                    // Radiant Gold fill
                    ui_image_slider_bg.color = new Color(1f, 0.85f, 0.2f, 1f);
                }
                else if (currentIsSafe)
                {
                    // PURE VIBRANT GREEN fill (never merges with blue!)
                    ui_image_slider_bg.color = new Color(0.18f, 0.95f, 0.25f, 1f);
                }
                else
                {
                    // Crisp silver/white fill (matches target reference)
                    ui_image_slider_bg.color = Color.white;
                }
            }

            if (ui_image_slider_frame != null)
            {
                if (currentIsSuper)
                {
                    ui_image_slider_frame.color = new Color(1f, 0.75f, 0.1f, 1f);
                }
                else if (currentIsSafe)
                {
                    ui_image_slider_frame.color = new Color(0.12f, 0.8f, 0.18f, 1f);
                }
                else
                {
                    ui_image_slider_frame.color = new Color(0.92f, 0.92f, 0.92f, 1f);
                }
            }

            if (ui_text_slider_number_value != null)
            {
                // Crisp bold charcoal black number
                ui_text_slider_number_value.color = new Color(0.12f, 0.12f, 0.12f, 1f);
            }
        }

        public void SetMarkerSprites(Sprite bgSprite, Sprite frameSprite)
        {
            sliderBgSprite = bgSprite;
            sliderFrameSprite = frameSprite;
            if (ui_image_slider_bg != null && bgSprite != null)
            {
                ui_image_slider_bg.sprite = bgSprite;
                ui_image_slider_bg.type = Image.Type.Sliced;
            }
            if (ui_image_slider_frame != null && frameSprite != null)
            {
                ui_image_slider_frame.sprite = frameSprite;
                ui_image_slider_frame.type = Image.Type.Sliced;
            }
        }

        public void SetSprites(Sprite defaultSprite, Sprite superSprite)
        {
            SetMarkerSprites(defaultSprite, null);
        }

        public void SetItemViews(List<UIZoneNumberItemView> items)
        {
            itemViews = items;
        }
    }
}
