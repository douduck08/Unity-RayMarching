using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDFUtils : MonoBehaviour {

    static Mesh _volumeCubeMesh;

    public static Mesh volumeCubeMesh {
        get {
            if (_volumeCubeMesh == null) {
                CreateCubeMesh ();
            }
            return _volumeCubeMesh;
        }
    }

    static void CreateCubeMesh () {
        GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
        _volumeCubeMesh = cube.GetComponent<MeshFilter> ().sharedMesh;
        DestroyImmediate (cube);
    }
}