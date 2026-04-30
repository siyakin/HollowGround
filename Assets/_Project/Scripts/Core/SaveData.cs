using System;
using System.Collections.Generic;
using HollowGround.NPCs;
using UnityEngine;

namespace HollowGround.Core
{
    [Serializable]
    public class SaveData
    {
        public string SaveName;
        public string SaveDate;
        public float PlayTime;

        public ResourceSave Resources;
        public TimeSave Time;
        public List<BuildingSave> Buildings = new();
        public ArmySave Army;
        public List<HeroSave> Heroes = new();
        public List<string> ResearchedTechs = new();
        public string CurrentResearch;
        public float CurrentResearchProgress;
        public List<QuestSave> Quests = new();
        public MutantAttackSave MutantAttack;
        public List<MapNodeSave> MapNodes = new();
        public List<IntIntEntry> RoadCells = new();
        public List<SettlerWalkerSave> Settlers = new();
    }

    [Serializable]
    public class StringIntEntry
    {
        public string Key;
        public int Value;
    }

    [Serializable]
    public class IntIntEntry
    {
        public int Key;
        public int Value;
    }

    [Serializable]
    public class ResourceSave
    {
        public List<StringIntEntry> Amounts = new();
        public List<StringIntEntry> Capacities = new();
    }

    [Serializable]
    public class TimeSave
    {
        public float GameTime;
        public int GameSpeed;
    }

    [Serializable]
    public class BuildingSave
    {
        public string BuildingDataName;
        public int GridX;
        public int GridZ;
        public int Level;
        public string State;
        public float ConstructionProgress;
        public float UpgradeProgress;
        public float ProductionTimer;
        public int Rotation;
    }

    [Serializable]
    public class ArmySave
    {
        public List<StringIntEntry> Troops = new();
        public List<TrainingSave> TrainingQueue = new();
    }

    [Serializable]
    public class TrainingSave
    {
        public string TroopDataName;
        public int Amount;
        public float RemainingTime;
        public float TotalTime;
    }

    [Serializable]
    public class HeroSave
    {
        public string Id;
        public string HeroDataName;
        public int Level;
        public int CurrentXP;
        public bool IsDeployed;
        public bool IsInjured;
        public float InjuryTimer;
        public EquipmentSave Weapon;
        public EquipmentSave Armor;
        public EquipmentSave Accessory;
    }

    [Serializable]
    public class EquipmentSave
    {
        public string Name;
        public int Slot;
        public int AttackBonus;
        public int DefenseBonus;
        public int HPBonus;
        public int Rarity;
    }

    [Serializable]
    public class QuestSave
    {
        public string QuestDataName;
        public string Status;
        public List<IntIntEntry> Progress = new();
    }

    [Serializable]
    public class MutantAttackSave
    {
        public int CurrentWaveIndex;
        public float AttackTimer;
        public bool CycleStarted;
    }

    [Serializable]
    public class MapNodeSave
    {
        public int GridX;
        public int GridY;
        public string NodeType;
        public bool IsExplored;
        public bool IsVisible;
    }
}