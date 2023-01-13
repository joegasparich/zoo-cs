using Zoo.util;

namespace Zoo.world; 

public static class LocationUtility {
    private static List<IntVec2> cellList = new();
    
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

    public static IntVec2? RandomWalkableCellInAreaInRadius(Area area, IntVec2 rootTile, int radius, AccessibilityType accessibilityType) {
        // Get list of walkable cells and pick one randomly
        cellList.Clear();
        for (var i = -radius; i < radius; i++) {
            for (var j = -radius; j < radius; j++) {
                if ((i * i) + (j * j) < radius * radius) {
                    var tile = rootTile + new IntVec2(i, j);
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
}