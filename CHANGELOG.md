# Changelog

All notable changes to Hollow Ground are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Versioning follows [SemVer](https://semver.org/): MAJOR.MINOR.PATCH

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
