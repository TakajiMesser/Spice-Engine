﻿using OpenTK.Graphics.OpenGL;
using SpiceEngineCore.Rendering.Textures;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace StarchUICore.Text
{
    public class FontManager : IFontProvider
    {
        public const int GLYPHS_PER_LINE = 16;
        public const int GLYPH_LINE_COUNT = 16;
        public const int GLYPH_WIDTH = 24;
        public const int GLYPH_HEIGHT = 32;
        public const int X_SPACING = 4;
        public const int Y_SPACING = 5;

        ITextureProvider _textureProvider;
        private Dictionary<string, Font> _fontByPath = new Dictionary<string, Font>();

        public FontManager(ITextureProvider textureProvider) => _textureProvider = textureProvider;

        public IFont AddFontFile(string filePath, int fontSize)
        {
            // TODO - Save bitmap to file, then check for its location on subsequent attempts to add this font file (i.e. cache after restart)
            var font = new Font(filePath, fontSize);
            var bitmap = font.ToBitmap();
            var texture = LoadFromBitmap(bitmap, false, false);

            var textureIndex = _textureProvider.AddTexture(texture);
            font.Texture = _textureProvider.RetrieveTexture(textureIndex);

            _fontByPath.Add(filePath, font);
            return font;
        }

        public IFont GetFont(string filePath) => _fontByPath[filePath];

        private Texture LoadFromBitmap(Bitmap bitmap, bool enableMipMap, bool enableAnisotrophy, TextureMinFilter minFilter = TextureMinFilter.Linear, TextureMagFilter magFilter = TextureMagFilter.Linear)
        {
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var texture = new Texture(bitmap.Width, bitmap.Height, 1)
            {
                Target = TextureTarget.Texture2D,
                MinFilter = minFilter,
                MagFilter = magFilter,
                WrapMode = TextureWrapMode.Repeat,
                EnableMipMap = enableMipMap,
                EnableAnisotropy = enableAnisotrophy
            };

            switch (data.PixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    texture.PixelInternalFormat = PixelInternalFormat.Rgb8;
                    texture.PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.ColorIndex;
                    texture.PixelType = PixelType.Bitmap;
                    break;
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    texture.PixelInternalFormat = PixelInternalFormat.Rgb8;
                    texture.PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Bgr;
                    texture.PixelType = PixelType.UnsignedByte;
                    break;
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    texture.PixelInternalFormat = PixelInternalFormat.Rgba;
                    texture.PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;
                    texture.PixelType = PixelType.UnsignedByte;
                    break;
            }

            texture.Bind();
            texture.Load(data.Scan0);

            bitmap.UnlockBits(data);
            bitmap.Dispose();

            return texture;
        }
    }
}
