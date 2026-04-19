# Hollow Ground — Dengeleme Referans Tablosu

## Bina Maliyetleri ve Uretim (Seviye 1)

| Bina | Odun | Metal | Yiyecek | Su | TechPart | Uretim | Sure | CC Req |
|------|------|-------|---------|-----|----------|--------|------|--------|
| CommandCenter | - | - | - | - | - | - | - | 0 |
| Farm | 50 | 30 | - | - | - | 10 Food | 5dk | 1 |
| WoodFactory | - | 40 | - | - | - | 12 Wood | 5dk | 1 |
| Mine | 50 | - | - | - | - | 8 Metal | 5dk | 1 |
| WaterWell | - | 30 | - | - | - | 8 Water | 5dk | 1 |
| Barracks | - | 60 | 80 | - | - | - | - | 1 |
| Shelter | 50 | 30 | - | - | - | +10 Pop | - | 1 |
| Storage | - | 60 | - | - | - | +500 Store | - | 1 |
| Generator | - | 80 | - | - | 10 | 5 Energy | 5dk | 1 |
| Workshop | - | 100 | - | - | 20 | - | - | 2 |
| ResearchLab | - | 120 | 30 | - | - | - | - | 3 |

## Maliyet Carpani (Seviye basina)

- CostMultiplier: **1.5x** (her seviye onceki seviyenin 1.5 kati)
- BuildTimeMultiplier: **1.3x**
- ProductionMultiplier: **1.2x** (her seviye %20 fazla uretim)

### Ornek: Farm Seviye 1-5

| Seviye | Maliyet (Odun/Metal) | Insaat Suresi | Uretim |
|--------|---------------------|---------------|--------|
| 1 | 50 / 30 | 10sn | 10 Food |
| 2 | 75 / 45 | 13sn | 12 Food |
| 3 | 113 / 68 | 17sn | 14 Food |
| 4 | 169 / 101 | 22sn | 17 Food |
| 5 | 253 / 152 | 29sn | 21 Food |

## Asker Egitim Maliyeti

| Birlik | Yiyecek | Metal | Egitim Suresi |
|--------|---------|-------|---------------|
| Infantry | 5 | 3 | 30sn |
| Sniper | 8 | 5 | 45sn |
| Heavy | 12 | 10 | 60sn |
| Scout | 3 | 2 | 20sn |
| Engineer | 10 | 8 | 50sn |

## Savas Gucu Referansi

| Birlik | Temel Guc | HP | Hiz |
|--------|-----------|-----|-----|
| Infantry | 10 | 100 | Orta |
| Sniper | 15 | 60 | Yavas |
| Heavy | 12 | 200 | Cok Yavas |
| Scout | 6 | 50 | Hizli |
| Engineer | 4 | 80 | Orta |

## Mutant Dalga Gucleri

| Dalga | Mutant Sayisi | Guc | Aralik |
|-------|---------------|-----|--------|
| 1 | 5 | 50 | 10dk |
| 2 | 8 | 60 | 11dk |
| 3 | 11 | 72 | 12dk |
| 5 | 17 | 104 | 15dk |
| 10 | 32 | 310 | 24dk |

## Hero Gacha Oranlari

| Rarity | Sans | Army Bonus |
|--------|------|------------|
| Common | 50% | Dusuk |
| Uncommon | 30% | Orta |
| Rare | 15% | Iyi |
| Epic | 4% | Yuksek |
| Legendary | 1% | Cok Yuksek |

## Faction Baslangic Iliskileri

| Faction | Puan | Iliski | Ozellik |
|---------|------|--------|---------|
| Scavenger Guild | 10 | Neutral | Kaynak ticareti |
| Iron Legion | -5 | Neutral (dusuk) | Askeri tech ticareti |
| Green Haven | 20 | Neutral (yuksek) | Yiyecek/su ticareti |

## Teknoloji Arastirma Maliyetleri

| Teknoloji | Metal | TechPart | Yiyecek | Odun | Sure |
|-----------|-------|----------|---------|------|------|
| Basic Construction | 50 | 5 | - | - | 2dk |
| Basic Agriculture | - | 5 | - | 40 | 1.5dk |
| Basic Weapons | 60 | 10 | - | - | 2.5dk |
| Basic Medicine | - | 8 | 50 | - | 2dk |
| Basic Exploration | - | 5 | - | 30 | 1.5dk |
| Advanced Construction | 150 | 15 | - | - | 5dk |
| Efficient Farming | - | 20 | 100 | - | 4dk |
| Advanced Weapons | 200 | 25 | - | - | 6.5dk |
| Radiation Treatment | - | 30 | 80 | - | 6dk |
| Radioactive Crossing | 100 | 40 | - | - | 6.5dk |

## Quest Odul Referansi

| Gorev Seviyesi | XP Odul | TechPart | Kaynak Odul |
|----------------|---------|----------|-------------|
| 1 (Tutorial) | 50 | 0-5 | Dusuk |
| 2 | 100 | 5-10 | Orta |
| 3 | 150-200 | 10-15 | Iyi |
| 4 | 250-300 | 15-25 | Yuksek |
| 5 | 500 | 25-50 | Cok Yuksek |

## Kaynak Baslangic Degerleri

| Kaynak | Baslangic | Kapasite |
|--------|-----------|----------|
| Wood | 200 | 500 |
| Metal | 100 | 500 |
| Food | 150 | 500 |
| Water | 80 | 500 |
| TechPart | 0 | 500 |
| Energy | 0 | 500 |

*Her Storage binasi +500 kapasite ekler (tum kaynaklar icin)*
