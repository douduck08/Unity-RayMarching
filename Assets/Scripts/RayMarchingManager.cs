using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayMarchingManager {

    static RayMarchingManager _instance;

    public static RayMarchingManager instance {
        get {
            if (_instance == null) {
                _instance = new RayMarchingManager ();
            }
            return _instance;
        }
    }

    RayMarchingManager () { }

    bool isDirty = true;
    List<RayMarchingVolume> volumes = new List<RayMarchingVolume> ();

    public Texture3D[] volumeArray;
    public Matrix4x4[] inverseTransformArray;

    public void RegisterRayMarchingVolume (RayMarchingVolume volume) {
        volumes.Add (volume);
        isDirty = true;
    }

    public void UnrgisterRayMarchingVolume (RayMarchingVolume volume) {
        volumes.Remove (volume);
        isDirty = true;
    }

    public bool RebuildDataArrayIfNeeded () {
        if (!isDirty) return false;

        // TODO: update data array
        return true;
    }
}