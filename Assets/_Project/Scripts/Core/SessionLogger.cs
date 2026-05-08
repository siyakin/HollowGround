using System;
using System.IO;
using System.Text;
using HollowGround.Army;
using HollowGround.Buildings;
using HollowGround.Combat;
using HollowGround.NPCs;
using HollowGround.Quests;
using HollowGround.Resources;
using HollowGround.Tech;
using HollowGround.UI;
using HollowGround.World;
using UnityEngine;

namespace HollowGround.Core
{
    public class SessionLogger : Singleton<SessionLogger>
    {

        private StringBuilder _sb;
        private string _filePath;
        private float _sessionStartTime;
        private int _logCount;
        private float _lastFlush;
        private const float FlushInterval = 2f;

        private void Start()
        {
            SubscribeEvents();

            var config = GameConfig.Instance;
            if (config == null || !config.EnableSessionLog) return;

            _sessionStartTime = Time.time;
            _sb = new StringBuilder();
            _logCount = 0;
            _lastFlush = 0f;

            string dir = Path.Combine(Application.persistentDataPath, "SessionLogs");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            _filePath = Path.Combine(dir, $"session_{timestamp}.log");

            _sb.AppendLine("========================================");
            _sb.AppendLine($"  Hollow Ground — Session Log");
            _sb.AppendLine($"  Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _sb.AppendLine($"  DevMode: {config.DevMode}");
            _sb.AppendLine($"  DisableMutantAttacks: {config.DisableMutantAttacks}");
            _sb.AppendLine($"  BoostStartingResources: {config.BoostStartingResources}");
            _sb.AppendLine("========================================");
            _sb.AppendLine();

            Log("Game started");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeEvents();
            Flush();
        }

        private void Update()
        {
            if (_sb == null) return;
            if (Time.time - _lastFlush > FlushInterval)
            {
                Flush();
                _lastFlush = Time.time;
            }
        }

        public void Log(string message)
        {
            if (_sb == null) return;
            float elapsed = Time.time - _sessionStartTime;
            string ts = FormatTime(elapsed);
            string line = $"[{ts}] {message}";
            _sb.AppendLine(line);
            _logCount++;
            if (_logCount % 600 == 0)
                Debug.Log($"[SessionLog] {_logCount} entries written");
        }

        private void Flush()
        {
            if (_sb == null || string.IsNullOrEmpty(_filePath)) return;
            try
            {
                File.WriteAllText(_filePath, _sb.ToString());
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SessionLog] Flush failed: {e.Message}");
            }
        }

        private string FormatTime(float seconds)
        {
            int h = (int)(seconds / 3600);
            int m = (int)((seconds % 3600) / 60);
            int s = (int)(seconds % 60);
            return $"{h:D2}:{m:D2}:{s:D2}";
        }

        #region Event Subscriptions

        private void SubscribeEvents()
        {
            ToastUI.OnToastShown += OnToastShown;

            var bm = BuildingManager.Instance;
            if (bm != null)
            {
                bm.OnBuildingAdded += OnBuildingAdded;
                bm.OnBuildingRemoved += OnBuildingRemoved;
            }

            foreach (var building in bm != null ? bm.AllBuildings : new System.Collections.Generic.List<Building>())
            {
                building.OnConstructionComplete += OnBuildingConstructionComplete;
                building.OnUpgradeComplete += OnBuildingUpgradeComplete;
                building.OnProduced += OnBuildingProduced;
                building.OnDestroyed += OnBuildingDestroyed;
                building.OnDamaged += OnBuildingDamaged;
                building.OnRepaired += OnBuildingRepaired;
            }

            var rm = ResourceManager.Instance;
            if (rm != null)
                rm.OnResourceChanged += OnResourceChanged;

            var army = ArmyManager.Instance;
            if (army != null)
            {
                army.OnTrainingStarted += OnTrainingStarted;
                army.OnTrainingCompleted += OnTrainingCompleted;
                army.OnTroopCountChanged += OnTroopCountChanged;
            }

            var mam = MutantAttackManager.Instance;
            if (mam != null)
            {
                mam.OnWaveWarning += OnWaveWarning;
                mam.OnWaveStarted += OnWaveStarted;
                mam.OnWaveEnded += OnWaveEnded;
            }

            var research = ResearchManager.Instance;
            if (research != null)
            {
                research.OnResearchStarted += OnResearchStarted;
                research.OnResearchCompleted += OnResearchCompleted;
            }

            var expeditions = ExpeditionSystem.Instance;
            if (expeditions != null)
            {
                expeditions.OnExpeditionLaunched += OnExpeditionLaunched;
                expeditions.OnExpeditionCompleted += OnExpeditionCompleted;
            }

            var gm = GameManager.Instance;
            if (gm != null)
                gm.OnStateChanged += OnGameStateChanged;

            var settlers = SettlerManager.Instance;
            if (settlers != null)
            {
                settlers.OnSettlerSpawned += OnSettlerSpawned;
                settlers.OnSettlerRemoved += OnSettlerRemoved;
            }

            var roads = Roads.RoadManager.Instance;
            if (roads != null)
                roads.OnRoadsGenerated += OnRoadsGenerated;

            var garden = GardenManager.Instance;
            if (garden != null)
                garden.OnGardenMerged += OnGardenMerged;

            var trade = TradeSystem.Instance;
            if (trade != null)
                trade.OnTradeCompleted += OnTradeCompleted;

            var qm = QuestManager.Instance;
            if (qm != null)
            {
                qm.OnQuestCompleted += OnQuestCompleted;
                qm.OnQuestTurnedIn += OnQuestTurnedIn;
            }
        }

        private void UnsubscribeEvents()
        {
            ToastUI.OnToastShown -= OnToastShown;

            var bm = BuildingManager.Instance;
            if (bm != null)
            {
                bm.OnBuildingAdded -= OnBuildingAdded;
                bm.OnBuildingRemoved -= OnBuildingRemoved;
            }

            var rm = ResourceManager.Instance;
            if (rm != null)
                rm.OnResourceChanged -= OnResourceChanged;

            var army = ArmyManager.Instance;
            if (army != null)
            {
                army.OnTrainingStarted -= OnTrainingStarted;
                army.OnTrainingCompleted -= OnTrainingCompleted;
                army.OnTroopCountChanged -= OnTroopCountChanged;
            }

            var mam = MutantAttackManager.Instance;
            if (mam != null)
            {
                mam.OnWaveWarning -= OnWaveWarning;
                mam.OnWaveStarted -= OnWaveStarted;
                mam.OnWaveEnded -= OnWaveEnded;
            }

            var research = ResearchManager.Instance;
            if (research != null)
            {
                research.OnResearchStarted -= OnResearchStarted;
                research.OnResearchCompleted -= OnResearchCompleted;
            }

            var expeditions = ExpeditionSystem.Instance;
            if (expeditions != null)
            {
                expeditions.OnExpeditionLaunched -= OnExpeditionLaunched;
                expeditions.OnExpeditionCompleted -= OnExpeditionCompleted;
            }

            var gm = GameManager.Instance;
            if (gm != null)
                gm.OnStateChanged -= OnGameStateChanged;

            var settlers = SettlerManager.Instance;
            if (settlers != null)
            {
                settlers.OnSettlerSpawned -= OnSettlerSpawned;
                settlers.OnSettlerRemoved -= OnSettlerRemoved;
            }

            var roads = Roads.RoadManager.Instance;
            if (roads != null)
                roads.OnRoadsGenerated -= OnRoadsGenerated;

            var garden = GardenManager.Instance;
            if (garden != null)
                garden.OnGardenMerged -= OnGardenMerged;

            var trade = TradeSystem.Instance;
            if (trade != null)
                trade.OnTradeCompleted -= OnTradeCompleted;

            var qm = QuestManager.Instance;
            if (qm != null)
            {
                qm.OnQuestCompleted -= OnQuestCompleted;
                qm.OnQuestTurnedIn -= OnQuestTurnedIn;
            }
        }

        #endregion

        #region Event Handlers

        private void OnToastShown(string text, Color color)
        {
            Log($"[TOAST] {text}");
        }

        private void OnGameStateChanged(GameState newState)
        {
            Log($"GAME STATE: {newState}");
        }

        private void OnBuildingAdded(Building building)
        {
            Log($"BUILDING PLACED: {building.Data.DisplayName} at {building.GridOrigin} | Level {building.Level} | State: {building.State}");
            building.OnConstructionComplete += OnBuildingConstructionComplete;
            building.OnUpgradeComplete += OnBuildingUpgradeComplete;
            building.OnProduced += OnBuildingProduced;
            building.OnDestroyed += OnBuildingDestroyed;
            building.OnDamaged += OnBuildingDamaged;
            building.OnRepaired += OnBuildingRepaired;
        }

        private void OnBuildingRemoved(Building building)
        {
            Log($"BUILDING REMOVED: {building.Data.DisplayName} at {building.GridOrigin}");
            building.OnConstructionComplete -= OnBuildingConstructionComplete;
            building.OnUpgradeComplete -= OnBuildingUpgradeComplete;
            building.OnProduced -= OnBuildingProduced;
            building.OnDestroyed -= OnBuildingDestroyed;
            building.OnDamaged -= OnBuildingDamaged;
            building.OnRepaired -= OnBuildingRepaired;
        }

        private void OnBuildingConstructionComplete(Building building)
        {
            Log($"CONSTRUCTION COMPLETE: {building.Data.DisplayName} Level {building.Level}");
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.ProgressObjective(ObjectiveType.BuildBuilding, building.Data.DisplayName, 1);
                Log($"[QUEST] ProgressObjective BuildBuilding {building.Data.DisplayName}");
            }
        }

        private void OnBuildingUpgradeComplete(Building building)
        {
            Log($"UPGRADE COMPLETE: {building.Data.DisplayName} -> Level {building.Level}");
        }

        private void OnBuildingProduced(Building building, ResourceType type, int amount)
        {
            Log($"PRODUCTION: {building.Data.DisplayName} produced {amount} {type}");
        }

        private void OnBuildingDestroyed(Building building)
        {
            Log($"BUILDING DESTROYED: {building.Data.DisplayName} at {building.GridOrigin}");
        }

        private void OnBuildingDamaged(Building building)
        {
            Log($"BUILDING DAMAGED: {building.Data.DisplayName} at {building.GridOrigin} | State: {building.State}");
        }

        private void OnBuildingRepaired(Building building)
        {
            Log($"BUILDING REPAITED: {building.Data.DisplayName} at {building.GridOrigin} | Level {building.Level}");
        }

        private void OnResourceChanged(ResourceType type, int newVal)
        {
            Log($"RESOURCE: {type} = {newVal}");
        }

        private void OnTrainingStarted(ArmyManager.TrainingQueueEntry entry)
        {
            Log($"TRAINING STARTED: {entry.Amount}x {entry.Data.DisplayName} | Time: {entry.TotalTime:F1}s");
        }

        private void OnTrainingCompleted(ArmyManager.TrainingQueueEntry entry)
        {
            Log($"TRAINING COMPLETE: {entry.Amount}x {entry.Data.DisplayName}");
            if (QuestManager.Instance != null)
                QuestManager.Instance.ProgressObjective(ObjectiveType.TrainTroops, entry.Data.DisplayName, entry.Amount);
        }

        private void OnTroopCountChanged(TroopType type, int count)
        {
            Log($"TROOPS: {type} count = {count}");
        }

        private void OnWaveWarning(MutantWaveData wave)
        {
            Log($"MUTANT WARNING: {wave.DisplayName} | Power: {wave.MutantPower} | Count: {wave.MutantCount}");
        }

        private void OnWaveStarted(MutantWaveData wave)
        {
            Log($"MUTANT ATTACK: {wave.DisplayName} | Power: {wave.MutantPower}");
        }

        private void OnWaveEnded(MutantWaveData wave, bool victory)
        {
            string result = victory ? "VICTORY" : "DEFEAT";
            Log($"MUTANT WAVE {result}: {wave.DisplayName}");
        }

        private void OnResearchStarted(TechNode node)
        {
            Log($"RESEARCH STARTED: {node.DisplayName} | Time: {node.ResearchTime:F1}s");
        }

        private void OnResearchCompleted(TechNode node)
        {
            Log($"RESEARCH COMPLETE: {node.DisplayName}");
            if (QuestManager.Instance != null)
                QuestManager.Instance.ProgressObjective(ObjectiveType.ResearchTech, node.DisplayName, 1);
        }

        private void OnExpeditionLaunched(ExpeditionSystem.ActiveExpedition expedition)
        {
            Log($"EXPEDITION LAUNCHED: -> {expedition.TargetName} | Travel: {expedition.TravelTime:F1}s");
        }

        private void OnExpeditionCompleted(ExpeditionSystem.ActiveExpedition expedition)
        {
            Log($"EXPEDITION ARRIVED: {expedition.TargetName}");
        }

        private void OnSettlerSpawned(SettlerWalker walker)
        {
            Log($"SETTLER SPAWNED | Total: {SettlerManager.Instance?.SettlerCount ?? 0}");
        }

        private void OnSettlerRemoved(SettlerWalker walker)
        {
            Log($"SETTLER REMOVED | Total: {SettlerManager.Instance?.SettlerCount ?? 0}");
        }

        private void OnRoadsGenerated(Building source, System.Collections.Generic.List<Vector2Int> newRoads)
        {
            Log($"ROAD GENERATED: {newRoads.Count} tiles for {source.Data.DisplayName} at {source.GridOrigin}");
        }

        private void OnGardenMerged(Building largeGarden)
        {
            Log($"GARDEN MERGED: 4 small gardens -> {largeGarden.Data.DisplayName} at {largeGarden.GridOrigin}");
            ToastUI.Show($"4 Gardens merged into {largeGarden.Data.DisplayName}!", UIColors.Default.Gold);
        }

        private void OnTradeCompleted(FactionData faction)
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.ProgressObjective(ObjectiveType.TradeWithFaction, faction.DisplayName, 1);
        }

        private void OnQuestCompleted(QuestInstance quest)
        {
            Log($"QUEST COMPLETED: {quest.Data.DisplayName}");
            ToastUI.Show($"Quest complete: {quest.Data.DisplayName}! Turn in for rewards.", UIColors.Default.Gold);
        }

        private void OnQuestTurnedIn(QuestInstance quest)
        {
            Log($"QUEST TURNED IN: {quest.Data.DisplayName}");
            var rewards = quest.Data.GetRewardMap();
            var parts = new System.Collections.Generic.List<string>();
            foreach (var kvp in rewards)
                parts.Add($"+{kvp.Value} {kvp.Key}");
            ToastUI.Show($"Turned in: {quest.Data.DisplayName} | {string.Join(" ", parts)}", UIColors.Default.Ok);
        }

        #endregion
    }
}
