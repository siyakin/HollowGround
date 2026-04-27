GENERATOR - 1x1 grid, 1.9x1.9m footprint. Z axis UP, Z=0 ground.
Post-apokaliptik enerji üretici. Paslı, yağlı, kablolar sarkık.

=== LEVEL EVRİMİ ===

L01 (2.5m yükseklik):
- Jeneratör gövdesi: box 0.5x0.4x0.4m (eski motosiklet motoru görünümü), paslı
- Egzoz borusu: silindir 0.025m radius x 0.3m yukarı, gövde üstünde
- Yakıt bidonu: silindir 0.08m radius x 0.2m, gövde yanında yatık
- 2 kablo: ince silindir 0.008m radius, gövdeden sarkıyor, uçları yerde
- Metal çerçeve: 4 ayak (0.02x0.15(Z)x0.02m) gövde altında
- GroundPlot: 1.9x0.02x1.9m
- Tri: 150-250

L03 (2.8m yükseklik):
- Dizel jeneratör: box 0.7x0.5x0.5m, daha büyük
- Büyük egzoz: silindir 0.03m radius x 0.5m, üstte baca (koyu renk uç)
- Yakıt deposu: silindir 0.12m radius x 0.35m, gövde yanında dik
- Yakıt borusu: ince silindir depodan gövdeye
- 4 kablo: gövdeden 4 yöne sarkan, yerde kıvrımlı
- Metal çerçeve büyüdü, altına sac plaka (koruma)
- 1 alet kutusu: kutu 0.15x0.1x0.1m yanında
- Tri: 250-400

L05 (3.1m yükseklik):
- Sanayi jeneratörü: box 0.8x0.6x0.6m, üzerinde panel (düzlem, düğmeler = küçük silindirler)
- Egzoz bacası: silindir 0.04m radius x 0.8m, kafes kapağı (ince silindirler üstte)
- Güç dağıtım panosu: kutu 0.3x0.4x0.08m, gövde üstünde, içinde sigorta kutuları (3 küçük kutu)
- 6 kablo: dağıtım panosundan çıkan, boru içinde (yarım silindir kanal) bir kısmı yeraltı
- Yakıt deposu büyüdü: silindir 0.15m radius x 0.4m, 2 ayak üzerinde
- Soğutma kanatları: gövde yanında 3 ince plaka (0.02m aralıkla)
- Tri: 400-600

L10 (3.85m yükseklik, tamamen yeni):
- Güç santrali: büyük kutu 1.2x1.0x1.0m, çelik gövde, üzerinde kontrol paneli (düzlem + 4 düğme + 2 gösterge = silindir)
- Soğutma kulesi: silindir 0.3m radius, alt geniş üst dar (konik), 0.8m yükseklik, gövde yanında
- Büyük egzoz: silindir 0.05m radius x 1.0m, susturucu (kutu 0.15x0.15x0.1m ortada)
- Solar panel: eğimli düzlem 0.6x0.04x0.8m, çelik ayaklar üzerinde, gövde arkasında
- Transformatör: kutu 0.3x0.4x0.3m, yanında, 2 büyük bağlantı (silindir 0.03m radius)
- Kablo kanalı: yerde yarım silindir 0.05m radius, 1.5m uzunluk, 4 kablo içinde
- Yakıt tankı büyük: silindir 0.25m radius x 0.6m, yatık, metal kayışlarla sabit
- Beton platform: 1.7x0.08x1.7m
- Uyarı tabelası: düzlem 0.2x0.15m, "HIGH VOLTAGE"
- Tri: 800-1200

=== CONSTRUCT ===
- Metal çerçeve (4 ayak + 2 kiriş) yarım
- Gövde yok, sadece motor bloğu (küçük kutu)
- 2 kablo yerde
- 1 alet kutusu
- GroundPlot
- Tri: 80-150

=== DAMAGED ===
- Gövde eğilmiş, 1 ayak kırık
- Egzoz bükülmüş
- Kablolar kopuk, yerde dağınık
- Yakıt bidonu devrilmiş, sıvı izi (vertex color)
- Siyah duman lekesi gövde üstünde
- Debris: metal parça, vida, tel
- Tri: 250-400

=== DESTROYED ===
- Sadece metal çerçeve kalıntıları (2 ayak)
- Motor bloğu yerde (kutu, ters dönmüş)
- Debris: 10-12 parça (metal, kablo, plastik, vida, boru)
- Yanık izi büyük alan
- Siyah leke + küçük yangın kalıntıları
- Tri: 150-300

=== RENK PALETİ ===
- mat_rusty_metal: #7A4631, Roughness 0.7, Metallic 0.3 (gövde, çerçeve, borular)
- mat_dark_metal: #3A3A38, Roughness 0.4, Metallic 0.6 (motor, transformatör, paneller)
- mat_oil: #2A2A25, Roughness 0.8, Metallic 0.2 (egzoz, yağ lekeleri)
- mat_fuel: #5A3A1A, Roughness 0.6, Metallic 0.1 (yakıt deposu)
- mat_cable: #3A3A2A, Roughness 0.7, Metallic 0.1 (kablolar)
- mat_concrete: #6B6B63, Roughness 0.9, Metallic 0.0 (L10 platform)
- mat_solar: #1A1A2E, Roughness 0.2, Metallic 0.5 (solar panel)
- mat_warning: #C4A83A, Roughness 0.6, Metallic 0.2 (tabela)
- mat_dirt: #6B5A3A, Roughness 0.95, Metallic 0.0 (zemin)
