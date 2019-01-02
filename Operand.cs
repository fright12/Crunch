using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Crunch
{
    public class Form : IEnumerable
    {
        public Polynomials PolynomialForm;
        public Numbers NumberForm;
        public Trigonometry TrigonometryForm;

        public Form(Polynomials polynomialForm, Numbers numberForm, Trigonometry trigonometryForm)
        {
            PolynomialForm = polynomialForm;
            NumberForm = numberForm;
            TrigonometryForm = trigonometryForm;
        }

        public IEnumerator GetEnumerator()
        {
            yield return PolynomialForm;
            yield return NumberForm;
        }
    }

    public enum Polynomials { Factored, Expanded }
    public enum Numbers { Exact, Decimal }
    public enum Trigonometry { Degrees, Radians }

    public enum Triple { Unchecked, Yes, No }

    internal abstract class Simplifier<T1, T2> : Simplifier<T1>
    {
        Simplifier<T2> t2;

        public Simplifier(Simplifier<T1> t1, Simplifier<T2> t2) : base(t1)
        {
            this.t2 = t2;
        }
    }

    internal abstract class Simplifier<T1>
    {
        Simplifier<T1> t1;

        public Simplifier(Simplifier<T1> t1)
        {
            this.t1 = t1;
        }
    }

    internal interface ISimplifier<T>
    {
        Operand Simplify(T t);
    }

    public sealed class Operand : IComparable<Operand>
    {
        private Simplifier os;

        public Operand Simplify(Polynomials p, Numbers n, Trigonometry t, Dictionary<char, Operand> dict)
        {
            os = new Simplifier(p, n, t, dict);

            print.log("formatting");
            Operand ans = os.Simplify(this);

            if (os.CanFactor)
            {
                PossibleForms[Polynomials.Factored] = true;
            }
            if (os.CanExpand)
            {
                PossibleForms[Polynomials.Expanded] = true;
            }

            if (os.ts.HasExactForm)
            {
                PossibleForms[Numbers.Exact] = true;
            }

            HasTrig = os.ts.tfs.HasTrig;

            return ans;
        }



        //Mess
        public bool IsNegative => (Value is Term && (Value as Term).Coefficient < 0) || (Value is Expression && (Value as Expression).IsNegative);

        public bool IsConstant() => (Value as dynamic)?.IsConstant() ?? false;
        public bool IsConstant(double d)
        {
            double constant;
            return IsConstant(out constant) && constant == d;
        }
        public bool IsConstant(out double v) { v = double.NaN; return (Value as dynamic).IsConstant(out v); }

        public bool HasTrig { get; private set; }

        internal Dictionary<Enum, bool> PossibleForms = new Dictionary<Enum, bool>()
        {
            { Numbers.Decimal, true }
        };
        public bool HasForm(Enum e) => PossibleForms.ContainsKey(e) && PossibleForms[e];
        //Mess



        public HashSet<char> Unknowns => os?.ts?.vs?.Unknowns ?? new HashSet<char>();

        internal Term TermForm => (Term)(Value as dynamic);
        internal Expression ExpressionForm => (Expression)(Value as dynamic);

        private object Value;

        internal Operand(Expression e) => Value = e;
        internal Operand(Term t) => Value = t;

        public static implicit operator Operand(double d) => new Operand(d);

        public static Operand operator +(Operand o1, Operand o2) { o1.Copy().Add(o2.Copy()); return o1; }
        public static Operand operator -(Operand o1, Operand o2) { o1.Copy().Subtract(o2.Copy()); return o1; }
        public static Operand operator *(Operand o1, Operand o2) { o1.Copy().Multiply(o2.Copy()); return o1; }
        public static Operand operator /(Operand o1, Operand o2) { o1.Copy().Divide(o2.Copy()); return o1; }
        public static Operand operator ^(Operand o1, Operand o2) { o1.Copy().Exponentiate(o2.Copy()); return o1; }

        internal void Add(Operand other)
        {
            Expression e = ExpressionForm;
            e.Add(other.ExpressionForm);
            Value = e;
        }
        internal void Subtract(Operand other)
        {
            other.Multiply(-1);
            Add(other);
        }
        internal void Multiply(Operand other)
        {
            Term t = TermForm;
            t.Multiply(other.TermForm);
            Value = t;
        }
        internal void Divide(Operand other)
        {
            other.Exponentiate(-1);
            Multiply(other);
        }
        internal void Exponentiate(Operand other)
        {
            Term t = TermForm;
            t.Exponentiate(other);
            Value = t;
        }

        public Polynomials GetPolynomials => Value is Term ? Polynomials.Factored : Polynomials.Expanded;

        public Operand Format(Polynomials polynomials, Numbers numbers, Trigonometry trig, Dictionary<char, Operand> knowns = null)
        {
            Form f = new Form(polynomials, numbers, trig);

            //Check to see if we already know we can't do this form
            foreach (Enum e in f)
            {
                bool valid;
                //If the key is in the dictionary yet, it's already been checked - if the value is false, we know we can't do this form
                if (PossibleForms.TryGetValue(e, out valid) && !valid)
                {
                    return null;
                }
            }

            foreach (Enum e in f)
            {
                //If we haven't checked it yet, assume we can't get this form - we'll have to find something that says otherwise
                if (!PossibleForms.ContainsKey(e))
                {
                    PossibleForms[e] = false;
                }
            }

            print.log("formatting");
            Operand ans = Simplify(polynomials, numbers, trig, knowns);

            //Make sure we found every form we were looking for
            foreach (Enum e in f)
            {
                //We never found anything that could be formatted this way - therefore the form doesn't exist
                if (!PossibleForms[e])
                {
                    return null;
                }
            }

            return ans;
        }

        public Operand Copy() => (Value as dynamic).Copy();

        public override string ToString() => (Value as dynamic).ToString();

        public override int GetHashCode() => (Value as dynamic).GetHashCode();

        public override bool Equals(object obj) => obj is Operand && (Value as dynamic).Equals((obj as Operand).Value as dynamic);

        public int CompareTo(Operand other) => Value is Term ? (Value as Term).CompareTo(other.TermForm) : (Value as Expression).CompareTo(other.ExpressionForm);

        internal class Simplifier : ISimplifier<Operand>
        {
            public bool CanFactor = false;
            public bool CanExpand = false;

            public Term.Simplifier ts;
            private Polynomials p;

            public Simplifier(Term.Simplifier ts, Polynomials p)
            {
                this.ts = ts;
                this.p = p;
            }

            public Simplifier(Polynomials p, Numbers n, Trigonometry t, Dictionary<char, Operand> dict)
            {
                this.ts = new Term.Simplifier(new Variable.Simplifier(this, dict, n), new TrigFunction.Simplifier(t, this), this, n);
                this.p = p;
            }

            public Operand Simplify(Operand o)
            {
                o = o.Copy();

                if (p == Polynomials.Factored)
                {
                    Term factored;

                    if (o.Value is Term)
                    {
                        factored = o.Value as Term;
                    }
                    else
                    {
                        Term.ToTerm(o.Value as Expression, out factored);
                    }

                    if (factored != null)
                    {
                        CanFactor = true;
                        return ts.Simplify(factored);
                    }
                }

                Expression temp = o.ExpressionForm;

                if (temp != null)
                {
                    CanExpand = true;
                    return new Expression.Simplifier(ts).Simplify(temp);
                }
                else
                {
                    return ts.Simplify(o.Value as Term);
                }
            }
        }
    }
}