using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent (typeof (MeshFilter))]
public class SDFRenderer : MonoBehaviour {

    ComputeShader bakerCs;
    TextureFormat texFormat = TextureFormat.RHalf;
    RenderTextureFormat rtFormat = RenderTextureFormat.RHalf;

    public int resolution = 64;
    public bool sphereTesting;

#if UNITY_EDITOR
    [ContextMenu ("Bake SDF into Texture3D (GPU)")]
    void BakeGPU () {
        string path = EditorUtility.SaveFilePanelInProject ("Save SDF into Texture3D asset", "", "asset", "");
        if (string.IsNullOrEmpty (path)) return;
        Debug.Log ("Save SDF into Texture3D asset: " + path);
        DateTime dt = DateTime.Now;

        var renderTexture = new RenderTexture (resolution, resolution, 0, rtFormat, RenderTextureReadWrite.Linear);
        renderTexture.enableRandomWrite = true;
        renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        renderTexture.volumeDepth = resolution;
        renderTexture.Create ();

        var meshFilter = GetComponent<MeshFilter> ();
        var mesh = meshFilter.sharedMesh;
        var bounds = mesh.bounds;
        var vertices = mesh.vertices;
        var triangles = mesh.triangles;

        if (bakerCs == null) {
            bakerCs = (ComputeShader) Resources.Load ("SDFBaker");
        }

        var kernal = bakerCs.FindKernel ("SDFBaking");
        var group = Mathf.CeilToInt (resolution / 1f);
        var padding = 2f / resolution;
        var verticesBuffer = new ComputeBuffer (vertices.Length, 12);
        verticesBuffer.SetData (vertices);
        var trianglesBuffer = new ComputeBuffer (triangles.Length, 4);
        trianglesBuffer.SetData (triangles);

        bakerCs.SetTexture (kernal, "result", renderTexture);
        bakerCs.SetBuffer (kernal, "vertices", verticesBuffer);
        bakerCs.SetBuffer (kernal, "triangles", trianglesBuffer);
        bakerCs.SetFloat ("resolution", resolution);
        bakerCs.SetInt ("triangleCount", triangles.Length / 3);
        bakerCs.SetVector ("boundsSize", new Vector4 (bounds.size.x + padding, bounds.size.y + padding, bounds.size.z + padding));
        bakerCs.SetVector ("boundsCenter", bounds.center);
        bakerCs.Dispatch (kernal, resolution, resolution, resolution);

        verticesBuffer.Release ();
        trianglesBuffer.Release ();

        var texture = TextureUtils.ConvertToTexture3D (renderTexture, texFormat);
        TimeSpan ts = DateTime.Now - dt;
        Debug.Log ("Baking time: " + ts);
        AssetDatabase.CreateAsset (texture, path);
    }

    [ContextMenu ("Test Baking Texture3D (GPU)")]
    void BakeTest () {
        string path = "Assets/test.asset";
        Debug.Log ("Save test Texture3D asset: " + path);
        DateTime dt = DateTime.Now;

        var renderTexture = new RenderTexture (resolution, resolution, 0, rtFormat, RenderTextureReadWrite.Linear);
        renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        renderTexture.volumeDepth = resolution;
        renderTexture.enableRandomWrite = true;
        renderTexture.wrapMode = TextureWrapMode.Clamp;
        renderTexture.Create ();

        var kernal = bakerCs.FindKernel ("TestBaking");
        bakerCs.SetTexture (kernal, "result", renderTexture);
        bakerCs.Dispatch (kernal, resolution, resolution, resolution);

        var texture = TextureUtils.ConvertToTexture3D (renderTexture, texFormat);
        TimeSpan ts = DateTime.Now - dt;
        Debug.Log ("Baking time: " + ts);
        AssetDatabase.CreateAsset (texture, path);
    }
#endif
}