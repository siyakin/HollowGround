using System.Collections.Generic;
using UnityEngine;

namespace HollowGround.UI
{
    public class PanelManager
    {
        private readonly Dictionary<string, GameObject> _panels = new();
        private readonly List<string> _history = new();
        private string _currentPanelId;

        public string CurrentPanel => _currentPanelId;
        public bool IsPanelOpen => _currentPanelId != null;
        public event System.Action<string> OnPanelOpened;
        public event System.Action<string> OnPanelClosed;

        private static readonly HashSet<string> OverlayPanels = new()
        {
            "BuildingInfo", "BattleReport", "Toast", "ResourceBar"
        };

        public void Register(string id, GameObject panel)
        {
            if (panel == null) return;
            _panels[id] = panel;
        }

        public void Toggle(string id)
        {
            if (!_panels.TryGetValue(id, out var panel)) return;

            if (_currentPanelId == id)
            {
                CloseCurrent();
                return;
            }

            if (_currentPanelId != null && !OverlayPanels.Contains(_currentPanelId))
            {
                CloseImmediate(_currentPanelId);
            }

            if (!OverlayPanels.Contains(id) && _currentPanelId != null)
                _history.Add(_currentPanelId);

            panel.SetActive(true);
            _currentPanelId = id;
            OnPanelOpened?.Invoke(id);
        }

        public void OpenOverlay(string id)
        {
            if (!_panels.TryGetValue(id, out var panel)) return;
            panel.SetActive(true);
            OnPanelOpened?.Invoke(id);
        }

        public void CloseOverlay(string id)
        {
            if (!_panels.TryGetValue(id, out var panel)) return;
            panel.SetActive(false);
            OnPanelClosed?.Invoke(id);
        }

        public void CloseCurrent()
        {
            if (_currentPanelId == null) return;

            CloseImmediate(_currentPanelId);

            if (_history.Count > 0)
            {
                string prev = _history[_history.Count - 1];
                _history.RemoveAt(_history.Count - 1);
                if (_panels.TryGetValue(prev, out var panel))
                {
                    panel.SetActive(true);
                    _currentPanelId = prev;
                    OnPanelOpened?.Invoke(prev);
                }
                else
                {
                    _currentPanelId = null;
                }
            }
            else
            {
                _currentPanelId = null;
            }
        }

        public void CloseAll()
        {
            foreach (var kvp in _panels)
            {
                if (kvp.Value != null && kvp.Value.activeSelf)
                {
                    kvp.Value.SetActive(false);
                    OnPanelClosed?.Invoke(kvp.Key);
                }
            }
            _currentPanelId = null;
            _history.Clear();
        }

        public bool IsOpen(string id)
        {
            return _currentPanelId == id;
        }

        private void CloseImmediate(string id)
        {
            if (_panels.TryGetValue(id, out var panel) && panel != null)
            {
                panel.SetActive(false);
                OnPanelClosed?.Invoke(id);
            }
        }
    }
}
