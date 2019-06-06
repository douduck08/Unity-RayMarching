using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor (typeof (SDFSampler))]
public class SDFSamplerEditor : Editor {

    SerializedProperty resolutionProperty;
    SerializedProperty scaleProperty;
    SerializedProperty iterationProperty;

    void OnEnable () {
        resolutionProperty = serializedObject.FindProperty ("resolution");
        scaleProperty = serializedObject.FindProperty ("scale");
        iterationProperty = serializedObject.FindProperty ("iteration");
    }

    public override void OnInspectorGUI () {
        serializedObject.Update ();

        EditorGUILayout.PropertyField (resolutionProperty);
        EditorGUILayout.PropertyField (scaleProperty);
        EditorGUILayout.PropertyField (iterationProperty);

        serializedObject.ApplyModifiedProperties ();

        if (GUILayout.Button ("Create Volume Data")) CreateVolumeData ();
    }

    void CreateVolumeData () {
        string path = EditorUtility.SaveFilePanelInProject ("Save SDF Volume into SDF asset", "", "asset", "");
        if (string.IsNullOrEmpty (path)) {
            return;
        }

        var output = new List<Object> ();
        foreach (SDFSampler sampler in targets) {
            var newPath = AssetDatabase.GenerateUniqueAssetPath (path);
            var asset = ScriptableObject.CreateInstance<SDFAsset> ();
            asset.Initialize (sampler);

            AssetDatabase.CreateAsset (asset, newPath);
            AssetDatabase.AddObjectToAsset (asset.texture, asset);
            output.Add (asset);
        }

        AssetDatabase.SaveAssets ();
        EditorUtility.FocusProjectWindow ();
        Selection.objects = output.ToArray ();
    }
}