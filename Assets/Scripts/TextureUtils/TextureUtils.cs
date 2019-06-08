using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TextureUtils {

    static ComputeShader _sliceTex3D;
    static ComputeShader sliceTex3D {
        get {
            if (_sliceTex3D == null) {
                _sliceTex3D = (ComputeShader) Resources.Load ("SliceTex3D");
            }
            return _sliceTex3D;
        }
    }

    public static RenderTexture GetSliceOfRenderTexture3D (RenderTexture source, int layer, RenderTextureFormat format = RenderTextureFormat.ARGB32) {
        if (source.dimension != TextureDimension.Tex3D) {
            Debug.LogError ("Source should be Tex3D");
            return null;
        }

        int width = source.width, height = source.height;
        RenderTexture output = new RenderTexture (width, height, 0, format);
        output.dimension = TextureDimension.Tex2D;
        output.enableRandomWrite = true;
        output.wrapMode = TextureWrapMode.Clamp;
        output.Create ();

        sliceTex3D.SetTexture (0, "voxels", source);
        sliceTex3D.SetInt ("layer", layer);
        sliceTex3D.SetTexture (0, "result", output);
        sliceTex3D.Dispatch (0, width, height, 1);

        return output;
    }

    public static Texture2D ConvertToTexture2D (RenderTexture source, TextureFormat format = TextureFormat.ARGB32) {
        if (source.dimension != TextureDimension.Tex2D) {
            Debug.LogError ("Source should be Tex2D for converting to Texture2D");
            return null;
        }

        int width = source.width, height = source.height;
        Texture2D output = new Texture2D (width, height, format, false);
        output.wrapMode = TextureWrapMode.Clamp;

        RenderTexture.active = source;
        output.ReadPixels (new Rect (0, 0, width, height), 0, 0);
        output.Apply ();
        return output;
    }

    public static Texture3D ConvertToTexture3D (RenderTexture source, TextureFormat format = TextureFormat.ARGB32) {
        if (source.dimension != TextureDimension.Tex3D) {
            Debug.LogError ("Source should be Tex3D for converting to Texture3D");
            return null;
        }

        int width = source.width, height = source.height, depth = source.volumeDepth;
        Texture3D output = new Texture3D (width, height, depth, format, false);
        output.wrapMode = TextureWrapMode.Clamp;

        Color[] outputPixels = new Color[width * height * depth];
        for (int z = 0; z < depth; z++) {
            var slice = GetSliceOfRenderTexture3D (source, z /*, format */ ); // Bug: cannot pass format becouse Unity doesn't support all format on ReadPixels() and GetPixels()
            var tex2D = ConvertToTexture2D (slice /*, format */ ); // cannot pass format
            var layerPixels = tex2D.GetPixels ();
            layerPixels.CopyTo (outputPixels, z * width * height);
            slice.Release ();
        }

        output.SetPixels (outputPixels);
        output.Apply ();
        return output;
    }
}