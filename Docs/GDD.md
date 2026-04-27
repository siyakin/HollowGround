# Hollow Ground — Game Design Document (GDD)

## Oyun Özeti

| Alan | Detay |
|------|-------|
| **İsim** | Hollow Ground |
| **Tür** | Şehir Kurma + Strateji + RPG |
| **Tema** | Nükleer Savaş Sonrası |
| **Platform** | PC / Mobil (Unity cross-platform) |
| **Motor** | Unity 6 + URP |
| **Görsel Stil** | 3D Low Poly |
| **Kamera** | Açılı üstten bakış (~45° eğimli, döndürülebilir, zoom) |
| **Hedef** | Tek kişilik (single-player) PVE deneyimi |

---

## Dünyanın Hikayesi

2047. Küresel nükleer çatışma sonrası dünya tanınmaz hale geldi. Bombalar durduğunda geriye sadece **kor** kaldı — yanmış şehirler, zehirli topraklar ve hayatta kalmaya çalışan insanlar.

Sen, **Hollow Ground** olarak bilinen bir bölgede uyanan bir hayatta kalansın. Burası bombalardan nispeten korunmuş bir vadi — ama güvenli değil. Mutantlar, çapulcular ve doğanın kendisi seni bekliyor.

Amacın: **Küllerden bir yerleşim kurmak, hayatta kalanları bir araya getirmek ve bu yeni dünyada bir düzen sağlamak.**

---

## Core Gameplay Loop

```
┌─────────────────────────────────────────────────┐
│                    BAŞLA                         │
│                      │                           │
│                      ▼                           │
│            ┌─────────────────┐                   │
│            │  Üssü Geliştir  │◄──────────┐       │
│            │  (Bina yap,     │           │       │
│            │   yükselt,      │           │       │
│            │   kaynak üret)  │           │       │
│            └────────┬────────┘           │       │
│                     │                    │       │
│                     ▼                    │       │
│            ┌─────────────────┐           │       │
│            │  Asker Topla &  │           │       │
│            │  Eğit           │           │       │
│            └────────┬────────┘           │       │
│                     │                    │       │
│                     ▼                    │       │
│       ┌──────────────────────────┐       │       │
│       │         SEÇ              │       │       │
│       ├──────────┬───────────────┤       │       │
│       │          │               │       │       │
│       ▼          ▼               ▼       │       │
│  ┌─────────┐ ┌────────┐  ┌───────────┐  │       │
│  │ Keşfet  │ │Savaş   │  │ İttifak   │  │       │
│  │ (Dünya  │ │(PvE    │  │ (NPC/Tic. │  │       │
│  │  harita │ │ mutant │  │  faction) │  │       │
│  │  gidip  │ │ baskın │  │           │  │       │
│  │  kaynak │ │ vs.)   │  │           │  │       │
│  │  topla) │ │        │  │           │  │       │
│  └────┬────┘ └───┬────┘  └─────┬─────┘  │       │
│       │          │             │         │       │
│       └──────────┴─────────────┘         │       │
│                  │                        │       │
│                  ▼                        │       │
│         ┌─────────────────┐               │       │
│         │  Ganimet Kazan  │               │       │
│         │  (kaynak, ekip- │               │       │
│         │   man, hero)    │───────────────┘       │
│         └─────────────────┘                       │
│                                                   │
└─────────────────────────────────────────────────┘
```

---

## Sistemler

### 1. Üs Yönetimi (Base Building)

#### 1.1 Bina Tipleri

| Bina | İşlev | Yapı Maliyeti |
|------|-------|---------------|
| **Komuta Merkezi** | Üssün kalbi, seviyesi diğer binaların limitini belirler | — (başlangıç) |
| **Barınak** | Nüfus kapasitesi artırır | Odun + Metal |
| **Kaynak Deposu** | Kaynak saklama kapasitesi | Metal |
| **Çiftlik** | Yiyecek üretir (zamanlayıcı ile) | Odun + Metal |
| **Su Kuyusu** | Su üretir | Metal |
| **Odun Fabrikası** | Odun üretir | Metal |
| **Maden** | Metal üretir | Odun |
| **Atölye** | Ekipman üretir (silah, zırh) | Metal + Tekno Parça |
| **Araştırma Lab** | Teknoloji ağacı, upgrade açar | Metal + Yiyecek |
| **Kışla** | Asker yetiştirir, kapasite artırır | Yiyecek + Metal |
| **Surlar** | Savunma, hasar azaltma | Metal + Odun |
| **Gözlem Kulesi** | Savaş uyarı, görüş alanı | Metal |
| **Ticaret Merkezi** | NPC faction ile ticaret | Odun + Metal |
| **Hastane** | Yaralı asker iyileştirme | Yiyecek + Su |
| **Jeneratör** | Enerji üretir | Metal + Tekno Parça |

#### 1.2 Bina Mekanikleri

- **Grid sistemi**: Binalar grid tabanlı yerleştirilir (her bina 1x1 veya 2x2 grid)
- **Yerleştirme**: Sürükle-bırak ile grid'e yapışır, yeşil/kırmızı validasyon
- **Yükseltme**: Her bina maksimum 10 seviye (Command Center limitini aşamaz)
- **Süreçler**: Üretim, yükseltme, eğitim — gerçek zamanlı zamanlayıcı
- **Yıkma**: Bina yıkılabilir, %50 kaynak iadesi
- **Rotasyon**: Bina yerleştirilirken R tuşu ile döndürme

#### 1.3 Kaynaklar

| Kaynak | Simgesi | Kaynak |
|--------|---------|--------|
| **Odun** | 🪵 | Odun Fabrikası, dünya haritası |
| **Metal** | ⚙️ | Maden, dünya haritası |
| **Yiyecek** | 🌾 | Çiftlik, dünya haritası |
| **Su** | 💧 | Su Kuyusu, dünya haritası |
| **Tekno Parça** | 🔩 | Harita loot, savaş ganimeti |
| **Enerji** | ⚡ | Jeneratör |

---

### 2. Askeri Sistem

#### 2.1 Birlik Tipleri

| Birlik | Tip | Güçlü Olduğu | Zayıf Olduğu |
|--------|-----|---------------|---------------|
| **Piyade** | Dengeli | — | — |
| **Nişancı** | Uzun menzilli | Uzaktan hasar | Yakın dövüş |
| **Ağır Asker** | Tank | Yüksek HP, hasar absorb | Hareket yavaş |
| **Gözcü** | Keşif | Hızlı, gizli | Düşük hasar |
| **Mühendis** | Destek | Tamir, tuzak kurma | Savaş zayıf |

#### 2.2 Ordu Mekanikleri

- **Kışla**: Asker eğitimi (zamanlayıcı, yiyecek + metal tükürür)
- **Ordu kompozisyonu**: Farklı birlik tiplerini karıştır, strateji belirle
- **Moral**: Yiyecek/su eksikliği morali düşürür → savaş gücü etkilenir
- **Kayıp**: Savaşta ölen askerler gerçekten kaybolur (Hastane ile kısmen kurtarılabilir)
- **Eğitim süresi**: Birlik tipine göre değişir (Piyade hızlı, Ağır Asker yavaş)

---

### 3. Dünya Haritası

#### 3.1 Harita Yapısı

```
┌──────────────────────────────────────────┐
│  ☢ Radyoaktif Bölge (High Level)         │
│  ┌────────────────────────────────────┐  │
│  │  Mutant Kampları                   │  │
│  │  ┌──────────────────────────────┐  │  │
│  │  │  NPC Yerleşimleri            │  │  │
│  │  │  ┌────────────────────────┐  │  │  │
│  │  │  │  ★ OYUNCU ÜSSÜ         │  │  │  │
│  │  │  │  (Başlangıç Noktası)   │  │  │  │
│  │  │  └────────────────────────┘  │  │  │
│  │  └──────────────────────────────┘  │  │
│  └────────────────────────────────────┘  │
└──────────────────────────────────────────┘
```

#### 3.2 Harita Noktaları

- **Kaynak Noktaları**: Odun, metal, yiyecek toplama alanları
- **Mutant Kampları**: Düşman NPCs, ganimet, temizlenince bölge açılır
- **Terk Edilmiş Binalar**: Keşif + loot (rastgele içerik)
- **NPC Yerleşimleri**: Ticaret, görev, ittifak kurbilgisi
- **Radyoaktif Alanlar**: Yüksek risk / yüksek ödül, özel ekipman gerekli
- **Boss Bölgeleri**: Özel düşmanlar, nadir loot, tek seferlik

#### 3.3 Seyahat Sistemi

- Asker grupları dünyaya gönderilir (bekleme süresi = mesafe)
- Keşif: Fog of war, gidilen bölgeler açılır
- Sefer sırasında üste bina/yükseltme devam eder
- Sefer iptal edilebilir (%50 birlik geri döner)

---

### 4. Hero (Karakter) Sistemi

#### 4.1 Hero Özellikleri

- **Nadirlik**: Common → Uncommon → Rare → Epic → Legendary
- **Rol**: Komutan, Savaşçı, Nişancı, Mühendis, Gözcü
- **Seviye**: XP ile yükselir (görev + savaş)
- **Yetenekler**: Her hero'nun 1 pasif + 1 aktif yeteneği
- **Ekipman**: Silah + Zırh + Aksesuar slotları

#### 4.2 Hero Edinme

- **Görev ödülü**: Ana hikaye görevleri (garanti hero)
- **Çağrı (Summon)**: Kaynak karşılığı rastgele hero (Common ağırlıklı)
- **Harita keşfi**: Özel NPC'ler katılabilir (kurtarma görevi)
- **Etkinlik**: Zamanlı özel hero'lar (gelecek)

#### 4.3 Hero Savaş Etkisi

- Hero ordunun başında → orduya buff verir
- Yetenek seviyesi → ordu gücü çarpanı
- Ekipman → direkt stat bonus
- Hero ölmez (savaş kaybedince yaralanır, bekleme süresi)

---

### 5. Savaş Sistemi

#### 5.1 PvE Savaş

- **Otomatik savaş**: Ordular çarpışır, sonuç hesaplanır
- **Savaş raporu**: Detaylı sonuç (kayıplar, ganimet, hasar dağılımı)
- **Strateji**: Ordu kompozisyonu + hero seçimi savaş sonucunu belirler
- **Baskın**: Mutant kamplarına saldırı → ganimet
- **Savunma**: Mutant saldırılarını püskürt (surlar + garnizon)

#### 5.2 Savaş Formülü (Taslak)

```
Ordu Gücü = Σ(Birlik_Sayısı × Birlik_Base_Gücü × Tip_Matchup_Çarpanı) × Hero_Buff × Moral_Çarpanı
Savaş Sonucu = Karşılaştırma(Atak_Ordu_Gücü, Savunma_Ordu_Gücü)
Kayıp_Oranı = f(Güç_Farkı, Rastgele_Faktör)
```

---

### 6. Teknoloji Ağacı

```
Seviye 1                    Seviye 2                    Seviye 3
────────                    ────────                    ────────
[Temel İnşaat]──────►[Gelişmiş İnşaat]──────►[Savunma Yapıları]
       │                      │
       ▼                      ▼
[Temel Tarım]────────►[Verimli Tarım]──────►[Hidroponik Tarım]
       │
       ▼
[Temel Silah]────────►[Gelişmiş Silah]─────►[Ağır Silahlar]
       │                      │
       ▼                      ▼
[Temel Tıp]──────────►[Gelişmiş Tıp]───────►[Radyasyon İlacı]
       │
       ▼
[Temel Keşif]────────►[Gelişmiş Keşif]────►[Radyoaktif Geçiş]
```

- Araştırma Lab'da yapılır
- Süreç zaman alır (gerçek zamanlı)
- Her seviye önceki seviyeye bağlı (prerequisite)
- Araştırma sırasında başka araştırma başlatılamaz (ilk versiyon)

---

### 7. Quest & İlerleme

#### 7.1 Görev Tipleri

| Tip | Açıklama | Örnek |
|-----|----------|-------|
| **Ana Görev** | Hikaye ilerlemesi, yeni mekanik açar | "İlk çiftliği kur" |
| **Yan Görev** | Ekstra ödül | "50 odun topla" |
| **Günlük** | Her gün yenilenir | "3 mutant kampı temizle" |
| **Zamanlı** | Etkinlik görevleri | "7 gün içinde 100 asker eğit" |

#### 7.2 İlerleme Hissi

- Command Center seviyesi = oyuncu genel ilerlemesi
- Her CC seviyesi yeni bina/mekanik açar
- Milestone ödülleri (CC lvl 3 → ücretsiz hero, CC lvl 5 → ticaret açılır)

---

### 8. UI/UX Tasarımı

#### 8.1 Ana Ekran (Üs Görünümü)

```
┌──────────────────────────────────────────────────────┐
│ [Odun:450] [Metal:280] [Yemek:150] [Su:100] [Enerji] │ ← Kaynak Bar
│ [Seviye: 3] [Nüfus: 12/20]                           │
│                                                       │
│                                                       │
│           [3D LOW POLY ÜS GÖRÜNÜMÜ]                  │
│        (Açılı Kamera ~45°, Döndürülebilir)           │
│                                                       │
│ ┌──────┐                                    ┌──────┐ │
│ │Mini  │                                    │Görev │ │
│ │Harita│                                    │Bild. │ │
│ └──────┘                                    └──────┘ │
│ ┌──────────────────────────────────────────────────┐ │
│ │[Yapı] [Araştır] [Ordu] [Hero] [Görev] [Harita] │ │ ← Aksiyon Bar
│ └──────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────┘
```

#### 8.2 HUD Elementleri

- **Üst bar**: Kaynaklar + seviye + nüfus
- **Sol alt**: Mini harita (tıklanabilir)
- **Sağ üst**: Görev bildirimleri (toast)
- **Alt merkez**: Aksiyon butonları (kategori tabs)
- **Merkez**: Bina yerleştirme grid overlay (inşaat modu)
- **ESC**: Ayarlar/Pause menüsü

---

### 9. Kaynak Dengeleme (Taslak)

#### 9.1 Üretim Oranları (Seviye 1 bina)

| Bina | Üretim | Süre |
|------|--------|------|
| Çiftlik | 10 yiyecek | 5 dk |
| Su Kuyusu | 8 su | 5 dk |
| Odun Fabrikası | 12 odun | 5 dk |
| Maden | 8 metal | 5 dk |
| Jeneratör | 5 enerji | 5 dk |

#### 9.2 Bina Maliyetleri (Seviye 1 → Seviye 2)

| Bina | Odun | Metal | Yiyecek | Su | Süre |
|------|------|-------|---------|-----|------|
| Barınak | 50 | 30 | — | — | 1 dk |
| Kışla | 80 | 60 | 20 | — | 2 dk |
| Atölye | 100 | 80 | — | — | 3 dk |
| Araştırma Lab | 120 | 100 | 30 | — | 5 dk |
| Surlar | 60 | 80 | — | — | 2 dk |

#### 9.3 Asker Eğitim Maliyeti

| Birlik | Yiyecek | Metal | Süre |
|--------|---------|-------|------|
| Piyade | 5 | 3 | 30 sn |
| Nişancı | 8 | 5 | 45 sn |
| Ağır Asker | 12 | 10 | 1 dk |
| Gözcü | 3 | 2 | 20 sn |
| Mühendis | 10 | 8 | 50 sn |

---

### 10. Kontroller (Taslak)

| Tuş | Eylem |
|-----|-------|
| WASD / Ok Tuşları | Kamera pan (hareket) |
| Fare Tekerleği | Zoom in/out |
| Orta Fare Tıklama + Sürükle | Kamera döndürme |
| Sol Tıklama | Seçim / Yerleştirme |
| Sağ Tıklama | İptal / Bilgi |
| R | Bina döndür (yerleştirme modu) |
| B | İnşaat menüsü aç/kapa |
| H | Hero paneli |
| M | Dünya haritası |
| Q | Quest günlüğü |
| ESC | Pause / Ayarlar |
| 1-6 | Kaynak bar hızlı erişim |

---

### 11. Ses & Müzik

| Alan | Tarz |
|------|------|
| **Müzik** | Atmosferik, post-apokaliptik ambient + distans davul |
| **Üs** | Rüzgar sesi, uzak patlama, inşaat sesleri |
| **Savaş** | Silah, patlama, çarpışma |
| **UI** | Tıklama, bildirim, seviye atlama fanfarı |
| **Hero** | Yetelik aktivasyon sesleri |

---

## Last War'dan Farklılaşma

| Özellik | Last War | Hollow Ground |
|---------|----------|---------------|
| Tema | Zombi kıyameti | Nükleer savaş sonrası |
| Çok oyunculu | Evet (MMO) | Hayır (Single-player PvE) |
| Görsel | Realistik | Low Poly stilize |
| Radyoaktivite | Yok | Evet (mekanik + bölge) |
| Teknoloji Ağacı | Basit | Derin, dallanan |
| Hero Sistemi | Gacha ağırlıklı | Keşif + görev odaklı |
| PvP | Evet | Hayır |
| İttifak | Gerçek oyuncular | NPC faction'lar |

---

*Son güncelleme: GDD v1.0 — Proje başlangıcı*
