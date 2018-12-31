using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Crunch.Engine
{
    public class RestrictedHashSet<T>
    {
        private HashSet<T> hashset;

        public RestrictedHashSet() => hashset = new HashSet<T>();
        public RestrictedHashSet(params T[] items) : this()
        {
            foreach (T t in items)
            {
                hashset.Add(t);
            }
        }

        public int Count => hashset.Count;
        public bool Contains(T item) => hashset.Contains(item);
        public HashSet<T>.Enumerator GetEnumerator() => hashset.GetEnumerator();

        internal bool Add(T item) => hashset.Add(item);
        internal bool Remove(T item) => hashset.Remove(item);
        internal void Clear() => hashset.Clear();
    }

    public enum Polynomials { Factored, Expanded }
    public enum Numbers { Exact, Decimal }
    public enum Trigonometry { Degrees, Radians }

    public sealed class Operand
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

        public Dictionary<char, Operand> Knowns = new Dictionary<char, Operand>();
        public RestrictedHashSet<char> Unknowns = new RestrictedHashSet<char>();

        public bool HasTrig { get; internal set; }

        internal Dictionary<Enum, bool> PossibleForms = new Dictionary<Enum, bool>()
        {
            { Numbers.Decimal, true }
        };
        public bool HasForm(Enum e) => PossibleForms.ContainsKey(e) && PossibleForms[e];

        internal static Operand Instance;

        internal Expression value;

        internal Operand(Expression e) => value = e;

        public static implicit operator Operand(double d) => new Operand(d);

        internal static Expression Add(Expression e1, Expression e2) => Expression.Add(e1, e2);
        internal static Expression Subtract(Expression e1, Expression e2) => Expression.Add(e1, Expression.Distribute(e2, -1));
        internal static Expression Multiply(Expression e1, Expression e2) => Term.Multiply(e1, e2);
        internal static Expression Divide(Expression e1, Expression e2) => Term.Multiply(e1, Term.Exponentiate(e2, -1));
        internal static Expression Exponentiate(Expression e1, Expression e2) => Term.Exponentiate(e1, e2);

        internal static Expression FilterIdentity(double identity, Func<List<Expression>, Expression> operation, Expression[] expressions)
        {
            List<Expression> list = new List<Expression>();

            foreach (Expression e in expressions)
            {
                if (!e.IsConstant(identity))
                {
                    list.Add(e);
                }
            }

            if (list.Count == 0)
            {
                return identity;
            }
            else if (list.Count == 1)
            {
                return list[0];
            }
            else
            {
                return operation(list);
            }
        }

        public static Operand operator +(Operand o1, Operand o2) => Add(o1.value.Copy(), o2.value.Copy());
        public static Operand operator -(Operand o1, Operand o2) => Subtract(o1.value.Copy(), o2.value.Copy());
        public static Operand operator *(Operand o1, Operand o2) => Multiply(o1.value.Copy(), o2.value.Copy());
        public static Operand operator /(Operand o1, Operand o2) => Divide(o1.value.Copy(), o2.value.Copy());
        public static Operand operator ^(Operand o1, Operand o2) => Exponentiate(o1.value.Copy(), o2.value.Copy());

        public Polynomials Polynomials => value.Terms.Count == 1 ? Polynomials.Factored : Polynomials.Expanded;

        public Operand Format(Polynomials polynomials, Numbers numbers, Trigonometry trig)
        {
            Instance = this;

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

            Unknowns.Clear();
            print.log("formatting");
            Operand ans = value.Format(f);

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

        public override string ToString() => value.ToString();

        public override int GetHashCode() => value.GetHashCode();

        public override bool Equals(object obj) => (obj is Operand && value.Equals((obj as Operand).value)) || (obj is Expression && value.Equals(obj as Expression));
    }
}