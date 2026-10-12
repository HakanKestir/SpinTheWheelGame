using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Core;
using WheelOfFortune.UI.Modals;

namespace WheelOfFortune.Editor
{
    public static class ExitModalPrefabBuilder
    {
        private const string PrefabsRoot = "Assets/Prefabs/Modals";

        [MenuItem("Wheel Of Fortune/Build Exit Modal & Prefabs")]
        public static void BuildAll()
        {
            if (!Directory.Exists(PrefabsRoot))
            {
                Directory.CreateDirectory(PrefabsRoot);
            }

            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

            Sprite cashIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/demo_content/UI_icon_cash.png");
            Sprite goldIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/demo_content/UI_icon_gold.png");
            Sprite neutralFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/demo_content/ui_card_frame_12px_neutral.png");
            Sprite gradientFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/demo_content/ui_card_frame_gardient.png");
            Sprite panelBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/demo_content/ui_card_panel_zone_bg.png");
            Sprite greyBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/demo_content/UI_button_grey_standard.png");
            Sprite orangeBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/demo_content/UI_button_orange_standard.png");

            // 1. Build ExitRewardCard_Item Prefab
            GameObject cardGO = new GameObject("ExitRewardCard_Item", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(UIExitRewardCardItemView));
            RectTransform cardRT = cardGO.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(140, 185);

            Image cardBg = cardGO.GetComponent<Image>();
            cardBg.sprite = panelBg;
            cardBg.color = new Color(0.10f, 0.13f, 0.20f, 0.95f);
            cardBg.type = Image.Type.Sliced;
            cardBg.maskable = true;
            cardBg.raycastTarget = false;

            // Frame
            GameObject frameGO = new GameObject("ui_image_card_frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            frameGO.transform.SetParent(cardGO.transform, false);
            RectTransform frameRT = frameGO.GetComponent<RectTransform>();
            frameRT.anchorMin = Vector2.zero;
            frameRT.anchorMax = Vector2.one;
            frameRT.sizeDelta = Vector2.zero;
            Image frameImg = frameGO.GetComponent<Image>();
            frameImg.sprite = neutralFrame;
            frameImg.color = new Color(0.35f, 0.50f, 0.75f, 0.85f);
            frameImg.type = Image.Type.Sliced;
            frameImg.maskable = true;
            frameImg.raycastTarget = false;

            // Amount Badge (Top)
            GameObject amountGO = new GameObject("ui_text_amount_value", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            amountGO.transform.SetParent(cardGO.transform, false);
            RectTransform amountRT = amountGO.GetComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0f, 1f);
            amountRT.anchorMax = new Vector2(1f, 1f);
            amountRT.pivot = new Vector2(0.5f, 1f);
            amountRT.anchoredPosition = new Vector2(0f, -8f);
            amountRT.sizeDelta = new Vector2(130, 26);
            TextMeshProUGUI amountTxt = amountGO.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) amountTxt.font = fontAsset;
            amountTxt.fontSize = 18;
            amountTxt.fontStyle = FontStyles.Bold;
            amountTxt.alignment = TextAlignmentOptions.Center;
            amountTxt.color = Color.white;
            amountTxt.text = "+100";
            amountTxt.maskable = true;
            amountTxt.raycastTarget = false;

            // Icon (Center)
            GameObject iconGO = new GameObject("ui_image_reward_icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGO.transform.SetParent(cardGO.transform, false);
            RectTransform iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = new Vector2(0f, 2f);
            iconRT.sizeDelta = new Vector2(74, 74);
            Image iconImg = iconGO.GetComponent<Image>();
            iconImg.sprite = cashIcon;
            iconImg.preserveAspect = true;
            iconImg.maskable = true;
            iconImg.raycastTarget = false;

            // Name Text (Bottom)
            GameObject nameGO = new GameObject("ui_text_name_value", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            nameGO.transform.SetParent(cardGO.transform, false);
            RectTransform nameRT = nameGO.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0f, 0f);
            nameRT.anchorMax = new Vector2(1f, 0f);
            nameRT.pivot = new Vector2(0.5f, 0f);
            nameRT.anchoredPosition = new Vector2(0f, 8f);
            nameRT.sizeDelta = new Vector2(130, 24);
            TextMeshProUGUI nameTxt = nameGO.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) nameTxt.font = fontAsset;
            nameTxt.fontSize = 13;
            nameTxt.fontStyle = FontStyles.Normal;
            nameTxt.alignment = TextAlignmentOptions.Center;
            nameTxt.color = new Color(0.85f, 0.88f, 0.95f, 0.9f);
            nameTxt.text = "Cash";
            nameTxt.maskable = true;
            nameTxt.raycastTarget = false;

            UIExitRewardCardItemView cardView = cardGO.GetComponent<UIExitRewardCardItemView>();
            cardView.SetReferences(cardBg, frameImg, iconImg, amountTxt, nameTxt);

            string cardPrefabPath = $"{PrefabsRoot}/ExitRewardCard_Item.prefab";
            GameObject savedCardPrefab = PrefabUtility.SaveAsPrefabAsset(cardGO, cardPrefabPath);
            Object.DestroyImmediate(cardGO);
            Debug.Log($"[ExitModalPrefabBuilder] Saved card prefab: {cardPrefabPath}");

            var scene = EditorSceneManager.GetActiveScene();
            GameObject rootGO = null;
            var allTransforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in allTransforms)
            {
                if (t.name == "ui_panel_cashout_root" && t.gameObject.scene == scene)
                {
                    rootGO = t.gameObject;
                    break;
                }
            }

            if (rootGO == null)
            {
                Debug.LogError("[ExitModalPrefabBuilder] ui_panel_cashout_root not found in scene!");
                return;
            }

            // Ensure backdrop Image & full stretch anchors (centered with 0 offset)
            RectTransform rootRT = rootGO.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.pivot = new Vector2(0.5f, 0.5f);
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;
            rootRT.anchoredPosition = Vector2.zero;
            rootRT.sizeDelta = Vector2.zero;

            Image rootImg = rootGO.GetComponent<Image>() ?? rootGO.AddComponent<Image>();
            rootImg.color = new Color(0.04f, 0.05f, 0.08f, 0.94f);
            rootImg.raycastTarget = true;

            // Find or recreate anim container
            Transform animTrans = rootGO.transform.Find("ui_transform_cashout_modal_anim_container");
            GameObject containerGO;
            if (animTrans == null)
            {
                containerGO = new GameObject("ui_transform_cashout_modal_anim_container", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                containerGO.transform.SetParent(rootGO.transform, false);
            }
            else
            {
                containerGO = animTrans.gameObject;
            }

            RectTransform containerRT = containerGO.GetComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.5f, 0.5f);
            containerRT.anchorMax = new Vector2(0.5f, 0.5f);
            containerRT.pivot = new Vector2(0.5f, 0.5f);
            containerRT.anchoredPosition = Vector2.zero;
            containerRT.sizeDelta = new Vector2(940, 720);

            Image containerBg = containerGO.GetComponent<Image>() ?? containerGO.AddComponent<Image>();
            containerBg.sprite = panelBg;
            containerBg.color = new Color(0.08f, 0.10f, 0.16f, 0.98f);
            containerBg.type = Image.Type.Sliced;

            // Clear old children inside container
            for (int i = containerGO.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(containerGO.transform.GetChild(i).gameObject);
            }

            // Decorative border glow
            GameObject borderGlowGO = new GameObject("ui_image_border_glow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            borderGlowGO.transform.SetParent(containerGO.transform, false);
            RectTransform borderRT = borderGlowGO.GetComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.sizeDelta = Vector2.zero;
            Image borderImg = borderGlowGO.GetComponent<Image>();
            borderImg.sprite = gradientFrame;
            borderImg.color = new Color(1f, 0.80f, 0.25f, 0.75f); // Radiant gold glow
            borderImg.type = Image.Type.Sliced;
            borderImg.raycastTarget = false;

            // Title
            GameObject titleGO = new GameObject("ui_text_cashout_title_value", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            titleGO.transform.SetParent(containerGO.transform, false);
            RectTransform titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 1f);
            titleRT.anchorMax = new Vector2(0.5f, 1f);
            titleRT.pivot = new Vector2(0.5f, 1f);
            titleRT.anchoredPosition = new Vector2(0f, -32f);
            titleRT.sizeDelta = new Vector2(880, 52);
            TextMeshProUGUI titleTxt = titleGO.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) titleTxt.font = fontAsset;
            titleTxt.fontSize = 36;
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.alignment = TextAlignmentOptions.Center;
            titleTxt.color = new Color(1f, 0.88f, 0.22f, 1f);
            titleTxt.text = "EXIT & SECURE LOOT?";

            // Subtitle
            GameObject subtitleGO = new GameObject("ui_text_cashout_subtitle_value", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            subtitleGO.transform.SetParent(containerGO.transform, false);
            RectTransform subRT = subtitleGO.GetComponent<RectTransform>();
            subRT.anchorMin = new Vector2(0.5f, 1f);
            subRT.anchorMax = new Vector2(0.5f, 1f);
            subRT.pivot = new Vector2(0.5f, 1f);
            subRT.anchoredPosition = new Vector2(0f, -92f);
            subRT.sizeDelta = new Vector2(820, 48);
            TextMeshProUGUI subTxt = subtitleGO.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) subTxt.font = fontAsset;
            subTxt.fontSize = 16;
            subTxt.alignment = TextAlignmentOptions.Center;
            subTxt.color = new Color(0.80f, 0.84f, 0.92f, 0.9f);
            subTxt.text = "Walking away safely? All earned rewards will be secured in your permanent vault!";

            // Currency Summary Container
            GameObject currSummGO = new GameObject("ui_panel_currency_summary", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            currSummGO.transform.SetParent(containerGO.transform, false);
            RectTransform currSummRT = currSummGO.GetComponent<RectTransform>();
            currSummRT.anchorMin = new Vector2(0.5f, 1f);
            currSummRT.anchorMax = new Vector2(0.5f, 1f);
            currSummRT.pivot = new Vector2(0.5f, 1f);
            currSummRT.anchoredPosition = new Vector2(0f, -150f);
            currSummRT.sizeDelta = new Vector2(860, 68);
            Image currSummImg = currSummGO.GetComponent<Image>();
            currSummImg.sprite = neutralFrame;
            currSummImg.color = new Color(0.12f, 0.16f, 0.24f, 0.92f);
            currSummImg.type = Image.Type.Sliced;

            // Cash Summary item
            GameObject cashIconGO = new GameObject("ui_image_cash_summary", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cashIconGO.transform.SetParent(currSummGO.transform, false);
            RectTransform cashIconRT = cashIconGO.GetComponent<RectTransform>();
            cashIconRT.anchoredPosition = new Vector2(-220f, 0f);
            cashIconRT.sizeDelta = new Vector2(44, 44);
            Image cashIconImg = cashIconGO.GetComponent<Image>();
            cashIconImg.sprite = cashIcon;
            cashIconImg.preserveAspect = true;
            cashIconImg.raycastTarget = false;

            GameObject cashTxtGO = new GameObject("ui_text_cash_gained_value", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            cashTxtGO.transform.SetParent(currSummGO.transform, false);
            RectTransform cashTxtRT = cashTxtGO.GetComponent<RectTransform>();
            cashTxtRT.anchoredPosition = new Vector2(-110f, 0f);
            cashTxtRT.sizeDelta = new Vector2(170, 44);
            TextMeshProUGUI cashTxt = cashTxtGO.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) cashTxt.font = fontAsset;
            cashTxt.fontSize = 24;
            cashTxt.fontStyle = FontStyles.Bold;
            cashTxt.alignment = TextAlignmentOptions.Left;
            cashTxt.color = new Color(0.2f, 0.95f, 0.35f, 1f); // Vibrant Emerald
            cashTxt.text = "+$1,000";
            cashTxt.raycastTarget = false;

            // Gold Summary item
            GameObject goldIconGO = new GameObject("ui_image_gold_summary", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            goldIconGO.transform.SetParent(currSummGO.transform, false);
            RectTransform goldIconRT = goldIconGO.GetComponent<RectTransform>();
            goldIconRT.anchoredPosition = new Vector2(120f, 0f);
            goldIconRT.sizeDelta = new Vector2(44, 44);
            Image goldIconImg = goldIconGO.GetComponent<Image>();
            goldIconImg.sprite = goldIcon;
            goldIconImg.preserveAspect = true;
            goldIconImg.raycastTarget = false;

            GameObject goldTxtGO = new GameObject("ui_text_gold_gained_value", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            goldTxtGO.transform.SetParent(currSummGO.transform, false);
            RectTransform goldTxtRT = goldTxtGO.GetComponent<RectTransform>();
            goldTxtRT.anchoredPosition = new Vector2(210f, 0f);
            goldTxtRT.sizeDelta = new Vector2(140, 44);
            TextMeshProUGUI goldTxt = goldTxtGO.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) goldTxt.font = fontAsset;
            goldTxt.fontSize = 24;
            goldTxt.fontStyle = FontStyles.Bold;
            goldTxt.alignment = TextAlignmentOptions.Left;
            goldTxt.color = new Color(1f, 0.85f, 0.15f, 1f); // Radiant Gold
            goldTxt.text = "+10";
            goldTxt.raycastTarget = false;
            goldTxt.fontStyle = FontStyles.Bold;
            goldTxt.alignment = TextAlignmentOptions.Left;
            goldTxt.color = new Color(1f, 0.85f, 0.15f, 1f); // Radiant Gold
            goldTxt.text = "+10";

            // Horizontal Side-Scroll Box Container
            GameObject scrollBoxGO = new GameObject("ui_scroll_rect_rewards", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect));
            scrollBoxGO.transform.SetParent(containerGO.transform, false);
            RectTransform scrollBoxRT = scrollBoxGO.GetComponent<RectTransform>();
            scrollBoxRT.anchorMin = new Vector2(0.5f, 0.5f);
            scrollBoxRT.anchorMax = new Vector2(0.5f, 0.5f);
            scrollBoxRT.pivot = new Vector2(0.5f, 0.5f);
            scrollBoxRT.anchoredPosition = new Vector2(0f, 20f);
            scrollBoxRT.sizeDelta = new Vector2(880, 220);

            Image scrollBoxBg = scrollBoxGO.GetComponent<Image>();
            scrollBoxBg.sprite = neutralFrame;
            scrollBoxBg.color = new Color(0.05f, 0.07f, 0.11f, 0.75f);
            scrollBoxBg.type = Image.Type.Sliced;

            ScrollRect scrollRect = scrollBoxGO.GetComponent<ScrollRect>();

            // Viewport
            GameObject viewportGO = new GameObject("ui_viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
            viewportGO.transform.SetParent(scrollBoxGO.transform, false);
            RectTransform viewportRT = viewportGO.GetComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = new Vector2(-16, -16);
            viewportRT.anchoredPosition = Vector2.zero;
            Image vpImg = viewportGO.GetComponent<Image>();
            vpImg.color = Color.clear;
            vpImg.raycastTarget = true;

            // Content Container
            GameObject contentGO = new GameObject("ui_transform_rewards_content", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            contentGO.transform.SetParent(viewportGO.transform, false);
            RectTransform contentRT = contentGO.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 0f);
            contentRT.anchorMax = new Vector2(0f, 1f);
            contentRT.pivot = new Vector2(0f, 0.5f);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(0, 0);

            HorizontalLayoutGroup hlg = contentGO.GetComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.spacing = 14f;
            hlg.padding = new RectOffset(14, 14, 8, 8);

            ContentSizeFitter csf = contentGO.GetComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            scrollRect.viewport = viewportRT;
            scrollRect.content = contentRT;
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.scrollSensitivity = 35f;

            // Empty hint text inside scrollbox
            GameObject emptyHintGO = new GameObject("ui_text_empty_hint_value", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            emptyHintGO.transform.SetParent(scrollBoxGO.transform, false);
            RectTransform emptyRT = emptyHintGO.GetComponent<RectTransform>();
            emptyRT.anchorMin = new Vector2(0.5f, 0.5f);
            emptyRT.anchorMax = new Vector2(0.5f, 0.5f);
            emptyRT.pivot = new Vector2(0.5f, 0.5f);
            emptyRT.anchoredPosition = Vector2.zero;
            emptyRT.sizeDelta = new Vector2(600, 40);
            TextMeshProUGUI emptyTxt = emptyHintGO.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) emptyTxt.font = fontAsset;
            emptyTxt.fontSize = 18;
            emptyTxt.alignment = TextAlignmentOptions.Center;
            emptyTxt.color = new Color(0.6f, 0.65f, 0.75f, 0.8f);
            emptyTxt.text = "No rewards collected in this run yet.";
            emptyHintGO.SetActive(false);

            // Action Buttons Row
            GameObject btnRowGO = new GameObject("ui_panel_button_row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            btnRowGO.transform.SetParent(containerGO.transform, false);
            RectTransform btnRowRT = btnRowGO.GetComponent<RectTransform>();
            btnRowRT.anchorMin = new Vector2(0.5f, 0f);
            btnRowRT.anchorMax = new Vector2(0.5f, 0f);
            btnRowRT.pivot = new Vector2(0.5f, 0f);
            btnRowRT.anchoredPosition = new Vector2(0f, 32f);
            btnRowRT.sizeDelta = new Vector2(880, 72);

            HorizontalLayoutGroup btnHlg = btnRowGO.GetComponent<HorizontalLayoutGroup>();
            btnHlg.childAlignment = TextAnchor.MiddleCenter;
            btnHlg.childControlWidth = false;
            btnHlg.childControlHeight = false;
            btnHlg.childForceExpandWidth = false;
            btnHlg.childForceExpandHeight = false;
            btnHlg.spacing = 30f;

            // 1. Cancel Button
            GameObject cancelBtnGO = new GameObject("ui_button_exit_cancel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            cancelBtnGO.transform.SetParent(btnRowGO.transform, false);
            RectTransform cancelBtnRT = cancelBtnGO.GetComponent<RectTransform>();
            cancelBtnRT.sizeDelta = new Vector2(340, 68);
            Image cancelBtnImg = cancelBtnGO.GetComponent<Image>();
            cancelBtnImg.sprite = greyBtnSprite;
            cancelBtnImg.type = Image.Type.Sliced;
            Button cancelBtn = cancelBtnGO.GetComponent<Button>();

            GameObject cancelTxtGO = new GameObject("ui_text_button_cancel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            cancelTxtGO.transform.SetParent(cancelBtnGO.transform, false);
            RectTransform cancelTxtRT = cancelTxtGO.GetComponent<RectTransform>();
            cancelTxtRT.anchorMin = Vector2.zero;
            cancelTxtRT.anchorMax = Vector2.one;
            cancelTxtRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI cancelTxt = cancelTxtGO.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) cancelTxt.font = fontAsset;
            cancelTxt.fontSize = 20;
            cancelTxt.fontStyle = FontStyles.Bold;
            cancelTxt.alignment = TextAlignmentOptions.Center;
            cancelTxt.color = Color.white;
            cancelTxt.text = "CONTINUE PLAYING";
            cancelTxt.raycastTarget = false;

            // 2. Confirm Exit Button
            GameObject confirmBtnGO = new GameObject("ui_button_exit_confirm", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            confirmBtnGO.transform.SetParent(btnRowGO.transform, false);
            RectTransform confirmBtnRT = confirmBtnGO.GetComponent<RectTransform>();
            confirmBtnRT.sizeDelta = new Vector2(340, 68);
            Image confirmBtnImg = confirmBtnGO.GetComponent<Image>();
            confirmBtnImg.sprite = orangeBtnSprite;
            confirmBtnImg.type = Image.Type.Sliced;
            Button confirmBtn = confirmBtnGO.GetComponent<Button>();

            GameObject confirmTxtGO = new GameObject("ui_text_button_confirm", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            confirmTxtGO.transform.SetParent(confirmBtnGO.transform, false);
            RectTransform confirmTxtRT = confirmTxtGO.GetComponent<RectTransform>();
            confirmTxtRT.anchorMin = Vector2.zero;
            confirmTxtRT.anchorMax = Vector2.one;
            confirmTxtRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI confirmTxt = confirmTxtGO.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null) confirmTxt.font = fontAsset;
            confirmTxt.fontSize = 22;
            confirmTxt.fontStyle = FontStyles.Bold;
            confirmTxt.alignment = TextAlignmentOptions.Center;
            confirmTxt.color = Color.white;
            confirmTxt.text = "TAKE LOOT & EXIT";
            confirmTxt.raycastTarget = false;

            // Configure UICashOutModalView component on root
            UICashOutModalView modalView = rootGO.GetComponent<UICashOutModalView>() ?? rootGO.AddComponent<UICashOutModalView>();
            modalView.SetReferences(cancelBtn, confirmBtn, scrollRect, contentRT, savedCardPrefab.GetComponent<UIExitRewardCardItemView>());
            EditorUtility.SetDirty(modalView);

            // Save ExitModal_View Prefab
            string modalPrefabPath = $"{PrefabsRoot}/ExitModal_View.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(rootGO, modalPrefabPath, InteractionMode.AutomatedAction);
            Debug.Log($"[ExitModalPrefabBuilder] Saved ExitModal_View prefab: {modalPrefabPath}");

            rootGO.SetActive(false); // Initially hidden

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[ExitModalPrefabBuilder] ★ Exit Modal and all modular prefabs built successfully!");
        }
    }
}
