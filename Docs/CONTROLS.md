# Hollow Ground — Keyboard Shortcuts & Controls

## Mevcut Kontroller

### Kamera
| Tus | Islev |
|-----|-------|
| WASD / Ok tusleri | Kamera kaydirma |
| Mouse Scroll | Zoom in/out |
| Mouse Middle Drag | Pan |
| Mouse Right Drag | Rotasyon |
| Shift + Right Drag | Pan (alternatif) |
| Mouse Edge Scroll | Ekran kenarinda kaydirma |

### Zaman Kontrolu
| Tus | Islev |
|-----|-------|
| ESC | Pause menu / panel kapat |
| F1 | About panel |
| F5 | Quick Save |
| F9 | Quick Load |
| F12 | Debug HUD |

### UI Paneller (ActionBar)
| Tus | Islev |
|-----|-------|
| BuildMenu | Bina insaat menu |
| TechTree | Arastirma agaci |
| Training | Askeri egitim |
| Hero | Hero panel |
| Settler | Isci yonetimi |
| QuestLog | Gorevler |
| FactionTrade | Ticaret |
| WorldMap | Dunya haritasi |

---

## Eksik: Standart PC Strategy Kısayolları

RimWorld, They Are Billions, Age of Empires, Civilization, Cities: Skylines gibi
strategji oyunlarinda standart olan kisayollar. Hollow Ground'a eklenmeli.

### 1. Fullscreen / Window (ZORUNLU)

| Tus | Islev | Kaynak Oyunler |
|-----|-------|----------------|
| Alt + Enter | Fullscreen <-> Windowed toggle | Neredeyse tum PC oyunlari |
| F11 | Fullscreen toggle (alternatif) | RimWorld, bircok Unity oyunu |

**Unity implementasyonu:**
```csharp
// Screen.fullScreen = !Screen.fullScreen;
// veya
// Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
```

**Unity Player Settings:**
- `Fullscreen Mode`: `FullScreen Window` (borderless)
- `Resizable Window`: true
- `Allow fullscreen switch`: true (Alt+Enter otomatik)

### 2. Zaman Kontrolu (Yuksek Oncelikli)

| Tus | Islev | Kaynak |
|-----|-------|--------|
| Space | Pause / Resume toggle | RimWorld, They Are Billions, AoE |
| 1 | Normal hiz (1x) | RimWorld |
| 2 | Hizli (2x) | RimWorld |
| 3 | Cok hizli (3x) | RimWorld |
| + / - | Hiz artir / azalt | Civilization |

### 3. Kamera Kisayollari (Orta Oncelikli)

| Tus | Islev | Kaynak |
|-----|-------|--------|
| Home | Kamerayi merkez/map origin'e gotur | Cities: Skylines |
| End | Kamerayi CC'ye gotur (FocusOn CC) | Ozel |
| Page Up | Zoom in | RimWorld |
| Page Down | Zoom out | RimWorld |
| Backspace | Kamera rotasyon sifirla | Ozel |

### 4. Save / Load Kisayollari (ZORUNLU)

| Tus | Islev | Kaynak |
|-----|-------|--------|
| Ctrl + S | Manual save dialog ac | Universal PC standardi |
| Ctrl + L | Load dialog ac | Universal PC standardi |
| F5 | Quick Save (mevcut) | Universal |
| F9 | Quick Load (mevcut) | Universal |

### 5. Ekran Goruntusu (Orta Oncelikli)

| Tus | Islev | Kaynak |
|-----|-------|--------|
| F10 | Screenshot (Application.persistentDataPath) | They Are Billions |
| Print Screen | Screenshot | Windows standardi |

### 6. UI Kisayollari (Dusuk Oncelikli)

| Tus | Islev | Kaynak |
|-----|-------|--------|
| B | Build menu toggle | AoE, Cities: Skylines |
| R | Rotasyon (bina yerlestirme) | Universal strategy |
| Q / E | Rotasyon (alternatif) | RimWorld |
| Delete | Secili binayi yik (demolish) | Universal |
| Escape | Secimi iptal / panel kapat | Universal |

### 7. Pause Menu (ZORUNLU)

| Tus | Islev | Kaynak |
|-----|-------|--------|
| Escape | Pause menu ac/kapat | Universal |
| Escape | Bina secimini iptal | Universal |

---

## Oncelik Sirasi

1. **Alt+Enter fullscreen** — Player Settings + kod ile
2. **Space = pause/resume** — TimeManager.TogglePause()
3. **1/2/3 zaman hizi** — TimeManager speed control
4. **Ctrl+S / Ctrl+L** — Save/Load panel ac
5. **R rotasyon** — BuildingPlacer rotation
6. **Delete demolish** — Building seciliyse yik
7. **Home/End kamera** — Merkeze/CC'ye git
8. **F10 screenshot** — Screen capture

---

## Unity Player Settings (Edit > Project Settings > Player)

```
Resolution and Presentation:
  Fullscreen Mode: FullScreen Window
  Default Screen Width: 1920
  Default Screen Height: 1080
  Resizable Window: true
  Allow fullscreen switch: true     <-- Alt+Enter otomatik calisir

Other Settings:
  Force Single Instance: true       <-- Ikinci kopya acilmasin
  Run In Background: true           <-- Alt+Tab sonrasi devam etsin
```

## Notlar

- `Allow fullscreen switch = true` Unity'nin Alt+Enter'i otomatik handle etmesini saglar
- `Run In Background = true` ile Alt+Tab yapinca oyun durmaz ama pause yapilabilir
- RimWorld'de focus kaybinda `Mute on focus lost` secenegi var — ses kapatma opsiyonu dusunulebilir
- Strategy oyunlarinda fullscreen genelde `FullScreen Window` (borderless) kullanilir — daha hizli Alt+Tab
