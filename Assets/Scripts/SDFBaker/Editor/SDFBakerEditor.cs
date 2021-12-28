using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (SDFBaker))]
public class SDFBakerEditor : Editor {
    public override void OnInspectorGUI () {
        base.OnInspectorGUI ();

        var baker = target as SDFBaker;
        var disableBaking = ((int)baker.resolution >= 64) && (baker.bakeMethod == SDFBaker.BakeMethod.CPUBruteForce);
        GUI.enabled = !disableBaking;
        if (GUILayout.Button ("Bake")) {
            baker.Bake ();
        }
        GUI.enabled = true;
        if (disableBaking) {
            EditorGUILayout.HelpBox ("Disabled CPU baking when resolution larger than 32.", MessageType.Info);
        }
        if (GUILayout.Button ("Save as Texture Asset")) {
            SaveAsTextureAsset (baker);
        }
    }

    void SaveAsTextureAsset (SDFBaker baker) {
        var path = EditorUtility.SaveFilePanel ("Save as ...", Application.dataPath, "SDF Texture.asset", "asset");
        if (path.Length != 0) {
            var relativePath = path.Substring (path.IndexOf ("Assets/"));
            AssetDatabase.CreateAsset (baker.targetTexture, relativePath);
        }
    }
}
