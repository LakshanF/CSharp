using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace TrimAll
{
    public class IL2080
    {

        Type _typeField;


        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        void GetTrimType()
        {          
            _typeField =  typeof(IL2080_Reflection);
        }

        public void DoTheTango()
        {
            GetTrimType();
            TestMethod();

        }

        void TestMethod()
        {
            // IL2080 Trim analysis: The requirements declared via the 'DynamicallyAccessedMembersAttribute' on the field '_typeField' 
            // don't match those on the implicit 'this' parameter of method 'Type.GetMethods()'.
            // The source value must declare at least the same requirements as those declared on the target location it's assigned to
            _typeField.GetMethods(); // Type.GetMethods is annotated with DynamicallyAccessedMemberTypes.PublicMethods
            Console.WriteLine($"Caller callee mismatch on DynamicallyAccessedMembers");
        }        

    }

    public class IL2080_Reflection
    {
        public string SayTrim()
        {
            return "SayTrim";
        }

    }
}