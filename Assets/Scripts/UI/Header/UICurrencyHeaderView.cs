using TMPro;
using UnityEngine;
using WheelOfFortune.Core;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Header
{
    public class UICurrencyHeaderView : UIAutoBinder
    {
        [Header("Currency Texts (Ending with _value)")]
        [SerializeField] private TextMeshProUGUI ui_text_cash_value;
        [SerializeField] private TextMeshProUGUI ui_text_gold_value;

        [Header("Dependencies")]
        [SerializeField] private PlayerInventoryManager playerInventoryManager;

        protected override void AutoBindReferences()
        {
            if (ui_text_cash_value == null)
            {
                ui_text_cash_value = FindTextByName("ui_text_cash_value");
            }
            if (ui_text_gold_value == null)
            {
                ui_text_gold_value = FindTextByName("ui_text_gold_value");
            }
            if (playerInventoryManager == null)
            {
                playerInventoryManager = FindFirstObjectByType<PlayerInventoryManager>();
            }
        }

        protected override void RegisterButtonCallbacks()
        {
            if (playerInventoryManager != null)
            {
                playerInventoryManager.OnWalletBalanceChanged += OnBalanceChanged;
            }
        }

        protected override void UnregisterButtonCallbacks()
        {
            if (playerInventoryManager != null)
            {
                playerInventoryManager.OnWalletBalanceChanged -= OnBalanceChanged;
            }
        }

        private void Start()
        {
            if (playerInventoryManager != null)
            {
                UpdateBalances(playerInventoryManager.PermanentCash, playerInventoryManager.PermanentGold);
            }
        }

        private void OnBalanceChanged(int cash, int gold)
        {
            UpdateBalances(cash, gold);
        }

        public void UpdateBalances(int cash, int gold)
        {
            if (ui_text_cash_value != null)
            {
                ui_text_cash_value.text = cash.ToString("N0");
            }
            if (ui_text_gold_value != null)
            {
                ui_text_gold_value.text = gold.ToString("N0");
            }
        }
    }
}
