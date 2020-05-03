﻿using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.ObjectModel;

namespace Iviz.App
{
    public class Resource
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Module
        {
            Grid,
            TF,
            PointCloud,
            Image,
            Robot,
            Marker,
            InteractiveMarker,
            JointState,
            DepthImageProjector,
            LaserScan
        }

        public class Info
        {
            public int Id { get; }
            public GameObject GameObject { get; }

            public Info(GameObject resource)
            {
                GameObject = resource;
                Id = resource.GetInstanceID();
            }

            public string Name => GameObject.name;

            public override string ToString()
            {
                return GameObject.ToString();
            }
        }

        public class MaterialsType
        {
            public Material Lit { get; }
            public Material SimpleLit { get; }
            public Material TexturedLit { get; }
            public Material ImagePreview { get; }
            public Material PointCloud { get; }
            public Material MeshList { get; }

            public MaterialsType()
            {
                SimpleLit = Resources.Load<Material>("BaseMaterials/SimpleWhite");
                Lit = Resources.Load<Material>("BaseMaterials/White");
                TexturedLit = Resources.Load<Material>("BaseMaterials/Textured Lit");
                ImagePreview = Resources.Load<Material>("BaseMaterials/ImagePreview");
                PointCloud = Resources.Load<Material>("Displays/PointCloud Material");
                MeshList = Resources.Load<Material>("Displays/MeshList Material");
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ColormapId
        {
            lines, pink, copper, bone, gray, winter, autumn, summer, spring, cool, hot, hsv, jet, parula
        };


        public class ColormapsType
        {
            public ReadOnlyDictionary<ColormapId, Texture2D> Textures { get; }

            public ReadOnlyCollection<string> Names { get; }

            public ColormapsType()
            {
                string[] names = new string[]
                {
                    "lines", "pink", "copper", "bone", "gray", "winter", "autumn", "summer", "spring", "cool", "hot", "hsv", "jet", "parula"
                };
                Names = new ReadOnlyCollection<string>(names);

                Dictionary<ColormapId, Texture2D> textures = new Dictionary<ColormapId, Texture2D>();
                for (int i = 0; i < Names.Count; i++)
                {
                    textures[(ColormapId)i] = Resources.Load<Texture2D>("Colormaps/" + Names[i]);
                }
                Textures = new ReadOnlyDictionary<ColormapId, Texture2D>(textures);
            }
        }

        public class MarkersType
        {
            public Info Cube { get; }
            public Info Cylinder { get; }
            public Info Sphere { get; }
            public Info Text { get; }
            public Info LineStrip { get; }
            public Info LineConnector { get; }
            public Info NamedBoundary { get; }
            public Info Arrow { get; }
            public Info SphereSimple { get; }
            public Info MeshList { get; }
            public Info PointList { get; }
            public Info MeshMarker { get; }

            public ReadOnlyDictionary<string, Info> Generic { get; }

            public MarkersType()
            {
                Cube = new Info(Resources.Load<GameObject>("Markers/Cube"));
                Cylinder = new Info(Resources.Load<GameObject>("Markers/Cylinder"));
                Sphere = new Info(Resources.Load<GameObject>("Markers/Sphere"));
                Text = new Info(Resources.Load<GameObject>("Markers/Text"));
                LineStrip = new Info(Resources.Load<GameObject>("Markers/LineStrip"));
                LineConnector = new Info(Resources.Load<GameObject>("Markers/LineConnector"));
                NamedBoundary = new Info(Resources.Load<GameObject>("Markers/NamedBoundary"));
                Arrow = new Info(Resources.Load<GameObject>("Markers/Arrow"));
                SphereSimple = new Info(Resources.Load<GameObject>("Spheres/sphere-LOD1"));
                MeshList = new Info(Resources.Load<GameObject>("Markers/MeshList"));
                PointList = new Info(Resources.Load<GameObject>("Markers/PointList"));
                MeshMarker = new Info(Resources.Load<GameObject>("Markers/MeshMarker"));

                Generic = new ReadOnlyDictionary<string, Info>(
                    new Dictionary<string, Info>()
                    {
                        ["Cube"] = Cube,
                        ["Cylinder"] = Cylinder,
                        ["Sphere"] = Sphere,
                        ["RightHand"] = new Info(Resources.Load<GameObject>("Markers/RightHand")),
                    });
            }
        }

        public class DisplaysType
        {
            public Info TFFrame { get; }
            public Info PointCloud { get; }
            public Info Grid { get; }
            public Info TF { get; }
            public Info Image { get; }
            public Info Robot { get; }
            public Info MarkerObject { get; }
            public Info Marker { get; }
            public Info InteractiveMarkerControlObject { get; }
            public Info InteractiveMarkerObject { get; }
            public Info InteractiveMarker { get; }
            public Info JointState { get; }
            public Info DepthImageProjector { get; }
            public Info LaserScan { get; }

            public DisplaysType()
            {
                TFFrame = new Info(Resources.Load<GameObject>("Displays/TFFrame"));
                PointCloud = new Info(Resources.Load<GameObject>("Displays/PointCloud"));
                Grid = new Info(Resources.Load<GameObject>("Displays/Grid"));
                TF = new Info(Resources.Load<GameObject>("Displays/TF"));
                Image = new Info(Resources.Load<GameObject>("Displays/Image"));
                Robot = new Info(Resources.Load<GameObject>("Displays/Robot"));
                MarkerObject = new Info(Resources.Load<GameObject>("Displays/MarkerObject"));
                Marker = new Info(Resources.Load<GameObject>("Displays/Marker"));
                InteractiveMarkerControlObject = new Info(Resources.Load<GameObject>("Displays/InteractiveMarkerControlObject"));
                InteractiveMarkerObject = new Info(Resources.Load<GameObject>("Displays/InteractiveMarkerObject"));
                InteractiveMarker = new Info(Resources.Load<GameObject>("Displays/InteractiveMarker"));
                JointState = new Info(Resources.Load<GameObject>("Displays/JointState"));
                DepthImageProjector = new Info(Resources.Load<GameObject>("Displays/DepthImageProjector"));
                LaserScan = new Info(Resources.Load<GameObject>("Displays/LaserScan"));
            }
        }

        public class WidgetsType
        {
            public Info DisplayButton { get; }
            public Info TopicsButton { get; }

            public WidgetsType()
            {
                DisplayButton = new Info(Resources.Load<GameObject>("Widgets/Display Button"));
                TopicsButton = new Info(Resources.Load<GameObject>("Widgets/Topics Button"));
            }
        }

        public class RobotsType
        {
            public ReadOnlyCollection<string> Names { get; }

            readonly Dictionary<string, Info> Objects = new Dictionary<string, Info>();

            public RobotsType()
            {
                string[] names = new string[]
                {
                "edu.iviz.dummybot",
                "com.clearpath.husky",
                "com.robotis.turtlebot3.burger",
                "com.robotis.turtlebot3.waffle",
                "com.willowgarage.pr2",
                "edu.fraunhofer.iosb.bob",
                "edu.kit.h2t.armar6"
                };

                Names = new ReadOnlyCollection<string>(names);
            }

            public Info GetObject(string name)
            {
                // robots are huge so they are only loaded on demand
                if (!Objects.TryGetValue(name, out Info info))
                {
                    info = new Info(Resources.Load<GameObject>("Robots/" + name));
                    Objects.Add(name, info);
                }
                return info;
            }
        }

        public const int ClickableLayer = 8;

        static MaterialsType materials;
        public static MaterialsType Materials => materials ?? (materials = new MaterialsType());

        static ColormapsType colormaps;
        public static ColormapsType Colormaps => colormaps ?? (colormaps = new ColormapsType());

        static MarkersType markers;
        public static MarkersType Markers => markers ?? (markers = new MarkersType());

        static DisplaysType displays;
        public static DisplaysType Displays => displays ?? (displays = new DisplaysType());

        static WidgetsType widgets;
        public static WidgetsType Widgets => widgets ?? (widgets = new WidgetsType());

        static RobotsType robots;
        public static RobotsType Robots => robots ?? (robots = new RobotsType());
    }
}