# Changelog

All notable changes to Hollow Ground are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Versioning follows [SemVer](https://semver.org/): MAJOR.MINOR.PATCH

## [0.20.0] - 2026-05-01

### Added
- SettlerRole: 12-role enum (None, Builder, Farmer, Miner, Woodcutter, WaterCarrier, Engineer, Medic, Guard, Researcher, Trader, Hauler) + display names
- SettlerJobManager: auto-assignment of idle settlers to buildings by priority, worker release on building destroy, building→workers mapping
- SettlerWalker: work cycle (Idle→Walking→Working→Walking→Resting→repeat), Role/AssignedBuilding properties
- BuildingData: WorkerSlot class, RequiredWorkers list, WorkerProductionBonus (0-1 dependency), GetTotalRequiredWorkers()
- Building: AssignedWorkerCount property, GetWorkerProductionModifier() formula: 1 - bonus * (1 - fillRatio)
- SettlerPanelUI: two-column population panel (building workers + active workers), event-driven refresh
- SettlerInfoUI: overlay panel on settler click, shows role/building/task/status
- BuildingSelector: combined building+settler raycast selection, SphereCollider on settlers (r=0.8)
- SettlerJobDataFactory: editor tools (Apply Default Worker Requirements, Show Report)
- SettlerWalkerSave: Role + AssignedBuildingGridX/Z fields (backward compatible)
- GameConfig: SettlerWorkDuration=8f, SettlerRestDuration=5f
- UIManager: ToggleSettlerPanel(), Settler/BtnSettler panel registration
- DebugHUD: F12 toggle, settler count display

### Fixed
- 6 BuildingData SOs with wrong m_Name (Barracks, Generator, Shelter, Storage, WaterWell, WoodFactory)
- Hospital SO Type: 0 (CommandCenter) → Type: 11 (Hospital)
- Deleted 9 legacy/yedek BuildingData SOs + 1 duplicate BuildingData.asset from root
- Removed Man/Woman character models — incompatible skeleton (HumanArmature vs CharacterArmature)
- CityPack character count reduced to 3: Worker, Adventurer, Suit (Business Man)
- SettlerWalker.OnMouseDown race condition: BuildingSelector.Update same-frame HideSettlerInfo override → removed OnMouseDown, unified selection in BuildingSelector.TrySelect()
- LoadSettlers missing SphereCollider → settlers unclickable after save/load

### Changed
- AGENTS.md: Faz 15 completed, Faz 16 documented, version 0.20.0
- VERSION: 0.19.0 → 0.20.0

## [0.19.0] - 2026-04-30

### Added
- SettlerWalker: grid-based NPC movement, state machine (Idle/Walk/Wait/ReturnHome)
- SettlerManager: population-based spawn, road pathfinding integration
- SettlerAnimationSetup: editor tools for Avatar generation, clip baking, AnimatorController
- 4 CityPack characters with animation: Worker, Adventurer, Suit, Casual_2
- 24 baked animation clips from Worker.fbx (Walk, Idle, Run, Death, etc.)
- SettlerController: Speed parameter, Idle/Walk transitions (0.15s blend)
- FBX import fix: avatarSetup=1 (CreateAnAvatarFromModel) for Generic rig characters
- Runtime URP material conversion (Standard → URP/Lit) for settler models
- DebugHUD: settler count, active count, population display
- SessionLogger: OnSettlerSpawned/OnSettlerRemoved event logging
- GameConfig: DisableSettlers, SettlersPerPopulation, MaxSettlers, SettlerMoveSpeed, SettlerIdleTime
- Save/Load: SettlerWalkerSave with position/state persistence

### Changed
- AGENTS.md: Faz 15 updated — animation system documented, 5 new discoveries added
- Building SO meta changes (Avatar fix applied to PostApoc characters)

### Editor Tools
- HollowGround > Settlers > Fix: Enable Avatar on All Characters
- HollowGround > Settlers > Fix: Rebuild Clips + Controller
- HollowGround > Settlers > Test: Spawn Animated Settler in Scene
- HollowGround > Settlers > Test: Verify Model Hierarchy

## [0.18.0] - 2026-04-29

### Added
- Organic road system — Settlers/Rome tarzı binalar arası otomatik yol oluşumu
- RoadManager singleton — BFS pathfinding (0-1 deque), bina inşaatı bitince yol oluşturma
- RoadVisualizer — connected tile sistemi (mask-based), Bezier köşeler, ellipsoid dead-end cap'ler
- Procedural dirt texture — 3-oktav Perlin noise, 64x64, world-position UV ile dikişsiz tiling
- Building rotation persistence — save/load ile bina yönü korunuyor
- Building.GetRotatedFootprint() — rotation'a göre (SizeX,SizeZ) veya (SizeZ,SizeX) döndürür
- Building.GetDoorCell() — kapı yönü hesaplama (rotation 0=-Z, 1=-X, 2=+Z, 3=+X)
- Auto-rotation — bina yerleştirirken yola doğru otomatik dönme (R ile override)
- Orphan road cleanup — bina yıkılınca 30s sonra bağlantısız yollar fade-out ile kaybolur (BFS connectivity)
- Manual road removal — sağ-tık ile yol silme (2s fade-out animasyonu)
- RoadManager.HasRoadAt() — belirli grid koordinatında yol var mı kontrolü
- SaveData.RoadCells — yol verisi kaydetme/yükleme (IntIntEntry listesi)
- BuildingSave.Rotation — bina rotasyonu kaydetme/yükleme
- SaveSystem: rotated footprint ile bina pozisyon/occupy hesaplama

### Changed
- Building.cs: Initialize() rotation parametresi alıyor, FreeGridCells/RemoveBuilding rotated footprint kullanıyor
- BuildingPlacer: _rotation Initialize'a geçiriliyor, _manualRotate flag ile R override
- SaveSystem.ApplyBuildings: rotation-aware transform, offset, OccupyCells
- GameInitializer.ResetBuildings: RoadManager.ClearAllRoads() çağrısı
- AGENTS.md: Roads/ klasörü, RoadManager GameManager GO'da, road system kuralları, 3 yeni discovery

### Known Issues
- Orphan road cleanup (30s) çalışmıyor — BFS connectivity check debug edilmeli
- Sağ-tık aktif/bağlı yolları da silebiliyor — sadece orphan yollar silinmeli
- Yol olan hücrelere bina yerleştirilebiliyor — BuildingPlacer'da road cell kontrolü gerekli

## [0.17.0] - 2026-04-29

### Added
- BuildMenuUI: ThemedButton entegrasyonu — disabled butonlar merkezi temadan renk aliyor
- HeroManager.AddHeroWithId() — save'den hero yüklenirken ID uyumu
- ResourceManager.Set() — kaynaga tam deger atama (additive degil)
- MutantAttackManager.RestoreState() — wave/timer save'den restore
- Building.ClearForLoad() — load sirasinda bina temizleme (kaynak iadesi yok, event yok)
- SaveMenuUI: cift tikla load, DestroyImmediate ile liste guncelleme, LayoutRebuilder
- SaveMenuUI: ScrollList + scrollbar editor setup (HollowGround > Setup Save Menu)
- UIManager.IsInputBlocked — merkezi input blok sistemi
- Input block: StrategyCamera, BuildingPlacer, BuildingSelector panel açıkken bloklanir
- SaveMenuUI.OnEnable/OnDisable — panel acildiginda zaman durur, kapandiginda devam eder
- UIManager.ResumeAfterLoad() — load sonrasi tam resume (pause + save menu + time)
- SaveSystem: BuildingNameAliases — eski Turkce SO isimleri otomatik eslestirme
- SaveSystem: FindBuildingData — DisplayName ile de arama destegi
- GameInitializer.ResetAllState() — oyun baslangicinda tam state sifirlama
- ArmyManager.ResetAll(), HeroManager.ResetAll() — manager sifirlama
- .kilo/command/new-issue.md, start-work.md, finish-work.md — GitHub Project workflow komutlari
- AGENTS.md: Calisma Akisi (GitHub Project Driven) bölümü eklendi
- GitHub Issues: 13 backlog item olusturuldu (P0/P1/P2 priority, XS-XL size)

### Fixed
- BuildMenuUI: kaynak yetmediginde buton disabled + NameText soluk renk
- SaveSystem.ApplyHeroes: hero ID uyumsuzlugu — yeni Guid yerine save ID kullaniliyor
- SaveSystem.ApplyResources: sadece eklemek yerine tam deger ataniyor
- SaveSystem.ApplyMutantAttack: wave/timer sifirlanmiyor, restore ediliyor
- SaveSystem.ApplySaveData sirasi: binalar once temizleniyor, sonra kaynaklar set ediliyor
- SaveSystem.ApplyBuildings: DestroyImmediate + offset hesabi (multi-cell binalar)
- GameManager, AudioManager: DontDestroyOnLoad kaldirildi (single-scene game)
- GameInitializer: CurrentState = GameState.Menu ile state sifirlama
- SceneSetupEditor: SaveMenuPanel trim() ile bulunuyor (trailing space sorunu)

### Changed
- SaveMenuUI: runtime BuildUI yerine scene-based SerializeField baglantilari
- SaveMenuUI: _built flag kaldirildi, her OnEnable'da RefreshList calisir
- BuildMenuFixer: ThemedButton + BuildingCardButton style otomatik ekleniyor

## [0.16.3] - 2026-04-28

### Added
- AboutPanelUI: dinamik statlar (SO sayisi, bina sayisi, hero sayisi, toplam oyun suresi)
- AboutPanelUI: versiyon VERSION dosyasindan dinamik okunur

### Fixed
- BuildMenuUI: kaynak yetmediginde bina butonlari disabled
- AboutPanelUI: tema fontu ve renkleri duzeltildi (AddThemedText, 16px)

## [0.16.2] - 2026-04-28

### Fixed
- BuildMenuUI: kaynak yetmediginde bina butonlari disabled gorunur (interactable=false)
- AboutPanelUI: versiyon numarasi beyaz renkte gosterilir

## [0.16.1] - 2026-04-28

### Fixed
- AboutPanelUI: versiyon text rengi beyaz yapildi

## [0.16.0] - 2026-04-28

### Added
- AboutPanelUI: F1 ile acilan hakkinda paneli (versiyon, credits, stats)
- UIManager: F1 ToggleAbout(), ESC oncelik sirasi (About > SaveMenu > Pause > Panel)
- Versiyon VERSION dosyasindan dinamik okunur

### Changed
- AGENTS.md: TMP Unicode tuzaklari, runtime panel toggle kurallari, versiyon gostergisi kurali eklendi

## [0.15.0] - 2026-04-28

### Added
- Merkezi UITheme sistemi: UIThemeManager (Singleton), ThemedButton, ThemedText
- Event-driven tema degisimi: SetTheme() / SetFont() ile runtime guncelleme
- Auto-contrast buton text: UIColors.ContrastText() / ContrastTextForButton()
- ActionBar selected state: panel acikken yesil highlight + hover
- UIPrimitiveFactory.CreateThemedButton() — otomatik ThemedButton component
- UIPrimitiveFactory.AddThemedText(styleType) — otomatik ThemedText component
- TechTreeUI kategori header'lari: dynamic contrast text (28px italic)
- ToastUI: theme font + UIColors entegrasyonu
- WorldMapUI: 5 buton CreateThemedButton'a gecirildi
- TechTreeUI: research butonu CreateThemedButton'a gecirildi
- TrainingPanelUI, HeroPanelUI, QuestLogUI, FactionTradeUI: CreateThemedButton gecisi
- SaveMenuUI: tamamen yeni tematik buton sistemi
- AGENTS.md: branch farkindaligi, merge disiplini, runtime UITheme kurallari

### Changed
- UIPrimitiveFactory: eski ApplyThemeStyles/LoadTheme kaldırildi, yeni CreateThemedButton eklendi
- ThemedButton: isSelected state + SetSelected() metodu eklendi
- UIManager: ActionBar hardcoded renkler kaldirildi, ThemedButton kullaniliyor
- SaveMenuUI: UIThemeTag kaldirildi, CreateThemedButton kullaniliyor

### Removed
- UIThemeTag kullanimi tum panellerden kaldirildi (UIThemeTag.cs backward compat icin duruyor)
- UIManager._btnNormal / _btnActive hardcoded renkler kaldirildi

---

## [0.14.0] - 2026-04-28

### Added
- GridOverlayRenderer — yerlestirme modunda grid gorunur, snake/zigzag pattern
- WeatherSystem — 5 hava durumu, auto-cycle, per-weather post-processing
- BuildingHighlight — secili bina outline mesh, pulsing alpha
- DamageEffects — ates/duman particle, explosion burst
- ScreenShake — Perlin noise shake, LateUpdate
- AtmosphereEffects — dust/fog/embers particle efektleri
- Pause menu (ESC) — Resume, Save/Load, Quit
- Singleton<T> base class — tum manager'lar icin
- UIPrimitiveFactory — merkezi UI olusturma utility
- UIColors — merkezi renk yonetimi
- CostEntryHelper.Costs() — merkezi maliyet utility

### Changed
- BuildMenuUI butonlari: kaynak kontrolu + toast mesajlari
- SessionLogger.SubscribeEvents() — SessionLog kapaliyken de calisir
- ArmyManager.CalculateArmyPower() — TroopData.BaseAttack kullanir

### Removed
- GameEvent.cs — C# event Action<T> kullaniliyor
- PlacementValidator.cs — GridSystem direkt kullaniliyor

---

## [0.13.0] - 2026-04-28

### Added
- Singleton<T> base class tum manager'lara uygulandi
- UIPrimitiveFactory ile merkezi UI primitifleri
- UIColors ile merkezi renk tanimlari
- CostEntryHelper ile merkezi maliyet olusturma
- 4 UI panel UIPrimitiveFactory + UIColors'a tasindi

### Changed
- BuildingDataFactory, TroopDataFactory, TechNodeFactory → CostEntryHelper.Costs()
- FactionTradeUI, SaveMenuUI, TechTreeUI, WorldMapUI → merkezi utility
- ToastUI yeniden yazildi
- Magic numbers → GameConfig SO'ya tasindi

### Removed
- Oluk kod: GameEvent.cs, PlacementValidator.cs

---

## [0.12.0] - 2026-04-28

### Added
- 105 FBX bina modelleri (15 bina x 7 state)
- BuildingData.BuildingModels struct
- State-based model swap: Building.cs UpdateModel()
- Hasar/tamir sistemi: ApplyDamage(), Repair()
- BuildingInfoUI: SmartPosition, Repair butonu, state renk kodlamasi
- Editor: FBX import configurator, model binder, binding report

---

## [0.11.0] - 2026-04-28

### Added
- Playtest: 13/13 test gecti
- GameConfig SO: DevMode, hiz carpanlari, mutant kontrolu
- SessionLogger: tum oyun eventlerini dosyaya yazar
- UITheme font runtime panellere uygulanir
- F5 QuickSave, F9 QuickLoad
- 3 FactionData SO (Scavenger Guild, Iron Legion, Green Haven)

---

## [0.10.0] - 2026-04-28

### Added
- BaseStarter ile baslangic sehir kurulumu
- 3 FactionData SO
- 10 TechNode SO
- 15 Quest SO
- BALANCE.md dengeleme referans tablosu

---

## [0.1.0] - [0.9.0]

### Summary
- Faz 1-9: Temel altyapi, bina sistemi, UI, askeri, savas, hero, dunya haritasi,
  ileri sistemler (tech/faction/quest/mutant), save/load/audio
- Her faz tamamen tamamlandi ve playtest edildi
