using System;
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
            Type t = GetTheType();
            Type[] typeArgs = { typeof(string)};

             Type constructed = t.MakeGenericType(typeArgs);
             Console.WriteLine($"MakeGenericType constructed: {constructed!=null}");
        }        
    }

    class IL2055_Reflection<T>
    {
        public bool Add(T t)
        {
            return true;
        }
    }
}