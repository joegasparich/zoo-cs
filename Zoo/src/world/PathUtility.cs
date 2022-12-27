using Zoo.util;

namespace Zoo.world; 

public static class PathUtility {
    public static (PathSpriteIndex, float) GetSpriteInfo(Path path) {
        var nw        = Find.World.Elevation.GetElevationAtPos(path.Pos);
        var ne        = Find.World.Elevation.GetElevationAtPos(new IntVec2(path.Pos.X + 1, path.Pos.Y));
        var sw        = Find.World.Elevation.GetElevationAtPos(new IntVec2(path.Pos.X, path.Pos.Y + 1));
        var se        = Find.World.Elevation.GetElevationAtPos(new IntVec2(path.Pos.X + 1, path.Pos.Y + 1));
        var elevation = JMath.Min(nw, ne, sw, se);

        if (nw.FEquals(ne) && nw.FEquals(sw) && nw.FEquals(se)) return (PathSpriteIndex.Flat, elevation);
        if (nw.FEquals(ne) && nw > sw && sw.FEquals(se)) return (PathSpriteIndex.HillNorth, elevation);
        if (nw.FEquals(ne) && nw < sw && sw.FEquals(se)) return (PathSpriteIndex.HillSouth, elevation);
        if (nw.FEquals(sw) && nw < ne && ne.FEquals(se)) return (PathSpriteIndex.HillEast, elevation);
        if (nw.FEquals(sw) && nw > ne && ne.FEquals(se)) return (PathSpriteIndex.HillWest, elevation);

        return (PathSpriteIndex.Flat, elevation);
    }
}