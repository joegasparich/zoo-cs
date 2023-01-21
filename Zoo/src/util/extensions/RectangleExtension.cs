using System.Numerics;
using Raylib_cs;

namespace Zoo.util; 

public static class RectangleExtension {
    public static Vector2 Position(this Rectangle rect) {
        return new Vector2(rect.x, rect.y);
    }
    public static Vector2 Dimensions(this Rectangle rect) {
        return new Vector2(rect.width, rect.height);
    }
    public static bool Contains(this Rectangle rect, Vector2 point) {
        return JMath.PointInRect(rect, point);
    }
    public static float XMax(this Rectangle rect) {
        return rect.x + rect.width;
    }
    public static float YMax(this Rectangle rect) {
        return rect.y + rect.height;
    }
    public static Rectangle ContractedBy(this Rectangle rect, float amt) {
        return new Rectangle(rect.x + amt, rect.y + amt, rect.width - amt * 2, rect.height - amt * 2);
    }
    public static Rectangle ExpandedBy(this Rectangle rect, float amt) {
        return new Rectangle(rect.x - amt, rect.y - amt, rect.width + amt * 2, rect.height + amt * 2);
    }
    public static Rectangle TopPct(this Rectangle rect, float pct) {
        return new Rectangle(rect.x, rect.y, rect.width, rect.height * pct);
    }
    public static Rectangle BottomPct(this Rectangle rect, float pct) {
        return new Rectangle(rect.x, rect.y + rect.height * (1 - pct), rect.width, rect.height * pct);
    }
    public static Rectangle LeftPct(this Rectangle rect, float pct) {
        return new Rectangle(rect.x, rect.y, rect.width * pct, rect.height);
    }
    public static Rectangle RightPct(this Rectangle rect, float pct) {
        return new Rectangle(rect.x + rect.width * (1 - pct), rect.y, rect.width * pct, rect.height);
    }
    public static Rectangle TopHalf(this Rectangle rect) {
        return rect.TopPct(0.5f);
    }
    public static Rectangle BottomHalf(this Rectangle rect) {
        return rect.BottomPct(0.5f);
    }
    public static Rectangle LeftHalf(this Rectangle rect) {
        return rect.LeftPct(0.5f);
    }
    public static Rectangle RightHalf(this Rectangle rect) {
        return rect.RightPct(0.5f);
    }
    public static Rectangle Multiply(this Rectangle rect, float amt) {
        return new Rectangle(rect.x * amt, rect.y * amt, rect.width * amt, rect.height * amt);
    }
}