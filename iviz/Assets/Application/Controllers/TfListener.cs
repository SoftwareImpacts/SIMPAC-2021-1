﻿using UnityEngine;
using System.Collections.Generic;
using tfMessage_v2 = Iviz.Msgs.Tf2Msgs.TFMessage;
using System.Linq;
using Iviz.Roslib;
using Iviz.Msgs.GeometryMsgs;
using Iviz.Msgs.Tf;
using System.Runtime.Serialization;
using System;
using Iviz.App;
using Iviz.Displays;
using Iviz.Resources;
using JetBrains.Annotations;
using Transform = UnityEngine.Transform;

namespace Iviz.Controllers
{
    [DataContract]
    public sealed class TfConfiguration : JsonToString, IConfiguration
    {
        [DataMember] public Guid Id { get; set; } = Guid.NewGuid();
        [DataMember] public Resource.Module Module => Resource.Module.TF;
        [DataMember] public bool Visible { get; set; } = true;
        [DataMember] public string Topic { get; set; } = "";
        [DataMember] public bool AxisVisible { get; set; } = true;
        [DataMember] public float AxisSize { get; set; } = 0.125f;
        [DataMember] public bool AxisLabelVisible { get; set; } = false;
        [DataMember] public bool ParentConnectorVisible { get; set; } = false;
        [DataMember] public bool ShowAllFrames { get; set; } = true;
    }

    public sealed class TfListener : ListenerController
    {
        public const string DefaultTopic = "/tf";
        public const string BaseFrameId = "map";

        const string DefaultTopicStatic = "/tf_static";

        public static TfListener Instance { get; private set; }
        [NotNull] public RosSender<tfMessage_v2> Publisher { get; }
        public IRosListener ListenerStatic { get; private set; }

        public static Camera MainCamera { get; set; }
        public static GuiCamera GuiCamera => GuiCamera.Instance;
        public static Light MainLight { get; set; }

        public static TfFrame MapFrame { get; private set; }
        public static TfFrame RootFrame { get; private set; }
        public static TfFrame UnityFrame { get; private set; }

        readonly InteractiveControl rootMarker;
        public static InteractiveControl RootMarker => Instance.rootMarker;

        public static TfFrame ListenersFrame => RootFrame;

        public override TfFrame Frame => MapFrame;

        readonly DisplayNode showAllListener;
        readonly DisplayNode staticListener;

        readonly Dictionary<string, TfFrame> frames = new Dictionary<string, TfFrame>();

        [NotNull]
        public static IEnumerable<string> FramesUsableAsHints =>
            Instance.frames.Values.Where(IsFrameUsableAsHint).Select(frame => frame.Id);

        static bool IsFrameUsableAsHint(TfFrame frame) => frame != RootFrame && frame != UnityFrame;

        public override IModuleData ModuleData { get; }

        readonly TfConfiguration config = new TfConfiguration();

        [NotNull]
        public TfConfiguration Config
        {
            get => config;
            set
            {
                config.Topic = value.Topic;
                AxisVisible = value.AxisVisible;
                FrameSize = value.AxisSize;
                AxisLabelVisible = value.AxisLabelVisible;
                ParentConnectorVisible = value.ParentConnectorVisible;
                ShowAllFrames = value.ShowAllFrames;
            }
        }

        public bool AxisVisible
        {
            get => config.AxisVisible;
            set
            {
                config.AxisVisible = value;
                foreach (var frame in frames.Values)
                {
                    frame.Visible = value;
                }

                if (rootMarker != null)
                {
                    rootMarker.Visible = value;
                }
            }
        }

        public bool AxisLabelVisible
        {
            get => config.AxisLabelVisible;
            set
            {
                config.AxisLabelVisible = value;
                foreach (var frame in frames.Values)
                {
                    frame.LabelVisible = value;
                }
            }
        }

        public float FrameSize
        {
            get => config.AxisSize;
            set
            {
                config.AxisSize = value;
                foreach (var frame in frames.Values)
                {
                    frame.FrameSize = value;
                }

                if (rootMarker != null)
                {
                    rootMarker.BaseScale = 2.5f * FrameSize;
                }
            }
        }

        public float FrameLabelSize => 0.5f * FrameSize;

        public bool ParentConnectorVisible
        {
            get => config.ParentConnectorVisible;
            set
            {
                config.ParentConnectorVisible = value;
                foreach (var frame in frames.Values)
                {
                    frame.ConnectorVisible = value;
                }
            }
        }

        public bool ShowAllFrames
        {
            get => config.ShowAllFrames;
            set
            {
                config.ShowAllFrames = value;
                if (value)
                {
                    foreach (var frame in frames.Values)
                    {
                        frame.AddListener(showAllListener);
                    }
                }
                else
                {
                    // we create a copy because this generally modifies the collection
                    var framesCopy = frames.Values.ToList();
                    foreach (var frame in framesCopy)
                    {
                        frame.RemoveListener(showAllListener);
                    }
                }
            }
        }

        public TfListener([NotNull] IModuleData moduleData)
        {
            ModuleData = moduleData ?? throw new ArgumentNullException(nameof(moduleData));
            Instance = this;

            DisplayNode defaultListener = SimpleDisplayNode.Instantiate("[.]", null);

            UnityFrame = Add(CreateFrameObject("TF", null, null));
            UnityFrame.ForceInvisible = true;
            UnityFrame.Visible = false;
            UnityFrame.AddListener(defaultListener);

            showAllListener = SimpleDisplayNode.Instantiate("[TFNode]", UnityFrame.transform);
            staticListener = SimpleDisplayNode.Instantiate("[TFStatic]", UnityFrame.transform);
            defaultListener.transform.parent = UnityFrame.transform;

            GameObject mainCameraObj = GameObject.Find("MainCamera");
            MainCamera = mainCameraObj.GetComponent<Camera>();

            GameObject mainLight = GameObject.Find("MainLight");
            MainLight = mainLight.GetComponent<Light>();

            Config = new TfConfiguration();

            RootFrame = Add(CreateFrameObject("/", UnityFrame.transform, UnityFrame));
            RootFrame.ForceInvisible = true;
            RootFrame.Visible = false;
            RootFrame.AddListener(defaultListener);

            MapFrame = Add(CreateFrameObject(BaseFrameId, UnityFrame.transform, RootFrame));
            MapFrame.Parent = RootFrame;
            MapFrame.AddListener(defaultListener);
            MapFrame.AcceptsParents = false;
            //BaseFrame.ForceInvisible = true;

            rootMarker = ResourcePool.GetOrCreate<InteractiveControl>(
                Resource.Displays.InteractiveControl,
                RootFrame.transform);
            rootMarker.name = "[InteractiveController for /]";
            rootMarker.TargetTransform = RootFrame.transform;
            rootMarker.InteractionMode = InteractiveControl.InteractionModeType.Disabled;
            rootMarker.BaseScale = 2.5f * FrameSize;

            Publisher = new RosSender<tfMessage_v2>(DefaultTopic);
        }

        public override void StartListening()
        {
            Listener = new RosListener<tfMessage_v2>(DefaultTopic, SubscriptionHandler_v2) {MaxQueueSize = 200};
            ListenerStatic = new RosListener<tfMessage_v2>(DefaultTopicStatic, SubscriptionHandlerStatic)
                {MaxQueueSize = 200};
        }

        void ProcessMessages([NotNull] TransformStamped[] transforms, bool isStatic)
        {
            foreach (TransformStamped t in transforms)
            {
                if (t.Transform.HasNaN())
                {
                    continue;
                }

                TimeSpan timestamp = t.Header.Stamp == default ? TimeSpan.MaxValue : t.Header.Stamp.ToTimeSpan();

                string childId = t.ChildFrameId;
                if (childId.Length != 0 && childId[0] == '/')
                {
                    childId = childId.Substring(1);
                }

                TfFrame child;
                if (isStatic)
                {
                    child = GetOrCreateFrame(childId, staticListener);
                    if (config.ShowAllFrames)
                    {
                        child.AddListener(showAllListener);
                    }
                }
                else if (config.ShowAllFrames)
                {
                    child = GetOrCreateFrame(childId, showAllListener);
                }
                else if (!TryGetFrameImpl(childId, out child))
                {
                    continue;
                }

                string parentId = t.Header.FrameId;
                if (parentId.Length != 0 && parentId[0] == '/') // remove starting '/' from tf v1
                {
                    parentId = parentId.Substring(1);
                }

                TfFrame parent = string.IsNullOrEmpty(parentId) ? RootFrame : GetOrCreateFrame(parentId);

                if (child.SetParent(parent))
                {
                    child.SetPose(timestamp, t.Transform.Ros2Unity());
                }
            }
        }

        public override void ResetController()
        {
            base.ResetController();

            ListenerStatic?.Reset();
            Publisher.Reset();

            bool prevShowAllFrames = ShowAllFrames;
            ShowAllFrames = false;

            var framesCopy = frames.Values.ToList();
            foreach (var frame in framesCopy)
            {
                frame.RemoveListener(staticListener);
            }

            ShowAllFrames = prevShowAllFrames;
        }

        [NotNull]
        TfFrame Add([NotNull] TfFrame t)
        {
            frames.Add(t.Id, t);
            return t;
        }

        [ContractAnnotation("=> false, frame:null; => true, frame:notnull")]
        public static bool TryGetFrame([NotNull] string id, out TfFrame frame)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return Instance.TryGetFrameImpl(id, out frame);
        }


        [NotNull]
        public static TfFrame GetOrCreateFrame([NotNull] string id, [CanBeNull] DisplayNode listener = null)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (id.Length != 0 && id[0] == '/')
            {
                id = id.Substring(1);
            }

            TfFrame frame = Instance.GetOrCreateFrameImpl(id);
            if (frame.Id != id)
            {
                Debug.LogWarning("Error: Broken resource pool! Requested " + id + ", received " + frame.Id);
            }

            if (listener != null)
            {
                frame.AddListener(listener);
            }

            return frame;
        }

        [NotNull]
        TfFrame GetOrCreateFrameImpl([NotNull] string id)
        {
            return TryGetFrameImpl(id, out TfFrame t) ? t : Add(CreateFrameObject(id, RootFrame.transform, RootFrame));
        }

        [NotNull]
        TfFrame CreateFrameObject([NotNull] string id, [CanBeNull] Transform parent, [CanBeNull] TfFrame parentFrame)
        {
            TfFrame frame = ResourcePool.GetOrCreate<TfFrame>(Resource.Displays.TfFrame, parent);
            frame.name = $"{{{id}}}";
            frame.Id = id;
            frame.Visible = config.AxisVisible;
            frame.FrameSize = config.AxisSize;
            frame.LabelVisible = config.AxisLabelVisible;
            frame.ConnectorVisible = config.ParentConnectorVisible;
            if (parentFrame != null)
            {
                frame.Parent = parentFrame;
            }

            return frame;
        }

        [ContractAnnotation("=> false, t:null; => true, t:notnull")]
        bool TryGetFrameImpl([NotNull] string id, out TfFrame t)
        {
            return frames.TryGetValue(id, out t);
        }

        void SubscriptionHandler_v1([NotNull] tfMessage msg)
        {
            ProcessMessages(msg.Transforms, false);
        }

        void SubscriptionHandler_v2([NotNull] tfMessage_v2 msg)
        {
            ProcessMessages(msg.Transforms, false);
        }

        void SubscriptionHandlerStatic([NotNull] tfMessage_v2 msg)
        {
            ProcessMessages(msg.Transforms, true);
        }

        public void MarkAsDead([NotNull] TfFrame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            frames.Remove(frame.Id);
            GuiCamera.Unselect(frame);
            frame.Stop();
            ResourcePool.Dispose(Resource.Displays.TfFrame, frame.gameObject);
        }

        public static void Publish([NotNull] tfMessage_v2 msg)
        {
            Instance.Publisher.Publish(msg);
        }

        public static UnityEngine.Pose RelativePoseToRoot(in UnityEngine.Pose unityPose)
        {
            if (!Settings.IsMobile)
            {
                return unityPose;
            }

            Transform rootFrame = RootFrame.transform;
            return new UnityEngine.Pose(
                rootFrame.InverseTransformPoint(unityPose.position),
                UnityEngine.Quaternion.Inverse(rootFrame.rotation) * unityPose.rotation
            );
        }

        static uint tfSeq;

        public static void Publish([CanBeNull] string parentFrame, [CanBeNull] string childFrame,
            in UnityEngine.Pose unityPose)
        {
            tfMessage_v2 msg = new tfMessage_v2
            (
                Transforms: new[]
                {
                    new TransformStamped
                    (
                        Header: RosUtils.CreateHeader(tfSeq++, parentFrame ?? BaseFrameId),
                        ChildFrameId: childFrame ?? "",
                        Transform: RelativePoseToRoot(unityPose).Unity2RosTransform()
                    )
                }
            );
            Publish(msg);
        }

        public void OnARModeChanged(bool _)
        {
            UpdateRootMarkerVisibility();
        }

        public static void UpdateRootMarkerVisibility()
        {
            bool arEnabled = ARController.Instance?.Visible ?? false;
            bool viewEnabled = ARController.Instance?.ShowRootMarker ?? false;
            if (arEnabled && viewEnabled)
            {
                RootMarker.InteractionMode = InteractiveControl.InteractionModeType.Frame;
                //MapFrame.ColliderEnabled = false;
                MapFrame.Selected = false;
            }
            else
            {
                RootMarker.InteractionMode = InteractiveControl.InteractionModeType.Disabled;
                //MapFrame.ColliderEnabled = !Settings.IsHololens;
            }
        }
    }
}