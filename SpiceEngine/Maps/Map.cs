﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using SpiceEngine.Entities.Lights;
using SpiceEngine.Physics.Collision;

namespace SpiceEngine.Maps
{
    /// <summary>
    /// A map should consist of a collection of static brushes, actors, a camera, and/or a player (cutscenes and menu's won't have a player)
    /// 
    /// </summary>
    public abstract class Map
    {
        public MapCamera Camera { get; set; }
        public List<MapActor> Actors { get; set; } = new List<MapActor>();
        public List<MapBrush> Brushes { get; set; } = new List<MapBrush>();
        public List<MapVolume> Volumes { get; set; } = new List<MapVolume>();
        public List<Light> Lights { get; set; } = new List<Light>();
        public List<string> SkyboxTextureFilePaths { get; set; } = new List<string>();

        protected abstract void CalculateBounds();

        public void Save(string path)
        {
            using (var writer = XmlWriter.Create(path))
            {
                var serializer = new NetDataContractSerializer();
                serializer.WriteObject(writer, this);
            }
        }

        public static Map Load(string path)
        {
            using (var reader = XmlReader.Create(path))
            {
                var serializer = new NetDataContractSerializer();
                var map = serializer.ReadObject(reader, true) as Map;

                map.CalculateBounds();
                return map;
            }
        }
    }
}