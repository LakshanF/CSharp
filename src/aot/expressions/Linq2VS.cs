using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace TempLinq2
{
    public interface IFoo
    {
        int Bar();
    }

    public sealed class FooImpl : IFoo
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public int Bar()
        {
            return 0;
        }
    }

    public class TestException : Exception
    {
        public TestException()
            : base("This is a test exception")
        {
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            //M1();
            //M2();
            M3();
            //M4();
            //M5();
            //M7();
            Console.WriteLine("Bye, World!");
        }

        private static void M7()
        {
            //Func<int, int> f1 = value=>M6(value);
            Expression<Func<int, int>> myFunc = value => M6(value);
            Console.WriteLine(myFunc.Compile(true)(5));
        }

        private static void M5()
        {
            Console.WriteLine(M6(5));
        }

        private static int M6(int value)
        {
            int result = 1;
            try
            {
                while (true)
                {
                    if (value>1)
                    {
                        result*=value--;
                    }
                    else
                    {
                        goto lable;
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
lable:
            //Console.WriteLine(result);
            return result;
        }

        private static void M4()
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
            int factorial = Expression.Lambda<Func<int, int>>(block, value).Compile(true)(5);

            Console.WriteLine(factorial);
            // Prints 120.
        }

        private static void x()
        {
            List<System.Linq.Expressions.Expression> trees =
                new List<System.Linq.Expressions.Expression>()
                    { System.Linq.Expressions.Expression.Constant("oak"),
          System.Linq.Expressions.Expression.Constant("fir"),
          System.Linq.Expressions.Expression.Constant("spruce"),
          System.Linq.Expressions.Expression.Constant("alder") };

            // Create an expression tree that represents creating and
            // initializing a one-dimensional array of type string.
            System.Linq.Expressions.NewArrayExpression newArrayExpression =
                System.Linq.Expressions.Expression.NewArrayInit(typeof(string), trees);

            // Output the string representation of the Expression.
            Console.WriteLine(newArrayExpression.ToString());

            // This code produces the following output:
            //
            // new [] {"oak", "fir", "spruce", "alder"}
            //TryExpression tryExp = Expression.TryCatch(
            //    Expression.Throw(Expression.Constant(new TestException()), typeof(int)),
            //    Expression.Catch(typeof(TestException), Expression.Constant(1), Expression.Constant(false)),
            //    Expression.Catch(typeof(TestException), Expression.Constant(2), Expression.Constant(true)),
            //    Expression.Catch(typeof(TestException), Expression.Constant(3))
            //    );
            ////Assert.Equal(2, Expression.Lambda<Func<int>>(tryExp).Compile(true)());
            //Console.WriteLine(Expression.Lambda<Func<int>>(tryExp).Compile(true)());
        }

        private static void M2()
        {
            // Add the following directive to the file:
            // using System.Linq.Expressions;

            // A TryExpression object that has a Catch statement.
            // The return types of the Try block and all Catch blocks must be the same.
            TryExpression tryCatchExpr =
                Expression.TryCatch(
                    Expression.Block(
                        Expression.Throw(Expression.Constant(new DivideByZeroException())),
                        Expression.Constant("Try block")
                    ),
                    Expression.Catch(
                        typeof(DivideByZeroException),
                        Expression.Constant("Catch block")
                    )
                );

            // The following statement first creates an expression tree,
            // then compiles it, and then runs it.
            // If the exception is caught,
            // the result of the TryExpression is the last statement
            // of the corresponding Catch statement.
            Console.WriteLine(Expression.Lambda<Func<string>>(tryCatchExpr).Compile(true)());

            // This code example produces the following output:
            //
            // Catch block
        }

        private static void M1()
        {
            var foo = new FooImpl();
            //            var iFoo = (IFoo)foo;
            var compiledDirectCallIntepret = CompileBar(foo, asInterfaceCall: false, preferInterpret: true);
            var result = compiledDirectCallIntepret();
            Console.WriteLine(result);
        }

        static Func<int> CompileBar(IFoo foo, bool asInterfaceCall, bool preferInterpret)
        {
            var fooType = asInterfaceCall ? typeof(IFoo) : foo.GetType();
            var methodInfo = fooType.GetMethod(nameof(IFoo.Bar));
            var instance = Expression.Constant(foo, fooType);
            var call = Expression.Call(instance, methodInfo);
            var lambda = Expression.Lambda(call);
            var compiledFunction = (Func<int>)lambda.Compile(preferInterpret);
            return compiledFunction;
        }

    }
}