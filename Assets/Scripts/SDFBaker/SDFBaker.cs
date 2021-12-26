using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent (typeof (MeshFilter))]
public class SDFBaker : MonoBehaviour {

    [Header ("Settings")]
    public float boundsPadding = 0.1f;
    public Texture3D targetTexture;

    [Header ("Create Config")]
    public Precision precision = Precision.Half;
    public int resolution = 32;

    [Header ("SDF Viewer")]
    public bool enable = false;
    [Range (0f, 1f)] public float slice = 0.5f;

    [SerializeField, HideInInspector]
    Material viewerMaterial;
    static Mesh quadMesh;

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
        if (targetTexture == null) {
            targetTexture = CreateTexture3D ();
        }

        // var colors = new Color[resolution * resolution * resolution];
        // for (int z = 0, index = 0; z < resolution; z++) {
        //     for (int y = 0; y < resolution; y++) {
        //         for (int x = 0; x < resolution; x++, index++) {
        //             var position = new Vector3 (x, y, z) * (1.0f / resolution) + Vector3.one * (0.5f / resolution);
        //             var radius = 0.4f;
        //             var dis = (position - Vector3.one * 0.5f).magnitude - radius;
        //             var color = new Color ((dis + 1.0f) * 0.5f, 0f, 0f);
        //             colors[index] = color;
        //         }
        //     }
        // }
        // targetTexture.SetPixels (colors);

        var distances = CpuBruteForce (resolution);
        targetTexture.SetPixels (distances.Select (d => new Color ((d + 1.0f) * 0.5f, 0f, 0f)).ToArray ());
        targetTexture.Apply ();
    }

    float[] CpuBruteForce (int resolution) {
        var meshFiler = GetComponent<MeshFilter> ();
        var mesh = meshFiler.sharedMesh;
        var vertices = mesh.vertices;
        var indices = mesh.GetIndices (0);
        var bounds = mesh.bounds;
        bounds.size = bounds.size + Vector3.one * boundsPadding;
        var maxEdge = Mathf.Max (bounds.size.x, bounds.size.y, bounds.size.z);

        var distances = new float[resolution * resolution * resolution];
        for (int z = 0, index = 0; z < resolution; z++) {
            for (int y = 0; y < resolution; y++) {
                for (int x = 0; x < resolution; x++, index++) {
                    var uv = GetTexcoord (x, y, z, resolution);
                    var samplePos = GetPosition (bounds, uv);

                    var d = maxEdge;
                    for (int i = 0; i < indices.Length; i += 3) {
                        var v1 = vertices[indices[i]];
                        var v2 = vertices[indices[i + 1]];
                        var v3 = vertices[indices[i + 2]];
                        d = Mathf.Min (d, DistanceToTriangle (samplePos, v1, v2, v3));
                    }
                    distances[index] = Mathf.Clamp (d / maxEdge, -1f, 1f);
                }
            }
        }
        return distances;
    }

    // ******* //
    // Helpers //
    // ******* //
    static Vector3 GetTexcoord (int x, int y, int z, int resolution) {
        return new Vector3 (x, y, z) * (1.0f / resolution) + Vector3.one * (0.5f / resolution);
    }

    static Vector3 GetPosition (Bounds bounds, Vector3 texcoord) {
        return bounds.min + new Vector3 (bounds.size.x * texcoord.x, bounds.size.y * texcoord.y, bounds.size.z * texcoord.z);
    }

    static float DistanceToTriangle (Vector3 p, Vector3 v1, Vector3 v2, Vector3 v3) {
        var v21 = v2 - v1;
        var v32 = v3 - v2;
        var v13 = v1 - v3;
        var n = Vector3.Cross (v21, v13);
        var p1 = p - v1;
        var p2 = p - v2;
        var p3 = p - v3;

        var outside =
            Mathf.Sign (Vector3.Dot (Vector3.Cross (v21, n), p1)) +
            Mathf.Sign (Vector3.Dot (Vector3.Cross (v32, n), p2)) +
            Mathf.Sign (Vector3.Dot (Vector3.Cross (v13, n), p3)) < 2.0f;

        if (outside) {
            var d1 = (v21 * Mathf.Clamp01 (Vector3.Dot (v21, p1) / v21.sqrMagnitude) - p1).magnitude;
            var d2 = (v32 * Mathf.Clamp01 (Vector3.Dot (v32, p2) / v32.sqrMagnitude) - p2).magnitude;
            var d3 = (v13 * Mathf.Clamp01 (Vector3.Dot (v13, p3) / v21.sqrMagnitude) - p3).magnitude;
            return Mathf.Min (d1, d2, d3);
        } else {
            return Mathf.Abs (Vector3.Dot (n, p1) / n.magnitude);
        }
    }

    // ****** //
    // Gizmos //
    // ****** //
    void OnDrawGizmos () {
        var meshFiler = GetComponent<MeshFilter> ();
        var bounds = meshFiler.sharedMesh.bounds;
        var paddedSize = bounds.size + Vector3.one * boundsPadding;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color (1f, 1f, 1f, .3f);
        Gizmos.DrawWireCube (bounds.center, bounds.size);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube (bounds.center, paddedSize);

        if (enable && viewerMaterial != null) {
            var scaledSize = new Vector3 (transform.localScale.x * paddedSize.x, transform.localScale.y * paddedSize.y, transform.localScale.z * paddedSize.z);
            var pos = transform.position + transform.forward * paddedSize.z * (slice - 0.5f);
            var matrix = Matrix4x4.TRS (pos, transform.rotation, scaledSize);
            viewerMaterial.SetTexture ("_SDFTexture", targetTexture);
            viewerMaterial.SetFloat ("_Slice", slice);
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
