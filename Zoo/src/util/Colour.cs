using System.Numerics;
using Raylib_cs;

namespace Zoo.util; 

public static class Colour {
    public static readonly Color Blueprint = Color.WHITE.WithAlpha(0.5f);

    public static Color IntToColour(int number) {
        return new Color(
            (number >>  0) & 255,
            (number >>  8) & 255,
            (number >> 16) & 255,
            255
        );
    }

    public static int ColourToInt(Color colour) {
        return ( colour.r << 0 ) | ( colour.g << 8 ) | ( colour.b << 16 );
    }
    
    public static Vector3 ToVector3(this Color color) {
        return new Vector3(color.r / 255f, color.g / 255f, color.b / 255f);
    }

    public static Color FromVector3(Vector3 vec) {
        return new Color((byte)(vec.X * 255), (byte)(vec.Y * 255), (byte)(vec.Z * 255), (byte)255);
    }
    
    public static Color Brighten(this Color color, float amount) {
        var (h, s, v) = Raylib.ColorToHSV(color);
        v = JMath.Clamp(v + amount, 0.0f, 1.0f);
        return Raylib.ColorFromHSV(h, s, v);
    }

    public static Color WithAlpha(this Color color, float alpha) {
        return new Color(color.r, color.g, color.b, (byte)(alpha * 255));
    }
}