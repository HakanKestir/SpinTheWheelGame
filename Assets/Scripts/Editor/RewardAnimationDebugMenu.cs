using UnityEditor;
using UnityEngine;
using WheelOfFortune.Core;
using WheelOfFortune.Data;

namespace WheelOfFortune.EditorTools
{
    /// <summary>
    /// Editor-only helpers for manually exercising reward animations in Play Mode.
    /// These act on the running scene state only and never edit project assets.
    /// </summary>
    public static class RewardAnimationDebugMenu
    {
        private const string MENU_ROOT = "WheelOfFortune/Debug/";
        private const string REWARD_FOLDER = "Assets/Data/Rewards";

        [MenuItem(MENU_ROOT + "Grant 1 Coin To Run", true)]
        [MenuItem(MENU_ROOT + "Grant 10 Coins To Run", true)]
        [MenuItem(MENU_ROOT + "Grant 1000 Coins To Run", true)]
        [MenuItem(MENU_ROOT + "Grant 1,000,000 Coins To Run", true)]
        [MenuItem(MENU_ROOT + "Grant 25 Gold To Run", true)]
        private static bool ValidateGrantToRun()
        {
            return Application.isPlaying;
        }

        [MenuItem(MENU_ROOT + "Grant 1 Coin To Run", priority = 100)]
        private static void GrantOneCoin()
        {
            Grant("item_cash", 1);
        }

        [MenuItem(MENU_ROOT + "Grant 10 Coins To Run", priority = 101)]
        private static void GrantTenCoins()
        {
            Grant("item_cash", 10);
        }

        [MenuItem(MENU_ROOT + "Grant 1000 Coins To Run", priority = 102)]
        private static void GrantThousandCoins()
        {
            Grant("item_cash", 1000);
        }

        [MenuItem(MENU_ROOT + "Grant 1,000,000 Coins To Run", priority = 103)]
        private static void GrantMillionCoins()
        {
            Grant("item_cash", 1000000);
        }

        [MenuItem(MENU_ROOT + "Grant 25 Gold To Run", priority = 104)]
        private static void GrantGold()
        {
            Grant("item_gold", 25);
        }

        private static void Grant(string assetName, int amount)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[RewardAnimationDebug] Enter Play Mode first.");
                return;
            }

            var inventory = UnityEngine.Object.FindFirstObjectByType<PlayerInventoryManager>();
            if (inventory == null)
            {
                Debug.LogWarning("[RewardAnimationDebug] No PlayerInventoryManager found in the open scene.");
                return;
            }

            var item = LoadItem(assetName);
            if (item == null)
            {
                Debug.LogWarning($"[RewardAnimationDebug] Reward item '{assetName}' was not found in {REWARD_FOLDER}.");
                return;
            }

            inventory.AddReward(item, amount);
            Debug.Log($"[RewardAnimationDebug] Granted {amount}x {item.DisplayName} to the current run.");
        }

        private static RewardItemSO LoadItem(string assetName)
        {
            string[] guids = AssetDatabase.FindAssets("t:RewardItemSO", new[] { REWARD_FOLDER });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == assetName)
                {
                    return AssetDatabase.LoadAssetAtPath<RewardItemSO>(path);
                }
            }

            return null;
        }
    }
}
