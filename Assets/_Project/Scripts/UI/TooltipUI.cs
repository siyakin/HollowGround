using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class TooltipUI : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float _showDelay = 0.4f;
        [SerializeField] private float _screenPadding = 16f;

        [Header("Container (auto-built if null)")]
        [SerializeField] private RectTransform _container;

        private float _hoverTimer;
        private bool _isShowing;
        private bool _pendingShow;
        private System.Func<TooltipData> _pendingDataProvider;

        private static TooltipUI _instance;
        public static bool IsVisible => _instance != null && _instance._isShowing;

        private void Awake()
        {
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void Update()
        {
            if (_pendingShow)
            {
                _hoverTimer += Time.unscaledDeltaTime;
                float delay = Core.GameConfig.Instance != null ? Core.GameConfig.Instance.TooltipShowDelay : _showDelay;
                if (_hoverTimer >= delay)
                {
                    _pendingShow = false;
                    if (_pendingDataProvider != null)
                    {
                        var data = _pendingDataProvider();
                        if (data != null)
                            Show(data);
                    }
                }
            }

            if (_isShowing)
                UpdatePosition();
        }

        public static void ScheduleShow(System.Func<TooltipData> dataProvider)
        {
            EnsureInstance();
            if (_instance == null) return;
            _instance.Cancel();
            _instance._pendingShow = true;
            _instance._hoverTimer = 0f;
            _instance._pendingDataProvider = dataProvider;
        }

        public static void ScheduleShowImmediate(System.Func<TooltipData> dataProvider)
        {
            EnsureInstance();
            if (_instance == null) return;
            _instance.Cancel();
            var data = dataProvider();
            if (data != null)
                _instance.Show(data);
        }

        public static void Hide()
        {
            if (_instance == null) return;
            _instance.Cancel();
            if (_instance._container != null)
                _instance._container.gameObject.SetActive(false);
            _instance._isShowing = false;
        }

        private static void EnsureInstance()
        {
            if (_instance != null) return;

            _instance = FindAnyObjectByType<TooltipUI>(FindObjectsInactive.Include);
            if (_instance != null) return;

            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            var go = new GameObject(nameof(TooltipUI));
            go.transform.SetParent(canvas.transform, false);

            var rootRt = go.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.pivot = new Vector2(0.5f, 0.5f);
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            var rootCg = go.AddComponent<CanvasGroup>();
            rootCg.blocksRaycasts = false;
            rootCg.interactable = false;

            _instance = go.AddComponent<TooltipUI>();
            _instance.BuildContainer();
        }

        private void BuildContainer()
        {
            var containerObj = new GameObject("Container");
            containerObj.transform.SetParent(transform, false);

            _container = containerObj.AddComponent<RectTransform>();
            _container.pivot = new Vector2(0f, 1f);
            _container.anchorMin = new Vector2(0f, 1f);
            _container.anchorMax = new Vector2(0f, 1f);
            _container.sizeDelta = new Vector2(300f, 100f);

            var bg = containerObj.AddComponent<Image>();
            bg.color = UIColors.Default.PanelBg;
            bg.raycastTarget = false;

            var outline = containerObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.32f, 0.35f, 0.40f, 0.9f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var containerCg = containerObj.AddComponent<CanvasGroup>();
            containerCg.blocksRaycasts = false;
            containerCg.interactable = false;

            containerObj.AddComponent<LayoutElement>().preferredWidth = 340f;
            containerObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var vlg = containerObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(16, 16, 12, 12);
            vlg.spacing = 3f;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            containerObj.SetActive(false);
        }

        private void Cancel()
        {
            _pendingShow = false;
            _hoverTimer = 0f;
            _pendingDataProvider = null;
        }

        private void Show(TooltipData data)
        {
            if (_container == null) BuildContainer();

            BuildContent(data);
            _container.gameObject.SetActive(true);
            _isShowing = true;
            UpdatePosition();
        }

        private void BuildContent(TooltipData data)
        {
            ClearContent();

            if (!string.IsNullOrEmpty(data.Title))
                AddLabel(data.Title, 16, data.HasTitleColor ? data.TitleColor : UIColors.Default.Text, FontStyles.Bold);

            if (!string.IsNullOrEmpty(data.Subtitle))
                AddLabel(data.Subtitle, 13, UIColors.Default.Muted, FontStyles.Normal);

            if (data.HasState)
                AddLabel(data.StateText, 13, data.StateColor, FontStyles.Bold);

            if (!string.IsNullOrEmpty(data.Description))
            {
                AddSpacer(4);
                AddLabel(data.Description, 13, UIColors.Default.Text, FontStyles.Normal);
            }

            if (data.Costs.Count > 0)
            {
                AddSpacer(4);
                AddLabel("Cost:", 12, UIColors.Default.Muted, FontStyles.Bold);
                foreach (var cost in data.Costs)
                {
                    bool enough = !cost.ShowHave || cost.Have >= cost.Amount;
                    var color = enough ? UIColors.GetResourceColor(cost.Type) : UIColors.Default.Danger;
                    string haveText = cost.ShowHave ? $" ({cost.Have}/{cost.Amount})" : $" x{cost.Amount}";
                    AddLabel($"  {cost.Type}{haveText}", 12, color, FontStyles.Normal);
                }
            }

            if (data.InfoLines.Count > 0)
            {
                AddSpacer(4);
                foreach (var line in data.InfoLines)
                    AddLabel(line, 12, UIColors.TextDim, FontStyles.Normal);
            }
        }

        private TMP_Text AddLabel(string text, float size, Color color, FontStyles style)
        {
            var tmp = UIPrimitiveFactory.AddText(_container, text, size, color, TextAlignmentOptions.MidlineLeft);
            tmp.fontStyle = style;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        private void AddSpacer(float height)
        {
            var spacer = UIPrimitiveFactory.CreateUIObject("Spacer", _container);
            var le = spacer.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
        }

        private void ClearContent()
        {
            if (_container == null) return;
            for (int i = _container.childCount - 1; i >= 0; i--)
                Destroy(_container.GetChild(i).gameObject);
        }

        private void UpdatePosition()
        {
            if (_container == null) return;

            var mousePos = UnityEngine.InputSystem.Mouse.current?.position.ReadValue() ?? Vector2.zero;
            float x = mousePos.x + 18f;
            float y = mousePos.y - 12f;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_container);
            float width = _container.rect.width;
            float height = _container.rect.height;

            float screenW = Screen.width;
            float screenH = Screen.height;

            if (x + width > screenW - _screenPadding)
                x = mousePos.x - width - 18f;
            if (y - height < _screenPadding)
                y = mousePos.y + 18f;
            if (x < _screenPadding)
                x = _screenPadding;
            if (y > screenH - _screenPadding)
                y = screenH - _screenPadding;

            _container.position = new Vector2(x, y);
        }
    }
}
