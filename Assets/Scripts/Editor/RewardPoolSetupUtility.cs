using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WheelOfFortune.Data;

namespace WheelOfFortune.Editor
{
    public static class RewardPoolSetupUtility
    {
        [MenuItem("Wheel Of Fortune/Setup Reward Pools and Database")]
        public static void ExecuteSetup()
        {
            // 1. Find all RewardItemSO assets in Assets/Data/Rewards
            string[] guids = AssetDatabase.FindAssets("t:RewardItemSO", new[] { "Assets/Data/Rewards" });
            var allRewardItems = new List<RewardItemSO>();
            var itemMap = new Dictionary<string, RewardItemSO>(System.StringComparer.OrdinalIgnoreCase);

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<RewardItemSO>(path);
                if (item != null)
                {
                    allRewardItems.Add(item);
                    if (!string.IsNullOrEmpty(item.Id))
                    {
                        itemMap[item.Id] = item;
                        itemMap[item.Id.Replace("item_", "")] = item;
                    }
                    itemMap[item.name] = item;
                    itemMap[item.name.Replace("item_", "")] = item;
                }
            }

            Debug.Log($"[RewardPoolSetupUtility] Loaded {allRewardItems.Count} unique RewardItemSO assets.");

            // 2. Create or load RewardDatabaseSO
            string dbPath = "Assets/Data/RewardDatabase.asset";
            var database = AssetDatabase.LoadAssetAtPath<RewardDatabaseSO>(dbPath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<RewardDatabaseSO>();
                AssetDatabase.CreateAsset(database, dbPath);
                Debug.Log($"[RewardPoolSetupUtility] Created RewardDatabase asset at {dbPath}.");
            }

            database.SetAllRewards(allRewardItems);
            EditorUtility.SetDirty(database);

            RewardItemSO bombItem = allRewardItems.Find(r => r.IsBomb);

            // 3. Populate Bronze Wheel Config (contains bomb + common rewards)
            string bronzePath = "Assets/Data/Wheels/BronzeWheelConfig.asset";
            var bronzeConfig = AssetDatabase.LoadAssetAtPath<WheelConfigSO>(bronzePath);
            if (bronzeConfig != null)
            {
                var bronzePool = new List<WheelRewardPoolEntry>();
                AddEntryIfPresent(bronzePool, itemMap, "cash", 100, 20);
                AddEntryIfPresent(bronzePool, itemMap, "gold", 3, 12);
                AddEntryIfPresent(bronzePool, itemMap, "pistol_pts", 5, 15);
                AddEntryIfPresent(bronzePool, itemMap, "smg_pts", 5, 15);
                AddEntryIfPresent(bronzePool, itemMap, "shotgun_pts", 5, 12);
                AddEntryIfPresent(bronzePool, itemMap, "chest_bronze", 1, 8);
                AddEntryIfPresent(bronzePool, itemMap, "grenade", 2, 10);
                AddEntryIfPresent(bronzePool, itemMap, "molotov", 2, 10);
                AddEntryIfPresent(bronzePool, itemMap, "healthshot", 2, 10);
                AddEntryIfPresent(bronzePool, itemMap, "armor_pts", 5, 12);

                bronzeConfig.SetRewardPool(bronzePool, bombItem);
                bronzeConfig.RollSlicesForZone(1);
                EditorUtility.SetDirty(bronzeConfig);
                Debug.Log($"[RewardPoolSetupUtility] Bronze Wheel pool populated with {bronzePool.Count} entries.");
            }

            // 4. Populate Silver Wheel Config (Safe Zone - higher base amounts, no bombs)
            string silverPath = "Assets/Data/Wheels/SilverWheelConfig.asset";
            var silverConfig = AssetDatabase.LoadAssetAtPath<WheelConfigSO>(silverPath);
            if (silverConfig != null)
            {
                var silverPool = new List<WheelRewardPoolEntry>();
                AddEntryIfPresent(silverPool, itemMap, "cash", 500, 20);
                AddEntryIfPresent(silverPool, itemMap, "gold", 15, 14);
                AddEntryIfPresent(silverPool, itemMap, "rifle_pts", 10, 15);
                AddEntryIfPresent(silverPool, itemMap, "sniper_pts", 10, 15);
                AddEntryIfPresent(silverPool, itemMap, "chest_silver", 1, 10);
                AddEntryIfPresent(silverPool, itemMap, "grenade", 5, 12);
                AddEntryIfPresent(silverPool, itemMap, "molotov", 5, 12);
                AddEntryIfPresent(silverPool, itemMap, "healthshot", 5, 12);
                AddEntryIfPresent(silverPool, itemMap, "easter_cap", 1, 6);
                AddEntryIfPresent(silverPool, itemMap, "armor_pts", 15, 14);

                silverConfig.SetRewardPool(silverPool, null);
                silverConfig.RollSlicesForZone(5);
                EditorUtility.SetDirty(silverConfig);
                Debug.Log($"[RewardPoolSetupUtility] Silver Wheel pool populated with {silverPool.Count} entries.");
            }

            // 5. Populate Golden Wheel Config (Super Zone - huge rewards & high-tier weapons, no bombs)
            string goldPath = "Assets/Data/Wheels/GoldenWheelConfig.asset";
            var goldConfig = AssetDatabase.LoadAssetAtPath<WheelConfigSO>(goldPath);
            if (goldConfig != null)
            {
                var goldPool = new List<WheelRewardPoolEntry>();
                AddEntryIfPresent(goldPool, itemMap, "cash", 2500, 20);
                AddEntryIfPresent(goldPool, itemMap, "gold", 50, 15);
                AddEntryIfPresent(goldPool, itemMap, "chest_gold", 1, 12);
                AddEntryIfPresent(goldPool, itemMap, "chest_super", 1, 8);
                AddEntryIfPresent(goldPool, itemMap, "rifle_tier2", 1, 10);
                AddEntryIfPresent(goldPool, itemMap, "sniper_tier3", 1, 8);
                AddEntryIfPresent(goldPool, itemMap, "pumpkin_helmet", 1, 6);
                AddEntryIfPresent(goldPool, itemMap, "healthshot", 10, 12);
                AddEntryIfPresent(goldPool, itemMap, "grenade", 10, 12);
                AddEntryIfPresent(goldPool, itemMap, "molotov", 10, 12);

                goldConfig.SetRewardPool(goldPool, null);
                goldConfig.RollSlicesForZone(30);
                EditorUtility.SetDirty(goldConfig);
                Debug.Log($"[RewardPoolSetupUtility] Golden Wheel pool populated with {goldPool.Count} entries.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[RewardPoolSetupUtility] ★ Successfully saved RewardDatabase and all Wheel configs!");
        }

        private static void AddEntryIfPresent(List<WheelRewardPoolEntry> pool, Dictionary<string, RewardItemSO> map, string key, int amount, int weight)
        {
            if (map.TryGetValue(key, out var item) && item != null)
            {
                pool.Add(new WheelRewardPoolEntry(item, amount, weight));
            }
            else
            {
                Debug.LogWarning($"[RewardPoolSetupUtility] Item key '{key}' not found in map.");
            }
        }
    }
}
