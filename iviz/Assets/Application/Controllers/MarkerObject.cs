﻿using System;
using System.Collections.Generic;
using System.Text;
using Iviz.Core;
using Iviz.Displays;
using Iviz.Msgs.GeometryMsgs;
using Iviz.Msgs.StdMsgs;
using Iviz.Msgs.VisualizationMsgs;
using Iviz.Resources;
using Iviz.Ros;
using Iviz.Roslib;
using JetBrains.Annotations;
using UnityEngine;
using Logger = Iviz.Core.Logger;
using Vector3 = UnityEngine.Vector3;

namespace Iviz.Controllers
{
    internal enum MarkerType
    {
        Arrow = Marker.ARROW,
        Cube = Marker.CUBE,
        Sphere = Marker.SPHERE,
        Cylinder = Marker.CYLINDER,
        LineStrip = Marker.LINE_STRIP,
        LineList = Marker.LINE_LIST,
        CubeList = Marker.CUBE_LIST,
        SphereList = Marker.SPHERE_LIST,
        Points = Marker.POINTS,
        TextViewFacing = Marker.TEXT_VIEW_FACING,
        MeshResource = Marker.MESH_RESOURCE,
        TriangleList = Marker.TRIANGLE_LIST,

        /// <summary>
        ///     New: Text marker that does not face the camera
        /// </summary>
        Text = 100,

        /// <summary>
        ///     New: Image marker. Pixels in Colors field, image width in Scale.z
        /// </summary>
        Image = 101
    }

    public enum MouseEventType
    {
        Click,
        Down,
        Up
    }

    public sealed class MarkerObject : FrameNode
    {
        const string WarnStr = "<color=yellow>Warning:</color> ";
        const string ErrorStr = "<color=red>Error:</color> ";

        readonly StringBuilder description = new StringBuilder();

        string id;

        int numErrors;
        int numWarnings;

        [CanBeNull] MarkerResource resource;
        [CanBeNull] Info<GameObject> resourceInfo;

        MarkerLineHelper lineHelper;
        MarkerLineHelper LineHelper => lineHelper ?? (lineHelper = new MarkerLineHelper());

        public Bounds? Bounds => resource == null
            ? (Bounds?) null
            : UnityUtils.TransformBound(resource.Bounds, resource.transform);

        public DateTime ExpirationTime { get; private set; }

        public bool OcclusionOnly
        {
            get => resource is ISupportsAROcclusion r && r.OcclusionOnly;
            set
            {
                if (resource is ISupportsAROcclusion arResource) arResource.OcclusionOnly = value;
            }
        }

        public Color Tint
        {
            get => resource is ISupportsTint r ? r.Tint : Color.white;
            set
            {
                if (resource is ISupportsTint tintResource) tintResource.Tint = value;
            }
        }

        public bool Visible
        {
            get => resource == null || resource.Visible;
            set
            {
                if (resource != null) resource.Visible = value;
            }
        }

        public void Set([NotNull] Marker msg)
        {
            if (msg == null) throw new ArgumentNullException(nameof(msg));

            id = MarkerListener.IdFromMessage(msg);
            name = id;

            numWarnings = 0;
            numErrors = 0;

            description.Length = 0;
            description.Append("<color=maroon><b>* Marker: ").Append(id).Append("</b></color>").AppendLine();
            description.Append("Type: <b>").Append(DescriptionFromType(msg)).Append("</b>").AppendLine();

            ExpirationTime = msg.Lifetime.IsZero ? DateTime.MaxValue : DateTime.Now + msg.Lifetime.ToTimeSpan();

            if (msg.Lifetime.IsZero)
            {
                ExpirationTime = DateTime.MaxValue;
                description.Append("Expiration: None").AppendLine();
            }
            else
            {
                ExpirationTime = DateTime.Now + msg.Lifetime.ToTimeSpan();
                description.Append("Expiration: ").Append(msg.Lifetime.Secs).Append(" secs").AppendLine();
            }

            UpdateResource(msg);

            if (resource == null)
            {
                if (msg.Type() == MarkerType.MeshResource)
                {
                    description.Append(ErrorStr).Append("Mesh resource not found").AppendLine();
                    numErrors++;
                }

                return;
            }

            UpdateTransform(msg);

            switch (msg.Type())
            {
                case MarkerType.Arrow:
                    CreateArrow(msg);
                    break;
                case MarkerType.Cube:
                case MarkerType.Sphere:
                case MarkerType.Cylinder:
                case MarkerType.MeshResource:
                    CrateMeshResource(msg);
                    break;
                case MarkerType.TextViewFacing:
                case MarkerType.Text:
                {
                    CreateTextResource(msg);
                    break;
                }
                case MarkerType.CubeList:
                case MarkerType.SphereList:
                {
                    CreateMeshList(msg);
                    break;
                }
                case MarkerType.LineList:
                {
                    CreateLine(msg, false);
                    break;
                }
                case MarkerType.LineStrip:
                {
                    CreateLine(msg, true);
                    break;
                }
                case MarkerType.Points:
                {
                    CreatePoints(msg);
                    break;
                }
                case MarkerType.TriangleList:
                {
                    CreateTriangleList(msg);
                    break;
                }
                case MarkerType.Image:
                    CreateImage(msg);
                    break;
            }
        }

        [NotNull]
        T ValidateResource<T>() where T : MarkerResource
        {
            if (!(resource is T result)) throw new InvalidOperationException("Resource is not set!");

            return result;
        }

        void CreateImage([NotNull] Marker msg)
        {
            ImageResource image = ValidateResource<ImageResource>();
            int count = msg.Colors.Length;
            int width = (int) msg.Scale.Z;
            int height = width == 0 ? 0 : count / width;
            if (width <= 0 || height <= 0 || width * height != count)
            {
                Debug.LogWarning("MarkerObject: Invalid image dimensions");
                return;
            }

            bool hasAlpha = msg.Colors.Any(color => color.A < 1);
            byte[] data;
            int bpp, j = 0;
            if (hasAlpha)
            {
                bpp = 3;
                data = new byte[count * 3];
                foreach (ColorRGBA color in msg.Colors)
                {
                    Color32 color32 = new Color(color.R, color.G, color.B);
                    data[j++] = color32.r;
                    data[j++] = color32.g;
                    data[j++] = color32.b;
                }
            }
            else
            {
                bpp = 4;
                data = new byte[count * 4];
                foreach (ColorRGBA color in msg.Colors)
                {
                    Color32 color32 = new Color(color.R, color.G, color.B);
                    data[j++] = color32.r;
                    data[j++] = color32.g;
                    data[j++] = color32.b;
                    data[j++] = color32.a;
                }
            }

            image.Set(width, height, bpp, data.AsSegment(), true);

            Vector3 imageScale = new Msgs.GeometryMsgs.Vector3(msg.Scale.X, msg.Scale.Y, 1).Ros2Unity().Abs();
            transform.localScale = imageScale;
        }

        void CreateTriangleList([NotNull] Marker msg)
        {
            description.Append("Scale: [")
                .Append(msg.Scale.X).Append(" | ")
                .Append(msg.Scale.Y).Append(" | ")
                .Append(msg.Scale.Z).Append("]").AppendLine();
            description.Append("Color: ")
                .Append(msg.Color.R).Append(" | ")
                .Append(msg.Color.G).Append(" | ")
                .Append(msg.Color.B).Append(" | ")
                .Append(msg.Color.A).AppendLine();

            if (msg.Points.Length == 0)
                description.Append("Elements: Empty").AppendLine();
            else
                description.Append("Elements: ").Append(msg.Points.Length).AppendLine();

            MeshTrianglesResource meshTriangles = ValidateResource<MeshTrianglesResource>();
            if (msg.Colors.Length != 0 && msg.Colors.Length != msg.Points.Length)
            {
                description.Append(ErrorStr).Append("Color array length ").Append(msg.Colors.Length)
                    .Append(" does not match point array length ").Append(msg.Points.Length).AppendLine();
                meshTriangles.Set(Array.Empty<Vector3>());
                numErrors++;
                return;
            }

            if (Mathf.Approximately(msg.Color.A, 0))
            {
                description.Append(WarnStr).Append("Color has alpha 0. Marker will not be visible").AppendLine();
                meshTriangles.Set(Array.Empty<Vector3>());
                numWarnings++;
                return;
            }

            if (msg.Points.Length % 3 != 0)
            {
                description.Append(ErrorStr).Append("Point array length ").Append(msg.Colors.Length)
                    .Append(" needs to be a multiple of 3").AppendLine();
                meshTriangles.Set(Array.Empty<Vector3>());
                numErrors++;
                return;
            }


            meshTriangles.Color = msg.Color.Sanitize().ToUnityColor();
            var points = new Vector3[msg.Points.Length];
            for (int i = 0; i < points.Length; i++) points[i] = msg.Points[i].Ros2Unity();

            if (msg.Colors.Length != 0)
            {
                var colors = new Color[msg.Colors.Length];
                for (int i = 0; i < colors.Length; i++) colors[i] = msg.Colors[i].ToUnityColor();

                meshTriangles.Set(points, colors);
            }
            else
            {
                meshTriangles.Set(points);
            }

            transform.localScale = msg.Scale.Ros2Unity().Abs();
        }

        void CreatePoints([NotNull] Marker msg)
        {
            PointListResource pointList = ValidateResource<PointListResource>();
            pointList.ElementScale = Mathf.Abs((float) msg.Scale.X);

            description.Append("Scale: ").Append(msg.Scale.X).AppendLine();
            description.Append("Color: ")
                .Append(msg.Color.R).Append(" | ")
                .Append(msg.Color.G).Append(" | ")
                .Append(msg.Color.B).Append(" | ")
                .Append(msg.Color.A).AppendLine();


            if (msg.Colors.Length != 0 && msg.Colors.Length != msg.Points.Length)
            {
                description.Append(ErrorStr).Append("Color array length ").Append(msg.Colors.Length)
                    .Append(" does not match point array length ").Append(msg.Points.Length).AppendLine();
                pointList.PointsWithColor = Array.Empty<PointWithColor>();
                numErrors++;
                return;
            }

            if (Mathf.Approximately(msg.Color.A, 0) || msg.Color.A.IsInvalid())
            {
                description.Append(WarnStr).Append("Color has alpha 0 or NaN").AppendLine();
                pointList.PointsWithColor = Array.Empty<PointWithColor>();
                numWarnings++;
                return;
            }

            if (msg.Points.Length == 0)
                description.Append("Elements: Empty").AppendLine();
            else
                description.Append("Elements: ").Append(msg.Points.Length).AppendLine();

            IEnumerable<PointWithColor> points;
            Color32 color32 = msg.Color.Sanitize().ToUnityColor32();
            if (msg.Colors.Length == 0)
            {
                IEnumerable<PointWithColor> PointEnumerator()
                {
                    foreach (Point position in msg.Points)
                        yield return new PointWithColor(position.Ros2Unity(), color32);
                }

                points = PointEnumerator();
            }
            else if (color32 == Color.white)
            {
                IEnumerable<PointWithColor> PointEnumerator()
                {
                    for (int i = 0; i < msg.Points.Length; i++)
                        yield return new PointWithColor(
                            msg.Points[i].Ros2Unity(),
                            msg.Colors[i].ToUnityColor32());
                }

                points = PointEnumerator();
            }
            else
            {
                Color color = color32;

                IEnumerable<PointWithColor> PointEnumerator()
                {
                    for (int i = 0; i < msg.Points.Length; i++)
                        yield return new PointWithColor(
                            msg.Points[i].Ros2Unity(),
                            color * msg.Colors[i].ToUnityColor());
                }

                points = PointEnumerator();
            }

            pointList.Set(points, msg.Points.Length);
            pointList.UseColormap = false;
        }

        void CreateLine([NotNull] Marker msg, bool isStrip)
        {
            LineResource lineResource = ValidateResource<LineResource>();
            float elementScale = Mathf.Abs((float) msg.Scale.X);

            description.Append("Scale: ").Append(elementScale).AppendLine();
            description.Append("Color: ")
                .Append(msg.Color.R).Append(" | ")
                .Append(msg.Color.G).Append(" | ")
                .Append(msg.Color.B).Append(" | ")
                .Append(msg.Color.A).AppendLine();

            if (msg.Points.Length == 0)
                description.Append("Elements: Empty").AppendLine();
            else
                description.Append("Elements: ").Append(msg.Points.Length).AppendLine();

            if (Mathf.Approximately(elementScale, 0) || elementScale.IsInvalid())
            {
                description.Append(WarnStr).Append("Scale value of 0 or NaN").AppendLine();
                lineResource.LinesWithColor = Array.Empty<LineWithColor>();
                numWarnings++;
                return;
            }

            if (msg.Colors.Length != 0 && msg.Colors.Length != msg.Points.Length)
            {
                description.Append(ErrorStr)
                    .Append("Color array length ").Append(msg.Colors.Length)
                    .Append(" does not match point array length ").Append(msg.Points.Length)
                    .AppendLine();
                lineResource.LinesWithColor = Array.Empty<LineWithColor>();
                numErrors++;
                return;
            }

            if (Mathf.Approximately(msg.Color.A, 0) || msg.Color.A.IsInvalid())
            {
                description.Append(WarnStr).Append("Color has alpha 0 or NaN").AppendLine();
                lineResource.LinesWithColor = Array.Empty<LineWithColor>();
                numWarnings++;
                return;
            }

            lineResource.ElementScale = elementScale;
            LineResource.DirectLineSetter setterCallback =
                isStrip ? LineHelper.GetLineSetterForStrip(msg) : LineHelper.GetLineSetterForList(msg);
            lineResource.SetDirect(setterCallback);
        }

        void CreateMeshList([NotNull] Marker msg)
        {
            MeshListResource meshList = ValidateResource<MeshListResource>();
            meshList.UseColormap = false;
            meshList.UseIntensityForScaleY = false;
            meshList.MeshResource = msg.Type() == MarkerType.CubeList
                ? Resource.Displays.Cube
                : Resource.Displays.Sphere;

            description.Append("Color: ")
                .Append(msg.Color.R).Append(" | ")
                .Append(msg.Color.G).Append(" | ")
                .Append(msg.Color.B).Append(" | ")
                .Append(msg.Color.A).AppendLine();

            if (msg.Colors.Length != 0 && msg.Colors.Length != msg.Points.Length)
            {
                description.Append(ErrorStr).Append("Color array length ").Append(msg.Colors.Length)
                    .Append(" does not match point array length ").Append(msg.Points.Length).AppendLine();
                meshList.PointsWithColor = Array.Empty<PointWithColor>();
                numErrors++;
                return;
            }

            if (Mathf.Approximately(msg.Color.A, 0))
            {
                description.Append(WarnStr).Append("Color has alpha 0").AppendLine();
                meshList.PointsWithColor = Array.Empty<PointWithColor>();
                numWarnings++;
                return;
            }

            IEnumerable<PointWithColor> points;
            Color32 color32 = msg.Color.Sanitize().ToUnityColor32();
            if (msg.Colors.Length == 0)
            {
                IEnumerable<PointWithColor> PointEnumerator()
                {
                    foreach (Point position in msg.Points)
                        yield return new PointWithColor(position.Ros2Unity(), color32);
                }

                points = PointEnumerator();
            }
            else if (color32 == Color.white)
            {
                IEnumerable<PointWithColor> PointEnumerator()
                {
                    for (int i = 0; i < msg.Points.Length; i++)
                        yield return new PointWithColor(
                            msg.Points[i].Ros2Unity(),
                            msg.Colors[i].ToUnityColor32());
                }

                points = PointEnumerator();
            }
            else
            {
                Color color = color32;

                IEnumerable<PointWithColor> PointEnumerator()
                {
                    for (int i = 0; i < msg.Points.Length; i++)
                        yield return new PointWithColor(
                            msg.Points[i].Ros2Unity(),
                            color * msg.Colors[i].ToUnityColor());
                }

                points = PointEnumerator();
            }

            meshList.Set(points);
        }

        void CreateTextResource([NotNull] Marker msg)
        {
            TextMarkerResource textResource = ValidateResource<TextMarkerResource>();
            textResource.Text = msg.Text;
            textResource.Color = msg.Color.Sanitize().ToUnityColor();
            textResource.BillboardEnabled = msg.Type() != MarkerType.Text;
            textResource.ElementSize = (float) msg.Scale.Z;

            description.Append("Scale: ").Append(msg.Scale.Z).AppendLine();
            if (Mathf.Approximately((float) msg.Scale.Z, 0) || msg.Scale.Z.IsInvalid())
            {
                description.Append(WarnStr).Append("Scale value of 0 or NaN").AppendLine();
                numWarnings++;
            }

            description.Append("Color: ")
                .Append(msg.Color.R).Append(" | ")
                .Append(msg.Color.G).Append(" | ")
                .Append(msg.Color.B).Append(" | ")
                .Append(msg.Color.A).AppendLine();
        }

        void CrateMeshResource([NotNull] Marker msg)
        {
            if (resource is MeshMarkerResource meshMarker) meshMarker.Color = msg.Color.Sanitize().ToUnityColor();

            description.Append("Scale: [")
                .Append(msg.Scale.X).Append(" | ")
                .Append(msg.Scale.Y).Append(" | ")
                .Append(msg.Scale.Z).Append("]").AppendLine();

            if (Mathf.Approximately((float) msg.Scale.SquaredNorm, 0))
            {
                description.Append(WarnStr).Append("Scale value of 0").AppendLine();
                numWarnings++;
            }
            else if (msg.Scale.HasNaN())
            {
                description.Append(WarnStr).Append("Scale value has NaN").AppendLine();
                numWarnings++;
            }

            description.Append("Color: ")
                .Append(msg.Color.R).Append(" | ")
                .Append(msg.Color.G).Append(" | ")
                .Append(msg.Color.B).Append(" | ")
                .Append(msg.Color.A).AppendLine();

            transform.localScale = msg.Scale.Ros2Unity().Abs();
        }


        void CreateArrow([NotNull] Marker msg)
        {
            ArrowResource arrowMarker = ValidateResource<ArrowResource>();
            arrowMarker.Color = msg.Color.Sanitize().ToUnityColor();
            switch (msg.Points.Length)
            {
                case 0:
                {
                    if (Mathf.Approximately((float) msg.Scale.SquaredNorm, 0))
                    {
                        description.Append(WarnStr).Append("Scale value of 0").AppendLine();
                        arrowMarker.Visible = false;
                        numWarnings++;
                        return;
                    }

                    if (msg.Scale.HasNaN())
                    {
                        description.Append(WarnStr).Append("Scale value of NaN").AppendLine();
                        arrowMarker.Visible = false;
                        numWarnings++;
                        return;
                    }

                    arrowMarker.Visible = true;
                    arrowMarker.Set(msg.Scale.Ros2Unity().Abs());


                    description.Append("Scale: [")
                        .Append(msg.Scale.X).Append(" | ")
                        .Append(msg.Scale.Y).Append(" | ")
                        .Append(msg.Scale.Z).Append("]").AppendLine();
                    return;
                }
                case 2:
                {
                    float sx = Mathf.Abs((float) msg.Scale.X);
                    description.Append("Scale: ").Append(msg.Scale.X).AppendLine();
                    switch (sx)
                    {
                        case 0:
                            description.Append(WarnStr).Append("Scale value of 0").AppendLine();
                            numWarnings++;
                            arrowMarker.Visible = false;
                            return;
                        case float.NaN:
                        case float.NegativeInfinity:
                        case float.PositiveInfinity:
                            description.Append(WarnStr).Append("Scale value of NaN or infinite").AppendLine();
                            numWarnings++;
                            arrowMarker.Visible = false;
                            return;
                        default:
                            arrowMarker.Visible = true;
                            arrowMarker.Set(msg.Points[0].Ros2Unity(), msg.Points[1].Ros2Unity(), sx);
                            return;
                    }
                }
                default:
                    description.Append(ErrorStr).Append("Point array should have a length of 0 or 2").AppendLine();
                    numErrors++;
                    break;
            }
        }

        void UpdateResource([NotNull] Marker msg)
        {
            var newResourceInfo = GetRequestedResource(msg);
            if (newResourceInfo == resourceInfo) return;

            if (resource != null && resourceInfo != null)
            {
                resource.DisposeResource(resourceInfo);
                resource = null;
            }

            resourceInfo = newResourceInfo;
            if (resourceInfo == null)
            {
                if (msg.Type() != MarkerType.MeshResource)
                {
                    Logger.Warn($"MarkerObject: Marker type '{msg.Type}' has no resource assigned!");
                    description.Append(ErrorStr).AppendLine("Unknown marker type ").Append(msg.Type).AppendLine();
                    numErrors++;
                }
                else
                {
                    description.Append(WarnStr)
                        .Append("Unknown mesh resource '").Append(msg.MeshResource).Append("'")
                        .AppendLine();
                    numWarnings++;
                    Logger.Warn($"MarkerObject: Unknown mesh resource '{msg.MeshResource}'");
                }

                return;
            }

            GameObject resourceGameObject = ResourcePool.GetOrCreate(resourceInfo, transform);

            resource = resourceGameObject.GetComponent<MarkerResource>();
            if (resource != null) return; // all OK

            if (msg.Type() != MarkerType.MeshResource)
                // shouldn't happen!
                Debug.LogWarning($"Resource '{resourceInfo}' has no MarkerResource!");

            resource = resourceGameObject.AddComponent<AssetWrapperResource>();

            //Clickable = Clickable; // reset value
        }

        void UpdateTransform([NotNull] Marker msg)
        {
            if (msg.FrameLocked)
            {
                AttachTo(msg.Header.FrameId);
                description.Append("Frame Locked to: <i>").Append(msg.Header.FrameId).Append("</i>").AppendLine();
            }
            else
            {
                AttachTo(msg.Header.FrameId, msg.Header.Stamp);
            }

            if (msg.Pose.HasNaN())
            {
                description.Append(WarnStr).Append("Pose contains NaN values").AppendLine();
                numWarnings++;
                return;
            }

            transform.SetLocalPose(msg.Pose.Ros2Unity());
        }

        [CanBeNull]
        static Info<GameObject> GetRequestedResource([NotNull] Marker msg)
        {
            switch (msg.Type())
            {
                case MarkerType.Arrow:
                    return Resource.Displays.Arrow;
                case MarkerType.Cylinder:
                    return Resource.Displays.Cylinder;
                case MarkerType.Cube:
                    return Resource.Displays.Cube;
                case MarkerType.Sphere:
                    return Resource.Displays.Sphere;
                case MarkerType.TextViewFacing:
                case MarkerType.Text:
                    return Resource.Displays.Text;
                case MarkerType.LineStrip:
                case MarkerType.LineList:
                    return Resource.Displays.Line;
                case MarkerType.MeshResource:
                    if (!Uri.TryCreate(msg.MeshResource, UriKind.Absolute, out Uri uri))
                    {
                        Debug.Log($"MarkerObject: Could not convert '{msg.MeshResource}' into an uri!");
                        return null;
                    }

                    if (!Resource.TryGetResource(uri, out Info<GameObject> info, ConnectionManager.ServiceProvider))
                    {
                        Debug.Log($"MarkerObject: Failed to obtain resource '{uri}'!");
                        return null;
                    }

                    return info;
                case MarkerType.CubeList:
                case MarkerType.SphereList:
                    return Resource.Displays.MeshList;
                case MarkerType.Points:
                    return Resource.Displays.PointList;
                case MarkerType.TriangleList:
                    return Resource.Displays.MeshTriangles;
                case MarkerType.Image:
                    return Resource.Displays.Image;
                default:
                    return null;
            }
        }

        [NotNull]
        static string DescriptionFromType([NotNull] Marker msg)
        {
            switch (msg.Type())
            {
                case MarkerType.Arrow:
                    return "Arrow";
                case MarkerType.Cylinder:
                    return "Cylinder";
                case MarkerType.Cube:
                    return "Cube";
                case MarkerType.Sphere:
                    return "Sphere";
                case MarkerType.TextViewFacing:
                    return "Text_View_Facing";
                case MarkerType.Text:
                    return "Text";
                case MarkerType.LineStrip:
                    return "LineStrip";
                case MarkerType.LineList:
                    return "LineList";
                case MarkerType.MeshResource:
                    return $"MeshResource: {msg.MeshResource}";
                case MarkerType.CubeList:
                    return "CubeList";
                case MarkerType.SphereList:
                    return "SphereList";
                case MarkerType.Points:
                    return "Points";
                case MarkerType.TriangleList:
                    return "TriangleList";
                case MarkerType.Image:
                    return "Image";
                default:
                    return "[Unknown]";
            }
        }

        public void GenerateLog([NotNull] StringBuilder baseDescription)
        {
            if (baseDescription == null) throw new ArgumentNullException(nameof(baseDescription));

            baseDescription.Append(description);
        }

        public void GetErrorCount(out int totalErrors, out int totalWarnings)
        {
            totalErrors = numErrors;
            totalWarnings = numWarnings;
        }


        public override void Stop()
        {
            base.Stop();

            if (resource == null || resourceInfo == null) return;

            resource.DisposeResource(resourceInfo);
            resource = null;
            resourceInfo = null;
        }
    }


    internal static class MarkerTypeHelper
    {
        public static MarkerType Type([NotNull] this Marker marker)
        {
            return (MarkerType) marker.Type;
        }
    }
}