using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayMarchingVolume : MonoBehaviour {
    public Texture3D sdfVolume;
    public Matrix4x4 inverseMatrix { get => Matrix4x4.TRS (transform.localPosition, transform.localRotation, Vector3.one).inverse; }
    public Vector3 scale { get => transform.localScale; }

    void OnDrawGizmos () {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube (Vector3.zero, Vector3.one);
    }
}