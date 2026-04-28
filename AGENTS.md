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
- **GitHub push oncesi derleme kontrolu ZORUNLU**: Her `git push` oncesi Unity'de derleme hatasi olmadigi dogrulanmali. Hata varsa push YASAK

### Pattern'ler
- Manager'lar **Singleton** pattern kullanir (Instance property)
- Sistemler birbirini **event** ile haber verir, direkt cagri yok
- Veriler **ScriptableObject** ile tanimlanir
- UI panelleri `UIManager.ToggleXxx()` ile acilir/kapanir
- Kodla yaratilan her UI elementine (Button, Label, Panel) **UIThemeTag** eklenmeli — `MakeButton` ve `MakeLabel` helper'lari otomatik ekler, semantic tag'i parametre olarak ver (orn: `UIStyleType.HeaderText`, `UIStyleType.DangerButton`)

---

## Proje Yapisi

```
Assets/_Project/
├── Scripts/
│   ├── Core/        GameManager, TimeManager, Singleton, GameInitializer,
│   │                SaveData, SaveSystem, AudioManager, BaseStarter,
│   │                PostProcessingSetup, AtmosphereEffects, GameConfig, SessionLogger,
│   │                WeatherSystem, CostEntryHelper
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
│   ├── UI/          UIManager, PanelManager, ResourceBarUI, BuildMenuUI, BuildingInfoUI,
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
SaveSystem, BaseStarter, GameInitializer, WeatherSystem

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
- PausePanel (ESC ile acilir, Resume/Save-Load/Quit butonlari, runtime olusturulur)
- DebugPanel (DebugText + DebugHUD)
- UIManager objesi (UIManager component + tum panel referanslari)

### Camera
- CameraRig altinda Main Camera (MainCamera tag'i atanmali, tek Audio Listener olmali)
- ScreenShake component CameraRig uzerinde

---

## Tamamlanan Fazlar (1-14)

| Faz | Durum | Aciklama |
|-----|-------|----------|
| 1 | ✅ | Temel altyapi: Camera, Grid, Resources, GameManager, Input |
| 2 | ✅ | Base Building: 10 bina SO, ghost preview, grid snap |
| 3 | ✅ | UI: 15 panel, UIManager, tum toggle metotlari |
| 4 | ✅ | Askeri: 5 birlik, egitim kuyrugu, moral sistemi |
| 5 | ✅ | Savas: BattleCalculator, BattleTarget, sefer sistemi |
| 6 | ✅ | Hero: 5 rol, gacha summon, ekipman, XP |
| 7 | ✅ | Dunya Haritasi: 10x10 grid, A*, fog of war, sefer |
| 8 | ✅ | Ileri: Tech tree, Faction/Ticaret, Quest, Mutant saldiri |
| 9 | ✅ | Save/Load + Audio: JSON save, auto-save, SFX pool |
| 10 | ✅ | Content: BaseStarter, 3 faction, 10 tech, 15 quest, BALANCE.md |
| 11 | ✅ | Playtest & Bugfix: 13/13 test gecti, GameConfig, SessionLogger |
| 12 | ✅ | Bina Model Sistemi: 105 FBX, state-based model swap, hasar/tamir |
| 13 | ✅ | Refactoring: Singleton<T>, UIPrimitiveFactory, UIColors, dead code silindi |
| 14 | ✅ | Visual & Polish: Grid overlay, weather, highlight, damage efektleri |

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
| 13 | Zaman Kontrolu | ✅ ResourceBarUI _timeText, hiz degisimi |

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
- `ScriptableObjects/Quests/` — 10 ek quest SO (QuestDataFactory ile) (5 mevcut, 10 daha eklenmeli)

### SO'lar Olusturuldu
- `ScriptableObjects/Buildings/` — 10 aktif bina SO ✅
- `ScriptableObjects/Troops/` — 5 birlik SO (Infantry, Scout, Heavy, Sniper, Engineer) ✅
- `ScriptableObjects/Heroes/` — 5 hero SO (Commander, Warrior, Ranger, Engineer, Scout) ✅
- `ScriptableObjects/TechNodes/` — 10 teknoloji SO ✅
- `ScriptableObjects/Factions/` — 3 faction SO ✅
- `ScriptableObjects/Quests/` — 5 quest SO ✅
- `ScriptableObjects/Targets/` — 5 BattleTarget SO ✅

### Sahne Kurulumlari
- `HollowGround > Setup Ground & Camera` ile ground plane + camera + lighting kalıcı olarak sahneye eklenir
- Ground layer = "Ground" (Layer 8), BuildingPlacer._groundMask = Ground
- CameraRig > Main Camera tag = MainCamera olmalı
- Camera.main null olabilir → BuildingPlacer _cam ile cache edilir
- GameInitializer artık ground oluşturmaz, sadece kamerayı merkezler

### Bina Modelleri (Blender → Unity)
- Rehber: `Docs/BLENDER_MODELING_GUIDE.md` — olculer, renk paleti, seviye stratejisi
- Prompt serisi: `Docs/BLENDER_PROMPTS.md` — her bina icin kopyala-yapistir prompt'lar
- Blender Z-up, Unity Y-up. FBX export: -Z forward, Y up, Apply Transform ON
- Grid cell: 2m. 1x1 footprint max 1.9x1.9m, 2x2 footprint max 3.9x3.9m
- Her bina: L01, L03, L05, L10 (active) + Construct + Damaged + Destroyed = 7 model
- Vertex color: R=rust, G=moss, B=dirt. Materyaller: 6-8 slot Principled BSDF
- **15 bina x 7 model = 105 FBX tamamlandi** (CC, Farm, Mine, Barracks, WaterWell, Generator, WoodFactory, Hospital, Storage, Shelter, Walls, WatchTower, Workshop, ResearchLab, TradeCenter)
- **Claude** Blender modelleme icin en basarili model, **Grok 4.3** parametrik yaklasimda iyi
- Tum bina spec'leri: `Docs/BuildingSpecs/` klasorunde (her bina icin ayri .md)
- Bina rehberi: `Docs/BUILDING_GUIDE.md` (oyuncu + tasarim referans)
- FBX import: `Assets/_Project/Models/Buildings/{BuildingName}/` altina

### Bina Model Sistemi (Faz 12 — Tamamlandi) ✅

### Refactoring Faz 13 (Tamamlandi) ✅

### Visual Faz 14 (Tamamlandi) ✅

**Runtime Grid Overlay:**
- `GridOverlayRenderer.cs` — LineRenderer ile yerlestirme modunda grid gorunur
- Snake/zigzag pattern (2 LineRenderer: H+V), camera-relative culling (30 hucre)
- Smooth fade-in/out (0.3s), bina footprint highlight (yesil/kirmizi, rotation destekli)
- `BuildingPlacer.CurrentRotation` property eklendi
- Grid lines hucre koselerine cizilir (WorldPos = center - halfCell), footprint highlight merkez bazli

**Weather System + Atmosfer:**
- `WeatherSystem.cs` — 5 hava durumu: Clear, LightRain, HeavyRain, DustStorm, RadiationStorm
- Auto-cycle 60-180s, weighted random (40/20/10/20/10%), 5s smooth transition
- Per-weather: post-processing (vignette, saturation, color filter, chromatic aberration)
- Per-weather: fog color/density, ambient lighting, particle systems
- Events: OnWeatherChanged, OnRadiationStormStart, OnRadiationStormEnd
- AtmosphereEffects: dust/fog varsayilan aktif, yeni Embers particle (kor parçaciklari)
- PostProcessingSetup: SetColorFilter(), SetChromaticAberration() API eklendi

**Bina Secim + Hasar Efektleri:**
- `BuildingHighlight.cs` — 1.05x outline mesh, URP Unlit transparent, pulsing alpha
- `DamageEffects.cs` — 3 fire emitter (additive blend), 2 smoke emitter, explosion burst
- `ScreenShake.cs` — Perlin noise shake, LateUpdate, exponential decay
- Auto-add: Building.Initialize() → BuildingHighlight + DamageEffects

**Particle Shader Fix:**
- Tum runtime ParticleSystem'ler `Universal Render Pipeline/Particles/Unlit` shader kullanmali
- `ApplyURPParticleMaterial()` helper her particle olusturulduktan hemen sonra cagrilmali
- Built-in `Particles/Standard Unlit` URP'de pembe/magenta flash verir

**Pause Menu (ESC):**
- ESC tuşu ile pause/resume toggle
- Runtime olusturulan PausePanel: Resume, Save/Load, Quit butonlari
- SaveMenuUI'ye Back butonu eklendi
- GameManager.TogglePause() + TimeManager.TogglePause() entegrasyonu

**Merkezi Altyapi Oluşturuldu:**
- `Singleton<T>` base class: `protected set Instance`, `OnDestroy` ile Instance temizleme, `Destroy(gameObject)` duplicate koruması
- `UIPrimitiveFactory`: 10+ static metod (CreateUIObject, AddThemedText, AddImage, CreateButton, StretchFull, SetAnchors, SetupPanelBackground, AddStandardVLG, AddRowHLG, AddLayoutElement)
- `UIColors`: PanelColors struct (PanelBg, RowBg, Text, Muted, Ok, Gold, Danger, Warn) + GetRarityColor, GetNodeColor, GetStateColor + Fog, Empty, Selected, PanelInner, TextDim
- `CostEntryHelper.Costs()`: Merkezi maliyet oluşturma utility

**Factory'ler Merkezi Utility'ye Taşındı:**
- BuildingDataFactory, TroopDataFactory, TechNodeFactory → lokal `Costs()` metodları kaldırıldı, `CostEntryHelper.Costs()` kullanılıyor

**4 UI Panel UIPrimitiveFactory + UIColors'a Taşındı:**
- FactionTradeUI, SaveMenuUI, TechTreeUI, WorldMapUI → tüm lokal CreateUIObject/AddText/CreateButton/StretchFull/SetAnchors metodları kaldırıldı
- Tüm inline `static readonly Color` tanımları UIColors'a taşındı
- BuildingInfoUI state renkleri → `UIColors.GetStateColor()`
- UIManager QuickSave/Load renkleri → UIColors.Default.Ok/Warn
- ToastUI bg → UIColors.Default.PanelBg
- UIPrimitiveFactory buton renkleri → UIColors.Default

**Magic Numbers → GameConfig SO'ya Taşındı:**
- `DemolishRefundRatio` (0.5f) — Building.cs Demolish()
- `RepairCostRatio` (0.5f) — Building.cs Repair()
- `WallDefenseBonus` (20) — MutantAttackManager CalculateDefensePower()
- `DefeatTroopLossRatio` (0.6f) — MutantAttackManager ExecuteWave()
- `ArmyManager.CalculateArmyPower()` → `* 10` hard-code yerine `TroopData.BaseAttack` (cache pattern ile)
- `GameConfigCreator` yeni alanları içeriyor

**Ölü Kod Silindi:**
- `GameEvent.cs` — C# `event Action<T>` kullanılıyor
- `PlacementValidator.cs` — GridSystem direkt kullanılıyor

**ToastUI Yeniden Yazıldı:**
- Singleton<T> inheritance kaldırıldı → basit `_instance` field
- `FindAnyObjectByType<ToastUI>(FindObjectsInactive.Include)` ile lazy activation
- `EnsureContainer()` ile runtime container oluşturma
- ToastPanel sahnede her zaman aktif olmalı

**SessionLogger Event Subscription Düzeltmesi:**
- `SubscribeEvents()` artık `EnableSessionLog`'dan bağımsız — toast'lar SessionLog kapalıyken de çalışıyor

**Toast Mesajları Eklendi (15+ yeni mesaj):**
- Bina: placed, built, upgraded, damaged, repaired, demolished, destroy
- Kaynak eksik: "Not enough... Food 5 short, Metal 10 short"
- CC level eksik: "Need Command Center Lv.2!"
- Upgrade/Repair kaynak eksik detaylı mesajlar
- Mutant: warning, attack, victory, defeat
- Araştırma tamam, sefer varış
- BuildMenuUI maliyet metni eksik kaynakları kırmızı gösteriyor
- BuildMenuUI butonları her zaman tıklanabilir — eksik kaynak toast ile gösteriliyor
- **BuildingData.BuildingModels** struct: 7 GameObject slot (Construct, L01, L03, L05, L10, Damaged, Destroyed)
- **Level threshold**: L01 (lv1-2), L03 (lv3-4), L05 (lv5-9), L10 (lv10)
- **State-based model swap**: Building.cs `UpdateModel()` state'e gore dogru modeli instantiate eder
  - Constructing → ConstructModel (fallback: L01)
  - Active/Upgrading → LevelModels (threshold'a gore)
  - Damaged → DamagedModel (fallback: level model)
  - Destroyed → DestroyedModel (2.5sn gosterim → otomatik kaldirma)
- **Z-fighting fix**: Model `localPosition.y = 0.015f` (1.5cm offset)
- **Ghost placement fix**: BuildingPlacer `_cachedCoords` / `_cachedWorldPos` ile ghost ve yerlestirme uyumu
- **Hasar/Tamir sistemi**: `ApplyDamage()` → Damaged state, `Repair()` → kaynak harcayip Active'e donme
- **MutantAttackManager**: Yenilgi durumunda `ApplyBuildingDamage()` hasar sayisini dondurur, toast ile REPAIR uyari
- **BuildingInfoUI**: SmartPosition (bina ekran pozisyonuna gore panel konumlanir, binayi ortemez), Repair butonu, state renk kodlamasi
- **SessionLogger**: OnDamaged/OnRepaired eventleri loglaniyor
- **Editor araçları**:
  - `HollowGround/FBX/Configure All Building FBX Imports` — 105 FBX toplu import ayari
  - `HollowGround/Models/Bind All Building Models` — FBX → BuildingData SO otomatik baglama
  - `HollowGround/Models/Show Binding Report` — her bina icin 7/7 durum raporu
- **UIThemeTag**: MakeLabel/MakeButton helper'lari otomatik UIThemeTag ekler (HeaderText, DangerButton, CostText, vs.)

### Gorsel/Polish (Editor isi, script gerektirmez)
- Post-processing (bloom 0.2, vignette 0.2, filmgrain kapalı) — PostProcessingSetup runtime ✅
- Atmosfer efektleri varsayılan kapalı (dust/fog particles) — AtmosphereEffects inspector'dan açılır ✅
- Bina inşaat animasyonu — BuildingConstructionAnimation otomatik eklenir ✅
- 15+ bina modeli sahne yerlesimi ✅
- [ ] 5+ karakter modeli sahne yerlesimi
- [ ] Sahne dekorasyonu ve atmosfer

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
18. Unity 6000.4'te `ModelImporter.normals`, `tangents`, `importColors`, `generateColliders`, `generateSecondaryUVSet`, `materialLocation`, `normalSmoothingSource` kaldırılmış — `SerializedProperty` ile erişilmeli
19. BuildingPlacer ghost pozisyonu ile yerlestirme koordinati farkli olabilir → `_cachedCoords` / `_cachedWorldPos` ile ghost frame'indeki koordinat kullanilmali
20. MutantAttackManager defeat'te `ApplyBuildingDamage()` → hasarli bina uretimi durur, kullanici Repair ile geri donmeli. SessionLogger'a OnDamaged/OnRepaired eklenmeli
21. `Setup UI Panels` sadece kendi olusturdugu panelleri siler (`DestroyExisting`), diger paneller (ResourceBar, BuildMenu, vs.) dokunulmaz
22. 1x1 binalar ground plane ile z-fighting yapar → model `localPosition.y = 0.015f` offset
23. Inactive GameObject'te `Awake()` çağrılmaz → Singleton Instance null kalır. ToastUI gibi UI panelleri her zaman aktif olmalı
24. `SessionLogger.SubscribeEvents()` SessionLog kapalıyken de çağrılmalı — yoksa event-driven toast'lar çalışmaz
25. `Singleton<T>.Destroy(gameObject)` tüm manager'lar aynı GO üzerinde olduğu için güvenli — duplicate GO'yu tamamen siler. `Destroy(this)` sadece component siler, Instance referansı kopar
26. `UIPrimitiveFactory.AddThemedText()` TMP_Text `richText = true` varsayılan — renkli maliyet metni çalışır
27. `BuildMenuUI.SelectBuilding()` paneli `gameObject.SetActive(false)` ile kapatırsa PanelManager state'i bozulur → `UIManager.Instance.ToggleBuildMenu()` kullanılmalı
28. `GridOverlayRenderer.WorldPos()` hucre merkezi değil köşe vermeli: `GetWorldPosition(x,z) - halfCell`. Footprint highlight `GetWorldPosition` direkt kullanmalı (zaten merkez verir), ekstra offset YASAK
29. `TimeDisplayUI.cs` kaldirildi — zaman gosterimi ResourceBarUI'da `_timeText` SerializeField uzerinden. UI text'leri runtime'da otomatik olusturmak YERINE manuel SerializeField ile baglamak tercih edilir

---

## UI Layout Kurallari

- PausePanel, SaveMenu gibi paneller **default kapali** olmali (SetActive false)
- Buton gruplari icin parent'a `Horizontal Layout Group` veya `Vertical Layout Group` ekle
- Child Force Expand: Width ✅ Height ❌ (genellikle)
- Her butonun `Layout Element`: Min Width 120, Min Height 40
- Buton onClick baglantisi: Obje surukle → dropdown'dan script/metot sec
- `BuildMenuUI` gibi component'lerde SerializeField ile referans baglanir

## Panel Yonetim Sistemi (PanelManager)

- **Tek panel kurali**: Ayni anda sadece 1 ana panel acik olabilir. Yeni panel acilirsa onceki otomatik kapanir
- **PanelManager**: Tum paneller string ID ile kaydedilir, Toggle/OpenOverlay/CloseCurrent ile yonetilir
- **Overlay paneller**: BuildingInfo, BattleReport, Toast, ResourceBar — diger panellerle eszamanli acilabilir
- **Panel gecmisi (stack)**: Panel acildiginda onceki panel history'ye eklenir, CloseCurrent ile geri donulur
- **ESC davranisi**: Panel aciksa → once paneli kapat, panel yoksa → pause menuyu ac
- **ActionBar highlight**: Aktif panelin butonu yesil (`_btnActive`) renkte, digerleri koyu (`_btnNormal`)
- **Pause menü**: Tum panelleri kapatir, Save/Quit alt-panel olarak calisir

---

## Dengeleme Kaynaklari

Tum dengeleme degerleri `Docs/BALANCE.md` dosyasinda:
- Bina maliyetleri ve uretim oranlari (seviye 1-5 ornekleri)
- Asker egitim maliyeti ve guc referansi
- Mutant dalga gucleri (1-10)
- Hero gacha oranlari (Common 50% → Legendary 1%)
- Faction baslangic iliskileri
- Teknoloji arastirma maliyetleri
- Quest odul referansi
- Kaynak baslangic degerleri

---

## Mimari Kurallar (Architecture Rules)

Bu kurallar tekrarlanan hataları ve gereksiz kod tekrarını önlemek için Faz 11 sonrası eklenmiştir.

### Manager Singleton Pattern
- Tüm Manager'lar `Singleton<T>` base class'ından inherit olmalıdır: `public class XxxManager : Singleton<XxxManager>`
- `Awake()` override gerekirse `protected override void Awake() { base.Awake(); ... }` şeklinde yazılmalı
- Kendi `Instance` property'si YAZILMAZ — `Singleton<T>` otomatik sağlar
- `DontDestroyOnLoad` gerekirse override Awake içinde `base.Awake()` sonrası eklenir

### UI Primitif Kodlama
- Yeni UI panel oluştururken `UIPrimitiveFactory` static metodları kullanılmalı:
  - `UIPrimitiveFactory.CreateUIObject()` — UI GameObject oluşturma
  - `UIPrimitiveFactory.AddThemedText()` — Theme font'lı TMP_Text
  - `UIPrimitiveFactory.AddText()` — Fontsuz TMP_Text
  - `UIPrimitiveFactory.AddImage()` — Image component
  - `UIPrimitiveFactory.CreateButton()` — Tam buton (bg + label + onClick)
  - `UIPrimitiveFactory.StretchFull()` — RectTransform stretch
  - `UIPrimitiveFactory.SetAnchors()` — Anchor ayarı
  - `UIPrimitiveFactory.SetupPanelBackground()` — Panel bg + CanvasGroup temizleme
  - `UIPrimitiveFactory.AddStandardVLG()` — Standart VerticalLayoutGroup
  - `UIPrimitiveFactory.AddRowHLG()` — Satır HorizontalLayoutGroup
  - `UIPrimitiveFactory.AddLayoutElement()` — LayoutElement ekleme
- **ASLA** `AddText`, `StretchFull`, `CreateUIObject` gibi metodları panel script'lerde tekrar tanımlama

### Renk Tanımları
- Tüm UI renkleri `UIColors` static class'ında tanımlanır
- `UIColors.Default.PanelBg`, `UIColors.Default.Ok`, `UIColors.Default.Gold` vb.
- Panel script'lerde `static readonly Color` tanımı YAPILMAZ
- Hero rarity renkleri: `UIColors.GetRarityColor(rarity)`
- Map node renkleri: `UIColors.GetNodeColor(type)`

### Domain→UI Ayrımı
- Domain logic (Building, MutantAttackManager, BattleManager vs.) **ASLA** doğrudan `ToastUI.Show()` çağırmaz
- Domain sınıfları event fırlatır: `OnConstructionComplete`, `OnDamaged`, `OnWaveWarning` vb.
- Toast mesajları UI katmanında (SessionLogger, UI paneller) event subscription ile gösterilir
- Sadece UI script'ler (TrainingPanelUI, UIManager vs.) ToastUI çağırabilir

### Resources.LoadAll Kullanımı
- `Resources.LoadAll<T>()` her çağrıda disk I/O yapar → Update/tick içinde YASAK
- Cache pattern: `private T[] _cachedData; private T[] AllData => _cachedData ??= Resources.LoadAll<T>("path");`
- `GameConfig.Instance` zaten singleton SO'dur, her frame erişimi güvenlidir

### Editor Factory Costs Helper
- `CostEntryHelper.Costs(params object[])` merkezi utility kullanılmalı
- BuildingDataFactory, TroopDataFactory, TechNodeFactory'de lokal `Costs()` metodu YAZILMAZ

### Runtime ScriptableObject Oluşturma
- `ScriptableObject.CreateInstance<T>()` runtime'da YASAK (save/load ile uyumsuz)
- Runtime verileri için plain C# class/struct kullanılır
- SO'lar sadece editörde tasarım verisi için kullanılır
- Örnek: `MutantWaveData` (plain class) vs `MutantWave` (SO, editör only)

### Ölü Kod (Dead Code)
- Kullanılmayan script dosyaları projede tutulmaz
- `GameEvent.cs` kaldırıldı — C# `event Action<T>` kullanılıyor
- `PlacementValidator.cs` kaldırıldı — `GridSystem` direkt kullanılıyor

### Hard-coded Magic Numbers
- Birlik gücü çarpanı: `TroopData.BaseAttack` üzerinden hesaplanmalı, `* 10` hard-code YASAK
- Bina tamir/para iadesi oranları `BuildingData` veya `GameConfig` SO'da tanımlanmalı
- `Resources.LoadAll<T>("")` ile boş string path YASAK — spesifik klasör yolu verilmeli

### Runtime Particle System Shader
- Runtime olusturulan her ParticleSystem `Universal Render Pipeline/Particles/Unlit` shader kullanmali
- Built-in `Particles/Standard Unlit` URP'de pembe/magenta gorunur — YASAK
- `ApplyURPParticleMaterial(ps)` helper her `AddComponent<ParticleSystem>()` sonrasi cagrilmali
- Fire/ember gibi parlak efektler icin additive blend (`_Blend=2, DstBlend=One`) kullanilmali

### UI Panel Kapatma Kurallari
- Panel'i `gameObject.SetActive(false)` ile dogrudan kapatmak YASAK — PanelManager state'i bozulur
- Panel kapatma her zaman `UIManager.ToggleXxx()` veya `PanelManager.CloseCurrent()` uzerinden yapilmali
- Ornek: `BuildMenuUI.SelectBuilding()` → `UIManager.Instance.ToggleBuildMenu()` kullanir

### ResourceBarUI
- `_timeText`, `_populationText`, `_levelText` SerializeField — sahnede manuel olusturulur, Inspector'dan baglanir
- `CompactSpacing()` runtime'da HorizontalLayoutGroup spacing=8 yapar
- `TimeDisplayUI.cs` kaldirildi — zaman gosterimi ResourceBarUI'da `_timeText` uzerinden

### GridOverlayRenderer Offset Kurallari
- `WorldPos(x, z)` hucre koselerini dondurur: `GetWorldPosition(x,z) - halfCell`
- Footprint highlight: `GetWorldPosition(cx, cz)` direkt kullanilir (center), ekstra offset YASAK
- `GetWorldPosition` zaten hucre merkezi dondurur: `origin + (x + 0.5) * cellSize`
