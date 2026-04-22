# Hollow Ground — AGENTS.md

## Proje Ozeti

**Hollow Ground** — Last War ilhamli, nukleer savas sonrasi strateji oyunu.
Tek kisi PvE: Sehir kurma + ordu yonetimi + hero sistemi + dunya kesfi.
**Motor:** Unity 6 + URP + 3D Low Poly + Yeni Input System + TextMeshPro

---

## Kritik Kurallar

### Dil ve Karakter
- Tum script dosyalari, SO dosyalari, Display Name'ler **Ingilizce** olmali
- **Turkce karakter YASAK** dosya adlarinda: c, g, o, u, s, i yerine English karsiliklari
- Kod yorumlari (comment) eklenmez, kullanici ozelle istemedikce

### Namespace Cakismalari
- `HollowGround.Resources` namespace'i `UnityEngine.Resources` ile cakisiyor → `UnityEngine.Resources.LoadAll<T>()` seklinde tam nitelik kullan
- `HollowGround.Camera` namespace'i `UnityEngine.Camera` ile cakisiyor → `UnityEngine.Camera.main` seklinde tam nitelik kullan

### Unity API
- **Eski Input System YASAK**: `Input.GetMouseButtonDown` yerine `Mouse.current.leftButton.wasPressedThisFrame`
- **TextMeshPro**: UI elementleri icin `UI > Button - TextMeshPro` ve `UI > Text - TextMeshPro`
- **FindObjectOfType YASAK**: Unity 6'da deprecated. `FindAnyObjectByType<T>()` kullan
- **FindFirstObjectByType YASAK**: Unity 6'da deprecated (instance ID ordering). `FindAnyObjectByType<T>()` kullan
- **FindObjectsByType(FindObjectsSortMode) YASAK**: Unity 6'da deprecated. `FindObjectsByType<T>()` veya `FindObjectsByType<T>(FindObjectsInactive)` kullan
- **Dictionary serialize olmaz**: `Dictionary<K,V>` Inspector'da gorunmez. Her zaman `List<Entry>` pattern'i kullan

### Pattern'ler
- Manager'lar **Singleton** pattern kullanir (Instance property)
- Sistemler birbirini **event** ile haber verir, direkt cagri yok
- Veriler **ScriptableObject** ile tanimlanir
- UI panelleri `UIManager.ToggleXxx()` ile acilir/kapanir

---

## Proje Yapisi

```
Assets/_Project/
├── Scripts/
│   ├── Core/        GameManager, TimeManager, GameEvent, Singleton, GameInitializer,
│   │                SaveData, SaveSystem, AudioManager, AudioConfig, BaseStarter,
│   │                PostProcessingSetup, AtmosphereEffects, GameConfig, SessionLogger
│   ├── Camera/      StrategyCamera
│   ├── Grid/        GridSystem, GridCell, GridVisualizer, PlacementValidator
│   ├── Buildings/   BuildingType, BuildingData, Building, BuildingManager,
│   │                BuildingPlacer, BuildingSelector, BuildingDatabase,
│   │                BuildingConstructionAnimation
│   ├── Resources/   ResourceType, ResourceManager
│   ├── Army/        TroopType, TroopData, ArmyManager
│   ├── Combat/      BattleCalculator, BattleTarget, BattleManager,
│   │                MutantWave, MutantAttackManager
│   ├── Heroes/      HeroEnums, HeroData, Hero, HeroManager
│   ├── World/       MapNodeData, WorldMap, ExpeditionSystem
│   ├── Tech/        TechNode, ResearchManager
│   ├── NPCs/        FactionData, TradeSystem
│   ├── Quests/      QuestEnums, QuestData, QuestInstance, QuestManager
│   ├── UI/          UIManager, ResourceBarUI, BuildMenuUI, BuildingInfoUI,
│   │                ToastUI, TrainingPanelUI, ArmyPanelUI, BattleReportUI,
│   │                HeroPanelUI, WorldMapUI, TechTreeUI, FactionTradeUI,
│   │                QuestLogUI, SaveMenuUI, DebugHUD,
│   │                UIThemeSO, UIThemeTag
│   └── Editor/      GridSystemEditor, BuildingDataFactory, TroopDataFactory,
│                     HeroDataFactory, QuestDataFactory, FactionDataFactory,
│                     TechNodeFactory, GhostMaterialCreator,
│                     UIThemeApplier, SceneSetupEditor, GameConfigCreator,
│                     PostProcessingProfileFactory, GroundSetupEditor
├── ScriptableObjects/
│   ├── Buildings/   9 aktif SO + 10 yedek (silinmesi gerekiyor)
│   ├── Targets/     5 BattleTarget SO
│   ├── Troops/      Klasor var, SO'lar henutz olusturulmadi
│   ├── Heroes/      Klasor henutz olusturulmadi
│   ├── TechNodes/   Klasor henutz olusturulmadi
│   ├── Factions/    Klasor henuzz olusturulmadi
│   └── Quests/      5 quest SO olusturuldu, 10 daha eklenmeli
├── Models/
│   ├── CityPack/    Bina modelleri, karakterler, prop'lar
│   ├── PostApocolypsePack/ Yapilar, zombiler, silahlar
│   ├── SurvivalPack/ Loot/prop objeler
│   └── NaturePack/  Agaclar, kayalar, terrain
├── Prefabs/
│   ├── ToastItem.prefab
│   └── UI/          NodeButton prefab henuzz olusturulmadi
├── Settings/
│   └── StrategyControls.inputactions
└── Docs/
    ├── GDD.md       Oyun tasarim dokumani
    ├── ROADMAP.md   Gelistirme plani (10 faz tamamlandi)
    └── BALANCE.md   Dengeleme referans tablosu
```

---

## Sahne Obje Yapisi

### GameManager Objeleri (GameManager GameObject uzerinde)
GameManager, TimeManager, ResourceManager, GridSystem, GridVisualizer,
BuildingPlacer, BuildingSelector, BuildingManager, ArmyManager,
BattleManager, HeroManager, WorldMap, ExpeditionSystem,
QuestManager, MutantAttackManager, ResearchManager, TradeSystem,
SaveSystem, BaseStarter, GameInitializer

### GameCanvas Alt Yapisi
- ResourceBar
- ActionBar (Yapi, Arastir, Ordu, Hero, Gorev, Harita, Ticaret butonlari — hepsi bagli)
- BuildMenu (3 buton: CommandCenter, Farm, Mine — kaynak kontrolu calisiyor)
- TrainingPanel
- BattleReportPanel
- HeroPanel
- WorldMapPanel (MapGrid + NodeInfoPanel + ExpeditionPanel)
- QuestLogPanel (kurulum yapildi)
- TechTreePanel (SceneSetupEditor ile otomatik)
- FactionTradePanel (SceneSetupEditor ile otomatik)
- SaveMenuPanel (kurulum yapildi)
- PausePanel (kurulum yapildi)
- DebugPanel (DebugText + DebugHUD)
- UIManager objesi (UIManager component + tum panel referanslari)

### Camera
- CameraRig altinda Main Camera (MainCamera tag'i atanmali, tek Audio Listener olmali)

---

## Tamamlanan Fazlar (1-10)

| Faz | Durum | Aciklama |
|-----|-------|----------|
| 1 | ✅ | Temel altyapi: Camera, Grid, Resources, GameManager, Input |
| 2 | ✅ | Base Building: 9 bina SO, ghost preview, grid snap |
| 3 | ✅ | UI: 15 panel, UIManager, tum toggle metotlari |
| 4 | ✅ | Askeri: 5 birlik, egitim kuyrugu, moral sistemi |
| 5 | ✅ | Savas: BattleCalculator, BattleTarget, sefer sistemi |
| 6 | ✅ | Hero: 5 rol, gacha summon, ekipman, XP |
| 7 | ✅ | Dunya Haritasi: 10x10 grid, A*, fog of war, sefer |
| 8 | ✅ | Ileri: Tech tree, Faction/Ticaret, Quest, Mutant saldiri |
| 9 | ✅ | Save/Load + Audio: JSON save, auto-save, SFX pool |
| 10 | ✅ | Content: BaseStarter, 3 faction, 10 tech, 15 quest, BALANCE.md |

---

## Bilinen Eksikler ve Sonraki Adimlar

### Playtest Faz 11 (Tamamlandi) ✅

Tum sistemler playtest edildi, 13/13 test gecti:

| # | Test | Durum |
|---|------|-------|
| 1 | Oyun Baslangic | ✅ Kamera, WASD, zoom, GameManager, ResourceBar |
| 2 | Bina Yerlestirme | ✅ CC, Farm, Mine, Barracks — toast + kaynak dususu |
| 3 | Kaynak Uretim | ✅ Farm→Food, Mine→Metal uretimi |
| 4 | UI Paneller | ✅ BuildMenu, BuildingInfo, Upgrade/Demolish |
| 5 | Askeri Sistem | ✅ Barracks→Infantry egitimi, army summary |
| 6 | Hero Sistemi | ✅ HeroPanel summon, DevMode ile TechPart boost |
| 7 | Dunya Haritasi | ✅ WorldMapUI, node secimi, sefer sistemi |
| 8 | Arastirma | ✅ TechTreeUI, 10 tech SO, START RESEARCH |
| 9 | Faction Ticaret | ✅ 3 faction SO, BUY/SELL, iliski sistemi |
| 10 | Gorev Sistemi | ✅ QuestLogUI, 5 quest SO, ACCEPT/TURN IN |
| 11 | Mutant Saldirisi | ✅ Warning toast, attack, bina yikimi, session log |
| 12 | Save/Load | ✅ F5 QuickSave, F9 QuickLoad, JSON |
| 13 | Zaman Kontrolu | ✅ TimeDisplayUI, hiz degisimi |

**Playtest'te eklenen/duzeltilen sistemler:**
- GameConfig SO — DevMode, DisableMutantAttacks, BoostStartingResources, SessionLog toggle
- SessionLogger — tum oyun eventlerini dosyaya yazar (persistentDataPath/SessionLogs/)
- DevMode ile test hizlandirma (0.1x build/production/training/research/expedition/mutant interval)
- Tum UI panelleri runtime olusturucuya cevrildi (TrainingPanel, HeroPanel, QuestLog, TechTree, SaveMenu, FactionTrade)
- UITheme font (Roboto) runtime panellere uygulaniyor
- Paneller alttan 60px ActionBar padding ile aciliyor
- Close butonlari kaldirildi, ActionBar toggle ile acilip kapaniyor
- Toast mesajlari: mutant warning/attack/victory/defeat
- F5/F9 QuickSave/QuickLoad eklendi
- Training/Research speed multiplier bug'i duzeltildi (speed /= devMult)
- SaveSystem dosya adi uyumsuzlugu duzeltildi
- ResearchManager sahnede eksikti — eklendi
- 3 FactionData SO olusturuldu (Scavenger Guild, Iron Legion, Green Haven)

### SO'lar Olusturulmadi (Editor'de yapilmali)
- `ScriptableObjects/Troops/` — 5 birlik SO (TroopDataFactory ile)
- `ScriptableObjects/Heroes/` — 5 hero SO (HeroDataFactory ile)
- `ScriptableObjects/Quests/` — 10 ek quest SO (QuestDataFactory ile)

### SO'lar Olusturuldu
- `ScriptableObjects/TechNodes/` — 10 teknoloji SO ✅
- `ScriptableObjects/Factions/` — 3 faction SO ✅

### Sahne Kurulumlari
- `HollowGround > Setup Ground & Camera` ile ground plane + camera + lighting kalıcı olarak sahneye eklenir
- Ground layer = "Ground" (Layer 8), BuildingPlacer._groundMask = Ground
- CameraRig > Main Camera tag = MainCamera olmalı
- Camera.main null olabilir → BuildingPlacer _cam ile cache edilir
- GameInitializer artık ground oluşturmaz, sadece kamerayı merkezler

### Gorsel/Polish (Editor isi, script gerektirmez)
- Post-processing (bloom 0.2, vignette 0.2, filmgrain kapalı) — PostProcessingSetup runtime
- Atmosfer efektleri varsayılan kapalı (dust/fog particles) — AtmosphereEffects inspector'dan açılır
- Bina inşaat animasyonu — BuildingConstructionAnimation otomatik eklenir
- 15+ bina modeli sahne yerlesimi
- 5+ karakter modeli sahne yerlesimi

### Potansiyel Bug'lar
- `GameInitializer.Start()` ile `GameManager.StartGame()` cagrilir ama sahnede `GameInitializer` yoksa oyun Playing state'e gecmez
- `BaseStarter.SetupBase()` manuel tetiklenmeli (ContextMenu) veya GameInitializer'a entegre edilmeli
- WorldMap.GenerateDefaultMap() runtime'da SO olusturur (ScriptableObject.CreateInstance) — save/load ile uyumlu degil

---

## Kesfedilen Tuzaklar (Discoveries)

1. `Dictionary<K,V>` Unity'de serialize olmaz → `List<Entry>` kullanildi (BuildingData.CostEntry, MutantWave.PenaltyEntry)
2. `ResourceType?` (nullable enum) serialization sorunu → `bool HasProduction` alani eklendi
3. GameCanvas'a Layout Group EKLENMEZ — cocuklarin boyutunu bozar, anchor'lari kirar
4. Canvas Scaler: `Scale With Screen Size`, Reference: 1920x1080. Game penceresinde Free Aspect yerine 16:9
5. TMP_Text ve Image ayni objede OLAMAZ — ayri parent/child yapilmali
6. Iki Audio Listener olmamali — Main Camera silinip CameraRig altindaki kameraya MainCamera tag'i atanmali
7. CommandCenterLevelRequired: 0 olmali ki ilk bina yerlestirilebilsin
8. GridSystem `_cells` dizisi Edit mode'da null — Editor script'te `Application.isPlaying` kontrolu eklendi
9. SO dosyalarinda Turkce karakter dosya adlari Unity'de bozulur
10. `FindObjectOfType` Unity 6'da deprecated → `FindAnyObjectByType`
11. `new() { new NestedType }` seklinde list initialization Editor script'lerde nested type cozemiyor → acik `List<FactionData.TradeOffer>` yazilmali
12. `FindFirstObjectByType` Unity 6'da deprecated (instance ID ordering) → `FindAnyObjectByType` kullanilmali
13. `FindObjectsByType(FindObjectsSortMode)` Unity 6'da deprecated → `FindObjectsByType<T>()` veya `FindObjectsByType<T>(FindObjectsInactive)` kullanilmali
14. Ground'u runtime'da oluşturma → editor'de kalıcı oluştur (HollowGround > Setup Ground & Camera). Aksi takdirde çift ground, z-fighting, layer sorunu
15. BuildingPlacer'da Camera.main null dönebilir → `_cam` field ile Awake'de cache et
16. AtmosphereEffects varsayılanları agresif olmamalı: fog density 0.004, dust/fog particles kapalı
17. GroundManager + GameInitializer aynı anda ground üretmez — sadece bir tanesi yapmalı

---

## UI Layout Kurallari

- PausePanel, SaveMenu gibi paneller **default kapali** olmali (SetActive false)
- Buton gruplari icin parent'a `Horizontal Layout Group` veya `Vertical Layout Group` ekle
- Child Force Expand: Width ✅ Height ❌ (genellikle)
- Her butonun `Layout Element`: Min Width 120, Min Height 40
- Buton onClick baglantisi: Obje surukle → dropdown'dan script/metot sec
- `BuildMenuUI` gibi component'lerde SerializeField ile referans baglanir

---

## Dengeleme Kaynaklari

Tum dengeleme degerleri `BALANCE.md` dosyasinda:
- Bina maliyetleri ve uretim oranlari (seviye 1-5 ornekleri)
- Asker egitim maliyeti ve guc referansi
- Mutant dalga gucleri (1-10)
- Hero gacha oranlari (Common 50% → Legendary 1%)
- Faction baslangic iliskileri
- Teknoloji arastirma maliyetleri
- Quest odul referansi
- Kaynak baslangic degerleri
