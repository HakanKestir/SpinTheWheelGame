using UnityEditor;
using UnityEngine;
using WheelOfFortune.Data;

namespace WheelOfFortune.Editor
{
    [CustomEditor(typeof(ZoneConfigSO))]
    public class ZoneConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty safeZoneIntervalProp;
        private SerializedProperty superZoneIntervalProp;
        private SerializedProperty restrictExitToSafeZonesProp;
        private SerializedProperty bronzeWheelConfigProp;
        private SerializedProperty silverWheelConfigProp;
        private SerializedProperty goldenWheelConfigProp;
        private SerializedProperty enableRewardMultiplicationProp;
        private SerializedProperty multiplierIncrementIntervalProp;
        private SerializedProperty superZonePreviewRewardProp;
        private SerializedProperty reviveGoldCostProp;
        private SerializedProperty initialCashProp;
        private SerializedProperty initialGoldProp;

        private void OnEnable()
        {
            safeZoneIntervalProp = serializedObject.FindProperty("safeZoneInterval");
            superZoneIntervalProp = serializedObject.FindProperty("superZoneInterval");
            restrictExitToSafeZonesProp = serializedObject.FindProperty("restrictExitToSafeZones");
            bronzeWheelConfigProp = serializedObject.FindProperty("bronzeWheelConfig");
            silverWheelConfigProp = serializedObject.FindProperty("silverWheelConfig");
            goldenWheelConfigProp = serializedObject.FindProperty("goldenWheelConfig");
            enableRewardMultiplicationProp = serializedObject.FindProperty("enableRewardMultiplication");
            multiplierIncrementIntervalProp = serializedObject.FindProperty("multiplierIncrementInterval");
            superZonePreviewRewardProp = serializedObject.FindProperty("superZonePreviewReward");
            reviveGoldCostProp = serializedObject.FindProperty("reviveGoldCost");
            initialCashProp = serializedObject.FindProperty("initialCash");
            initialGoldProp = serializedObject.FindProperty("initialGold");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Zone Progression & Intervals", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(safeZoneIntervalProp);
            EditorGUILayout.PropertyField(superZoneIntervalProp);
            EditorGUILayout.PropertyField(restrictExitToSafeZonesProp, new GUIContent("Restrict Exit To Safe/Super Zones"));
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Wheel Configurations", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(bronzeWheelConfigProp);
            EditorGUILayout.PropertyField(silverWheelConfigProp);
            EditorGUILayout.PropertyField(goldenWheelConfigProp);
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Multiplier Controls", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(enableRewardMultiplicationProp, new GUIContent("Enable Reward Multiplication"));

            EditorGUI.BeginDisabledGroup(!enableRewardMultiplicationProp.boolValue);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(multiplierIncrementIntervalProp, new GUIContent("Multiplier Increment Interval (Zones)"));
            if (!enableRewardMultiplicationProp.boolValue)
            {
                EditorGUILayout.HelpBox("Reward multiplication is disabled. Multiplier remains at 1x across all zones.", MessageType.Info);
            }
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Super Zone Preview", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(superZonePreviewRewardProp);
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Economy & Revive", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(reviveGoldCostProp);
            EditorGUILayout.PropertyField(initialCashProp);
            EditorGUILayout.PropertyField(initialGoldProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
