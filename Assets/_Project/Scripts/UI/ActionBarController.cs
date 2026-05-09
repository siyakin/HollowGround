using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class ActionBarController
    {
        private readonly Dictionary<string, Button> _buttons = new();
        private readonly Dictionary<string, ThemedButton> _themed = new();
        private readonly PanelManager _panels;

        public ActionBarController(PanelManager panels)
        {
            _panels = panels;
        }

        public void Initialize()
        {
            var actionBar = FindActionBar();
            if (actionBar == null) return;

            var map = new (string id, string btnName)[]
            {
                ("BuildMenu", "BtnBuild"),
                ("TechTree", "BtnResearch"),
                ("Training", "BtnArmy"),
                ("Hero", "BtnHero"),
                ("Settler", "BtnSettler"),
                ("QuestLog", "BtnQuest"),
                ("FactionTrade", "BtnTrade"),
                ("WorldMap", "BtnMap")
            };

            foreach (var (id, btnName) in map)
            {
                var t = actionBar.transform.Find(btnName);
                if (t == null) continue;

                var btn = t.GetComponent<Button>();
                if (btn == null) continue;

                _buttons[id] = btn;
                var themed = t.GetComponent<ThemedButton>();
                if (themed == null)
                    themed = t.gameObject.AddComponent<ThemedButton>();
                themed.styleType = UIStyleType.ActionBarButton;
                _themed[id] = themed;
            }
        }

        public void UpdateHighlights()
        {
            foreach (var kvp in _themed)
            {
                if (kvp.Value == null) continue;
                kvp.Value.SetSelected(_panels.IsOpen(kvp.Key));
            }
        }

        private static Transform FindActionBar()
        {
            var canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
                return canvas.transform.Find("ActionBar");
            return null;
        }
    }
}
