using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Camera))]
public class RayMarchingCamera : MonoBehaviour {

    new Camera camera;
    Vector3[] frustumCorners = new Vector3[4];
    Matrix4x4 frustumCornersMatrix;

    RenderTexture result;
    ComputeShader renderCs;
    int renderKernal, groupX, groupY;

    ComputeBuffer volumeIndexBuffer;
    ComputeBuffer inverseMatrixBuffer;

    void OnEnable () {
        renderCs = (ComputeShader) Resources.Load ("RayMarching");
        renderKernal = renderCs.FindKernel ("CSMain");

        camera = GetComponent<Camera> ();
        result = new RenderTexture (camera.pixelWidth, camera.pixelHeight, 0);
        result.enableRandomWrite = true;
        result.wrapMode = TextureWrapMode.Clamp;
        result.Create ();
        renderCs.SetTexture (renderKernal, "result", result);

        uint x, y, z;
        renderCs.GetKernelThreadGroupSizes (renderKernal, out x, out y, out z);
        groupX = Mathf.CeilToInt (camera.pixelWidth / x);
        groupY = Mathf.CeilToInt (camera.pixelHeight / y);

        volumeIndexBuffer = new ComputeBuffer (RayMarchingManager.MAX_VOLUME_INSTANCE_NUMBER, 4);
        inverseMatrixBuffer = new ComputeBuffer (RayMarchingManager.MAX_VOLUME_INSTANCE_NUMBER, 4 * 16);

        renderCs.SetBuffer (renderKernal, "volumeIndexBuffer", volumeIndexBuffer);
        renderCs.SetBuffer (renderKernal, "inverseMatrixBuffer", inverseMatrixBuffer);
    }

    void OnDisable () {
        if (volumeIndexBuffer != null) volumeIndexBuffer.Release ();
        if (inverseMatrixBuffer != null) inverseMatrixBuffer.Release ();
    }

    void OnRenderImage (RenderTexture src, RenderTexture dest) {
        UpdateKernalParameters ();
        UpdateComputeBuffer ();

        renderCs.Dispatch (renderKernal, groupX, groupY, 1);
        Graphics.Blit (result, dest);
    }

    void UpdateKernalParameters () {
        camera.CalculateFrustumCorners (new Rect (0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
        frustumCornersMatrix.SetRow (0, transform.localToWorldMatrix.MultiplyVector (Vector3.Normalize (frustumCorners[0])));
        frustumCornersMatrix.SetRow (1, transform.localToWorldMatrix.MultiplyVector (Vector3.Normalize (frustumCorners[1])));
        frustumCornersMatrix.SetRow (2, transform.localToWorldMatrix.MultiplyVector (Vector3.Normalize (frustumCorners[2])));
        frustumCornersMatrix.SetRow (3, transform.localToWorldMatrix.MultiplyVector (Vector3.Normalize (frustumCorners[3])));
        renderCs.SetMatrix ("cameraFrustumCorners", frustumCornersMatrix);
    }

    void UpdateComputeBuffer () {
        if (RayMarchingManager.instance.RebuildDataArrayIfNeeded ()) {
            var volumeIndexArray = RayMarchingManager.instance.volumeIndexArray;
            var inverseMatrixArray = RayMarchingManager.instance.inverseMatrixArray;
            volumeIndexBuffer.SetData (volumeIndexArray);
            inverseMatrixBuffer.SetData (inverseMatrixArray);
            renderCs.SetInt ("volumeInstanceNumber", volumeIndexArray.Length);

            for (int i = 0; i < RayMarchingManager.MAX_VOLUME_TEXTURE_NUMBER; i++) {
                var texture = RayMarchingManager.instance.GetSdfVolumeTexture (i);
                if (texture != null) {
                    renderCs.SetTexture (renderKernal, "sdfVolumeTexture" + i.ToString (), texture);
                } else {
                    renderCs.SetTexture (renderKernal, "sdfVolumeTexture" + i.ToString (), RayMarchingManager.instance.emptyVolume);
                }
            }
        }
    }
}