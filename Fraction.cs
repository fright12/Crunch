using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crunch.Engine
{
    /*public class Fraction //: Operand
    {
        //public bool IsConstant => Numerator is Constant && Denominator is Constant;

        private Term numerator;
        private Term denominator;

        private Fraction(Term n, Term d)
        {
            //print.log("making fraction with " + n + " and " + d);
            Term.Divide(ref n, ref d);

            numerator = n;
            denominator = d;
        }

        public static implicit operator Fraction(Term t) => new Fraction(t, new Term(1));
        public static implicit operator Fraction(Expression e) => new Fraction((Term)e, new Term(1));

        public Fraction Copy() => new Fraction(numerator.Copy(), denominator.Copy());

        private void invert()
        {
            var temp = numerator;
            numerator = denominator;
            denominator = temp;
        }

        //private static Operand Divide(Operand o1, Operand o2) => (o1 as dynamic).Divide(o2 as dynamic) ?? o1 * (o2 ^ -1);

        /*private static Operand Construct(Operand numerator, Operand denominator)
        {
            if (numerator is Fraction || denominator is Fraction)
            {
                Fraction f = (denominator as Fraction) ?? new Fraction(denominator, 1);
                return numerator * new Fraction(f.Denominator, f.Numerator);
            }
            else if (denominator is Constant && (denominator as Constant).Value == 1)
            {
                return numerator;
            }
            return new Fraction(numerator, denominator);
        }

        public Operand Simplify()
        {
            Expression e1 = numerator.Copy();
            Expression e2 = denominator.Copy();
            Term n = (Term)(e1.Simplify() as Expression);
            Term d = (Term)(e2.Simplify() as Expression);

            if (n.IsConstant && d.IsConstant)
            {
                return null;// n.Coefficient / d.Coefficient;
            }
            else
            {
                return null;// new Fraction(n, d);
            }
        }

        /******************************* ADDITION ******************************
        public Fraction Add(Fraction f)
        {
            //print.log("adding fractions " + this + " and " + f);
            Term d = null;// denominator.Copy().Multiply(f.denominator);
            return null;// new Fraction((Term)numerator.Multiply(f.denominator).Add(f.numerator.Multiply(denominator)), d);

            //Term gcd = Term.GCD(denominator, f.denominator);
            //gcd.Exponentiate(-1);

            //return new Fraction(numerator.Multiply(f.denominator).Multiply(gcd).Add(f.numerator.Multiply(denominator).Multiply(gcd)), (Term)denominator * f.denominator * gcd);
        }

        //public new Operand Add(Operand o) => (IsConstant && !(o is Constant)) ? null : (Numerator + Denominator * o) / Denominator;
        //public Operand Add(Fraction f) => Denominator.Equals(f.Denominator) ? (Numerator + f.Numerator) / Denominator : (Numerator * f.Denominator + Denominator * f.Numerator) / (Denominator * f.Denominator);

        /******************************* MULTIPLICATION *******************************/
        //public new Operand Multiply(Operand o) => (IsConstant && !(o is Constant)) ? null : (Numerator * o) / Denominator;

        //public Fraction Multiply(Fraction f) => new Fraction(numerator.Multiply(f.numerator), denominator.Multiply(f.denominator));

        //public new Fraction Multiply(Operand o) {return new Fraction((Term)((numerator * o) as dynamic), denominator); }

        //public Operand Multiply(Fraction f) => (Numerator * f.Numerator) / (Denominator * f.Denominator);

        /******************************* EXPONENTIATION ******************************
        public new Operand Exponentiate(Operand o)
        {
            bool b = false;// RemoveNegative(ref o);
            if (b)
            {
                invert();
                return Exponentiate(o);
            }

            return null;// numerator.Exponentiate(o) / denominator.Exponentiate(o);
        }

        /*public Operand Exponentiate(Constant c)
        {
            if (c.Value < 0)
            {
                return Construct(1, this) ^ (new Constant(System.Math.Abs(c.Value)));
            }
            if (c.Value == 1)
            {
                return this;
            }
            return new Fraction(Numerator ^ c, Denominator ^ c);
        }

        public override bool Equals(object obj)
        {
            return (obj as Fraction)?.numerator == numerator && (obj as Fraction).denominator == denominator;
        }

        public override string ToString()
        {
            string bottom = denominator.ToString();
            return "(" + numerator.ToString() + ")" + ((bottom == "1") ? "" : ("/(" + denominator.ToString() + ")"));
        }
    }*/
}