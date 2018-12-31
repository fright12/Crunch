﻿using System;
using System.Collections.Generic;
using System.Text;

using System.Extensions;
using Crunch.Machine;

namespace Crunch.Engine
{
    public static class Math
    {
        public static int DecimalPlaces = 3;
        public static Dictionary<char, Operand> KnownVariables = new Dictionary<char, Operand>()
        {
            { 'e', System.Math.E },
            { 'π', System.Math.PI }
        };

        private static OrderedTrie<Operator> operations;
        private static Func<object, object> negator = (o) => Operand.Multiply(o.ParseOperand(), -1);
        private static Operator exponentiate = BinaryOperator(Operand.Exponentiate);

        static Math()
        {
            operations = new OrderedTrie<Operator>(
                new KeyValuePair<string, Operator>[6]
                {
                    new KeyValuePair<string, Operator>("sin", Trig("sin", System.Math.Sin, System.Math.Asin)),
                    new KeyValuePair<string, Operator>("cos", Trig("cos", System.Math.Cos, System.Math.Acos)),
                    new KeyValuePair<string, Operator>("tan", Trig("tan", System.Math.Tan, System.Math.Atan)),
                    Function.MachineInstructions("log_", 2, (o) => System.Math.Log(o[1], o[0])),
                    Function.MachineInstructions("log", 1, (o) => System.Math.Log10(o[0])),
                    Function.MachineInstructions("ln", 1, (o) => System.Math.Log(o[0], System.Math.E))
                },
                new KeyValuePair<string, Operator>[1]
                {
                    new KeyValuePair<string, Operator>("^", exponentiate)
                },
                new KeyValuePair<string, Operator>[2]
                {
                    new KeyValuePair<string, Operator>("/", BinaryOperator(Operand.Divide)),
                    new KeyValuePair<string, Operator>("*", BinaryOperator(Operand.Multiply))
                },
                new KeyValuePair<string, Operator>[2]
                {
                    new KeyValuePair<string, Operator>("-", BinaryOperator(Operand.Subtract)),
                    new KeyValuePair<string, Operator>("+", BinaryOperator(Operand.Add))
                }
                );
        }

        private static bool isInverse;

        private static Operator Trig(string name, Func<double, double> normal, Func<double, double> inverse)
        {
            Func<Node<object>, Node<object>> next = (node) =>
            {
                isInverse = false;

                if (node.Next.Value == exponentiate)
                {
                    Expression e = (node + 2).Value.ParseOperand();
                    if (e != null && e.IsConstant(-1))
                    {
                        (node + 2).Value = 1;
                        isInverse = true;
                    }

                    return node + 3;
                }
                else
                {
                    return node.Next;
                }
            };

            return new Operator(
                (o) =>
                {
                    //Expression e = o[0].ParseOperand();
                    //double d;
                    //return e.IsConstant(out d) ? new Term(f(d)) : new Term(new Function(name, e, null));

                    Func<Trigonometry, double, double> temp = (trig, d) => normal(trig == Trigonometry.Degrees ? d * System.Math.PI / 180 : d);

                    if (isInverse)
                    {
                        temp = (trig, d) =>
                        {
                            double value = inverse(d);
                            if (double.IsNaN(value))
                            {

                            }
                            return value * (trig == Trigonometry.Degrees ? 180 / System.Math.PI : 1);
                        };
                    }

                    return new TrigFunction(name + (isInverse ? "^-1" : ""), o[0].ParseOperand(), temp);
                },
                next);
        }

        /*private static double trig(Func<double, double> f, Expression e)
        {
            Term t = ((Term)o).AllFormats().Last() as Term;
            if (Term.IsConstant(t))
            {
                return f(t.Numerator / t.Denominator * System.Math.PI / 180);
            }
            else
            {
                throw new Exception("Cannot operate on non-constant value " + t);
            }
        }*/

        private static Operator BinaryOperator(Func<Expression, Expression, Expression> f) => new BinaryOperator((o1, o2) => f(o1.ParseOperand(), o2.ParseOperand()));

        //private static Operator UnaryOperator(Func<Operand, Operand> f) => new UnaryOperator((o) => f(o.ParseOperand()));

        public static Operand Evaluate(string str)
        {
            var temp = new Dictionary<char, Operand>();
            return Evaluate(str, ref temp);
        }

        public static Operand Evaluate(string str, ref Dictionary<char, Operand> substitutions)
        {
            Operand ans = null;

            if (Testing.Active)
            {
                ans = Parse.Math(str, operations, negate: negator).ParseOperand();
                print.log("finished evaluating", ans);
            }
            else
            {
                try
                {
                    ans = Parse.Math(str, operations, negate: negator).ParseOperand();
                }
                catch (Exception e)
                {
                    print.log("error evaluating", e.Message);
                }
            }
            
            List<Operand> list = new List<Operand>();

            return ans;
        }

        internal static Expression[] ParseOperands(this object[] list)
        {
            Expression[] ans = new Expression[list.Length];
            
            for (int i = 0; i < list.Length; i++)
            {
                ans[i] = list[i].ParseOperand();
            }

            return ans;
        }

        private static Expression ParseOperand(this object str)
        {
            while (str is Quantity)
            {
                str = (str as Quantity).First.Value;
            }

            if (str is Expression || str is Term)
            {
                return str as dynamic;
            }
            if (str is Function)
            {
                return new Term(str as Function);
            }

            string s = str.ToString();
            if (s.Substring(0, 1).IsNumber())
            {
                return new Term(s);
            }
            else
            {
                if (s.Length > 1 || operations.Contains(s).ToBool())
                {
                    throw new Exception("Cannot operate on value " + str + " of type " + str.GetType());
                }

                return new Term(s[0]);
            }
        }
    }
}
