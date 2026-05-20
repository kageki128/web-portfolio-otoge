using MyProject.Actor;
using MyProject.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(OtogeActorHub))]
public class OtogeActorHubEditor : Editor
{
    OtogeType previewType = OtogeType.Tetra;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Shared Actor State", EditorStyles.boldLabel);
        previewType = (OtogeType)EditorGUILayout.EnumPopup("Otoge Type", previewType);
        EditorGUILayout.HelpBox("編集モードで押して、Shared Actorの状態を一括反映します。", MessageType.Info);

        using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
        {
            if (!GUILayout.Button("Apply To Shared Actors")) return;
        }

        foreach (var targetObject in targets)
        {
            var hub = (OtogeActorHub)targetObject;
            Undo.RegisterFullObjectHierarchyUndo(hub.gameObject, "Apply Shared Actors State");
            hub.SetSharedActorsState(previewType);
            EditorUtility.SetDirty(hub);
            EditorSceneManager.MarkSceneDirty(hub.gameObject.scene);
        }
    }
}
