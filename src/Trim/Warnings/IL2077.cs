using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace TrimAll
{
    public class IL2077
    {

        Type _typeField;

        void NeedsPublicConstructors([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
        {
            Console.WriteLine($"Caller callee mismatch on DynamicallyAccessedMembers");
        }


        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        void GetTrimType()
        {          
            _typeField =  typeof(IL2077_Reflection);
        }

        public void Run()
        {
            GetTrimType();
            TestMethod();

        }

        void TestMethod()
        {
            // IL2077 Trim analysis: The requirements declared via the 'DynamicallyAccessedMembersAttribute' on the field '_typeField' 
            // don't match those on the parameter 'type' of method 'NeedsPublicConstructors'.
            // The source value must declare at least the same requirements as those declared on the target location it's assigned to
            NeedsPublicConstructors(_typeField);
        }        

    }

    class IL2077_Reflection
    {
        public string SayTrim()
        {
            return "SayTrim";
        }

    }
}