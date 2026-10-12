using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Core;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Modals
{
    public class UICashOutModalView : UIAutoBinder
    {
        [Header("Modal Transforms")]
        [SerializeField] private GameObject ui_panel_cashout_root;
        [SerializeField] private RectTransform ui_transform_cashout_modal_anim_container;

        [Header("Texts (Ending with _value)")]
        [SerializeField] private TextMeshProUGUI ui_text_cashout_title_value;
        [SerializeField] private TextMeshProUGUI ui_text_cashout_subtitle_value;
        [SerializeField] private TextMeshProUGUI ui_text_cashout_summary_value;
        [SerializeField] private TextMeshProUGUI ui_text_cash_gained_value;
        [SerializeField] private TextMeshProUGUI ui_text_gold_gained_value;
        [SerializeField] private TextMeshProUGUI ui_text_empty_hint_value;

        [Header("Horizontal Side-Scroll Box")]
        [SerializeField] private ScrollRect ui_scroll_rect_rewards;
        [SerializeField] private RectTransform ui_transform_rewards_content;
        [SerializeField] private Scrollbar ui_scrollbar_horizontal;
        [SerializeField] private UIExitRewardCardItemView rewardCardPrefab;

        [Header("Buttons")]
        [SerializeField] private Button ui_button_exit_cancel;
        [SerializeField] private Button ui_button_exit_confirm;
        [SerializeField] private Button ui_button_cashout_claim;

        [Header("Dependencies")]
        [SerializeField] private GameFlowManager gameFlowManager;
        [SerializeField] private PlayerInventoryManager playerInventoryManager;

        private readonly List<UIExitRewardCardItemView> activeCards = new();

        public UIExitRewardCardItemView RewardCardPrefab => rewardCardPrefab;
        public void SetRewardCardPrefab(UIExitRewardCardItemView prefab) => rewardCardPrefab = prefab;

        public void InitializeView()
        {
            AutoBindReferences();
            RegisterButtonCallbacks();
            InitializeScrollBox();
            HideModal();
        }

        protected override void Awake()
        {
            base.Awake();
            InitializeView();
        }

        protected override void AutoBindReferences()
        {
            if (ui_panel_cashout_root == null)
            {
                var t = FindChildComponent<Transform>("ui_panel_cashout_root");
                if (t != null) ui_panel_cashout_root = t.gameObject;
                else ui_panel_cashout_root = gameObject;
            }
            if (ui_transform_cashout_modal_anim_container == null)
            {
                ui_transform_cashout_modal_anim_container = FindChildComponent<RectTransform>("ui_transform_cashout_modal_anim_container");
            }
            if (ui_text_cashout_title_value == null)
            {
                ui_text_cashout_title_value = FindTextByName("ui_text_cashout_title_value");
            }
            if (ui_text_cashout_subtitle_value == null)
            {
                ui_text_cashout_subtitle_value = FindTextByName("ui_text_cashout_subtitle_value");
            }
            if (ui_text_cashout_summary_value == null)
            {
                ui_text_cashout_summary_value = FindTextByName("ui_text_cashout_summary_value");
            }
            if (ui_text_cash_gained_value == null)
            {
                ui_text_cash_gained_value = FindTextByName("ui_text_cash_gained_value");
            }
            if (ui_text_gold_gained_value == null)
            {
                ui_text_gold_gained_value = FindTextByName("ui_text_gold_gained_value");
            }
            if (ui_text_empty_hint_value == null)
            {
                ui_text_empty_hint_value = FindTextByName("ui_text_empty_hint_value");
            }

            if (ui_scroll_rect_rewards == null)
            {
                ui_scroll_rect_rewards = GetComponentInChildren<ScrollRect>(true);
            }
            if (ui_transform_rewards_content == null)
            {
                ui_transform_rewards_content = FindChildComponent<RectTransform>("ui_transform_rewards_content");
            }
            if (ui_scrollbar_horizontal == null)
            {
                ui_scrollbar_horizontal = GetComponentInChildren<Scrollbar>(true);
            }

            if (ui_button_exit_cancel == null)
            {
                ui_button_exit_cancel = FindButtonByName("ui_button_exit_cancel");
            }
            if (ui_button_exit_confirm == null)
            {
                ui_button_exit_confirm = FindButtonByName("ui_button_exit_confirm");
                if (ui_button_exit_confirm == null)
                {
                    ui_button_exit_confirm = FindButtonByName("ui_button_cashout_claim");
                }
            }
            if (ui_button_cashout_claim == null)
            {
                ui_button_cashout_claim = FindButtonByName("ui_button_cashout_claim");
            }

            if (gameFlowManager == null)
            {
                gameFlowManager = FindFirstObjectByType<GameFlowManager>();
            }
            if (playerInventoryManager == null)
            {
                playerInventoryManager = FindFirstObjectByType<PlayerInventoryManager>();
            }
        }

        protected override void RegisterButtonCallbacks()
        {
            if (ui_button_exit_cancel != null)
            {
                ui_button_exit_cancel.onClick.AddListener(OnCancelClicked);
            }
            if (ui_button_exit_confirm != null)
            {
                ui_button_exit_confirm.onClick.AddListener(OnConfirmExitClicked);
            }
            if (ui_button_cashout_claim != null && ui_button_cashout_claim != ui_button_exit_confirm)
            {
                ui_button_cashout_claim.onClick.AddListener(OnConfirmExitClicked);
            }

            if (gameFlowManager != null)
            {
                gameFlowManager.OnExitConfirmationRequested += ShowModal;
            }
        }

        protected override void UnregisterButtonCallbacks()
        {
            if (ui_button_exit_cancel != null)
            {
                ui_button_exit_cancel.onClick.RemoveListener(OnCancelClicked);
            }
            if (ui_button_exit_confirm != null)
            {
                ui_button_exit_confirm.onClick.RemoveListener(OnConfirmExitClicked);
            }
            if (ui_button_cashout_claim != null && ui_button_cashout_claim != ui_button_exit_confirm)
            {
                ui_button_cashout_claim.onClick.RemoveListener(OnConfirmExitClicked);
            }

            if (gameFlowManager != null)
            {
                gameFlowManager.OnExitConfirmationRequested -= ShowModal;
            }
        }


        public void InitializeScrollBox()
        {
            if (ui_scroll_rect_rewards == null)
            {
                ui_scroll_rect_rewards = GetComponentInChildren<ScrollRect>(true);
            }

            if (ui_scroll_rect_rewards != null)
            {
                ui_scroll_rect_rewards.horizontal = true;
                ui_scroll_rect_rewards.vertical = false;
                ui_scroll_rect_rewards.movementType = ScrollRect.MovementType.Elastic;
                ui_scroll_rect_rewards.elasticity = 0.1f;
                ui_scroll_rect_rewards.inertia = true;
                ui_scroll_rect_rewards.decelerationRate = 0.135f;
                ui_scroll_rect_rewards.scrollSensitivity = 35f;

                if (ui_scrollbar_horizontal != null)
                {
                    ui_scroll_rect_rewards.horizontalScrollbar = ui_scrollbar_horizontal;
                    ui_scroll_rect_rewards.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                    ui_scroll_rect_rewards.horizontalScrollbarSpacing = 4f;
                }

                // Ensure Viewport has RectMask2D and transparent raycast target Image
                RectTransform viewportRT = ui_scroll_rect_rewards.viewport;
                if (viewportRT == null && ui_transform_rewards_content != null)
                {
                    viewportRT = ui_transform_rewards_content.parent as RectTransform;
                    ui_scroll_rect_rewards.viewport = viewportRT;
                }

                if (viewportRT != null)
                {
                    if (!viewportRT.TryGetComponent<RectMask2D>(out _))
                    {
                        viewportRT.gameObject.AddComponent<RectMask2D>();
                    }

                    var vpImage = viewportRT.GetComponent<Image>() ?? viewportRT.gameObject.AddComponent<Image>();
                    vpImage.color = Color.clear;
                    vpImage.raycastTarget = true;
                    vpImage.maskable = false;
                }

                // Ensure Content Layout
                if (ui_transform_rewards_content != null)
                {
                    ui_scroll_rect_rewards.content = ui_transform_rewards_content;

                    var hlg = ui_transform_rewards_content.GetComponent<HorizontalLayoutGroup>() ?? ui_transform_rewards_content.gameObject.AddComponent<HorizontalLayoutGroup>();
                    hlg.childAlignment = TextAnchor.MiddleLeft;
                    hlg.childControlWidth = false;
                    hlg.childControlHeight = true;
                    hlg.childForceExpandWidth = false;
                    hlg.childForceExpandHeight = true;
                    hlg.spacing = 14f;
                    hlg.padding = new RectOffset(16, 16, 12, 12);

                    var csf = ui_transform_rewards_content.GetComponent<ContentSizeFitter>() ?? ui_transform_rewards_content.gameObject.AddComponent<ContentSizeFitter>();
                    csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                }
            }
        }

        public void ShowModal()
        {
            if (ui_panel_cashout_root != null)
            {
                ui_panel_cashout_root.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }

            if (gameFlowManager == null) gameFlowManager = FindFirstObjectByType<GameFlowManager>();
            if (playerInventoryManager == null) playerInventoryManager = FindFirstObjectByType<PlayerInventoryManager>();

            int currentZone = gameFlowManager != null ? gameFlowManager.ZoneManager.CurrentZone : 1;

            if (ui_text_cashout_title_value != null)
            {
                ui_text_cashout_title_value.text = "EXIT & SECURE LOOT?";
            }

            if (ui_text_cashout_subtitle_value != null)
            {
                ui_text_cashout_subtitle_value.text = $"Walking away at Zone {currentZone}. All earned rewards will be secured in your permanent vault!";
            }

            int pendingCash = playerInventoryManager != null ? playerInventoryManager.GetPendingRunCash() : 0;
            int pendingGold = playerInventoryManager != null ? playerInventoryManager.GetPendingRunGold() : 0;

            if (ui_text_cash_gained_value != null)
            {
                ui_text_cash_gained_value.text = pendingCash > 0 ? $"+${pendingCash:N0}" : "$0";
            }

            if (ui_text_gold_gained_value != null)
            {
                ui_text_gold_gained_value.text = pendingGold > 0 ? $"+{pendingGold:N0}" : "0";
            }

            RebuildRewardCards();

            if (ui_transform_cashout_modal_anim_container != null)
            {
                ui_transform_cashout_modal_anim_container.DOKill();
                ui_transform_cashout_modal_anim_container.localScale = Vector3.one * 0.7f;
                ui_transform_cashout_modal_anim_container.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
            }
        }

        private void RebuildRewardCards()
        {
            if (playerInventoryManager == null || ui_transform_rewards_content == null) return;

            var rewards = playerInventoryManager.CurrentRunRewards;
            bool hasRewards = (rewards != null && rewards.Count > 0);

            if (ui_text_empty_hint_value != null)
            {
                ui_text_empty_hint_value.gameObject.SetActive(!hasRewards);
                if (!hasRewards)
                {
                    ui_text_empty_hint_value.text = "No rewards collected in this run yet.";
                }
            }

            if (rewardCardPrefab == null) return;

            int count = hasRewards ? rewards.Count : 0;

            // Instantiate cards as needed
            while (activeCards.Count < count)
            {
                var newCard = Instantiate(rewardCardPrefab, ui_transform_rewards_content);
                newCard.name = $"ui_exit_card_{activeCards.Count}";
                EnsureMaskable(newCard.gameObject);
                activeCards.Add(newCard);
            }

            for (int i = 0; i < activeCards.Count; i++)
            {
                if (i < count)
                {
                    activeCards[i].gameObject.SetActive(true);
                    EnsureMaskable(activeCards[i].gameObject);
                    activeCards[i].SetReward(rewards[i]);
                }
                else
                {
                    activeCards[i].gameObject.SetActive(false);
                }
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(ui_transform_rewards_content);

            if (ui_scroll_rect_rewards != null)
            {
                ui_scroll_rect_rewards.horizontalNormalizedPosition = 0f;
            }
        }

        private void EnsureMaskable(GameObject go)
        {
            if (go == null) return;
            var images = go.GetComponentsInChildren<Image>(true);
            foreach (var img in images) img.maskable = true;
            var texts = go.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var txt in texts) txt.maskable = true;
        }

        public void HideModal()
        {
            if (ui_transform_cashout_modal_anim_container != null)
            {
                ui_transform_cashout_modal_anim_container.DOKill();
            }
            if (ui_panel_cashout_root != null)
            {
                ui_panel_cashout_root.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnCancelClicked()
        {
            HideModal();
        }

        private void OnConfirmExitClicked()
        {
            HideModal();
            if (gameFlowManager == null) gameFlowManager = FindFirstObjectByType<GameFlowManager>();
            if (gameFlowManager != null)
            {
                gameFlowManager.ConfirmExitAndCashOut();
            }
        }

        public void SetReferences(Button cancelBtn, Button confirmBtn, ScrollRect scrollRect, RectTransform content, UIExitRewardCardItemView cardPf)
        {
            ui_button_exit_cancel = cancelBtn;
            ui_button_exit_confirm = confirmBtn;
            ui_scroll_rect_rewards = scrollRect;
            ui_transform_rewards_content = content;
            rewardCardPrefab = cardPf;
        }
    }
}
