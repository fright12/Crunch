using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parse;

namespace Crunch
{
    using Operator = Operator<Token>;
    using Operation = KeyValuePair<string, Operator<Token>>;

    public static partial class Math
    {
        public static bool IsOpeningToken(this Token token) => token is Token.Separator separator && separator.IsOpening;

        private static readonly Operator SUBTRACT = new Reader.BinaryOperator(
            (o1, o2) => o1.Subtract(o2),
            prev: (itr) =>
            {
                // Get the thing before
                itr.MovePrev();
                Token previous = itr.Current;
                itr.MoveNext();

                // Add a 0 before if the thing before is not something we can operate on
                // If it's null or not an operand, then we can't operate on it, UNLESS
                //  it's a closing parenthesis (because we can parse that to get an operand)
                //Member member = previous?.Class ?? ;
                if (previous == null || (!(previous is Token.Operand) && previous.IsOpeningToken()))
                //(member != Member.Operand && member != Member.Closing))
                {
                    itr.Add(-1, new Token.Operand<Operand>(new Operand(0)));
                }

                Reader.BinaryOperator.Prev(itr);
            },
            juxtapose: true);

        /*internal static readonly Reader MathReader = new Reader(
            new System.Extensions.Trie<Tuple<Operator<Operand>, int>>
            {
                { "sin", new Value(Reader.Trig("sin", System.Math.Sin, System.Math.Asin), 0) },

                { "cos", new Value(Reader.Trig("cos", System.Math.Cos, System.Math.Acos), 0) },
                { "tan", new Value(Reader.Trig("tan", System.Math.Tan, System.Math.Atan), 0) },
                { "log_", new Value(Function("log_", 2, (o) => System.Math.Log(o[1], o[0])), 0) },
                { "log", new Value(Function("log", 1, (o) => System.Math.Log(o[0], ImplicitLogarithmBase)), 0) },
                { "ln", new Value(Function("ln", 1, (o) => System.Math.Log(o[0], System.Math.E)), 0) },
                { "sqrt", new Value(Function("sqrt", 1, (o) => System.Math.Pow(o[0], 0.5)), 0) },
                { "^", new Value(new Reader.BinaryOperator((o1, o2) => o1.Exponentiate(o2)) { Order = ProcessingOrder.RightToLeft }, 1) },
                { "/", new Value(new Reader.BinaryOperator((o1, o2) => o1.Divide(o2)), 2) },
                { "*", new Value(new Reader.BinaryOperator((o1, o2) => o1.Multiply(o2)), 2) },
                { "-", new Value(SUBTRACT, 3) },
                { "+", new Value(new Reader.BinaryOperator((o1, o2) => o1.Add(o2), juxtapose: true), 3) }
            }
            );*/

        internal static readonly Reader MathReader = new Reader(
            new Operation[]
            {
                new Operation("sin", Reader.Trig("sin", System.Math.Sin, System.Math.Asin)),
                new Operation("cos", Reader.Trig("cos", System.Math.Cos, System.Math.Acos)),
                new Operation("tan", Reader.Trig("tan", System.Math.Tan, System.Math.Atan)),
                new Operation("log_", Function("log_", 2, (o) => System.Math.Log(o[1], o[0]))),
                new Operation("log", Function("log", 1, (o) => System.Math.Log(o[0], ImplicitLogarithmBase))),
                new Operation("ln", Function("ln", 1, (o) => System.Math.Log(o[0], System.Math.E))),
                new Operation("sqrt", Function("sqrt", 1, (o) => System.Math.Pow(o[0], 0.5))),
            },
            new Operation[]
            {
                new Operation("^", new Reader.BinaryOperator((o1, o2) => o1.Exponentiate(o2)) { Order = ProcessingOrder.RightToLeft }),
            },
            new Operation[]
            {
                new Operation("/", new Reader.BinaryOperator((o1, o2) => o1.Divide(o2))),
                new Operation("*", new Reader.BinaryOperator((o1, o2) => o1.Multiply(o2)))
            },
            new Operation[]
            {
                new Operation("-", SUBTRACT),
                new Operation("+", new Reader.BinaryOperator((o1, o2) => o1.Add(o2), juxtapose: true))
            });

        public static Operator<Token> Function(string name, int parameterCount, Func<double[], double> operation)
        {
            Action<IEditEnumerator<Token>>[] targets = new Action<IEditEnumerator<Token>>[parameterCount];

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

            return new Operator<Token>((o) =>
            {
                Operand[] operands = new Operand[o.Length];
                for(int i = 0; i < o.Length; i++)
                {
                    operands[i] = (Operand)o[i].Value;
                }
                return new Token.Operand<Operand>(new Operand(new Term(new Function(name, operation, operands))));
            }, ProcessingOrder.RightToLeft, targets);
            //return new Operator<Operand>((o) => new Operand(new Term(new Function(name, operation, o))), ProcessingOrder.RightToLeft, targets);
        }

        public static Operand Evaluate(string str) => (Operand)MathReader.Parse(str).Value;
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
