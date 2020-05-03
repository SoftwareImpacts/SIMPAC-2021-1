﻿using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Iviz.App
{
    public class DepthImageProjectorDisplayData : DisplayData
    {
        DepthImageProjector display;
        DepthImageProjectorPanelContents panel;

        public override DataPanelContents Panel => panel;
        public override Resource.Module Module => Resource.Module.DepthImageProjector;

        readonly List<string> depthImageCandidates = new List<string>();
        readonly List<string> colorImageCandidates = new List<string>();


        public override DisplayData Initialize(DisplayListPanel displayList, string topic, string type)
        {
            base.Initialize(displayList, topic, type);

            GameObject displayObject = ResourcePool.GetOrCreate(Resource.Displays.DepthImageProjector);
            display = displayObject.GetComponent<DepthImageProjector>();
            display.Parent = TFListener.DisplaysFrame;
            panel = DataPanelManager.GetPanelByResourceType(Resource.Module.DepthImageProjector) as DepthImageProjectorPanelContents;
            return this;
        }

        public override DisplayData Deserialize(JToken j)
        {
            display.Config = j.ToObject<DepthImageProjector.Configuration>();
            display.ColorImage = GetImageWithName(display.ColorName);
            display.DepthImage = GetImageWithName(display.DepthName);
            return this;
        }


        public override void Cleanup()
        {
            base.Cleanup();

            display.Stop();
            ResourcePool.Dispose(Resource.Displays.DepthImageProjector, display.gameObject);
            display = null;
        }

        public override void SetupPanel()
        {
            depthImageCandidates.Clear();
            depthImageCandidates.Add("<none>");
            depthImageCandidates.AddRange(
                DisplayListPanel.DisplayDatas.
                Where(x => x.Module == Resource.Module.Image).
                Select(x => x.Topic)
            );
            panel.Depth.Options = depthImageCandidates;
            panel.Depth.Value = display.DepthName;

            colorImageCandidates.Clear();
            colorImageCandidates.Add("<none>");
            colorImageCandidates.AddRange(
                DisplayListPanel.DisplayDatas.
                Where(x => x.Module == Resource.Module.Image).
                Select(x => x.Topic)
            );
            panel.Color.Options = colorImageCandidates;
            panel.Color.Value = display.ColorName;

            panel.PointSize.Value = display.PointSize;
            panel.FOV.Value = display.FovAngle;

            panel.PointSize.ValueChanged += f =>
            {
                display.PointSize = f;
            };
            panel.Depth.ValueChanged += (i, s) =>
            {
                if (i == 0)
                {
                    display.DepthImage = null;
                }
                else
                {
                    display.DepthImage = GetImageWithName(s);
                }
                display.DepthName = s;
            };
            panel.Color.ValueChanged += (i, s) =>
            {
                if (i == 0)
                {
                    display.ColorImage = null;
                }
                else
                {
                    display.ColorImage = GetImageWithName(s);
                }
                display.ColorName = s;
            };
            panel.FOV.ValueChanged += f =>
            {
                display.FovAngle = f;
            };
            panel.CloseButton.Clicked += () =>
            {
                DataPanelManager.HideSelectedPanel();
                DisplayListPanel.RemoveDisplay(this);
            };
        }

        ImageListener GetImageWithName(string name)
        {
            return DisplayListPanel.DisplayDatas.
                Where(x => x.Module == Resource.Module.Image).
                Select(x => x as ImageDisplayData).
                FirstOrDefault(x => x.Topic == name)?.Image;
        }

        public override void UpdatePanel()
        {
            base.UpdatePanel();

            depthImageCandidates.Clear();
            depthImageCandidates.Add("<none>");
            depthImageCandidates.AddRange(
                DisplayListPanel.DisplayDatas.
                Where(x => x.Module == Resource.Module.Image).
                Select(x => x.Topic)
            );
            panel.Depth.Options = depthImageCandidates;

            colorImageCandidates.Clear();
            colorImageCandidates.Add("<none>");
            colorImageCandidates.AddRange(
                DisplayListPanel.DisplayDatas.
                Where(x => x.Module == Resource.Module.Image).
                Select(x => x.Topic)
            );
            panel.Color.Options = colorImageCandidates;
        }

        public override JToken Serialize()
        {
            return JToken.FromObject(display.Config);
        }
    }
}