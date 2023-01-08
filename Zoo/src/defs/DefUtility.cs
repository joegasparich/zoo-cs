using Zoo.defs;
using Zoo.util;

namespace Zoo; 

[AttributeUsage(AttributeTargets.Class)]
public class DefOf : Attribute {}

public static class DefUtility {
    public static void LoadDefOfs() {
        var types = TypeUtility.GetTypesWithAttribute<DefOf>();

        foreach (var type in types) {
            var fields = type.GetFields();

            foreach (var field in fields) {
                if (field.FieldType.BaseType != typeof(Def)) continue;
                
                field.SetValue(null, Find.AssetManager.Get(field.FieldType, field.Name));
            }
        }
    }
}

[DefOf]
public static class WallDefOf {
    public static WallDef IronBarFence;
}

[DefOf]
public static class BiomeDefOf {
    public static BiomeDef Grass;
}