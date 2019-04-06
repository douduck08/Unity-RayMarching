using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent (typeof (MeshFilter))]
public class SDFBaker : MonoBehaviour {

    public int resolution = 64;
    public bool sphereTesting;

    float DF_Sphere (Vector3 pos, out Vector3 normal) {
        float dis = Vector3.Magnitude (pos);
        normal = pos.normalized;
        return dis - 0.49f;
    }

    Vector3 MappingToMeshPos (Bounds bounds, Vector3 pos) {
        pos.x *= bounds.size.x;
        pos.y *= bounds.size.y;
        pos.z *= bounds.size.z;
        pos = pos + bounds.center;
        return pos;
    }

    float DotSelf (Vector3 v) {
        return Vector3.Dot (v, v);
    }

    float DF_Triangle (Vector3 pos, Vector3 a, Vector3 b, Vector3 c) {
        Vector3 ab = b - a, bc = c - b, ca = a - c;
        Vector3 ap = pos - a, bp = pos - b, cp = pos - c;
        Vector3 normal = Vector3.Cross (ab, ca);

        float value;
        if (Mathf.Sign (Vector3.Dot (Vector3.Cross (ab, normal), ap)) +
            Mathf.Sign (Vector3.Dot (Vector3.Cross (bc, normal), bp)) +
            Mathf.Sign (Vector3.Dot (Vector3.Cross (ca, normal), cp)) < 2f) {
            value = Mathf.Min (
                Mathf.Min (
                    DotSelf (ab * Mathf.Clamp01 (Vector3.Dot (ab, ap) / Vector3.Dot (ab, ab)) - ap),
                    DotSelf (bc * Mathf.Clamp01 (Vector3.Dot (bc, bp) / Vector3.Dot (bc, bc)) - bp)),
                DotSelf (ca * Mathf.Clamp01 (Vector3.Dot (ca, cp) / Vector3.Dot (ca, ca)) - cp)
            );
        } else {
            value = Vector3.Dot (normal, ap) * Vector3.Dot (normal, ap) / Vector3.Dot (normal, normal);
        }

        return Mathf.Sqrt (value);
    }

    bool RayTriangleIntersect (Vector3 origin, Vector3 dir, Vector3 p1, Vector3 p2, Vector3 p3) {
        Vector3 e1 = p2 - p1, e2 = p3 - p1;
        Vector3 h = Vector3.Cross (dir, e2);
        var a = Vector3.Dot (e1, h);
        if (Mathf.Abs (a) < 0.0000001f) return false;

        var f = 1f / a;
        Vector3 s = origin - p1;
        var u = f * (Vector3.Dot (s, h));
        if (u < 0.0 || u > 1.0) return false;

        Vector3 q = Vector3.Cross (s, e1);
        var v = f * Vector3.Dot (dir, q);
        if (v < 0.0 || u + v > 1.0) return false;

        var t = f * Vector3.Dot (e2, q);
        if (t > 0.0000001f) {
            // outIntersectionPoint = origin + dir * t;
            return true;
        } else {
            return false;
        }
    }

    float GetMeshDistance (Vector3[] vertices, int[] triangles, Vector3 pos, out Vector3 normal) {
        var triangleCount = triangles.Length / 3;
        var dis = 1e10f;
        var intersectCount = 0;
        for (int i = 0; i < triangleCount; i++) {
            var p1 = vertices[triangles[i * 3]];
            var p2 = vertices[triangles[i * 3 + 1]];
            var p3 = vertices[triangles[i * 3 + 2]];
            dis = Mathf.Min (dis, DF_Triangle (pos, p1, p2, p3));
            intersectCount += RayTriangleIntersect (pos, Vector3.forward, p1, p2, p3) ? 1 : 0;
        }
        normal = Vector3.one;
        return dis * (intersectCount % 2 == 0 ? 1f : -1f);
    }

    void OnDrawGizmos () {

    }

#if UNITY_EDITOR
    [ContextMenu ("Bake sdf into Texture3D")]
    void Bake (MenuCommand command) {
        string path = EditorUtility.SaveFilePanelInProject ("Save sdf into Texture3D asset", "", "asset", "");
        if (string.IsNullOrEmpty (path)) return;

        Debug.Log ("Save sdf into Texture3D asset: " + path);

        var meshFilter = GetComponent<MeshFilter> ();
        var mesh = meshFilter.sharedMesh;
        var bounds = mesh.bounds;
        var vertices = mesh.vertices;
        var triangles = mesh.triangles;

        DateTime dt = DateTime.Now;
        var texture = new Texture3D (resolution, resolution, resolution, TextureFormat.RGBA32, false);
        var colors = new Color[resolution * resolution * resolution];
        float dis;
        Vector3 normal;
        for (int z = 0, index = 0; z < resolution; z++) {
            for (int y = 0; y < resolution; y++) {
                for (int x = 0; x < resolution; x++, index++) {
                    var pos = new Vector3 (
                        ((float) x + 0.5f) / resolution - 0.5f,
                        ((float) y + 0.5f) / resolution - 0.5f,
                        ((float) z + 0.5f) / resolution - 0.5f
                    );
                    if (sphereTesting) {
                        dis = DF_Sphere (pos, out normal) * 0.5f + 0.5f;
                    } else {
                        dis = GetMeshDistance (vertices, triangles, MappingToMeshPos (bounds, pos), out normal) * 0.5f + 0.5f;
                    }
                    normal = normal * 0.5f + new Vector3 (0.5f, 0.5f, 0.5f);
                    colors[index] = new Color (normal.x, normal.y, normal.z, dis);
                }
            }
        }
        texture.SetPixels (colors);
        texture.Apply ();
        TimeSpan ts = DateTime.Now - dt;
        Debug.Log ("Baking time: " + ts);

        // string uid = AssetDatabase.AssetPathToGUID (path);
        // if (!string.IsNullOrEmpty (uid)) {
        //     var oldAsset = AssetDatabase.LoadAssetAtPath (path, typeof (Texture3D));
        // }
        AssetDatabase.CreateAsset (texture, path);
    }
#endif
}