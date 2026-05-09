# Hollow Ground

Nukleer savas sonrasi sehir kurma + strateji + RPG oyunu. Unity 6 + URP + 3D Low Poly.

## Hizli Baslangic

- **GDD**: [GDD.md](./GDD.md) — Oyun tasarim dokumani
- **Roadmap**: [ROADMAP.md](./ROADMAP.md) — Gelistirme plani
- **Balans**: [BALANCE.md](./BALANCE.md) — Oyun dengesi referansi
- **Kontroller**: [CONTROLS.md](./CONTROLS.md) — Klavye kisayollari arastirmasi
- **Modelleme**: [BLENDER_MODELING_GUIDE.md](./BLENDER_MODELING_GUIDE.md) — Blender rehberi
- **Bina Spec**: [BuildingSpecs/](./BuildingSpecs/) — Her bina icin ayri spec dokumani

## Durum: v0.28.0

19 faz tamamlandi, 2 playtest gecti (Faz 11: 13/13, Faz 19: 15/15).

### Tamamlanan Sistemler

| Modul | Detay |
|-------|-------|
| Grid System | 100x100 grid, cell state, snap-to-grid, terrain, water shader |
| Building System | 17 bina SO (15 + Garden + GardenLarge), 105 FBX model, state-based swap |
| Road System | BFS pathfinding, otomatik yol olusumu, orphan cleanup |
| Resource System | 6 kaynak, kapasite yonetimi, baslangic degerleri |
| Army System | 5 birlik SO, egitim kuyrugu, moral sistemi |
| Combat System | BattleCalculator + BattleTarget SO, sefer sistemi, mutant dalgalari |
| Hero System | 5 rol, 5 rarity, gacha summon, ekipman, XP |
| World Map | 10x10 grid, fog of war, A*, sefer sistemi |
| Tech Tree | 10 tech SO, prerequisite zinciri, arastirma kuyrugu |
| Faction/Trade | 3 faction SO, al/sat mekanizmasi, iliski sistemi |
| Quest System | 5 quest SO, 6 objective type, trigger sistemi |
| Save/Load | JSON serilestirme, auto-save, full state capture |
| Settler Walker | WalkerBase + WalkerManager, pool, cell occupancy, animasyon |
| Settler Job | 12 rol, auto-assign, isci bazli uretim, SettlerPanelUI |
| Garden Merge | 4 kucuk → 1 buyuk garden merge, NeedsRoads flag |
| Minimap | RenderTexture, viewport frame, tiklama navigasyonu |
| UI (16+ Panel) | ResourceBar, BuildMenu, BuildingInfo, WorldMap, TechTree, FactionTrade, QuestLog, SaveMenu, Hero, Training, Army, BattleReport, Settler, SettlerInfo, DebugHUD, Minimap |
| Editor Tools | 8+ fabrika araci, custom inspector, scene setup menuleri |
| Visual | Post-processing, weather (5 durum), highlight, damage efektleri, grid overlay |

### Bekleyen

- 7 acik issue (#34-#40)
- 10 ek quest SO
- Garden FBX (L03/L05/L10/Damaged/Destroyed) + Save/Load merge state
- NPC Visual Feedback, SettlerPanel Enrichment, Quick Tooltips
- Karakter modelleri, sahne dekorasyonu

## Proje Yapisi

```
Assets/_Project/
├── Scripts/           ~80+ C# script — 15+ modul
│   ├── Core/          GameManager, SaveSystem, TimeManager, GameInitializer, AudioManager, WeatherSystem
│   ├── Camera/        StrategyCamera, ScreenShake, MinimapCamera
│   ├── Grid/          GridSystem, GridVisualizer, GridOverlayRenderer, MapRenderer, WaterSurface
│   ├── Buildings/     BuildingData, Building, BuildingManager, BuildingPlacer, GardenManager
│   ├── Roads/         RoadManager, RoadVisualizer
│   ├── Resources/     ResourceType, ResourceManager
│   ├── Army/          TroopData, ArmyManager
│   ├── Combat/        BattleCalculator, BattleManager, MutantAttackManager
│   ├── Heroes/        HeroData, Hero, HeroManager
│   ├── World/         MapNodeData, WorldMap, ExpeditionSystem
│   ├── Tech/          TechNode, ResearchManager
│   ├── NPCs/          FactionData, TradeSystem, SettlerManager, WalkerManager
│   ├── Domain/        WalkerStateMachine, BattleCalc, ProductionCalc, PathfinderService
│   ├── Quests/        QuestData, QuestInstance, QuestManager
│   ├── UI/            16+ panel script, UIPrimitiveFactory, UIColors, UIThemeSO
│   └── Editor/        8+ editor araci (SO fabrikalari, scene setup, FBX binder)
├── ScriptableObjects/ Buildings, Troops, Heroes, TechNodes, Factions, Quests, Targets, Maps
├── Models/            4 model paketi + 105 bina FBX (Buildings/)
├── Prefabs/           ToastItem, UI prefabs
├── Settings/          StrategyControls.inputactions, RenderTexture
├── Shaders/           Water.shader (URP custom)
└── Docs/              GDD, ROADMAP, BALANCE, CONTROLS, BuildingSpecs, Blender rehberleri
```

## Teknik Stack

- **Unity 6** + **URP**
- **C#** (.NET Standard 2.1)
- **New Input System**
- **TextMeshPro** (UI)
