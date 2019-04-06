using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Camera))]
public class SDFRenderer : MonoBehaviour {

    [SerializeField] Material sdfMaterial;

    new Camera camera;
    // Material material;
    Vector3[] frustumCorners = new Vector3[4];
    Matrix4x4 frustumCornersMatrix;

    void Start () {
        camera = GetComponent<Camera> ();
        // material = new Material (sdfShader);
    }

    void OnRenderImage (RenderTexture src, RenderTexture dest) {
        camera.CalculateFrustumCorners (new Rect (0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
        frustumCornersMatrix.SetRow (0, frustumCorners[0]);
        frustumCornersMatrix.SetRow (1, frustumCorners[1]);
        frustumCornersMatrix.SetRow (2, frustumCorners[2]);
        frustumCornersMatrix.SetRow (3, frustumCorners[3]);
        Shader.SetGlobalMatrix ("_FrustumCorners", frustumCornersMatrix);
        Graphics.Blit (null, dest, sdfMaterial);
    }

}