﻿using Iviz.Controllers;
using Iviz.Core;
using Iviz.Resources;
using JetBrains.Annotations;

namespace Iviz.App
{
    /// <summary>
    /// <see cref="PointCloudPanelContents"/> 
    /// </summary>

    public sealed class PointCloudModuleData : ListenerModuleData
    {
        [NotNull] readonly PointCloudListener listener;
        [NotNull] readonly PointCloudPanelContents panel;

        protected override ListenerController Listener => listener;

        public override DataPanelContents Panel => panel;
        public override Resource.Module Module => Resource.Module.PointCloud;
        public override IConfiguration Configuration => listener.Config;


        public PointCloudModuleData([NotNull] ModuleDataConstructor constructor) :
        base(constructor.ModuleList,
            constructor.GetConfiguration<PointCloudConfiguration>()?.Topic ?? constructor.Topic, constructor.Type)
        {
            panel = DataPanelManager.GetPanelByResourceType<PointCloudPanelContents>(Resource.Module.PointCloud);
            listener = new PointCloudListener(this);
            if (constructor.Configuration == null)
            {
                listener.Config.Topic = Topic;
            }
            else
            {
                listener.Config = (PointCloudConfiguration)constructor.Configuration;
            }
            listener.StartListening();
            UpdateModuleButton();
        }

        public override void SetupPanel()
        {
            panel.Listener.RosListener = listener.Listener;
            panel.Frame.Owner = listener;

            string minIntensityStr = listener.MeasuredIntensityBounds.x.ToString("#,0.##", UnityUtils.Culture);
            string maxIntensityStr = listener.MeasuredIntensityBounds.y.ToString("#,0.##", UnityUtils.Culture);
            panel.NumPoints.Label = 
                $"<b>{listener.Size} Points</b>\n" +
                (listener.Size == 0 ? "Empty" :
                    listener.IsIntensityUsed ? $"[{minIntensityStr} .. {maxIntensityStr}]" :
                    "Color");

            panel.Colormap.Index = (int)listener.Colormap;
            panel.PointSize.Value = listener.PointSize;
            panel.IntensityChannel.Options = listener.FieldNames;
            panel.IntensityChannel.Value = listener.IntensityChannel;
            panel.HideButton.State = listener.Visible;

            panel.ForceMinMax.Value = listener.ForceMinMax;
            panel.MinIntensity.Value = listener.MinIntensity;
            panel.MaxIntensity.Value = listener.MaxIntensity;
            panel.MinIntensity.Interactable = listener.ForceMinMax;
            panel.MaxIntensity.Interactable = listener.ForceMinMax;
            panel.FlipMinMax.Value = listener.FlipMinMax;

            panel.PointSize.ValueChanged += f =>
            {
                listener.PointSize = f;
            };
            panel.IntensityChannel.ValueChanged += (_, s) =>
            {
                listener.IntensityChannel = s;
            };
            panel.Colormap.ValueChanged += (i, _) =>
            {
                listener.Colormap = (Resource.ColormapId)i;
            };
            panel.CloseButton.Clicked += () =>
            {
                DataPanelManager.HideSelectedPanel();
                ModuleListPanel.RemoveModule(this);
            };
            panel.HideButton.Clicked += () =>
            {
                listener.Visible = !listener.Visible;
                panel.HideButton.State = listener.Visible;
                UpdateModuleButton();
            };
            panel.ForceMinMax.ValueChanged += f =>
            {
                listener.ForceMinMax = f;
                panel.MinIntensity.Interactable = f;
                panel.MaxIntensity.Interactable = f;
            };
            panel.FlipMinMax.ValueChanged += f =>
            {
                listener.FlipMinMax = f;
            };
            panel.MinIntensity.ValueChanged += f =>
            {
                listener.MinIntensity = f;
            };
            panel.MaxIntensity.ValueChanged += f =>
            {
                listener.MaxIntensity = f;
            };
        }

        public override void UpdatePanel()
        {
            base.UpdatePanel();
            panel.IntensityChannel.Options = listener.FieldNames;

            string minIntensityStr = listener.MeasuredIntensityBounds.x.ToString("#,0.##", UnityUtils.Culture);
            string maxIntensityStr = listener.MeasuredIntensityBounds.y.ToString("#,0.##", UnityUtils.Culture);
            panel.NumPoints.Label = 
                $"<b>{listener.Size} Points</b>\n" +
                (listener.Size == 0 ? "Empty" :
                listener.IsIntensityUsed ? $"[{minIntensityStr} .. {maxIntensityStr}]" :
                "Color");
        }

        public override void AddToState(StateConfiguration config)
        {
            config.PointClouds.Add(listener.Config);
        }
    }
}
