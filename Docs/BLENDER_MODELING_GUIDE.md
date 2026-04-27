# Hollow Ground — Blender Modeling Guide

## Technical Specs
- Unity 6 + URP, 3D Low Poly
- Grid cell: 2m x 2m
- Pivot: bottom center of model at Y=0
- FBX export: -Z forward, Y up, Apply Transform ON, Scale Factor 1.0
- Blender units: Metric, Meters
- Tri budget: 1x1=200-500(L1), 1x1=800-1200(L10), 2x2=400-800(L1), 2x2=1500-2500(L10)
- Flat shading, no ngons, edge split for hard edges
- Vertex color channels: R=corrosion, G=moss, B=dirt

## Building Sizes
| Building | Grid | Blender Bounds | baseH | step |
|----------|------|----------------|-------|------|
| CommandCenter | 2x2 | 3.9x3.9m | 4.0m | 0.45m |
| Farm | 2x2 | 3.9x3.9m | 2.0m | 0.2m |
| Mine | 2x2 | 3.9x3.9m | 3.0m | 0.3m |
| Barracks | 2x2 | 3.9x3.9m | 3.0m | 0.2m |
| Shelter | 2x2 | 3.9x3.9m | 3.0m | 0.3m |
| WoodFactory | 2x2 | 3.9x3.9m | 3.0m | 0.2m |
| WaterWell | 1x1 | 1.9x1.9m | 2.0m | 0.15m |
| Hospital | 1x1 | 1.9x1.9m | 3.0m | 0.2m |
| Generator | 1x1 | 1.9x1.9m | 2.5m | 0.15m |
| Storage | 1x1 | 1.9x1.9m | 2.5m | 0.25m |
| Walls | 1x1 | 1.9x0.4m | 2.0m | 0.2m |
| WatchTower | 1x1 | 1.9x1.9m | 5.0m | 0.7m |

## Color Palette
| Material | Hex | Roughness |
|----------|-----|-----------|
| Rusty Metal | #7A4631 | 0.7 |
| Dirty Concrete | #6B6B63 | 0.9 |
| Rotten Wood | #5C4023 | 0.85 |
| Dark Metal | #3A3A38 | 0.4 |
| Sand/Soil | #A89060 | 0.95 |
| Moss | #4A6B3A | 0.9 |
| Dirty Cloth | #7B7B6F | 0.95 |
| Warning Yellow | #C4A83A | 0.6 |
| Oxide Red | #8B3A2A | 0.75 |
| Dirty Glass | #4A6060 | 0.3 |

## Level Evolution Pattern
- L1: Basic, rusty, minimal detail, worn
- L3: +1 extension, patched roof, hanging cables
- L5: 2nd floor/wing, antenna, barrels, crates
- L10: Restored, reinforced concrete, solar panels, flag, lights

## State Models
- Construction: Scaffold frame + partial walls + debris
- Damaged: Roof 30% collapsed, holes, debris, scorch marks
- Destroyed: Only foundation + debris field + scorch

## File Structure
```
Assets/_Project/Models/Buildings/{BuildingName}/
  {Name}_L01.fbx, {Name}_L03.fbx, {Name}_L05.fbx, {Name}_L10.fbx
  {Name}_Construct.fbx, {Name}_Damaged.fbx, {Name}_Destroyed.fbx
```
