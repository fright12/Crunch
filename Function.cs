﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crunch
{
    internal class TrigFunction : Function, IComparable<TrigFunction>
    {
        new public Func<double, Trigonometry, double> Operation;
        public Func<double, double> Regular;
        public Func<double, double> Inverse;

        public bool IsInverse = false;

        public TrigFunction(string name, Operand input, Func<double, Trigonometry, double> operation) : base(name, input)
        {
            Operation = operation;
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
                }

                if (skip || o.IsConstant(out constant))
                {
                    double temp = System.Math.Round(tf.Operation(constant, Units), 14);

                    ValidateAnswer(temp);
                    HasTrig = true;
                    return temp;
                }
                else
                {
                    return new Term(new TrigFunction(tf.Name, o, tf.Operation));
                }
            }
        }
    }

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
