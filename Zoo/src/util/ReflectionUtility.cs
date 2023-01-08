using System.Reflection;

namespace Zoo.util; 

public static class Objects {
    public static void CopyPropsTo<T1, T2>(this T1 source, ref T2 destination) {
        var sourceMembers = GetMembers(source.GetType());
        var destinationMembers = GetMembers(destination.GetType());

        // Copy data from source to destination
        foreach (var sourceMember in sourceMembers) {
            if (!CanRead(sourceMember)) {
                continue;
            }
            var destinationMember = destinationMembers.FirstOrDefault(x => x.Name.ToLower() == sourceMember.Name.ToLower());
            if (destinationMember == null || !CanWrite(destinationMember)) {
                continue;
            }
            SetObjectValue(ref destination, destinationMember, GetMemberValue(source, sourceMember));
        }
    }
    public static void InheritFrom<T1, T2>(this T1 child, T2 parent) {
        var childMembers      = GetMembers(child.GetType());
        var parentMembers = GetMembers(parent.GetType());
        
        // Copy data from source to destination
        foreach (var parentMember in parentMembers) {
            if (!CanRead(parentMember)) {
                continue;
            }
            var childMember = childMembers.FirstOrDefault(x => x.Name.ToLower() == parentMember.Name.ToLower());
            if (childMember == null || !CanWrite(childMember)) {
                continue;
            }
            if (childMember.GetValue(child) != null) {
                continue;
            }
            SetObjectValue(ref child, childMember, GetMemberValue(parent, parentMember));
        }
    }

    private static void SetObjectValue<T>(ref T obj, MemberInfo member, object value) {
        // Boxing method used for modifying structures
        var boxed = obj.GetType().IsValueType ? (object)obj : obj;
        SetMemberValue(ref boxed, member, value);
        obj = (T)boxed;
    }

    private static void SetMemberValue<T>(ref T obj, MemberInfo member, object value) {
        if (IsProperty(member)) {
            var prop = (PropertyInfo)member;
            if (prop.SetMethod != null) {
                prop.SetValue(obj, value);
            }
        } else if (IsField(member)) {
            var field = (FieldInfo)member;
            field.SetValue(obj, value);
        }
    }

    private static object GetMemberValue(object obj, MemberInfo member) {
        object result = null;
        if (IsProperty(member)) {
            var prop = (PropertyInfo)member;
            result = prop.GetValue(obj, prop.GetIndexParameters().Count() == 1 ? new object[] { null } : null);
        } else if (IsField(member)) {
            var field = (FieldInfo)member;
            result = field.GetValue(obj);
        }
        return result;
    }

    private static bool CanWrite(MemberInfo member) {
        return IsProperty(member) ? ((PropertyInfo)member).CanWrite : IsField(member);
    }

    private static bool CanRead(MemberInfo member) {
        return IsProperty(member) ? ((PropertyInfo)member).CanRead : IsField(member);
    }

    private static bool IsProperty(MemberInfo member) {
        return IsType(member.GetType(), typeof(PropertyInfo));
    }

    private static bool IsField(MemberInfo member) {
        return IsType(member.GetType(), typeof(FieldInfo));
    }

    private static bool IsType(Type type, Type targetType) {
        return type.Equals(targetType) || type.IsSubclassOf(targetType);
    }
    
    public static object GetValue(this MemberInfo memberInfo, object forObject) {
        switch (memberInfo.MemberType) {
            case MemberTypes.Field:
                return ((FieldInfo)memberInfo).GetValue(forObject);
            case MemberTypes.Property:
                return ((PropertyInfo)memberInfo).GetValue(forObject);
            default:
                throw new NotImplementedException();
        }
    } 

    private static List<MemberInfo> GetMembers(Type type) {
        var flags = BindingFlags.Instance | BindingFlags.Public
            | BindingFlags.NonPublic;
        var members = new List<MemberInfo>();
        members.AddRange(type.GetProperties(flags));
        members.AddRange(type.GetFields(flags));
        return members;
    }
}