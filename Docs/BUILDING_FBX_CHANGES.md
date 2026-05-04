# Building FBX Blender Degisiklikleri

Bu dosya Blender'da yapilan bina modeli degisikliklerini kaydeder.
Her degisiklik tarih ve aciklama ile listelenir.

---

## Bina Temel Boyutlari

Grid cell = 2m. Maksimum footprint: 1x1 → 1.9m, 2x2 → 3.9m.
**Onemli:** Binalar seviye atlarsa da grid footprint (SizeX/SizeZ) hic degismez.
L01'den L10'a kadar tum modeller ayni width/depth sinirlari icinde kalmalidir. Sadece yukseklik (height) artar.

| Bina            | Grid  | Width(m) | Depth(m) | Height L01(m) | Height L10(m) |
|-----------------|-------|----------|----------|----------------|----------------|
| CommandCenter   | 2x2   | 3.9      | 3.9      | 4.0            | 8.1            |
| Farm            | 2x2   | 3.9      | 3.9      | 2.0            | 3.8            |
| Mine            | 2x2   | 3.9      | 3.9      | 3.0            | 5.7            |
| Barracks        | 2x2   | 3.9      | 3.9      | 3.0            | 4.8            |
| WaterWell       | 1x1   | 1.9      | 1.9      | 2.0            | 3.35           |
| Generator       | 1x1   | 1.9      | 1.9      | 2.5            | 3.85           |
| WoodFactory     | 2x2   | 3.9      | 3.9      | 3.0            | 4.8            |
| Hospital        | 1x1   | 1.9      | 1.9      | 3.0            | 4.8            |
| Storage         | 1x1   | 1.9      | 1.9      | 2.5            | 4.75           |
| Shelter         | 2x2   | 3.9      | 3.9      | 3.0            | 5.7            |
| Walls           | 1x1   | 1.9      | 0.4      | 2.0            | 3.8            |
| WatchTower      | 1x1   | 1.9      | 1.9      | 5.0            | 11.3           |
| Workshop        | 1x1   | 1.9      | 1.9      | 2.0            | 3.5            |
| ResearchLab     | 2x2   | 3.9      | 3.9      | 3.0            | 5.0            |
| TradeCenter     | 2x2   | 3.9      | 3.9      | 2.5            | 4.5            |

---

## #1 — Bina modelleri ground ile ayni yukseklikte olmali
- **Tarih:** 2026-05-03
- **Degisiklik:** Tum bina FBX modellerinin pivot noktasi Y=0 (ground seviyesi) olacak sekilde ayarlanmali. Yollar `Y=0.05` yukseklikte render ediliyor, bina modelleri de ayni referans noktasindan baslamali.
- **Etkilenen dosyalar:** `Assets/_Project/Models/Buildings/` altindaki tum 105 FBX (15 bina x 7 model)
- **Unity taraf:** `Building.cs` → `GroundYOffset = 0.05f` olarak guncellendi.
