using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Crunch.Engine
{
    /*internal abstract class Operable
    {
        public abstract Expression AsExpression();
        public abstract Term AsTerm();

        //public static Operable Subtract(Operable o1, Operable o2) => Expression.Add(o1, Term.Multiply(o2, -1));
        //public static Term Divide(Operable o1, Operable o2) => Term.Multiply(o1, Term.Exponentiate(o2, -1));
        //public static Term Exponentiate(Operable o1, Operable o2) => Term.ex
    }*/

    public enum Polynomials { Factored, Expanded }
    public enum Numbers { Exact, Decimal }
    public enum Trigonometry { Degrees, Radians }

    public enum Choice { True, False, Unchecked }

    public class Options : IEnumerator<Choice>
    {
        public Choice Current => throw new NotImplementedException();
        object IEnumerator.Current => Current;

        private Choice[] choices;
        private int choosen = 0;

        public Options(params Choice[] choices) => this.choices = choices;

        public void Dispose() { }

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    public class Combinations<T>
    {
        private List<Type> list;
        private Dictionary<int, T> dict;

        public T this[params Enum[] enums]
        {
            get { return dict[Index(enums)]; }
            set { dict[Index(enums)] = value; }
        }

        public Combinations(params Type[] types)
        {
            list = new List<Type>();
            dict = new Dictionary<int, T>();

            foreach (Type t in types)
            {
                list.Add(t);
            }
        }

        public bool ContainsCombination(params Enum[] enums)
        {
            int index = Index(enums);

            if (index == -1)
            {
                return false;
            }
            else
            {
                return dict.ContainsKey(index);
            }
        }

        public void RemoveChoice(Enum e)
        {
            for (int i = 0; i < System.Math.Pow(2, list.Count - 1); i++)
            {
                int power = list.IndexOf(e.GetType());
                if (power == -1)
                {
                    return;
                }

                int index = (int)System.Math.Pow(2, power);
                dict.Remove(i + i / index * index + index * (int)(dynamic)e);
            }
        }

        private int Index(params Enum[] enums)
        {
            if (enums.Length != list.Count)
            {
                return -1;
            }

            int index = 0;
            foreach (Enum e in enums)
            {
                int power = list.IndexOf(e.GetType());

                if (power == -1)
                {
                    return -1;
                }

                index += (int)System.Math.Pow(2, power) * (int)(dynamic)e;
            }

            return index;
        }
    }

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
            
            return list.Count == 1 ? list[0] : operation(list);
        }

        /*internal static Expression Add(dynamic o1, dynamic o2) => ((Expression)o1).Add((Expression)o2);
        internal static Expression Subtract(dynamic o1, dynamic o2) => Add(o1, Multiply(o2, -1));
        internal static Term Multiply(dynamic o1, dynamic o2) => ((Term)o1).Multiply((Term)o2);
        internal static Term Divide(dynamic o1, dynamic o2) => Multiply(o1, Exponentiate(o2, -1));
        internal static Term Exponentiate(dynamic o1, dynamic o2) => ((Term)o1).Exponentiate((Expression)o2);*/

        public static Operand operator +(Operand o1, Operand o2) => Add(o1.value.Copy(), o2.value.Copy());// o1.value.Copy().Add(o2.value.Copy());
        public static Operand operator -(Operand o1, Operand o2) => Subtract(o1.value.Copy(), o2.value.Copy());
        public static Operand operator *(Operand o1, Operand o2) => Multiply(o1.value.Copy(), o2.value.Copy());// o1.value.Copy().Multiply(o2.value.Copy());
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

            //print.log("formatting", polynomials, numbers, trig);
            Unknowns.Clear();
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

        //public Operand Format(bool factored, bool fraction, bool deg, Dictionary<char, Operand> knowns = null) => value.Format(factored: factored, fraction: fraction, deg: deg, knowns: knowns);

        public override string ToString() => value.ToString();
    }

    /*public abstract class Operand
    {
        public static implicit operator Operand(double d) => new Term(d);

        public static Operand operator +(Operand o1, Operand o2) => o1.Add(o2 as dynamic) ?? o2.Add(o1 as dynamic) ?? throw new Exception("Cannont add operands of type " + o1.GetType() + " and " + o2.GetType());
        public static Operand operator -(Operand o1, Operand o2) => o1 + -1 * o2;
        public static Operand operator *(Operand o1, Operand o2) => o1.Multiply(o2 as dynamic) ?? o2.Multiply(o1 as dynamic) ?? throw new Exception("Cannont add operands of type " + o1.GetType() + " and " + o2.GetType());
        public static Operand operator /(Operand o1, Operand o2) => o1 * (o2 ^ -1);
        public static Operand operator ^(Operand o1, Operand o2) => ((Term)(o1 as dynamic)).Exponentiate(o2);

        internal abstract Operand Add(Term other);
        internal abstract Operand Add(Expression other);
        private Operand Add(Operand other) => null;

        internal abstract Operand Multiply(Term other);
        internal abstract Operand Multiply(Expression other);
        private Operand Multiply(Operand other) => null;

        public abstract List<Operand> AllFormats(Dictionary<char, Operand> knowns = null);
        internal abstract List<char> Unknowns();
        internal abstract Operand Copy();

        /*private static Operand add<T1, T2>(T1 o1, T2 o2) => (o1 as Operand<T2>)?.Add(o2) ?? (o2 as Operand<T1>)?.Add(o1) ?? throw new Exception("Cannont add operands of type " + typeof(T1) + " and " + typeof(T2));
        private static Operand multiply<T1, T2>(T1 o1, T2 o2) => (o1 as Operand<T2>)?.Multiply(o2) ?? (o2 as Operand<T1>)?.Multiply(o1) ?? throw new Exception("Cannont multiply operands of type " + typeof(T1) + " and " + typeof(T2));*/

        //internal bool IsConstant(oper => (this is Term && (this as Term).IsConstant);
        //internal bool IsNegative { get; }

        /*protected abstract Operand Multiply(Constant c);
        protected virtual Operand Multiply(Term t) => t.Multiply(this);
        protected abstract Operand Multiply(Expression e);

        protected abstract Operand Add(Constant c);
        protected abstract Operand Add(Term t);
        protected abstract Operand Add(Expression e);

        /*public virtual Operand Simplify() => this;

        public virtual bool IsNegative() => false;

        public Operand Add(Operand o) => null;
        public Operand Multiply(Operand o) => null;
        public Operand Divide(Operand o) => null;
        public Operand Exponentiate(Operand o) => null;
        public bool RemoveNegative(ref Operand o)
        {
            bool b = o.IsNegative();
            if (b)
            {
                o *= -1;
            }

            return b;
        }

        public static Operand operator +(Operand o1, Operand o2) => (o1 as dynamic).Add(o2 as dynamic) ?? (o2 as dynamic).Add(o1 as dynamic);
        public static Operand operator -(Operand o1, Operand o2) => o1 + (-1 * o2);
        public static Operand operator *(Operand o1, Operand o2) => (o1 as dynamic).Multiply(o2 as dynamic) ?? (o2 as dynamic).Multiply(o1 as dynamic);
        public static Operand operator /(Operand o1, Operand o2) => o1 * (o2 ^ -1);
        public static Operand operator ^(Operand o1, Operand o2) => (o1 as dynamic).Exponentiate(o2 as dynamic);

        public static implicit operator Operand(Variable v) => null;// new Term(v);
        public static implicit operator Operand(double d) => null;// new Term(d);

        public override bool Equals(object obj) => obj.GetType() == GetType() && obj.GetHashCode() == GetHashCode();
        public override int GetHashCode() => ToString().GetHashCode();
    }*/
}