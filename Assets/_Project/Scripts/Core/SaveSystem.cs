using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HollowGround.Army;
using HollowGround.Buildings;
using HollowGround.Combat;
using HollowGround.Heroes;
using HollowGround.Quests;
using HollowGround.Resources;
using HollowGround.Grid;
using HollowGround.World;
using HollowGround.NPCs;
using HollowGround.Roads;
using UnityEngine;

namespace HollowGround.Core
{
    public class SaveSystem : Singleton<SaveSystem>
    {

        private const string SaveFolder = "Saves";
        private const string FileExtension = ".json";
        private float _autoSaveTimer;

        public event System.Action OnSaveCompleted;
        public event System.Action<SaveData> OnLoadCompleted;
        public event System.Action<string> OnSaveFailed;

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                return;

            var cfg = GameConfig.Instance;
            if (cfg == null || !cfg.EnableAutoSave) return;

            _autoSaveTimer += Time.deltaTime;
            if (_autoSaveTimer >= cfg.AutoSaveInterval)
            {
                _autoSaveTimer = 0f;
                AutoSave();
            }
        }

        public SaveData CaptureSaveData()
        {
            var data = new SaveData
            {
                SaveDate = DateTime.Now.ToString("o"),
                PlayTime = TimeManager.Instance != null ? TimeManager.Instance.GameTime : 0f
            };

            CaptureResources(data);
            CaptureTime(data);
            CaptureBuildings(data);
            CaptureArmy(data);
            CaptureHeroes(data);
            CaptureResearch(data);
            CaptureQuests(data);
            CaptureMutantAttack(data);
            CaptureMap(data);
            CaptureRoads(data);
            CaptureSettlers(data);

            return data;
        }

        public void Save(string fileName)
        {
            try
            {
                var data = CaptureSaveData();
                data.SaveName = fileName;
                string json = JsonUtility.ToJson(data, true);
                string folder = GetSaveFolderPath();
                Directory.CreateDirectory(folder);
                string path = Path.Combine(folder, fileName + FileExtension);
                File.WriteAllText(path, json);
                OnSaveCompleted?.Invoke();
            }
            catch (Exception e)
            {
                OnSaveFailed?.Invoke(e.Message);
            }
        }

        public void AutoSave()
        {
            Save("autosave");
        }

        public void QuickSave()
        {
            Save("quicksave");
        }

        public SaveData Load(string fileName)
        {
            string path = GetSaveFilePath(fileName);
            if (!File.Exists(path)) return null;

            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<SaveData>(json);

            if (data != null)
            {
                ApplySaveData(data);
                OnLoadCompleted?.Invoke(data);
            }

            return data;
        }

        public List<SaveData> GetAllSaves()
        {
            var saves = new List<SaveData>();
            string folder = GetSaveFolderPath();
            if (!Directory.Exists(folder)) return saves;

            foreach (var file in Directory.GetFiles(folder, "*" + FileExtension))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var data = JsonUtility.FromJson<SaveData>(json);
                    if (data != null)
                    {
                        // Always use actual filename so Load() can find the file
                        data.SaveName = Path.GetFileNameWithoutExtension(file);
                        saves.Add(data);
                    }
                }
                catch { }
            }

            return saves.OrderByDescending(s => s.SaveDate).ToList();
        }

        public bool DeleteSave(string fileName)
        {
            string path = GetSaveFilePath(fileName);
            if (!File.Exists(path)) return false;
            File.Delete(path);
            return true;
        }

        public bool HasSave(string fileName)
        {
            return File.Exists(GetSaveFilePath(fileName));
        }

        private void ApplySaveData(SaveData data)
        {
            ApplyBuildings(data);
            ApplyResources(data);
            ApplyTime(data);
            ApplyArmy(data);
            ApplyHeroes(data);
            ApplyResearch(data);
            ApplyQuests(data);
            ApplyMutantAttack(data);
            ApplyMap(data);
            ApplyRoads(data);
            ApplySettlers(data);
        }

        #region Capture

        private void CaptureResources(SaveData data)
        {
            if (ResourceManager.Instance == null) return;

            data.Resources = new ResourceSave();
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                string key = type.ToString();
                data.Resources.Amounts.Add(new StringIntEntry { Key = key, Value = ResourceManager.Instance.Get(type) });
                data.Resources.Capacities.Add(new StringIntEntry { Key = key, Value = ResourceManager.Instance.GetCapacity(type) });
            }
        }

        private void CaptureTime(SaveData data)
        {
            if (TimeManager.Instance == null) return;
            data.Time = new TimeSave
            {
                GameTime = TimeManager.Instance.GameTime,
                GameSpeed = TimeManager.Instance.GameSpeed
            };
        }

        private void CaptureBuildings(SaveData data)
        {
            if (BuildingManager.Instance == null) return;

            foreach (var building in BuildingManager.Instance.AllBuildings)
            {
                if (building == null || building.Data == null) continue;

                data.Buildings.Add(new BuildingSave
                {
                    BuildingDataName = building.Data.name,
                    GridX = building.GridOrigin.x,
                    GridZ = building.GridOrigin.y,
                    Level = building.Level,
                    State = building.State.ToString(),
                    ConstructionProgress = building.ConstructionProgress,
                    UpgradeProgress = building.UpgradeProgress,
                    Rotation = building.Rotation
                });
            }
        }

        private void CaptureArmy(SaveData data)
        {
            if (ArmyManager.Instance == null) return;

            data.Army = new ArmySave();
            var troops = ArmyManager.Instance.GetAllTroops();
            foreach (var kvp in troops)
                data.Army.Troops.Add(new StringIntEntry { Key = kvp.Key.ToString(), Value = kvp.Value });

            foreach (var entry in ArmyManager.Instance.GetTrainingQueue())
            {
                data.Army.TrainingQueue.Add(new TrainingSave
                {
                    TroopDataName = entry.Data != null ? entry.Data.name : "",
                    Amount = entry.Amount,
                    RemainingTime = entry.RemainingTime,
                    TotalTime = entry.TotalTime
                });
            }
        }

        private void CaptureHeroes(SaveData data)
        {
            if (HeroManager.Instance == null) return;

            foreach (var hero in HeroManager.Instance.AllHeroes)
            {
                data.Heroes.Add(new HeroSave
                {
                    Id = hero.Id,
                    HeroDataName = hero.Data != null ? hero.Data.name : "",
                    Level = hero.Level,
                    CurrentXP = hero.CurrentXP,
                    IsDeployed = hero.IsDeployed,
                    IsInjured = hero.IsInjured,
                    InjuryTimer = hero.InjuryTimer,
                    Weapon = CaptureEquipment(hero.Weapon),
                    Armor = CaptureEquipment(hero.Armor),
                    Accessory = CaptureEquipment(hero.Accessory)
                });
            }
        }

        private EquipmentSave CaptureEquipment(EquipmentItem item)
        {
            if (item == null) return null;
            return new EquipmentSave
            {
                Name = item.Name,
                Slot = (int)item.Slot,
                AttackBonus = item.AttackBonus,
                DefenseBonus = item.DefenseBonus,
                HPBonus = item.HPBonus,
                Rarity = (int)item.Rarity
            };
        }

        private void CaptureResearch(SaveData data)
        {
            if (Tech.ResearchManager.Instance == null) return;

            var researched = Tech.ResearchManager.Instance.GetResearchedTechs();
            foreach (var tech in researched)
                data.ResearchedTechs.Add(tech.name);

            var current = Tech.ResearchManager.Instance.CurrentResearch;
            if (current != null)
            {
                data.CurrentResearch = current.name;
                data.CurrentResearchProgress = current.ResearchProgress;
            }
        }

        private void CaptureQuests(SaveData data)
        {
            if (QuestManager.Instance == null) return;

            foreach (var quest in QuestManager.Instance.GetAllQuests())
            {
                var qs = new QuestSave
                {
                    QuestDataName = quest.Data != null ? quest.Data.name : "",
                    Status = quest.Status.ToString()
                };
                foreach (var kvp in quest.Progress)
                    qs.Progress.Add(new IntIntEntry { Key = kvp.Key, Value = kvp.Value });
                data.Quests.Add(qs);
            }
        }

        private void CaptureMutantAttack(SaveData data)
        {
            if (MutantAttackManager.Instance == null) return;

            data.MutantAttack = new MutantAttackSave
            {
                CurrentWaveIndex = MutantAttackManager.Instance.CurrentWave,
                AttackTimer = MutantAttackManager.Instance.TimeUntilAttack,
                CycleStarted = true
            };
        }

        private void CaptureMap(SaveData data)
        {
            if (WorldMap.Instance == null) return;

            foreach (var node in WorldMap.Instance.AllNodes)
            {
                data.MapNodes.Add(new MapNodeSave
                {
                    GridX = node.GridPosition.x,
                    GridY = node.GridPosition.y,
                    NodeType = node.NodeType.ToString(),
                    IsExplored = node.IsExplored,
                    IsVisible = node.IsVisible
                });
            }
        }

        private void CaptureSettlers(SaveData data)
        {
            if (SettlerManager.Instance == null) return;
            data.Settlers = SettlerManager.Instance.CaptureSettlersSave();
        }

        #endregion

        #region Apply

        private static int FindValue(List<StringIntEntry> list, string key, int defaultVal = 0)
        {
            var entry = list.FirstOrDefault(e => e.Key == key);
            return entry != null ? entry.Value : defaultVal;
        }

        private static int FindValue(List<IntIntEntry> list, int key, int defaultVal = 0)
        {
            var entry = list.FirstOrDefault(e => e.Key == key);
            return entry != null ? entry.Value : defaultVal;
        }

        private void ApplyResources(SaveData data)
        {
            if (ResourceManager.Instance == null || data.Resources == null) return;

            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                string key = type.ToString();
                int amount = FindValue(data.Resources.Amounts, key);
                int cap = FindValue(data.Resources.Capacities, key, 500);
                ResourceManager.Instance.SetCapacity(type, cap);
                ResourceManager.Instance.Set(type, amount);
            }
        }

        private void ApplyTime(SaveData data)
        {
            if (TimeManager.Instance == null || data.Time == null) return;
            TimeManager.Instance.SetTime(data.Time.GameTime);
            TimeManager.Instance.SetSpeed(data.Time.GameSpeed);
        }

        private void ApplyBuildings(SaveData data)
        {
            if (BuildingManager.Instance == null) return;

            var existing = BuildingManager.Instance.AllBuildings.ToList();
            foreach (var b in existing)
                b.ClearForLoad();

            if (RoadManager.Instance != null)
                RoadManager.Instance.ClearAllRoads();

            foreach (var bs in data.Buildings)
            {
                if (bs.State == BuildingState.Placing.ToString()) continue;

                var buildingData = LoadBuildingDataByName(bs.BuildingDataName);
                if (buildingData == null)
                {
                    Debug.LogWarning($"[SaveSystem] BuildingData not found: {bs.BuildingDataName}");
                    continue;
                }

                int rotation = bs.Rotation;
                int sx = rotation % 2 == 0 ? buildingData.SizeX : buildingData.SizeZ;
                int sz = rotation % 2 == 0 ? buildingData.SizeZ : buildingData.SizeX;

                Vector3 worldPos = GridSystem.Instance != null
                    ? GridSystem.Instance.GetWorldPosition(bs.GridX, bs.GridZ)
                    : Vector3.zero;

                float offsetX = (sx - 1) * (GridSystem.Instance != null ? GridSystem.Instance.CellSize : 2f) * 0.5f;
                float offsetZ = (sz - 1) * (GridSystem.Instance != null ? GridSystem.Instance.CellSize : 2f) * 0.5f;

                var go = new GameObject(buildingData.DisplayName);
                go.transform.position = new Vector3(worldPos.x + offsetX, worldPos.y, worldPos.z + offsetZ);
                go.transform.rotation = Quaternion.Euler(0, rotation * 90f, 0);

                var building = go.AddComponent<Building>();
                building.Initialize(buildingData, new Vector2Int(bs.GridX, bs.GridZ), rotation);

                if (System.Enum.TryParse<BuildingState>(bs.State, out var state))
                    building.RestoreFromSave(bs.Level, state, bs.ConstructionProgress, bs.UpgradeProgress);

                if (GridSystem.Instance != null)
                    GridSystem.Instance.OccupyCells(bs.GridX, bs.GridZ, sx, sz, go);

                BuildingManager.Instance.RegisterBuilding(building);
            }
        }

        private static readonly Dictionary<string, string> BuildingNameAliases = new()
        {
            { "Su Kuyusu", "WaterWell" },
            { "Depo", "Storage" },
            { "Ciftlik", "Farm" },
            { "Maden", "Mine" },
            { "Odun Fabrikasi", "WoodFactory" },
            { "Jenerator", "Generator" },
            { "Kisla", "Barracks" },
            { "Barinak", "Shelter" },
            { "Hastane", "Hospital" },
            { "Siginak", "Shelter" },
            { "Building Red", "Storage" },
            { "Building Green", "Farm" },
            { "Town House", "Shelter" },
        };

        private static BuildingData LoadBuildingDataByName(string name)
        {
            var result = FindBuildingData(name);
            if (result != null) return result;

            if (BuildingNameAliases.TryGetValue(name, out string alias))
                result = FindBuildingData(alias);

            return result;
        }

        private static BuildingData FindBuildingData(string name)
        {
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:BuildingData");
            foreach (var guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var bd = UnityEditor.AssetDatabase.LoadAssetAtPath<BuildingData>(path);
                if (bd != null && (bd.name == name || bd.DisplayName == name))
                    return bd;
            }
            return null;
#else
            return UnityEngine.Resources.LoadAll<BuildingData>("Buildings")
                .FirstOrDefault(b => b.name == name || b.DisplayName == name);
#endif
        }

        private void ApplyArmy(SaveData data)
        {
            if (ArmyManager.Instance == null || data.Army == null) return;

            foreach (var entry in data.Army.Troops)
            {
                if (Enum.TryParse<TroopType>(entry.Key, out var type))
                    ArmyManager.Instance.AddTroops(type, entry.Value);
            }
        }

        private void ApplyHeroes(SaveData data)
        {
            if (HeroManager.Instance == null) return;

            var allHeroData = UnityEngine.Resources.LoadAll<HeroData>("Heroes");

            foreach (var hs in data.Heroes)
            {
                var heroData = allHeroData.FirstOrDefault(h => h.name == hs.HeroDataName);
                if (heroData == null) continue;

                HeroManager.Instance.AddHeroWithId(heroData, hs.Id);
                var hero = HeroManager.Instance.GetHero(hs.Id);

                if (hero == null) continue;

                for (int i = 1; i < hs.Level; i++)
                    hero.AddXP(hero.Data.GetXPForLevel(i));

                hero.IsDeployed = hs.IsDeployed;
                hero.IsInjured = hs.IsInjured;
                hero.InjuryTimer = hs.InjuryTimer;
                hero.Weapon = ApplyEquipment(hs.Weapon);
                hero.Armor = ApplyEquipment(hs.Armor);
                hero.Accessory = ApplyEquipment(hs.Accessory);
            }
        }

        private EquipmentItem ApplyEquipment(EquipmentSave es)
        {
            if (es == null) return null;
            return new EquipmentItem
            {
                Name = es.Name,
                Slot = (EquipmentSlot)es.Slot,
                AttackBonus = es.AttackBonus,
                DefenseBonus = es.DefenseBonus,
                HPBonus = es.HPBonus,
                Rarity = (HeroRarity)es.Rarity
            };
        }

        private void ApplyResearch(SaveData data)
        {
            if (Tech.ResearchManager.Instance == null) return;

            var allTechs = UnityEngine.Resources.LoadAll<Tech.TechNode>("TechNodes");
            foreach (var techName in data.ResearchedTechs)
            {
                var tech = allTechs.FirstOrDefault(t => t.name == techName);
                if (tech != null)
                {
                    tech.IsResearched = true;
                    tech.IsResearching = false;
                }
            }

            if (!string.IsNullOrEmpty(data.CurrentResearch))
            {
                var current = allTechs.FirstOrDefault(t => t.name == data.CurrentResearch);
                if (current != null)
                {
                    Tech.ResearchManager.Instance.StartResearch(current);
                }
            }
        }

        private void ApplyQuests(SaveData data)
        {
            if (QuestManager.Instance == null) return;

            var allQuests = UnityEngine.Resources.LoadAll<QuestData>("Quests");
            var questPool = allQuests.ToList();
            QuestManager.Instance.LoadQuests(questPool);

            foreach (var qs in data.Quests)
            {
                var questData = allQuests.FirstOrDefault(q => q.name == qs.QuestDataName);
                if (questData == null) continue;

                var instance = QuestManager.Instance.GetAllQuests().FirstOrDefault(q => q.Data == questData);
                if (instance == null) continue;

                if (Enum.TryParse<QuestStatus>(qs.Status, out var status))
                    instance.Status = status;

                foreach (var entry in qs.Progress)
                    instance.AddProgress(entry.Key, entry.Value);
            }
        }

        private void ApplyMutantAttack(SaveData data)
        {
            if (MutantAttackManager.Instance == null || data.MutantAttack == null) return;

            if (data.MutantAttack.CycleStarted)
            {
                MutantAttackManager.Instance.RestoreState(
                    data.MutantAttack.CurrentWaveIndex,
                    data.MutantAttack.AttackTimer);
            }
        }

        private void ApplyMap(SaveData data)
        {
            if (WorldMap.Instance == null || data.MapNodes == null || data.MapNodes.Count == 0) return;

            foreach (var ns in data.MapNodes)
            {
                var node = WorldMap.Instance.GetNode(ns.GridX, ns.GridY);
                if (node == null) continue;

                if (ns.IsExplored) node.SetExplored(true);
                if (ns.IsVisible) node.SetVisible(true);
            }
        }

        private void CaptureRoads(SaveData data)
        {
            if (RoadManager.Instance == null) return;
            foreach (var cell in RoadManager.Instance.GetRoadCellsForSave())
                data.RoadCells.Add(new IntIntEntry { Key = cell.x, Value = cell.y });
        }

        private void ApplyRoads(SaveData data)
        {
            if (RoadManager.Instance == null || data.RoadCells == null) return;
            var cells = new List<Vector2Int>();
            foreach (var rc in data.RoadCells)
                cells.Add(new Vector2Int(rc.Key, rc.Value));
            RoadManager.Instance.LoadRoadCells(cells);
        }

        private void ApplySettlers(SaveData data)
        {
            if (SettlerManager.Instance == null || data.Settlers == null) return;
            SettlerManager.Instance.LoadSettlers(data.Settlers);
        }

        #endregion

        #region File Helpers

        private string GetSaveFolderPath()
        {
            return Path.Combine(Application.persistentDataPath, SaveFolder);
        }

        private string GetSaveFilePath(string fileName)
        {
            return Path.Combine(GetSaveFolderPath(), fileName + FileExtension);
        }

        #endregion
    }
}