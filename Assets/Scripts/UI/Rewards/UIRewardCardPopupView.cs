using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Core;
using WheelOfFortune.Data;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Rewards
{
    public class UIRewardCardPopupView : UIAutoBinder
    {
        [Header("Card Transforms")]
        [SerializeField] private GameObject ui_panel_card_root;
        [SerializeField] private RectTransform ui_transform_card_anim_container;
        [SerializeField] private RectTransform ui_transform_card_glow;

        [Header("Card UI Elements (Names ending with _value)")]
        [SerializeField] private Image ui_image_card_frame;
        [SerializeField] private Image ui_image_card_bg;
        [SerializeField] private Image ui_image_card_icon;
        [SerializeField] private Image ui_image_card_glow;
        [SerializeField] private TextMeshProUGUI ui_text_card_title_value;
        [SerializeField] private TextMeshProUGUI ui_text_card_amount_value;

        [Header("Flying Tokens Setup")]
        [SerializeField] private RectTransform ui_transform_tokens_container;
        [SerializeField] private GameObject tokenPrefab;
        [SerializeField] private int tokenCount = 4;
        [SerializeField] private float flightDuration = 0.55f;

        [Header("Dependencies")]
        [SerializeField] private GameFlowManager gameFlowManager;
        [SerializeField] private UIRewardsTrayView rewardsTrayView;

        private readonly List<GameObject> activeTokens = new();
        private Tween glowRotateTween;

        protected override void Awake()
        {
            base.Awake();
            if (ui_transform_card_anim_container == null)
            {
                ui_transform_card_anim_container = FindChildComponent<RectTransform>("ui_transform_card_anim_container");
            }
            if (ui_panel_card_root == null)
            {
                ui_panel_card_root = ui_transform_card_anim_container != null ? ui_transform_card_anim_container.gameObject : null;
            }
            if (ui_panel_card_root != null)
            {
                ui_panel_card_root.SetActive(false);
            }
        }

        protected override void AutoBindReferences()
        {
            if (ui_transform_card_anim_container == null)
            {
                ui_transform_card_anim_container = FindChildComponent<RectTransform>("ui_transform_card_anim_container");
            }
            if (ui_panel_card_root == null)
            {
                ui_panel_card_root = ui_transform_card_anim_container != null ? ui_transform_card_anim_container.gameObject : null;
            }
            if (ui_transform_card_glow == null)
            {
                ui_transform_card_glow = FindChildComponent<RectTransform>("ui_transform_card_glow");
            }
            if (ui_transform_tokens_container == null)
            {
                ui_transform_tokens_container = FindChildComponent<RectTransform>("ui_transform_tokens_container");
            }

            if (ui_image_card_frame == null) ui_image_card_frame = FindImageByName("ui_image_card_frame");
            if (ui_image_card_bg == null) ui_image_card_bg = FindImageByName("ui_image_card_bg");
            if (ui_image_card_icon == null) ui_image_card_icon = FindImageByName("ui_image_card_icon");
            if (ui_image_card_glow == null) ui_image_card_glow = FindImageByName("ui_image_card_glow");

            if (ui_text_card_title_value == null) ui_text_card_title_value = FindTextByName("ui_text_card_title_value");
            if (ui_text_card_amount_value == null) ui_text_card_amount_value = FindTextByName("ui_text_card_amount_value");

            if (gameFlowManager == null) gameFlowManager = FindFirstObjectByType<GameFlowManager>();
            if (rewardsTrayView == null) rewardsTrayView = FindFirstObjectByType<UIRewardsTrayView>();
        }

        protected override void RegisterButtonCallbacks() { }
        protected override void UnregisterButtonCallbacks() { }

        private void Start()
        {
            if (ui_panel_card_root != null)
            {
                ui_panel_card_root.SetActive(false);
            }
        }

        public void SetCardRoot(GameObject root)
        {
            ui_panel_card_root = root;
        }

        public void PlayRewardCelebration(RewardItemSO item, int amount, Action onAnimationFinished)
        {
            if (gameObject != null && !gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }
            if (ui_panel_card_root != null)
            {
                ui_panel_card_root.SetActive(true);
            }
            StartCoroutine(RewardCelebrationRoutine(item, amount, onAnimationFinished));
        }

        private IEnumerator RewardCelebrationRoutine(RewardItemSO item, int amount, Action onFinished)
        {
            if (item == null || ui_panel_card_root == null || ui_transform_card_anim_container == null)
            {
                onFinished?.Invoke();
                yield break;
            }

            // 1. Setup Card Visuals
            ui_panel_card_root.SetActive(true);

            if (ui_text_card_title_value != null)
            {
                ui_text_card_title_value.text = item.DisplayName.ToUpper();
            }

            if (ui_text_card_amount_value != null)
            {
                ui_text_card_amount_value.text = $"x{amount}";
            }

            if (ui_image_card_icon != null)
            {
                ui_image_card_icon.gameObject.SetActive(true);
                ui_image_card_icon.sprite = item.Icon;
                ui_image_card_icon.color = Color.white;
                ui_image_card_icon.preserveAspect = true;
            }

            // 2. Animate Card Pop-in with Ease.OutBack
            ui_transform_card_anim_container.DOKill();
            ui_transform_card_anim_container.localScale = Vector3.zero;
            ui_transform_card_anim_container.DOScale(Vector3.one, 0.45f).SetEase(Ease.OutBack);

            // Rotate ambient glow if active
            if (ui_transform_card_glow != null && ui_transform_card_glow.gameObject.activeSelf)
            {
                glowRotateTween?.Kill();
                ui_transform_card_glow.localEulerAngles = Vector3.zero;
                glowRotateTween = ui_transform_card_glow
                    .DOLocalRotate(new Vector3(0, 0, 360f), 8f, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1);
            }

            // Hold card display for player excitement
            yield return new WaitForSeconds(0.7f);

            // 3. Determine target position on left tray
            Vector3 targetScreenPos = GetTargetTrayPosition();

            // 4. Spawn flying icon tokens along arc
            int count = Mathf.Clamp(tokenCount, 3, 6);
            Vector3 startPos = ui_transform_card_anim_container.position;

            for (int i = 0; i < count; i++)
            {
                SpawnAndLaunchFlyingToken(item.Icon, startPos, targetScreenPos, i * 0.08f);
            }

            // Wait for flight duration
            yield return new WaitForSeconds(flightDuration + (count * 0.08f) + 0.1f);

            // 5. Shrink Card away
            ui_transform_card_anim_container.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.3f);

            glowRotateTween?.Kill();
            if (ui_image_card_icon != null)
            {
                ui_image_card_icon.gameObject.SetActive(false);
            }
            ui_panel_card_root.SetActive(false);

            onFinished?.Invoke();
        }

        private Vector3 GetTargetTrayPosition()
        {
            if (rewardsTrayView != null)
            {
                var rt = rewardsTrayView.GetComponent<RectTransform>();
                if (rt != null)
                {
                    // Target near top-center of tray (where exit button & slots sit)
                    return rt.TransformPoint(new Vector3(rt.rect.width * 0.5f, -120f, 0f));
                }
            }
            // Fallback left side
            return new Vector3(Screen.width * 0.12f, Screen.height * 0.65f, 0f);
        }

        private void SpawnAndLaunchFlyingToken(Sprite icon, Vector3 startWorldPos, Vector3 targetWorldPos, float delay)
        {
            if (ui_transform_tokens_container == null) return;

            GameObject tokenGO;
            if (tokenPrefab != null)
            {
                tokenGO = Instantiate(tokenPrefab, ui_transform_tokens_container);
            }
            else
            {
                // Dynamic fallback if prefab reference is missing
                tokenGO = new GameObject("ui_token_fly_instance", typeof(RectTransform), typeof(Image));
                tokenGO.transform.SetParent(ui_transform_tokens_container, false);
                var rt = tokenGO.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(48f, 48f);
            }

            tokenGO.SetActive(false);
            tokenGO.transform.position = startWorldPos;
            tokenGO.transform.localScale = Vector3.one * 0.4f;

            // Find all Images in token hierarchy
            var allImages = tokenGO.GetComponentsInChildren<Image>(true);
            Transform iconChild = tokenGO.transform.Find("ui_image_token_icon");

            if (iconChild != null && iconChild.TryGetComponent<Image>(out var childIconImg))
            {
                childIconImg.sprite = icon;
                childIconImg.color = Color.white;
                childIconImg.preserveAspect = true;
                childIconImg.enabled = true;
                childIconImg.gameObject.SetActive(true);

                // Disable any background frame Image on the parent so only the item sprite flies
                var rootImg = tokenGO.GetComponent<Image>();
                if (rootImg != null)
                {
                    rootImg.enabled = false;
                }
            }
            else
            {
                // Direct root Image
                var rootImg = tokenGO.GetComponent<Image>();
                if (rootImg != null)
                {
                    rootImg.sprite = icon;
                    rootImg.color = Color.white;
                    rootImg.preserveAspect = true;
                    rootImg.enabled = true;
                }
            }

            // Ensure NO unassigned Image component renders as a solid white square
            foreach (var img in allImages)
            {
                if (img.sprite == null)
                {
                    img.enabled = false;
                }
            }

            StartCoroutine(LaunchTokenRoutine(tokenGO, startWorldPos, targetWorldPos, delay));
        }

        private IEnumerator LaunchTokenRoutine(GameObject tokenGO, Vector3 startPos, Vector3 targetPos, float delay)
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            if (tokenGO == null) yield break;
            tokenGO.SetActive(true);

            // Random slight scatter at start
            Vector3 scatteredStart = startPos + new Vector3(UnityEngine.Random.Range(-40f, 40f), UnityEngine.Random.Range(-40f, 40f), 0f);
            tokenGO.transform.position = scatteredStart;

            // Scale pop
            tokenGO.transform.DOScale(Vector3.one * 1.1f, 0.15f).SetEase(Ease.OutBack);

            // Flying path: arc trajectory
            Vector3 midPoint = (scatteredStart + targetPos) * 0.5f + new Vector3(0f, UnityEngine.Random.Range(50f, 120f), 0f);
            Vector3[] path = new Vector3[] { scatteredStart, midPoint, targetPos };

            tokenGO.transform.DOPath(path, flightDuration, PathType.CatmullRom)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    if (tokenGO != null)
                    {
                        tokenGO.transform.DOScale(Vector3.zero, 0.12f).OnComplete(() =>
                        {
                            if (tokenGO != null)
                            {
                                Destroy(tokenGO);
                            }
                        });
                    }
                });
        }

        public void SetTokenPrefab(GameObject prefab)
        {
            tokenPrefab = prefab;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            glowRotateTween?.Kill();
        }
    }
}
