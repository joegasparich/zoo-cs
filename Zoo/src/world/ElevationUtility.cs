using System.Numerics;

namespace Zoo.world; 

public static class ElevationUtility {
    public const float ElevationHeight = 0.5f;
    
    public static float GetSlopeElevation(SlopeVariant slope, Vector2 pos) {
        var relX = pos.X % 1f;
        var relY = pos.Y % 1f;
        
        // Tried to come up with a formula instead of using enums but I'm too dumb
        switch (slope) {
            case SlopeVariant.Flat:
                return 0;
            case SlopeVariant.N:
                return ElevationHeight * relY;
            case SlopeVariant.S:
                return ElevationHeight * (1 - relY);
            case SlopeVariant.W:
                return ElevationHeight * relX;
            case SlopeVariant.E:
                return ElevationHeight * (1 - relX);
            case SlopeVariant.SE:
                return ElevationHeight * MathF.Max(1 - relX - relY, 0.0f);
            case SlopeVariant.SW:
                return ElevationHeight * MathF.Max(1 - (1 - relX) - relY, 0.0f);
            case SlopeVariant.NE:
                return ElevationHeight * MathF.Max(1 - relX - (1 - relY), 0.0f);
            case SlopeVariant.NW:
                return ElevationHeight * MathF.Max(1 - (1 - relX) - (1 - relY), 0.0f);
            case SlopeVariant.ISE:
                return ElevationHeight * MathF.Min(1 - relX + 1 - relY, 1.0f);
            case SlopeVariant.ISW:
                return ElevationHeight * MathF.Min(relX + 1 - relY, 1.0f);
            case SlopeVariant.INE:
                return ElevationHeight * MathF.Min(1 - relX + relY, 1.0f);
            case SlopeVariant.INW:
                return ElevationHeight * MathF.Min(relX + relY, 1.0f);
            case SlopeVariant.I1:
                return ElevationHeight * MathF.Max(1 - relX - relY, 1 - (1 - relX) - (1 - relY));
            case SlopeVariant.I2:
                return ElevationHeight * MathF.Max(1 - (1 - relX) - relY, 1 - relX - (1 - relY));
            default:
                // You shouldn't be here
                return 0;
        }
    }

    public static SlopeVariant GetSlopeVariant(Elevation nw, Elevation ne, Elevation sw, Elevation se) {
        if (nw == ne && nw == sw && nw == se) return SlopeVariant.Flat;
        if (nw == ne && sw == se && nw < sw) return SlopeVariant.N;
        if (nw == sw && ne == se && nw > ne) return SlopeVariant.E;
        if (nw == ne && sw == se && nw > sw) return SlopeVariant.S;
        if (nw == sw && ne == se && nw < ne) return SlopeVariant.W;
        if (se == sw && se == ne && nw > se) return SlopeVariant.SE;
        if (sw == nw && sw == se && ne > sw) return SlopeVariant.SW;
        if (ne == nw && ne == se && sw > ne) return SlopeVariant.NE;
        if (nw == sw && nw == ne && se > nw) return SlopeVariant.NW;
        if (se == sw && se == ne && nw < se) return SlopeVariant.INW;
        if (sw == nw && sw == se && ne < sw) return SlopeVariant.INE;
        if (ne == nw && ne == se && sw < ne) return SlopeVariant.ISW;
        if (nw == sw && nw == ne && se < nw) return SlopeVariant.ISE;
        if (nw == se && sw == ne && nw > ne) return SlopeVariant.I1;
        if (nw == se && sw == ne && nw < ne) return SlopeVariant.I2;

        // How did you get here?
        return SlopeVariant.Flat;
    }

    public static float GetSlopeVariantColourOffset(SlopeVariant slope, Vector2 pos, Side quadrant) {
        float F  = 0.0f;
        float NW = 1.0f,   N = 0.75f, W = 0.5f,   NE = 0.25f;
        float SW = -0.25f, E = -0.5f, S = -0.75f, SE = -1.0f;
        
        var xRel = pos.X / BiomeGrid.BiomeScale % 1;
        var yRel = pos.Y / BiomeGrid.BiomeScale % 1;

        switch (slope) {
            case SlopeVariant.N:
                return N;
            case SlopeVariant.S:
                return S;
            case SlopeVariant.W:
                return W;
            case SlopeVariant.E:
                return E;
            case SlopeVariant.NW:
                return xRel + yRel > 0.5 || (xRel + yRel == 0.5 && (quadrant == Side.South || quadrant == Side.East))
                   ? NW
                   : F;
            case SlopeVariant.NE:
                return xRel - yRel < 0 || (xRel - yRel == 0 && (quadrant == Side.South || quadrant == Side.West))
                   ? NE
                   : F;
            case SlopeVariant.SW:
                return yRel - xRel < 0 || (yRel - xRel == 0 && (quadrant == Side.North || quadrant == Side.East))
                   ? SW
                   : F;
            case SlopeVariant.SE:
                return xRel + yRel < 0.5 || (xRel + yRel == 0.5 && (quadrant == Side.North || quadrant == Side.West))
                   ? SE
                   : F;
            case SlopeVariant.INW:
                if (xRel + yRel > 0.5f) return F;
                return (xRel + yRel == 0.5f && (quadrant == Side.South || quadrant == Side.East)) ? F : NW;
            case SlopeVariant.INE:
                if (yRel - xRel >= 0.5f) return F;
                return (yRel - xRel == 0.0f && (quadrant == Side.South || quadrant == Side.West)) ? F : NE;
            case SlopeVariant.ISW:
                if (xRel - yRel >= 0.5f) return F;
                return (xRel - yRel == 0.0f && (quadrant == Side.North || quadrant == Side.East)) ? F : SW;
            case SlopeVariant.ISE:
                if (xRel + yRel < 0.5f) return F;
                return (xRel + yRel == 0.5f && (quadrant == Side.North || quadrant == Side.West)) ? F : SE;
            case SlopeVariant.I1:
                return xRel + yRel > 0.5 || (xRel + yRel == 0.5 && (quadrant == Side.South || quadrant == Side.East))
                   ? NW
                   : SE;
            case SlopeVariant.I2:
                return xRel - yRel < 0 || (xRel - yRel == 0 && (quadrant == Side.South || quadrant == Side.West))
                   ? NE
                   : SW;
            case SlopeVariant.Flat:
            default:
                return F;
        }
    }
}