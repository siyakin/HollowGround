using HollowGround.Core;
using TMPro;
using UnityEngine;

namespace HollowGround.UI
{
    public class TimeDisplayUI : MonoBehaviour
    {
        private TMP_Text _text;

        private void Start()
        {
            var bg = gameObject.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
            bg.raycastTarget = false;

            var child = new GameObject("Text", typeof(RectTransform));
            child.transform.SetParent(transform, false);
            var cr = child.GetComponent<RectTransform>();
            cr.anchorMin = Vector2.zero;
            cr.anchorMax = Vector2.one;
            cr.offsetMin = Vector2.zero;
            cr.offsetMax = Vector2.zero;

            var canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            _text = child.AddComponent<TextMeshProUGUI>();
            _text.fontSize = 20;
            _text.alignment = TextAlignmentOptions.Center;
            _text.color = new Color(0.8f, 0.9f, 1f);
            _text.raycastTarget = false;
        }

        private void Update()
        {
            if (_text == null || TimeManager.Instance == null) return;
            int t = Mathf.FloorToInt(TimeManager.Instance.GameTime);
            int h = t / 3600;
            int m = (t % 3600) / 60;
            int s = t % 60;
            _text.text = $"x{TimeManager.Instance.GameSpeed}  {h:D2}:{m:D2}:{s:D2}";
        }
    }
}
