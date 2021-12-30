using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Camera))]
public class RayMarchingCamera : MonoBehaviour {

    public List<RayMarchingVolume> volumes = new List<RayMarchingVolume> ();

    class CameraParameters {
        public Camera camera;
        public Vector3[] frustumCorners = new Vector3[4];
        public Matrix4x4 frustumCornersMatrix;
    }

    class SceneParameters {
    }

    RenderTexture result;
    CameraParameters cameraParams = new CameraParameters ();
    SceneParameters sceneParams = new SceneParameters ();

    [Header ("Resources")]
    [SerializeField] ComputeShader renderingCS;

    void OnRenderImage (RenderTexture src, RenderTexture dest) {
        var descriptor = src.descriptor;
        descriptor.useMipMap = false;
        descriptor.enableRandomWrite = true;
        var result = RenderTexture.GetTemporary (descriptor);
        result.Create ();

        renderingCS.SetTexture (0, "_Result", result);
        UpdateCameraParameters (renderingCS);
        UpdateSceneParameters (renderingCS);

        const int THREAD_GROUP_SIZE = 8;
        renderingCS.Dispatch (0, Mathf.CeilToInt (descriptor.width / THREAD_GROUP_SIZE), Mathf.CeilToInt (descriptor.height / THREAD_GROUP_SIZE), 1);
        Graphics.Blit (result, dest);

        RenderTexture.ReleaseTemporary (result);
    }

    void UpdateCameraParameters (ComputeShader renderingCS) {
        if (cameraParams.camera == null) {
            cameraParams.camera = GetComponent<Camera> ();
        }

        var camera = cameraParams.camera;
        var cameraPosition = camera.transform.position;
        var nearClip = camera.nearClipPlane;
        var farClip = camera.farClipPlane;

        camera.CalculateFrustumCorners (new Rect (0, 0, 1, 1), farClip, Camera.MonoOrStereoscopicEye.Mono, cameraParams.frustumCorners);
        cameraParams.frustumCornersMatrix.SetRow (0, transform.localToWorldMatrix.MultiplyVector (Vector3.Normalize (cameraParams.frustumCorners[0])));
        cameraParams.frustumCornersMatrix.SetRow (1, transform.localToWorldMatrix.MultiplyVector (Vector3.Normalize (cameraParams.frustumCorners[1])));
        cameraParams.frustumCornersMatrix.SetRow (2, transform.localToWorldMatrix.MultiplyVector (Vector3.Normalize (cameraParams.frustumCorners[2])));
        cameraParams.frustumCornersMatrix.SetRow (3, transform.localToWorldMatrix.MultiplyVector (Vector3.Normalize (cameraParams.frustumCorners[3])));

        renderingCS.SetVector ("_CameraPosition", cameraPosition);
        renderingCS.SetVector ("_CameraParams", new Vector4 (nearClip, farClip));
        renderingCS.SetMatrix ("_FrustumCorners", cameraParams.frustumCornersMatrix);
    }

    void UpdateSceneParameters (ComputeShader renderingCS) {
        if (volumes.Count > 0) {
            // test
            renderingCS.SetMatrix ("_InverseTransform", volumes[0].inverseMatrix);
            renderingCS.SetVector ("_Scale", volumes[0].scale);
        }
    }
}