# Hollow Ground — AGENTS.md

## Mevcut Versiyon: 0.28.0

## Versiyon Kurallari

- **Tek kaynak:** `VERSION` dosyasi (kok dizinde)
- **SemVer:** `MAJOR.MINOR.PATCH` — MAJOR: oynanabilir, MINOR: yeni faz/ozellik, PATCH: bug fix
- **Git tag**: Her merge'de `v0.X.Y` tag olusturulur
- **CHANGELOG.md**: Her versiyonda ne degisti yazilir
- **Commit/push**: Kullanici acikca istemedikce yapilmaz

## Calisma Akisi (GitHub Project Driven)

Tum isler **GitHub Project** uzerinden yonetilir. Issue yoksa kod yazilmaz.

- **URL:** https://github.com/users/siyakin/projects/2
- **Status:** Todo → In progress → Done
- **Workflow:** Issue ac → `feature/xxx` branch → kod yaz → PR → review → main merge → tag + CHANGELOG
- **Commit mesajlari:** `#issue-no` ile referans verilmeli (orn: `fix: hero ID mismatch on load #8`)
- **gh CLI:** Issue ve PR islemleri icin `gh` komutu kullanilir

## Proje Ozeti

**Hollow Ground** — Last War ilhamli, nukleer savas sonrasi strateji oyunu.
Tek kisi PvE: Sehir kurma + ordu yonetimi + hero sistemi + dunya kesfi.
**Motor:** Unity 6 + URP + 3D Low Poly + Yeni Input System + TextMeshPro

---

## Kritik Kurallar

### Dil ve Karakter
- Tum script/SO/Display Name'ler **Ingilizce** olmali
- **Turkce karakter YASAK** dosya adlarinda
- Kod yorumlari eklenmez, kullanici ozelle istemedikce

### Namespace Cakismalari
- `UnityEngine.Resources.LoadAll<T>()` tam nitelik kullan
- `UnityEngine.Camera.main` tam nitelik kullan

### Unity API
- **Eski Input System YASAK**: `Mouse.current.leftButton.wasPressedThisFrame`
- **FindObjectOfType/FindFirstObjectByType YASAK**: `FindAnyObjectByType<T>()` kullan
- **FindObjectsByType(FindObjectsSortMode) YASAK**: `FindObjectsByType<T>()` veya `FindObjectsByType<T>(FindObjectsInactive)` kullan
- **Dictionary serialize olmaz**: `List<Entry>` pattern'i kullan
- **Push oncesi derleme kontrolu ZORUNLU**
- **Feature branch'leri sik merge et**: main'den uzak branch'ler dosya butunlugunu bozar

### Pattern'ler
- Manager'lar **Singleton<T>** pattern kullanir (Instance property)
- Sistemler birbirini **event** ile haber verir, direkt cagri yok
- Veriler **ScriptableObject** ile tanimlanir
- UI panelleri `UIManager.ToggleXxx()` ile acilir/kapanir
- Domain logic **ASLA** `ToastUI.Show()` cagirmaz — event firlatir, UI katmani dinler

---

## Proje Yapisi

```
Assets/_Project/
├── Scripts/
│   ├── Core/        GameManager, TimeManager, Singleton, GameInitializer, SaveData, SaveSystem,
│   │                AudioManager, PostProcessingSetup, AtmosphereEffects, GameConfig, SessionLogger,
│   │                WeatherSystem, CostEntryHelper
│   ├── Camera/      StrategyCamera, ScreenShake, MinimapCamera
│   ├── Grid/        GridSystem, GridCell, GridVisualizer, GridOverlayRenderer, MapRenderer,
│   │                MapTemplate, TerrainTile, TerrainType, WaterSurface
│   ├── Buildings/   BuildingType, BuildingData, Building, BuildingManager, BuildingPlacer,
│   │                BuildingSelector, BuildingDatabase, BuildingConstructionAnimation,
│   │                BuildingHighlight, DamageEffects, GardenManager
│   ├── Roads/       RoadManager, RoadVisualizer
│   ├── Resources/   ResourceType, ResourceManager
│   ├── Army/        TroopType, TroopData, ArmyManager
│   ├── Combat/      BattleCalculator, BattleTarget, BattleManager, MutantWave, MutantAttackManager
│   ├── Heroes/      HeroEnums, HeroData, Hero, HeroManager
│   ├── World/       MapNodeData, WorldMap, ExpeditionSystem
│   ├── Tech/        TechNode, ResearchManager
│   ├── NPCs/        FactionData, TradeSystem, SettlerWalker, SettlerManager, SettlerJobManager,
│   │                WalkerBase, WalkerManager
│   ├── Domain/      Walkers/WalkerStateMachine, Combat/BattleCalc, Production/ProductionCalc,
│   │                Pathfinding/PathfinderService
│   ├── Quests/      QuestEnums, QuestData, QuestInstance, QuestManager
│   ├── UI/          UIManager, PanelManager, ResourceBarUI, BuildMenuUI, BuildingInfoUI, ToastUI,
│   │                TrainingPanelUI, ArmyPanelUI, BattleReportUI, HeroPanelUI, WorldMapUI,
│   │                TechTreeUI, FactionTradeUI, QuestLogUI, SaveMenuUI, DebugHUD,
│   │                UIThemeSO, UIThemeTag, MinimapUI
│   └── Editor/      GridSystemEditor, BuildingDataFactory, TroopDataFactory, HeroDataFactory,
│                     QuestDataFactory, FactionDataFactory, TechNodeFactory, GhostMaterialCreator,
│                     UIThemeApplier, SceneSetupEditor, GameConfigCreator,
│                     PostProcessingProfileFactory, GroundSetupEditor
├── ScriptableObjects/
│   ├── Buildings/   17 aktif bina SO (15 + Garden + GardenLarge)
│   ├── Targets/     5 BattleTarget SO
│   ├── Troops/      5 birlik SO
│   ├── Heroes/      5 hero SO
│   ├── TechNodes/   10 tech SO
│   ├── Factions/    3 faction SO
│   ├── Quests/      5 quest SO (10 daha eklenmeli)
│   └── Maps/        MapTemplate, DefaultMap
├── Models/          CityPack, PostApocolypsePack, SurvivalPack, NaturePack, Buildings (105 FBX)
├── Prefabs/         ToastItem.prefab, UI/
├── Settings/        StrategyControls.inputactions
├── Shaders/         Water.shader (URP custom)
└── Docs/            GDD.md, ROADMAP.md, BALANCE.md
```

---

## Sahne Obje Yapisi

### Sahne Root Nesneleri (DEGISTIRILEMEZ)

| # | Nesne Adi | Aciklama |
|---|-----------|----------|
| 1 | `Directional Light` | URP sun direction |
| 2 | `GameObject` | GameManager GO — tum manager'lar burada |
| 3 | `CameraRig` | Altinda Main Camera + MinimapCamera |
| 4 | `GameCanvas` | Tum UI bu Canvas altinda |
| 5 | `EventSystem` | Unity EventSystem |
| 6 | `UIManager` | UIManager component + panel referanslari |
| 7 | `GameInitializer` | StartGame() tetikler |
| 8 | `Ground` | Layer: Ground (8) |

**Yeni root nesne OLUSTURULMAZ / SILINMEZ / ISIM DEGISTIRILMEZ.**

### GameManager Objeleri (`GameObject` root uzerinde)

GameManager, TimeManager, ResourceManager, GridSystem, GridVisualizer, BuildingPlacer,
BuildingSelector, BuildingManager, ArmyManager, BattleManager, HeroManager, WorldMap,
ExpeditionSystem, QuestManager, MutantAttackManager, ResearchManager, TradeSystem,
SaveSystem, GameInitializer, WeatherSystem, RoadManager, SettlerManager,
SettlerJobManager, WalkerManager, MapRenderer, GardenManager

### GameCanvas Alt Yapisi
ResourceBar, ActionBar, BuildMenu, TrainingPanel, BattleReportPanel, HeroPanel,
WorldMapPanel, QuestLogPanel, TechTreePanel, FactionTradePanel, SaveMenuPanel,
SettlerPanel, SettlerInfoPanel, PausePanel, DebugPanel, MinimapPanel

---

## Tamamlanan Fazlar (1-19)

| Faz | Aciklama |
|-----|----------|
| 1 | Temel altyapi: Camera, Grid, Resources, GameManager, Input |
| 2 | Base Building: 10 bina SO, ghost preview, grid snap |
| 3 | UI: 15+ panel, UIManager, tum toggle metotlari |
| 4 | Askeri: 5 birlik, egitim kuyrugu, moral sistemi |
| 5 | Savas: BattleCalculator, BattleTarget, sefer sistemi |
| 6 | Hero: 5 rol, gacha summon, ekipman, XP |
| 7 | Dunya Haritasi: 10x10 grid, A*, fog of war, sefer |
| 8 | Ileri: Tech tree, Faction/Ticaret, Quest, Mutant saldiri |
| 9 | Save/Load + Audio: JSON save, auto-save, SFX pool |
| 10 | Content: Starting buildings, 3 faction, 10 tech, 15 quest, BALANCE.md |
| 11 | Playtest & Bugfix: 13/13 test, GameConfig, SessionLogger |
| 12 | Bina Model: 105 FBX, state-based model swap, hasar/tamir |
| 13 | Refactoring: Singleton<T>, UIPrimitiveFactory, UIColors, dead code silindi |
| 14 | Visual: Grid overlay, weather, highlight, damage efektleri, pause menu |
| 15 | Settler Walker: NPC yolu yurume, nufus bazli spawn, animasyon |
| 16 | Settler Job: 12 rol, auto-assign, isci bazli uretim, SettlerPanelUI |
| 17 | Terrain + Domain: MapTemplate, MapRenderer, water shader, WalkerBase, WalkerStateMachine |
| 17b | Toast Overhaul: Stacked multi-toast, slide animation, load toast suppression |
| 18 | Garden & Merge: 4-garden merge, NeedsRoads flag, FBX updates |
| 19 | DebugHUD 3-tab + quest triggers + TrainingPanel fix + playtest (v0.27.0) |

**Playtest:** Faz 11 (13/13) ve Faz 19 (15/15) tam gecti.

---

## Yapilacaklar

### Eksikler
- [ ] ScriptableObjects/Quests/ — 10 ek quest SO (QuestDataFactory ile)
- [ ] Garden: L03/L05/L10/Damaged/Destroyed FBX modelleri
- [ ] Garden: Save/Load merge state
- [ ] Garden: SettlerJobManager worker role (Farmer for Garden)
- [ ] Garden: BuildingSpecs dokumani
- [ ] NPC Visual Feedback: toz particle, ayak sesi SFX, hasat animasyonu
- [ ] SettlerPanel Enrichment: ozet satiri, bina bazli dagilim, rol pasta grafik
- [ ] Quick Tooltips: bina hover, settler tikla tooltip
- [ ] 5+ karakter modeli sahne yerlesimi
- [ ] Sahne dekorasyonu ve atmosfer

### Acik Issue'lar
- **#34** Training queue not restored on load
- **#35** Building ProductionTimer save/load eksik
- **#36** World Map & Expedition system rework
- **#37** RoadManager orphan road cleanup calismiyor
- **#38** Manual road removal aktif/bagli yollari da silebiliyor
- **#39** Yol olan hucrelere bina yerlestirilebiliyor (partially fixed in v0.25.0)
- **#40** WorldMap.GenerateDefaultMap runtime SO

### Gorsel/Polish (Editor isi)
- [x] Post-processing, atmosfer, bina insaat animasyonu, 15+ bina modeli sahne yerlesimi
- [ ] Karakter modelleri, sahne dekorasyonu

---

## Kesfedilen Tuzaklar (Discoveries)

En sik tekrar eden sorunlar — yeni kod yazarken dikkat:

1. `Dictionary<K,V>` serialize olmaz → `List<Entry>`
2. GameCanvas'a Layout Group EKLENMEZ — boyut/anchor bozar
3. TMP_Text ve Image ayni objede OLAMAZ
4. Iki Audio Listener olmamali — tek MainCamera tag
5. Ground'u runtime'da olusturma → editor'de kalici (`Setup Ground & Camera`)
6. Camera.main null donebilir → field ile cache et
7. `Singleton<T>.Destroy(gameObject)` ayni GO'daki tum manager'lar icin guvenli
8. `BuildMenuUI.SelectBuilding()` → `UIManager.ToggleBuildMenu()` kullanmali, `SetActive(false)` YASAK
9. Blender `-Z forward` export → kapi yonu: rotation 0=-Z, 1=-X, 2=+Z, 3=+X
10. FBX Instantiate sonrasi Animator/Avatar kaybolur → kaynak asset'ten al
11. `DestroyImmediate()` kullanilmali (Animator FBX cleanup)
12. `Animator.Rebind()` setup sonrasi cagrilmali
13. OnMouseDown + BuildingSelector ayni frame race condition → tek merkezde yonet
14. Domain katmaninda ToastUI.Show() YASAK → event firlat
15. 2x2 bina merkezi: `cellSize * 0.5f` offset, `cellSize * 1.0f` degil
16. `Resources.LoadAll<T>()` disk I/O yapar → cache pattern kullan
17. `ScriptableObject.CreateInstance<T>()` runtime'da YASAK (save/load uyumsuz)
18. Inactive GameObject'te `Awake()` cagrilmaz — UI panelleri her zaman aktif olmali
19. `SessionLogger.SubscribeEvents()` SessionLog kapaliyken de cagrilmali
20. Roboto font SADECE Latin — emoji/Unicode sembol YASAK → ASCII kullan
21. `OnConstructionComplete` callback icinde `DestroyImmediate` → MissingReferenceException, 1 frame geciktir
22. `HasEnoughResources()` null ise optimistik `true` don (timing fix)
23. `git checkout HEAD -- scene.unity` SerializeField baglantilarini kalici kaybeder
24. `LoadSettlers()` SphereCollider eklemeli — CreatePoolSettler ile ayni setup

---

## Mimari Kurallar

### Manager Singleton Pattern
- `public class XxxManager : Singleton<XxxManager>`
- `Awake()` override: `protected override void Awake() { base.Awake(); ... }`
- Kendi Instance property'si YAZILMAZ

### UI Primitif Kodlama
- `UIPrimitiveFactory` static metodlari kullan: CreateUIObject, AddThemedText, AddText, AddImage,
  CreateButton, StretchFull, SetAnchors, SetupPanelBackground, AddStandardVLG, AddRowHLG, AddLayoutElement
- Panel script'lerde tekrar tanimlama YASAK

### Renk Tanımları
- Tum UI renkleri `UIColors` static class'inda — inline `static readonly Color` YASILMAMALIDIR
- `UIColors.Default.PanelBg`, `.Ok`, `.Gold`, `.Danger`, `.Warn` vb.

### UI Theme
- Her runtime panel `BuildUI()` sonunda `UIPrimitiveFactory.ApplyThemeStyles(transform)` cagirmali
- Text tag'lari: HeaderText, BodyText, LabelText, CostText, WarningText, DangerText
- Buton tag'lari: ConfirmButton, DangerButton, ActionBarButton, BuildingCardButton, TabButton

### UI Panel Kurallari
- `gameObject.SetActive(false)` ile direkt kapatma YASAK → `UIManager.ToggleXxx()` kullan
- ESC oncelik: About > SaveMenu > Pause > Panel > Pause toggle
- Panel default kapali olmali (SetActive false)
- Layout Group: Child Force Expand Width=ON Height=OFF

### Save/Load Kurallari
- Load'da `ClearForLoad()` kullan (iade yok, event yok)
- `ApplySaveData` sirasi: binalar temizle → kaynaklar set et
- Load'da `DestroyImmediate` kullan
- Hero: `AddHeroWithId(data, id)` ile save ID koru
- Kaynak: `Set()` ile tam deger, `Add()` degil
- BuildingData eslestirme: asset name → DisplayName → BuildingNameAliases
- TechNode: `ResetAllState()` her baslangicta

### Input Block Sistemi
- `UIManager.IsInputBlocked` — panel aciksa true
- StrategyCamera, BuildingPlacer, BuildingSelector Update'te kontrol eder
- Load sonrasi `ResumeAfterLoad()` ile tam resume

### Runtime Particle Shader
- Her runtime ParticleSystem: `Universal Render Pipeline/Particles/Unlit`
- Built-in `Particles/Standard Unlit` URP'de magenta — YASAK
- `ApplyURPParticleMaterial(ps)` her AddComponent sonrasi

### Grid Overlay Offset
- `WorldPos(x, z)` = `GetWorldPosition(x,z) - halfCell` (kose)
- Footprint highlight: `GetWorldPosition(cx, cz)` direkt (merkez), ekstra offset YASAK

### Resources.LoadAll Kullanimi
- Update/tick icinde YASAK → cache pattern: `_cachedData ??= Resources.LoadAll<T>("path")`

### Editor Menuleri
- `HollowGround > Setup UI Panels` / `Setup Save Menu` / `Setup Minimap` / `Setup Ground & Camera`
- `HollowGround > FBX/Configure All Building FBX Imports` / `Models/Bind All Building Models`
- `HollowGround > Settlers/...` (Avatar fix, clip bake, test)

---

## Bina Modelleri (Blender → Unity)

- Rehber: `Docs/BLENDER_MODELING_GUIDE.md` — olculer, renk paleti
- Prompt serisi: `Docs/BLENDER_PROMPTS.md` — kopyala-yapistir prompt'lar
- Grid cell: 2m. 1x1 max 1.9x1.9m, 2x2 max 3.9x3.9m
- Her bina: 7 model (L01, L03, L05, L10, Construct, Damaged, Destroyed)
- 15 bina x 7 model = 105 FBX tamamlandi
- FBX export: -Z forward, Y up, Apply Transform ON
- Vertex color: R=rust, G=moss, B=dirt
- FBX import: `Assets/_Project/Models/Buildings/{BuildingName}/`
- Level threshold: L01 (lv1-2), L03 (lv3-4), L05 (lv5-9), L10 (lv10)
- Model offset: `localPosition.y = 0.015f` (z-fighting fix)

---

## Dengeleme

Tum degerler `Docs/BALANCE.md` dosyasinda: bina maliyetleri, uretim, asker, hero gacha, faction, tech, quest odul, baslangic kaynaklari.
