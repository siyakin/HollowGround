# Blender MCP Prompt Series — Hollow Ground Buildings

> Her prompt'u kopyala-yapistir ile gonder. Sirayla git. Claude'un cevabini bekle, sonrakini gonder.
> **KRITIK:** Blender Z-up kullanir. Tum promptlarda Z=height, X/Y=ground plane.

## ILERLEME DURUMU — TAMAMLANDI

**TOPLAM: 15 bina x 7 model = 105 model tamamlandi**

| Bina | L01 | L03 | L05 | L10 | Construct | Damaged | Destroyed |
|------|-----|-----|-----|-----|-----------|---------|-----------|
| CommandCenter | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Farm | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Mine | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Barracks | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| WaterWell | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Generator | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| WoodFactory | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Hospital | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Storage | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Shelter | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Walls | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| WatchTower | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Workshop | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| ResearchLab | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| TradeCenter | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

### AI Model Degerlendirmesi
| Model | Basari | Not |
|-------|--------|-----|
| Claude (en iyi) | ⭐⭐⭐⭐⭐ | Acik ara en basarili, temiz is |
| Grok 4.3 | ⭐⭐⭐⭐ | Token bitirmiyor, parametrik iyi, komplekste zor |
| Gemini | ⭐⭐⭐ | Fena degil ama en iyi degil |
| Claude Sonnet | ⭐⭐⭐ | Z ekseninde hata, boolean yarida birakir |
| Walls | 1x1 | 1.9x0.4m | ❌ Bekliyor |
| WatchTower | 1x1 | 1.9x1.9m | ❌ Bekliyor |
| Workshop | 1x1 | 1.9x1.9m | ❌ Bekliyor |
| ResearchLab | 2x2 | 3.9x3.9m | ❌ Bekliyor |
| TradeCenter | 2x2 | 3.9x3.9m | ❌ Bekliyor |

### Dersler Cikarilanlar
1. **Z=UP sart**: Blender'da Z yukari, X/Y yatay. Prompt'larda her boyut (X)x(Z)x(Y) formatinda yazilmali
2. **Tek bina per sahne**: Birden fazla bina ayni sahnede olunca Sonet karisiyor. Her model ayri .blend dosyasinda
3. **Detayli koordinat sart**: "Duplicate and modify" degil, her parcanin tam X/Z/Y koordinatini ver
4. **Sonet Z-ekseni zayif**: Kompleks geometri yerlesiminde Z pozisyonlarinda hata yapiyor. Grok daha iyi
5. **Grok 4.3 basarili**: Token bitirmiyor, boolean islemleri yarida birakmiyor, pozisyonlama dogru
6. **Objeleri ayri tut**: Merge etme, ayri named objects olsun. Pozisyon hatasi olursa elle duzeltmek kolay
7. **Materyal ayri prompt**: Geometri + vertex color bitince ayri prompt ile materyal ver, sonra export et
8. **Kaydet**: Her adimda .blend kaydet, MCP kopabilir

---

## PROMPT 0 — SESSION OPENER (Ilk mesaj)

```
We are creating 3D low-poly building models for a post-apocalyptic strategy game called Hollow Ground. Unity 6 + URP pipeline.

RULES (save these for the entire session):
- Blender units: Metric, Meters
- Z AXIS IS UP. Use Z for height, X and Y for ground plane. Bottom of every model at Z=0.
- Grid cell size: 2m x 2m in Unity
- 1x1 buildings: max 1.9m x 1.9m footprint (on X-Y plane)
- 2x2 buildings: max 3.9m x 3.9m footprint (on X-Y plane)
- Pivot/Origin: bottom center, Z=0 is ground level
- Flat shading only (no smooth shading anywhere)
- No ngons allowed - all quads or tris
- Tri budget: 2x2 L1=400-700, 1x1 L1=200-400
- Post-apocalyptic theme: rusty, dusty, warped, patched, decayed
- Each mesh object gets edge split modifier for hard edges
- Apply all transforms before export
- FBX export: -Z forward, Y up, Apply Transform enabled, Face smoothing
- Use vertex colors: R=rust/corrosion amount, G=moss, B=dirt/grime

When I say "build X", create the mesh in Blender using bpy commands. Do NOT add materials yet - I will ask for materials separately. Focus only on geometry and vertex colors.

Confirm you understand these rules and are ready.
```

---

## PROMPT 1 — CC LEVEL 1 (Geometri)

```
Delete everything in the scene. Start fresh.

Build Command Center Level 1. 2x2 building, 3.9m x 3.9m footprint, max height 4.0m.

CRITICAL: Blender Z axis is UP. Use Z for height, X and Y for ground plane. Bottom of model at Z=0.

Build all parts, then join into ONE object named "CC_L01". Apply transforms BEFORE joining. After joining, apply Edge Split modifier, set origin to bottom center at Z=0, apply all transforms again.

Parts (all dimensions in meters):

1. FOUNDATION SLAB
   - Box: 3.9 x 0.15 x 3.9 (X width, Z height, Y depth)
   - Bottom face at Z=0, so center at Z=0.075
   - Bevel edges 0.04m
   - Push 3-4 top vertices down 0.01m for worn look

2. LEFT SHIPPING CONTAINER
   - Box: 1.8 (X) x 2.4 (Z) x 3.7 (Y)
   - Bottom at Z=0.15, center X=-0.95
   - Front face (positive Y): boolean cut door 1.2 (X) x 2.0 (Z), centered, bottom at Z=0.15
   - Warp 3-4 side vertices inward 0.02-0.04m

3. RIGHT SHIPPING CONTAINER
   - Same size as left, center X=+0.95
   - Front face: boolean cut window 0.4 x 0.4m, center at Z=1.8
   - Rotate whole container 1 degree on X (slight tilt)

4. CONNECTING BRACKETS (x2)
   - Small L-brackets 0.08 x 0.15 x 0.15m bridging the gap between containers on top

5. CORRUGATED ROOF
   - Plane 4.0 x 4.0m on top (Z approx 2.55), give 0.04m thickness
   - Loop cuts every 0.15m along X, alternate Z offset +/-0.02m for ridges
   - Front-right corner sag 0.08m lower

6. ANTENNA MAST + DISH
   - Cylinder 0.025m radius x 1.5m tall (Z), on roof top-right corner
   - Half-sphere 0.2m radius at top, tilted 30 deg on X

7. ENTRANCE DETAILS
   - 2 pallets: boxes 0.8 x 0.08 x 1.0m at Z=0.04
   - 3 sandbags: cylinders 0.12m radius x 0.12m tall in a row

8. GENERATOR
   - Box 0.35 x 0.3 x 0.25m on right side ground level
   - Thin cylinder cable from generator to right container wall

JOIN ALL into "CC_L01". Edge Split modifier. Origin to bottom center Z=0. Apply all transforms.

Vertex colors (R=rust, G=moss, B=dirt):
- Foundation: 0.3, 0.3, 0.2
- Containers + brackets: 0.6, 0.1, 0.2
- Roof: 0.5, 0.1, 0.4
- Antenna: 0.2, 0.0, 0.1
- Pallets: 0.2, 0.2, 0.1
- Sandbags: 0.1, 0.1, 0.3
- Generator: 0.3, 0.0, 0.2

Report tri count. Target: 500-700.
```

---

## PROMPT 2 — CC LEVEL 1 MATERYAL

```
Create these materials for CC_L01 and assign them by vertex color thresholds. Use Principled BSDF only, no textures:

1. "mat_concrete": Base Color #6B6B63, Roughness 0.9, Metallic 0.0
   Assign to: foundation slab (faces with R approx 0.3, G approx 0.3)

2. "mat_rusty_metal": Base Color #7A4631, Roughness 0.7, Metallic 0.3
   Assign to: containers, brackets, roof (faces with R approx 0.5-0.6)

3. "mat_dark_metal": Base Color #3A3A38, Roughness 0.4, Metallic 0.6
   Assign to: antenna mast and dish (faces with R approx 0.2, G approx 0.0, B approx 0.1)

4. "mat_rotten_wood": Base Color #5C4023, Roughness 0.85, Metallic 0.0
   Assign to: pallets (faces with R approx 0.2, G approx 0.2, B approx 0.1)

5. "mat_sand": Base Color #A89060, Roughness 0.95, Metallic 0.0
   Assign to: sandbags (faces with B approx 0.3, R approx 0.1)

6. "mat_generator": Base Color #4A4A48, Roughness 0.6, Metallic 0.4
   Assign to: generator box and cable (faces with R approx 0.3, G approx 0.0, B approx 0.2)

Create each material, then assign the correct faces using material slots. Report which material slot count you end up with.
```

---

## PROMPT 3 — CC LEVEL 1 EXPORT

```
Export CC_L01 as FBX:
- Select only the CC_L01 object
- File path: CC_L01.fbx
- Forward axis: -Z Forward
- Up axis: Y Up
- Apply Transform: ON
- Smoothing type: Face
- Only selected objects: ON
- Do NOT include cameras, lights, or armatures

After export, confirm file written and report final tri count and vertex count.
```

---

## PROMPT 4 — CC LEVEL 3

```
Duplicate CC_L01 and modify it to create CC_L03 (Command Center Level 3). Height target: 4.9m.

CRITICAL: Z axis is UP. All heights on Z axis.

Changes from L1:
1. Add a third shipping container on the right-back side, forming an L-shape:
   - Same container style, rotated 90 degrees on Z axis
   - Size: 1.8 (X) x 2.0 (Z) x 1.8 (Y) (shorter variant)
   - Connected to main structure with a covered walkway (0.3m wide bridge with tin roof)
2. Add a fabric awning over the entrance:
   - Angled plane 2.0 (X) x 1.5 (Y)m, supported by one metal pole (0.03m dia, 1.8m Z)
3. Enlarge the generator:
   - Replace small generator with diesel generator: 0.6 x 0.5 x 0.4m box
   - Add exhaust pipe: cylinder 0.03m radius, 0.7m tall (Z) on top
4. Add hanging cables:
   - 2-3 cables (thin cylinders, 0.01m radius) drooping from roof edges
5. Stack sandbags into a wall (2 rows high on Z, 6 bags total)
6. Add a warning sign: flat plane 0.3 (X) x 0.2 (Z)m on front, tilted slightly

Name it "CC_L03". Tri target: 800-1000. Apply vertex colors similar to L1 but more R (rust) on new parts.
```

---

## PROMPT 5 — CC LEVEL 5

```
Duplicate CC_L03 and modify to create CC_L05. Height target: 5.8m.

CRITICAL: Z axis is UP.

Changes from L3:
1. Add a watch tower on the top-left corner:
   - 4 metal legs: 0.05m square cross-section, 2.2m tall (Z)
   - Platform: 1.4 (X) x 1.4 (Y) x 0.05 (Z)m flat box with railings (0.05x0.05 cross-section, 0.7m Z tall)
   - Small pyramidal roof over platform (4 faces, 30 degree slope)
2. Replace the antenna with a larger Yagi-style communication antenna:
   - Central vertical bar: 0.02m dia, 1.2m tall (Z)
   - 5 horizontal elements: 0.015m dia, 0.35m wide, spaced 0.2m apart on Z
   - Mount on tower platform
3. Add a spotlight:
   - Cylinder 0.12m dia x 0.1m tall (Z) on tower railing
4. Add electrical panel on right wall:
   - Box 0.5 (X) x 0.7 (Z) x 0.08 (Y)m with many cables coming out (6+ thin cylinders)
5. Add 2 metal barrels near generator: cylinder 0.2m dia x 0.6m tall (Z) each

Name it "CC_L05". Tri target: 1200-1500.
```

---

## PROMPT 6 — CC LEVEL 10

```
Create CC_L10 from scratch (not a copy - major redesign). Height target: 8.1m. Footprint still 3.9x3.9m.

CRITICAL: Z axis is UP. All heights on Z axis.

This is a fully restored and fortified command fortress.

Geometry:
1. Reinforced concrete main body:
   - 3.9 (X) x 6.0 (Z) x 3.9 (Y)m box (2 floors, each 3m Z)
   - Floor divider at Z=3.0m (0.15m thick slab)
   - 4 windows per floor per side: 0.4 (X/Y) x 0.6 (Z)m rectangular cutouts
   - Main entrance: 1.2 (X) x 2.2 (Z)m opening on front, with heavy door frame
   - Edges slightly chamfered (0.03m bevel)
2. Second floor balcony on front:
   - 3.9 (X) x 0.6 (Y)m platform extending 0.6m from front wall at Z=3.0m
   - Metal railing: 0.03m square tubes, 0.8m tall (Z)
3. Radar dome on roof:
   - Half-sphere 0.5m radius on a 0.3m tall (Z) cylindrical base
   - Offset to right side of roof
4. Solar panel array:
   - 4 panels, each 0.7 (X) x 0.04 (Z) x 1.1 (Y)m, tilted 15 degrees
   - Metal frame around each: 0.02x0.02 square tube
   - Arranged in 2x2 grid on left side of roof
5. Communication tower:
   - Lattice tower: 4 legs tapering from 0.4m base to 0.15m top, 1.5m tall (Z)
   - Red aviation warning light: small sphere 0.05m at very top
6. Flag pole:
   - Cylinder 0.025m dia, 1.8m tall (Z) on right side of roof
   - Flag: 0.5 (X) x 0.35 (Z)m plane with 3 wavy vertex displacements
   - Torn edge on right side (missing triangles)
7. Guard posts at ground entrance:
   - 2 boxes 0.5 (X) x 0.9 (Z) x 0.5 (Y)m, one each side of entrance
   - Sandbag walls connecting them (6 cylinders)
8. Reinforced corners:
   - L-shaped metal plates at building corners: 0.25m wide, full height (Z), 0.02m thick

Name it "CC_L10". Tri target: 1800-2200.
Materials: reuse same material slots from CC_L01 + add:
- "mat_glass": #4A6060, Roughness 0.3, Metallic 0.1 (windows)
- "mat_solar": #1A1A2E, Roughness 0.2, Metallic 0.5 (solar panels)
- "mat_fabric": #8B0000, Roughness 0.95, Metallic 0.0 (flag)
```

---

## PROMPT 7 — CC CONSTRUCTION

```
Create CC_Construct - the construction scaffold model for Command Center.
Footprint: 3.9x3.9m, height: 3.0m (scaffold frame only).

CRITICAL: Z axis is UP.

Geometry:
1. 4 corner posts: 0.06 x 2.5 (Z) x 0.06m boxes
2. 4 mid-side posts: same size, centered on each wall
3. Horizontal beams at 3 levels (Z=0.5, Z=1.5, Z=2.5):
   - Connect all posts with 0.04x0.04 cross-section beams
4. Diagonal cross-braces on 2 sides:
   - Thin cylinders 0.015m radius, from bottom-left to top-right
5. Foundation platform: 3.9 x 0.08 (Z) x 3.9m box at Z=0.04
6. Partial wall on front-left (representing in-progress wall):
   - 1.5 (X) x 1.2 (Z) x 0.04 (Y)m plane, only covering bottom half of one section
7. Construction materials on ground:
   - Brick stack: 0.5 x 0.25 x 0.35m box (offset to right side)
   - 2 sand bags: cylinders 0.2m dia x 0.12m tall (Z)
   - Lumber pile: 3 thin boxes 0.8 (X) x 0.03 (Z) x 0.1 (Y)m, stacked
8. Temporary ladder:
   - 2 rails: 0.02 x 2.0 (Z) x 0.02m, 0.3m apart
   - 6 rungs: 0.01m dia cylinders every 0.3m on Z
   - Leaning against front at 70 degrees

Name it "CC_Construct". Tri target: 200-350.
Materials: "mat_dark_metal" for all metal, "mat_rotten_wood" for lumber.
```

---

## PROMPT 8 — CC DAMAGED

```
Duplicate CC_L01 and modify to create CC_Damaged (50% HP state).
Keep same footprint, show battle damage.

CRITICAL: Z axis is UP.

Changes from L1:
1. Roof: push right-front corner vertices down 0.5m on Z (collapsed section)
   - Add 2 broken beam pieces poking through (0.04m square, angled)
2. Left container front wall: delete a 0.8 (X) x 0.6 (Z)m section (shell hole)
   - Add jagged edges by moving surrounding vertices inward randomly
3. Right container: rotate the whole container 3 degrees on Z axis (shifted)
4. Antenna: break it - remove top half, bend remaining part 15 degrees
5. Generator: remove (destroyed) - leave only a scorch mark on ground
   - Scorch: dark circle on foundation (vertex color B=0.8, R=0.5)
6. Add debris on ground around building (8-10 pieces):
   - 3 metal scraps: thin warped planes 0.2-0.4m, various angles
   - 2 concrete chunks: rough cubes 0.1-0.2m
   - 2 wood splinters: thin elongated boxes
   - 1 sandbag burst: flattened cylinder
7. Scorch marks: blacken vertex colors on front wall around hole
8. Overall: increase R vertex color channel by 0.2 on everything (more rust/exposure)

Name it "CC_Damaged". Tri target: 600-800.
```

---

## PROMPT 9 — CC DESTROYED

```
Create CC_Destroyed - fully destroyed Command Center. Footprint: 3.9x3.9m.
This is a rubble pile, not a building.

CRITICAL: Z axis is UP.

Geometry:
1. Foundation slab (survived): same 3.9 x 0.15 (Z) x 3.9m box
   - Crack it: add loop cuts and offset vertices to create 2-3 large cracks
   - One corner broken off entirely (delete vertices)
2. One surviving wall section:
   - 1.8 (X) x 1.5 (Z) x 0.08 (Y)m plane, standing but tilted 8 degrees backward
   - Jagged top edge (vary vertex Z heights by 0.1-0.3m)
3. Debris field across entire 3.9x3.9m area (15-20 pieces):
   - 4 corrugated metal sheets: warped planes 0.3-0.6m, lying flat or angled
   - 3 concrete blocks: rough cubes 0.15-0.3m
   - 3 metal frame pieces: L-shaped or straight, 0.3-0.5m long
   - 2 wooden planks: thin boxes, partially buried in ground
   - 2 pipe segments: half-cylinders 0.3-0.5m long
   - 1 door frame remnant: rectangular frame, lying on side
4. Large scorch mark on foundation:
   - Dark vertex color circle 1.5m diameter, offset to center
   - R=0.5, B=0.3 vertex color
5. Ash pile: low mound 0.6m dia x 0.08m tall (Z) (smoothed hemisphere)

Name it "CC_Destroyed". Tri target: 300-500.
Materials: same as L01 but add "mat_scorch": #2A2A28, Roughness 1.0 for scorch area.
```

---

## PROMPT 10 — EXPORT ALL CC

```
Export all Command Center models. For each object, select it individually and export:

1. CC_L01 -> CC_L01.fbx
2. CC_L03 -> CC_L03.fbx
3. CC_L05 -> CC_L05.fbx
4. CC_L10 -> CC_L10.fbx
5. CC_Construct -> CC_Construct.fbx
6. CC_Damaged -> CC_Damaged.fbx
7. CC_Destroyed -> CC_Destroyed.fbx

Each export: -Z forward, Y up, Apply Transform ON, Face smoothing, Selected Objects only.
Report tri count for each.
```

---

## PROMPT 11+ — SONRAKI BINALAR (Tekrar eden pattern)

Farm icin:
```
Now clear the scene and build Farm Level 1.

CRITICAL: Z axis is UP. All heights on Z axis.

2x2 building (3.9x3.9m footprint on X-Y plane, max height 2.0m on Z).

Description: Open-air farm with wooden fence, tilled soil rows, and a small tool shed. Post-apocalyptic - scavenged tools, patched fence.

Geometry:
1. Ground plot: 3.9 x 0.05 (Z) x 3.9m, slightly raised
2. Wooden fence around perimeter:
   - Posts: 0.05 x 0.8 (Z) x 0.05m every 0.8m (10 posts per side)
   - Rails: 2 horizontal beams 0.03x0.03m between posts, at Z=0.25 and Z=0.55
   - Missing/broken: remove 1-2 posts on front, 1 rail section
3. 4 crop rows inside:
   - Raised soil beds: 0.4 (X) x 0.08 (Z) x 3.0 (Y)m boxes with wavy top surface
   - Small green sprouts: tiny cones 0.02m base x 0.06m tall (Z), 5-6 per row
4. Tool shed in back-right corner:
   - Box: 1.0 (X) x 1.2 (Z) x 1.0 (Y)m, tilted tin roof (one side higher on Z)
   - Door opening: 0.5 (X) x 0.9 (Z)m cutout
5. Scarecrow: thin cylinder post + crossbar + burlap sack head
   - Height: 1.5m (Z)
6. Watering cans: 2 small cylinder+cone shapes near shed
7. Crate: 0.3 x 0.25 x 0.3m box near crop rows

Name "Farm_L01". Tri target: 400-600.
Vertex colors: G=0.4 on soil beds (moss/growth), R=0.3 on fence (rust/decay), B=0.2 overall.
```

(Bu pattern'i her bina icin tekrarla - sadece aciklama ve detaylari degistir.)
