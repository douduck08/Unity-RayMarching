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

    ComputeBuffer volumesBuffer;
    ComputeBuffer inverseTransformsBuffer;

    void OnEnable () {
        renderCs = (ComputeShader) Resources.Load ("RayMarching");
        renderKernal = renderCs.FindKernel ("CSMain");

        camera = GetComponent<Camera> ();
        result = new RenderTexture (camera.pixelWidth, camera.pixelHeight, 0);
        result.enableRandomWrite = true;
        result.wrapMode = TextureWrapMode.Clamp;
        result.Create ();
        renderCs.SetTexture (renderKernal, "Result", result);

        uint x, y, z;
        renderCs.GetKernelThreadGroupSizes (renderKernal, out x, out y, out z);
        groupX = Mathf.CeilToInt (camera.pixelWidth / x);
        groupY = Mathf.CeilToInt (camera.pixelHeight / y);

        // TODO: init ComputeBuffer
    }

    void OnDisable () {
        if (volumesBuffer != null) volumesBuffer.Release ();
        if (inverseTransformsBuffer != null) inverseTransformsBuffer.Release ();
    }

    void OnRenderImage (RenderTexture src, RenderTexture dest) {
        UpdateKernalParameters ();
        UpdateComputeBuffer ();

        renderCs.Dispatch (renderKernal, groupX, groupY, 1);
        Graphics.Blit (result, dest);
    }

    void UpdateKernalParameters () {
        camera.CalculateFrustumCorners (new Rect (0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
        frustumCornersMatrix.SetRow (0, frustumCorners[0]);
        frustumCornersMatrix.SetRow (1, frustumCorners[1]);
        frustumCornersMatrix.SetRow (2, frustumCorners[2]);
        frustumCornersMatrix.SetRow (3, frustumCorners[3]);
        renderCs.SetMatrix ("_FrustumCorners", frustumCornersMatrix);
    }

    void UpdateComputeBuffer () {
        if (RayMarchingManager.instance.RebuildDataArrayIfNeeded ()) {
            var volumeArray = RayMarchingManager.instance.volumeArray;
            var invertTransformArray = RayMarchingManager.instance.inverseTransformArray;

            // TODO: update ComputeBuffer
            volumesBuffer.SetData (volumeArray);
            inverseTransformsBuffer.SetData (invertTransformArray);
            renderCs.SetInt ("volumeNumber", volumeArray.Length);
        }
    }
}