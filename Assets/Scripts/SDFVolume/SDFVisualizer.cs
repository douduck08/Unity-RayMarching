using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SDFVisualizer : MonoBehaviour {

    public enum Mode {
        Distance,
        Gradient
    }

    [SerializeField] SDFAsset sdfAsset;
    [SerializeField] Mode mode;
    [SerializeField, Range (0f, 1f)] float depth = 0.5f;

    MeshRenderer meshRenderer;
    Material material;

    void OnEnable () {
        material = new Material (Shader.Find ("Hidden/SDFVisualizer"));
        material.name = "Hidden/SDFVisualizer";

        meshRenderer = GetComponent<MeshRenderer> ();
        meshRenderer.sharedMaterial = material;

        OnValidate ();
    }

    void OnValidate () {
        if (material == null || sdfAsset == null) return;

        material.SetTexture ("_MainTex", sdfAsset.texture);
        material.SetFloat ("_Depth", depth);
        material.SetFloat ("_Mode", (float) mode);
    }
}