using Zoo.util;

namespace Zoo.world; 

public static class FootPathUtility {
    public static (FootPathSpriteIndex, float) GetSpriteInfo(FootPath footPath) {
        var nw        = Find.World.Elevation.GetElevationAtPos(footPath.Pos);
        var ne        = Find.World.Elevation.GetElevationAtPos(new IntVec2(footPath.Pos.X + 1, footPath.Pos.Y));
        var sw        = Find.World.Elevation.GetElevationAtPos(new IntVec2(footPath.Pos.X, footPath.Pos.Y + 1));
        var se        = Find.World.Elevation.GetElevationAtPos(new IntVec2(footPath.Pos.X + 1, footPath.Pos.Y + 1));
        var elevation = JMath.Min(nw, ne, sw, se);

        if (nw.FEquals(ne) && nw.FEquals(sw) && nw.FEquals(se)) return (FootPathSpriteIndex.Flat, elevation);
        if (nw.FEquals(ne) && nw > sw && sw.FEquals(se)) return (FootPathSpriteIndex.HillNorth, elevation);
        if (nw.FEquals(ne) && nw < sw && sw.FEquals(se)) return (FootPathSpriteIndex.HillSouth, elevation);
        if (nw.FEquals(sw) && nw < ne && ne.FEquals(se)) return (FootPathSpriteIndex.HillEast, elevation);
        if (nw.FEquals(sw) && nw > ne && ne.FEquals(se)) return (FootPathSpriteIndex.HillWest, elevation);

        return (FootPathSpriteIndex.Flat, elevation);
    }
}