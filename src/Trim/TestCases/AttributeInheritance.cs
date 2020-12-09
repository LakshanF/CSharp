using System;

Console.WriteLine(Attribute.GetCustomAttributes(typeof(Derived).GetProperty("X"), inherit: true).Length==3);

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
class FooAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
class BarAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
class BazAttribute : Attribute { }

class Base
{
    [Foo]
    public virtual int X { get; }
}

class Mid : Base
{
    [Bar]
    public override int X => base.X;
}

class Derived : Mid
{
    [Baz]
    public override int X => base.X;
}