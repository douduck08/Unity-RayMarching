using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent (typeof (MeshFilter))]
public class SDFBaker : MonoBehaviour {

    public enum Resolution {
        _8 = 8,
        _16 = 16,
        _32 = 32,
        _64 = 64,
    }

    public enum Precision {
        Half,
        Float
    }

    [Header ("Bake Settings")]
    public float boundsPadding = 0.1f;
    public Resolution resolution = Resolution._32;
    public Precision precision = Precision.Half;
    public Texture3D targetTexture;

    [Header ("SDF Viewer")]
    public bool enable = false;
    [Range (0f, 1f)] public float slice = 0.5f;

    static Mesh quadMesh;
    [SerializeField, HideInInspector] Material viewerMaterial;

    TextureFormat ParseTextureFormat () {
        switch (precision) {
            case Precision.Half:
                return TextureFormat.RHalf;
            case Precision.Float:
                return TextureFormat.RFloat;
        }
        return TextureFormat.RHalf;
    }

    Texture3D CreateTexture3D () {
        var textureFormat = ParseTextureFormat ();
        var resolution = (int)this.resolution;
        var texture = new Texture3D (resolution, resolution, resolution, textureFormat, false);
        texture.name = "SDF Texture (Instance)";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply ();

        targetTexture = texture;
        return texture;
    }

    public void Bake () {
        var resolution = (int)this.resolution;
        if (targetTexture == null) {
            targetTexture = CreateTexture3D ();
        } else {
            if (targetTexture.width != targetTexture.height || targetTexture.width != targetTexture.depth || targetTexture.height != targetTexture.depth) {
                Debug.Log ("SDFBaker: Target texture is equilateral. Created a new instance.");
                targetTexture = CreateTexture3D ();
            } else if (targetTexture.width != resolution) {
                Debug.Log ("SDFBaker: Target texture resolition is not matched. Created a new instance.");
                targetTexture = CreateTexture3D ();
            } else if (targetTexture.format != ParseTextureFormat ()) {
                Debug.Log ("SDFBaker: Target texture format is not matched. Created a new instance.");
                targetTexture = CreateTexture3D ();
            }
        }

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
        var rayDirection = new Vector3 (Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f));
        for (int z = 0, index = 0; z < resolution; z++) {
            for (int y = 0; y < resolution; y++) {
                for (int x = 0; x < resolution; x++, index++) {
                    var uv = GetTexcoord (x, y, z, resolution);
                    var samplePos = GetPosition (bounds, uv);

                    var d = maxEdge;
                    var intersectionCount = 0;
                    for (int i = 0; i < indices.Length; i += 3) {
                        var v1 = vertices[indices[i]];
                        var v2 = vertices[indices[i + 1]];
                        var v3 = vertices[indices[i + 2]];
                        d = Mathf.Min (d, DistanceToTriangle (samplePos, v1, v2, v3));
                        if (RaycastToTriangle (samplePos, rayDirection, v1, v2, v3)) {
                            intersectionCount += 1;
                        }
                    }
                    if (intersectionCount % 2 == 1) {
                        d *= -1f;
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
        // Ref: https://iquilezles.org/www/articles/triangledistance/triangledistance.htm
        var v12 = v2 - v1;
        var v23 = v3 - v2;
        var v31 = v1 - v3;
        var n = Vector3.Cross (v12, v31);
        if (n.sqrMagnitude < float.Epsilon) {
            return float.MaxValue;
        }

        var p1 = p - v1;
        var p2 = p - v2;
        var p3 = p - v3;
        var outside =
            Mathf.Sign (Vector3.Dot (Vector3.Cross (v12, n), p1)) +
            Mathf.Sign (Vector3.Dot (Vector3.Cross (v23, n), p2)) +
            Mathf.Sign (Vector3.Dot (Vector3.Cross (v31, n), p3)) < 2.0f;

        if (outside) {
            var d1 = (v12 * Mathf.Clamp01 (Vector3.Dot (v12, p1) / v12.sqrMagnitude) - p1).magnitude;
            var d2 = (v23 * Mathf.Clamp01 (Vector3.Dot (v23, p2) / v23.sqrMagnitude) - p2).magnitude;
            var d3 = (v31 * Mathf.Clamp01 (Vector3.Dot (v31, p3) / v12.sqrMagnitude) - p3).magnitude;
            return Mathf.Min (d1, d2, d3);
        } else {
            return Mathf.Abs (Vector3.Dot (n, p1) / n.magnitude);
        }
    }

    static bool RaycastToTriangle (Vector3 origin, Vector3 direction, Vector3 v1, Vector3 v2, Vector3 v3) {
        // Ref: https://iquilezles.org/www/articles/intersectors/intersectors.htm
        // Ref: https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/moller-trumbore-ray-triangle-intersection
        var v12 = v2 - v1;
        var v13 = v3 - v1;
        var pVec = Vector3.Cross (direction, v13);
        var det = Vector3.Dot (pVec, v12);
        if (Mathf.Abs (det) < float.Epsilon) { return false; }

        var invDet = 1f / det;
        var tVec = origin - v1;
        var u = invDet * Vector3.Dot (pVec, tVec);
        if (u < 0 || u > 1) { return false; }

        var qVec = Vector3.Cross (tVec, v12);
        var v = invDet * Vector3.Dot (qVec, direction);
        if (v < 0 || u + v > 1) { return false; }

        var t = invDet * Vector3.Dot (qVec, v13);
        if (t < 0f) { return false; }

        return true;
    }

    // ****** //
    // Gizmos //
    // ****** //
    void OnDrawGizmos () {
        var meshFiler = GetComponent<MeshFilter> ();
        var bounds = meshFiler.sharedMesh.bounds;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color (1f, 1f, 1f, .3f);
        Gizmos.DrawWireCube (bounds.center, bounds.size);

        bounds.size = bounds.size + Vector3.one * boundsPadding;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube (bounds.center, bounds.size);

        if (enable && viewerMaterial != null) {
            var scaledSize = new Vector3 (transform.localScale.x * bounds.size.x, transform.localScale.y * bounds.size.y, transform.localScale.z * bounds.size.z);
            var pos = transform.position + transform.forward * scaledSize.z * (slice - 0.5f);
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
