using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayMarchingVolume : MonoBehaviour {

    [SerializeField] SDFAsset sdfAsset;

    public Texture3D volume {
        get {
            if (sdfAsset == null) {
                return null;
            }
            return sdfAsset.texture;
        }
    }

    public Matrix4x4 inverseMatrix { get; protected set; }

    void OnEnable () {
        inverseMatrix = this.transform.worldToLocalMatrix;
        RayMarchingManager.instance.RegisterRayMarchingVolume (this);
    }

    void OnDisable () {
        RayMarchingManager.instance.UnrgisterRayMarchingVolume (this);
    }
}