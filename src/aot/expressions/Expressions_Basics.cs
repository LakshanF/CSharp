using System;
using System.Linq.Expressions;

using System.Diagnostics.CodeAnalysis;
public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(M1(5));
        M2();
        M3();
        M4();
    }


    static int M1(int value)
    {
        int result = 1;
        while (true)
        {
            if (value > 1)
            {
                result *= value--;
            }
            else
            {
                goto L1;
            }
        }
L1:
        return result;
    }

    static void M2()
    {
        Func<int, int> f1 = value =>
        {
            int result = 1;
            while (true)
            {
                if (value > 1)
                {
                    result *= value--;
                }
                else
                {
                    goto L1;
                }
            }
            L1:
            return result;
        };

        Console.WriteLine(f1(5));
    }

    static void M3()
    {
        // Creating a parameter expression.  
        ParameterExpression value = Expression.Parameter(typeof(int), "value");

        // Creating an expression to hold a local variable.
        ParameterExpression result = Expression.Parameter(typeof(int), "result");

        // Creating a label to jump to from a loop.  
        LabelTarget label = Expression.Label(typeof(int));

        // Creating a method body.  
        BlockExpression block = Expression.Block(
            // Adding a local variable.  
            new[] { result },
            // Assigning a constant to a local variable: result = 1  
            Expression.Assign(result, Expression.Constant(1)),
                // Adding a loop.  
                Expression.Loop(
                   // Adding a conditional block into the loop.  
                   Expression.IfThenElse(
                       // Condition: value > 1  
                       Expression.GreaterThan(value, Expression.Constant(1)),
                       // If true: result *= value --  
                       Expression.MultiplyAssign(result,
                           Expression.PostDecrementAssign(value)),
                       // If false, exit the loop and go to the label.  
                       Expression.Break(label, result)
                   ),
               // Label to jump to.  
               label
            )
        );

        // Compile and execute an expression tree.  
        int factorial = Expression.Lambda<Func<int, int>>(block, value).Compile()(5);

        Console.WriteLine(factorial);
        // Prints 120.    \
    }

    static void M4()
    {
        Func<int, int> f1 = value =>
        {
            int result = 1;
            while (true)
            {
                if (value > 1)
                {
                    result *= value--;
                }
                else
                {
                    goto L1;
                }
            }
L1:
            return result;
        };

        Expression<Func<int, int>> expression;
        // @TODO - how can I use f1 in expression

        Console.WriteLine("@TODO");
    }

}
