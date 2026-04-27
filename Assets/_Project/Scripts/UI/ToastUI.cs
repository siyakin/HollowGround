using System.Collections;
using System.Collections.Generic;
using HollowGround.Core;
using TMPro;
using UnityEngine;

namespace HollowGround.UI
{
    public class ToastUI : Singleton<ToastUI>
    {

        [SerializeField] private float _displayDuration = 3f;
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private int _maxToasts = 5;

        private readonly Queue<ToastMessage> _queue = new();
        private readonly List<GameObject> _activeToasts = new();
        private bool _isShowing;
        private Transform _toastContainer;

        public struct ToastMessage
        {
            public string Text;
            public Color Color;
        }

        protected override void Awake()
        {
            base.Awake();

            var containerObj = new GameObject("ToastContainer", typeof(RectTransform));
            containerObj.transform.SetParent(transform, false);
            var containerRect = containerObj.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.85f);
            containerRect.anchorMax = new Vector2(0.5f, 0.85f);
            containerRect.pivot = new Vector2(0.5f, 1f);
            containerRect.sizeDelta = new Vector2(400f, 200f);
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.SetAsLastSibling();

            var cg = containerObj.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;

            var vlg = containerObj.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.spacing = 6;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            _toastContainer = containerObj.transform;
        }

        public static void Show(string text, Color? color = null)
        {
            if (Instance == null)
            {
                Debug.Log($"[Toast] {text}");
                return;
            }
            var msg = new ToastMessage { Text = text, Color = color ?? Color.white };
            Instance._queue.Enqueue(msg);
            Instance.TryShowNext();
        }

        private void TryShowNext()
        {
            if (_isShowing || _queue.Count == 0) return;
            if (_activeToasts.Count >= _maxToasts)
                RemoveOldestToast();

            var msg = _queue.Dequeue();
            StartCoroutine(ShowToastCoroutine(msg));
        }

        private IEnumerator ShowToastCoroutine(ToastMessage msg)
        {
            _isShowing = true;

            var toast = new GameObject("Toast", typeof(RectTransform));
            toast.transform.SetParent(_toastContainer, false);

            var layout = toast.AddComponent<UnityEngine.UI.LayoutElement>();
            layout.minWidth = 300;
            layout.minHeight = 34;
            layout.preferredWidth = 350;
            layout.preferredHeight = 34;

            var bg = toast.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            bg.raycastTarget = false;

            var textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(toast.transform, false);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 18;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = msg.Color;
            tmp.text = msg.Text;
            tmp.raycastTarget = false;
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(12, 4);
            textRect.offsetMax = new Vector2(-12, -4);

            var cg = toast.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            _activeToasts.Add(toast);

            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = elapsed / _fadeDuration;
                yield return null;
            }
            cg.alpha = 1f;

            yield return new WaitForSecondsRealtime(_displayDuration);

            elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = 1f - (elapsed / _fadeDuration);
                yield return null;
            }

            _activeToasts.Remove(toast);
            Destroy(toast);

            _isShowing = false;
            TryShowNext();
        }

        private void RemoveOldestToast()
        {
            if (_activeToasts.Count == 0) return;
            Destroy(_activeToasts[0]);
            _activeToasts.RemoveAt(0);
        }
    }
}
