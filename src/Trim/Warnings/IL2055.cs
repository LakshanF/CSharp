using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TrimAll
{
    public class IL2055
    {
        Type GetTheType()
        {
            Type generic = typeof(IL2055_Reflection<>);
            return generic;
        }

        public void DoTheTango()
        {
            typeof(IL2055_Reflection<>).MakeGenericType(new Type[] { typeof(IL2055) });
            Type t = GetTheType();
            Type[] typeArgs = { typeof(string)};

             Type constructed = t.MakeGenericType(typeArgs);
             Console.WriteLine($"MakeGenericType constructed: {constructed!=null}");
        }        
    }

    class IL2055_Reflection<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>
    {
        public bool Add(T t)
        {
            return true;
        }
    }
}

/**
class Lazy<[DynamicallyAccessedMembers(DynamicallyAccessedMemberType.PublicParameterlessConstructor)] T>
{
    // ...
}

void TestMethod(Type unknownType)
{
    // IL2055 Trim analysis: Call to `System.Type.MakeGenericType(Type[])` can not be statically analyzed. It's not possible to guarantee the availability of requirements of the generic type.
    typeof(Lazy<>).MakeGenericType(new Type[] { typeof(TestType) });

    // IL2055 Trim analysis: Call to `System.Type.MakeGenericType(Type[])` can not be statically analyzed. It's not possible to guarantee the availability of requirements of the generic type.
    unknownType.MakeGenericType(new Type[] { typeof(TestType) });
}
**/