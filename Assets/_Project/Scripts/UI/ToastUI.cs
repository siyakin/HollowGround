using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HollowGround.UI
{
    public class ToastUI : MonoBehaviour
    {
        public static ToastUI Instance { get; private set; }

        [SerializeField] private float _displayDuration = 3f;
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private int _maxToasts = 5;
        [SerializeField] private GameObject _toastPrefab;
        [SerializeField] private Transform _toastContainer;

        private readonly Queue<ToastMessage> _queue = new();
        private readonly List<GameObject> _activeToasts = new();
        private bool _isShowing;

        public struct ToastMessage
        {
            public string Text;
            public Color Color;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public static void Show(string text, Color? color = null)
        {
            if (Instance == null) return;
            var msg = new ToastMessage
            {
                Text = text,
                Color = color ?? Color.white
            };
            Instance._queue.Enqueue(msg);
            Instance.TryShowNext();
        }

        public static void Show(string text) => Show(text, Color.white);

        private void TryShowNext()
        {
            if (_isShowing || _queue.Count == 0) return;

            if (_activeToasts.Count >= _maxToasts)
            {
                RemoveOldestToast();
            }

            var msg = _queue.Dequeue();
            StartCoroutine(ShowToastCoroutine(msg));
        }

        private IEnumerator ShowToastCoroutine(ToastMessage msg)
        {
            _isShowing = true;

            GameObject toast = Instantiate(_toastPrefab, _toastContainer);
            _activeToasts.Add(toast);

            var textComponent = toast.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = msg.Text;
                textComponent.color = msg.Color;
            }

            CanvasGroup cg = toast.GetComponent<CanvasGroup>();
            if (cg == null) cg = toast.AddComponent<CanvasGroup>();

            cg.alpha = 0f;

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
            GameObject oldest = _activeToasts[0];
            _activeToasts.RemoveAt(0);
            Destroy(oldest);
        }
    }
}
