﻿using UnityEngine;
using System.Linq;
using System;
using UnityEngine.EventSystems;
using Iviz.App.Displays;
using Iviz.Msgs.VisualizationMsgs;
using Iviz.Displays;
using Iviz.App.Listeners;
using Iviz.Resources;

namespace Iviz.App.Listeners
{
    enum MarkerType
    {
        ARROW = Marker.ARROW,
        CUBE = Marker.CUBE,
        SPHERE = Marker.SPHERE,
        CYLINDER = Marker.CYLINDER,
        LINE_STRIP = Marker.LINE_STRIP,
        LINE_LIST = Marker.LINE_LIST,
        CUBE_LIST = Marker.CUBE_LIST,
        SPHERE_LIST = Marker.SPHERE_LIST,
        POINTS = Marker.POINTS,
        TEXT_VIEW_FACING = Marker.TEXT_VIEW_FACING,
        MESH_RESOURCE = Marker.MESH_RESOURCE,
        TRIANGLE_LIST = Marker.TRIANGLE_LIST,
    }

    public class MarkerObject : ClickableNode
    {
        const string packagePrefix = "package://ibis/";

        public event Action<Vector3, int> Clicked;

        public string Id { get; private set; }

        MarkerResource resource;
        Resource.Info<GameObject> resourceType;
        Mesh cacheCube, cacheSphere;

        public override Bounds Bounds => resource?.Bounds ?? new Bounds();
        public override Bounds WorldBounds => resource?.WorldBounds ?? new Bounds();

        public DateTime ExpirationTime { get; private set; }

        void Awake()
        {
            cacheCube = Resource.Markers.Cube.Object.GetComponent<MeshFilter>().sharedMesh;
            cacheSphere = Resource.Markers.SphereSimple.Object.GetComponent<MeshFilter>().sharedMesh;
        }

        public void Set(Marker msg)
        {
            Id = MarkerListener.IdFromMessage(msg);
            name = Id;

            ExpirationTime = msg.Lifetime.IsZero ?
                DateTime.MaxValue :
                DateTime.Now + msg.Lifetime.ToTimeSpan();

            Resource.Info<GameObject> newResourceType = GetRequestedResource(msg);
            if (newResourceType != resourceType)
            {
                if (resource != null)
                {
                    ResourcePool.Dispose(resourceType, resource.gameObject);
                    resource = null;
                }
                resourceType = newResourceType;
                if (resourceType == null)
                {
                    if (msg.Type() == MarkerType.MESH_RESOURCE)
                    {
                        Logger.Error($"MarkerObject: Unknown mesh resource '{msg.MeshResource}'");
                    }
                    else
                    {
                        Logger.Error($"MarkerObject: Marker type '{msg.Type()}'");
                    }
                    return;
                }
                resource = ResourcePool.GetOrCreate<MarkerResource>(resourceType, transform);
                if (resource == null)
                {
                    Debug.LogError("Resource " + resourceType + " has no MarkerResource!");
                }
            }
            if (resource == null)
            {
                return;
            }

            UpdateTransform(msg);

            resource.gameObject.layer = Resource.ClickableLayer;

            switch (msg.Type())
            {
                case MarkerType.CUBE:
                case MarkerType.SPHERE:
                case MarkerType.CYLINDER:
                    MeshMarkerResource meshMarker = resource as MeshMarkerResource;
                    meshMarker.Color = msg.Color.Sanitize().ToUnityColor();
                    transform.localScale = msg.Scale.Ros2Unity().Abs();
                    break;
                case MarkerType.TEXT_VIEW_FACING:
                    TextMarkerResource textResource = resource as TextMarkerResource;
                    textResource.Text = msg.Text;
                    textResource.Color = msg.Color.Sanitize().ToUnityColor();
                    transform.localScale = (float)msg.Scale.Z * Vector3.one;
                    break;
                case MarkerType.CUBE_LIST:
                case MarkerType.SPHERE_LIST:
                    MeshListResource meshList = resource as MeshListResource;
                    meshList.Mesh = (msg.Type() == MarkerType.CUBE_LIST) ? cacheCube : cacheSphere;
                    meshList.SetSize(msg.Points.Length);
                    meshList.Scale = msg.Scale.Ros2Unity().Abs();
                    meshList.Color = msg.Color.Sanitize().ToUnityColor();
                    meshList.Colors = (msg.Colors.Length == 0) ? null : msg.Colors.Select(x => x.ToUnityColor32());
                    meshList.Points = msg.Points.Select(x => x.Ros2Unity());
                    break;
                case MarkerType.LINE_LIST:
                    {
                        LineResource lineResource = resource as LineResource;
                        lineResource.Scale = (float)msg.Scale.X;
                        LineWithColor[] lines = new LineWithColor[msg.Points.Length / 2];
                        if (msg.Colors.Length == 0)
                        {
                            Color32 color = msg.Color.Sanitize().ToUnityColor32();
                            for (int i = 0; i < lines.Length; i++)
                            {
                                lines[i] = new LineWithColor(
                                    msg.Points[2 * i + 0].Ros2Unity(), color,
                                    msg.Points[2 * i + 1].Ros2Unity(), color
                                    );
                            }
                        }
                        else
                        {
                            Color color = msg.Color.Sanitize().ToUnityColor();
                            for (int i = 0; i < lines.Length; i++)
                            {
                                lines[i] = new LineWithColor(
                                    msg.Points[2 * i + 0].Ros2Unity(), color * msg.Colors[2 * i + 0].ToUnityColor(),
                                    msg.Points[2 * i + 1].Ros2Unity(), color * msg.Colors[2 * i + 1].ToUnityColor()
                                    );
                            }
                        }
                        lineResource.LinesWithColor = lines;
                        break;
                    }
                case MarkerType.LINE_STRIP:
                    {
                        LineResource lineResource = resource as LineResource;
                        lineResource.Scale = (float)msg.Scale.X;
                        LineWithColor[] lines = new LineWithColor[msg.Points.Length - 1];
                        if (msg.Colors.Length == 0)
                        {
                            Color32 color = msg.Color.Sanitize().ToUnityColor32();
                            for (int i = 0; i < lines.Length; i++)
                            {
                                lines[i] = new LineWithColor(
                                    msg.Points[i + 0].Ros2Unity(), color,
                                    msg.Points[i + 1].Ros2Unity(), color
                                    );
                            }
                        }
                        else
                        {
                            Color color = msg.Color.Sanitize().ToUnityColor();
                            for (int i = 0; i < lines.Length; i++)
                            {
                                lines[i] = new LineWithColor(
                                    msg.Points[i + 0].Ros2Unity(), color * msg.Colors[i + 0].ToUnityColor(),
                                    msg.Points[i + 1].Ros2Unity(), color * msg.Colors[i + 1].ToUnityColor()
                                    );
                            }
                        }
                        lineResource.LinesWithColor = lines;
                        break;
                    }
                case MarkerType.POINTS:
                    {
                        PointListResource pointList = resource as PointListResource;
                        pointList.Scale = msg.Scale.Ros2Unity().Abs();
                        PointWithColor[] points = new PointWithColor[msg.Points.Length];
                        Color color = msg.Color.Sanitize().ToUnityColor();
                        if (msg.Colors.Length == 0 || color == Color.black)
                        {
                            for (int i = 0; i < points.Length; i++)
                            {
                                points[i] = new PointWithColor(msg.Points[i].Ros2Unity(), color);
                            }
                        }
                        else if (color == Color.white)
                        {
                            for (int i = 0; i < points.Length; i++)
                            {
                                points[i] = new PointWithColor(
                                    msg.Points[i].Ros2Unity(),
                                    msg.Colors[i].ToUnityColor32());
                            }
                        }
                        else
                        {
                            for (int i = 0; i < points.Length; i++)
                            {
                                points[i] = new PointWithColor(
                                    msg.Points[i].Ros2Unity(),
                                    color * msg.Colors[i].ToUnityColor());
                            }
                        }
                        pointList.PointsWithColor = points;
                        pointList.UseIntensityTexture = false;
                        break;
                    }
                case MarkerType.TRIANGLE_LIST:
                    MeshTrianglesResource meshTriangles = resource as MeshTrianglesResource;
                    meshTriangles.Color = msg.Color.Sanitize().ToUnityColor();
                    if (msg.Colors.Length != 0)
                    {
                        meshTriangles.Set(
                            msg.Points.Select(x => x.Ros2Unity()).ToArray(),
                            msg.Colors.Select(x => x.ToUnityColor()).ToArray()
                            );
                    }
                    else
                    {
                        meshTriangles.Set(msg.Points.Select(x => x.Ros2Unity()).ToArray());
                    }
                    break;
            }
        }

        void UpdateTransform(Marker msg)
        {
            if (msg.FrameLocked)
            {
                SetParent(msg.Header.FrameId);
            }
            else if (msg.Header.FrameId == "")
            {
                Parent = TFListener.ListenersFrame;
            }
            else
            {
                Pose pose = TFListener.GetOrCreateFrame(msg.Header.FrameId).transform.AsPose();
                Parent = TFListener.BaseFrame;
                transform.SetLocalPose(pose);
            }

            transform.SetLocalPose(msg.Pose.Ros2Unity());
        }

        Resource.Info<GameObject> GetRequestedResource(Marker msg)
        {
            switch (msg.Type())
            {
                case MarkerType.ARROW: return Resource.Markers.Arrow;
                case MarkerType.CYLINDER: return Resource.Markers.Cylinder;
                case MarkerType.CUBE: return Resource.Markers.Cube;
                case MarkerType.SPHERE: return Resource.Markers.Sphere;
                case MarkerType.TEXT_VIEW_FACING: return Resource.Markers.Text;
                case MarkerType.LINE_STRIP:
                case MarkerType.LINE_LIST:
                    return Resource.Markers.Line;
                case MarkerType.MESH_RESOURCE:
                    if (!Uri.IsWellFormedUriString(msg.MeshResource, UriKind.Absolute))
                    {
                        return null;
                    }
                    if (msg.MeshResource.StartsWith(packagePrefix))
                    {
                        string resourcePath = msg.MeshResource.Substring(packagePrefix.Length);
                        return Resource.Markers.Generic.TryGetValue(resourcePath, out Resource.Info<GameObject> info) ? info : null;
                    }
                    return null;
                case MarkerType.CUBE_LIST:
                case MarkerType.SPHERE_LIST:
                    return Resource.Markers.MeshList;
                case MarkerType.POINTS:
                    return Resource.Markers.PointList;
                case MarkerType.TRIANGLE_LIST:
                    return Resource.Markers.MeshTriangles;
                default:
                    return null;
            }
        }

        public override void Stop()
        {
            base.Stop();
            if (resource == null)
            {
                return;
            }
            resource.ColliderEnabled = false;
            ResourcePool.Dispose(resourceType, resource.gameObject);
            resource = null;
            resourceType = null;
            Clicked = null;
        }

        public bool ColliderEnabled
        {
            get => resource?.ColliderEnabled ?? false;
            set
            {
                if (resource != null)
                {
                    resource.ColliderEnabled = value;
                }
            }
        }

        public override string Name => name;

        public override Pose BoundsPose => resource?.transform.AsPose() ?? new Pose();

        public override Vector3 BoundsScale => Vector3.one;

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (!eventData.IsPointerMoving())
            {
                if (eventData.clickCount == 1)
                {
                    Clicked?.Invoke(eventData.pointerCurrentRaycast.worldPosition, 0);
                }
                else if (eventData.clickCount == 2)
                {
                    Clicked?.Invoke(eventData.pointerCurrentRaycast.worldPosition, 1);
                }
            }
            base.OnPointerClick(eventData);
        }
    }

    static class MarkerTypeHelper
    {
        public static MarkerType Type(this Marker marker)
        {
            return (MarkerType)marker.Type;
        }
    }
}