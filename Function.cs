using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crunch
{
    internal class TrigFunction : Function, IComparable<TrigFunction>
    {
        public Func<double, double> Regular;
        public Func<double, double> Inverse;

        public bool IsInverse = false;

        public TrigFunction(string name, Operand input, Func<double, double> regular, Func<double, double> inverse) : base(name, input)
        {
            Regular = regular;
            Inverse = inverse;
        }

        public int CompareTo(TrigFunction other) => base.CompareTo(other);

        new internal class Simplifier : ISimplifier<TrigFunction>
        {
            public bool HasTrig = false;

            private Trigonometry Units;
            private Operand.Simplifier Format;

            public Simplifier(Trigonometry units, Operand.Simplifier format)
            {
                Units = units;
                Format = format;
            }

            public Operand Simplify(TrigFunction tf)
            {
                Operand o = null;

                double constant;
                bool skip = tf.Operands[0].IsConstant(out constant);

                if (!skip)
                {
                    o = Format.Simplify(tf.Operands[0]);
                    //o = tf.Operands[0].Simplify(Format);
                }

                if (skip || o.IsConstant(out constant))
                {
                    if (!tf.IsInverse && Units == Trigonometry.Degrees)
                    {
                        constant *= System.Math.PI / 180;
                    }

                    double temp = System.Math.Round(tf.IsInverse ? tf.Inverse(constant) : tf.Regular(constant), 14);
                    ValidateAnswer(temp);
                    
                    if (tf.IsInverse && Units == Trigonometry.Degrees)
                    {
                        temp *= 180 / System.Math.PI;
                    }

                    HasTrig = true;
                    return temp;
                }
                else
                {
                    return new Term(new TrigFunction(tf.Name, o, tf.Regular, tf.Inverse));
                }
            }
        }
    }

    //internal delegate T FunctionWithVariableParamterCount<T>(params T[] operands);

    internal class Function : IComparable<Function>
    {
        public string Name { get; private set; }

        public Func<double[], double> Operation;
        public Operand[] Operands;

        public Function(string name, params Operand[] operands)
        {
            Name = name;
            Operands = operands;
        }

        public Function(string name, Func<double[], double> operation, params Operand[] operands) //: this(name, input) => Operation = operation;
        {
            Name = name;
            Operation = (d) =>
            {
                double ans = operation(d);
                ValidateAnswer(ans);
                return ans;
            };
            Operands = operands;
        }

        protected static void ValidateAnswer(double ans)
        {
            if (double.IsNaN(ans) || double.IsInfinity(ans))
            {
                throw new Exception("Invalid answer");
            }
        }

#if !DEBUG
        public static KeyValuePair<string, Machine.Operator> MachineInstructions(string name, int parameterCount, Func<double[], double> operation)
        {
            Func<LinkedListNode<object>, LinkedListNode<object>>[] targets = new Func<LinkedListNode<object>, LinkedListNode<object>>[parameterCount];

            for (int i = 0; i<parameterCount; i++)
            {
                int j = i + 1;
        targets[i] = (n) =>
                {
                    for (int k = 0; k<j; k++)
                    {
                        n = n.Next;
                    }
                    return n;
                };
            }

            return new KeyValuePair<string, Machine.Operator>(name, new Machine.Operator((o) => new Operand(new Term(new Function(name, operation, ParseOperands(o)))), targets));
        }
        
        private static Operand[] ParseOperands(object[] list)
        {
            Operand[] ans = new Operand[list.Length];

            for (int i = 0; i < list.Length; i++)
            {
                ans[i] = Reader.ParseOperand(list[i]);
            }

            return ans;
        }
#endif

        public override string ToString()
        {
            string ans = Name;
            
            foreach (Operand e in Operands)
            {
                ans += "(" + e + ")";
            }

            return ans;
        }

        public override bool Equals(object obj) => obj.ToString() == ToString();
        public override int GetHashCode() => ToString().GetHashCode();

        public int CompareTo(Function other)
        {
            if (Operands.Length != other.Operands.Length)
            {
                return Operands.Length.CompareTo(other.Operands.Length);
            }

            for (int i = 0; i < Operands.Length; i++)
            {
                int compare = Operands[i].CompareTo(other.Operands[i]);

                if (compare != 0)
                {
                    return compare;
                }
            }

            return Name.CompareTo(other.Name);
        }

        internal class Simplifier : ISimplifier<Function>
        {
            private Operand.Simplifier Format;

            public Simplifier(Operand.Simplifier format)
            {
                Format = format;
            }

            public Operand Simplify(Function f)
            {
                double[] inputs = new double[f.Operands.Length];
                for (int i = 0; f.Operands[i].IsConstant(out inputs[i]) || Format.Simplify(f.Operands[i]).IsConstant(out inputs[i]); i++)
                {
                    if (i == inputs.Length - 1)
                    {
                        return f.Operation(inputs);
                    }
                }

                return new Term(new Function(f.Name, f.Operands));
            }
        }
    }
}
