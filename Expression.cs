using System;
using System.Collections.Generic;
using System.Text;

using System.Extensions;

namespace Crunch.Engine
{
    internal class Expression
    {
        public IReadOnlyList<Term> Terms => terms;
        private List<Term> terms = new List<Term>();
        private SortedSet<Term> test = new SortedSet<Term>();

        public bool IsNegative => terms.Count == 1 && terms[0].Coefficient < 0;

        public bool IsConstant(out double d)
        {
            d = 0;
            return terms.Count == 0 || (terms.Count == 1 && terms[0].IsConstant(out d));
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
                terms.Add(t);
            }
        }

        public static implicit operator Operand(Expression e) => new Operand(e);
        public static implicit operator Expression(Term t) => new Expression(t);
        public static implicit operator Expression(double d) => new Expression(new Term(d));

        public static Expression Add(params Expression[] expressions)
        {
            if (expressions.Length == 2)
            {
                print.log("adding expression " + expressions[0] + " and expression " + expressions[1]);
            }

            return Operand.FilterIdentity(0,
                (list) =>
                {
                    Expression ans = new Expression();

                    foreach (Expression e in list)
                    {
                        //If the expression is a wrapped term (an expression with one term), make sure the term doesn't have any hashed expressions (if it does, distribute everything)
                        Expression other = e.terms.Count == 1 ? e.terms[0].ToExpression() : e;

                        for (int i = 0; i < other.terms.Count; i++)
                        {
                            int j = 0;
                            for (; j < ans.terms.Count; j++)
                            {
                                Term t;
                                if (ans.terms[j].TryAdd(other.terms[i], out t))
                                {
                                    ans.terms[j] = t;
                                    if (ans.terms[j].Coefficient == 0)
                                    {
                                        ans.terms.RemoveAt(j--);
                                    }
                                    break;
                                }
                            }

                            if (j == ans.terms.Count)
                            {
                                ans.terms.Add(other.terms[i]);
                            }
                        }
                    }

                    return ans;
                },
                expressions);
        }

        public Expression Format(Operand.Form form)
        {
            /*if (form.PolynomialForm == Polynomials.Factored)
            {
                Term factored;

                if (Term.ToTerm(Copy(), out factored))
                {
                    Operand.Instance.PossibleForms[Polynomials.Factored] = true;
                    return factored.Format(form);
                }
            }

            Expression e = terms.Count == 1 ? terms[0].Copy().ToExpression() : this;

            //The answer is just 1 term
            if (e.terms.Count == 1)
            {
                return e.terms[0].Format(form);
            }
            else
            {
                Operand.Instance.PossibleForms[Polynomials.Expanded] = true;

                Expression ans = new Expression();

                foreach (Term t in e.terms)
                {
                    ans = Add(ans, t.Format(form));
                }

                return ans;
            }*/

            if (form.PolynomialForm == Polynomials.Factored)
            {
                Term factored;
                
                if (Term.ToTerm(Copy(), out factored))
                {
                    Operand.Instance.PossibleForms[Polynomials.Factored] = true;
                }

                return factored.Format(form);
            }
            else
            {
                Expression e = terms.Count == 1 ? terms[0].Copy().ToExpression() : this;

                //The answer is just 1 term
                if (e.terms.Count == 1)
                {
                    return e.terms[0].Format(form);
                }
                else
                {
                    Operand.Instance.PossibleForms[Polynomials.Expanded] = true;

                    Expression ans = new Expression();

                    foreach (Term t in e.terms)
                    {
                        ans = Add(ans, t.Format(form));
                    }

                    return ans;
                }
            }
        }

        public static Expression Distribute(Expression e1, Expression e2)
        {
            if (e2.terms.Count > e1.terms.Count)
            {
                return Distribute(e2, e1);
            }

            Expression ans = new Expression();
            print.log("multiplying expressions " + e1 + " and " + e2);
            foreach (Term t1 in e1.terms)
            {
                for (int i = 0; i < e2.terms.Count; i++)
                {
                    Term t2 = (i == e2.terms.Count - 1) ? t1 : t1.Copy();
                    //print.log(e);
                    //t2.Multiply(other.terms[i].Copy());
                    ans = Add(ans, Term.Multiply(t2, e2.terms[i].Copy()));
                }
            }
            
            return ans;
        }

        public Expression Copy()
        {
            Expression e = new Expression();
            foreach (Term t in terms)
            {
                e.terms.Add(t.Copy() as Term);
            }
            return e;
        }

        /*public static int Compare(Expression x, Expression y)
        {
            List<Term> xterms = new List<Term>(x.terms);
            xterms.Sort(new Term.TermComparer());

            List<Term> yterms = new List<Term>(y.terms);
            yterms.Sort(new Term.TermComparer());

            for (int i = 0; i < System.Math.Max(xterms.Count, yterms.Count); i++)
            {
                if (i >= xterms.Count)
                {
                    return -1;
                }
                else if (i >= yterms.Count)
                {
                    return 1;
                }

                int ithTerm = new Term.TermComparer().Compare(xterms[i], yterms[i]);

                if (ithTerm != 0)
                {
                    return ithTerm;
                }
            }

            return 0;
        }*/

        public override bool Equals(object obj)
        {
            /*if (!(obj is Expression) && !(obj is Term))
            {
                return false;
            }

            Expression other = obj as Expression ?? obj as Term;
            
            if (other.terms.Count != terms.Count)
            {
                return false;
            }
            //print.log("expression comparing " + this + " to " + other);
            List<Term> list1 = new List<Term>(terms);
            list1.Sort(new Term.TermComparer());
            /*print.log("list1");
            foreach (Term t in list1)
                print.log(t);
            List<Term> list2 = new List<Term>(other.terms);
            list2.Sort(new Term.TermComparer());
            /*print.log("list2");
            foreach (Term t in list2)
                print.log(t?.ToString() ?? "null");
            for (int i = 0; i < list1.Count; i++)
            {
                //print.log(";lkajsdf;lsd", i, list1[i]?.ToString() ?? "null", list2[i]?.ToString() ?? "null");
                if (!list1[i].Equals(list2[i]))
                {
                    print.log(i + "th terms are not equal: " + list1[i] + " and " + list2[i]);
                    return false;
                }
            }

            return true;*/

            return obj.GetType() == GetType() && obj.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode() => ToString().GetHashCode();

        public override string ToString()
        {
            if (terms.Count == 0)
            {
                return "0";
            }

            string s = "";
            for (int i = 0; i < terms.Count; i++)
            {
                string temp = terms[i].ToString();
                s += (i > 0 && temp[0] != '-') ? "+" + temp : temp;
            }
            return terms.Count > 1 ? "(" + s + ")" : s;
        }
    }
}