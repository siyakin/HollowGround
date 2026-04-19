using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HollowGround.World
{
    public class WorldMap : MonoBehaviour
    {
        public static WorldMap Instance { get; private set; }

        [Header("Map Settings")]
        [SerializeField] private int _mapWidth = 10;
        [SerializeField] private int _mapHeight = 10;
        [SerializeField] private float _nodeSpacing = 100f;

        [Header("Player Base Position")]
        [SerializeField] private Vector2Int _basePosition = new(4, 4);

        private Dictionary<Vector2Int, MapNodeData> _nodes = new();
        private List<MapNodeData> _allNodes = new();

        public int MapWidth => _mapWidth;
        public int MapHeight => _mapHeight;
        public float NodeSpacing => _nodeSpacing;
        public Vector2Int BasePosition => _basePosition;
        public List<MapNodeData> AllNodes => _allNodes;

        public event Action<MapNodeData> OnNodeExplored;
        public event Action OnMapUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize(List<MapNodeData> nodes)
        {
            _nodes.Clear();
            _allNodes.Clear();

            foreach (var node in nodes)
            {
                _nodes[node.GridPosition] = node;
                _allNodes.Add(node);
            }

            RevealArea(_basePosition, 2);
            OnMapUpdated?.Invoke();
        }

        public MapNodeData GetNode(Vector2Int pos)
        {
            return _nodes.TryGetValue(pos, out var node) ? node : null;
        }

        public MapNodeData GetNode(int x, int y) => GetNode(new Vector2Int(x, y));

        public List<MapNodeData> GetNeighbors(MapNodeData node)
        {
            var neighbors = new List<MapNodeData>();
            Vector2Int[] directions = {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
                new(1, 1), new(1, -1), new(-1, 1), new(-1, -1)
            };

            foreach (var dir in directions)
            {
                var neighbor = GetNode(node.GridPosition + dir);
                if (neighbor != null)
                    neighbors.Add(neighbor);
            }

            return neighbors;
        }

        public List<MapNodeData> GetVisibleNodes()
        {
            return _allNodes.Where(n => n.IsVisible).ToList();
        }

        public List<MapNodeData> GetExploredNodes()
        {
            return _allNodes.Where(n => n.IsExplored).ToList();
        }

        public void ExploreNode(Vector2Int pos)
        {
            var node = GetNode(pos);
            if (node == null) return;

            if (!node.IsExplored)
            {
                node.SetExplored(true);
                node.SetVisible(true);
                OnNodeExplored?.Invoke(node);
            }

            RevealArea(pos, node.RevealRadius);
            OnMapUpdated?.Invoke();
        }

        private void RevealArea(Vector2Int center, int radius)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (dx * dx + dy * dy > radius * radius) continue;

                    var node = GetNode(center.x + dx, center.y + dy);
                    if (node != null)
                    {
                        node.SetVisible(true);
                        if (!node.IsExplored)
                            node.SetExplored(true);
                    }
                }
            }
        }

        public float GetDistance(Vector2Int from, Vector2Int to)
        {
            return Vector2Int.Distance(from, to);
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
        {
            var openSet = new List<Vector2Int> { start };
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, float> { [start] = 0 };
            var fScore = new Dictionary<Vector2Int, float> { [start] = Vector2Int.Distance(start, end) };

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(n => fScore.GetValueOrDefault(n, float.MaxValue)).First();

                if (current == end)
                {
                    var path = new List<Vector2Int>();
                    while (cameFrom.ContainsKey(current))
                    {
                        path.Add(current);
                        current = cameFrom[current];
                    }
                    path.Reverse();
                    return path;
                }

                openSet.Remove(current);

                foreach (var dir in new Vector2Int[] {
                    Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                {
                    var neighbor = current + dir;
                    var neighborNode = GetNode(neighbor);
                    if (neighborNode == null) continue;

                    float tentativeG = gScore[current] + 1;
                    if (tentativeG < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = tentativeG + Vector2Int.Distance(neighbor, end);

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return new List<Vector2Int>();
        }

        public void GenerateDefaultMap()
        {
            var nodes = new List<MapNodeData>();

            for (int x = 0; x < _mapWidth; x++)
            {
                for (int y = 0; y < _mapHeight; y++)
                {
                    var node = ScriptableObject.CreateInstance<MapNodeData>();
                    node.DisplayName = $"Sector {x},{y}";
                    node.GridPosition = new Vector2Int(x, y);

                    if (x == _basePosition.x && y == _basePosition.y)
                    {
                        node.NodeType = MapNodeType.PlayerBase;
                        node.StartsRevealed = true;
                        node.DisplayName = "Player Base";
                    }
                    else
                    {
                        float dist = Vector2Int.Distance(new Vector2Int(x, y), _basePosition);
                        node.NodeType = GetRandomNodeType(dist);
                        node.StartsRevealed = false;
                    }

                    node.RevealRadius = 1;
                    nodes.Add(node);
                }
            }

            Initialize(nodes);
        }

        private MapNodeType GetRandomNodeType(float distanceFromBase)
        {
            float roll = UnityEngine.Random.value;

            if (distanceFromBase < 3f)
            {
                if (roll < 0.5f) return MapNodeType.ResourceNode;
                if (roll < 0.8f) return MapNodeType.AbandonedBuilding;
                return MapNodeType.MutantCamp;
            }
            else if (distanceFromBase < 5f)
            {
                if (roll < 0.3f) return MapNodeType.ResourceNode;
                if (roll < 0.5f) return MapNodeType.MutantCamp;
                if (roll < 0.7f) return MapNodeType.AbandonedBuilding;
                if (roll < 0.9f) return MapNodeType.NPCSettlement;
                return MapNodeType.RadioactiveZone;
            }
            else
            {
                if (roll < 0.2f) return MapNodeType.MutantCamp;
                if (roll < 0.4f) return MapNodeType.RadioactiveZone;
                if (roll < 0.6f) return MapNodeType.BossArea;
                if (roll < 0.8f) return MapNodeType.NPCSettlement;
                return MapNodeType.ResourceNode;
            }
        }
    }
}
