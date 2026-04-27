WALLS - 1x1 grid, 1.9x0.4m footprint (uzun duvar). Z axis UP, Z=0 ground.
Post-apokaliptik savunma duvarı. Tuğla, beton, dikenli tel.

=== LEVEL EVRİMİ ===

L01 (2.0m yükseklik):
- Tuğla/moloz duvar: box 1.9x1.8x0.25m, düzensiz üst kenar (vertex'ler 0.1-0.2m değişken), tuğla dokulu
- Dikenli tel: torus 0.3m major radius x 0.01m minor, 2 adet üst kenarda
- 1 destek payandası: kutu 0.1x0.8x0.1m, duvar arkasında çapraz
- GroundPlot: 1.9x0.02x0.4m
- Tri: 150-250

L03 (2.4m yükseklik):
- Beton bariyer: box 1.9x2.2x0.3m, pürüzlü yüzey, düz üst
- Dikenli tel 3 sıra: torus üst kenarda
- Telsiz direği: silindir 0.02m radius x 0.6m, duvar ucunda, üstünde küçük kutu (telsiz)
- Kum torbası: 3 silindir 0.1m radius x 0.1m, duvar önünde
- Tri: 250-400

L05 (2.8m yükseklik):
- Reinforced concrete duvar: box 1.9x2.6x0.35m, metal takviye çubuklar (ince silindir) yüzeyde
- Makineli tüfek yuvası: duvarın bir ucunda çentik (kutu açıklık 0.3x0.2m), üstte koruma (düzlem)
- Dikenli tel 4 sıra
- 2 telsiz direği (iki uçta)
- Kum torbaları duvarı: 6 silindir, 2 sıra
- Arama feneri: silindir 0.05m radius x 0.06m, duvar ucunda, yönlü
- Tri: 400-600

L10 (3.8m yükseklik):
- Çift duvar: iç duvar box 1.9x3.5x0.2m + dış duvar 1.9x3.0x0.15m, arası 0.1m
- Nöbetçi yeri: duvar ortasında küçük çıkıntı (kutu 0.4x0.5x0.3m), üstü korumalı
- Floodlight: silindir 0.06m radius x 0.08m, nöbetçi yerinde
- Dikenli tel 5 sıra, iç + dış
- 2 floodlight (iki uçta)
- Metal takviyeler: 4 dikey çubuk (silindir 0.015m radius) her yüzde
- Kum torbaları: 8 adet, 2 sıra
- Beton platform: 1.9x0.08x0.5m
- Tri: 700-1000

=== CONSTRUCT ===
- 2 kazık (0.04x0.5x0.04m)
- Yarım duvar (kutu 1.9x0.5x0.15m)
- 1 kum torbası
- Tuğla yığını (küçük kütle)
- GroundPlot
- Tri: 80-150

=== DAMAGED ===
- Duvarın ortasında büyük delik (0.5x0.4m)
- Üst kenar düzensiz
- Dikenli tel sarkık bir tarafta
- Kum torbaları dağılmış
- Debris: tuğla parçaları, beton kırığı, tel
- Tri: 250-400

=== DESTROYED ===
- Sadece temel (kutu 1.9x0.1x0.2m)
- 1-2 tuğla ayakta
- Debris: 8-10 parça (tuğla, beton, tel, kum torbası patlamış)
- Yanık izi
- Tri: 150-250

=== RENK PALETİ ===
- mat_brick: #8B5A3A, Roughness 0.9, Metallic 0.0 (tuğla)
- mat_concrete: #6B6B63, Roughness 0.9, Metallic 0.0 (beton duvar, platform)
- mat_rusty_metal: #7A4631, Roughness 0.7, Metallic 0.3 (dikenli tel, takviye, telsiz)
- mat_dark_metal: #3A3A38, Roughness 0.4, Metallic 0.6 (fener, tüfek yuvası)
- mat_sand: #A89060, Roughness 0.95, Metallic 0.0 (kum torbaları)
- mat_dirt: #6B5A3A, Roughness 0.95, Metallic 0.0 (zemin)
