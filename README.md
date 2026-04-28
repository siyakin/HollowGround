# Hollow Ground

> Last War ilhamli, nukleer savas sonrasi strateji oyunu.  
> Sehir kurma + ordu yonetimi + hero sistemi + dunya kesfi.  
> **Tek kisi PvE** —Unity 6 + URP + 3D Low Poly–

---

## Ozet

2047. Kuresel nukleer catismadan sonra dunya taninmaz hale geldi. Sen, **Hollow Ground** olarak bilinen bir bolgede uyanan bir hayatta kalansin. Kullerden bir yerlesim kur, hayatta kalanlari bir araya getir ve bu yeni dunyada bir duzen sagla.

---

## Ozellikler

### Sehir Kurma
- **15 bina turu** — CommandCenter, Farm, Mine, Barracks, WaterWell, Generator, WoodFactory, Hospital, Storage, Shelter, Walls, WatchTower, Workshop, ResearchLab, TradeCenter
- Seviye sistemi (1-10), yukseltme, yikma, tamir
- Grid-tabanli yerlestirme, ghost preview, rotasyon
- **105 FBX model** — her bina 7 state (Construct, L01, L03, L05, L10, Damaged, Destroyed)
- Hasar/tamir mekanigi — mutant saldirilari binalari hasarlandirir

### Ordu Sistemi
- **5 birlik turu** — Infantry, Scout, Heavy, Sniper, Engineer
- Egitim kuyrugu, moral sistemi, guc hesabi
- Kisladan egitim, ordu kompozisyonu yonetimi

### Savas
- BattleCalculator — hasar/kayip hesaplama, matchup carpanlari
- 5 BattleTarget — savunma birlikleri, ganimet, mesafe
- Sefer sistemi — ilerleme cubugu, otomatik savas, savas raporu

### Hero Sistemi
- **5 hero rol** — Commander, Warrior, Ranger, Engineer, Scout
- Gacha summon (Common 50% > Legendary 1%)
- Seviye, XP, ekipman slotlari (silah, zirh, aksesuar)
- Ordu bonusu

### Dunya Haritasi
- 10x10 grid, fog of war, A* pathfinding
- 7 node tipi — keşfedilmemis alanlar "?" ile gosterilir
- Sefer gonderme, ilerleme takibi

### Ileri Sistemler
- **Tech Tree** — 10 arastirma, prerequisite zinciri
- **Faction Ticaret** — 3 NPC faction (Scavenger Guild, Iron Legion, Green Haven)
- **Gorev Sistemi** — 5 quest (10 daha planlaniyor), kabul/ilerleme/turn-in
- **Mutant Saldirilari** — zamanli dalga dognusu, uyari, savunma hesabi

### UI
- 15+ UI panel — BuildMenu, BuildingInfo, Training, Army, Hero, WorldMap, TechTree, FactionTrade, QuestLog, SaveMenu
- ActionBar ile panel toggle, PanelManager ile tek panel kurali
- Toast bildirimleri (15+ mesaj turu)
- UITheme sistemi — post-apokaliptik koyu tema

### Visual & Atmosfer
- **WeatherSystem** — 5 hava durumu (Clear, LightRain, HeavyRain, DustStorm, RadiationStorm)
- Grid overlay — yerlestirme modunda gorunur, footprint highlight
- Building highlight — secili bina outline efekti
- Damage effects — ates/duman particle, explosion burst
- Screen shake — Perlin noise ile sarsinti
- Atmosfer — dust/fog/embers particle efektleri
- Post-processing — bloom, vignette, color filter, chromatic aberration

### Save/Load
- JSON tabanli save/load sistemi
- Auto-save (5dk), QuickSave (F5), QuickLoad (F9)
- Save slot yonetimi

---

## Teknik

| Alan | Teknoloji |
|------|-----------|
| Motor | Unity 6 (6000.4) |
| Render Pipeline | URP |
| Gorsel Stil | 3D Low Poly |
| Input | Yeni Input System |
| UI Text | TextMeshPro |
| Bina Modelleri | Blender FBX (105 adet) |

### Mimari
- **Singleton pattern** — tum manager'lar `Singleton<T>` base class'indan inherit
- **Event-driven** — sistemler birbirini event ile haberverir, direkt cagri yok
- **ScriptableObject** — veri tanimlari SO ile
- **UIPrimitiveFactory** — merkezi UI olusturma utility'si
- **UIColors** — merkezi renk yonetimi
- **GameConfig SO** — dengeleme ve dev mod ayarlari

---

## Proje Yapisi

```
Assets/_Project/
├── Scripts/
│   ├── Core/        GameManager, TimeManager, Singleton, GameInitializer,
│   │                SaveData, SaveSystem, AudioManager, BaseStarter,
│   │                PostProcessingSetup, AtmosphereEffects, GameConfig,
│   │                SessionLogger, WeatherSystem, CostEntryHelper
│   ├── Camera/      StrategyCamera, ScreenShake
│   ├── Grid/        GridSystem, GridCell, GridVisualizer, GridOverlayRenderer
│   ├── Buildings/   BuildingType, BuildingData, Building, BuildingManager,
│   │                BuildingPlacer, BuildingSelector, BuildingDatabase,
│   │                BuildingConstructionAnimation, BuildingHighlight, DamageEffects
│   ├── Resources/   ResourceType, ResourceManager
│   ├── Army/        TroopType, TroopData, ArmyManager
│   ├── Combat/      BattleCalculator, BattleTarget, BattleManager,
│   │                MutantWave, MutantAttackManager
│   ├── Heroes/      HeroEnums, HeroData, Hero, HeroManager
│   ├── World/       MapNodeData, WorldMap, ExpeditionSystem
│   ├── Tech/        TechNode, ResearchManager
│   ├── NPCs/        FactionData, TradeSystem
│   ├── Quests/      QuestEnums, QuestData, QuestInstance, QuestManager
│   ├── UI/          UIManager, PanelManager, ResourceBarUI, BuildMenuUI,
│   │                BuildingInfoUI, ToastUI, TrainingPanelUI, ArmyPanelUI,
│   │                BattleReportUI, HeroPanelUI, WorldMapUI, TechTreeUI,
│   │                FactionTradeUI, QuestLogUI, SaveMenuUI, DebugHUD,
│   │                UIThemeSO, UIThemeTag, UIPrimitiveFactory, UIColors
│   └── Editor/      BuildingDataFactory, TroopDataFactory, HeroDataFactory,
│                     QuestDataFactory, FactionDataFactory, TechNodeFactory,
│                     SceneSetupEditor, GameConfigCreator, GroundSetupEditor
├── ScriptableObjects/
│   ├── Buildings/   10 aktif bina SO
│   ├── Troops/      5 birlik SO
│   ├── Heroes/      5 hero SO
│   ├── TechNodes/   10 teknoloji SO
│   ├── Factions/    3 faction SO
│   ├── Quests/      5 quest SO
│   └── Targets/     5 BattleTarget SO
├── Models/
│   ├── Buildings/   15 bina x 7 model = 105 FBX
│   ├── CityPack/
│   ├── PostApocolypsePack/
│   ├── SurvivalPack/
│   └── NaturePack/
└── Docs/
    ├── GDD.md       Oyun tasarim dokumani
    ├── ROADMAP.md   Gelistirme plani (14 faz tamamlandi)
    └── BALANCE.md   Dengeleme referans tablosu
```

---

## Gelistirme Durumu

**14 faz tamamlandi:**

| Faz | Aciklama |
|-----|----------|
| 1 | Temel altyapi: Camera, Grid, Resources, GameManager, Input |
| 2 | Base Building: 10 bina SO, ghost preview, grid snap |
| 3 | UI: 15 panel, UIManager, tum toggle metotlari |
| 4 | Askeri: 5 birlik, egitim kuyrugu, moral sistemi |
| 5 | Savas: BattleCalculator, BattleTarget, sefer sistemi |
| 6 | Hero: 5 rol, gacha summon, ekipman, XP |
| 7 | Dunya Haritasi: 10x10 grid, A*, fog of war, sefer |
| 8 | Ileri: Tech tree, Faction/Ticaret, Quest, Mutant saldiri |
| 9 | Save/Load + Audio: JSON save, auto-save, SFX pool |
| 10 | Content: BaseStarter, 3 faction, 10 tech, 15 quest, BALANCE.md |
| 11 | Playtest & Bugfix: 13/13 test gecti, GameConfig, SessionLogger |
| 12 | Bina Model Sistemi: 105 FBX, state-based model swap, hasar/tamir |
| 13 | Refactoring: Singleton<T>, UIPrimitiveFactory, UIColors, dead code silindi |
| 14 | Visual & Polish: Grid overlay, weather, highlight, damage efektleri |

### Bekleyen
- [ ] 10 ek quest SO (5 mevcut, 10 daha planlaniyor)
- [ ] Hero yetenek agaci gorsellestirme
- [ ] 5+ karakter modeli sahne yerlesimi
- [ ] Sahne dekorasyonu ve atmosfer

---

## Dokumantasyon

| Dosya | Aciklama |
|-------|----------|
| [GDD.md](Docs/GDD.md) | Oyun tasarim dokumani |
| [ROADMAP.md](Docs/ROADMAP.md) | Faz bazli gelistirme plani |
| [BALANCE.md](Docs/BALANCE.md) | Dengeleme referans tablosu |
| [AGENTS.md](AGENTS.md) | AI agent kurallari ve teknik rehber |

---

## Lisans

Ozel proje — tum haklari saklidir.
