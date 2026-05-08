using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HollowGround.UI
{
    public class ToastUI : MonoBehaviour
    {
        public static event System.Action<string, Color> OnToastShown;

        [SerializeField] private float _displayDuration = 3f;
        [SerializeField] private float _fadeDuration = 0.4f;
        [SerializeField] private float _slideDuration = 0.3f;
        [SerializeField] private int _maxVisibleToasts = 4;
        [SerializeField] private float _toastSpacing = 8f;

        private static ToastUI _instance;
        private readonly List<ActiveToast> _activeToasts = new();
        private Transform _toastContainer;
        private bool _initialized;

        private class ActiveToast
        {
            public GameObject GameObject;
            public RectTransform RectTransform;
            public CanvasGroup CanvasGroup;
            public Coroutine LifecycleCoroutine;
            public bool IsFadingOut;
        }

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
            _instance.CreateToast(msg);
            OnToastShown?.Invoke(text, msg.Color);
        }

        private void EnsureContainer()
        {
            if (_initialized) return;
            _initialized = true;

            Transform existing = transform.Find("ToastContainer");
            if (existing != null)
            {
                _toastContainer = existing;
                return;
            }

            var containerObj = new GameObject("ToastContainer", typeof(RectTransform));
            containerObj.transform.SetParent(transform, false);
            var cr = containerObj.GetComponent<RectTransform>();
            cr.anchorMin = new Vector2(0.5f, 1f);
            cr.anchorMax = new Vector2(0.5f, 1f);
            cr.pivot = new Vector2(0.5f, 1f);
            cr.sizeDelta = new Vector2(420f, 0f);
            cr.anchoredPosition = new Vector2(0f, -60f);

            var cg = containerObj.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;

            _toastContainer = containerObj.transform;
        }

        private void CreateToast(ToastMessage msg)
        {
            EnsureContainer();

            if (_activeToasts.Count >= _maxVisibleToasts)
                ForceRemoveOldest();

            var toastObj = new GameObject("Toast", typeof(RectTransform));
            toastObj.transform.SetParent(_toastContainer, false);

            var toastRect = toastObj.GetComponent<RectTransform>();
            toastRect.anchorMin = new Vector2(0f, 1f);
            toastRect.anchorMax = new Vector2(1f, 1f);
            toastRect.pivot = new Vector2(0.5f, 1f);
            toastRect.sizeDelta = new Vector2(0f, 36f);

            var bg = toastObj.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(UIColors.Default.PanelBg.r, UIColors.Default.PanelBg.g, UIColors.Default.PanelBg.b, 0.92f);
            bg.raycastTarget = false;

            var textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(toastObj.transform, false);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = msg.Color;
            tmp.text = msg.Text;
            tmp.raycastTarget = false;
            var theme = UIThemeManager.Instance?.CurrentTheme;
            if (theme != null && theme.defaultFont != null) tmp.font = theme.defaultFont;
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(14, 4);
            textRect.offsetMax = new Vector2(-14, -4);

            var cg = toastObj.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            var activeToast = new ActiveToast
            {
                GameObject = toastObj,
                RectTransform = toastRect,
                CanvasGroup = cg,
                IsFadingOut = false
            };

            _activeToasts.Add(activeToast);
            RepositionToasts();
            activeToast.LifecycleCoroutine = StartCoroutine(ToastLifecycle(activeToast));
        }

        private IEnumerator ToastLifecycle(ActiveToast toast)
        {
            float elapsed;

            elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                toast.CanvasGroup.alpha = Mathf.Clamp01(elapsed / _fadeDuration);
                yield return null;
            }
            toast.CanvasGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(_displayDuration);

            toast.IsFadingOut = true;
            elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                toast.CanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / _fadeDuration);
                yield return null;
            }

            RemoveToast(toast);
        }

        private void RemoveToast(ActiveToast toast)
        {
            _activeToasts.Remove(toast);
            if (toast.GameObject != null)
                Destroy(toast.GameObject);
            RepositionToasts();
        }

        private void ForceRemoveOldest()
        {
            if (_activeToasts.Count == 0) return;

            var oldest = _activeToasts[0];
            if (oldest.LifecycleCoroutine != null)
                StopCoroutine(oldest.LifecycleCoroutine);

            _activeToasts.RemoveAt(0);
            if (oldest.GameObject != null)
            {
                StartCoroutine(FadeOutAndDestroy(oldest));
            }
        }

        private IEnumerator FadeOutAndDestroy(ActiveToast toast)
        {
            float elapsed = 0f;
            float startAlpha = toast.CanvasGroup != null ? toast.CanvasGroup.alpha : 0f;
            while (elapsed < _fadeDuration * 0.5f)
            {
                elapsed += Time.unscaledDeltaTime;
                if (toast.CanvasGroup != null)
                    toast.CanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / (_fadeDuration * 0.5f));
                yield return null;
            }
            if (toast.GameObject != null)
                Destroy(toast.GameObject);
        }

        private void RepositionToasts()
        {
            float currentY = 0f;
            for (int i = _activeToasts.Count - 1; i >= 0; i--)
            {
                var toast = _activeToasts[i];
                if (toast.RectTransform == null) continue;

                float targetY = -currentY;
                if (Mathf.Abs(toast.RectTransform.anchoredPosition.y - targetY) > 0.5f)
                {
                    if (toast.RectTransform.anchoredPosition.y != 0f || currentY == 0f)
                        StartCoroutine(SlideTo(toast.RectTransform, targetY));
                    else
                        toast.RectTransform.anchoredPosition = new Vector2(0f, targetY);
                }

                currentY += 36f + _toastSpacing;
            }
        }

        private IEnumerator SlideTo(RectTransform rect, float targetY)
        {
            float startY = rect.anchoredPosition.y;
            float elapsed = 0f;
            while (elapsed < _slideDuration && rect != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _slideDuration;
                t = t * t * (3f - 2f * t);
                rect.anchoredPosition = new Vector2(0f, Mathf.Lerp(startY, targetY, t));
                yield return null;
            }
            if (rect != null)
                rect.anchoredPosition = new Vector2(0f, targetY);
        }
    }
}
