using System.Numerics;
using Raylib_cs;
using Zoo.defs;
using Zoo.util;

namespace Zoo.world; 

public static class TileUtility {
    public static Vector2 TileCentre(this IntVec2 tile) {
        return tile + new Vector2(0.5f, 0.5f);
    }
    public static bool InMap(this IntVec2 tile) {
        return Find.World.IsPositionInMap(tile);
    }
    public static bool IsWalkable(this IntVec2 tile, AccessibilityType accessibilityType) {
        return Find.World.IsTileWalkable(tile, accessibilityType);
    }
    public static Area? GetArea(this IntVec2 tile) {
        return Find.World.Areas.GetAreaAtTile(tile);
    }
    public static FootPath? GetFootPath(this IntVec2 tile) {
        return Find.World.FootPaths.GetFootPathAtTile(tile);
    }

    public static bool CanPlace(ObjectDef obj, IntVec2 tile) {
        for (int i = 0; i < obj.Size.X; i++) {
            for (int j = 0; j < obj.Size.Y; j++) {
                var t = (tile + new Vector2(i, j)).Floor();
                
                if (Find.World.GetTileObjectAtTile(t) != null) return false;
                if (Find.World.Elevation.IsTileWater(t)) return false;
                if (!Find.World.IsPositionInMap(t)) return false;
                if (Find.World.GetEntitiesAtTile(t).Any()) return false;
                
                foreach (var wall in Find.World.Walls.GetWallsSurroundingTile(t)) {
                    if (!wall.Exists) continue;
                    var placementBounds = new Rectangle(tile.X, tile.Y, obj.Size.X, obj.Size.Y);
                    if (placementBounds.ContractedBy(0.1f).Contains(wall.WorldPos)) return false;
                }
            }
        }

        return true;
    }
}