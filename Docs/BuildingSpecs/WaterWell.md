WATER WELL - 1x1 grid, 1.9x1.9m footprint. Z axis UP, Z=0 ground.
Post-apokaliptik su kuyusu. Paslı metal, taş, ahşap.

=== LEVEL EVRİMİ ===

L01 (2.0m yükseklik):
- Kuyu ağzı: silindir 0.4m radius x 0.4(Z)m, içi boş (üstü açık), taş doku
- Taş temel: silindir etrafında 8 küçük küre (0.06m) dize şeklinde
- Ahşap çark: 2 direk (0.04x0.8(Z)x0.04m) kuyu iki yanında + silindir (0.02m radius x 0.5m) yatay bağlantı + kol (0.03m radius x 0.3m çap çark)
- 1 kova: silindir 0.06m radius x 0.1m + yarım küre alt + ince silindir kulp
- Halat: ince silindir 0.005m radius, çarktan kovaya sarkan
- Ahşap kapak: düzlem 0.5x0.5m, kuyu yanında yatık duruyor
- GroundPlot: 1.9x0.02x1.9m
- Tri: 200-350

L03 (2.3m yükseklik):
- Kuyu ağzı taş duvarla çevrili: silindir 0.45m radius x 0.5(Z)m, daha kalın duvar
- Metal çark: direkler metal (silindir 0.02m radius), çark daha büyük (0.04m radius x 0.4m)
- 2 kova: biri çarkta asılı, biri yerde
- Su birikintisi: kuyu yanında düzlem, mavimsi vertex color
- 1 metal boru: ince silindir kuyu ağzından yana uzanıyor
- 1 küçük bidon: silindir 0.08m radius x 0.2m yanında
- Tri: 300-500

L05 (2.6m yükseklik):
- Kuyu üzerine pompa mekanizması: metal kutu 0.3x0.3x0.2m, kol (silindir 0.02m radius x 0.4m), dişli (silindir 0.05m radius x 0.04m)
- Çıkış borusu: L-shaped silindir 0.025m radius, pompadan yukarı sonra yana
- Küçük su deposu: silindir 0.2m radius x 0.4(Z)m, 4 ince ayak (0.02m radius x 0.3m) üzerinde
- Bağlantı borusu: pompadan su deposuna
- 2 metal boru yerde (eski sistemden kalma)
- 1 büyük bidon: silindir 0.1m radius x 0.3m
- Kuyu taş duvarı kaldı, kapak yok
- Tri: 450-650

L10 (3.35m yükseklik, tamamen yeni):
- Motorlu pompa: büyük metal kutu 0.5x0.4x0.3m, üzerinde egzoz (silindir 0.03m radius x 0.15m), yanında motor (silindir 0.1m radius x 0.15m)
- Büyük su tankı: silindir 0.4m radius x 1.0(Z)m, 4 ayak (0.03m radius x 0.3m) üzerinde, tepesinde kapak (disk)
- Filtrasyon ünitesi: kutu 0.2x0.3x0.15m, borularla tank'a bağlı
- Boru ağı: 4-5 boru (silindir 0.02m radius) pompa → filtre → tank, bir kısmı yere iniyor
- Musluk: tank altında, ince silindir + valve (küçük kutu)
- Beton platform: kutu 1.5x0.08x1.5m tüm sistem altında
- Kontrol paneli: küçük kutu 0.1x0.1x0.05m pompa üzerinde
- 2 büyük boru yerden yukarı (gelen su hattı)
- Tri: 700-1000

=== CONSTRUCT ===
- 4 kazık (0.04x0.6x0.04m) kuyu alanı çevresinde
- Gergi telleri
- Yarım yapılmış taş duvar (3-4 taş)
- Ahşap çark direkleri (çark yok)
- 1 kova yerde
- GroundPlot
- Tri: 100-200

=== DAMAGED ===
- Kuyu duvarı çatlamış, 2 taş düşmüş
- Çark eğilmiş, halat kopuk
- Kova yerde devrilmiş
- 1 direk kırık
- Debris: taş parçaları, tahta kırıntısı, paslı metal
- Su birikintisi etrafta (sızıntı)
- Tri: 300-450

=== DESTROYED ===
- Kuyu ağzı tıkalı (moloz)
- Duvar yok, sadece temel izi
- Çark tamamen yıkık: direkler yerde, çark ayrı
- Debris: 10-12 parça (taş, ahşap, metal, halat parçası, kova parçası)
- Yanık izi
- Çamur alanı (su taşkını)
- Tri: 200-350

=== RENK PALETİ ===
- mat_stone: #8B8B7A, Roughness 0.95, Metallic 0.0 (kuyu duvarı, taşlar)
- mat_rusty_metal: #7A4631, Roughness 0.7, Metallic 0.3 (çark, borular, pompa, tank)
- mat_dark_metal: #3A3A38, Roughness 0.4, Metallic 0.6 (motor, dişli, valve)
- mat_rotten_wood: #5C4023, Roughness 0.85, Metallic 0.0 (direkler, kapak)
- mat_water: #4A7A8A, Roughness 0.2, Metallic 0.1 (su birikintisi)
- mat_concrete: #6B6B63, Roughness 0.9, Metallic 0.0 (L10 platform)
- mat_dirt: #6B5A3A, Roughness 0.95, Metallic 0.0 (zemin)
- mat_rope: #8B7355, Roughness 0.95, Metallic 0.0 (halat)
