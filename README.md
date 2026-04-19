# Hollow Ground

Nükleer savaş sonrası şehir kurma + strateji oyunu. Unity 6 + URP + 3D Low Poly.

## Hızlı Başlangıç

- **GDD**: [GDD.md](./GDD.md) — Oyun tasarım dokümanı
- **Roadmap**: [ROADMAP.md](./ROADMAP.md) — Geliştirme planı (10 faz)
- **Proje Yapısı**: `Assets/_Project/`

## Proje Yapısı

```
Assets/_Project/
├── Scripts/           # Kaynak kod (14 modül)
│   ├── Core/          # GameManager, GameState, SaveSystem, TimeManager
│   ├── Grid/          # Grid tabanlı bina yerleştirme
│   ├── Buildings/     # Bina sistemi (üretim, yükseltme)
│   ├── Resources/     # Kaynak yönetimi
│   ├── Army/          # Askeri sistem
│   ├── Combat/        # Savaş motoru
│   ├── Heroes/        # Hero sistemi
│   ├── World/         # Dünya haritası
│   ├── Tech/          # Teknoloji ağacı
│   ├── Quests/        # Görev sistemi
│   ├── NPCs/          # NPC ve faction
│   ├── Camera/        # Strateji kamera
│   ├── UI/            # Arayüz panelleri
│   └── Editor/        # Editor araçları
├── ScriptableObjects/ # Veri asset'leri
├── Prefabs/           # Prefab'lar
├── Models/            # 3D modeller (low poly)
├── Materials/         # Materyaller
├── Animations/        # Animator ve clip'ler
├── Audio/             # Ses ve müzik
├── Scenes/            # Sahne dosyaları
└── Settings/          # Ayarlar ve input
```

## Teknik Stack

- **Unity 6** + **URP**
- **C#** (.NET Standard 2.1)
- **New Input System**
- **DOTween** (animasyonlar)
- **TextMeshPro** (UI)

## Durum

Planlama aşaması — Faz 1 başlatılacak.
