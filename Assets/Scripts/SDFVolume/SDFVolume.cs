using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent (typeof (MeshRenderer))]
public class SDFVolume : MonoBehaviour {

    [SerializeField] SDFAsset sdfAsset;
    [SerializeField] Color color = Color.white;
    [SerializeField, Range (0f, 1f)] float glossiness = 0.5f;
    [SerializeField, Range (0f, 1f)] float metallic = 0f;

    MeshRenderer meshRenderer;
    Material material;

    void OnEnable () {
        material = new Material (Shader.Find ("Custom/SDFVolume"));
        material.name = "Custom/SDFVolume";

        meshRenderer = GetComponent<MeshRenderer> ();
        meshRenderer.sharedMaterial = material;

        OnValidate ();
    }

    void OnValidate () {
        if (material == null || sdfAsset == null) return;

        material.SetTexture ("_Volume", sdfAsset.texture);
        material.SetColor ("_Color", color);
        material.SetFloat ("_Glossiness", glossiness);
        material.SetFloat ("_Metallic", metallic);
    }
}