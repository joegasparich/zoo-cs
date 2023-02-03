using Zoo.util;

namespace Zoo.world; 

public static class LocationUtility {
    private static List<IntVec2> cellList = new();

    public static IntVec2 RandomCellInRadius(IntVec2 rootTile, float radius) {
        var r     = radius       * MathF.Sqrt(Rand.Float());
        var theta = Rand.Float() * 2 * MathF.PI;
        var x     = rootTile.X + r * MathF.Cos(theta);
        var y     = rootTile.Y + r * MathF.Sin(theta);
        return new IntVec2(x.FloorToInt(), y.FloorToInt());
    }
    
    public static IntVec2 RandomCellInArea(Area area) {
        return area.Tiles.RandomElement();
    }
    
    public static IntVec2? RandomWalkableCellInArea(Area area, AccessibilityType accessibilityType) {
        // First try randomly picking some cells
        var tries = 5;
        while (tries > 0) {
            var cell = RandomCellInArea(area);
            if (cell.IsWalkable(accessibilityType)) {
                return cell;
            }
            tries--;
        }
        
        // Otherwise get list of walkable cells and pick one randomly
        cellList.Clear();
        foreach (var cell in area.Tiles) {
            if (cell.IsWalkable(accessibilityType)) {
                cellList.Add(cell);
            }
        }
        if (cellList.Count > 0) {
            return cellList.RandomElement();
        }
        
        // No walkable cells found
        return null;
    }

    public static IntVec2? RandomWalkableCellInAreaInRadius(IntVec2 rootTile, float radius, Area area, AccessibilityType accessibilityType) {
        // Get list of walkable cells and pick one randomly
        cellList.Clear();
        for (var i = -radius; i < radius; i++) {
            for (var j = -radius; j < radius; j++) {
                if ((i * i) + (j * j) < radius * radius) {
                    var tile = rootTile + new IntVec2(i.FloorToInt(), j.FloorToInt());
                    if (!tile.InMap()) continue;
                    if (tile.GetArea() != area) continue;
                    if (!tile.IsWalkable(accessibilityType)) continue;
                    
                    cellList.Add(tile);
                } 
            }
        }
        
        if (cellList.Count > 0) {
            return cellList.RandomElement();
        }
        
        // No walkable cells found in area
        return null;
    }

    public static IntVec2? RandomAccessibleCellInRadius(IntVec2 rootTile, float radius, HashSet<Area> accessibleAreas, AccessibilityType accessibilityType) {
        // First try randomly picking some cells
        var tries = 5;
        while (tries > 0) {
            var cell = RandomCellInRadius(rootTile, radius);
            if (accessibleAreas.Contains(cell.GetArea()))
                return cell;
            tries--;
        }
        
        // Otherwise get list of accessible cells in radius and pick one randomly
        cellList.Clear();
        for (var i = -radius; i < radius; i++) {
            for (var j = -radius; j < radius; j++) {
                if ((i * i) + (j * j) < radius * radius) {
                    var tile = rootTile + new IntVec2(i.FloorToInt(), j.FloorToInt());
                    if (!tile.InMap()) continue;
                    if (!accessibleAreas.Contains(tile.GetArea())) continue;
                    
                    cellList.Add(tile);
                } 
            }
        }
        
        if (cellList.Count > 0) {
            return cellList.RandomElement();
        }
        
        // No valid cells found
        return null;
    }
}