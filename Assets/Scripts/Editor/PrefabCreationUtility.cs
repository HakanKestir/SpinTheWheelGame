using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WheelOfFortune.Core;
using WheelOfFortune.UI.Header;
using WheelOfFortune.UI.Modals;
using WheelOfFortune.UI.Rewards;
using WheelOfFortune.UI.Wheel;
using WheelOfFortune.UI.Zone;

namespace WheelOfFortune.Editor
{
    public static class PrefabCreationUtility
    {
        private const string PrefabsRoot = "Assets/Prefabs";

        [InitializeOnLoadMethod]
        private static void RegisterAutoRun()
        {
            EditorApplication.update += CheckAndRun;
        }

        private static void CheckAndRun()
        {
            if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode) return;
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "SampleScene") return;

            if (!File.Exists($"{PrefabsRoot}/Modals/BombModal_View.prefab"))
            {
                EditorApplication.update -= CheckAndRun;
                Debug.Log("[PrefabCreationUtility] Generating reusable modular prefabs...");
                GenerateAllPrefabs(showDialog: false);
            }
        }

        [MenuItem("Wheel Of Fortune/★ Generate Reusable Prefabs", priority = 1)]
        public static void MenuGeneratePrefabs()
        {
            GenerateAllPrefabs(showDialog: true);
        }

        public static void GenerateAllPrefabs(bool showDialog)
        {
            // Ensure directories exist
            EnsureDirectory($"{PrefabsRoot}/Core");
            EnsureDirectory($"{PrefabsRoot}/Wheel");
            EnsureDirectory($"{PrefabsRoot}/Rewards");
            EnsureDirectory($"{PrefabsRoot}/Zone");
            EnsureDirectory($"{PrefabsRoot}/Header");
            EnsureDirectory($"{PrefabsRoot}/Modals");

            var scene = EditorSceneManager.GetActiveScene();

            // 1. Sub-item / Template Prefabs first
            // A) Reward Tray Slot Item
            GameObject slotTemplateGO = GameObject.Find("ui_slot_item_template");
            GameObject slotPrefab = null;
            if (slotTemplateGO != null)
            {
                slotPrefab = SavePrefab(slotTemplateGO, $"{PrefabsRoot}/Rewards/RewardTray_SlotItem.prefab", connectScene: true);
            }

            // B) Flying Token Item
            GameObject tokenTemplateGO = GameObject.Find("ui_token_fly_template");
            GameObject tokenPrefab = null;
            if (tokenTemplateGO != null)
            {
                tokenPrefab = SavePrefab(tokenTemplateGO, $"{PrefabsRoot}/Rewards/FlyToken_Item.prefab", connectScene: true);
            }

            // C) Wheel Slice Item
            GameObject sliceTemplateGO = GameObject.Find("ui_slice_item_0");
            GameObject slicePrefab = null;
            if (sliceTemplateGO != null)
            {
                slicePrefab = SavePrefab(sliceTemplateGO, $"{PrefabsRoot}/Wheel/WheelSlice_Item.prefab", connectScene: true);
            }

            // D) Zone Number Item
            GameObject zoneItemGO = GameObject.Find("ui_zone_number_item_6") ?? GameObject.Find("ui_zone_number_item_0");
            GameObject zoneNumberPrefab = null;
            if (zoneItemGO != null)
            {
                zoneNumberPrefab = SavePrefab(zoneItemGO, $"{PrefabsRoot}/Zone/ZoneNumber_Item.prefab", connectScene: true);
            }

            // 2. Complex View Prefabs
            // A) Reward Tray View
            GameObject trayGO = GameObject.Find("ui_panel_rewards_tray");
            if (trayGO != null)
            {
                var trayView = trayGO.GetComponent<UIRewardsTrayView>();
                if (trayView != null && slotPrefab != null)
                {
                    trayView.SetSlotPrefab(slotPrefab.GetComponent<UIRewardTraySlotItemView>());
                    EditorUtility.SetDirty(trayView);
                }
                SavePrefab(trayGO, $"{PrefabsRoot}/Rewards/RewardTray_View.prefab", connectScene: true);
            }

            // B) Winner Reward Card Celebration Popup
            GameObject cardPopupGO = GameObject.Find("ui_panel_reward_card_popup");
            if (cardPopupGO != null)
            {
                var cardPopupView = cardPopupGO.GetComponent<UIRewardCardPopupView>();
                if (cardPopupView != null && tokenPrefab != null)
                {
                    cardPopupView.SetTokenPrefab(tokenPrefab);
                    EditorUtility.SetDirty(cardPopupView);
                }
                SavePrefab(cardPopupGO, $"{PrefabsRoot}/Rewards/RewardCard_Popup.prefab", connectScene: true);
            }

            // C) Wheel Card View
            GameObject wheelCardGO = GameObject.Find("ui_panel_wheel_center");
            if (wheelCardGO != null)
            {
                var wheelView = wheelCardGO.GetComponent<UIWheelView>();
                if (wheelView != null && slicePrefab != null)
                {
                    wheelView.SetSliceItemPrefab(slicePrefab.GetComponent<UIWheelSliceItemView>());
                    EditorUtility.SetDirty(wheelView);
                }
                SavePrefab(wheelCardGO, $"{PrefabsRoot}/Wheel/WheelCard_View.prefab", connectScene: true);
            }

            // D) Zone Tracker Bar
            GameObject zoneTrackerGO = GameObject.Find("ui_panel_zone_tracker");
            if (zoneTrackerGO != null)
            {
                var trackerBar = zoneTrackerGO.GetComponent<UIZoneTrackerView>();
                if (trackerBar != null && zoneNumberPrefab != null)
                {
                    trackerBar.SetZoneItemPrefab(zoneNumberPrefab.GetComponent<UIZoneNumberItemView>());
                    EditorUtility.SetDirty(trackerBar);
                }
                SavePrefab(zoneTrackerGO, $"{PrefabsRoot}/Zone/ZoneTracker_Bar.prefab", connectScene: true);
            }

            // E) Zone Badges Panel (Super Zone & Safe Zone right badges)
            GameObject zoneBadgesGO = GameObject.Find("ui_panel_zone_badges");
            if (zoneBadgesGO != null)
            {
                SavePrefab(zoneBadgesGO, $"{PrefabsRoot}/Zone/ZoneBadges_Panel.prefab", connectScene: true);
            }

            // F) Currency Header Panel
            GameObject currHeaderGO = GameObject.Find("ui_panel_currency_header");
            if (currHeaderGO != null)
            {
                SavePrefab(currHeaderGO, $"{PrefabsRoot}/Header/CurrencyHeader_Panel.prefab", connectScene: true);
            }

            // G) Top Header Panel (combining tracker bar, badges, currencies)
            GameObject topHeaderGO = GameObject.Find("ui_panel_top_header");
            if (topHeaderGO != null)
            {
                SavePrefab(topHeaderGO, $"{PrefabsRoot}/Header/TopHeader_Panel.prefab", connectScene: true);
            }

            // H) Bomb Modal
            GameObject bombModalGO = FindObjectIncludingInactive("ui_panel_bomb_root");
            if (bombModalGO != null)
            {
                SavePrefab(bombModalGO, $"{PrefabsRoot}/Modals/BombModal_View.prefab", connectScene: true);
            }

            // I) Cash Out Modal
            GameObject cashOutModalGO = FindObjectIncludingInactive("ui_panel_cashout_root");
            if (cashOutModalGO != null)
            {
                SavePrefab(cashOutModalGO, $"{PrefabsRoot}/Modals/CashOutModal_View.prefab", connectScene: true);
            }

            // J) Game Managers Core
            GameObject managersGO = FindObjectIncludingInactive("[GameManagers]");
            if (managersGO != null)
            {
                SavePrefab(managersGO, $"{PrefabsRoot}/Core/GameManagers.prefab", connectScene: true);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[PrefabCreationUtility] ★ All modular prefabs successfully created and connected to scene!");

            if (showDialog)
            {
                EditorUtility.DisplayDialog("Prefabs Generated", "All modular prefabs have been successfully created under Assets/Prefabs and connected to the scene!", "OK");
            }
        }

        private static GameObject SavePrefab(GameObject sourceGO, string path, bool connectScene)
        {
            if (sourceGO == null) return null;
            GameObject savedPrefab;
            if (connectScene)
            {
                savedPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(sourceGO, path, InteractionMode.AutomatedAction);
            }
            else
            {
                savedPrefab = PrefabUtility.SaveAsPrefabAsset(sourceGO, path);
            }
            Debug.Log($"[PrefabCreationUtility] Saved prefab: {path}");
            return savedPrefab;
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static GameObject FindObjectIncludingInactive(string name)
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            var all = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (var t in all)
            {
                if (t.name == name && t.gameObject.scene == activeScene)
                {
                    return t.gameObject;
                }
            }
            return GameObject.Find(name);
        }
    }
}
