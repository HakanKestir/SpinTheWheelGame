using System;
using System.Collections;
using UnityEngine;
using WheelOfFortune.Data;

namespace WheelOfFortune.Core
{
    public class GameFlowManager : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private WheelSpinController wheelSpinController;
        [SerializeField] private ZoneProgressionManager zoneProgressionManager;
        [SerializeField] private PlayerInventoryManager playerInventoryManager;
        [SerializeField] private WheelOfFortune.UI.Wheel.UIWheelView wheelView;

        [Header("Editor Debug / Testing")]
        [Tooltip("Set to 0-7 to force a specific slice index, or -1 for random")]
        [SerializeField] private int forceSliceIndex = -1;

        private GameState currentState = GameState.ReadyToSpin;

        public event Action<GameState> OnStateChanged;
        public event Action<WheelSliceConfig, int> OnRewardWon;
        public event Action OnBombHit;
        public event Action OnRevived;
        public event Action OnRunReset;
        public event Action OnCashOutCompleted;
        public event Action OnExitConfirmationRequested;

        public GameState CurrentState => currentState;
        public WheelSpinController SpinController => wheelSpinController;
        public ZoneProgressionManager ZoneManager => zoneProgressionManager;
        public PlayerInventoryManager InventoryManager => playerInventoryManager;

        private void Awake()
        {
            EnsureInputSystemModule();
        }

        private void EnsureInputSystemModule()
        {
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (eventSystem == null)
            {
                eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            }

            if (eventSystem != null)
            {
                var legacy = eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                if (legacy != null)
                {
                    Destroy(legacy);
                }

                if (eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
                {
                    eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                }
            }
        }

        private void Start()
        {
            SetupCurrentZoneWheel();
            SetState(GameState.ReadyToSpin);
        }

        public void SetupCurrentZoneWheel()
        {
            if (zoneProgressionManager == null || wheelSpinController == null) return;

            var activeConfig = zoneProgressionManager.ActiveWheelConfig;
            int multiplier = zoneProgressionManager.CurrentMultiplier;

            if (activeConfig != null)
            {
                activeConfig.RollSlicesForZone(zoneProgressionManager.CurrentZone);
            }

            wheelSpinController.Initialize(activeConfig, multiplier);

            if (wheelView == null)
            {
                wheelView = FindFirstObjectByType<WheelOfFortune.UI.Wheel.UIWheelView>();
            }

            if (wheelView != null && activeConfig != null)
            {
                wheelView.UpdateWheelDisplay(activeConfig, multiplier);
            }
        }

        public void RequestSpin()
        {
            if (currentState != GameState.ReadyToSpin) return;
            if (wheelSpinController == null || wheelSpinController.IsSpinning) return;

            var config = zoneProgressionManager.ActiveWheelConfig;
            if (config == null)
            {
                Debug.LogError("[GameFlowManager] No active wheel config for current zone!");
                return;
            }

            int targetIndex;
            if (forceSliceIndex >= 0 && forceSliceIndex < WheelConfigSO.SliceCount)
            {
                targetIndex = forceSliceIndex;
            }
            else
            {
                targetIndex = config.SelectRandomSliceIndex();
            }

            SetState(GameState.Spinning);
            wheelSpinController.SpinToSlice(targetIndex, OnWheelSpinFinished);
        }

        [SerializeField] private WheelOfFortune.UI.Rewards.UIRewardCardPopupView rewardCardPopupView;

        private void OnWheelSpinFinished(int winningIndex)
        {
            var config = zoneProgressionManager.ActiveWheelConfig;
            var winningSlice = config.GetSlice(winningIndex);

            if (winningSlice == null)
            {
                Debug.LogError($"[GameFlowManager] Invalid slice at index {winningIndex}");
                SetState(GameState.ReadyToSpin);
                return;
            }

            if (winningSlice.IsBomb)
            {
                SetState(GameState.BombEncounter);
                OnBombHit?.Invoke();
            }
            else
            {
                int multiplier = zoneProgressionManager.CurrentMultiplier;
                int wonAmount = winningSlice.GetAmount(multiplier);

                SetState(GameState.RewardAwarded);
                playerInventoryManager.AddReward(winningSlice.RewardItem, wonAmount);
                OnRewardWon?.Invoke(winningSlice, wonAmount);

                if (rewardCardPopupView != null)
                {
                    if (!rewardCardPopupView.gameObject.activeInHierarchy)
                    {
                        rewardCardPopupView.gameObject.SetActive(true);
                    }

                    rewardCardPopupView.PlayRewardCelebration(winningSlice.RewardItem, wonAmount, CompleteRewardPresentation);
                }
                else
                {
                    StartCoroutine(ProcessZoneAdvanceRoutine());
                }
            }
        }

        public void CompleteRewardPresentation()
        {
            zoneProgressionManager.AdvanceZone();
            SetupCurrentZoneWheel();
            SetState(GameState.ReadyToSpin);
        }

        private IEnumerator ProcessZoneAdvanceRoutine()
        {
            yield return new WaitForSeconds(1.2f);
            CompleteRewardPresentation();
        }

        public void RequestReviveWithCurrency()
        {
            if (currentState != GameState.BombEncounter) return;

            int cost = zoneProgressionManager.Config.ReviveGoldCost;
            if (playerInventoryManager.TrySpendGold(cost))
            {
                ExecuteRevive();
            }
            else
            {
                Debug.LogWarning("[GameFlowManager] Not enough gold to revive!");
            }
        }

        public void RequestReviveBonus()
        {
            if (currentState != GameState.BombEncounter) return;
            // Bonus revive for demo evaluation
            ExecuteRevive();
        }

        private void ExecuteRevive()
        {
            // Revive: keep all collected rewards and advance to next zone
            zoneProgressionManager.AdvanceZone();
            SetupCurrentZoneWheel();
            SetState(GameState.ReadyToSpin);
            OnRevived?.Invoke();
        }

        public void RequestGiveUp()
        {
            if (currentState != GameState.BombEncounter) return;

            // Player forfeits all rewards
            playerInventoryManager.ClearRunRewards();
            zoneProgressionManager.ResetToStart();
            SetupCurrentZoneWheel();
            SetState(GameState.ReadyToSpin);
            OnRunReset?.Invoke();
        }

        public void RequestExitConfirmation()
        {
            if (currentState != GameState.ReadyToSpin) return;
            if (zoneProgressionManager != null && !zoneProgressionManager.CanLeave)
            {
                Debug.LogWarning("[GameFlowManager] Exit not allowed at this zone (safe or super zones only)!");
                return;
            }
            OnExitConfirmationRequested?.Invoke();
        }

        public void RequestExit()
        {
            RequestExitConfirmation();
        }

        public void ConfirmExitAndCashOut()
        {
            if (currentState != GameState.ReadyToSpin && currentState != GameState.CashOut) return;

            // 1. Bank cash and gold into permanent wallet (other items discarded)
            if (playerInventoryManager != null)
            {
                playerInventoryManager.BankRewardsToWallet();
            }

            // 2. Reset run progression and wheel to Zone 1
            if (zoneProgressionManager != null)
            {
                zoneProgressionManager.ResetToStart();
            }
            SetupCurrentZoneWheel();

            // 3. Restore state to ready to spin
            SetState(GameState.ReadyToSpin);
            OnRunReset?.Invoke();
            OnCashOutCompleted?.Invoke();
        }

        public void RestartNewRunAfterCashOut()
        {
            ConfirmExitAndCashOut();
        }

        private void SetState(GameState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(currentState);
        }

        public void SetReferences(WheelSpinController spinner, ZoneProgressionManager zoneMgr, PlayerInventoryManager invMgr, WheelOfFortune.UI.Rewards.UIRewardCardPopupView cardPopup = null, WheelOfFortune.UI.Wheel.UIWheelView uiWheel = null)
        {
            wheelSpinController = spinner;
            zoneProgressionManager = zoneMgr;
            playerInventoryManager = invMgr;
            if (cardPopup != null) rewardCardPopupView = cardPopup;
            if (uiWheel != null) wheelView = uiWheel;
        }
    }
}
