using System;
using System.Reflection;
using System.Linq.Expressions;

public class Program
{
    static void Main(string[] args)
    {
        M1();
    }

    static void M1()
    {
            ushort?[] values = new ushort?[] { null, 0, 1, ushort.MaxValue };
            for (int i = 0; i < values.Length; i++)
            {
                for (int j = 0; j < values.Length; j++)
                {
                    VerifyDivideNullableUShort(values[i], values[j], true);
                }
            }
    }

    private static void VerifyDivideNullableUShort(ushort? a, ushort? b, bool useInterpreter)
    {
        Expression<Func<ushort?>> e =
            Expression.Lambda<Func<ushort?>>(
                Expression.Divide(
                    Expression.Constant(a, typeof(ushort?)),
                    Expression.Constant(b, typeof(ushort?)),
                    typeof(Program).GetTypeInfo().GetDeclaredMethod("DivideNullableUShort")));
        Func<ushort?> f = e.Compile(useInterpreter);

        if (a.HasValue && b == 0)
        {
            try
            {
                f();
            }
            catch(Exception ex){Console.WriteLine(ex.GetType().Name);}
            //Assert.Throws<DivideByZeroException>(() => f());
        }
        else
        {
            var f1 = f();
            Console.WriteLine(f1);
         //   Assert.Equal((ushort?)(a / b), f());
        }
    }


    public static ushort DivideNullableUShort(ushort a, ushort b)
    {
        return (ushort)(a / b);
    }

}

