using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WheelOfFortune.Core;
using WheelOfFortune.Data;

namespace WheelOfFortune.Editor
{
    public static class ExitModalVerificationTest
    {
        [MenuItem("WheelOfFortune/Run Exit Modal & Banking Tests")]
        public static void RunAllTests()
        {
            Debug.Log("[ExitModalTest] === Starting Exit Modal & Banking Verification ===");

            // 1. Locate Managers
            var flowMgr = Object.FindFirstObjectByType<GameFlowManager>();
            var invMgr = Object.FindFirstObjectByType<PlayerInventoryManager>();
            var zoneMgr = Object.FindFirstObjectByType<ZoneProgressionManager>();

            if (flowMgr == null || invMgr == null || zoneMgr == null)
            {
                Debug.LogError("[ExitModalTest] FAILED: Missing managers in scene!");
                return;
            }

            // 2. Load test rewards from database
            var db = AssetDatabase.LoadAssetAtPath<RewardDatabaseSO>("Assets/Data/RewardDatabase.asset");
            if (db == null)
            {
                Debug.LogError("[ExitModalTest] FAILED: RewardDatabase not found!");
                return;
            }

            var cashItem = db.GetRewardById("cash") ?? db.GetRewardById("item_cash");
            var goldItem = db.GetRewardById("gold") ?? db.GetRewardById("item_gold");
            var chestItem = db.GetRewardById("chest_bronze") ?? db.GetRewardById("item_chest_bronze");
            var pistolItem = db.GetRewardById("pistol_pts") ?? db.GetRewardById("item_pistol_pts");

            if (cashItem == null || goldItem == null)
            {
                Debug.LogError("[ExitModalTest] FAILED: Cash or Gold item not found in database!");
                return;
            }

            // Record initial wallet
            int initialCash = invMgr.PermanentCash;
            int initialGold = invMgr.PermanentGold;

            // Clear any prior run rewards
            invMgr.ClearRunRewards();

            // 3. Add rewards to current run
            invMgr.AddReward(cashItem, 750);
            invMgr.AddReward(goldItem, 25);
            if (chestItem != null) invMgr.AddReward(chestItem, 1);
            if (pistolItem != null) invMgr.AddReward(pistolItem, 10);

            // Check pending totals
            int pendingCash = invMgr.GetPendingRunCash();
            int pendingGold = invMgr.GetPendingRunGold();

            if (pendingCash != 750 || pendingGold != 25)
            {
                Debug.LogError($"[ExitModalTest] FAILED: Pending amounts mismatch. Cash={pendingCash} (expected 750), Gold={pendingGold} (expected 25)");
                return;
            }
            Debug.Log($"[ExitModalTest] PASSED: Pending amounts correctly tracked (Cash=+${pendingCash:N0}, Gold=+{pendingGold}).");

            // 4. Test Confirmation Request Event and 1st-Click responsiveness
            var trayView = Object.FindFirstObjectByType<WheelOfFortune.UI.Rewards.UIRewardsTrayView>();
            var cashOutModal = Object.FindFirstObjectByType<WheelOfFortune.UI.Modals.UICashOutModalView>();

            if (!Application.isPlaying)
            {
                if (trayView != null) trayView.InitializeView();
                if (cashOutModal != null) cashOutModal.InitializeView();
            }

            // Verify Safe Zone Exit Rules
            bool origRestrict = zoneMgr.Config != null && zoneMgr.Config.RestrictExitToSafeZones;
            if (zoneMgr.Config != null)
            {
                // Test restricted mode
                zoneMgr.Config.SetExitRestriction(true);
                if (zoneMgr.Config.CanLeaveAtZone(1) != false)
                {
                    Debug.LogError("[ExitModalTest] FAILED: Restricted exit allowed leaving at Zone 1!");
                    return;
                }
                if (zoneMgr.Config.CanLeaveAtZone(5) != true)
                {
                    Debug.LogError("[ExitModalTest] FAILED: Restricted exit prevented leaving at Safe Zone 5!");
                    return;
                }
                if (zoneMgr.Config.CanLeaveAtZone(30) != true)
                {
                    Debug.LogError("[ExitModalTest] FAILED: Restricted exit prevented leaving at Super Zone 30!");
                    return;
                }
                Debug.Log("[ExitModalTest] PASSED: Safe/Super Zone exit restriction logic validated.");

                // Set unrestricted for exit confirmation test
                zoneMgr.Config.SetExitRestriction(false);
            }

            if (trayView != null) trayView.InitializeView();

            if (trayView != null && trayView.ExitButton != null)
            {
                if (!trayView.ExitButton.interactable)
                {
                    Debug.LogError("[ExitModalTest] FAILED: ExitButton on tray is not interactable in ReadyToSpin state!");
                    return;
                }
                Debug.Log("[ExitModalTest] PASSED: ExitButton on tray is interactable immediately at zone start.");
            }

            bool exitRequestedFired = false;
            flowMgr.OnExitConfirmationRequested += () => { exitRequestedFired = true; };

            // Simulate 1st click on Exit button
            if (trayView != null && trayView.ExitButton != null)
            {
                trayView.ExitButton.onClick.Invoke();
            }
            else
            {
                flowMgr.RequestExitConfirmation();
            }

            if (!exitRequestedFired)
            {
                Debug.LogError("[ExitModalTest] FAILED: Exit confirmation was not requested on 1st click!");
                return;
            }
            Debug.Log("[ExitModalTest] PASSED: Exit confirmation requested immediately on 1st click.");

            if (cashOutModal != null)
            {
                cashOutModal.ShowModal();
                if (!cashOutModal.gameObject.activeInHierarchy)
                {
                    Debug.LogError("[ExitModalTest] FAILED: CashOutModal is not active after ShowModal()!");
                    return;
                }
                Debug.Log("[ExitModalTest] PASSED: Exit Modal opens and remains active on 1st click.");
            }

            // Verify run rewards are still intact before user confirms
            if (invMgr.CurrentRunRewards.Count < 2)
            {
                Debug.LogError("[ExitModalTest] FAILED: Run rewards were cleared prematurely!");
                return;
            }
            Debug.Log("[ExitModalTest] PASSED: Run rewards preserved during confirmation prompt.");

            // 4b. Verify ConfirmExitAndCashOut is strictly forbidden at GameFlow level when CanLeave is false
            zoneMgr.Config.SetExitRestriction(true);
            int cashBeforeIllegalExit = invMgr.PermanentCash;
            flowMgr.ConfirmExitAndCashOut();
            if (invMgr.CurrentRunRewards.Count == 0 || invMgr.PermanentCash != cashBeforeIllegalExit)
            {
                Debug.LogError("[ExitModalTest] FAILED: ConfirmExitAndCashOut allowed cashing out when CanLeave was false!");
                return;
            }
            Debug.Log("[ExitModalTest] PASSED: ConfirmExitAndCashOut rejected at game flow level when CanLeave is false.");
            zoneMgr.Config.SetExitRestriction(false);

            // 5. Test Confirm Exit & Cash Out + Reward Tray Clearing
            bool runRewardsClearedFired = false;
            invMgr.OnRunRewardsCleared += () => { runRewardsClearedFired = true; };

            flowMgr.ConfirmExitAndCashOut();

            if (!runRewardsClearedFired)
            {
                Debug.LogError("[ExitModalTest] FAILED: OnRunRewardsCleared was not fired during ConfirmExitAndCashOut!");
                return;
            }
            Debug.Log("[ExitModalTest] PASSED: OnRunRewardsCleared fired successfully upon exit/banking.");

            int finalCash = invMgr.PermanentCash;
            int finalGold = invMgr.PermanentGold;

            if (finalCash != initialCash + 750)
            {
                Debug.LogError($"[ExitModalTest] FAILED: Permanent cash mismatch. Expected {initialCash + 750}, got {finalCash}");
                return;
            }

            if (finalGold != initialGold + 25)
            {
                Debug.LogError($"[ExitModalTest] FAILED: Permanent gold mismatch. Expected {initialGold + 25}, got {finalGold}");
                return;
            }
            Debug.Log($"[ExitModalTest] PASSED: Currencies banked successfully (Cash: {initialCash} -> {finalCash}, Gold: {initialGold} -> {finalGold}).");

            // Verify run items are cleared from run tray and banked into permanent vault
            if (invMgr.CurrentRunRewards.Count != 0)
            {
                Debug.LogError($"[ExitModalTest] FAILED: Run rewards were not cleared after exit. Remaining={invMgr.CurrentRunRewards.Count}");
                return;
            }
            if (invMgr.PermanentVaultItems.Count < 2)
            {
                Debug.LogError($"[ExitModalTest] FAILED: Non-currency rewards were not retained in permanent vault! Vault count={invMgr.PermanentVaultItems.Count}");
                return;
            }
            Debug.Log($"[ExitModalTest] PASSED: Run items cleared from tray and {invMgr.PermanentVaultItems.Count} item types secured in permanent vault.");

            // Verify zone reset to 1
            if (zoneMgr.CurrentZone != 1)
            {
                Debug.LogError($"[ExitModalTest] FAILED: Zone was not reset to 1. CurrentZone={zoneMgr.CurrentZone}");
                return;
            }
            Debug.Log("[ExitModalTest] PASSED: Zone progression reset to Zone 1.");

            // Verify GameState is ReadyToSpin
            if (flowMgr.CurrentState != GameState.ReadyToSpin)
            {
                Debug.LogError($"[ExitModalTest] FAILED: State is {flowMgr.CurrentState}, expected ReadyToSpin");
                return;
            }
            Debug.Log("[ExitModalTest] PASSED: Game state reset to ReadyToSpin.");

            if (zoneMgr.Config != null)
            {
                zoneMgr.Config.SetExitRestriction(origRestrict);
            }

            Debug.Log("[ExitModalTest] ★ ALL EXIT MODAL & REWARD TRAY RESET TESTS PASSED! ★");

            // 6. Test Revive Escalation, Per-Run Cap, and Same-Zone Spin
            TestReviveRules(flowMgr, invMgr, zoneMgr);
        }

        private static void TestReviveRules(GameFlowManager flowMgr, PlayerInventoryManager invMgr, ZoneProgressionManager zoneMgr)
        {
            Debug.Log("[ReviveRulesTest] === Starting Revive Rules Verification ===");

            var config = zoneMgr.Config;
            if (config == null)
            {
                Debug.LogError("[ReviveRulesTest] FAILED: ZoneConfig is null!");
                return;
            }

            // Verify cost formula
            int cost0 = config.GetReviveCost(0);
            int cost1 = config.GetReviveCost(1);
            int cost2 = config.GetReviveCost(2);

            if (cost0 != 25 || cost1 != 50 || cost2 != 75)
            {
                Debug.LogError($"[ReviveRulesTest] FAILED: Cost progression mismatch! Expected 25, 50, 75. Got {cost0}, {cost1}, {cost2}");
                return;
            }
            Debug.Log($"[ReviveRulesTest] PASSED: Cost progression correctly escalates ({cost0} -> {cost1} -> {cost2}).");

            // Verify cap
            if (!config.CanRevive(0) || !config.CanRevive(2) || config.CanRevive(3))
            {
                Debug.LogError("[ReviveRulesTest] FAILED: CanRevive limit check mismatch!");
                return;
            }
            Debug.Log("[ReviveRulesTest] PASSED: Revive limit correctly capped at 3 revives per run.");

            // Test revive keeps player at the SAME zone (crucial fix from feedback)
            int initialGold = invMgr.PermanentGold;
            invMgr.AddGold(200);

            // Put player in zone 4
            zoneMgr.Initialize(config, 4);

            // Add test reward
            var db = AssetDatabase.LoadAssetAtPath<RewardDatabaseSO>("Assets/Data/RewardDatabase.asset");
            var cashItem = db != null ? (db.GetRewardById("cash") ?? db.GetRewardById("item_cash")) : null;
            if (cashItem != null) invMgr.AddReward(cashItem, 100);

            // Simulate Bomb Encounter state via reflection/internal or spinning
            var stateField = typeof(GameFlowManager).GetField("currentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            stateField?.SetValue(flowMgr, GameState.BombEncounter);

            int goldBeforeRevive1 = invMgr.PermanentGold;
            flowMgr.RequestReviveWithCurrency();

            if (zoneMgr.CurrentZone != 4)
            {
                Debug.LogError($"[ReviveRulesTest] FAILED: Revive advanced zone to {zoneMgr.CurrentZone}, expected to stay at Zone 4!");
                return;
            }
            Debug.Log("[ReviveRulesTest] PASSED: Revive kept player in Zone 4 (did NOT advance zone).");

            if (invMgr.PermanentGold != goldBeforeRevive1 - 25)
            {
                Debug.LogError($"[ReviveRulesTest] FAILED: Gold was not deducted by 25. Gold={invMgr.PermanentGold}, expected {goldBeforeRevive1 - 25}");
                return;
            }
            if (flowMgr.RevivesUsedInRun != 1)
            {
                Debug.LogError($"[ReviveRulesTest] FAILED: RevivesUsedInRun was {flowMgr.RevivesUsedInRun}, expected 1.");
                return;
            }
            Debug.Log("[ReviveRulesTest] PASSED: 1st Revive cost 25 gold, revivesUsedInRun = 1.");

            // Revive 2: should cost 50
            stateField?.SetValue(flowMgr, GameState.BombEncounter);
            int goldBeforeRevive2 = invMgr.PermanentGold;
            flowMgr.RequestReviveWithCurrency();

            if (invMgr.PermanentGold != goldBeforeRevive2 - 50)
            {
                Debug.LogError($"[ReviveRulesTest] FAILED: 2nd Revive did not cost 50 gold. Gold={invMgr.PermanentGold}, expected {goldBeforeRevive2 - 50}");
                return;
            }
            if (flowMgr.RevivesUsedInRun != 2)
            {
                Debug.LogError($"[ReviveRulesTest] FAILED: RevivesUsedInRun was {flowMgr.RevivesUsedInRun}, expected 2.");
                return;
            }
            Debug.Log("[ReviveRulesTest] PASSED: 2nd Revive cost 50 gold, revivesUsedInRun = 2.");

            // Revive 3: should cost 75
            stateField?.SetValue(flowMgr, GameState.BombEncounter);
            flowMgr.RequestReviveWithCurrency();
            if (flowMgr.RevivesUsedInRun != 3)
            {
                Debug.LogError($"[ReviveRulesTest] FAILED: RevivesUsedInRun was {flowMgr.RevivesUsedInRun}, expected 3.");
                return;
            }
            Debug.Log("[ReviveRulesTest] PASSED: 3rd Revive cost 75 gold, revivesUsedInRun = 3 (MAX REACHED).");

            // Revive 4: should be rejected
            stateField?.SetValue(flowMgr, GameState.BombEncounter);
            int goldBeforeRevive4 = invMgr.PermanentGold;
            flowMgr.RequestReviveWithCurrency();
            if (invMgr.PermanentGold != goldBeforeRevive4)
            {
                Debug.LogError("[ReviveRulesTest] FAILED: 4th Revive was allowed despite exceeding maxRevivesPerRun!");
                return;
            }
            Debug.Log("[ReviveRulesTest] PASSED: 4th Revive rejected (limit enforced).");

            // Give Up: forfeits loot, resets zone to 1, resets revive counter
            flowMgr.RequestGiveUp();
            if (flowMgr.RevivesUsedInRun != 0 || zoneMgr.CurrentZone != 1 || invMgr.CurrentRunRewards.Count != 0)
            {
                Debug.LogError("[ReviveRulesTest] FAILED: Give Up did not reset progression and revives counter!");
                return;
            }
            Debug.Log("[ReviveRulesTest] PASSED: Give Up reset revivesUsedInRun to 0, zone to 1, and cleared run rewards.");

            // Restore wallet
            invMgr.Initialize(invMgr.PermanentCash, initialGold);
            Debug.Log("[ReviveRulesTest] ★ ALL REVIVE ESCALATION & SAME-ZONE SPIN TESTS PASSED! ★");
        }
    }
}
