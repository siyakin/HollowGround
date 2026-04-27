using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HollowGround.UI
{
    public class ToastUI : MonoBehaviour
    {
        [SerializeField] private float _displayDuration = 3f;
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private int _maxToasts = 5;

        private static ToastUI _instance;
        private readonly Queue<ToastMessage> _queue = new();
        private readonly List<GameObject> _activeToasts = new();
        private bool _isShowing;
        private Transform _toastContainer;

        public struct ToastMessage
        {
            public string Text;
            public Color Color;
        }

        private void Awake()
        {
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void Start()
        {
            EnsureContainer();
        }

        public static void Show(string text, Color? color = null)
        {
            if (_instance == null)
            {
                var found = FindAnyObjectByType<ToastUI>(FindObjectsInactive.Include);
                if (found != null)
                {
                    found.gameObject.SetActive(true);
                    _instance = found;
                    found.EnsureContainer();
                }
                else
                {
                    Debug.Log($"[Toast] {text}");
                    return;
                }
            }

            var msg = new ToastMessage { Text = text, Color = color ?? Color.white };
            _instance._queue.Enqueue(msg);
            _instance.TryShowNext();
        }

        private void EnsureContainer()
        {
            if (_toastContainer != null) return;

            Transform existing = transform.Find("ToastContainer");
            if (existing != null)
            {
                _toastContainer = existing;
                return;
            }

            var containerObj = new GameObject("ToastContainer", typeof(RectTransform));
            containerObj.transform.SetParent(transform, false);
            var cr = containerObj.GetComponent<RectTransform>();
            cr.anchorMin = new Vector2(0f, 0.5f);
            cr.anchorMax = new Vector2(1f, 0.85f);
            cr.pivot = new Vector2(0.5f, 1f);
            cr.offsetMin = Vector2.zero;
            cr.offsetMax = Vector2.zero;

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
            layout.minWidth = 200;
            layout.minHeight = 30;
            layout.preferredWidth = 350;
            layout.preferredHeight = 34;

            var bg = toast.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0.1f, 0.1f, 0.12f, 0.92f);
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
