using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Core;
using WheelOfFortune.Data;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Rewards
{
    public class UIRewardsTrayView : UIAutoBinder
    {
        [Header("Tray UI Elements")]
        [SerializeField] private Button ui_button_exit;
        [SerializeField] private RectTransform ui_transform_tray_content;
        [SerializeField] private ScrollRect ui_scroll_rect_rewards;
        [SerializeField] private Scrollbar ui_scrollbar_tray;
        [SerializeField] private UIRewardTraySlotItemView slotPrefab;

        [Header("Dependencies")]
        [SerializeField] private GameFlowManager gameFlowManager;
        [SerializeField] private ZoneProgressionManager zoneProgressionManager;
        [SerializeField] private PlayerInventoryManager playerInventoryManager;

        private readonly List<UIRewardTraySlotItemView> activeSlots = new();

        public Button ExitButton => ui_button_exit;
        public ScrollRect RewardsScrollRect => ui_scroll_rect_rewards;

        protected override void AutoBindReferences()
        {
            if (ui_button_exit == null)
            {
                ui_button_exit = FindButtonByName("ui_button_exit");
            }
            if (ui_transform_tray_content == null)
            {
                var t = FindChildComponent<RectTransform>("ui_transform_tray_content");
                if (t != null) ui_transform_tray_content = t;
            }
            if (ui_scroll_rect_rewards == null)
            {
                ui_scroll_rect_rewards = GetComponentInChildren<ScrollRect>(true);
            }
            if (ui_scrollbar_tray == null)
            {
                ui_scrollbar_tray = GetComponentInChildren<Scrollbar>(true);
            }

            if (gameFlowManager == null)
            {
                gameFlowManager = FindFirstObjectByType<GameFlowManager>();
            }
            if (zoneProgressionManager == null)
            {
                zoneProgressionManager = FindFirstObjectByType<ZoneProgressionManager>();
            }
            if (playerInventoryManager == null)
            {
                playerInventoryManager = FindFirstObjectByType<PlayerInventoryManager>();
            }
        }

        protected override void RegisterButtonCallbacks()
        {
            if (ui_button_exit != null)
            {
                ui_button_exit.onClick.AddListener(OnExitClicked);
            }
            EventBus.Subscribe<RewardAddedEvent>(OnRewardAdded);
            EventBus.Subscribe<RunRewardsClearedEvent>(OnRewardsCleared);
            EventBus.Subscribe<RunRewardsRestoredEvent>(OnRewardsRestored);
            EventBus.Subscribe<RunRewardsBankedEvent>(OnRunRewardsBanked);
            EventBus.Subscribe<int>(OnZoneChanged);
            if (gameFlowManager != null)
            {
                gameFlowManager.OnStateChanged += OnGameStateChanged;
                gameFlowManager.OnRunReset += OnRunReset;
            }
        }

        protected override void UnregisterButtonCallbacks()
        {
            if (ui_button_exit != null)
            {
                ui_button_exit.onClick.RemoveListener(OnExitClicked);
            }
            EventBus.Unsubscribe<RewardAddedEvent>(OnRewardAdded);
            EventBus.Unsubscribe<RunRewardsClearedEvent>(OnRewardsCleared);
            EventBus.Unsubscribe<RunRewardsRestoredEvent>(OnRewardsRestored);
            EventBus.Unsubscribe<RunRewardsBankedEvent>(OnRunRewardsBanked);
            EventBus.Unsubscribe<int>(OnZoneChanged);
            if (gameFlowManager != null)
            {
                gameFlowManager.OnStateChanged -= OnGameStateChanged;
                gameFlowManager.OnRunReset -= OnRunReset;
            }
        }

        public void InitializeView()
        {
            AutoBindReferences();
            RefreshButtonCallbacks();
            InitializeScrollTray();
            UpdateExitButtonState();
        }

        protected override void Awake()
        {
            base.Awake();
            InitializeView();
        }

        private void Start()
        {
            UpdateExitButtonState();
        }

        public void InitializeScrollTray()
        {
            if (ui_scroll_rect_rewards == null)
            {
                ui_scroll_rect_rewards = GetComponentInChildren<ScrollRect>(true);
            }
            if (ui_scrollbar_tray == null)
            {
                ui_scrollbar_tray = GetComponentInChildren<Scrollbar>(true);
            }
            if (ui_transform_tray_content == null)
            {
                ui_transform_tray_content = FindChildComponent<RectTransform>("ui_transform_tray_content");
            }

            if (ui_scroll_rect_rewards != null)
            {
                // 1. Ensure Viewport is properly anchored and has raycast target & RectMask2D
                RectTransform viewportRT = ui_scroll_rect_rewards.viewport;
                if (viewportRT == null && ui_transform_tray_content != null)
                {
                    viewportRT = ui_transform_tray_content.parent as RectTransform;
                    ui_scroll_rect_rewards.viewport = viewportRT;
                }

                if (viewportRT != null)
                {
                    viewportRT.anchorMin = Vector2.zero;
                    viewportRT.anchorMax = Vector2.one;
                    viewportRT.pivot = new Vector2(0.5f, 0.5f);
                    viewportRT.offsetMin = Vector2.zero;
                    viewportRT.offsetMax = Vector2.zero;

                    // Ensure RectMask2D clips child items
                    if (!viewportRT.TryGetComponent<RectMask2D>(out _))
                    {
                        viewportRT.gameObject.AddComponent<RectMask2D>();
                    }

                    // Viewport MUST have an Image with raycastTarget = true so pointer dragging & mouse-wheel scroll events hit the ScrollRect!
                    var vpImage = viewportRT.GetComponent<Image>() ?? viewportRT.gameObject.AddComponent<Image>();
                    vpImage.color = Color.clear;
                    vpImage.raycastTarget = true;
                    vpImage.maskable = false;
                }

                // 2. Configure ScrollRect properties
                ui_scroll_rect_rewards.horizontal = false;
                ui_scroll_rect_rewards.vertical = true;
                ui_scroll_rect_rewards.movementType = ScrollRect.MovementType.Elastic;
                ui_scroll_rect_rewards.elasticity = 0.1f;
                ui_scroll_rect_rewards.inertia = true;
                ui_scroll_rect_rewards.decelerationRate = 0.135f;
                ui_scroll_rect_rewards.scrollSensitivity = 35f;
                ui_scroll_rect_rewards.verticalScrollbar = ui_scrollbar_tray;
                ui_scroll_rect_rewards.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                ui_scroll_rect_rewards.verticalScrollbarSpacing = 4f;

                // 3. Content RectTransform & Layout
                if (ui_transform_tray_content != null)
                {
                    ui_scroll_rect_rewards.content = ui_transform_tray_content;
                    ui_transform_tray_content.anchorMin = new Vector2(0f, 1f);
                    ui_transform_tray_content.anchorMax = new Vector2(1f, 1f);
                    ui_transform_tray_content.pivot = new Vector2(0.5f, 1f);
                    ui_transform_tray_content.anchoredPosition = Vector2.zero;

                    var vlg = ui_transform_tray_content.GetComponent<VerticalLayoutGroup>() ?? ui_transform_tray_content.gameObject.AddComponent<VerticalLayoutGroup>();
                    vlg.childAlignment = TextAnchor.UpperCenter;
                    vlg.childControlWidth = true;
                    vlg.childControlHeight = false;
                    vlg.childForceExpandWidth = true;
                    vlg.childForceExpandHeight = false;
                    vlg.spacing = 10f;
                    vlg.padding = new RectOffset(4, 4, 10, 10);

                    var csf = ui_transform_tray_content.GetComponent<ContentSizeFitter>() ?? ui_transform_tray_content.gameObject.AddComponent<ContentSizeFitter>();
                    csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                }

                // 4. Scrollbar and Handles
                if (ui_scrollbar_tray != null)
                {
                    ui_scrollbar_tray.direction = Scrollbar.Direction.BottomToTop;
                    ui_scrollbar_tray.interactable = true;

                    var trackImg = ui_scrollbar_tray.GetComponent<Image>();
                    if (trackImg != null) trackImg.raycastTarget = true;

                    if (ui_scrollbar_tray.handleRect != null)
                    {
                        var handleImg = ui_scrollbar_tray.handleRect.GetComponent<Image>();
                        if (handleImg != null) handleImg.raycastTarget = true;
                    }
                }
            }

            // 5. Ensure Slot template and existing slots have maskable = true
            if (slotPrefab != null)
            {
                EnsureMaskable(slotPrefab.gameObject);
            }
        }

        private void EnsureMaskable(GameObject go)
        {
            if (go == null) return;
            var images = go.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                img.maskable = true;
            }
            var texts = go.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var txt in texts)
            {
                txt.maskable = true;
            }
        }

        private void OnExitClicked()
        {
            if (gameFlowManager == null)
            {
                gameFlowManager = FindFirstObjectByType<GameFlowManager>();
            }

            if (gameFlowManager != null)
            {
                gameFlowManager.RequestExitConfirmation();
            }
        }

        private void OnZoneChanged(int newZone)
        {
            UpdateExitButtonState();
        }

        private void OnGameStateChanged(GameState state)
        {
            UpdateExitButtonState(state);
        }

        public void UpdateExitButtonState(GameState? overrideState = null)
        {
            if (ui_button_exit == null) return;
            if (gameFlowManager == null) gameFlowManager = FindFirstObjectByType<GameFlowManager>();
            if (zoneProgressionManager == null) zoneProgressionManager = FindFirstObjectByType<ZoneProgressionManager>();

            // Player can leave when wheel is not spinning AND when the zone is safe or super zone
            GameState state = overrideState ?? (gameFlowManager != null ? gameFlowManager.CurrentState : GameState.ReadyToSpin);
            bool isReady = (state == GameState.ReadyToSpin);
            bool isZoneAllowed = (zoneProgressionManager == null || zoneProgressionManager.CanLeave);
            ui_button_exit.interactable = isReady && isZoneAllowed;
        }

        private void OnRunRewardsBanked(RunRewardsBankedEvent evt)
        {
            ClearTraySlots();
        }

        private void OnRunReset()
        {
            ClearTraySlots();
            UpdateExitButtonState();
        }

        private void OnRewardAdded(RewardAddedEvent evt)
        {
            RebuildTraySlots(evt.Item != null ? evt.Item.Id : null);
        }

        private void OnRewardsCleared(RunRewardsClearedEvent evt)
        {
            ClearTraySlots();
        }

        private void OnRewardsRestored(RunRewardsRestoredEvent evt)
        {
            RebuildTraySlots(null);
        }

        /// <summary>
        /// Returns the slot the collect tokens should fly to: the row that
        /// already holds this item, or null when the item will be appended.
        /// </summary>
        public UIRewardTraySlotItemView GetCollectTargetSlot(RewardItemSO item)
        {
            return FindActiveSlot(item);
        }

        /// <summary>
        /// Brings a tray slot into the visible viewport so a collect animation
        /// never targets (or updates) a half-hidden row.
        /// </summary>
        public void ScrollToSlot(UIRewardTraySlotItemView slot)
        {
            if (slot == null || ui_scroll_rect_rewards == null) return;
            var viewport = ui_scroll_rect_rewards.viewport;
            var content = ui_scroll_rect_rewards.content;
            if (viewport == null || content == null) return;

            float viewportHeight = viewport.rect.height;
            float contentHeight = content.rect.height;
            if (contentHeight <= viewportHeight)
            {
                ui_scroll_rect_rewards.verticalNormalizedPosition = 1f;
                return;
            }

            var slotRT = slot.transform as RectTransform;
            float slotHeight = slotRT != null ? slotRT.rect.height : 72f;
            // Content is top-anchored: localPosition.y is the (negative) distance
            // from the content top to the slot's top edge.
            float slotTop = -slot.transform.localPosition.y;
            float slotBottom = slotTop + slotHeight;

            float scrollOffset = -content.anchoredPosition.y;
            if (slotBottom > scrollOffset + viewportHeight)
            {
                scrollOffset = slotBottom - viewportHeight;
            }
            if (slotTop < scrollOffset)
            {
                scrollOffset = slotTop;
            }
            scrollOffset = Mathf.Clamp(scrollOffset, 0f, contentHeight - viewportHeight);

            content.anchoredPosition = new Vector2(content.anchoredPosition.x, -scrollOffset);
            float normalized = 1f - scrollOffset / (contentHeight - viewportHeight);
            ui_scroll_rect_rewards.verticalNormalizedPosition = Mathf.Clamp01(normalized);
        }

        public void LockScroll(bool locked)
        {
            if (ui_scroll_rect_rewards == null) return;
            ui_scroll_rect_rewards.enabled = !locked;
            if (ui_scrollbar_tray != null)
            {
                ui_scrollbar_tray.interactable = !locked;
            }
        }

        public Vector3 GetCollectTargetPosition(RewardItemSO item)
        {
            var existingSlot = FindActiveSlot(item);
            if (existingSlot != null)
            {
                return existingSlot.transform.position;
            }

            float slotHeight = slotPrefab != null ? ((RectTransform)slotPrefab.transform).rect.height : 72f;
            RectTransform viewport = ui_scroll_rect_rewards != null ? ui_scroll_rect_rewards.viewport : null;

            if (viewport != null && ui_transform_tray_content != null
                && ui_transform_tray_content.rect.height + slotHeight > viewport.rect.height)
            {
                // Content will auto-scroll to the bottom, so the new row lands at the viewport bottom.
                return viewport.TransformPoint(new Vector3(0f, viewport.rect.yMin + (slotHeight * 0.5f), 0f));
            }

            var lastActiveSlot = GetLastActiveSlot();
            if (lastActiveSlot != null)
            {
                return lastActiveSlot.transform.position + new Vector3(0f, -(slotHeight + 10f), 0f);
            }

            if (ui_transform_tray_content != null)
            {
                return ui_transform_tray_content.TransformPoint(new Vector3(0f, -(slotHeight * 0.5f), 0f));
            }

            return transform.position;
        }

        private UIRewardTraySlotItemView FindActiveSlot(RewardItemSO item)
        {
            if (item == null) return null;

            foreach (var slot in activeSlots)
            {
                if (slot != null && slot.gameObject.activeSelf && slot.CurrentItemId == item.Id)
                {
                    return slot;
                }
            }
            return null;
        }

        private UIRewardTraySlotItemView GetLastActiveSlot()
        {
            for (int i = activeSlots.Count - 1; i >= 0; i--)
            {
                if (activeSlots[i] != null && activeSlots[i].gameObject.activeSelf)
                {
                    return activeSlots[i];
                }
            }
            return null;
        }

        private void RebuildTraySlots(string highlightedItemId)
        {
            if (playerInventoryManager == null || ui_transform_tray_content == null || slotPrefab == null) return;

            var rewards = playerInventoryManager.CurrentRunRewards;

            // Re-use or instantiate slot items
            while (activeSlots.Count < rewards.Count)
            {
                var newSlot = Instantiate(slotPrefab, ui_transform_tray_content);
                newSlot.name = $"ui_slot_{activeSlots.Count}";
                EnsureMaskable(newSlot.gameObject);
                activeSlots.Add(newSlot);
            }

            for (int i = 0; i < activeSlots.Count; i++)
            {
                if (i < rewards.Count)
                {
                    activeSlots[i].gameObject.SetActive(true);
                    EnsureMaskable(activeSlots[i].gameObject);
                    bool shouldAnimate = !string.IsNullOrEmpty(highlightedItemId)
                        && rewards[i].Item != null
                        && rewards[i].Item.Id == highlightedItemId;
                    activeSlots[i].SetReward(rewards[i], shouldAnimate);
                }
                else
                {
                    activeSlots[i].gameObject.SetActive(false);
                }
            }

            // Force layout rebuild so content size fitter updates height immediately
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(ui_transform_tray_content);

            // Auto-scroll so the row that just received a reward is visible,
            // whether it is a freshly appended slot (bottom) or an existing one.
            if (!string.IsNullOrEmpty(highlightedItemId) && ui_scroll_rect_rewards != null)
            {
                UIRewardTraySlotItemView highlightedSlot = null;
                foreach (var slot in activeSlots)
                {
                    if (slot != null && slot.gameObject.activeSelf && slot.CurrentItemId == highlightedItemId)
                    {
                        highlightedSlot = slot;
                        break;
                    }
                }

                if (highlightedSlot != null)
                {
                    ScrollToSlot(highlightedSlot);
                }
                else
                {
                    ui_scroll_rect_rewards.verticalNormalizedPosition = 0f;
                }
            }
        }

        public void ClearTraySlots()
        {
            foreach (var slot in activeSlots)
            {
                if (slot != null)
                {
                    slot.gameObject.SetActive(false);
                }
            }
            if (ui_scroll_rect_rewards != null)
            {
                ui_scroll_rect_rewards.verticalNormalizedPosition = 1f;
            }
        }

        public void SetSlotPrefab(UIRewardTraySlotItemView prefab)
        {
            slotPrefab = prefab;
        }
    }
}
