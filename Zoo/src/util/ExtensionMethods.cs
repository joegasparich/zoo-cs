using System.ComponentModel;
using System.Numerics;
using System.Reflection;

namespace Zoo.util; 

public static class ExtensionMethods {
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
    
    public static bool NullOrEmpty( this string str )
    {
        return string.IsNullOrEmpty(str);
    }
    
    public static IntVec2 Floor(this Vector2 vec) {
        return new IntVec2(JMath.FloorToInt(vec.X), JMath.FloorToInt(vec.Y));
    }
}