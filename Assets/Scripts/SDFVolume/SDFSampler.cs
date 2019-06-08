using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDFSampler : MonoBehaviour {

    [SerializeField] int resolution = 50;
    [SerializeField] float scale = 1f;
    [SerializeField, Range (1, 10)] int iteration = 10;
    [SerializeField] bool useGpu = true;

    const int groupSize = 4;
    ComputeShader bakerCs;

    void OnDrawGizmos () {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube (Vector3.zero, Vector3.one * scale);
    }

    public Color[] GenerateBitmap (out int sdfResolution, out float sdfScale) {
        var distance = new float[resolution * resolution * resolution];
        if (useGpu) {
            if (bakerCs == null) {
                bakerCs = (ComputeShader) Resources.Load ("SDFBaker");
            }

            var meshFilter = GetComponent<MeshFilter> ();
            var mesh = meshFilter.sharedMesh;
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;

            var kernal = bakerCs.FindKernel ("SDFBaking");
            var verticesBuffer = new ComputeBuffer (vertices.Length, 12);
            var trianglesBuffer = new ComputeBuffer (triangles.Length, 4);
            var distancesBuffer = new ComputeBuffer (distance.Length, 4);

            verticesBuffer.SetData (vertices);
            trianglesBuffer.SetData (triangles);

            bakerCs.SetBuffer (kernal, "vertices", verticesBuffer);
            bakerCs.SetBuffer (kernal, "triangles", trianglesBuffer);
            bakerCs.SetBuffer (kernal, "distances", distancesBuffer);
            bakerCs.SetInt ("triangleCount", triangles.Length / 3);
            bakerCs.SetInt ("resolution", resolution);
            bakerCs.SetFloat ("scale", scale);

            var group = Mathf.CeilToInt (1f * resolution / groupSize);
            bakerCs.Dispatch (kernal, group, group, group);
            distancesBuffer.GetData (distance);

            verticesBuffer.Release ();
            trianglesBuffer.Release ();
            distancesBuffer.Release ();
        } else {
            for (var xi = 0; xi < resolution; xi++) {
                for (var yi = 0; yi < resolution; yi++) {
                    for (var zi = 0; zi < resolution; zi++) {
                        var x = 1f * xi / (resolution - 1) - 0.5f;
                        var y = 1f * yi / (resolution - 1) - 0.5f;
                        var z = 1f * zi / (resolution - 1) - 0.5f;

                        var pos = new Vector3 (x, y, z) * scale;
                        pos = transform.TransformPoint (pos);

                        var dist = SearchDistance (pos) / scale;
                        distance[GetIndex (xi, yi, zi)] = dist;
                    }
                }
            }
        }

        var colors = new Color[distance.Length];
        var s = (resolution - 1) * 0.5f;
        for (var xi = 0; xi < resolution; xi++) {
            for (var yi = 0; yi < resolution; yi++) {
                for (var zi = 0; zi < resolution; zi++) {
                    var d = distance[GetIndex (xi, yi, zi)];
                    var dx = distance[GetIndex (xi + 1, yi, zi)] - distance[GetIndex (xi - 1, yi, zi)];
                    var dy = distance[GetIndex (xi, yi + 1, zi)] - distance[GetIndex (xi, yi - 1, zi)];
                    var dz = distance[GetIndex (xi, yi, zi + 1)] - distance[GetIndex (xi, yi, zi - 1)];
                    colors[GetIndex (xi, yi, zi)] = new Color (
                        dx * s + 0.5f,
                        dy * s + 0.5f,
                        dz * s + 0.5f,
                        d * 0.5f + 0.5f
                    );
                }
            }
        }

        sdfResolution = resolution;
        sdfScale = scale;
        return colors;
    }

    int GetIndex (int xi, int yi, int zi) {
        xi = Mathf.Clamp (xi, 0, resolution - 1);
        yi = Mathf.Clamp (yi, 0, resolution - 1);
        zi = Mathf.Clamp (zi, 0, resolution - 1);
        return xi + resolution * (yi + resolution * zi);
    }

    float SearchDistance (Vector3 pos) {
        var r = scale * 0.5f;
        var step = r * 0.5f;
        for (var i = 0; i < iteration; i++) {
            r += (Physics.CheckSphere (pos, r) ? -1 : 1) * step;
            step *= 0.5f;
        }
        return r;
    }
}