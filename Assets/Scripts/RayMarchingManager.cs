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

    public const int MAX_VOLUME_TEXTURE_NUMBER = 4;
    public const int MAX_VOLUME_INSTANCE_NUMBER = 100;

    bool isDirty = true;
    List<RayMarchingVolume> volumes = new List<RayMarchingVolume> ();
    List<Texture3D> sdfVolumeList;

    public Texture3D emptyVolume;
    public int[] volumeIndexArray;
    public Matrix4x4[] inverseMatrixArray;

    RayMarchingManager () {
        emptyVolume = new Texture3D (1, 1, 1, TextureFormat.RGBAHalf, true);
        emptyVolume.name = "Empty Volume";
        emptyVolume.filterMode = FilterMode.Bilinear;
        emptyVolume.wrapMode = TextureWrapMode.Clamp;
        emptyVolume.SetPixels (new [] { new Color (0, 0, 0, 1) });
        emptyVolume.Apply ();
    }

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

        sdfVolumeList = new List<Texture3D> ();
        foreach (var volume in volumes) {
            if (sdfVolumeList.Count < MAX_VOLUME_TEXTURE_NUMBER && !sdfVolumeList.Contains (volume.volume)) {
                sdfVolumeList.Add (volume.volume);
            }
        }
        // Debug.Log ("Enabled volume texture number: " + sdfVolumeList.Count);

        var indexList = new List<int> ();
        var matrixList = new List<Matrix4x4> ();
        foreach (var volume in volumes) {
            var index = sdfVolumeList.FindIndex (p => p == volume.volume);
            if (index >= 0) {
                indexList.Add (index);
                matrixList.Add (volume.inverseMatrix);
            }
        }

        volumeIndexArray = indexList.ToArray ();
        inverseMatrixArray = matrixList.ToArray ();
        return true;
    }

    public Texture3D GetSdfVolumeTexture (int index) {
        if (sdfVolumeList == null || index >= sdfVolumeList.Count) return null;
        return sdfVolumeList[index];
    }
}