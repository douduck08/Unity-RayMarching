using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Camera))]
public class RayMarchingCamera : MonoBehaviour {

    [System.Serializable]
    class SceneParameters {
        [ColorUsage (false)] public Color skyColor;
        [ColorUsage (false)] public Color ambientColor;
        public float groundLevel;
        [ColorUsage (false)] public Color groundColor;
        public Light light;
    }

    class CameraParameters {
        public Camera camera;
        public Vector3[] frustumCorners = new Vector3[4];
        public Matrix4x4 frustumCornersMatrix;
    }

    struct InstanceData {
        public Matrix4x4 transform;
        public Matrix4x4 inverseTransform;
        public Vector3 scale;
        public uint index;
        public Color color;
    }

    public enum DebugMode {
        Off = 0,
        Distance = 1,
        Normal = 2
    }

    const int MAX_SDF_TEXTURE_NUMBER = 8;
    const int MAX_INSTANCE_NUMBER = 64;
    const int INSTANCE_DATA_SIZE = sizeof (float) * 16 + sizeof (float) * 16 + sizeof (float) * 3 + sizeof (uint) + sizeof (float) * 4;

    [SerializeField] List<RayMarchingVolume> volumes = new List<RayMarchingVolume> ();
    [SerializeField] SceneParameters sceneParams = new SceneParameters ();
    [SerializeField] DebugMode debugMode = DebugMode.Off;

    CameraParameters cameraParams = new CameraParameters ();
    RenderTexture result;
    ComputeBuffer instanceDataBuffer;

    [Header ("Resources")]
    [SerializeField, HideInInspector] ComputeShader renderingCS;

    void OnRenderImage (RenderTexture src, RenderTexture dest) {
        var descriptor = src.descriptor;
        descriptor.useMipMap = false;
        descriptor.enableRandomWrite = true;
        var result = RenderTexture.GetTemporary (descriptor);
        result.Create ();

        var kernal = renderingCS.FindKernel ("CSMain");
        if (kernal == -1) {
            return;
        }

        if (instanceDataBuffer == null) {
            instanceDataBuffer = new ComputeBuffer (MAX_INSTANCE_NUMBER, INSTANCE_DATA_SIZE, ComputeBufferType.Default);
        }

        kernal += (int)debugMode;
        renderingCS.SetTexture (kernal, "_Result", result);
        UpdateCameraParameters (renderingCS);
        UpdateSceneParameters (renderingCS);
        UpdateInstanceData (renderingCS, kernal);

        const int THREAD_GROUP_SIZE = 8;
        renderingCS.Dispatch (kernal, Mathf.CeilToInt (descriptor.width / THREAD_GROUP_SIZE), Mathf.CeilToInt (descriptor.height / THREAD_GROUP_SIZE), 1);
        Graphics.Blit (result, dest);

        RenderTexture.ReleaseTemporary (result);
    }

    void OnDestroy () {
        if (instanceDataBuffer != null) {
            instanceDataBuffer.Release ();
            instanceDataBuffer = null;
        }
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
        renderingCS.SetVector ("_SkyColor", sceneParams.skyColor);
        renderingCS.SetVector ("_AmbientColor", sceneParams.ambientColor);
        renderingCS.SetFloat ("_GroundLevel", sceneParams.groundLevel);
        renderingCS.SetVector ("_GroundColor", sceneParams.groundColor);
        renderingCS.SetVector ("_LightColor", sceneParams.light.color * sceneParams.light.intensity);
        if (sceneParams.light.type == LightType.Directional) {
            Vector4 light = -sceneParams.light.transform.forward;
            light.w = 0;
            renderingCS.SetVector ("_LightPosition", light);
        } else {
            Vector4 light = sceneParams.light.transform.position;
            light.w = 1;
            renderingCS.SetVector ("_LightPosition", light);
        }
    }

    void UpdateInstanceData (ComputeShader renderingCS, int kernal) {
        if (volumes.Count > 0) {
            var instanceCount = Mathf.Min (volumes.Count, MAX_INSTANCE_NUMBER);
            List<Texture3D> volumeTextures = new List<Texture3D> ();
            InstanceData[] instanceDatas = new InstanceData[instanceCount];
            for (int i = 0; i < instanceCount; i++) {
                var index = volumeTextures.FindIndex (t => t == volumes[i].sdfTexture);
                if (index == -1) {
                    index = volumeTextures.Count;
                    volumeTextures.Add (volumes[i].sdfTexture);
                }
                instanceDatas[i].transform = volumes[i].matrix;
                instanceDatas[i].inverseTransform = volumes[i].inverseMatrix;
                instanceDatas[i].scale = volumes[i].scale;
                instanceDatas[i].index = (uint)index;
                instanceDatas[i].color = volumes[i].color;
            }

            instanceDataBuffer.SetData (instanceDatas);
            renderingCS.SetBuffer (kernal, "_InstanceData", instanceDataBuffer);
            renderingCS.SetInt ("_InstanceCount", instanceCount);
            for (int i = 0; i < MAX_SDF_TEXTURE_NUMBER; i++) {
                if (i < volumeTextures.Count) {
                    renderingCS.SetTexture (kernal, "_Volume" + i, volumeTextures[i]);
                } else {
                    renderingCS.SetTexture (kernal, "_Volume" + i, volumeTextures[volumeTextures.Count - 1]);
                }
            }
        }
    }
}