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
- [ ] Sahne kurulumları (BuildingInfo, ToastUI, TechTree, FactionTrade, QuestLog bağlantıları)

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

## Faz 9 — Save/Load & Polish 🔶

- [x] SaveData — tüm oyun verisini tutan seri serialize edilebilir yapı
- [x] SaveSystem — JSON save/load + auto-save (5dk) + quicksave
- [x] SaveMenuUI — kayıt listesi, yükleme, silme, yeni kayıt
- [x] AudioManager — müzik + SFX pool sistemi + ses ayarları
- [x] AudioConfig SO — 19 ses tipi tanımı
- [ ] Post-processing (bloom, vignette)
- [ ] Bina inşaat animasyonu
- [ ] Atmosfer efektleri

---

## Faz 10 — Content ✅

- [x] BaseStarter — otomatik baslangic us yerlesimi (CC + Farm + WoodFactory + WaterWell)
- [x] 3 NPC Faction SO factory (Scavenger Guild, Iron Legion, Green Haven)
- [x] 10 arastirma SO factory (5 temel + 5 gelismis)
- [x] 15 gorev SO factory (10 ana + 5 yan)
- [x] BALANCE.md — tum dengeleme referans tablosu
- [ ] 15+ bina modeli sahne yerlesimi (editor isi)
- [ ] 5+ karakter modeli sahne yerlesimi (editor isi)
- [ ] Sahne dekorasyonu ve atmosfer (editor isi)

---

## Mevcut Script Sayısı: 55+

### Klasör Yapısı
```
Assets/_Project/Scripts/
├── Core/        GameManager, TimeManager, GameEvent, Singleton, GameInitializer,
│                SaveData, SaveSystem, AudioManager, AudioConfig, BaseStarter
├── Camera/      StrategyCamera
├── Grid/        GridSystem, GridCell, GridVisualizer, PlacementValidator
├── Buildings/   BuildingType, BuildingData, Building, BuildingManager,
│                BuildingPlacer, BuildingSelector, BuildingDatabase
├── Resources/   ResourceType, ResourceManager
├── Army/        TroopType, TroopData, ArmyManager
├── Combat/      BattleCalculator, BattleTarget, BattleManager,
│                MutantWave, MutantAttackManager
├── Heroes/      HeroEnums, HeroData, Hero, HeroManager
├── World/       MapNodeData, WorldMap, ExpeditionSystem
├── Tech/        TechNode, ResearchManager
├── NPCs/        FactionData, TradeSystem
├── Quests/      QuestEnums, QuestData, QuestInstance, QuestManager
├── UI/          UIManager, ResourceBarUI, BuildMenuUI, BuildingInfoUI,
│                ToastUI, TrainingPanelUI, ArmyPanelUI, BattleReportUI,
│                HeroPanelUI, WorldMapUI, TechTreeUI, FactionTradeUI,
│                QuestLogUI, SaveMenuUI, DebugHUD
└── Editor/      GridSystemEditor, BuildingDataFactory, TroopDataFactory,
                 HeroDataFactory, QuestDataFactory, FactionDataFactory,
                 TechNodeFactory, GhostMaterialCreator
```

### Dokumanlar
```
ROADMAP.md    — Gelistirme plani
GDD.md        — Oyun tasarim dokumanı
BALANCE.md    — Dengeleme referans tablosu
```

*Son güncelleme: Faz 10 tamamlandı*
