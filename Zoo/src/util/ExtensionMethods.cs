using System.ComponentModel;
using System.Numerics;
using System.Reflection;
using Raylib_cs;

namespace Zoo.util; 

public static class ExtensionMethods {
    // Float //
    public static bool FEquals(this float a, float b) {
        return Math.Abs(a - b) < 0.0001f;
    }
    public static int RoundToInt(this float f) => (int)MathF.Round(f);
    public static int FloorToInt(this float f) => (int)MathF.Floor(f);
    public static int CeilToInt(this float  f) => (int)MathF.Ceiling(f);
    
    // Enum //
    public static int ToInt(this Enum e) {
        return Convert.ToInt32(e);
    }
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
    public static bool NullOrEmpty( this string str ) {
        return string.IsNullOrEmpty(str);
    }
    
    // Vector2 //
    public static IntVec2 Floor(this Vector2 vec) {
        return new IntVec2(vec.X.FloorToInt(), vec.Y.FloorToInt());
    }
    public static void Deconstruct(this Vector2 v, out float x, out float y) {
        x = v.X;
        y = v.Y;
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
    
    // List //
    public static T Pop<T>(this IList<T> source) {
        if(!source.Any()) throw new Exception();
        var element = source[^1];
        source.RemoveAt(source.Count - 1);
        return element;
    }
    public static void MoveItemAtIndexToFront<T>(this IList<T> list, int index) {
        var item = list[index];
        list.RemoveAt(index);
        list.Insert(0, item);
    }
}