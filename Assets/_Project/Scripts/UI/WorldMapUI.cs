using System.Collections.Generic;
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
        [Header("Map Display")]
        [SerializeField] private Transform _mapGrid;
        [SerializeField] private GameObject _nodeButtonPrefab;

        [Header("Node Info")]
        [SerializeField] private GameObject _nodeInfoPanel;
        [SerializeField] private TMP_Text _nodeNameText;
        [SerializeField] private TMP_Text _nodeTypeText;
        [SerializeField] private TMP_Text _nodeDistanceText;
        [SerializeField] private Button _exploreButton;
        [SerializeField] private Button _sendExpeditionButton;

        [Header("Expedition Setup")]
        [SerializeField] private GameObject _expeditionPanel;
        [SerializeField] private TMP_Text _troopSelectText;
        [SerializeField] private Button _confirmExpeditionButton;
        [SerializeField] private Button _cancelExpeditionButton;

        [Header("Expedition Tracker")]
        [SerializeField] private TMP_Text _expeditionStatusText;
        [SerializeField] private Slider _expeditionProgressBar;

        private MapNodeData _selectedNode;
        private readonly Dictionary<TroopType, int> _selectedTroops = new();

        private void OnEnable()
        {
            GenerateMap();
            if (WorldMap.Instance != null)
                WorldMap.Instance.OnMapUpdated += GenerateMap;
        }

        private void OnDisable()
        {
            if (WorldMap.Instance != null)
                WorldMap.Instance.OnMapUpdated -= GenerateMap;
        }

        private void Update()
        {
            UpdateExpeditionTracker();
        }

        private void GenerateMap()
        {
            if (WorldMap.Instance == null || _mapGrid == null) return;
            if (_nodeButtonPrefab == null) return;

            foreach (Transform child in _mapGrid)
                Destroy(child.gameObject);

            for (int y = WorldMap.Instance.MapHeight - 1; y >= 0; y--)
            {
                for (int x = 0; x < WorldMap.Instance.MapWidth; x++)
                {
                    var node = WorldMap.Instance.GetNode(x, y);
                    if (node == null) continue;

                    GameObject btn = Instantiate(_nodeButtonPrefab, _mapGrid);
                    btn.name = $"Node_{x}_{y}";

                    var tmp = btn.GetComponentInChildren<TMP_Text>();
                    if (tmp != null)
                    {
                        if (!node.IsVisible)
                        {
                            tmp.text = "?";
                            tmp.color = Color.gray;
                        }
                        else if (node.NodeType == MapNodeType.PlayerBase)
                        {
                            tmp.text = "★";
                            tmp.color = Color.yellow;
                        }
                        else
                        {
                            tmp.text = GetNodeIcon(node.NodeType);
                            tmp.color = GetNodeColor(node.NodeType);
                        }
                    }

                    var button = btn.GetComponent<Button>();
                    if (button != null && node.IsVisible)
                    {
                        int cx = x, cy = y;
                        button.onClick.AddListener(() => SelectNode(cx, cy));
                    }

                    button.interactable = node.IsVisible;
                }
            }
        }

        public void SelectNode(int x, int y)
        {
            if (WorldMap.Instance == null) return;

            _selectedNode = WorldMap.Instance.GetNode(x, y);
            if (_selectedNode == null) return;

            if (_nodeInfoPanel != null) _nodeInfoPanel.SetActive(true);

            float dist = WorldMap.Instance.GetDistance(WorldMap.Instance.BasePosition, _selectedNode.GridPosition);

            if (_nodeNameText != null) _nodeNameText.text = _selectedNode.DisplayName;
            if (_nodeTypeText != null) _nodeTypeText.text = _selectedNode.NodeType.ToString();
            if (_nodeDistanceText != null) _nodeDistanceText.text = $"Distance: {dist:F1}";

            if (_exploreButton != null)
            {
                _exploreButton.gameObject.SetActive(!_selectedNode.IsExplored);
                _exploreButton.onClick.RemoveAllListeners();
                _exploreButton.onClick.AddListener(() => ExploreNode());
            }

            if (_sendExpeditionButton != null)
            {
                bool canExpedition = _selectedNode.HasBattle && _selectedNode.GridPosition != WorldMap.Instance.BasePosition;
                _sendExpeditionButton.gameObject.SetActive(canExpedition);
                _sendExpeditionButton.onClick.RemoveAllListeners();
                _sendExpeditionButton.onClick.AddListener(() => ShowExpeditionPanel());
            }
        }

        private void ExploreNode()
        {
            if (_selectedNode == null || WorldMap.Instance == null) return;
            WorldMap.Instance.ExploreNode(_selectedNode.GridPosition);
            GenerateMap();
        }

        private void ShowExpeditionPanel()
        {
            if (_expeditionPanel != null) _expeditionPanel.SetActive(true);

            _selectedTroops.Clear();
            if (ArmyManager.Instance != null)
            {
                foreach (TroopType type in System.Enum.GetValues(typeof(TroopType)))
                {
                    int count = ArmyManager.Instance.GetTroopCount(type);
                    if (count > 0) _selectedTroops[type] = 1;
                }
            }

            UpdateTroopSelectText();
        }

        private void UpdateTroopSelectText()
        {
            if (_troopSelectText == null) return;

            var parts = new List<string>();
            foreach (var kvp in _selectedTroops)
            {
                if (kvp.Value > 0) parts.Add($"{kvp.Key}: {kvp.Value}");
            }
            _troopSelectText.text = parts.Count > 0 ? string.Join("  ", parts) : "No troops selected";
        }

        public void ConfirmExpedition()
        {
            if (_selectedNode == null || ExpeditionSystem.Instance == null) return;

            if (ExpeditionSystem.Instance.SendExpedition(_selectedNode.GridPosition, _selectedTroops))
            {
                ToastUI.Show($"Expedition sent to {_selectedNode.DisplayName}!");
                if (_expeditionPanel != null) _expeditionPanel.SetActive(false);
            }
            else
            {
                ToastUI.Show("Cannot send expedition!");
            }
        }

        public void CancelExpedition()
        {
            if (_expeditionPanel != null) _expeditionPanel.SetActive(false);
            _selectedTroops.Clear();
        }

        private void UpdateExpeditionTracker()
        {
            if (ExpeditionSystem.Instance == null) return;

            var expeditions = ExpeditionSystem.Instance.GetActiveExpeditions();

            if (_expeditionStatusText != null)
            {
                if (expeditions.Count > 0)
                {
                    var current = expeditions[0];
                    string status = current.IsReturning ? "Returning" : "Traveling";
                    _expeditionStatusText.text = $"{current.TargetName} ({status}) {current.RemainingTime:F0}s";
                }
                else
                {
                    _expeditionStatusText.text = "No active expeditions";
                }
            }

            if (_expeditionProgressBar != null)
            {
                _expeditionProgressBar.value = expeditions.Count > 0 ? expeditions[0].Progress : 0f;
            }
        }

        private string GetNodeIcon(MapNodeType type)
        {
            return type switch
            {
                MapNodeType.ResourceNode => "R",
                MapNodeType.MutantCamp => "!",
                MapNodeType.AbandonedBuilding => "B",
                MapNodeType.NPCSettlement => "N",
                MapNodeType.RadioactiveZone => "X",
                MapNodeType.BossArea => "S",
                _ => "?"
            };
        }

        private Color GetNodeColor(MapNodeType type)
        {
            return type switch
            {
                MapNodeType.ResourceNode => Color.green,
                MapNodeType.MutantCamp => Color.red,
                MapNodeType.AbandonedBuilding => new Color(0.6f, 0.4f, 0.2f),
                MapNodeType.NPCSettlement => Color.cyan,
                MapNodeType.RadioactiveZone => Color.magenta,
                MapNodeType.BossArea => new Color(1f, 0.5f, 0f),
                _ => Color.white
            };
        }
    }
}
