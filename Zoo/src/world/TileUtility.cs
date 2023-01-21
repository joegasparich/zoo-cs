﻿using System.Numerics;
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
}