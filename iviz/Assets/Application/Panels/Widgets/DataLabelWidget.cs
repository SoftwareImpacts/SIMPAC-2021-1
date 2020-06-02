﻿using Iviz.Resources;
using UnityEngine;
using UnityEngine.UI;

namespace Iviz.App
{
    public class DataLabelWidget : MonoBehaviour, IWidget
    {
        public Text label;
        bool interactable = true;

        public string Label
        {
            get => label.text;
            set
            {
                name = "DataLabel:" + value;
                label.text = value;
            }
        }
        public bool Interactable
        {
            get => interactable;
            set
            {
                interactable = value;
                label.color = value ? Resource.Colors.EnabledFontColor : Resource.Colors.DisabledFontColor;
            }
        }

        public DataLabelWidget SetLabel(string f)
        {
            Label = f;
            return this;
        }

        public void ClearSubscribers()
        {
        }
    }
}