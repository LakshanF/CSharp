using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace TrimAll
{
    public class IL2067
    {

        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type GetTrimType()
        {          
            return typeof(IL2067_Reflection);
        }

        public void DoTheTango()
        {
            Type ct1 = GetTrimType();
            TestMethod(ct1);

        }

        void NeedsPublicConstructors([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
        {
            Console.WriteLine($"Caller callee mismatch on DynamicallyAccessedMembers");
        }

        void TestMethod(Type type)
        {
            // IL2067 Trim analysis: The requirements declared via the 'DynamicallyAccessedMembersAttribute' on the parameter 'type' of method 'TestMethod' 
            // don't match those on the parameter 'type' of method 'NeedsPublicConstructors'. 
            // The source value must declare at least the same requirements as those declared on the target location it's assigned to
            NeedsPublicConstructors(type);
        }

    }

    class IL2067_Reflection
    {
        public string SayTrim()
        {
            return "SayTrim";
        }

    }
}