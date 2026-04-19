# Hollow Ground — Playtest Faz 11: Sistem Test Oturumu

## Baglam

Hollow Ground, Last War ilhamli nukleer savas sonrasi strateji oyunu. Unity 6 + URP + 3D Low Poly.
Faz 1-10 tamamlandi, Faz 11 playtest asamasinda. Bina yerlestirme duzeltildi, camera yeniden yazildi.
**Bu oturumun amaci:** tum sistemleri sirayla test edip calisir durumda oldugundan emin olmak, bozuk/eksik baglantilari bulup duzeltmek.

## On Kosullar (Sahne Durumu)

- Ground plane: pos=(50,0,50) scale=20x20, Layer=Ground (8)
- CameraRig > Main Camera: tag=MainCamera, _cam serialized ref bagli
- GameManager objesi uzerinde: tum manager'lar (19 component)
- GameCanvas altinda: tum UI panelleri, UIManager baglantılari
- StrategyCamera bounds: (-30,-30) to (130,130), initialZoom=35
- BuildingPlacer _groundMask = Ground layer
- CommandCenter SO: CommandCenterLevelRequired=0

## Kritik Kurallar

- Tum script dosyalari, SO dosyalari Ingilizce
- Dosya adlarinda Turkce karakter YASAK
- Kod yorumu eklenmez
- `FindObjectOfType`/`FindFirstObjectByType` YASAK → `FindAnyObjectByType`
- `FindObjectsByType(FindObjectsSortMode)` YASAK → `FindObjectsByType<T>()`
- Eski Input System YASAK → `Mouse.current`, `Keyboard.current`
- Dictionary serialize olmaz → `List<Entry>` pattern
- `UnityEngine.Resources` ve `UnityEngine.Camera` icin tam nitelik kullan
- Manager'lar Singleton, sistemler event ile haberlesir

## Test Plani (Sirayla Uygula)

### Test 1: Oyun Baslangic
- [ ] Play'e basla → konsolda hata yok
- [ ] GameManager state = Playing
- [ ] ResourceBarUI kaynaklari gosteriyor (Wood:200, Metal:100, Food:150, Water:80)
- [ ] Kamera grid merkezinde (50,0,50) bakis acisiyla
- [ ] WASD/Ok tuslari calisiyor, scroll zoom calisiyor
- [ ] Sag tik + surukle kamera dondurme calisiyor

### Test 2: Bina Yerlestirme
- [ ] ActionBar'dan Yapi butonuna tikla → BuildMenu aciliyor
- [ ] CommandCenter karti gorunur, maliyet gosteriyor, interaktif
- [ ] CommandCenter sec → ghost preview gorunuyor, grid'e snap oluyor
- [ ] Gecerli konumda yesil, gecersizde kirmizi material
- [ ] Sol tik ile yerlestir → kaynaklar dustu, bina sahneye eklendi
- [ ] R ile dondurme calisiyor
- [ ] Sag tik / ESC ile iptal calisiyor
- [ ] Farm, Mine, WaterWell gibi diger binalari da dene
- [ ] Ayni alana tekrar yerlestirme engelleniyor (occupied cell)

### Test 3: Kaynak Uretim
- [ ] Farm yerlestir → inshaat asamasinda (Constructing state)
- [ ] Inshaat tamamlandiginda state → Active
- [ ] Active Farm uretim tick'i basliyor (ProductionInterval gecince Food artiyor)
- [ ] ResourceBarUI'da Food degeri artis gosteriyor
- [ ] StorageCapacity sistemidepo siniri calisiyor

### Test 4: UI Paneller
- [ ] BuildMenu acilip kapaniyor
- [ ] BuildingInfoUI → bina secince aciliyor, seviye/durum/uretim gosteriyor
- [ ] Upgrade butonu calisiyor (kaynak varsa)
- [ ] Demolish butonu calisiyor (bina siliniyor, %50 iade)
- [ ] ToastUI bildirimler gorunuyor
- [ ] Tum ActionBar butonlari calisiyor (Yapi, Ordu, Hero, Gorev, Harita)

### Test 5: Askeri Sistem
- [ ] TrainingPanelUI aciliyor
- [ ] Birlik sec → egitim kuyruguna ekleniyor
- [ ] Egitim tamamlandiginda orduya ekleniyor
- [ ] ArmyPanelUI ordu kompozisyonunu gosteriyor
- [ ] Kaynak yetersizse egitim baslamiyor

### Test 6: Hero Sistemi
- [ ] HeroPanelUI aciliyor
- [ ] Summon butonu calisiyor (TechPart harciyor, hero ekliyor)
- [ ] Hero detay gosterimi (seviye, statlar, ekipman)
- [ ] Ekipman slotlari calisiyor

### Test 7: Donya Haritasi
- [ ] WorldMapPanel aciliyor
- [ ] 10x10 grid node'lari gorunuyor
- [ ] Fog of war calisiyor (komsu node'lar aciliyor)
- [ ] Node sec → info panel gosteriyor
- [ ] Sefer gonder → ilerleme basliyor, sure bitince sonuc donuyor

### Test 8: Arastirma / Tech Tree
- [ ] TechTreePanel aciliyor
- [ ] Tech kartlari gorunuyor
- [ ] Research butonu calisiyor, ilerleme basliyor
- [ ] Tamamlaninca bonus aktif oluyor

### Test 9: Faction Ticaret
- [ ] FactionTradePanel aciliyor
- [ ] Faction listesi gorunuyor
- [ ] Faction sec → al/sat teklifleri gosteriliyor
- [ ] Ticaret yapilabiliyor (kaynak degisimi)

### Test 10: Gorev Sistemi
- [ ] QuestLogPanel aciliyor
- [ ] Aktif/mevcut/tamamlanmis sekmeleri calisiyor
- [ ] Quest kabul → aktif listeye ekleniyor
- [ ] Quest ilerleme takip ediliyor
- [ ] Turn-in → odul veriliyor

### Test 11: Mutant Saldirisi
- [ ] MutantAttackManager zamanli saldiri baslatiyor
- [ ] Uyari bildirimi geliyor
- [ ] Savunma hesabi yapiliyor (ordu gucu vs dalga gucu)
- [ ] Sonuc: basari/terk, odul/ceza

### Test 12: Save/Load
- [ ] SaveMenuPanel aciliyor
- [ ] Yeni kayit olusturuluyor
- [ ] Kayit yukleniyor, oyun durumu dogru restore ediliyor
- [ ] Kayit silme calisiyor
- [ ] Auto-save (5dk) calisiyor

### Test 13: Zaman Kontrolu
- [ ] Pause calisiyor (PausePanel aciliyor)
- [ ] Hiz degistirme (1x, 2x, 3x) calisiyor
- [ ] TimeManager ile uretim hizi dogru olculuyor

## Bilinen Eksikler (Testte Kesfedilirse Not Al)

- TrainingPanelUI'de Turkce stringler var: "Yetersiz kaynak!", "egitimi basladi"
- WorldMap.GenerateDefaultMap() runtime'da SO olusturuyor — save/load ile uyumsuz olabilir
- GridVisualizer z-fighting sorunu olabilir
- BaseStarter.SetupBase() manuel tetiklenmeli (ContextMenu)
- 10 yedek bina SO silinmeli (ScriptableObjects/Buildings/ altinda gereksiz olanlar)
- TroopData SO'lar henutz olusturulmadi (TroopDataFactory ile editor'de yapilmali)
- HeroData SO'lar henutz olusturulmadi
- TechNode, Faction, Quest SO'lari henutz olusturulmadi (factory mevcut)

## Her Test Icin Not Al

Her test adami icin sonucu yaz:
- **GECTI** → calisiyor, sorun yok
- **HATA** → hata mesaji, hangi dosyada, ne bekleniyor ne oluyor
- **KISMEN** → calisiyor ama eksik/kotu UX var, acikla

Son olarak tum hatalari oncelik sirasina gore listele ve duzelt.
