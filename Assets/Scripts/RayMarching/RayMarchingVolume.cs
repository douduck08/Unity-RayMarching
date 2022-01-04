using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayMarchingVolume : MonoBehaviour {
    public Texture3D sdfTexture;
    public Vector3 volumeSize = Vector3.one;
    [ColorUsage (false)] public Color color;

    public Matrix4x4 matrix { get => Matrix4x4.TRS (transform.localPosition, transform.localRotation, Vector3.one); }
    public Matrix4x4 inverseMatrix { get => Matrix4x4.TRS (transform.localPosition, transform.localRotation, Vector3.one).inverse; }
    public Vector3 scale { get => new Vector3 (transform.localScale.x * volumeSize.x, transform.localScale.y * volumeSize.y, transform.localScale.z * volumeSize.z); }

    void OnDrawGizmos () {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube (Vector3.zero, volumeSize);
    }
}