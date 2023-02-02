using Zoo.defs;
using Zoo.util;

namespace Zoo; 

public class DefRef<T> where T : Def {
    private T?     def = null;
    private string name;
    
    public DefRef(string name) {
        this.name = name;
    }

    public T Def => def ??= Find.AssetManager.GetDef<T>(name);
    
    public static implicit operator T(DefRef<T> wrapper) {
        return wrapper.Def;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class DefOf : Attribute {}

public static class DefUtility {
    public static void LoadDefOfs() {
        var types = TypeUtility.GetTypesWithAttribute<DefOf>();

        foreach (var type in types) {
            var fields = type.GetFields();

            foreach (var field in fields) {
                if (!typeof(Def).IsAssignableFrom(field.FieldType.BaseType)) {
                    Debug.Warn($"Failed to load DefOf for {field.Name}");
                    continue;
                };
                
                field.SetValue(null, Find.AssetManager.GetDef(field.FieldType, field.Name));
            }
        }
    }
}

[DefOf]
public static class WallDefOf {
    public static WallDef IronBarFence;
}

[DefOf]
public static class TerrainDefOf {
    public static TerrainDef Grass;
}

[DefOf]
public static class NeedDefOf {
    public static NeedDef Hunger;
    public static NeedDef Thirst;
    public static NeedDef Energy;
}

[DefOf]
public static class ActorDefOf {
    public static ActorDef Guest;
    public static ActorDef Keeper;
}