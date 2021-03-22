﻿using SpiceEngineCore.Geometry;
using System;

namespace SpiceEngineCore.Rendering.Textures
{
    public interface ITexture
    {
        int Handle { get; }
        //TextureTarget Target { get; set; }

        bool EnableMipMap { get; set; }
        bool EnableAnisotropy { get; set; }
        //public bool Bindless { get; set; }

        //PixelInternalFormat PixelInternalFormat { get; set; }
        //PixelFormat PixelFormat { get; set; }
        //PixelType PixelType { get; set; }

        int Width { get; }
        int Height { get; }
        int Depth { get; }

        //TextureMinFilter MinFilter { get; set; }
        //TextureMagFilter MagFilter { get; set; }
        //TextureWrapMode WrapMode { get; set; }
        Color4 BorderColor { get; set; }

        void Bind();
        void BindImageTexture(int index);
        void Unbind();
        void GenerateMipMap();
        void Resize(int width, int height, int depth);
        Color4 ReadPixelColor(int x, int y);

        void ReserveMemory();
        //void Load(byte[] pixels);
        void Load(IntPtr pixels);
        void Unload();
    }
}
