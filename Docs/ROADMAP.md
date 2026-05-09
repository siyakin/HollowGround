# Hollow Ground — ROADMAP

## Vizyon
Last War ilhamli, nukleer savas sonrasi strateji oyunu.
Sehir kurma + ordu yonetimi + hero sistemi + dunya kesfi.

---

## Tamamlanan Fazlar

| Faz | Ad | Ozet |
|-----|----|------|
| 1 | Temel Altyapi ✅ | Unity 6 + URP + Input System, StrategyCamera, GridSystem, ResourceManager, GameManager |
| 2 | Base Building ✅ | 15 BuildingType, BuildingData SO, ghost preview, grid snap, BuildingManager |
| 3 | UI ✅ | 15+ panel (UIManager), ResourceBar, BuildMenu, BuildingInfo, ToastUI, DebugHUD |
| 4 | Askeri ✅ | 5 birlik SO, egitim kuyrugu, moral sistemi, ArmyManager |
| 5 | Savas ✅ | BattleCalculator, BattleTarget SO, sefer sistemi, BattleReportUI |
| 6 | Hero ✅ | 5 rol/rarity, gacha summon, ekipman, XP, HeroPanelUI |
| 7 | Dunya Haritasi ✅ | 10x10 grid, fog of war, A* pathfinding, ExpeditionSystem |
| 8 | Ileri Sistemler ✅ | TechTree, Faction/Ticaret, Quest, Mutant saldiri |
| 9 | Save/Load + Audio ✅ | JSON save, auto-save, AudioManager, UITheme, post-processing |
| 10 | Content ✅ | Starting buildings (GameInitializer), 3 faction, 10 tech, 15 quest, BALANCE.md |
| 11 | Playtest ✅ | 13/13 test gecti, GameConfig, SessionLogger |
| 12 | Bina Modeller ✅ | 105 FBX (15 bina x 7 state), state-based model swap, hasar/tamir |
| 13 | Refactoring ✅ | Singleton<T>, UIPrimitiveFactory, UIColors, dead code silindi |
| 14 | Visual ✅ | Grid overlay, weather, highlight, damage efektleri, pause menu |
| 15 | Settler Walker ✅ | NPC yolu yurume, nufus bazli spawn, animasyon |
| 16 | Settler Job ✅ | 12 rol, auto-assign, isci bazli uretim, SettlerPanelUI |
| 17 | Terrain + Domain ✅ | MapTemplate, MapRenderer, water shader, WalkerBase, WalkerStateMachine |
| 17b | Toast Overhaul ✅ | Stacked multi-toast, slide animation, load suppression |
| 18 | Garden & Merge ✅ | 4-garden merge, NeedsRoads flag, FBX updates |
| 19 | DebugHUD + Quest ✅ | 3-tab DebugHUD, quest triggers, TrainingPanel fix, 15/15 playtest |

**v0.27.0** — 19 faz tamamlandi, 2 playtest gecti.

---

## Bekleyen Islemler

### Yarin/Yakin (P0)
- [ ] #34 Training queue not restored on load
- [ ] #35 Building ProductionTimer save/load eksik
- [ ] #39 Yol olan hucrelere bina yerlestirilebiliyor (partial fix)

### Kisa Vade (P1)
- [ ] #36 World Map & Expedition system rework
- [ ] #37 RoadManager orphan road cleanup
- [ ] #38 Manual road removal aktif/bagli yollari da silebiliyor
- [ ] #40 WorldMap.GenerateDefaultMap runtime SO
- [ ] 10 ek quest SO (QuestDataFactory ile)

### Orta Vade (P2)
- [ ] Garden: L03/L05/L10/Damaged/Destroyed FBX
- [ ] Garden: Save/Load merge state
- [ ] Garden: SettlerJobManager worker role
- [ ] NPC Visual Feedback (toz, ayak sesi, hasat animasyonu)
- [ ] SettlerPanel Enrichment (ozet, dagilim, pasta grafik)
- [ ] Quick Tooltips (bina hover, settler tikla)
- [ ] CONTROLS.md: Eksik kisayollar (Space pause, 1/2/3 speed, Ctrl+S, Delete demolish)

### Polish (Editor isi)
- [ ] 5+ karakter modeli sahne yerlesimi
- [ ] Sahne dekorasyonu ve atmosfer
- [ ] Garden BuildingSpecs dokumani

---

## Versiyon Gecmisi

| Versiyon | Tarih | Faz |
|----------|-------|-----|
| 0.10.0 | 2026-04-28 | Content (Faz 10) |
| 0.11.0 | 2026-04-28 | Playtest (Faz 11) |
| 0.12.0 | 2026-04-28 | Bina Modeller (Faz 12) |
| 0.13.0 | 2026-04-28 | Refactoring (Faz 13) |
| 0.14.0 | 2026-04-28 | Visual (Faz 14) |
| 0.15.0 | 2026-04-28 | UITheme (Faz 14b) |
| 0.16.0 | 2026-04-28 | About Panel |
| 0.17.0 | 2026-04-29 | Save/Load Fix |
| 0.18.0 | 2026-04-29 | Road System |
| 0.19.0 | 2026-04-30 | Settler Walker |
| 0.20.0 | 2026-05-02 | Settler Job (Faz 16) |
| 0.21.0 | 2026-05-02 | Water Shader |
| 0.22.0 | 2026-05-03 | Domain Layer (Faz 17) |
| 0.23.0 | 2026-05-03 | Namespace Fixes |
| 0.24.0 | 2026-05-06 | Toast Overhaul (Faz 17b) |
| 0.25.0 | 2026-05-06 | Bug Batch |
| 0.26.0 | 2026-05-08 | Garden & Merge (Faz 18) |
| 0.27.0 | 2026-05-08 | DebugHUD + Quest (Faz 19) |
