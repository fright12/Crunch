using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crunch.Engine
{
    internal class TrigFunction : Function
    {
        new public Func<Trigonometry, double, double> Operation;
        //public Func<double, double> Inverse;

        public TrigFunction(string name, Expression input, Func<Trigonometry, double, double> operation) : base(name, input)
        {
            Operation = operation;
            //Inverse = inverse;
        }
    }

    //internal delegate T FunctionWithVariableParamterCount<T>(params T[] operands);

    internal class Function
    {
        public string Name { get; private set; }

        public Func<double[], double> Operation;
        public Expression[] Operands;

        public Function(string name, params Expression[] operands)
        {
            Name = name;
            Operands = operands;
        }

        public Function(string name, Func<double[], double> operation, params Expression[] operands) //: this(name, input) => Operation = operation;
        {
            Name = name;
            Operation = (d) =>
            {
                double ans = operation(d);
                if (double.IsNaN(ans))
                {
                    throw new Exception("NaN answer");
                }
                return ans;
            };
            Operands = operands;
        }
        
        public static KeyValuePair<string, Machine.Operator> MachineInstructions(string name, int parameterCount, Func<double[], double> operation)
        {
            Func<Machine.Node<object>, Machine.Node<object>>[] targets = new Func<Machine.Node<object>, Machine.Node<object>>[parameterCount];

            for (int i = 0; i < parameterCount; i++)
            {
                int j = i + 1;
                targets[i] = (n) => n + j;
            }

            return new KeyValuePair<string, Machine.Operator>(name, new Machine.Operator((o) => new Function(name, operation, o.ParseOperands()), targets));
        }

        public override string ToString()
        {
            string ans = Name;
            
            foreach (Expression e in Operands)
            {
                ans += "(" + e + ")";
            }

            return ans;
        }

        public override bool Equals(object obj) => obj.ToString() == ToString();
        public override int GetHashCode() => ToString().GetHashCode();
    }
}
