using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Camera))]
public class SDFCamera : MonoBehaviour {

    RenderTexture result;
    ComputeShader renderCs;
    int renderKernal, groupX, groupY;

    new Camera camera;
    Vector3[] frustumCorners = new Vector3[4];
    Matrix4x4 frustumCornersMatrix;

    public Texture3D testVolume;
    public Transform testAnchor;

    void Start () {
        camera = GetComponent<Camera> ();
        groupX = Mathf.CeilToInt (camera.pixelWidth / 8);
        groupY = Mathf.CeilToInt (camera.pixelHeight / 8);

        result = new RenderTexture (camera.pixelWidth, camera.pixelHeight, 0);
        result.enableRandomWrite = true;
        result.wrapMode = TextureWrapMode.Clamp;
        result.Create ();

        renderCs = (ComputeShader) Resources.Load ("SDFRendering");
        renderKernal = renderCs.FindKernel ("CSMain");
        renderCs.SetTexture (renderKernal, "Result", result);

        renderCs.SetTexture (renderKernal, "_VolumeTex", testVolume);
    }

    void OnRenderImage (RenderTexture src, RenderTexture dest) {
        UpdateParameter ();
        renderCs.Dispatch (renderKernal, groupX, groupY, 1);
        Graphics.Blit (result, dest);
    }

    void UpdateParameter () {
        camera.CalculateFrustumCorners (new Rect (0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
        frustumCornersMatrix.SetRow (0, frustumCorners[0]);
        frustumCornersMatrix.SetRow (1, frustumCorners[1]);
        frustumCornersMatrix.SetRow (2, frustumCorners[2]);
        frustumCornersMatrix.SetRow (3, frustumCorners[3]);

        renderCs.SetMatrix ("_FrustumCorners", frustumCornersMatrix);
        renderCs.SetMatrix ("_VolumeInvTransform", testAnchor.worldToLocalMatrix);
    }
}