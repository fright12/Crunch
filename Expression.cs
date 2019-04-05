using System;
using System.Collections.Generic;
using System.Text;

using System.Extensions;
using System.Collections;

namespace Crunch
{
    internal class Expression : IComparable<Expression>, IEnumerable<Term>
    {
        public int TermCount => Terms.Count;

        private SortedSet<Term> Terms = new SortedSet<Term>(Comparer<Term>.Create((a, b) => b.CompareTo(a)));

        private Term First
        {
            get
            {
                IEnumerator<Term> itr = Terms.GetEnumerator();
                itr.MoveNext();
                return itr.Current;
            }
        }

        public bool IsNegative => Terms.Count == 1 && First.Coefficient < 0;

        public bool IsConstant(out double d)
        {
            d = 0;
            return Terms.Count == 0 || (Terms.Count == 1 && First.IsConstant(out d));
        }
        public bool IsConstant(double d) => IsConstant((temp) => d == temp);
        public bool IsConstant(Func<double, bool> condition = null)
        {
            double d;
            return IsConstant(out d) && (condition == null || condition(d));
        }

        private Expression(params Term[] list)
        {
            foreach(Term t in list)
            {
                if (t == null)
                {
                    throw new Exception("can't add null");
                }
                Terms.Add(t);
            }
        }

        public static explicit operator Expression(Term t) => t.ToExpression() ?? new Expression(t);
        public static implicit operator Operand(Expression e) => new Operand(e);
        public static implicit operator Expression(double d) => new Expression(new Term(d));

        internal void Add(Expression other)
        {
            foreach (Term t1 in other.Terms)
            {
                int j = 0;
                foreach(Term t2 in Terms)
                {
                    if (t2.TryAdd(t1))
                    {
                        if (t2.Coefficient == 0)
                        {
                            Terms.Remove(t2);
                            j--;
                        }
                        break;
                    }
                    j++;
                }

                if (j == Terms.Count)
                {
                    Terms.Add(t1);
                }
            }
        }

        public static Expression Distribute(Expression e1, Expression e2) => distribute(e1.Terms, e2.Terms);
        public static Expression Distribute(Expression e1, params Term[] terms) => distribute(e1.Terms, terms);
        private static Expression distribute(ICollection<Term> e1, ICollection<Term> e2)
        {
            if (e2.Count > e1.Count)
            {
                return distribute(e2, e1);
            }

            Expression ans = new Expression();
            print.log("multiplying expressions " + e1 + " and " + e2);
            foreach (Term t1 in e1)
            {
                int i = 0;
                foreach (Term t2 in e2)
                {
                    Term t1copy = (i == e2.Count - 1) ? t1 : t1.Copy();
                    t1copy.Multiply(t2.Copy());
                    ans.Add(new Expression(t1copy));
                }
            }

            return ans;
        }

        public Expression Copy()
        {
            Expression e = new Expression();
            foreach (Term t in Terms)
            {
                e.Terms.Add(t.Copy() as Term);
            }
            return e;
        }

        public int CompareTo(Expression other)
        {
            //print.log("comparing expression " + this + " to " + other);
            if (Terms.Count != other.Terms.Count)
            {
                return Terms.Count.CompareTo(other.Terms.Count);
            }

            IEnumerator<Term> itr1 = Terms.GetEnumerator();
            IEnumerator<Term> itr2 = other.Terms.GetEnumerator();
            while (itr1.MoveNext())
            {
                itr2.MoveNext();
                
                int compare = itr1.Current.CompareTo(itr2.Current);
                
                if (compare != 0)
                {
                    return compare;
                }
            }
            
            return 0;
        }

        public override bool Equals(object obj)
        {
            Expression other = obj as Expression ?? (Expression)(obj as Term);

            if (other == null)
            {
                return false;
            }
            
            return CompareTo(other) == 0;
        }

        public override int GetHashCode() => ToString().GetHashCode();

        public override string ToString()
        {
            if (Terms.Count == 0)
            {
                return "0";
            }

            string s = "";
            int i = 0;
            foreach (Term t in Terms)
            {
                string temp = t.ToString();
                s += (i++ > 0 && temp[0] != '-') ? "+" + temp : temp;
            }
            return Terms.Count > 1 ? "(" + s + ")" : s;
        }

        public IEnumerator<Term> GetEnumerator() => Terms.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public class Simplifier : ISimplifier<Expression>
        {
            public Term.Simplifier ts;

            private Polynomials p;

            public Simplifier(Term.Simplifier ts)
            {
                this.ts = ts;
            }

            public Operand Simplify(Expression e)
            {
                Operand ans = new Expression();

                foreach (Term t in e.Terms)
                {
                    ans.Add(ts.Simplify(t.Copy()));
                }

                return ans;
            }
        }
    }
}