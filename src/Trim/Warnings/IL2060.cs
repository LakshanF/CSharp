using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace TrimAll
{
    public class IL2060
    {
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
        Type GetTheType()
        {
            Type generic = typeof(IL2060_Reflection);
            return generic;
        }

        public void DoTheTango()
        {
            Type t = GetTheType();

            var m1 = t.GetMethod("Add");
                        
            Type[] typeArgs = { typeof(int)};

            MethodInfo constructed = m1.MakeGenericMethod(typeArgs);
            Console.WriteLine($"MakeGenericMethod constructed: {constructed!=null}");
        }        
    }

    class IL2060_Reflection
    {
        public bool Add<T>(string s)
        {
            T t = default(T);
            if(t==null)
                return true;
            else
                return false;
        }
    }
}