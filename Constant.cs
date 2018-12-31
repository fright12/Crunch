using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch.Engine
{
    /*internal class Constant : Term // Operand, Operand<Constant>
    {
        public static readonly double MaxConstant = System.Math.Pow(10, 15);

        public double Value;
        public bool IsInteger => (int)Value == Value;

        public override bool IsConstant => throw new NotImplementedException();

        //public override bool IsConstant => true;

        //public static Operand Create(double d) => d < MaxConstant ? new Constant(d) : Term.Exponentiate(10, 24);
        private Constant(double value) : base() => Value = value;

        public static implicit operator Constant(double d) => new Constant(d);

        public Operand Multiply(Constant c) => Value * c.Value;
        public Operand Add(Constant c) => Value + c.Value;

        //public static Constant operator +(Constant c1, Constant c2) => 
        //public static Constant operator *(Constant c1, Constant c2) => new Constant(c1.Value * c2.Value);

        //public override Operand Simplest() => new Constant(Value);
        public override bool Equals(object obj) => obj is Constant && (obj as Constant).Value == Value;
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
    }*/
}
