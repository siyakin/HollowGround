using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HollowGround.UI
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Func<TooltipData> _provider;
        private bool _showImmediate;

        public void SetContent(string title, string description)
        {
            _provider = () => TooltipContentBuilder.ForText(title, description);
            _showImmediate = false;
        }

        public void SetContent(string title, string subtitle, string description)
        {
            _provider = () => TooltipContentBuilder.ForText(title, subtitle, description);
            _showImmediate = false;
        }

        public void SetProvider(Func<TooltipData> provider, bool showImmediate = false)
        {
            _provider = provider;
            _showImmediate = showImmediate;
        }

        public void Clear()
        {
            _provider = null;
            _showImmediate = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_provider == null) return;

            if (_showImmediate)
                TooltipUI.ScheduleShowImmediate(_provider);
            else
                TooltipUI.ScheduleShow(_provider);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipUI.Hide();
        }

        private void OnDisable()
        {
            if (TooltipUI.IsVisible)
                TooltipUI.Hide();
        }
    }
}
