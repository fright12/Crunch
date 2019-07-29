using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parse;

namespace Crunch
{
    using Operator = Operator<Operand>;
    using Value = Tuple<Operator<Operand>, int>;

    public static partial class Math
    {
#if DEBUG
        private static readonly Operator SUBTRACT = new Reader.BinaryOperator(
            (o1, o2) => o1.Subtract(o2),
            prev: (itr) =>
            {
                // Get the thing before
                itr.MovePrev();
                object previous = itr.Current;
                itr.MoveNext();

                // Add a 0 before if the thing before is not something we can operate on
                // If it's null or not an operand, then we can't operate on it, UNLESS
                //  it's a closing parenthesis (because we can parse that to get an operand)
                if (previous == null || (MathReader.Classify(previous) != Member.Operand && MathReader.Classify(previous) != Member.Closing))
                {
                    itr.Add(-1, new Operand(0));
                }

                Reader.BinaryOperator.Prev(itr);
            },
            juxtapose: true);

        internal static readonly Reader MathReader = new Reader(
            new System.Extensions.Trie<Tuple<Operator<Operand>, int>>
            {
                { "sin", new Value(Reader.Trig("sin", System.Math.Sin, System.Math.Asin), 0) },
                //{ "sin^(-1)", Function("sin^-1", 1, (o) => System.Math.Asin(o[0])) },
                //{ "sin^-1", Function.MachineInstructions("sin^-1", 1, (o) => System.Math.Asin(o[0])).Value },
                { "cos", new Value(Reader.Trig("cos", System.Math.Cos, System.Math.Acos), 0) },
                { "tan", new Value(Reader.Trig("tan", System.Math.Tan, System.Math.Atan), 0) },
                { "log_", new Value(Function("log_", 2, (o) => System.Math.Log(o[1], o[0])), 0) },
                { "log", new Value(Function("log", 1, (o) => System.Math.Log(o[0], ImplicitLogarithmBase)), 0) },
                { "ln", new Value(Function("ln", 1, (o) => System.Math.Log(o[0], System.Math.E)), 0) },
                { "sqrt", new Value(Function("sqrt", 1, (o) => System.Math.Pow(o[0], 0.5)), 0) },
            /*},
            new Dictionary<string, Operator>
            {*/
                { "^", new Value(new Reader.BinaryOperator((o1, o2) => o1.Exponentiate(o2)) { Order = ProcessingOrder.RightToLeft }, 1) },
            /*},
            new Dictionary<string, Operator>
            {*/
                { "/", new Value(new Reader.BinaryOperator((o1, o2) => o1.Divide(o2)), 2) },
                { "*", new Value(new Reader.BinaryOperator((o1, o2) => o1.Multiply(o2)), 2) },
            /*},
            new Dictionary<string, Operator>
            {*/
                { "-", new Value(SUBTRACT, 3) },
                { "+", new Value(new Reader.BinaryOperator((o1, o2) => o1.Add(o2), juxtapose: true), 3) }
            }
            );

        public static Operator<Operand> Function(string name, int parameterCount, Func<double[], double> operation)
        {
            Action<IEditEnumerator<object>>[] targets = new Action<IEditEnumerator<object>>[parameterCount];

            for (int i = 0; i < parameterCount; i++)
            {
                int j = i + 1;
                targets[i] = (itr) =>
                {
                    for (int k = 0; k < j; k++)
                    {
                        Reader.BinaryOperator.Next(itr);
                    }
                };
            }

            return new Operator<Operand>((o) => new Operand(new Term(new Function(name, operation, o))), ProcessingOrder.RightToLeft, targets);
        }
#endif

        public static Operand Evaluate(string str)
        {
            Print.Log("evaluating " + str);
#if DEBUG
            //return MathReader.Parse(str);
#endif
            try
            {
#if DEBUG
                return MathReader.Parse(str);
#else
                return Reader.Evaluate(str);
#endif
            }
            catch (Exception e)
            {
                Print.Log("error evaluating", e.Message);
                return null;
            }
        }
    }

    public static partial class Math
    {
        public static int DecimalPlaces = 3;
        public static double ImplicitLogarithmBase = 10;

        public static bool CanExponentiate(double b, double p) => !double.IsInfinity(System.Math.Pow(b, p));

        public static double GCD(double a, double b)
        {
            if (a == 0)
            {
                return b;
            }
            if (b == 0)
            {
                return a;
            }

            var sign = System.Math.Sign(a) * System.Math.Sign(b);
            a = System.Math.Abs(a);
            b = System.Math.Abs(b);

            return sign * a > b ? GCD(a % b, b) : GCD(a, b % a);
        }

        public static bool EqualDecimals(string c1, string c2)
        {
            int i = 0;
            for (; i < System.Math.Min(c1.Length, c2.Length) - 1; i++)
            {
                if (c1[i] != c2[i])
                {
                    string check = c1[i] == '0' ? c1 : c2;

                    for (int j = i; j < check.Length; j++)
                    {
                        if (check[j] != '0' && check[j] != '.')
                        {
                            return false;
                        }
                    }

                    return System.Math.Abs(c1[i - 1] - c2[i - 1]) <= 1;
                }
            }

            return true;
        }
    }
}
