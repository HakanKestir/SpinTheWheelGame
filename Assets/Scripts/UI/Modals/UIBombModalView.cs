using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Core;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Modals
{
    public class UIBombModalView : UIAutoBinder
    {
        [Header("Modal Transforms")]
        [SerializeField] private GameObject ui_panel_bomb_root;
        [SerializeField] private RectTransform ui_transform_bomb_modal_anim_container;
        [SerializeField] private RectTransform ui_transform_skull_icon;

        [Header("Buttons")]
        [SerializeField] private Button ui_button_give_up;
        [SerializeField] private Button ui_button_revive_currency;
        [SerializeField] private Button ui_button_revive_bonus;

        [Header("Texts (Ending with _value)")]
        [SerializeField] private TextMeshProUGUI ui_text_revive_cost_value;

        [Header("Dependencies")]
        [SerializeField] private GameFlowManager gameFlowManager;
        [SerializeField] private PlayerInventoryManager playerInventoryManager;

        protected override void Awake()
        {
            base.Awake();
            HideModal();
        }

        protected override void AutoBindReferences()
        {
            if (ui_panel_bomb_root == null)
            {
                var t = FindChildComponent<Transform>("ui_panel_bomb_root");
                if (t != null) ui_panel_bomb_root = t.gameObject;
            }
            if (ui_transform_bomb_modal_anim_container == null)
            {
                ui_transform_bomb_modal_anim_container = FindChildComponent<RectTransform>("ui_transform_bomb_modal_anim_container");
            }
            if (ui_transform_skull_icon == null)
            {
                ui_transform_skull_icon = FindChildComponent<RectTransform>("ui_transform_skull_icon");
            }
            if (ui_button_give_up == null)
            {
                ui_button_give_up = FindButtonByName("ui_button_give_up");
            }
            if (ui_button_revive_currency == null)
            {
                ui_button_revive_currency = FindButtonByName("ui_button_revive_currency");
            }
            if (ui_button_revive_bonus == null)
            {
                ui_button_revive_bonus = FindButtonByName("ui_button_revive_bonus");
            }
            if (ui_text_revive_cost_value == null)
            {
                ui_text_revive_cost_value = FindTextByName("ui_text_revive_cost_value");
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
            if (ui_button_give_up != null)
            {
                ui_button_give_up.onClick.AddListener(OnGiveUpClicked);
            }
            if (ui_button_revive_currency != null)
            {
                ui_button_revive_currency.onClick.AddListener(OnReviveCurrencyClicked);
            }
            if (ui_button_revive_bonus != null)
            {
                ui_button_revive_bonus.onClick.AddListener(OnReviveBonusClicked);
            }

            if (gameFlowManager != null)
            {
                gameFlowManager.OnBombHit += ShowModal;
                gameFlowManager.OnRevived += HideModal;
                gameFlowManager.OnRunReset += HideModal;
            }
        }

        protected override void UnregisterButtonCallbacks()
        {
            if (ui_button_give_up != null)
            {
                ui_button_give_up.onClick.RemoveListener(OnGiveUpClicked);
            }
            if (ui_button_revive_currency != null)
            {
                ui_button_revive_currency.onClick.RemoveListener(OnReviveCurrencyClicked);
            }
            if (ui_button_revive_bonus != null)
            {
                ui_button_revive_bonus.onClick.RemoveListener(OnReviveBonusClicked);
            }

            if (gameFlowManager != null)
            {
                gameFlowManager.OnBombHit -= ShowModal;
                gameFlowManager.OnRevived -= HideModal;
                gameFlowManager.OnRunReset -= HideModal;
            }
        }

        private void Start()
        {
            HideModal();
        }

        public void ShowModal()
        {
            if (ui_panel_bomb_root != null)
            {
                ui_panel_bomb_root.SetActive(true);
            }

            if (ui_text_revive_cost_value != null && gameFlowManager != null)
            {
                int cost = gameFlowManager.ZoneManager.Config.ReviveGoldCost;
                ui_text_revive_cost_value.text = cost.ToString();

                // Check affordability
                bool canAfford = playerInventoryManager != null && playerInventoryManager.CanAffordGold(cost);
                if (ui_button_revive_currency != null)
                {
                    ui_button_revive_currency.interactable = canAfford;
                }
            }

            // Pop-in animation
            if (ui_transform_bomb_modal_anim_container != null)
            {
                ui_transform_bomb_modal_anim_container.DOKill();
                ui_transform_bomb_modal_anim_container.localScale = Vector3.one * 0.7f;
                ui_transform_bomb_modal_anim_container.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
            }

            // Skull pulsing animation
            if (ui_transform_skull_icon != null)
            {
                ui_transform_skull_icon.DOKill();
                ui_transform_skull_icon.localScale = Vector3.one;
                ui_transform_skull_icon.DOScale(Vector3.one * 1.15f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }
        }

        public void HideModal()
        {
            if (ui_transform_bomb_modal_anim_container != null)
            {
                ui_transform_bomb_modal_anim_container.DOKill();
            }
            if (ui_transform_skull_icon != null)
            {
                ui_transform_skull_icon.DOKill();
            }
            if (ui_panel_bomb_root != null)
            {
                ui_panel_bomb_root.SetActive(false);
            }
        }

        private void OnGiveUpClicked()
        {
            if (gameFlowManager != null)
            {
                gameFlowManager.RequestGiveUp();
            }
        }

        private void OnReviveCurrencyClicked()
        {
            if (gameFlowManager != null)
            {
                gameFlowManager.RequestReviveWithCurrency();
            }
        }

        private void OnReviveBonusClicked()
        {
            if (gameFlowManager != null)
            {
                gameFlowManager.RequestReviveBonus();
            }
        }
    }
}
