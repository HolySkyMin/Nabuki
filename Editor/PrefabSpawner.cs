using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Nabuki.Editor
{
    public static class PrefabSpawner
    {
        [MenuItem("GameObject/Nabuki/Templates/Dialogue Character", false, 24)]
        public static void SpawnCharacterTemplate()
        {
            Selection.activeObject = PrefabUtility.InstantiatePrefab(Resources.Load("Character Template"));
            PrefabUtility.UnpackPrefabInstance(Selection.activeObject as GameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        [MenuItem("GameObject/Nabuki/Templates/Dialogue Field", false, 24)]
        public static void SpawnFieldTemplate()
        {
            Selection.activeObject = PrefabUtility.InstantiatePrefab(Resources.Load("Dialogue Field"));
            PrefabUtility.UnpackPrefabInstance(Selection.activeObject as GameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        [MenuItem("GameObject/Nabuki/Templates/Dialogue Audio", false, 24)]
        public static void SpawnAudioTemplate()
        {
            Selection.activeObject = PrefabUtility.InstantiatePrefab(Resources.Load("Dialogue Audio"));
            PrefabUtility.UnpackPrefabInstance(Selection.activeObject as GameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        [MenuItem("GameObject/Nabuki/Templates/Dialogue Background", false, 24)]
        public static void SpawnBackgroundTemplate()
        {
            Selection.activeObject = PrefabUtility.InstantiatePrefab(Resources.Load("Dialogue Background"));
            PrefabUtility.UnpackPrefabInstance(Selection.activeObject as GameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        [MenuItem("GameObject/Nabuki/Standard Dialogue", false, 25)]
        public static void SpawnStandardDialogue()
        {
            Selection.activeObject = PrefabUtility.InstantiatePrefab(Resources.Load("Standard Dialogue"));
            PrefabUtility.UnpackPrefabInstance(Selection.activeObject as GameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        [MenuItem("GameObject/Nabuki/Standard Dialogue UGUI", false, 25)]
        public static void SpawnStandardDialogueUGUI()
        {
            Selection.activeObject = PrefabUtility.InstantiatePrefab(Resources.Load("Standard Dialogue UGUI"));
            PrefabUtility.UnpackPrefabInstance(Selection.activeObject as GameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
    }
}