using UnityEngine;
using UnityEditor;
using Wigro.Runtime;
using System;

namespace Wigro.Editor
{
    [CustomEditor(typeof(Settings))]
    internal sealed class SettingsInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            SerializedObject serializedSettings = new SerializedObject(target);

            SerializedProperty folderPorperty = serializedSettings.FindProperty("_folder");
            EditorGUILayout.PropertyField(folderPorperty, new GUIContent("Output Folder"));
            if (folderPorperty != null)
            {
                string path = AssetDatabase.GetAssetPath(folderPorperty.objectReferenceValue);

                if (!AssetDatabase.IsValidFolder(path))
                {
                    EditorGUILayout.HelpBox("Not a folder!", MessageType.Error);
                    folderPorperty.objectReferenceValue = null;
                }
            }

            SerializedProperty amountProperty = serializedSettings.FindProperty("_amount");
            int newAmount = EditorGUILayout.IntField("Inventory Size", amountProperty.intValue);
            amountProperty.intValue = Math.Max(newAmount, 10);

            SerializedProperty flagsProperty = serializedSettings.FindProperty("_flags");
            int currentFlags = flagsProperty.intValue;

            bool openAnimated = (currentFlags & (int)InventoryFlags.OpenAnimated) != 0;
            openAnimated = EditorGUILayout.Toggle("Open Animated", openAnimated);

            bool closeAnimated = (currentFlags & (int)InventoryFlags.CloseAnimated) != 0;
            closeAnimated = EditorGUILayout.Toggle("Close Animated", closeAnimated);

            bool showInfo = (currentFlags & (int)InventoryFlags.ShowInfo) != 0;
            showInfo = EditorGUILayout.Toggle("Show Info", showInfo);

            int newFlags = 0;

            if (openAnimated) 
                newFlags |= (int)InventoryFlags.OpenAnimated;

            if (closeAnimated) 
                newFlags |= (int)InventoryFlags.CloseAnimated;

            if (showInfo) 
                newFlags |= (int)InventoryFlags.ShowInfo;

            flagsProperty.intValue = newFlags;

            serializedSettings.ApplyModifiedProperties();
        }
    }
}

