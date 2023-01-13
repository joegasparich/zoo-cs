using Zoo.util;

namespace Zoo.world; 

public static class LocationUtility {
    public static IntVec2 RandomCellInArea(Area area) {
        return area.Tiles.RandomElement();
    }
    
    private static List<IntVec2> walkableCells = new();
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
        walkableCells.Clear();
        foreach (var cell in area.Tiles) {
            if (cell.IsWalkable(accessibilityType)) {
                walkableCells.Add(cell);
            }
        }
        if (walkableCells.Count > 0) {
            return walkableCells.RandomElement();
        }
        
        // No walkable cells found
        return null;
    }
}