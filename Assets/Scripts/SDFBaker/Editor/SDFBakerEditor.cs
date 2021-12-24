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
        if (GUILayout.Button ("Create 3D Texture")) {
            Create3DTextureAsset (baker);
        }
        if (GUILayout.Button ("Bake")) {
            baker.Bake ();
        }
    }

    void Create3DTextureAsset (SDFBaker baker) {
        var path = EditorUtility.SaveFilePanel ("Create 3D Texture", Application.dataPath, "SDF Texture.asset", "asset");
        if (path.Length != 0) {
            var texture = baker.CreateTexture3D ();
            var relativePath = path.Substring (path.IndexOf ("Assets/"));
            AssetDatabase.CreateAsset (texture, relativePath);
        }
    }
}
