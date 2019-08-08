﻿using OpenTK;
using SpiceEngine.Rendering.Meshes;
using System;

namespace SpiceEngine.Rendering.Textures
{
    public class TextureID : IRenderable
    {
        private float _alpha = 1.0f;

        public TextureID(string filePath) => FilePath = filePath;
        //public TextureID(int id) => ID = id;

        public string FilePath { get; private set; }
        public int ID { get; set; }
        public Vector3 Position { get; set; }

        public float Alpha
        {
            get => _alpha;
            set
            {
                if (_alpha != value)
                {
                    var oldValue = _alpha;
                    _alpha = value;
                    AlphaChanged?.Invoke(this, new AlphaEventArgs(oldValue, value));
                }
            }
        }

        public bool IsAnimated => false;
        public bool IsTransparent => Alpha < 1.0f;

        public event EventHandler<AlphaEventArgs> AlphaChanged;

        public TextureID Duplicate()
        {
            return new TextureID(FilePath)
            {
                ID = ID,
                Alpha = Alpha
            };
        }
    }
}
