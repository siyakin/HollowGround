# Hollow Ground — ROADMAP

## Vizyon
Last War ilhamlı, nükleer savaş sonrası strateji oyunu.
Şehir kurma + ordu yönetimi + hero sistemi + dünya keşfi.

---

## Faz 1 — Temel Altyapı ✅

- [x] Unity 6 + URP + Input System
- [x] StrategyCamera — WASD, orta-tık pan, sağ-tık döndürme, scroll zoom, edge pan
- [x] GridSystem — 2D grid, cell state, snap-to-grid, debug görselleştirme
- [x] ResourceManager — 6 kaynak, kapasite, event sistemi
- [x] GameManager + TimeManager — oyun durumu, hız kontrolü
- [x] GameEvent SO — sistemler arası haberleşme
- [x] GameInitializer — oyun başlatma, quest yükleme, mutant döngüsü

---

## Faz 2 — Base Building ✅

- [x] BuildingType (15 tip) + BuildingCategory (4 kategori)
- [x] BuildingData SO — CostEntry listesi, üretim, seviye verisi
- [x] Building.cs — inşaat, üretim tick, yükseltme, yıkma
- [x] BuildingManager — bina takibi, nüfus/depo hesabı, CC seviyesi
- [x] BuildingPlacer — ghost preview, grid snap, yeşil/kırmızı validasyon
- [x] BuildingSelector — sol tık seçim
- [x] 9 bina SO hazır (CC, Çiftlik, Odun, Maden, Su, Kışla, Barınak, Depo, Jeneratör)

---

## Faz 3 — UI ✅

- [x] UIManager — panel yönetimi, toggle metotları (15 panel)
- [x] ResourceBarUI — kaynak değişim event ile güncelleme
- [x] BuildMenuUI — kaynak kontrolü, buton interaktifliği
- [x] BuildingInfoUI — seviye, üretim, upgrade, yık butonu
- [x] ToastUI — bildirim kuyruğu
- [x] DebugHUD — canlı debug bilgileri
- [x] TrainingPanelUI — asker eğitim paneli
- [x] ArmyPanelUI — ordu kompozisyonu
- [x] BattleReportUI — savaş sonuç ekranı
- [x] HeroPanelUI — hero listesi + detay + summon
- [x] WorldMapUI — grid görselleştirme, node seçimi, sefer paneli
- [x] TechTreeUI — araştırma ağacı paneli
- [x] FactionTradeUI — faction ticaret paneli
- [x] QuestLogUI — görev paneli (aktif/mevcut/tamamlanmış sekmeler)
- [x] Sahne kurulumları (BuildingInfo, ToastUI, TechTree, FactionTrade, QuestLog bağlantıları)

---

## Faz 4 — Askeri Sistem ✅

- [x] TroopType (5 tip) + TroopRole (5 rol)
- [x] TroopData SO — statlar, matchup, eğitim maliyeti
- [x] ArmyManager — ordu havuzu, eğitim kuyruğu, moral, güç hesabı
- [x] TrainingPanelUI — kışla eğitim arayüzü
- [x] ArmyPanelUI — ordu kompozisyonu gösterimi
- [x] Moral sistemi (yiyecek/su etkisi)
- [x] 5 birlik SO fabrikası (Piyade, Nişancı, Ağır, Gözcü, Mühendis)

---

## Faz 5 — Savaş Sistemi ✅

- [x] BattleCalculator — hasar/kayıp hesaplama, matchup çarpanları, %10 varyans
- [x] BattleTarget SO — hedef tanımı, savunma birlikleri, ganimet, mesafe
- [x] BattleManager — sefer gönderme, süreç, otomatik savaş
- [x] BattleReportUI — zafer/yenilgi, kayıplar, ganimet, hayatta kalanlar
- [x] Sefer takip — ilerleme çubuğu, kalan süre
- [x] Ganimet sistemi — kazanılan kaynaklar otomatik eklenir

---

## Faz 6 — Hero Sistemi ✅

- [x] HeroEnums — 5 rarity, 5 rol, 3 ekipman slotu
- [x] HeroData SO — statlar, yetenekler, summon maliyeti
- [x] Hero.cs — seviye, XP, ekipman, yaralı durumu
- [x] HeroManager — hero havuzu, summon (gacha), XP, ordu bonusu
- [x] HeroPanelUI — hero listesi + detay paneli + summon butonu
- [x] HeroDataFactory — 5 hero SO menüsü (Commander, Warrior, Ranger, Engineer, Scout)
- [x] Ekipman slotları (silah, zırh, aksesuar) — EquipmentItem class
- [ ] Yetenek ağacı görselleştirme

---

## Faz 7 — Dünya Haritası ✅

- [x] MapNodeData SO — 7 node tipi, keşif durumu, BattleTarget referansı
- [x] WorldMap — 10x10 grid, fog of war, A* pathfinding, otomatik harita üretimi
- [x] ExpeditionSystem — sefer gönderme, ilerleme, savaş, dönüş
- [x] WorldMapUI — grid görselleştirme, node seçimi, sefer paneli
- [x] FogOfWar — görünmeyen alanlar "?" ile gösterilir, keşfedilince açılır

---

## Faz 8 — İleri Sistemler ✅

- [x] TechNode SO — prerequisites, maliyet, süre
- [x] ResearchManager — araştırma süreci
- [x] TechTreeUI — araştırma paneli (kart + detay + progress)
- [x] FactionData SO — NPC faction, ilişki seviyesi
- [x] TradeSystem — al/sat mekanizması
- [x] FactionTradeUI — faction seçimi + al/sat paneli
- [x] QuestEnums — QuestType, QuestStatus, ObjectiveType
- [x] QuestData SO — amaçlar, ödüller, prerequisite zinciri
- [x] QuestInstance — runtime quest durumu (ilerleme, tamamlanma)
- [x] QuestManager — lifecycle (kabul, ilerleme, turn-in, ödül)
- [x] QuestLogUI — görev paneli (3 sekme, detay, kabul/turn-in)
- [x] QuestDataFactory — 5 örnek görev SO menüsü
- [x] MutantWave SO — dalga tanımı (güç, ceza, ödül)
- [x] MutantAttackManager — zamanlı saldırı döngüsü, uyarı, savunma hesabı
- [x] GameInitializer — quest yükleme + mutant döngüsü + oyun başlatma

---

## Faz 9 — Save/Load & Polish ✅

- [x] SaveData — tüm oyun verisini tutan seri serialize edilebilir yapı
- [x] SaveSystem — JSON save/load + auto-save (5dk) + quicksave
- [x] SaveMenuUI — kayıt listesi, yükleme, silme, yeni kayıt
- [x] AudioManager — müzik + SFX pool sistemi + ses ayarları
- [x] AudioConfig SO — 19 ses tipi tanımı
- [x] UIThemeSO — merkezi UI tema sistemi (buton, panel, metin stilleri)
- [x] UIThemeTag — element bazlı stil etiketleme bileşeni
- [x] UIThemeApplier — HollowGround > Apply UI Theme editor aracı (Ctrl+Shift+T)
- [x] Post-apokaliptik koyu tema uygulandı (nav bar, build/training/hero panelleri)
- [x] StrategyCamera yeniden yazıldı — raycast bağımlılığı kaldırıldı, zoom/pan/rotate düzeltildi
- [x] Post-processing (bloom, vignette)
- [x] Bina inşaat animasyonu
- [x] Atmosfer efektleri

---

## Faz 10 — Content ✅

- [x] BaseStarter — otomatik baslangic us yerlesimi (CC + Farm + WoodFactory + WaterWell)
- [x] 3 NPC Faction SO factory (Scavenger Guild, Iron Legion, Green Haven)
- [x] 10 arastirma SO factory (5 temel + 5 gelismis)
- [x] 15 gorev SO factory (10 ana + 5 yan)
- [x] BALANCE.md — tum dengeleme referans tablosu
- [x] 15+ bina modeli sahne yerlesimi ✅
- [ ] 5+ karakter modeli sahne yerlesimi (editor isi)
- [ ] Sahne dekorasyonu ve atmosfer (editor isi)

---

## Faz 12 — Bina Model Sistemi ✅

- [x] 15 bina x 7 model = 105 FBX (Blender ile üretildi)
- [x] BuildingData.BuildingModels struct — 7 GameObject slot (Construct, L01, L03, L05, L10, Damaged, Destroyed)
- [x] Level threshold: L01 (lv1-2), L03 (lv3-4), L05 (lv5-9), L10 (lv10)
- [x] State-based model swap — Building.cs UpdateModel() state'e göre doğru modeli instantiate eder
- [x] Z-fighting fix — model localPosition.y = 0.015f offset
- [x] Ghost placement fix — BuildingPlacer _cachedCoords / _cachedWorldPos ile ghost ve yerleştirme uyumu
- [x] Hasar/Tamir sistemi — ApplyDamage(), Repair(), kaynak harcayıp Active'e dönme
- [x] Editor araçları — FBX toplu import ayarı, FBX→SO otomatik bağlama, binding raporu

---

## Faz 13 — Refactoring ✅

- [x] Singleton<T> base class — protected set Instance, Destroy(gameObject) duplicate koruması
- [x] UIPrimitiveFactory — 10+ static metod (CreateUIObject, AddThemedText, CreateButton, vb.)
- [x] UIColors — merkezi renk tanımları (PanelColors, GetRarityColor, GetNodeColor, GetStateColor, GetResourceColor)
- [x] CostEntryHelper.Costs() — merkezi maliyet oluşturma utility
- [x] 4 UI panel UIPrimitiveFactory + UIColors'a taşındı (FactionTrade, SaveMenu, TechTree, WorldMap)
- [x] Magic numbers → GameConfig SO'ya taşındı (DemolishRefundRatio, RepairCostRatio, WallDefenseBonus, vb.)
- [x] Ölü kod silindi (GameEvent.cs, PlacementValidator.cs)
- [x] ToastUI yeniden yazıldı (lazy activation, runtime container)
- [x] SessionLogger event subscription düzeltmesi
- [x] 15+ yeni toast mesajı (bina events, kaynak eksik detay, mutant saldırı)

---

## Faz 14 — Visual & Polish ✅

- [x] GridOverlayRenderer — LineRenderer ile grid görselleştirme, snake/zigzag pattern, smooth fade
- [x] Bina footprint highlight — yeşil/kırmızı, rotation destekli
- [x] WeatherSystem — 5 hava durumu (Clear, LightRain, HeavyRain, DustStorm, RadiationStorm)
- [x] Post-processing per-weather (vignette, saturation, color filter, chromatic aberration)
- [x] BuildingHighlight — 1.05x outline mesh, pulsing alpha
- [x] DamageEffects — 3 fire emitter, 2 smoke emitter, explosion burst
- [x] ScreenShake — Perlin noise shake, exponential decay
- [x] AtmosphereEffects — dust/fog particles, embers particle
- [x] Pause menü (ESC) — Resume, Save/Load, Quit butonları
- [x] BuildMenu kaynak ikonları — renkli ● ile kaynak maliyeti gösterimi

---

## Faz 11 — Playtest & Bugfix ✅

- [x] Ground plane kalıcı olarak sahneye eklendi (HollowGround > Setup Ground & Camera)
- [x] "Ground" layer oluşturuldu, BuildingPlacer _groundMask ayarlandı
- [x] Camera.main null check eklendi (BuildingPlacer _cam cache)
- [x] StrategyCamera FocusOn düzeltildi (_currentPos de senkronize ediliyor)
- [x] StrategyCamera bounds grid merkezine göre ayarlandı (-30,130)
- [x] CameraRig > Main Camera tag = MainCamera ayarlandı
- [x] Bina yerleştirme çalışıyor (CommandCenter test edildi)
- [x] BuildingManager GameManager objesine eklendi
- [x] GroundManager.cs silindi (çift ground üretiyordu)
- [x] GameInitializer ground oluşturma kaldırıldı (editor ile kalıcı)
- [x] 14 bug düzeltmesi (onClick listeners, Türkçe metinler, prefab eksikleri, mantık hataları)
- [x] Post-processing değerleri düşürüldü (bloom 0.2, vignette 0.2, filmgrain kapalı)
- [x] AtmosphereEffects varsayılanları düşürüldü (fog 0.004, dust kapalı, fog particles kapalı)
- [x] Kaynak üretim testi (Farm → Food, Mine → Metal)
- [x] Tüm bina tipleri yerleştirme testi (CC, Farm, Mine, Barracks)
- [x] UI panel testleri (ResourceBar, BuildMenu, BuildingInfo)
- [x] Askeri sistem testi (eğitim, ordu)
- [x] Hero sistemi testi (summon, TechPart ile)
- [x] Dünya haritası testi (sefer, fog of war)
- [x] Save/Load testi (F5 QuickSave, F9 QuickLoad)
- [x] GameConfig SO — DevMode, DisableMutantAttacks, BoostStartingResources, SessionLog
- [x] SessionLogger — tüm oyun eventlerini dosyaya yazar
- [x] Faction Ticaret testi (3 faction, BUY/SELL, ilişki)
- [x] Araştırma testi (TechTreeUI, 10 tech SO)
- [x] Görev sistemi testi (QuestLogUI, 5 quest SO)
- [x] Mutant saldırı testi (wave, bina yıkımı, toast mesajları)
- [x] Tüm UI paneller runtime oluşturucuya çevrildi
- [x] ActionBar padding (60px) tüm panellere uygulandı
- [x] Close butonları kaldırıldı, ActionBar toggle ile açılıp kapanıyor
- [x] UITheme font (Roboto) runtime panellere uygulanıyor
- [x] FactionData SO'lar oluşturuldu (Scavenger Guild, Iron Legion, Green Haven)

### Klasör Yapısı
```
Assets/_Project/Scripts/
├── Core/        GameManager, TimeManager, Singleton, GameInitializer,
│                SaveData, SaveSystem, AudioManager, AudioConfig, BaseStarter,
│                PostProcessingSetup, AtmosphereEffects, GameConfig, SessionLogger,
│                WeatherSystem
├── Camera/      StrategyCamera, ScreenShake
├── Grid/        GridSystem, GridCell, GridVisualizer, GridOverlayRenderer
├── Buildings/   BuildingType, BuildingData, Building, BuildingManager,
│                BuildingPlacer, BuildingSelector, BuildingDatabase,
│                BuildingConstructionAnimation, BuildingHighlight, DamageEffects
├── Resources/   ResourceType, ResourceManager
├── Army/        TroopType, TroopData, ArmyManager
├── Combat/      BattleCalculator, BattleTarget, BattleManager,
│                MutantWave, MutantAttackManager
├── Heroes/      HeroEnums, HeroData, Hero, HeroManager
├── World/       MapNodeData, WorldMap, ExpeditionSystem
├── Tech/        TechNode, ResearchManager
├── NPCs/        FactionData, TradeSystem
├── Quests/      QuestEnums, QuestData, QuestInstance, QuestManager
├── UI/          UIManager, PanelManager, ResourceBarUI, BuildMenuUI, BuildingInfoUI,
│                ToastUI, TrainingPanelUI, ArmyPanelUI, BattleReportUI,
│                HeroPanelUI, WorldMapUI, TechTreeUI, FactionTradeUI,
│                QuestLogUI, SaveMenuUI, DebugHUD,
│                UIThemeSO, UIThemeTag, UIPrimitiveFactory, UIColors
└── Editor/      GridSystemEditor, BuildingDataFactory, TroopDataFactory,
                 HeroDataFactory, QuestDataFactory, FactionDataFactory,
                 TechNodeFactory, GhostMaterialCreator,
                 UIThemeApplier, SceneSetupEditor, GameConfigCreator,
                 PostProcessingProfileFactory, GroundSetupEditor
```

### Dokumanlar
```
ROADMAP.md    — Gelistirme plani
GDD.md        — Oyun tasarim dokumanı
BALANCE.md    — Dengeleme referans tablosu
```

*Son güncelleme: Faz 15 — Organic Road System*

---

## Faz 15 — Organic Road System ✅

- [x] Building rotation persistence — save/load ile bina yönü korunuyor
- [x] Building.GetDoorCell() — kapı yönü hesaplama (rotation 0=-Z, 1=-X, 2=+Z, 3=+X)
- [x] Building.GetRotatedFootprint() — rotation'a göre footprint döndürme
- [x] RoadManager singleton — BFS pathfinding (0-1 deque), bina inşaatı bitince yol oluşturma
- [x] RoadVisualizer — connected tile sistemi, Bezier köşeler, ellipsoid caps, procedural texture
- [x] Auto-rotation — bina yerleştirirken yola doğru otomatik dönme
- [x] Orphan road cleanup — bina yıkılınca 30s sonra bağlantısız yollar fade-out
- [x] Manual road removal — sağ-tık ile yol silme
- [x] Save/load entegrasyonu — yol verisi + bina rotasyonu kaydetme/yükleme
- [ ] Orphan cleanup bug fix — BFS connectivity check çalışmıyor
- [ ] Sağ-tık sadece orphan yolları silmeli — aktif yollar korunmali
- [ ] Yol hücrelerine bina yerleştirme engeli — BuildingPlacer road cell kontrolü

*Son güncelleme: AGENTS.md dokümantasyon guncellemesi — tum tamamlanan isler yansitildi*

---

## Faz 17 — Settler Visual & Depth

### 17a — WalkerBase Refactoring (DONE)

- [x] WalkerBase abstract sinifi — grid-based movement, path following, rotation smoothing
- [x] WalkerBase abstract OnArrivedDestination() callback
- [x] WalkerManager singleton — tum walker'larin merkezi Update() dongusu (tek Tick loop)
- [x] SettlerWalker : WalkerBase refactoring — mevcut work cycle korunarak
- [x] WalkerStateMachine entegrasyonu — domain layer state management
- [x] Path cache — WalkerManager'da Dictionary<(start,end), path> cache
- [x] Path cache invalidation — RoadManager.OnRoadsChanged event'inde cache temizleme
- [x] GameObject pool — WalkerManager'da Stack<SettlerWalker> recycle mekanizmi
- [x] Grid-cell occupancy — walker'larin ust uste binmesini onleme
- [x] TimeManager.GameSpeed tek noktadan okuma
- [x] SettlerManager sadelestirme — tick delegasyonu WalkerManager'a
- [x] Save/load uyumlulugu — SettlerWalkerSave format degismemeli
- [x] Worker rebalancing — yeni bina aktif oldugunda worker yeniden dagitimi (ReassignJob)
- [x] BuildingInfoUI worker bilgisi — assigned/required worker gosterimi
- [x] GameConfig dengeleme — SettlersPerPopulation=1.0, MaxSettlers=50
- [x] Playtest 13/13 gecmeli, Settler job system calismali

### 17b — NPC Visual Feedback (#23)

- [ ] Settler tasima efekti: toz particle + ayak sesi SFX
- [ ] Farm hasat animasyonu: yesil isik + hasat particle burst
- [ ] Mine/WoodFactory is animasyonu: kaynak particle efekti
- [ ] Atak sirasinda Defender settler davranisi
- [ ] Settler calisma durumu mini ikon/indicator

### 17c — SettlerPanel Enrichment (#24)

- [ ] Genel ozet satiri: "7/12 settler calisiyor"
- [ ] Bina bazli isci dagilimi gosterimi
- [ ] Rol dagilimi pasta grafik
- [ ] Bosta settler uyarisi
- [ ] Eksik isci olan binalar listesi

### 17d — Quick Tooltips (#25)

- [ ] Bina hover tooltip: worker fill ratio + production bonus
- [ ] Settler tikla tooltip: rol, atama suresi, moral

*Son güncelleme: Faz 17a tamamen tamamlandi — WalkerBase + pool + occupancy + WalkerStateMachine + rebalancing + GameConfig*
