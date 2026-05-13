using System.Collections.Generic;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.UI
{
    public class TooltipData
    {
        public string Title;
        public string Subtitle;
        public string Description;
        public string StateText;
        public Color StateColor;
        public bool HasState;
        public List<CostLine> Costs = new();
        public List<string> InfoLines = new();
        public Color TitleColor;
        public bool HasTitleColor;

        public struct CostLine
        {
            public ResourceType Type;
            public int Amount;
            public int Have;
            public bool ShowHave;
        }
    }
}
