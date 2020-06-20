﻿using UnityEngine;
using System.Collections;
using Iviz.App;
using System;
using System.Net;
using System.Text;
using System.IO;

namespace Iviz.App
{
    public class TFDialogData : DialogData
    {
        TFDialogContents panel;
        public override IDialogPanelContents Panel => panel;

        public override void Initialize(DisplayListPanel panel)
        {
            base.Initialize(panel);
            this.panel = (TFDialogContents)DialogPanelManager.GetPanelByType(DialogPanelType.TF);
        }

        public override void SetupPanel()
        {
            panel.Active = true;
            panel.Close.Clicked += Close;
        }

        public override void UpdatePanel()
        {
            base.UpdatePanel();
            panel.TFLog.Flush();
        }

        void Close()
        {
            DialogPanelManager.HidePanelFor(this);
        }
    }
}