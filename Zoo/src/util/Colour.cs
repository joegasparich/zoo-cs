using System.Numerics;
using Raylib_cs;

namespace Zoo.util; 

public static class Colour {
    public static Vector3 Rgb2Hsv(Vector3 rgb) {
        var   hsv = new Vector3();

        var min = JMath.Min(rgb.X, rgb.Y, rgb.Z);
        var max = JMath.Max(rgb.X, rgb.Y, rgb.Z);
        
        hsv.Z = max;
        var delta = max - min;
        if (delta < 0.00001) {
            hsv.Y = 0;
            hsv.X = 0;
            return hsv;
        }
        if (max > 0) {
            hsv.Y = delta / max;
        } else {
            hsv.Y = 0;
            hsv.X = 0;
            return hsv;
        }
        if (rgb.X >= max) {
            hsv.X = (rgb.Y - rgb.Z) / delta;
        } else if (rgb.Y >= max) {
            hsv.X = 2 + (rgb.Z - rgb.X) / delta;
        } else {
            hsv.X = 4 + (rgb.X - rgb.Y) / delta;
        }
        
        hsv.X *= 60;
        if (hsv.X < 0) {
            hsv.X += 360;
        }
        
        return hsv;
    }
    
    public static Vector3 Hsv2Rgb(Vector3 hsv) {
        if (hsv.Y == 0.0f) {
            return new Vector3(hsv.Z, hsv.Z, hsv.Z);
        }
        
        var hh = hsv.X;
        if (hh >= 360.0f) {
            hh = 0.0f;
        }
        hh /= 60.0f;
        var i  = (int) hh;
        var ff = hh - i;
        var p  = hsv.Z * (1.0f - hsv.Y);
        var q  = hsv.Z * (1.0f - hsv.Y * ff);
        var t  = hsv.Z * (1.0f - hsv.Y * (1.0f - ff));

        return i switch {
            0 => new Vector3(hsv.Z, t, p),
            1 => new Vector3(q, hsv.Z, p),
            2 => new Vector3(p, hsv.Z, t),
            3 => new Vector3(p, q, hsv.Z),
            4 => new Vector3(t, p, hsv.Z),
            _ => new Vector3(hsv.Z, p, q)
        };
    }
    
    public static Vector3 ToVector3(this Color color) {
        return new Vector3(color.r / 255f, color.g / 255f, color.b / 255f);
    }

    public static Color FromVector3(Vector3 vec) {
        return new Color((byte)(vec.X * 255), (byte)(vec.Y * 255), (byte)(vec.Z * 255), (byte)255);
    }
    
    public static Color Brighten(this Color color, float amount) {
        var hsv = Rgb2Hsv(color.ToVector3());
        var rgb = Hsv2Rgb(hsv with { Z = JMath.Clamp(hsv.Z + amount, 0.0f, 1.0f) });
        return FromVector3(rgb);
    }
}