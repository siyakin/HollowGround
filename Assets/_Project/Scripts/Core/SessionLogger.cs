using System;
using System.IO;
using System.Text;
using HollowGround.Army;
using HollowGround.Buildings;
using HollowGround.Combat;
using HollowGround.NPCs;
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
            if (_logCount % 50 == 0)
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
        }

        private void UnsubscribeEvents()
        {
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
        }

        #endregion

        #region Event Handlers

        private void OnGameStateChanged(GameState newState)
        {
            Log($"Game state changed to: {newState}");
        }

        private void OnBuildingAdded(Building building)
        {
            string origin = building.GridOrigin.ToString();
            Log($"BUILDING PLACED: {building.Data.DisplayName} at {origin} | Level {building.Level} | State: {building.State}");
            ToastUI.Show($"{building.Data.DisplayName} placed!", UIColors.Default.Ok);
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
            ToastUI.Show($"{building.Data.DisplayName} built!", UIColors.Default.Ok);
        }

        private void OnBuildingUpgradeComplete(Building building)
        {
            Log($"UPGRADE COMPLETE: {building.Data.DisplayName} → Level {building.Level}");
            ToastUI.Show($"{building.Data.DisplayName} upgraded to Lv.{building.Level}!", UIColors.Default.Gold);
        }

        private void OnBuildingProduced(Building building, ResourceType type, int amount)
        {
            Log($"PRODUCTION: {building.Data.DisplayName} produced {amount} {type}");
        }

        private void OnBuildingDestroyed(Building building)
        {
            Log($"BUILDING DESTROYED: {building.Data.DisplayName} at {building.GridOrigin}");
            ToastUI.Show($"{building.Data.DisplayName} destroyed!", UIColors.Default.Danger);
        }

        private void OnBuildingDamaged(Building building)
        {
            Log($"BUILDING DAMAGED: {building.Data.DisplayName} at {building.GridOrigin} | State: {building.State}");
            ToastUI.Show($"{building.Data.DisplayName} damaged! Needs repair.", UIColors.Default.Warn);
        }

        private void OnBuildingRepaired(Building building)
        {
            Log($"BUILDING REPAIRED: {building.Data.DisplayName} at {building.GridOrigin} | Level {building.Level}");
            ToastUI.Show($"{building.Data.DisplayName} repaired!", UIColors.Default.Ok);
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
        }

        private void OnTroopCountChanged(TroopType type, int count)
        {
            Log($"TROOPS: {type} count = {count}");
        }

        private void OnWaveWarning(MutantWaveData wave)
        {
            Log($"WARNING: MUTANT WARNING: {wave.DisplayName} | Power: {wave.MutantPower} | Count: {wave.MutantCount}");
            ToastUI.Show($"WARNING: {wave.MutantCount} mutants approaching! Power: {wave.MutantPower}", UIColors.Default.Warn);
        }

        private void OnWaveStarted(MutantWaveData wave)
        {
            Log($"MUTANT ATTACK: {wave.DisplayName} | Power: {wave.MutantPower}");
            ToastUI.Show($"Mutant wave {wave.WaveNumber} attacking!", UIColors.Default.Danger);
        }

        private void OnWaveEnded(MutantWaveData wave, bool victory)
        {
            string result = victory ? "VICTORY" : "DEFEAT";
            Log($" MUTANT WAVE {result}: {wave.DisplayName} | Survived");
            if (victory)
                ToastUI.Show($"Wave {wave.WaveNumber} defeated!", UIColors.Default.Ok);
            else
                ToastUI.Show($"Wave {wave.WaveNumber} — DEFEATED! Buildings damaged.", UIColors.Default.Danger);
        }

        private void OnResearchStarted(TechNode node)
        {
            Log($"RESEARCH STARTED: {node.DisplayName} | Time: {node.ResearchTime:F1}s");
        }

        private void OnResearchCompleted(TechNode node)
        {
            Log($"RESEARCH COMPLETE: {node.DisplayName}");
            ToastUI.Show($"Research complete: {node.DisplayName}!", UIColors.Default.Ok);
        }

        private void OnExpeditionLaunched(ExpeditionSystem.ActiveExpedition expedition)
        {
            Log($"EXPEDITION LAUNCHED: → {expedition.TargetName} | Travel: {expedition.TravelTime:F1}s");
        }

        private void OnExpeditionCompleted(ExpeditionSystem.ActiveExpedition expedition)
        {
            Log($"EXPEDITION ARRIVED: {expedition.TargetName}");
            ToastUI.Show($"Expedition arrived: {expedition.TargetName}", UIColors.Default.Ok);
        }

        private void OnSettlerSpawned(SettlerWalker walker)
        {
            Log($"SETTLER SPAWNED | Total: {SettlerManager.Instance?.SettlerCount ?? 0}");
        }

        private void OnSettlerRemoved(SettlerWalker walker)
        {
            Log($"SETTLER REMOVED | Total: {SettlerManager.Instance?.SettlerCount ?? 0}");
        }

        #endregion
    }
}
