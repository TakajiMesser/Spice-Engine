﻿using OpenTK.Graphics;
using SpiceEngineCore.Rendering.Textures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SpiceEngine.Rendering.Textures
{
    public class TextureManager : ITextureProvider, IDisposable
    {
        private ConcurrentDictionary<string, int> _indexByPath = new ConcurrentDictionary<string, int>(); 
        private List<ITexture> _textures = new List<ITexture>();

        private object _textureLock = new object();

        public bool EnableMipMapping { get; set; } = true;
        public bool EnableAnisotropy { get; set; } = true;

        public IInvoker Invoker { get; set; }

        public TextureManager() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        /// <returns>Returns a lookup index for this texture</returns>
        public int AddTexture(ITexture texture)
        {
            lock (_textureLock)
            {
                var index = _textures.Count;
                _textures.Add(texture);
                return index;
            }
        }

        public int AddTexture(string texturePath)
        {
            return _indexByPath.GetOrAdd(texturePath, p =>
            {
                var index = 0;

                lock (_textureLock)
                {
                    index = _textures.Count;
                    _textures.Add(null);
                }

                // TODO - If Invoker is null, queue this action up
                Invoker?.Run(() =>
                {
                    var texture = Texture.LoadFromFile(texturePath, EnableMipMapping, EnableAnisotropy);
                    _textures[index] = texture;
                });

                return index;
            });

            /*if (_idByPath.ContainsKey(texturePath))
            {
                return _idByPath[texturePath];
            }
            else
            {
                var texture = Texture.LoadFromFile(texturePath, EnableMipMapping, EnableAnisotropy);
                if (texture != null)
                {
                    int id = AddTexture(texture);

                    _idByPath.Add(texturePath, id);
                    return id;
                }
                else
                {
                    return 0;
                }
            }*/
        }

        public void Clear()
        {
            // TODO - Probably need to unbind/unload textures here...
            _indexByPath.Clear();
            _textures.Clear();
        }

        public ITexture RetrieveTexture(int index) => (index >= 0 && index < _textures.Count) ? _textures[index] : null;

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue && GraphicsContext.CurrentContext != null && !GraphicsContext.CurrentContext.IsDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                //GL.DeleteTexture(_handle);
                disposedValue = true;
            }
        }

        ~TextureManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
