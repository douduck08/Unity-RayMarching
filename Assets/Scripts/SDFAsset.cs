using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDFAsset : ScriptableObject {

    [SerializeField, HideInInspector] Texture3D _texture;

    public Texture3D texture {
        get { return _texture; }
    }

    public void Initialize (SDFSampler sampler) {
        int dim;
        float scale;
        var colors = sampler.GenerateBitmap (out dim, out scale);

        _texture = new Texture3D (dim, dim, dim, TextureFormat.RGBAHalf, true);
        _texture.name = "Distance Field Texture";
        _texture.filterMode = FilterMode.Bilinear;
        _texture.wrapMode = TextureWrapMode.Clamp;
        _texture.SetPixels (colors);
        _texture.Apply ();
    }
}