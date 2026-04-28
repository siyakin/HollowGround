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

            UIPrimitiveFactory.StretchFull(_root, new Vector2(0f, 60f), Vector2.zero);

            var bg = UIPrimitiveFactory.CreateUIObject("Background", _root);
            UIPrimitiveFactory.StretchFull(bg);
            UIPrimitiveFactory.AddImage(bg, UIColors.Default.PanelBg);

            var header = UIPrimitiveFactory.CreateUIObject("Header", _root);
            UIPrimitiveFactory.SetAnchors(header, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            header.anchoredPosition = new Vector2(0, -30);
            header.sizeDelta = new Vector2(-40, 50);
            var titleLabel = UIPrimitiveFactory.AddThemedText(header, "WORLD MAP", 28, UIColors.Default.Text, TextAlignmentOptions.Center);
            UIPrimitiveFactory.StretchFull(titleLabel.rectTransform);

            _gridRect = UIPrimitiveFactory.CreateUIObject("MapGrid", _root);
            UIPrimitiveFactory.SetAnchors(_gridRect, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
            _gridRect.anchoredPosition = new Vector2(40, -20);
            int gridPx = (_tileSize + _tileSpacing) * 10;
            _gridRect.sizeDelta = new Vector2(gridPx, gridPx);

            _infoPanel = UIPrimitiveFactory.CreateUIObject("InfoPanel", _root);
            UIPrimitiveFactory.SetAnchors(_infoPanel, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
            _infoPanel.anchoredPosition = new Vector2(-40, -20);
            _infoPanel.sizeDelta = new Vector2(360, 540);
            UIPrimitiveFactory.AddImage(_infoPanel, UIColors.PanelInner);
            BuildInfoPanel();

            _expeditionListPanel = UIPrimitiveFactory.CreateUIObject("ExpeditionList", _root);
            UIPrimitiveFactory.SetAnchors(_expeditionListPanel, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            _expeditionListPanel.anchoredPosition = new Vector2(0, 30);
            _expeditionListPanel.sizeDelta = new Vector2(800, 70);
            UIPrimitiveFactory.AddImage(_expeditionListPanel, UIColors.PanelInner);
            _expeditionListText = UIPrimitiveFactory.AddThemedText(_expeditionListPanel, "No active expeditions", 16, UIColors.Default.Muted, TextAlignmentOptions.Center);
            UIPrimitiveFactory.StretchFull(_expeditionListText.rectTransform, new Vector2(10, 5), new Vector2(-10, -5));

            _expeditionSetupPanel = UIPrimitiveFactory.CreateUIObject("ExpeditionSetup", _root);
            UIPrimitiveFactory.SetAnchors(_expeditionSetupPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            _expeditionSetupPanel.anchoredPosition = Vector2.zero;
            _expeditionSetupPanel.sizeDelta = new Vector2(520, 500);
            UIPrimitiveFactory.AddImage(_expeditionSetupPanel, UIColors.PanelInner);
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
                    var tileRoot = UIPrimitiveFactory.CreateUIObject($"Tile_{x}_{y}", _gridRect);
                    UIPrimitiveFactory.SetAnchors(tileRoot, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));
                    tileRoot.anchoredPosition = new Vector2(x * step, y * step);
                    tileRoot.sizeDelta = new Vector2(_tileSize, _tileSize);

                    var borderGo = UIPrimitiveFactory.CreateUIObject("Border", tileRoot);
                    UIPrimitiveFactory.StretchFull(borderGo);
                    var borderImg = UIPrimitiveFactory.AddImage(borderGo, new Color(0, 0, 0, 0));

                    var bgGo = UIPrimitiveFactory.CreateUIObject("BG", tileRoot);
                    UIPrimitiveFactory.StretchFull(bgGo, new Vector2(2, 2), new Vector2(-2, -2));
                    var bgImg = UIPrimitiveFactory.AddImage(bgGo, UIColors.Empty);

                    var iconText = UIPrimitiveFactory.AddThemedText(tileRoot, "", 22, UIColors.Default.Text, TextAlignmentOptions.Center);
                    UIPrimitiveFactory.StretchFull(iconText.rectTransform);

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
            var legend = UIPrimitiveFactory.CreateUIObject("Legend", _root);
            UIPrimitiveFactory.SetAnchors(legend, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));
            legend.anchoredPosition = new Vector2(40, 30);
            legend.sizeDelta = new Vector2(200, 160);
            UIPrimitiveFactory.AddImage(legend, UIColors.PanelInner);

            var legendText = UIPrimitiveFactory.AddThemedText(legend,
                "<b>LEGEND</b>\n" +
                "<color=#F2BE26>*</color> Base\n" +
                "<color=#4DBF59>R</color> Resource\n" +
                "<color=#D94040>M</color> Mutant Camp\n" +
                "<color=#996633>B</color> Ruins\n" +
                "<color=#4DB3E6>N</color> Settlement\n" +
                "<color=#BF4DD9>X</color> Radioactive\n" +
                "<color=#F28019>!</color> Boss",
                13, UIColors.Default.Text, TextAlignmentOptions.TopLeft);
            UIPrimitiveFactory.StretchFull(legendText.rectTransform, new Vector2(10, 5), new Vector2(-10, -5));
        }

        private void BuildInfoPanel()
        {
            _infoTitleText = UIPrimitiveFactory.AddThemedText(_infoPanel, "Select a tile", 20, UIColors.Default.Text, TextAlignmentOptions.TopLeft);
            var titleRect = _infoTitleText.rectTransform;
            UIPrimitiveFactory.SetAnchors(titleRect, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            titleRect.anchoredPosition = new Vector2(0, -15);
            titleRect.sizeDelta = new Vector2(-24, 30);

            _infoBodyText = UIPrimitiveFactory.AddThemedText(_infoPanel, "Click a visible tile on the map to see details.",
                14, UIColors.Default.Muted, TextAlignmentOptions.TopLeft);
            var bodyRect = _infoBodyText.rectTransform;
            UIPrimitiveFactory.SetAnchors(bodyRect, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            bodyRect.anchoredPosition = new Vector2(0, -55);
            bodyRect.sizeDelta = new Vector2(-24, 340);

            _infoArmyText = UIPrimitiveFactory.AddThemedText(_infoPanel, "", 13, UIColors.Default.Muted, TextAlignmentOptions.TopLeft);
            var armyRect = _infoArmyText.rectTransform;
            UIPrimitiveFactory.SetAnchors(armyRect, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
            armyRect.anchoredPosition = new Vector2(0, 60);
            armyRect.sizeDelta = new Vector2(-24, 90);

            _sendExpeditionButton = UIPrimitiveFactory.CreateButton(_infoPanel, "SendExpeditionBtn", "SEND EXPEDITION", OpenExpeditionSetup);
            var btnRect = _sendExpeditionButton.GetComponent<RectTransform>();
            UIPrimitiveFactory.SetAnchors(btnRect, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
            btnRect.anchoredPosition = new Vector2(0, 20);
            btnRect.sizeDelta = new Vector2(-24, 40);
            _sendExpeditionButton.gameObject.SetActive(false);
        }

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
                    view.Background.color = UIColors.Fog;
                    view.Icon.text = "";
                    view.Button.interactable = false;
                }
                else
                {
                    view.Button.interactable = true;
                    if (isBase)
                    {
                        view.Background.color = UIColors.GetNodeColor(MapNodeType.PlayerBase);
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
                view.Border.color = isSelected ? UIColors.Selected : new Color(0, 0, 0, 0);
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
                if (myPower >= enemyPower * 1.3f) { verdict = "Favorable"; c = UIColors.Default.Ok; }
                else if (myPower >= enemyPower * 0.9f) { verdict = "Even match"; c = UIColors.Default.Warn; }
                else { verdict = "Outmatched"; c = UIColors.Default.Danger; }
                armySb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{verdict}</color>");
            }
            _infoArmyText.text = armySb.ToString();

            bool canSend = totalTroops > 0 && !(node.IsCleared && !node.IsRepeatable);
            _sendExpeditionButton.gameObject.SetActive(canSend);
        }

        private void BuildExpeditionSetupPanel()
        {
            var title = UIPrimitiveFactory.AddThemedText(_expeditionSetupPanel, "SELECT TROOPS", 18, UIColors.Default.Text, TextAlignmentOptions.Center);
            var titleRect = title.rectTransform;
            UIPrimitiveFactory.SetAnchors(titleRect, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            titleRect.anchoredPosition = new Vector2(0, -15);
            titleRect.sizeDelta = new Vector2(0, 30);

            _setupTroopContainer = UIPrimitiveFactory.CreateUIObject("TroopRows", _expeditionSetupPanel);
            UIPrimitiveFactory.SetAnchors(_setupTroopContainer, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            _setupTroopContainer.anchoredPosition = new Vector2(0, -55);
            _setupTroopContainer.sizeDelta = new Vector2(-30, 280);

            var vlg = _setupTroopContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            _setupPowerText = UIPrimitiveFactory.AddThemedText(_expeditionSetupPanel, "", 14, UIColors.Default.Text, TextAlignmentOptions.Center);
            var powerRect = _setupPowerText.rectTransform;
            UIPrimitiveFactory.SetAnchors(powerRect, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
            powerRect.anchoredPosition = new Vector2(0, 110);
            powerRect.sizeDelta = new Vector2(-30, 40);

            _setupTroopsText = UIPrimitiveFactory.AddThemedText(_expeditionSetupPanel, "", 12, UIColors.Default.Muted, TextAlignmentOptions.Center);
            var troopsRect = _setupTroopsText.rectTransform;
            UIPrimitiveFactory.SetAnchors(troopsRect, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
            troopsRect.anchoredPosition = new Vector2(0, 75);
            troopsRect.sizeDelta = new Vector2(-30, 30);

            var cancelBtn = UIPrimitiveFactory.CreateButton(_expeditionSetupPanel, "Cancel", "CANCEL", CloseExpeditionSetup);
            var cancelRect = cancelBtn.GetComponent<RectTransform>();
            UIPrimitiveFactory.SetAnchors(cancelRect, new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            cancelRect.anchoredPosition = new Vector2(0, 20);
            cancelRect.sizeDelta = new Vector2(-20, 40);

            var confirmBtn = UIPrimitiveFactory.CreateThemedButton(_expeditionSetupPanel, "Confirm", "CONFIRM", ConfirmExpedition, UIStyleType.ConfirmButton);
            var confirmRect = confirmBtn.GetComponent<RectTransform>();
            UIPrimitiveFactory.SetAnchors(confirmRect, new Vector2(0.5f, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
            confirmRect.anchoredPosition = new Vector2(0, 20);
            confirmRect.sizeDelta = new Vector2(-20, 40);
            var confirmImg = confirmBtn.GetComponent<Image>();
            if (confirmImg != null) confirmImg.color = UIColors.Default.Ok;
        }

        private void BuildTroopRow(TroopType type, int available)
        {
            var row = UIPrimitiveFactory.CreateUIObject($"Row_{type}", _setupTroopContainer);
            row.sizeDelta = new Vector2(0, 36);
            UIPrimitiveFactory.AddImage(row, new Color(0.2f, 0.2f, 0.22f, 1f));

            var label = UIPrimitiveFactory.AddThemedText(row, type.ToString(), 14, UIColors.Default.Text, TextAlignmentOptions.MidlineLeft);
            UIPrimitiveFactory.SetAnchors(label.rectTransform, new Vector2(0, 0), new Vector2(0.45f, 1), new Vector2(0, 0.5f));
            label.rectTransform.anchoredPosition = new Vector2(10, 0);
            label.rectTransform.sizeDelta = Vector2.zero;

            var minusBtn = UIPrimitiveFactory.CreateButton(row, "Minus", "-", () => ChangeTroopCount(type, -1));
            var minusRt = minusBtn.GetComponent<RectTransform>();
            UIPrimitiveFactory.SetAnchors(minusRt, new Vector2(0.45f, 0.5f), new Vector2(0.45f, 0.5f), new Vector2(0, 0.5f));
            minusRt.anchoredPosition = new Vector2(0, 0);
            minusRt.sizeDelta = new Vector2(30, 28);

            var countText = UIPrimitiveFactory.AddThemedText(row, "0", 14, UIColors.Default.Text, TextAlignmentOptions.Center);
            UIPrimitiveFactory.SetAnchors(countText.rectTransform, new Vector2(0.55f, 0.5f), new Vector2(0.75f, 0.5f), new Vector2(0.5f, 0.5f));
            countText.rectTransform.anchoredPosition = Vector2.zero;
            countText.rectTransform.sizeDelta = new Vector2(0, 28);

            var plusBtn = UIPrimitiveFactory.CreateButton(row, "Plus", "+", () => ChangeTroopCount(type, 1));
            var plusRt = plusBtn.GetComponent<RectTransform>();
            UIPrimitiveFactory.SetAnchors(plusRt, new Vector2(0.8f, 0.5f), new Vector2(0.8f, 0.5f), new Vector2(0, 0.5f));
            plusRt.anchoredPosition = new Vector2(0, 0);
            plusRt.sizeDelta = new Vector2(30, 28);

            var availText = UIPrimitiveFactory.AddThemedText(row, $"/ {available}", 12, UIColors.Default.Muted, TextAlignmentOptions.MidlineLeft);
            UIPrimitiveFactory.SetAnchors(availText.rectTransform, new Vector2(0.85f, 0), new Vector2(1, 1), new Vector2(0, 0.5f));
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
                ToastUI.Show("No troops available to send!", UIColors.Default.Warn);
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
                ToastUI.Show("Expedition system not available!", UIColors.Default.Warn);
                return;
            }
            if (_selectedTroops.Values.Sum() == 0)
            {
                ToastUI.Show("Select at least one troop!", UIColors.Default.Warn);
                return;
            }

            bool ok = ExpeditionSystem.Instance.SendExpedition(_selectedNode.GridPosition, _selectedTroops);
            if (ok)
            {
                ToastUI.Show($"Expedition sent to {_selectedNode.DisplayName}!", UIColors.Default.Ok);
                CloseExpeditionSetup();
                RefreshInfoPanel();
            }
            else
            {
                ToastUI.Show("Cannot send expedition!", UIColors.Default.Danger);
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
                _expeditionListText.color = UIColors.Default.Muted;
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
            _expeditionListText.color = UIColors.Default.Text;
        }

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
            Color c = UIColors.GetNodeColor(type);
            if (cleared) c = Color.Lerp(c, UIColors.Empty, 0.55f);
            return c;
        }
    }
}
