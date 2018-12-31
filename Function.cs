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

    internal class Function
    {
        public string Name { get; private set; }

        public Func<Expression, Expression> Operation;
        public Expression Input;

        public Function(string name, Expression input)
        {
            Name = name;
            Input = input;
        }

        public Function(string name, Expression input, Func<Expression, Expression> operation) : this(name, input) => Operation = operation;

        public override string ToString() => Name + Input.ToString();

        public override bool Equals(object obj) => obj.ToString() == ToString();
        public override int GetHashCode() => ToString().GetHashCode();
    }
}
