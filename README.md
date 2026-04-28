# Hollow Ground

Nükleer savaş sonrası şehir kurma + strateji + RPG oyunu. Unity 6 + URP + 3D Low Poly.

## Hızlı Başlangıç

- **GDD**: [GDD.md](Docs/GDD.md) — Oyun tasarım dokümanı
- **Roadmap**: [ROADMAP.md](Docs/ROADMAP.md) — Geliştirme planı (14 faz tamamlandı)
- **Balans**: [BALANCE.md](Docs/BALANCE.md) — Oyun dengesi referansı
- **Agent Rehberi**: [AGENTS.md](AGENTS.md) — AI agent kuralları ve teknik rehber

## Proje Yapısı

```
Assets/_Project/
├── Scripts/           # 75+ C# script — 14 modül
│   ├── Core/          # GameManager, SaveSystem, TimeManager, Singleton<T>,
│   │                  # GameInitializer, AudioManager, WeatherSystem,
│   │                  # GameConfig, SessionLogger, CostEntryHelper
│   ├── Grid/          # 50x50 grid sistemi, GridOverlayRenderer
│   ├── Buildings/     # 15 bina tipi, üretim, yükseltme, yıkım, hasar/tamir,
│   │                  # state-based model swap, BuildingHighlight, DamageEffects
│   ├── Resources/     # 6 kaynak tipi, kapasite yönetimi
│   ├── Army/          # 5 birlik tipi, eğitim kuyruğu, moral sistemi
│   ├── Combat/        # Otomatik savaş çözümleme, mutant dalgaları
│   ├── Heroes/        # Gacha summon, seviye/xp, ekipman, yaralanma
│   ├── World/         # 10x10 dünya haritası, sis perdesi, A* pathfinding
│   ├── Tech/          # Teknoloji ağacı, araştırma kuyruğu
│   ├── Quests/        # 5 görev SO, zincir sistemi
│   ├── NPCs/          # Faction ilişkileri, ticaret sistemi
│   ├── Camera/        # RTS strateji kamerası + ScreenShake
│   ├── UI/            # 15 UI panel, UIPrimitiveFactory, UIColors, UIThemeSO
│   └── Editor/        # 13 editor aracı (SO fabrikaları, FBX binder, scene setup)
├── ScriptableObjects/
│   ├── Buildings/     # 10 aktif bina SO
│   ├── Troops/        # 5 birlik SO (Infantry, Scout, Heavy, Sniper, Engineer)
│   ├── Heroes/        # 5 hero SO (Commander, Warrior, Ranger, Engineer, Scout)
│   ├── TechNodes/     # 10 teknoloji SO
│   ├── Factions/      # 3 faction SO (Scavenger Guild, Iron Legion, Green Haven)
│   ├── Quests/        # 5 quest SO
│   └── Targets/       # 5 BattleTarget SO
├── Models/
│   ├── Buildings/     # 15 bina x 7 model = 105 FBX (Blender)
│   ├── CityPack/      # Şehir modelleri, karakterler, aksesuarlar
│   ├── PostApocolypsePack/ # Silahlar, zombiler, sokak objeleri
│   ├── NaturePack/    # Ağaçlar, kayalar, çalılar, çiçekler
│   └── SurvivalPack/  # Kamp ekipmanları, aletler, eşyalar
├── Prefabs/           # UI prefablari (ToastItem, NodeButton, HeroCard,
│   └── UI/            # QuestItem, OfferItem, SaveSlot)
├── Materials/         # Ghost materyalleri (geçerli/geçersiz yerleştirme)
├── Audio/             # Müzik, SFX, ortam sesi
└── Settings/          # Input action map, render pipeline ayarları
```

## Uygulanan Sistemler

| Modül | Durum | Detay |
|-------|-------|-------|
| Game State | ✅ | Menü/Oyun/Duraklatma/İnşa state machine |
| Grid System | ✅ | 50x50 grid, hücre durumu, snap-to-grid, grid overlay |
| Building System | ✅ | 15 bina tipi, inşa/üretim/yükseltme/yıkım/tamir, state-based model swap |
| Bina Modelleri | ✅ | 105 FBX (15 bina x 7 state), Blender ile üretildi |
| Resource System | ✅ | 6 kaynak, başlangıç değerleri, kapasite yönetimi |
| Army System | ✅ | 5 birlik tipi, eğitim kuyruğu, moral, güç hesaplama |
| Combat System | ✅ | Otomatik çözümleme, sefer yönetimi, ganimet |
| Mutant Attacks | ✅ | Zamanlı dalga sistemi, büyüyen zorluk, bina hasarlandırma |
| Hero System | ✅ | Gacha summon, 5 nadirlik, seviye/xp, ekipman, yaralanma |
| World Map | ✅ | 10x10 grid, sis perdesi, A* pathfinding, sefer sistemi |
| Tech Tree | ✅ | 10 teknoloji SO, önkoşul zinciri, araştırma kuyruğu |
| Quest System | ✅ | 5 quest SO, zincir sistemi, kabul/ilerleme/turn-in |
| Trade System | ✅ | 3 NPC faction, alım/satım, itibar sistemi |
| Save/Load | ✅ | JSON serileştirme, auto-save (5dk), QuickSave (F5), QuickLoad (F9) |
| Audio Manager | ✅ | 19 ses tipi, SFX pool, müzik, ses seviyesi kontrolü |
| Strategy Camera | ✅ | WASD, zoom, rotasyon, edge panning, sınırlar, ScreenShake |
| UI (15 Panel) | ✅ | Kaynak barı, bina menü, ordu, hero, dünya haritası, tech ağacı, görev, ticaret, kayıt, pause |
| UITheme | ✅ | Merkezi tema sistemi, post-apokaliptik koyu tema, UIThemeTag |
| Editor Tools | ✅ | 13 fabrika aracı, FBX binder, custom inspector, scene setup |
| Weather System | ✅ | 5 hava durumu, auto-cycle, per-weather post-processing |
| Visual Effects | ✅ | Grid overlay, bina highlight, hasar efektleri, screen shake, atmosfer |
| Post-processing | ✅ | Bloom, vignette, color filter, chromatic aberration, weather-driven |
| Panel Manager | ✅ | Tek panel kuralı, panel geçmişi (stack), ActionBar highlight |
| Session Logger | ✅ | Tüm oyun eventlerini dosyaya yazar |
| GameConfig | ✅ | DevMode, hız çarpanları, mutant kontrolü, dengeleme parametreleri |

## Tamamlanmamış / Bekleyen

- [ ] **10 ek quest SO** — 5 mevcut, QuestDataFactory ile 10 daha eklenmeli
- [ ] **Hero yetenek ağacı görselleştirme** — yetenek sistemi var, UI görselleştirmesi eksik
- [ ] **5+ karakter modeli sahne yerleşimi** — editor işi
- [ ] **Sahne dekorasyonu ve atmosfer** — editor işi

## Teknik Stack

- **Unity 6** + **URP**
- **C#** (.NET Standard 2.1)
- **New Input System**
- **TextMeshPro** (UI)
- **Blender** (bina modelleri, 105 FBX)

## Durum

14 faz tamamlandı. Tüm sistemlerin kod altyapısı, 105 bina modeli, UI tema sistemi, visual efektler ve weather sistemi hazır. Playtest 13/13 test geçti. 75+ script, 14 modül, 38+ ScriptableObject, 13 editor aracı. Quest içerikleri, karakter modelleri ve sahne dekorasyonu bekleniyor.
