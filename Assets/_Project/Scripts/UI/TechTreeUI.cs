using System.Collections.Generic;
using System.Linq;
using HollowGround.Resources;
using HollowGround.Tech;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class TechTreeUI : MonoBehaviour
    {
        private static readonly Color ColorPanelBg = new(0.08f, 0.09f, 0.11f, 0.92f);
        private static readonly Color ColorPanelInner = new(0.14f, 0.15f, 0.17f, 1f);
        private static readonly Color ColorColumnBg = new(0.11f, 0.12f, 0.14f, 1f);
        private static readonly Color ColorCardLocked = new(0.18f, 0.18f, 0.2f, 1f);
        private static readonly Color ColorCardAvailable = new(0.22f, 0.3f, 0.4f, 1f);
        private static readonly Color ColorCardResearching = new(0.45f, 0.35f, 0.15f, 1f);
        private static readonly Color ColorCardCompleted = new(0.2f, 0.45f, 0.22f, 1f);
        private static readonly Color ColorTextPrimary = new(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color ColorTextMuted = new(0.65f, 0.65f, 0.7f, 1f);
        private static readonly Color ColorTextDim = new(0.45f, 0.45f, 0.48f, 1f);
        private static readonly Color ColorOk = new(0.35f, 0.8f, 0.4f, 1f);

        private readonly Dictionary<TechCategory, Color> _categoryColors = new()
        {
            { TechCategory.Construction, new Color(0.5f, 0.55f, 0.7f) },
            { TechCategory.Agriculture,  new Color(0.4f, 0.7f, 0.35f) },
            { TechCategory.Military,     new Color(0.8f, 0.35f, 0.3f) },
            { TechCategory.Medicine,     new Color(0.3f, 0.7f, 0.75f) },
            { TechCategory.Exploration,  new Color(0.75f, 0.55f, 0.2f) },
        };

        private TMP_FontAsset _themeFont;

        private TMP_FontAsset ThemeFont
        {
            get
            {
                if (_themeFont != null) return _themeFont;
#if UNITY_EDITOR
                var theme = UnityEditor.AssetDatabase.LoadAssetAtPath<UIThemeSO>("Assets/_Project/ScriptableObjects/UITheme.asset");
#else
                var theme = UnityEngine.Resources.LoadAll<UIThemeSO>("").Length > 0
                    ? UnityEngine.Resources.LoadAll<UIThemeSO>("")[0] : null;
#endif
                if (theme != null && theme.defaultFont != null)
                    _themeFont = theme.defaultFont;
                return _themeFont;
            }
        }

        private RectTransform _root;
        private RectTransform _columnsContainer;
        private RectTransform _detailPanel;
        private TMP_Text _detailName;
        private TMP_Text _detailCategory;
        private TMP_Text _detailDesc;
        private TMP_Text _detailCost;
        private TMP_Text _detailBonuses;
        private TMP_Text _detailPrereqs;
        private Button _detailResearchBtn;
        private TMP_Text _detailResearchBtnText;

        private readonly Dictionary<TechNode, TechCardView> _cards = new();
        private TechNode _selectedTech;
        private List<TechNode> _allTechs = new();
        private bool _built;

        private class TechCardView
        {
            public TechNode Data;
            public Image Background;
            public TMP_Text NameText;
            public TMP_Text StatusText;
            public Image ProgressFill;
            public RectTransform ProgressBar;
            public Button Button;
        }

        private void OnEnable()
        {
            if (!_built) BuildUI();
            ReloadTechs();
            RefreshAll();

            if (ResearchManager.Instance != null)
            {
                ResearchManager.Instance.OnResearchStarted += HandleResearchStarted;
                ResearchManager.Instance.OnResearchCompleted += HandleResearchCompleted;
            }
        }

        private void OnDisable()
        {
            if (ResearchManager.Instance != null)
            {
                ResearchManager.Instance.OnResearchStarted -= HandleResearchStarted;
                ResearchManager.Instance.OnResearchCompleted -= HandleResearchCompleted;
            }
        }

        private void Update()
        {
            if (ResearchManager.Instance == null || !ResearchManager.Instance.IsResearching) return;
            var active = ResearchManager.Instance.CurrentResearch;
            if (active != null && _cards.TryGetValue(active, out var view))
            {
                UpdateCardVisual(view);
            }
        }

        private void HandleResearchStarted(TechNode node) => RefreshAll();
        private void HandleResearchCompleted(TechNode node) => RefreshAll();

        private void ReloadTechs()
        {
            _allTechs = UnityEngine.Resources.LoadAll<TechNode>("TechNodes").ToList();
        }

        // === PART 2: BuildUI ===

        private void BuildUI()
        {
            _root = GetComponent<RectTransform>();
            if (_root == null)
            {
                Debug.LogError("[TechTreeUI] Must be on a RectTransform (UI Canvas child).");
                return;
            }

            _root.offsetMin = new Vector2(0f, 60f);
            _root.offsetMax = new Vector2(0f, 0f);

            foreach (Transform child in _root)
                Destroy(child.gameObject);

            var bg = CreateUIObject("Background", _root);
            StretchFull(bg);
            AddImage(bg, ColorPanelBg);

            var cg = gameObject.GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.alpha = 1f;

            var header = CreateUIObject("Header", _root);
            SetAnchors(header, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            header.anchoredPosition = new Vector2(0, -30);
            header.sizeDelta = new Vector2(-40, 50);
            var titleLabel = AddThemedText(header, "TECHNOLOGY TREE", 28, TextAlignmentOptions.Center, ColorTextPrimary);
            StretchFull(titleLabel.rectTransform);

            _columnsContainer = CreateUIObject("Columns", _root);
            SetAnchors(_columnsContainer, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
            _columnsContainer.offsetMin = new Vector2(40, 30);
            _columnsContainer.offsetMax = new Vector2(-420, -80);
            var hlg = _columnsContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.UpperLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            _detailPanel = CreateUIObject("DetailPanel", _root);
            SetAnchors(_detailPanel, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f));
            _detailPanel.offsetMin = new Vector2(-400, 30);
            _detailPanel.offsetMax = new Vector2(-20, -80);
            AddImage(_detailPanel, ColorPanelInner);
            BuildDetailPanel();

            _built = true;
        }

        private void BuildColumns()
        {
            foreach (Transform child in _columnsContainer)
                Destroy(child.gameObject);

            _cards.Clear();

            foreach (TechCategory category in System.Enum.GetValues(typeof(TechCategory)))
            {
                var column = CreateUIObject($"Col_{category}", _columnsContainer);
                AddImage(column, ColorColumnBg);

                var vlg = column.gameObject.AddComponent<VerticalLayoutGroup>();
                vlg.spacing = 6;
                vlg.padding = new RectOffset(8, 8, 8, 8);
                vlg.childAlignment = TextAnchor.UpperCenter;
                vlg.childControlWidth = true;
                vlg.childControlHeight = false;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;

                var header = CreateUIObject("Header", column);
                var headerLE = header.gameObject.AddComponent<LayoutElement>();
                headerLE.minHeight = 32;
                headerLE.preferredHeight = 32;
                Color catColor = _categoryColors.TryGetValue(category, out var cc) ? cc : Color.gray;
                AddImage(header, catColor);
                var headerText = AddThemedText(header, category.ToString().ToUpper(), 16, TextAlignmentOptions.Center, Color.white);
                StretchFull(headerText.rectTransform);

                var techsInCat = _allTechs
                    .Where(t => t.Category == category)
                    .OrderBy(t => t.Prerequisites.Count)
                    .ThenBy(t => t.DisplayName)
                    .ToList();

                foreach (var tech in techsInCat)
                    BuildCard(column, tech);
            }
        }

        private void BuildCard(RectTransform parent, TechNode tech)
        {
            var card = CreateUIObject($"Card_{tech.name}", parent);
            var le = card.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 80;
            le.preferredHeight = 80;

            var bg = AddImage(card, ColorCardLocked);

            var btn = card.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => SelectTech(tech));

            var nameText = AddThemedText(card, tech.DisplayName, 15, TextAlignmentOptions.TopLeft, ColorTextPrimary);
            var nameRt = nameText.rectTransform;
            SetAnchors(nameRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            nameRt.anchoredPosition = new Vector2(0, -4);
            nameRt.sizeDelta = new Vector2(-12, 22);

            var statusText = AddThemedText(card, "", 13, TextAlignmentOptions.MidlineLeft, ColorTextMuted);
            var statusRt = statusText.rectTransform;
            SetAnchors(statusRt, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f));
            statusRt.anchoredPosition = new Vector2(0, -2);
            statusRt.sizeDelta = new Vector2(-12, 18);

            var progressBar = CreateUIObject("ProgressBar", card);
            SetAnchors(progressBar, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
            progressBar.anchoredPosition = new Vector2(0, 8);
            progressBar.sizeDelta = new Vector2(-12, 6);
            AddImage(progressBar, new Color(0, 0, 0, 0.4f));

            var progressFillContainer = CreateUIObject("Fill", progressBar);
            SetAnchors(progressFillContainer, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f));
            progressFillContainer.anchoredPosition = Vector2.zero;
            progressFillContainer.sizeDelta = new Vector2(0, 0);
            var fillImg = AddImage(progressFillContainer, ColorOk);

            _cards[tech] = new TechCardView
            {
                Data = tech,
                Background = bg,
                NameText = nameText,
                StatusText = statusText,
                ProgressFill = fillImg,
                ProgressBar = progressBar,
                Button = btn
            };
        }

        private void BuildDetailPanel()
        {
            _detailName = AddThemedText(_detailPanel, "Select a technology", 22, TextAlignmentOptions.TopLeft, ColorTextPrimary);
            var nameRt = _detailName.rectTransform;
            SetAnchors(nameRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            nameRt.anchoredPosition = new Vector2(0, -15);
            nameRt.sizeDelta = new Vector2(-24, 30);

            _detailCategory = AddThemedText(_detailPanel, "", 14, TextAlignmentOptions.TopLeft, ColorTextMuted);
            var catRt = _detailCategory.rectTransform;
            SetAnchors(catRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            catRt.anchoredPosition = new Vector2(0, -48);
            catRt.sizeDelta = new Vector2(-24, 20);

            _detailDesc = AddThemedText(_detailPanel, "", 15, TextAlignmentOptions.TopLeft, ColorTextPrimary);
            var descRt = _detailDesc.rectTransform;
            SetAnchors(descRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            descRt.anchoredPosition = new Vector2(0, -78);
            descRt.sizeDelta = new Vector2(-24, 60);

            _detailCost = AddThemedText(_detailPanel, "", 15, TextAlignmentOptions.TopLeft, ColorTextMuted);
            var costRt = _detailCost.rectTransform;
            SetAnchors(costRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            costRt.anchoredPosition = new Vector2(0, -150);
            costRt.sizeDelta = new Vector2(-24, 80);

            _detailResearchBtn = CreateButton(_detailPanel, "ResearchBtn", "START RESEARCH", StartResearchFromDetail);
            var btnRt = _detailResearchBtn.GetComponent<RectTransform>();
            SetAnchors(btnRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            btnRt.anchoredPosition = new Vector2(0, -250);
            btnRt.sizeDelta = new Vector2(-24, 50);
            _detailResearchBtnText = _detailResearchBtn.GetComponentInChildren<TextMeshProUGUI>();
            var confirmImg = _detailResearchBtn.GetComponent<Image>();
            if (confirmImg != null) confirmImg.color = ColorOk;

            _detailBonuses = AddThemedText(_detailPanel, "", 15, TextAlignmentOptions.TopLeft, ColorTextPrimary);
            var bonRt = _detailBonuses.rectTransform;
            SetAnchors(bonRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            bonRt.anchoredPosition = new Vector2(0, -310);
            bonRt.sizeDelta = new Vector2(-24, 100);

            _detailPrereqs = AddThemedText(_detailPanel, "", 14, TextAlignmentOptions.TopLeft, ColorTextDim);
            var prereqRt = _detailPrereqs.rectTransform;
            SetAnchors(prereqRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            prereqRt.anchoredPosition = new Vector2(0, -420);
            prereqRt.sizeDelta = new Vector2(-24, 60);
        }

        // === PART 3: Refresh + Card ===

        private void RefreshAll()
        {
            if (!_built) return;
            if (_cards.Count == 0 || _cards.Count != _allTechs.Count)
                BuildColumns();

            foreach (var view in _cards.Values)
                UpdateCardVisual(view);

            UpdateDetailPanel();
        }

        private void UpdateCardVisual(TechCardView view)
        {
            if (view == null || view.Data == null) return;
            var tech = view.Data;

            bool canStart = ResearchManager.Instance != null &&
                            ResearchManager.Instance.CanStartResearch(tech);
            bool isResearched = tech.IsResearched;
            bool isResearching = tech.IsResearching;

            Color bg;
            string status;
            if (isResearched) { bg = ColorCardCompleted; status = "Completed"; }
            else if (isResearching) { bg = ColorCardResearching; status = $"Researching {tech.ResearchProgress:P0}"; }
            else if (canStart) { bg = ColorCardAvailable; status = "Available"; }
            else { bg = ColorCardLocked; status = tech.CanResearch() ? "Insufficient resources" : "Locked"; }

            if (view.Background != null) view.Background.color = bg;
            if (view.NameText != null)
                view.NameText.color = (isResearched || isResearching || canStart) ? ColorTextPrimary : ColorTextDim;
            if (view.StatusText != null) view.StatusText.text = status;

            if (view.ProgressBar != null)
            {
                view.ProgressBar.gameObject.SetActive(isResearching);
                if (isResearching && view.ProgressFill != null)
                {
                    var fillRt = view.ProgressFill.rectTransform;
                    fillRt.anchorMax = new Vector2(Mathf.Clamp01(tech.ResearchProgress), 1);
                }
            }
        }

        private void SelectTech(TechNode tech)
        {
            _selectedTech = tech;
            UpdateDetailPanel();
        }

        private void UpdateDetailPanel()
        {
            if (_selectedTech == null)
            {
                _detailName.text = "Select a technology";
                _detailCategory.text = "";
                _detailDesc.text = "Click a tech card to see full details and start research.";
                _detailCost.text = "";
                _detailBonuses.text = "";
                _detailPrereqs.text = "";
                _detailResearchBtn.gameObject.SetActive(false);
                return;
            }

            var tech = _selectedTech;
            _detailName.text = tech.DisplayName;

            Color catColor = _categoryColors.TryGetValue(tech.Category, out var cc) ? cc : Color.gray;
            string catHex = ColorUtility.ToHtmlStringRGB(catColor);
            _detailCategory.text = $"<color=#{catHex}>{tech.Category.ToString().ToUpper()}</color>   Tier {Mathf.Max(1, tech.Prerequisites.Count + 1)}";

            _detailDesc.text = string.IsNullOrEmpty(tech.Description) ? "" : tech.Description;

            var costSb = new System.Text.StringBuilder();
            costSb.AppendLine("<b>COST</b>");
            var costs = tech.GetCost();
            if (costs.Count == 0) costSb.AppendLine("<color=#A6A6AE>Free</color>");
            foreach (var kvp in costs)
            {
                int have = ResourceManager.Instance != null ? ResourceManager.Instance.Get(kvp.Key) : 0;
                string colorTag = have >= kvp.Value ? "#C8C8C8" : "#E64D4D";
                costSb.AppendLine($"<color={colorTag}>{kvp.Key}: {kvp.Value} <size=10>(have {have})</size></color>");
            }
            costSb.AppendLine($"<color=#A6A6AE>Time: {tech.ResearchTime:F0}s</color>");
            _detailCost.text = costSb.ToString();

            var bonSb = new System.Text.StringBuilder();
            bonSb.AppendLine("<b>BONUSES</b>");
            bool hasBonus = false;
            if (tech.ProductionBonus > 0) { bonSb.AppendLine($"  Production +{tech.ProductionBonus:P0}"); hasBonus = true; }
            if (tech.TrainingSpeedBonus > 0) { bonSb.AppendLine($"  Training speed +{tech.TrainingSpeedBonus:P0}"); hasBonus = true; }
            if (tech.ExpeditionSpeedBonus > 0) { bonSb.AppendLine($"  Expedition speed +{tech.ExpeditionSpeedBonus:P0}"); hasBonus = true; }
            if (tech.DefenseBonus > 0) { bonSb.AppendLine($"  Defense +{tech.DefenseBonus:P0}"); hasBonus = true; }
            if (!hasBonus) bonSb.AppendLine("<color=#A6A6AE>  No passive bonuses.</color>");
            _detailBonuses.text = bonSb.ToString();

            var prereqSb = new System.Text.StringBuilder();
            if (tech.Prerequisites.Count > 0)
            {
                prereqSb.AppendLine("<b>REQUIRES</b>");
                foreach (var pr in tech.Prerequisites)
                {
                    if (pr == null) continue;
                    string check = pr.IsResearched ? "<color=#59CC66>[OK]</color>" : "<color=#E64D4D>[X]</color>";
                    prereqSb.AppendLine($"  {check} {pr.DisplayName}");
                }
            }
            _detailPrereqs.text = prereqSb.ToString();

            bool isResearched = tech.IsResearched;
            bool isResearching = tech.IsResearching;
            bool canStart = ResearchManager.Instance != null &&
                            ResearchManager.Instance.CanStartResearch(tech);

            var dbgCosts = tech.GetCost();
            string dbgCost = string.Join(", ", dbgCosts.Select(c => $"{c.Key}={c.Value}"));
            string dbgHave = ResourceManager.Instance != null
                ? string.Join(", ", dbgCosts.Select(c => $"{c.Key}={ResourceManager.Instance.Get(c.Key)}"))
                : "RM null";
            Debug.Log($"[TechTree] {tech.DisplayName} | Cost: {dbgCost} | Have: {dbgHave} | canStart={canStart}");

            _detailResearchBtn.gameObject.SetActive(true);
            _detailResearchBtn.interactable = canStart;
            var btnImg = _detailResearchBtn.GetComponent<Image>();
            if (btnImg != null)
                btnImg.color = canStart ? ColorOk : new Color(0.3f, 0.3f, 0.32f, 1f);

            if (_detailResearchBtnText != null)
            {
                if (isResearched) _detailResearchBtnText.text = "COMPLETED";
                else if (isResearching) _detailResearchBtnText.text = "IN PROGRESS...";
                else if (!tech.CanResearch()) _detailResearchBtnText.text = "PREREQS NOT MET";
                else if (!canStart) _detailResearchBtnText.text = "INSUFFICIENT RESOURCES";
                else _detailResearchBtnText.text = "START RESEARCH";
            }
        }

        public void StartResearchFromDetail()
        {
            if (_selectedTech == null) return;
            if (ResearchManager.Instance == null)
            {
                ToastUI.Show("Research system not available!");
                return;
            }

            if (ResearchManager.Instance.IsResearching)
            {
                ToastUI.Show("Another research is already in progress!");
                return;
            }

            if (ResearchManager.Instance.StartResearch(_selectedTech))
            {
                ToastUI.Show($"Research started: {_selectedTech.DisplayName}");
                RefreshAll();
            }
            else
            {
                ToastUI.Show("Cannot start research!");
            }
        }

        // === PART 4: UI primitives ===

        private static RectTransform CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
            return rt;
        }

        private static Image AddImage(RectTransform rt, Color color)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = true;
            return img;
        }

        private static TMP_Text AddText(RectTransform parent, string text, float fontSize,
            TextAlignmentOptions alignment, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.richText = true;
            tmp.raycastTarget = false;
            return tmp;
        }

        private TMP_Text AddThemedText(RectTransform parent, string text, float fontSize,
            TextAlignmentOptions alignment, Color color)
        {
            var tmp = AddText(parent, text, fontSize, alignment, color);
            if (ThemeFont != null) tmp.font = ThemeFont;
            return tmp;
        }

        private Button CreateButton(Transform parent, string name, string label, System.Action onClick)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.25f, 0.27f, 0.32f, 1f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = colors;

            var txtGo = new GameObject("Label", typeof(RectTransform));
            txtGo.transform.SetParent(go.transform, false);
            var txtRt = txtGo.GetComponent<RectTransform>();
            txtRt.localScale = Vector3.one;
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;
            var tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            tmp.raycastTarget = false;
            if (ThemeFont != null) tmp.font = ThemeFont;

            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }

        private static void StretchFull(RectTransform rt, Vector2? offsetMin = null, Vector2? offsetMax = null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = offsetMin ?? Vector2.zero;
            rt.offsetMax = offsetMax ?? Vector2.zero;
        }

        private static void SetAnchors(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
        }
    }
}

