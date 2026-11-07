using UnityEditor;
using UnityEngine;
using WheelOfFortune.Data;

namespace WheelOfFortune.Editor
{
    public static class RewardPoolVerificationTest
    {
        [MenuItem("WheelOfFortune/Run Dynamic Wheel & Multiplier Tests")]
        public static void RunAllTests()
        {
            Debug.Log("[VerificationTest] === Starting Reward Pool & Multiplier Verification ===");

            // 1. Verify RewardDatabase
            var db = AssetDatabase.LoadAssetAtPath<RewardDatabaseSO>("Assets/Data/RewardDatabase.asset");
            if (db == null || db.AllRewards.Count != 20)
            {
                Debug.LogError($"[VerificationTest] FAILED: RewardDatabase count is {db?.AllRewards.Count ?? 0}, expected 20.");
                return;
            }
            Debug.Log($"[VerificationTest] PASSED: RewardDatabase has all {db.AllRewards.Count} game rewards.");

            // 2. Verify Wheel Configs & Pools
            var bronze = AssetDatabase.LoadAssetAtPath<WheelConfigSO>("Assets/Data/Wheels/BronzeWheelConfig.asset");
            var silver = AssetDatabase.LoadAssetAtPath<WheelConfigSO>("Assets/Data/Wheels/SilverWheelConfig.asset");
            var gold = AssetDatabase.LoadAssetAtPath<WheelConfigSO>("Assets/Data/Wheels/GoldenWheelConfig.asset");

            if (bronze.RewardPool.Count < 7 || silver.RewardPool.Count < 8 || gold.RewardPool.Count < 8)
            {
                Debug.LogError("[VerificationTest] FAILED: One or more wheels have insufficient reward pool entries.");
                return;
            }
            Debug.Log($"[VerificationTest] PASSED: Bronze pool={bronze.RewardPool.Count}, Silver pool={silver.RewardPool.Count}, Gold pool={gold.RewardPool.Count}.");

            // 3. Test Bronze Dynamic Rolling (Must have exactly 1 bomb and 7 non-bombs)
            for (int trial = 1; trial <= 5; trial++)
            {
                bronze.RollSlicesForZone(trial);
                int bombCount = 0;
                for (int i = 0; i < WheelConfigSO.SliceCount; i++)
                {
                    if (bronze.GetSlice(i).IsBomb) bombCount++;
                }
                if (bombCount != 1)
                {
                    Debug.LogError($"[VerificationTest] FAILED: Bronze trial {trial} had {bombCount} bombs, expected 1.");
                    return;
                }
            }
            Debug.Log("[VerificationTest] PASSED: Bronze wheel dynamically rolls exactly 1 bomb across multiple trials.");

            // 4. Test Silver & Gold Dynamic Rolling (Must have 0 bombs)
            for (int trial = 1; trial <= 5; trial++)
            {
                silver.RollSlicesForZone(trial * 5);
                gold.RollSlicesForZone(trial * 30);
                for (int i = 0; i < WheelConfigSO.SliceCount; i++)
                {
                    if (silver.GetSlice(i).IsBomb || gold.GetSlice(i).IsBomb)
                    {
                        Debug.LogError("[VerificationTest] FAILED: Safe or Super wheel rolled a bomb!");
                        return;
                    }
                }
            }
            Debug.Log("[VerificationTest] PASSED: Silver & Gold wheels never roll bombs.");

            // 5. Test Multiplier Logic in ZoneConfigSO
            var zoneConfig = AssetDatabase.LoadAssetAtPath<ZoneConfigSO>("Assets/Data/MainZoneConfig.asset");
            zoneConfig.SetMultiplicationSettings(true, 4);
            int m1 = zoneConfig.GetRewardMultiplierForZone(1); // 1x
            int m4 = zoneConfig.GetRewardMultiplierForZone(4); // 1x
            int m5 = zoneConfig.GetRewardMultiplierForZone(5); // 2x
            int m9 = zoneConfig.GetRewardMultiplierForZone(9); // 3x
            int m29 = zoneConfig.GetRewardMultiplierForZone(29); // 8x

            if (m1 != 1 || m4 != 1 || m5 != 2 || m9 != 3 || m29 != 8)
            {
                Debug.LogError($"[VerificationTest] FAILED: Multiplier calculation incorrect. Z1={m1}, Z4={m4}, Z5={m5}, Z9={m9}, Z29={m29}");
                return;
            }
            Debug.Log($"[VerificationTest] PASSED: Multiplier scaling enabled (Z1={m1}x, Z5={m5}x, Z9={m9}x, Z29={m29}x).");

            // Test Multiplier Disabled
            zoneConfig.SetMultiplicationSettings(false, 4);
            int mDis1 = zoneConfig.GetRewardMultiplierForZone(1);
            int mDis5 = zoneConfig.GetRewardMultiplierForZone(5);
            int mDis29 = zoneConfig.GetRewardMultiplierForZone(29);
            if (mDis1 != 1 || mDis5 != 1 || mDis29 != 1)
            {
                Debug.LogError($"[VerificationTest] FAILED: Disabled multiplier did not return 1x. Z1={mDis1}, Z5={mDis5}, Z29={mDis29}");
                return;
            }
            Debug.Log("[VerificationTest] PASSED: Disabled multiplier returns 1x across all zones.");

            // Restore enabled
            zoneConfig.SetMultiplicationSettings(true, 4);
            // 6. Test Zone-Based Reward Scaling (Zone 29 vs Zone 1)
            var modelZ1 = bronze.CreateRuntimeModelForZone(1, zoneConfig.ZoneProgressionCurve);
            var modelZ29 = bronze.CreateRuntimeModelForZone(29, zoneConfig.ZoneProgressionCurve);

            if (modelZ1.SliceCount != WheelConfigSO.SliceCount || modelZ29.SliceCount != WheelConfigSO.SliceCount)
            {
                Debug.LogError("[VerificationTest] FAILED: Runtime wheel models slice count mismatch!");
                return;
            }

            int totalAmountZ1 = 0;
            int totalAmountZ29 = 0;
            for (int i = 0; i < WheelConfigSO.SliceCount; i++)
            {
                var s1 = modelZ1.GetSlice(i);
                var s29 = modelZ29.GetSlice(i);
                if (s1 != null && !s1.IsBomb) totalAmountZ1 += s1.GetAmount(1);
                if (s29 != null && !s29.IsBomb) totalAmountZ29 += s29.GetAmount(1);
            }

            if (totalAmountZ29 <= totalAmountZ1)
            {
                Debug.LogError($"[VerificationTest] FAILED: Zone 29 rewards ({totalAmountZ29}) did not scale higher than Zone 1 ({totalAmountZ1})!");
                return;
            }
            Debug.Log($"[VerificationTest] PASSED: Zone progression scaling active. Total base rewards at Z1={totalAmountZ1} -> Z29={totalAmountZ29} (~{totalAmountZ29 / (float)totalAmountZ1:F1}x).");

            Debug.Log("[VerificationTest] ★ ALL TESTS PASSED SUCCESSFULLY! ★");
        }
    }
}
