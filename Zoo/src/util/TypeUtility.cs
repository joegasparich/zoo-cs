using System.Reflection;

namespace Zoo.util; 

public static class TypeUtility {
    public static IEnumerable<Type> GetTypesWithAttribute<Att>() where Att : Attribute {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            foreach (Type type in assembly.GetTypes()) {
                var attribs = type.GetCustomAttributes(typeof(Att), false);
                if (attribs != null && attribs.Length > 0) {
                    yield return type;
                }
            }
        }
    }
}