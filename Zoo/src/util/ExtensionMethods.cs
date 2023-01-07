using System.ComponentModel;
using System.Numerics;
using System.Reflection;
using Raylib_cs;

namespace Zoo.util; 

public static class ExtensionMethods {
    // Float //
    public static bool NearlyEquals(this float a, float b) {
        return Math.Abs(a - b) < 0.0001f;
    }
    public static int RoundToInt(this float f) => (int)MathF.Round(f);
    public static int FloorToInt(this float f) => (int)MathF.Floor(f);
    public static int CeilToInt(this float  f) => (int)MathF.Ceiling(f);
    
    // Enum //
    // https://stackoverflow.com/questions/1415140/can-my-enums-have-friendly-names
    public static string? GetDescription(this Enum value)
    {
        Type    type = value.GetType();
        string? name = Enum.GetName(type, value);
        
        if (name != null) {
            FieldInfo? field = type.GetField(name);
            
            if (field != null) {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr){
                    return attr.Description;
                }
            }
        }
        return null;
    }
    
    // String //
    public static bool NullOrEmpty(this string str) {
        return string.IsNullOrEmpty(str);
    }
    public static string Capitalise(this string str) {
        if (str.Length > 1) {
            return char.ToUpper(str[0]) + str.Substring(1);
        }
        return str.ToUpper();
    }
    public static string ToSnakeCase(this string str) {
        return str.ToLower().Replace(" ", "_");
    }
    
    // Vector2 //
    public static IntVec2 Floor(this Vector2 vec) {
        return new IntVec2(vec.X.FloorToInt(), vec.Y.FloorToInt());
    }
    public static IntVec2 Round(this Vector2 vec) {
        return new IntVec2(vec.X.RoundToInt(), vec.Y.RoundToInt());
    }
    public static void Deconstruct(this Vector2 v, out float x, out float y) {
        x = v.X;
        y = v.Y;
    }
    public static float Distance(this Vector2 a, Vector2 b) {
        return MathF.Sqrt(MathF.Pow(a.X - b.X, 2) + MathF.Pow(a.Y - b.Y, 2));
    }
    public static float DistanceSquared(this Vector2 a, Vector2 b) {
        return MathF.Pow(a.X - b.X, 2) + MathF.Pow(a.Y - b.Y, 2);
    }
    public static Vector2 Normalised(this Vector2 v) {
        if (v.LengthSquared() == 0) return Vector2.Zero;
        return v / v.Length();
    }
    public static Vector3 ToVector3(this Vector2 v) {
        return new Vector3(v.X, v.Y, 0);
    }
    
    // Vector3 //
    public static void Deconstruct(this Vector3 v, out float x, out float y, out float z) {
        x = v.X;
        y = v.Y;
        z = v.Z;
    }
    
    // Rectangle //
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
    
    // List //
    public static T Pop<T>(this IList<T> source) {
        if(!source.Any()) throw new Exception();
        var element = source[^1];
        source.RemoveAt(source.Count - 1);
        return element;
    }
    public static T Dequeue<T>(this IList<T> source) {
        if(!source.Any()) throw new Exception();
        var element = source[0];
        source.RemoveAt(0);
        return element;
    }
    public static void MoveItemAtIndexToFront<T>(this IList<T> list, int index) {
        var item = list[index];
        list.RemoveAt(index);
        list.Insert(0, item);
    }
    public static void MoveItemAtIndexToBack<T>(this IList<T> list, int index) {
        var item = list[index];
        list.RemoveAt(index);
        list.Add(item);
    }
    public static bool NullOrEmpty<T>( this List<T>? list ) {
        return list == null || list.Count == 0;
    }
    
    // Texture2D //
    public static bool Empty(this Texture2D tex) {
        return tex.id == 0;
    }
    
    // Key //
    public static bool IsAlphanumeric(this KeyboardKey key) {
        return key is >= KeyboardKey.KEY_APOSTROPHE and <= KeyboardKey.KEY_GRAVE;
    }
}