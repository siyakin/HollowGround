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
        private static readonly Color ColorColumnBg = new(0.11f, 0.12f, 0.14f, 1f);
        private static readonly Color ColorCardLocked = new(0.18f, 0.18f, 0.2f, 1f);
        private static readonly Color ColorCardAvailable = new(0.22f, 0.3f, 0.4f, 1f);
        private static readonly Color ColorCardResearching = new(0.45f, 0.35f, 0.15f, 1f);
        private static readonly Color ColorCardCompleted = new(0.2f, 0.45f, 0.22f, 1f);

        private readonly Dictionary<TechCategory, Color> _categoryColors = new()
        {
            { TechCategory.Construction, new Color(0.5f, 0.55f, 0.7f) },
            { TechCategory.Agriculture,  new Color(0.4f, 0.7f, 0.35f) },
            { TechCategory.Military,     new Color(0.8f, 0.35f, 0.3f) },
            { TechCategory.Medicine,     new Color(0.3f, 0.7f, 0.75f) },
            { TechCategory.Exploration,  new Color(0.75f, 0.55f, 0.2f) },
        };

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

        private void BuildUI()
        {
            _root = GetComponent<RectTransform>();
            if (_root == null)
            {
                Debug.LogError("[TechTreeUI] Must be on a RectTransform (UI Canvas child).");
                return;
            }

            UIPrimitiveFactory.StretchFull(_root, new Vector2(0f, 60f), Vector2.zero);

            foreach (Transform child in _root)
                Destroy(child.gameObject);

            var bg = UIPrimitiveFactory.CreateUIObject("Background", _root);
            UIPrimitiveFactory.StretchFull(bg);
            UIPrimitiveFactory.AddImage(bg, UIColors.Default.PanelBg);

            var cg = gameObject.GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.alpha = 1f;

            var header = UIPrimitiveFactory.CreateUIObject("Header", _root);
            UIPrimitiveFactory.SetAnchors(header, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            header.anchoredPosition = new Vector2(0, -30);
            header.sizeDelta = new Vector2(-40, 50);
            var titleLabel = UIPrimitiveFactory.AddThemedText(header, "TECHNOLOGY TREE", 28, UIColors.Default.Text, TextAlignmentOptions.Center);
            UIPrimitiveFactory.StretchFull(titleLabel.rectTransform);

            _columnsContainer = UIPrimitiveFactory.CreateUIObject("Columns", _root);
            UIPrimitiveFactory.SetAnchors(_columnsContainer, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
            _columnsContainer.offsetMin = new Vector2(40, 30);
            _columnsContainer.offsetMax = new Vector2(-420, -80);
            var hlg = _columnsContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.UpperLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            _detailPanel = UIPrimitiveFactory.CreateUIObject("DetailPanel", _root);
            UIPrimitiveFactory.SetAnchors(_detailPanel, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f));
            _detailPanel.offsetMin = new Vector2(-400, 30);
            _detailPanel.offsetMax = new Vector2(-20, -80);
            UIPrimitiveFactory.AddImage(_detailPanel, UIColors.PanelInner);
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
                var column = UIPrimitiveFactory.CreateUIObject($"Col_{category}", _columnsContainer);
                UIPrimitiveFactory.AddImage(column, ColorColumnBg);

                var vlg = column.gameObject.AddComponent<VerticalLayoutGroup>();
                vlg.spacing = 6;
                vlg.padding = new RectOffset(8, 8, 8, 8);
                vlg.childAlignment = TextAnchor.UpperCenter;
                vlg.childControlWidth = true;
                vlg.childControlHeight = false;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;

                var colHeader = UIPrimitiveFactory.CreateUIObject("Header", column);
                UIPrimitiveFactory.AddLayoutElement(colHeader.gameObject, minHeight: 32, preferredHeight: 32);
                Color catColor = _categoryColors.TryGetValue(category, out var cc) ? cc : Color.gray;
                UIPrimitiveFactory.AddImage(colHeader, catColor);
                var headerText = UIPrimitiveFactory.AddText(colHeader, category.ToString().ToUpper(), 16, UIColors.ContrastText(catColor), TextAlignmentOptions.Center);
                var theme = UIThemeManager.Instance?.CurrentTheme;
                if (theme != null)
                {
                    if (theme.defaultFont != null) headerText.font = theme.defaultFont;
                    if (theme.headerStyle != null)
                    {
                        headerText.fontSize = theme.headerStyle.fontSize;
                        headerText.fontStyle = theme.headerStyle.fontStyle;
                    }
                }
                UIPrimitiveFactory.StretchFull(headerText.rectTransform);

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
            var card = UIPrimitiveFactory.CreateUIObject($"Card_{tech.name}", parent);
            UIPrimitiveFactory.AddLayoutElement(card.gameObject, minHeight: 80, preferredHeight: 80);

            var bg = UIPrimitiveFactory.AddImage(card, ColorCardLocked);

            var btn = card.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => SelectTech(tech));

            var nameText = UIPrimitiveFactory.AddThemedText(card, tech.DisplayName, 15, UIColors.Default.Text, TextAlignmentOptions.TopLeft);
            var nameRt = nameText.rectTransform;
            UIPrimitiveFactory.SetAnchors(nameRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            nameRt.anchoredPosition = new Vector2(0, -4);
            nameRt.sizeDelta = new Vector2(-12, 22);

            var statusText = UIPrimitiveFactory.AddThemedText(card, "", 13, UIColors.Default.Muted, TextAlignmentOptions.MidlineLeft);
            var statusRt = statusText.rectTransform;
            UIPrimitiveFactory.SetAnchors(statusRt, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f));
            statusRt.anchoredPosition = new Vector2(0, -2);
            statusRt.sizeDelta = new Vector2(-12, 18);

            var progressBar = UIPrimitiveFactory.CreateUIObject("ProgressBar", card);
            UIPrimitiveFactory.SetAnchors(progressBar, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
            progressBar.anchoredPosition = new Vector2(0, 8);
            progressBar.sizeDelta = new Vector2(-12, 6);
            UIPrimitiveFactory.AddImage(progressBar, new Color(0, 0, 0, 0.4f));

            var progressFillContainer = UIPrimitiveFactory.CreateUIObject("Fill", progressBar);
            UIPrimitiveFactory.SetAnchors(progressFillContainer, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f));
            progressFillContainer.anchoredPosition = Vector2.zero;
            progressFillContainer.sizeDelta = new Vector2(0, 0);
            var fillImg = UIPrimitiveFactory.AddImage(progressFillContainer, UIColors.Default.Ok);

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
            _detailName = UIPrimitiveFactory.AddThemedText(_detailPanel, "Select a technology", 22, UIColors.Default.Text, TextAlignmentOptions.TopLeft);
            var nameRt = _detailName.rectTransform;
            UIPrimitiveFactory.SetAnchors(nameRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            nameRt.anchoredPosition = new Vector2(0, -15);
            nameRt.sizeDelta = new Vector2(-24, 30);

            _detailCategory = UIPrimitiveFactory.AddThemedText(_detailPanel, "", 14, UIColors.Default.Muted, TextAlignmentOptions.TopLeft);
            var catRt = _detailCategory.rectTransform;
            UIPrimitiveFactory.SetAnchors(catRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            catRt.anchoredPosition = new Vector2(0, -48);
            catRt.sizeDelta = new Vector2(-24, 20);

            _detailDesc = UIPrimitiveFactory.AddThemedText(_detailPanel, "", 15, UIColors.Default.Text, TextAlignmentOptions.TopLeft);
            var descRt = _detailDesc.rectTransform;
            UIPrimitiveFactory.SetAnchors(descRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            descRt.anchoredPosition = new Vector2(0, -78);
            descRt.sizeDelta = new Vector2(-24, 60);

            _detailCost = UIPrimitiveFactory.AddThemedText(_detailPanel, "", 15, UIColors.Default.Muted, TextAlignmentOptions.TopLeft);
            var costRt = _detailCost.rectTransform;
            UIPrimitiveFactory.SetAnchors(costRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            costRt.anchoredPosition = new Vector2(0, -150);
            costRt.sizeDelta = new Vector2(-24, 80);

            _detailResearchBtn = UIPrimitiveFactory.CreateButton(_detailPanel, "ResearchBtn", "START RESEARCH", StartResearchFromDetail);
            var btnRt = _detailResearchBtn.GetComponent<RectTransform>();
            UIPrimitiveFactory.SetAnchors(btnRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            btnRt.anchoredPosition = new Vector2(0, -250);
            btnRt.sizeDelta = new Vector2(-24, 50);
            _detailResearchBtnText = _detailResearchBtn.GetComponentInChildren<TextMeshProUGUI>();
            var confirmImg = _detailResearchBtn.GetComponent<Image>();
            if (confirmImg != null) confirmImg.color = UIColors.Default.Ok;

            _detailBonuses = UIPrimitiveFactory.AddThemedText(_detailPanel, "", 15, UIColors.Default.Text, TextAlignmentOptions.TopLeft);
            var bonRt = _detailBonuses.rectTransform;
            UIPrimitiveFactory.SetAnchors(bonRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            bonRt.anchoredPosition = new Vector2(0, -310);
            bonRt.sizeDelta = new Vector2(-24, 100);

            _detailPrereqs = UIPrimitiveFactory.AddThemedText(_detailPanel, "", 14, UIColors.TextDim, TextAlignmentOptions.TopLeft);
            var prereqRt = _detailPrereqs.rectTransform;
            UIPrimitiveFactory.SetAnchors(prereqRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            prereqRt.anchoredPosition = new Vector2(0, -420);
            prereqRt.sizeDelta = new Vector2(-24, 60);
        }

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
                view.NameText.color = (isResearched || isResearching || canStart) ? UIColors.Default.Text : UIColors.TextDim;
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

            _detailResearchBtn.gameObject.SetActive(true);
            _detailResearchBtn.interactable = canStart;
            var btnImg = _detailResearchBtn.GetComponent<Image>();
            if (btnImg != null)
                btnImg.color = canStart ? UIColors.Default.Ok : new Color(0.3f, 0.3f, 0.32f, 1f);

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
                ToastUI.Show("Research system not available!", UIColors.Default.Warn);
                return;
            }

            if (ResearchManager.Instance.IsResearching)
            {
                ToastUI.Show("Another research is already in progress!", UIColors.Default.Warn);
                return;
            }

            if (ResearchManager.Instance.StartResearch(_selectedTech))
            {
                ToastUI.Show($"Research started: {_selectedTech.DisplayName}", UIColors.Default.Ok);
                RefreshAll();
            }
            else
            {
                ToastUI.Show("Cannot start research!", UIColors.Default.Danger);
            }
        }
    }
}
