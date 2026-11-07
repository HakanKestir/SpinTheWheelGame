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
        private int revivesUsedInRun = 0;

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
        public int RevivesUsedInRun => revivesUsedInRun;
        public int MaxRevivesPerRun => zoneProgressionManager != null && zoneProgressionManager.Config != null
            ? zoneProgressionManager.Config.MaxRevivesPerRun
            : 3;
        public int CurrentReviveCost => zoneProgressionManager != null && zoneProgressionManager.Config != null
            ? zoneProgressionManager.Config.GetReviveCost(revivesUsedInRun)
            : 25;
        public bool CanReviveInRun => zoneProgressionManager != null && zoneProgressionManager.Config != null
            && zoneProgressionManager.Config.CanRevive(revivesUsedInRun);

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

            var wheelModel = zoneProgressionManager.ActiveWheelModel;
            int multiplier = zoneProgressionManager.CurrentMultiplier;

            wheelSpinController.Initialize(wheelModel, multiplier);

            if (wheelView == null)
            {
                wheelView = FindFirstObjectByType<WheelOfFortune.UI.Wheel.UIWheelView>();
            }

            if (wheelView != null && wheelModel != null)
            {
                wheelView.UpdateWheelDisplay(wheelModel, multiplier);
            }
        }

        public void RequestSpin()
        {
            if (currentState != GameState.ReadyToSpin) return;
            if (wheelSpinController == null || wheelSpinController.IsSpinning) return;

            var wheelModel = zoneProgressionManager.ActiveWheelModel;
            if (wheelModel == null)
            {
                Debug.LogError("[GameFlowManager] No active wheel model for current zone!");
                return;
            }

            int targetIndex;
            if (forceSliceIndex >= 0 && forceSliceIndex < wheelModel.SliceCount)
            {
                targetIndex = forceSliceIndex;
            }
            else
            {
                targetIndex = wheelModel.SelectRandomSliceIndex();
            }

            SetState(GameState.Spinning);
            wheelSpinController.SpinToSlice(targetIndex, OnWheelSpinFinished);
        }

        [SerializeField] private WheelOfFortune.UI.Rewards.UIRewardCardPopupView rewardCardPopupView;

        private void OnWheelSpinFinished(int winningIndex)
        {
            var wheelModel = zoneProgressionManager.ActiveWheelModel;
            var winningSlice = wheelModel != null ? wheelModel.GetSlice(winningIndex) : null;

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

            if (!CanReviveInRun)
            {
                Debug.LogWarning("[GameFlowManager] Maximum revives reached for this run!");
                return;
            }

            int cost = CurrentReviveCost;
            if (playerInventoryManager.TrySpendGold(cost))
            {
                revivesUsedInRun++;
                ExecuteRevive();
            }
            else
            {
                Debug.LogWarning("[GameFlowManager] Not enough gold to revive!");
            }
        }

        public void RequestReviveBonus()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (currentState != GameState.BombEncounter) return;
            // Bonus revive for demo evaluation in dev / editor builds
            ExecuteRevive();
#else
            Debug.LogWarning("[GameFlowManager] Bonus revive is disabled in release builds.");
#endif
        }

        private void ExecuteRevive()
        {
            // Revive: preserve all collected run rewards and allow re-spinning in the CURRENT zone (never advance)
            if (wheelSpinController != null)
            {
                wheelSpinController.StopImmediate();
            }
            if (zoneProgressionManager != null)
            {
                zoneProgressionManager.RefreshCurrentZoneWheel();
            }
            SetupCurrentZoneWheel();
            SetState(GameState.ReadyToSpin);
            OnRevived?.Invoke();
        }

        public void RequestGiveUp()
        {
            if (currentState != GameState.BombEncounter) return;

            // Player forfeits all rewards and resets progression
            revivesUsedInRun = 0;
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

            // Game flow level validation: strictly forbid exit if outside safe/super zones
            if (zoneProgressionManager != null && !zoneProgressionManager.CanLeave)
            {
                Debug.LogWarning("[GameFlowManager] Cannot cash out outside of safe or super zones!");
                return;
            }

            revivesUsedInRun = 0;

            // 1. Bank cash and gold into permanent wallet (and items into vault)
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
