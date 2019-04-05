using System;
using System.Collections.Generic;
using System.Text;

using System.Extensions;
using Crunch.Machine;

namespace Crunch
{
    public class Reader : Machine.Reader
    {
        private static Func<LinkedListNode<object>, LinkedListNode<object>> NextOperand = (node) =>
        {
            //Next node is a minus sign
            if (node.Next != null && node.Next.Value == subtract)
            {
                //Delete the negative sign
                node.List.Remove(node.Next);
                
                //Negate what's after
                node.Next.Value = negator(node.Next.Value);
            }

            return node.Next;
        };

        private static Func<LinkedListNode<object>, LinkedListNode<object>> PreviousOrZero = (node) =>
        {
            if (node.Previous == null)
            {
                node.List.AddBefore(node, 0);
            }

            return node.Previous;
        };

        //internal static Reader Instance => instance ?? (instance = new Reader());
        //private static Reader instance;

        //protected override Trie<Operator> Operations => operations;
        //private Trie<Operator> operations;
        
        private static Func<object, object> negator = (o) => //Operand.Multiply(o.ParseOperand(), -1);
        {
            Operand temp = ParseOperand(o);
            temp.Multiply(-1);
            return temp;
        };
        //private static Operator exponentiate = BinaryOperator(Operand.Exponentiate);
        private static Operator exponentiate = BinaryOperator((o1, o2) => o1.Exponentiate(o2));
        private static Operator subtract = new Operator((o) =>
        {
            Operand ans = ParseOperand(o[0]);
            ans.Subtract(ParseOperand(o[1]));
            return ans;
        }, PreviousOrZero, NextOperand);

        public Reader(params KeyValuePair<string, Operator>[][] data) : base(data) { }
        private static Reader Instance;

        static Reader()
        {
            Instance = new Reader(
                new KeyValuePair<string, Operator>[]
                {
                    new KeyValuePair<string, Operator>("sin", Trig("sin", System.Math.Sin, System.Math.Asin)),
                    new KeyValuePair<string, Operator>("cos", Trig("cos", System.Math.Cos, System.Math.Acos)),
                    new KeyValuePair<string, Operator>("tan", Trig("tan", System.Math.Tan, System.Math.Atan)),
                    Function.MachineInstructions("log_", 2, (o) => System.Math.Log(o[1], o[0])),
                    Function.MachineInstructions("log", 1, (o) => System.Math.Log(o[0], Math.ImplicitLogarithmBase)),
                    Function.MachineInstructions("ln", 1, (o) => System.Math.Log(o[0], System.Math.E)),
                    Function.MachineInstructions("sqrt", 1, (o) => System.Math.Pow(o[0], 0.5))
                },
                new KeyValuePair<string, Operator>[]
                {
                    new KeyValuePair<string, Operator>("^", exponentiate)
                },
                new KeyValuePair<string, Operator>[]
                {
                    new KeyValuePair<string, Operator>("/", BinaryOperator((o1, o2) => o1.Divide(o2))),
                    new KeyValuePair<string, Operator>("*", BinaryOperator((o1, o2) => o1.Multiply(o2)))
                },
                new KeyValuePair<string, Operator>[]
                {
                    new KeyValuePair<string, Operator>("-", subtract),
                    new KeyValuePair<string, Operator>("+", BinaryOperator((o1, o2) => o1.Add(o2)))
                }
                );
        }

        private static bool isInverse;

        private static Operator Trig(string name, Func<double, double> normal, Func<double, double> inverse)
        {
            Func<LinkedListNode<object>, LinkedListNode<object>> next = (node) =>
            {
                isInverse = false;
                
                if (node.Next.Value == exponentiate)
                {
                    Operand e = ParseOperand(node.Next.Next.Value);
                    
                    if (e != null && e.IsConstant(-1))
                    //if ((node + 2).IsEqualTo("-") && (node + 3).IsEqualTo("1"))
                    {
                        node.Next.Next.Value = 1;
                        
                        isInverse = true;
                    }

                    return node.Next.Next.Next;
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

                    return new TrigFunction(name + (isInverse ? "^-1" : ""), ParseOperand(o[0]), temp);
                },
                next);
        }

        private static Operator BinaryOperator(Action<Operand, Operand> operation) => new BinaryOperator((o1, o2) =>
        {
            Operand ans = ParseOperand(o1);
            operation(ans, ParseOperand(o2));
            return ans;
        }, (node) => node.Previous, NextOperand);

        public static Operand Evaluate(string str)
        {
            print.log("evaluating " + str);
            try
            {
                return ParseOperand(Instance.Parse(str));
            }
            catch (Exception e)
            {
                print.log("error evaluating", e.Message);
                return null;
            }
        }

        internal static Operand ParseOperand(object str)
        {
            while (str is LinkedList<object>)
            {
                str = (str as LinkedList<object>).First?.Value;
            }
            
            if (str == null)
            {
                throw new Exception("Incorrectly formatted math");
            }
            if (str is Operand || str is Expression || str is Term)
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
                if (s.Length > 1 || str is Operator) // Instance.Operations.Contains(s).ToBool())
                {
                    throw new Exception("Cannot operate on value " + str + " of type " + str.GetType());
                }

                return new Term(s[0]);
            }
        }
    }
}
