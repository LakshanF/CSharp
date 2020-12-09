using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace TrimAll
{
    public class IL2075
    {

        //This will suppress IL2072 since we want to focus on 2075
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type GetTrimType()
        {          
            return typeof(IL2075_Reflection);
        }

        public void DoTheTango()
        {
            Type ct1 = GetTrimType();

            var m1 = ct1.GetMethod("SayTrim");
            Console.WriteLine("DynamicallyAccessedMembers method mismatch: " + m1.Invoke(Activator.CreateInstance(ct1), null));

        }
    }

    class IL2075_Reflection
    {
        public string SayTrim()
        {
            return "SayTrim";
        }

    }
}