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
            zoneConfig.SetMultiplicationSettings(true);
            float m1 = zoneConfig.GetRewardMultiplierForZone(1);
            float m5 = zoneConfig.GetRewardMultiplierForZone(5);
            float m10 = zoneConfig.GetRewardMultiplierForZone(10);
            float m30 = zoneConfig.GetRewardMultiplierForZone(30);
            float m60 = zoneConfig.GetRewardMultiplierForZone(60);

            if (!(m1 <= m5 && m5 <= m10 && m10 <= m30 && m30 <= m60 && m60 > m30))
            {
                Debug.LogError($"[VerificationTest] FAILED: Multiplier curve is not increasing. Z1={m1}, Z5={m5}, Z10={m10}, Z30={m30}, Z60={m60}");
                return;
            }
            Debug.Log($"[VerificationTest] PASSED: Zone curve scaling enabled (Z1={m1:0.##}x, Z5={m5:0.##}x, Z10={m10:0.##}x, Z30={m30:0.##}x, Z60={m60:0.##}x).");

            // Test Multiplier Disabled
            zoneConfig.SetMultiplicationSettings(false);
            float mDis1 = zoneConfig.GetRewardMultiplierForZone(1);
            float mDis5 = zoneConfig.GetRewardMultiplierForZone(5);
            float mDis29 = zoneConfig.GetRewardMultiplierForZone(29);
            if (!Mathf.Approximately(mDis1, 1f) || !Mathf.Approximately(mDis5, 1f) || !Mathf.Approximately(mDis29, 1f))
            {
                Debug.LogError($"[VerificationTest] FAILED: Disabled multiplier did not return 1x. Z1={mDis1}, Z5={mDis5}, Z29={mDis29}");
                return;
            }
            Debug.Log("[VerificationTest] PASSED: Disabled multiplier returns 1x across all zones.");

            // Restore enabled
            zoneConfig.SetMultiplicationSettings(true);
            // 6. Test Zone-Based Reward Scaling (Zone 29 vs Zone 1)
            var model = bronze.CreateRuntimeModelForZone(1);

            if (model.SliceCount != WheelConfigSO.SliceCount)
            {
                Debug.LogError("[VerificationTest] FAILED: Runtime wheel model slice count mismatch!");
                return;
            }

            float multZ1 = zoneConfig.GetRewardMultiplierForZone(1);
            float multZ29 = zoneConfig.GetRewardMultiplierForZone(29);
            int totalAmountZ1 = 0;
            int totalAmountZ29 = 0;
            for (int i = 0; i < WheelConfigSO.SliceCount; i++)
            {
                var slice = model.GetSlice(i);
                if (slice == null || slice.IsBomb) continue;
                totalAmountZ1 += slice.GetAmount(multZ1);
                totalAmountZ29 += slice.GetAmount(multZ29);
            }

            if (totalAmountZ29 <= totalAmountZ1)
            {
                Debug.LogError($"[VerificationTest] FAILED: Zone 29 rewards ({totalAmountZ29}) did not scale higher than Zone 1 ({totalAmountZ1})!");
                return;
            }
            Debug.Log($"[VerificationTest] PASSED: Zone progression scaling active. Total rewards at Z1={totalAmountZ1} -> Z29={totalAmountZ29} (~{totalAmountZ29 / (float)totalAmountZ1:F1}x).");

            Debug.Log("[VerificationTest] ★ ALL TESTS PASSED SUCCESSFULLY! ★");
        }
    }
}
