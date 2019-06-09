using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayMarchingVolume : MonoBehaviour {

    [SerializeField] SDFAsset sdfAsset;
    Matrix4x4 invTransform;

    public Texture3D volume {
        get {
            if (sdfAsset == null) {
                return null;
            }
            return sdfAsset.texture;
        }
    }

    public Matrix4x4 inverseTransform {
        get {
            return invTransform;
        }
    }

    void OnEnable () {
        invTransform = this.transform.worldToLocalMatrix;
        RayMarchingManager.instance.RegisterRayMarchingVolume (this);
    }

    void OnDisable () {
        RayMarchingManager.instance.UnrgisterRayMarchingVolume (this);
    }

}