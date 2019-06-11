using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent (typeof (MeshRenderer))]
public class SDFVolume : MonoBehaviour {

    [SerializeField] SDFAsset sdfAsset;
    [SerializeField] float scale = 1f;

    [SerializeField] Color color = Color.white;
    [SerializeField, Range (0f, 1f)] float glossiness = 0.5f;
    [SerializeField, Range (0f, 1f)] float metallic = 0f;

    Material material;

    void OnEnable () {
        material = new Material (Shader.Find ("Custom/SDFVolume"));
        material.name = "Custom/SDFVolume";
        OnValidate ();
    }

    void OnValidate () {
        if (material == null || sdfAsset == null) return;

        material.SetTexture ("_Volume", sdfAsset.texture);
        material.SetColor ("_Color", color);
        material.SetFloat ("_Glossiness", glossiness);
        material.SetFloat ("_Metallic", metallic);
    }

    void OnRenderObject () {
        var matrix = transform.localToWorldMatrix;
        matrix = matrix * Matrix4x4.Scale (new Vector3 (scale, scale, scale));
        Graphics.DrawMesh (SDFUtils.volumeCubeMesh, matrix, material, 0);
    }
}