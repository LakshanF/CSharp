using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Collections;

public class Program
{
    static void Main(string[] args)
    {
        CheckLiftedDivideNullableNumberTest(true);
    }


    public static void CheckLiftedDivideNullableNumberTest(bool useInterpreter)
    {
        VerifyDivideNullableNumber(new Number(0), new Number(1), useInterpreter);
    }

    private static void VerifyDivideNullableNumber(Number? a, Number? b, bool useInterpreter)
    {
        Expression<Func<Number?>> e =
            Expression.Lambda<Func<Number?>>(
                Expression.Divide(
                    Expression.Constant(a, typeof(Number?)),
                    Expression.Constant(b, typeof(Number?))));
        Console.WriteLine(e.Body.Type);
        Debug.Assert(e.Body.Type == typeof(Number?));
        Func<Number?> f = e.Compile(useInterpreter);

        if (a.HasValue && b == new Number(0))
        {
            try
            {
                f();
            }
            catch (Exception ex) { Console.WriteLine(ex.GetType().Name); }
        }
        else
        {
            ReproEqual(a / b, f());
        }
    }

    static void ReproEqual<T>(T expected, T actual)
    {
        ReproEqual(expected, actual, new MyComparer<T>());
    }

    static void ReproEqual<T>(T expected, T actual, IEqualityComparer<T> comparer)
    {
        if (!comparer.Equals(expected, actual))
            throw new ArgumentException("NOT");
    }


}

public class MyComparer<T> : IEqualityComparer<T>
{


    static readonly IEqualityComparer DefaultInnerComparer = new MyComparerAdapter<object>(new MyComparer<object>());
    readonly Func<IEqualityComparer> innerComparerFactory;

    public MyComparer(IEqualityComparer innerComparer = null)
    {
        innerComparerFactory = () => innerComparer ?? DefaultInnerComparer;
    }

    public bool Equals(T? x, T? y)
    {
        // Implements IEquatable<typeof(y)>?
        var iequatableY = typeof(IEquatable<>).MakeGenericType(y.GetType()).GetTypeInfo();
        if (iequatableY.IsAssignableFrom(x.GetType().GetTypeInfo()))
        {
            var equalsMethod = iequatableY.GetDeclaredMethod(nameof(IEquatable<T>.Equals));
            if (equalsMethod == null)
                return false;

            return equalsMethod.Invoke(x, new object[] { y }) is true;
        }
        return false;
    }

    public int GetHashCode([DisallowNull] T obj)
    {
        throw new NotImplementedException();
    }
}

class MyComparerAdapter<T> : IEqualityComparer
{
    readonly IEqualityComparer<T> innerComparer;

    public MyComparerAdapter(IEqualityComparer<T> innerComparer)
    {
        if (innerComparer == null)
            throw new ArgumentNullException(nameof(innerComparer));

        this.innerComparer = innerComparer;
    }

    public new bool Equals(object x, object y)
    {
        return innerComparer.Equals((T)x, (T)y);
    }

    public int GetHashCode(object obj)
    {
        throw new NotImplementedException();
    }
}

public struct Number : IEquatable<Number>
{
    private readonly int _value;

    public Number(int value)
    {
        _value = value;
    }

    public static Number operator /(Number l, Number r) => new Number(l._value / r._value);
    public static bool operator ==(Number l, Number r) => l._value == r._value;
    public static bool operator !=(Number l, Number r) => l._value != r._value;

    public override bool Equals(object obj) => obj is Number && Equals((Number)obj);
    public bool Equals(Number other) => _value == other._value;
    public override int GetHashCode() => _value;
}

