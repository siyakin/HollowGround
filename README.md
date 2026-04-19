# Hollow Ground

Nükleer savaş sonrası şehir kurma + strateji + RPG oyunu. Unity 6 + URP + 3D Low Poly.

## Hızlı Başlangıç

- **GDD**: [GDD.md](./GDD.md) — Oyun tasarım dokümanı
- **Roadmap**: [ROADMAP.md](./ROADMAP.md) — Geliştirme planı (10 faz)
- **Balans**: [BALANCE.md](./BALANCE.md) — Oyun dengesi referansı

## Proje Yapısı

```
Assets/_Project/
├── Scripts/           # 55 C# script — 14 modül, ~4500+ satır
│   ├── Core/          # GameManager, SaveSystem, TimeManager, GameInitializer, AudioManager
│   ├── Grid/          # 50x50 grid sistemi, yerleştirme doğrulama
│   ├── Buildings/     # 15 bina tipi, üretim, yükseltme, yıkım, ghost preview
│   ├── Resources/     # 6 kaynak tipi, kapasite yönetimi
│   ├── Army/          # 5 birlik tipi, eğitim kuyruğu, moral sistemi
│   ├── Combat/        # Otomatik savaş çözümleme, mutant dalgaları
│   ├── Heroes/        # Gacha summon, seviye/xp, ekipman, yaralanma
│   ├── World/         # 10x10 dünya haritası, sis perdesi, A* pathfinding
│   ├── Tech/          # Teknoloji ağacı, araştırma kuyruğu
│   ├── Quests/        # 10 görev tipi, zincir sistemi, günlük görevler
│   ├── NPCs/          # Faction ilişkileri, ticaret sistemi
│   ├── Camera/        # RTS strateji kamerası (WASD, zoom, rotasyon)
│   ├── UI/            # 15 UI panel (kaynak barı, bina menü, dünya haritası, vb.)
│   └── Editor/        # 8 editor aracı (SO fabrikaları, custom inspector)
├── ScriptableObjects/ # Bina, birlik, hero, görev, savaş hedefi verileri
├── Prefabs/           # UI prefablari (SaveSlot, NodeButton, HeroCard, Toast)
├── Models/            # 4 model paketi (~100+ model)
│   ├── CityPack/      # Şehir modelleri, karakterler, aksesuarlar
│   ├── PostApocolypsePack/ # Silahlar, zombiler, sokak objeleri
│   ├── NaturePack/    # Ağaçlar, kayalar, çalılar, çiçekler
│   └── SurvivalPack/  # Kamp ekipmanları, aletler, eşyalar
├── Materials/         # Ghost materyalleri (geçerli/geçersiz yerleştirme)
├── Audio/             # Müzik, SFX, ortam sesi (boş — içerik eklenmesi bekleniyor)
└── Settings/          # Input action map, render pipeline ayarları
```

## Uygulanan Sistemler

| Modül | Durum | Detay |
|-------|-------|-------|
| Game State | Tamamlandı | Menü/Oyun/Duraklatma/İnşa state machine |
| Grid System | Tamamlandı | 50x50 grid, hücre durumu, snap-to-grid |
| Building System | Tamamlandı | 15 bina tipi, inşa/üretim/yükseltme/yıkım döngüsü |
| Resource System | Tamamlandı | 6 kaynak, başlangıç değerleri, kapasite yönetimi |
| Army System | Tamamlandı | 5 birlik tipi, eğitim kuyruğu, moral, güç hesaplama |
| Combat System | Tamamlandı | Otomatik çözümleme, sefer yönetimi, ganimet |
| Mutant Attacks | Tamamlandı | Zamanlı dalga sistemi, büyüyen zorluk, savunma hesaplama |
| Hero System | Tamamlandı | Gacha summon, 5 nadirlik, seviye/xp, ekipman, yaralanma |
| World Map | Tamamlandı | 10x10 grid, sis perdesi, A* pathfinding, sefer sistemi |
| Tech Tree | Tamamlandı | 5 kategori, önkoşul zinciri, araştırma kuyruğu |
| Quest System | Tamamlandı | 4 görev tipi, 10 hedef tipi, zincir sistemi |
| Trade System | Tamamlandı | Faction ilişkileri, alım/satım, itibar |
| Save/Load | Tamamlandı | JSON serileştirme, otomatik kayıt, tüm state yakalama |
| Audio Manager | Tamamlandı | 19 ses tipi, SFX pool, müzik, ses seviyesi kontrolü |
| Strategy Camera | Tamamlandı | WASD, zoom, rotasyon, edge panning, sınırlar |
| UI (15 Panel) | Tamamlandı | Kaynak barı, bina menü, ordu, hero, dünya haritası, tech ağacı, görev, ticaret, kayıt |
| Editor Tools | Tamamlandı | 8 fabrika aracı + custom inspector |

## Tamamlanmamış / Bekleyen

- **Sahne düzeni**: Manager GameObject'leri ve UI Canvas sahnede henüz yerleştirilmedi
- **Bina prefablari**: 3D bina prefab'ları oluşturulmadı (modeller var, prefab yok)
- **Birlik/Hero prefablari**: Görsel prefab'lar eksik
- **Ses içerikleri**: Music, SFX, Ambient klasörleri boş
- **Animasyonlar**: Hiçbir animasyon clip'i yok
- **Faction/Tech SO verileri**: Klasörler mevcut ama .asset dosyaları oluşturulmadı
- **Araştırma bonusları**: `ResearchManager.ApplyBonuses()` sadece logluyor, sistemlere entegre değil
- **Hero yetenekleri**: İsim/açıklama var, çalışan yetenek sistemi yok
- **Post-processing**: Bloom, vignette, atmosfer efektleri yok
- **Görev objective hookları**: Sadece mutant saldırıları tetikliyor, diğer sistemlerden çağrılmıyor

## Teknik Stack

- **Unity 6** + **URP**
- **C#** (.NET Standard 2.1)
- **New Input System**
- **TextMeshPro** (UI)

## Durum

Tüm sistemlerin kod altyapısı tamamlandı (~4500+ satır, 55 script, 14 modül). Editor araçları, ScriptableObject verileri ve 4 model paketi hazır. Sahne entegrasyonu, prefab oluşturma, ses/animasyon içerikleri ve polish aşamaları bekleniyor.
