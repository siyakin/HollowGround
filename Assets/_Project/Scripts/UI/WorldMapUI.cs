using System.Collections.Generic;
using System.Linq;
using HollowGround.Army;
using HollowGround.Combat;
using HollowGround.World;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class WorldMapUI : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int _tileSize = 58;
        [SerializeField] private int _tileSpacing = 4;

        private static readonly Color ColorFog = new(0.12f, 0.12f, 0.14f, 1f);
        private static readonly Color ColorEmpty = new(0.28f, 0.28f, 0.3f, 1f);
        private static readonly Color ColorBase = new(0.95f, 0.75f, 0.15f, 1f);
        private static readonly Color ColorResource = new(0.3f, 0.75f, 0.35f, 1f);
        private static readonly Color ColorMutant = new(0.85f, 0.25f, 0.25f, 1f);
        private static readonly Color ColorAbandoned = new(0.6f, 0.45f, 0.25f, 1f);
        private static readonly Color ColorNPC = new(0.3f, 0.7f, 0.9f, 1f);
        private static readonly Color ColorRadio = new(0.75f, 0.3f, 0.85f, 1f);
        private static readonly Color ColorBoss = new(0.95f, 0.5f, 0.1f, 1f);
        private static readonly Color ColorSelected = new(1f, 0.95f, 0.4f, 1f);
        private static readonly Color ColorPanelBg = new(0.08f, 0.09f, 0.11f, 0.92f);
        private static readonly Color ColorPanelInner = new(0.14f, 0.15f, 0.17f, 1f);
        private static readonly Color ColorTextPrimary = new(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color ColorTextMuted = new(0.65f, 0.65f, 0.7f, 1f);
        private static readonly Color ColorOk = new(0.35f, 0.8f, 0.4f, 1f);
        private static readonly Color ColorWarn = new(0.95f, 0.55f, 0.2f, 1f);
        private static readonly Color ColorDanger = new(0.9f, 0.3f, 0.3f, 1f);

        private readonly Dictionary<Vector2Int, TileView> _tiles = new();
        private MapNodeData _selectedNode;
        private readonly Dictionary<TroopType, int> _selectedTroops = new();
        private readonly Dictionary<TroopType, TMP_Text> _setupTroopCountTexts = new();

        private RectTransform _root;
        private RectTransform _gridRect;
        private RectTransform _infoPanel;
        private RectTransform _expeditionListPanel;
        private RectTransform _expeditionSetupPanel;
        private TMP_Text _infoTitleText;
        private TMP_Text _infoBodyText;
        private TMP_Text _infoArmyText;
        private Button _sendExpeditionButton;
        private TMP_Text _expeditionListText;
        private TMP_Text _setupPowerText;
        private TMP_Text _setupTroopsText;
        private RectTransform _setupTroopContainer;

        private bool _built;

        private class TileView
        {
            public Button Button;
            public Image Background;
            public Image Border;
            public TMP_Text Icon;
            public Vector2Int Pos;
        }

        private void OnEnable()
        {
            if (!_built) BuildUI();
            RefreshAll();
            if (WorldMap.Instance != null)
                WorldMap.Instance.OnMapUpdated += RefreshAll;
            if (BattleManager.Instance != null)
                BattleManager.Instance.OnBattleCompleted += HandleBattleCompleted;
        }

        private void OnDisable()
        {
            if (WorldMap.Instance != null)
                WorldMap.Instance.OnMapUpdated -= RefreshAll;
            if (BattleManager.Instance != null)
                BattleManager.Instance.OnBattleCompleted -= HandleBattleCompleted;
        }

        private void Update()
        {
            UpdateExpeditionList();
        }

        private void HandleBattleCompleted(BattleManager.BattleReport report)
        {
            RefreshAll();
        }

        // === PART 2: BuildUI goes here ===

        private void BuildUI()
        {
            _root = GetComponent<RectTransform>();
            if (_root == null)
            {
                Debug.LogError("[WorldMapUI] Must be on a RectTransform (UI Canvas child).");
                return;
            }

            foreach (Transform child in _root)
                Destroy(child.gameObject);

            var bg = CreateUIObject("Background", _root);
            StretchFull(bg);
            AddImage(bg, ColorPanelBg);

            var header = CreateUIObject("Header", _root);
            SetAnchors(header, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            header.anchoredPosition = new Vector2(0, -30);
            header.sizeDelta = new Vector2(-40, 50);
            var titleLabel = AddText(header, "WORLD MAP", 28, TextAlignmentOptions.Center, ColorTextPrimary);
            StretchFull(titleLabel.rectTransform);

            var closeBtn = CreateButton(header, "Close", "X", () =>
            {
                if (UIManager.Instance != null) UIManager.Instance.ToggleWorldMap();
                else gameObject.SetActive(false);
            });
            var closeRt = closeBtn.GetComponent<RectTransform>();
            SetAnchors(closeRt, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
            closeRt.anchoredPosition = new Vector2(-10, 0);
            closeRt.sizeDelta = new Vector2(40, 40);

            _gridRect = CreateUIObject("MapGrid", _root);
            SetAnchors(_gridRect, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
            _gridRect.anchoredPosition = new Vector2(40, -20);
            int gridPx = (_tileSize + _tileSpacing) * 10;
            _gridRect.sizeDelta = new Vector2(gridPx, gridPx);

            _infoPanel = CreateUIObject("InfoPanel", _root);
            SetAnchors(_infoPanel, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
            _infoPanel.anchoredPosition = new Vector2(-40, -20);
            _infoPanel.sizeDelta = new Vector2(360, 540);
            AddImage(_infoPanel, ColorPanelInner);
            BuildInfoPanel();

            _expeditionListPanel = CreateUIObject("ExpeditionList", _root);
            SetAnchors(_expeditionListPanel, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            _expeditionListPanel.anchoredPosition = new Vector2(0, 30);
            _expeditionListPanel.sizeDelta = new Vector2(800, 70);
            AddImage(_expeditionListPanel, ColorPanelInner);
            _expeditionListText = AddText(_expeditionListPanel, "No active expeditions", 16,
                TextAlignmentOptions.Center, ColorTextMuted);
            StretchFull(_expeditionListText.rectTransform, new Vector2(10, 5), new Vector2(-10, -5));

            _expeditionSetupPanel = CreateUIObject("ExpeditionSetup", _root);
            SetAnchors(_expeditionSetupPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            _expeditionSetupPanel.anchoredPosition = Vector2.zero;
            _expeditionSetupPanel.sizeDelta = new Vector2(520, 500);
            AddImage(_expeditionSetupPanel, ColorPanelInner);
            BuildExpeditionSetupPanel();
            _expeditionSetupPanel.gameObject.SetActive(false);

            BuildGrid();
            BuildLegend();
            _built = true;
        }

        private void BuildGrid()
        {
            if (WorldMap.Instance == null) return;

            _tiles.Clear();
            int w = WorldMap.Instance.MapWidth;
            int h = WorldMap.Instance.MapHeight;
            int step = _tileSize + _tileSpacing;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var tileRoot = CreateUIObject($"Tile_{x}_{y}", _gridRect);
                    SetAnchors(tileRoot, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));
                    tileRoot.anchoredPosition = new Vector2(x * step, y * step);
                    tileRoot.sizeDelta = new Vector2(_tileSize, _tileSize);

                    var borderGo = CreateUIObject("Border", tileRoot);
                    StretchFull(borderGo);
                    var borderImg = AddImage(borderGo, new Color(0, 0, 0, 0));

                    var bgGo = CreateUIObject("BG", tileRoot);
                    StretchFull(bgGo, new Vector2(2, 2), new Vector2(-2, -2));
                    var bgImg = AddImage(bgGo, ColorEmpty);

                    var iconText = AddText(tileRoot, "", 22, TextAlignmentOptions.Center, ColorTextPrimary);
                    StretchFull(iconText.rectTransform);

                    var btn = tileRoot.gameObject.AddComponent<Button>();
                    btn.targetGraphic = bgImg;

                    int cx = x, cy = y;
                    btn.onClick.AddListener(() => OnTileClicked(cx, cy));

                    _tiles[new Vector2Int(x, y)] = new TileView
                    {
                        Button = btn,
                        Background = bgImg,
                        Border = borderImg,
                        Icon = iconText,
                        Pos = new Vector2Int(x, y)
                    };
                }
            }
        }

        private void BuildLegend()
        {
            var legend = CreateUIObject("Legend", _root);
            SetAnchors(legend, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));
            legend.anchoredPosition = new Vector2(40, 30);
            legend.sizeDelta = new Vector2(200, 160);
            AddImage(legend, ColorPanelInner);

            var legendText = AddText(legend,
                "<b>LEGEND</b>\n" +
                "<color=#F2BE26>*</color> Base\n" +
                "<color=#4DBF59>R</color> Resource\n" +
                "<color=#D94040>M</color> Mutant Camp\n" +
                "<color=#996633>B</color> Ruins\n" +
                "<color=#4DB3E6>N</color> Settlement\n" +
                "<color=#BF4DD9>X</color> Radioactive\n" +
                "<color=#F28019>!</color> Boss",
                13, TextAlignmentOptions.TopLeft, ColorTextPrimary);
            StretchFull(legendText.rectTransform, new Vector2(10, 5), new Vector2(-10, -5));
        }

        private void BuildInfoPanel()
        {
            _infoTitleText = AddText(_infoPanel, "Select a tile", 20, TextAlignmentOptions.TopLeft, ColorTextPrimary);
            var titleRect = _infoTitleText.rectTransform;
            SetAnchors(titleRect, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            titleRect.anchoredPosition = new Vector2(0, -15);
            titleRect.sizeDelta = new Vector2(-24, 30);

            _infoBodyText = AddText(_infoPanel, "Click a visible tile on the map to see details.",
                14, TextAlignmentOptions.TopLeft, ColorTextMuted);
            var bodyRect = _infoBodyText.rectTransform;
            SetAnchors(bodyRect, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            bodyRect.anchoredPosition = new Vector2(0, -55);
            bodyRect.sizeDelta = new Vector2(-24, 340);

            _infoArmyText = AddText(_infoPanel, "", 13, TextAlignmentOptions.TopLeft, ColorTextMuted);
            var armyRect = _infoArmyText.rectTransform;
            SetAnchors(armyRect, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
            armyRect.anchoredPosition = new Vector2(0, 60);
            armyRect.sizeDelta = new Vector2(-24, 90);

            _sendExpeditionButton = CreateButton(_infoPanel, "SendExpeditionBtn", "SEND EXPEDITION", OpenExpeditionSetup);
            var btnRect = _sendExpeditionButton.GetComponent<RectTransform>();
            SetAnchors(btnRect, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
            btnRect.anchoredPosition = new Vector2(0, 20);
            btnRect.sizeDelta = new Vector2(-24, 40);
            _sendExpeditionButton.gameObject.SetActive(false);
        }

        // === PART 3: Refresh / click / info panel ===

        private void RefreshAll()
        {
            if (!_built || WorldMap.Instance == null) return;
            RefreshTiles();
            RefreshInfoPanel();
        }

        private void RefreshTiles()
        {
            foreach (var kvp in _tiles)
            {
                var node = WorldMap.Instance.GetNode(kvp.Key);
                var view = kvp.Value;
                if (node == null) continue;

                bool visible = node.IsVisible;
                bool isBase = node.NodeType == MapNodeType.PlayerBase;

                if (!visible)
                {
                    view.Background.color = ColorFog;
                    view.Icon.text = "";
                    view.Button.interactable = false;
                }
                else
                {
                    view.Button.interactable = true;
                    if (isBase)
                    {
                        view.Background.color = ColorBase;
                        view.Icon.text = "*";
                        view.Icon.color = Color.black;
                    }
                    else
                    {
                        view.Background.color = GetNodeColor(node.NodeType, node.IsCleared);
                        view.Icon.text = GetNodeIcon(node.NodeType);
                        view.Icon.color = Color.white;
                    }
                }

                bool isSelected = _selectedNode != null && _selectedNode.GridPosition == view.Pos;
                view.Border.color = isSelected ? ColorSelected : new Color(0, 0, 0, 0);
            }
        }

        private void OnTileClicked(int x, int y)
        {
            if (WorldMap.Instance == null) return;
            var node = WorldMap.Instance.GetNode(x, y);
            if (node == null || !node.IsVisible) return;

            _selectedNode = node;
            RefreshTiles();
            RefreshInfoPanel();
        }

        private void RefreshInfoPanel()
        {
            if (_selectedNode == null)
            {
                _infoTitleText.text = "Select a tile";
                _infoBodyText.text = "Click a visible tile on the map to see details.";
                _infoArmyText.text = "";
                _sendExpeditionButton.gameObject.SetActive(false);
                return;
            }

            var node = _selectedNode;
            float dist = WorldMap.Instance.GetDistance(WorldMap.Instance.BasePosition, node.GridPosition);
            float travelSeconds = dist * 30f;

            _infoTitleText.text = node.DisplayName;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"<color=#A6A6AE>Type:</color> {FormatNodeType(node.NodeType)}");
            sb.AppendLine($"<color=#A6A6AE>Distance:</color> {dist:F1} tiles");
            sb.AppendLine($"<color=#A6A6AE>Travel time:</color> ~{travelSeconds:F0}s (one way)");
            sb.AppendLine();

            if (node.NodeType == MapNodeType.PlayerBase)
            {
                sb.AppendLine("<color=#F2BE26>This is your base.</color>");
                _infoBodyText.text = sb.ToString();
                _infoArmyText.text = "";
                _sendExpeditionButton.gameObject.SetActive(false);
                return;
            }

            if (node.IsCleared && !node.IsRepeatable)
            {
                sb.AppendLine("<color=#4DBF59>Already cleared.</color>");
                _infoBodyText.text = sb.ToString();
                _infoArmyText.text = "";
                _sendExpeditionButton.gameObject.SetActive(false);
                return;
            }

            bool explored = node.IsExplored;

            if (node.HasBattle)
            {
                var bt = node.BattleTarget;
                sb.AppendLine("<b>ENEMY FORCES</b>");
                if (explored)
                {
                    var defenders = bt.GetDefenderArmy();
                    if (defenders.Count == 0)
                    {
                        sb.AppendLine("<color=#A6A6AE>None.</color>");
                    }
                    else
                    {
                        foreach (var kvp in defenders)
                            sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
                    }
                    int enemyPower = EstimateDefenderPower(defenders);
                    sb.AppendLine($"<color=#A6A6AE>Estimated power:</color> {enemyPower}");
                }
                else
                {
                    sb.AppendLine("<color=#A6A6AE>Unknown. Scout or attack to find out.</color>");
                }
                sb.AppendLine();

                sb.AppendLine("<b>EXPECTED LOOT</b>");
                if (explored)
                {
                    var loot = bt.GetLoot();
                    if (loot.Count == 0) sb.AppendLine("<color=#A6A6AE>None.</color>");
                    foreach (var kvp in loot)
                        sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
                }
                else
                {
                    sb.AppendLine("<color=#A6A6AE>Hidden.</color>");
                }
            }
            else
            {
                sb.AppendLine("<color=#A6A6AE>No combat target here. Sending an expedition will scout the area.</color>");
            }

            _infoBodyText.text = sb.ToString();

            var armySb = new System.Text.StringBuilder();
            armySb.AppendLine("<b>YOUR ARMY</b>");
            int myPower = ArmyManager.Instance != null ? ArmyManager.Instance.CalculateArmyPower() : 0;
            int totalTroops = ArmyManager.Instance != null ? ArmyManager.Instance.TotalTroopCount : 0;
            armySb.AppendLine($"Troops: {totalTroops} | Power: {myPower}");

            if (node.HasBattle && explored)
            {
                int enemyPower = EstimateDefenderPower(node.BattleTarget.GetDefenderArmy());
                string verdict;
                Color c;
                if (myPower >= enemyPower * 1.3f) { verdict = "Favorable"; c = ColorOk; }
                else if (myPower >= enemyPower * 0.9f) { verdict = "Even match"; c = ColorWarn; }
                else { verdict = "Outmatched"; c = ColorDanger; }
                armySb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{verdict}</color>");
            }
            _infoArmyText.text = armySb.ToString();

            bool canSend = totalTroops > 0 && !(node.IsCleared && !node.IsRepeatable);
            _sendExpeditionButton.gameObject.SetActive(canSend);
        }

        // === PART 4: Expedition setup ===

        private void BuildExpeditionSetupPanel()
        {
            var title = AddText(_expeditionSetupPanel, "SELECT TROOPS", 18, TextAlignmentOptions.Center, ColorTextPrimary);
            var titleRect = title.rectTransform;
            SetAnchors(titleRect, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            titleRect.anchoredPosition = new Vector2(0, -15);
            titleRect.sizeDelta = new Vector2(0, 30);

            _setupTroopContainer = CreateUIObject("TroopRows", _expeditionSetupPanel);
            SetAnchors(_setupTroopContainer, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            _setupTroopContainer.anchoredPosition = new Vector2(0, -55);
            _setupTroopContainer.sizeDelta = new Vector2(-30, 280);

            var vlg = _setupTroopContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            _setupPowerText = AddText(_expeditionSetupPanel, "", 14, TextAlignmentOptions.Center, ColorTextPrimary);
            var powerRect = _setupPowerText.rectTransform;
            SetAnchors(powerRect, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
            powerRect.anchoredPosition = new Vector2(0, 110);
            powerRect.sizeDelta = new Vector2(-30, 40);

            _setupTroopsText = AddText(_expeditionSetupPanel, "", 12, TextAlignmentOptions.Center, ColorTextMuted);
            var troopsRect = _setupTroopsText.rectTransform;
            SetAnchors(troopsRect, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
            troopsRect.anchoredPosition = new Vector2(0, 75);
            troopsRect.sizeDelta = new Vector2(-30, 30);

            var cancelBtn = CreateButton(_expeditionSetupPanel, "Cancel", "CANCEL", CloseExpeditionSetup);
            var cancelRect = cancelBtn.GetComponent<RectTransform>();
            SetAnchors(cancelRect, new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            cancelRect.anchoredPosition = new Vector2(0, 20);
            cancelRect.sizeDelta = new Vector2(-20, 40);

            var confirmBtn = CreateButton(_expeditionSetupPanel, "Confirm", "CONFIRM", ConfirmExpedition);
            var confirmRect = confirmBtn.GetComponent<RectTransform>();
            SetAnchors(confirmRect, new Vector2(0.5f, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
            confirmRect.anchoredPosition = new Vector2(0, 20);
            confirmRect.sizeDelta = new Vector2(-20, 40);
            var confirmImg = confirmBtn.GetComponent<Image>();
            if (confirmImg != null) confirmImg.color = ColorOk;
        }

        private void BuildTroopRow(TroopType type, int available)
        {
            var row = CreateUIObject($"Row_{type}", _setupTroopContainer);
            row.sizeDelta = new Vector2(0, 36);
            AddImage(row, new Color(0.2f, 0.2f, 0.22f, 1f));

            var label = AddText(row, type.ToString(), 14, TextAlignmentOptions.MidlineLeft, ColorTextPrimary);
            SetAnchors(label.rectTransform, new Vector2(0, 0), new Vector2(0.45f, 1), new Vector2(0, 0.5f));
            label.rectTransform.anchoredPosition = new Vector2(10, 0);
            label.rectTransform.sizeDelta = Vector2.zero;

            var minusBtn = CreateButton(row, "Minus", "-", () => ChangeTroopCount(type, -1));
            var minusRt = minusBtn.GetComponent<RectTransform>();
            SetAnchors(minusRt, new Vector2(0.45f, 0.5f), new Vector2(0.45f, 0.5f), new Vector2(0, 0.5f));
            minusRt.anchoredPosition = new Vector2(0, 0);
            minusRt.sizeDelta = new Vector2(30, 28);

            var countText = AddText(row, "0", 14, TextAlignmentOptions.Center, ColorTextPrimary);
            SetAnchors(countText.rectTransform, new Vector2(0.55f, 0.5f), new Vector2(0.75f, 0.5f), new Vector2(0.5f, 0.5f));
            countText.rectTransform.anchoredPosition = Vector2.zero;
            countText.rectTransform.sizeDelta = new Vector2(0, 28);

            var plusBtn = CreateButton(row, "Plus", "+", () => ChangeTroopCount(type, 1));
            var plusRt = plusBtn.GetComponent<RectTransform>();
            SetAnchors(plusRt, new Vector2(0.8f, 0.5f), new Vector2(0.8f, 0.5f), new Vector2(0, 0.5f));
            plusRt.anchoredPosition = new Vector2(0, 0);
            plusRt.sizeDelta = new Vector2(30, 28);

            var availText = AddText(row, $"/ {available}", 12, TextAlignmentOptions.MidlineLeft, ColorTextMuted);
            SetAnchors(availText.rectTransform, new Vector2(0.85f, 0), new Vector2(1, 1), new Vector2(0, 0.5f));
            availText.rectTransform.anchoredPosition = Vector2.zero;
            availText.rectTransform.sizeDelta = Vector2.zero;

            _setupTroopCountTexts[type] = countText;
        }

        private void OpenExpeditionSetup()
        {
            if (_selectedNode == null || ArmyManager.Instance == null) return;

            _selectedTroops.Clear();
            _setupTroopCountTexts.Clear();
            foreach (Transform child in _setupTroopContainer)
                Destroy(child.gameObject);

            foreach (TroopType type in System.Enum.GetValues(typeof(TroopType)))
            {
                int available = ArmyManager.Instance.GetTroopCount(type);
                if (available <= 0) continue;
                _selectedTroops[type] = 0;
                BuildTroopRow(type, available);
            }

            if (_selectedTroops.Count == 0)
            {
                ToastUI.Show("No troops available to send!");
                return;
            }

            _expeditionSetupPanel.gameObject.SetActive(true);
            UpdateSetupSummary();
        }

        private void CloseExpeditionSetup()
        {
            _expeditionSetupPanel.gameObject.SetActive(false);
            _selectedTroops.Clear();
        }

        private void ChangeTroopCount(TroopType type, int delta)
        {
            if (!_selectedTroops.ContainsKey(type)) return;
            int available = ArmyManager.Instance != null ? ArmyManager.Instance.GetTroopCount(type) : 0;
            int newVal = Mathf.Clamp(_selectedTroops[type] + delta, 0, available);
            _selectedTroops[type] = newVal;

            if (_setupTroopCountTexts.TryGetValue(type, out var txt))
                txt.text = newVal.ToString();

            UpdateSetupSummary();
        }

        private void UpdateSetupSummary()
        {
            int sentTroops = _selectedTroops.Values.Sum();
            int sentPower = sentTroops * 10;
            float morale = ArmyManager.Instance != null ? ArmyManager.Instance.GetMorale() : 1f;
            sentPower = Mathf.CeilToInt(sentPower * morale);

            var parts = new List<string>();
            foreach (var kvp in _selectedTroops)
                if (kvp.Value > 0) parts.Add($"{kvp.Key}:{kvp.Value}");
            _setupTroopsText.text = parts.Count == 0 ? "<color=#A6A6AE>No troops selected</color>" : string.Join("  ", parts);

            if (_selectedNode != null && _selectedNode.HasBattle && _selectedNode.IsExplored)
            {
                int enemy = EstimateDefenderPower(_selectedNode.BattleTarget.GetDefenderArmy());
                string color;
                string verdict;
                if (sentPower == 0) { color = "#A6A6AE"; verdict = "Select troops"; }
                else if (sentPower >= enemy * 1.3f) { color = "#59CC66"; verdict = "Favorable"; }
                else if (sentPower >= enemy * 0.9f) { color = "#F28C33"; verdict = "Even match"; }
                else { color = "#E64D4D"; verdict = "Outmatched"; }
                _setupPowerText.text = $"Your power: {sentPower}  |  Enemy: {enemy}  |  <color={color}>{verdict}</color>";
            }
            else
            {
                _setupPowerText.text = $"Your power: {sentPower}";
            }
        }

        public void ConfirmExpedition()
        {
            if (_selectedNode == null) return;
            if (ExpeditionSystem.Instance == null)
            {
                ToastUI.Show("Expedition system not available!");
                return;
            }
            if (_selectedTroops.Values.Sum() == 0)
            {
                ToastUI.Show("Select at least one troop!");
                return;
            }

            bool ok = ExpeditionSystem.Instance.SendExpedition(_selectedNode.GridPosition, _selectedTroops);
            if (ok)
            {
                ToastUI.Show($"Expedition sent to {_selectedNode.DisplayName}!");
                CloseExpeditionSetup();
                RefreshInfoPanel();
            }
            else
            {
                ToastUI.Show("Cannot send expedition!");
            }
        }

        private void UpdateExpeditionList()
        {
            if (!_built || _expeditionListText == null) return;

            var worldExps = ExpeditionSystem.Instance != null ? ExpeditionSystem.Instance.GetActiveExpeditions() : null;
            var battleExps = BattleManager.Instance != null ? BattleManager.Instance.GetExpeditions() : null;

            int total = (worldExps?.Count ?? 0) + (battleExps?.Count ?? 0);
            if (total == 0)
            {
                _expeditionListText.text = "<color=#A6A6AE>No active expeditions</color>";
                _expeditionListText.color = ColorTextMuted;
                return;
            }

            var sb = new System.Text.StringBuilder();
            if (worldExps != null)
            {
                foreach (var exp in worldExps)
                {
                    string phase = exp.IsReturning ? "Returning" : "Traveling";
                    sb.Append($"<color=#F2BE26>></color> {exp.TargetName} [{phase}] {exp.RemainingTime:F0}s   ");
                }
            }
            if (battleExps != null)
            {
                foreach (var exp in battleExps)
                {
                    sb.Append($"<color=#E64D4D>X</color> {exp.Name} [Engaging] {exp.RemainingTime:F0}s   ");
                }
            }
            _expeditionListText.text = sb.ToString();
            _expeditionListText.color = ColorTextPrimary;
        }

        // === PART 5: Helpers + UI primitives ===

        private int EstimateDefenderPower(Dictionary<TroopType, int> troops)
        {
            int p = 0;
            foreach (var kvp in troops) p += kvp.Value * 10;
            return p;
        }

        private string FormatNodeType(MapNodeType t)
        {
            return t switch
            {
                MapNodeType.PlayerBase => "Player Base",
                MapNodeType.ResourceNode => "Resource Node",
                MapNodeType.MutantCamp => "Mutant Camp",
                MapNodeType.AbandonedBuilding => "Abandoned Building",
                MapNodeType.NPCSettlement => "NPC Settlement",
                MapNodeType.RadioactiveZone => "Radioactive Zone",
                MapNodeType.BossArea => "Boss Area",
                _ => t.ToString()
            };
        }

        private string GetNodeIcon(MapNodeType type)
        {
            return type switch
            {
                MapNodeType.ResourceNode => "R",
                MapNodeType.MutantCamp => "M",
                MapNodeType.AbandonedBuilding => "B",
                MapNodeType.NPCSettlement => "N",
                MapNodeType.RadioactiveZone => "X",
                MapNodeType.BossArea => "!",
                _ => "?"
            };
        }

        private Color GetNodeColor(MapNodeType type, bool cleared)
        {
            Color c = type switch
            {
                MapNodeType.ResourceNode => ColorResource,
                MapNodeType.MutantCamp => ColorMutant,
                MapNodeType.AbandonedBuilding => ColorAbandoned,
                MapNodeType.NPCSettlement => ColorNPC,
                MapNodeType.RadioactiveZone => ColorRadio,
                MapNodeType.BossArea => ColorBoss,
                _ => ColorEmpty
            };
            if (cleared) c = Color.Lerp(c, ColorEmpty, 0.55f);
            return c;
        }

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

        private static Button CreateButton(Transform parent, string name, string label, System.Action onClick)
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
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            tmp.raycastTarget = false;

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

