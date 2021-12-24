using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (MeshFilter))]
public class SDFBaker : MonoBehaviour {

    [Header ("Settings")]
    public float padding = 0.1f;
    public Texture3D targetTexture;

    [Header ("Create Config")]
    public Precision precision = Precision.Half;
    public int resolution = 64;

    static Mesh quadMesh;
    [SerializeField, HideInInspector] Material viewerMaterial;

    public enum Precision {
        Half,
        Float
    }

    public Texture3D CreateTexture3D () {
        var textureFormat = TextureFormat.RHalf;
        if (precision == Precision.Float) {
            textureFormat = TextureFormat.RFloat;
        }

        var texture = new Texture3D (resolution, resolution, resolution, textureFormat, false);
        texture.name = "SDF Texture";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply ();

        targetTexture = texture;
        return texture;
    }

    public void Bake () {
        var colors = new Color[resolution * resolution * resolution];
        for (int i = 0; i < colors.Length; i++) {
            colors[i] = Color.white;
        }
        targetTexture.SetPixels (colors);
        targetTexture.Apply ();
    }

    void OnDrawGizmos () {
        var meshFiler = GetComponent<MeshFilter> ();
        var bounds = meshFiler.sharedMesh.bounds;
        var paddedSize = bounds.size + Vector3.one * padding;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color (1f, 1f, 1f, .3f);
        Gizmos.DrawWireCube (bounds.center, bounds.size);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube (bounds.center, paddedSize);

        if (viewerMaterial != null) {
            var scaledSize = new Vector3 (transform.localScale.x * paddedSize.x, transform.localScale.y * paddedSize.y, transform.localScale.z * paddedSize.z);
            var matrix = Matrix4x4.TRS (transform.position, transform.rotation, scaledSize);
            viewerMaterial.SetPass (0);
            Graphics.DrawMeshNow (GetQuadMesh (), matrix);
        }
    }

    static Mesh GetQuadMesh () {
        if (quadMesh == null) {
            var go = GameObject.CreatePrimitive (PrimitiveType.Quad);
            quadMesh = go.GetComponent<MeshFilter> ().sharedMesh;
            DestroyImmediate (go);
        }
        return quadMesh;
    }
}
