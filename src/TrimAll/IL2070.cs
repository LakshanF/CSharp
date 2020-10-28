using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace TrimAll
{
    public class IL2070
    {
        public void DoTheTango()
        {
            DoThePropertyTango(typeof(IL2070_Reflection));
        }

        //void DoThePropertyTango([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]Type ct1)
        void DoThePropertyTango(Type ct1)
        {
            var buddyProperties = ct1.GetProperties();
            Console.WriteLine($"DynamicallyAccessedMembers Property mismatch: {buddyProperties.Count()} - {(buddyProperties.Count()>0?buddyProperties.ToArray()[0]:"")}");
        }

    }

    class IL2070_Reflection
    {
        public string PropertySayTrim{get;}
        public string SayTrimReflection()
        {
            return "SayTrimReflection";
        }
    }
}