using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Extensions;

namespace Crunch
{
    using Pair = KeyValuePair<object, Operand>;

    internal class Term : IComparable<Term>
    {
        public double Coefficient
        {
            get { return coefficient[0] / coefficient[1]; }
            private set { coefficient[0] = value; coefficient[1] = 1; }
        }
        public double Numerator
        {
            get { return coefficient[0]; }
            private set { setCoefficient(numerator: value); }
        }
        public double Denominator
        {
            get { return coefficient[1]; }
            private set { setCoefficient(denominator: value); }
        }

        /********************************** REPRESENTATION **********************************/
        private GroupedDictionary<Operand> members = new GroupedDictionary<Operand>();
        private double[] coefficient = new double[2] { 1, 1 };
        /********************************** REPRESENTATION **********************************/

        private int nonConstantKeyCount => members.Count - members.TypeCount(typeof(double));

        public static readonly double MAX_COEFFICIENT = System.Math.Pow(10, 15);
        //public static readonly double MinCoefficient = 0.1;
        private static readonly double TEN = 10;

        public bool IsConstant()
        {
            double d = double.NaN;
            return IsConstant(out d);
        }
        public bool IsConstant(out double value)
        {
            value = double.NaN;

            if (nonConstantKeyCount != 0)
            {
                return false;
            }
            
            value = Coefficient;
            int count = 0;
            foreach(KeyValuePair<double, double> pair in constantKeyValues())
            {
                value *= System.Math.Pow(pair.Key, pair.Value);
                count++;
            }
            
            if (count != members.Count)
            {
                value = double.NaN;
                return false;
            }
            else
            {
                return true;
            }
        }

        public List<KeyValuePair<double, double>> constantKeyValues()
        {
            List<KeyValuePair<double, double>> pairs = new List<KeyValuePair<double, double>>();

            foreach (double d in members.EnumerateKeys<double>())
            {
                double power;
                if (members[d].IsConstant(out power) && Math.CanExponentiate(d, power))
                {
                    pairs.Add(new KeyValuePair<double, double>(d, power));
                }
            }

            return pairs;
        }

        public Term(double d) => Numerator = d;
        public Term(string number) => Numerator = trimZeroes(number);
        public Term(char c) => multiply(c);
        public Term(Function f) => multiply(f);
        private Term() { }

        public static implicit operator Operand(Term t) => new Operand(t);

        public static Term Parse(string s)
        {
            if (s.Length == 1 && !Machine.StringClassification.IsNumber(s))
            {
                return new Term(s[0]);
            }
            else
            {
                return new Term(s);
            }
        }

        public Expression ToExpression()
        {
            if (members.TypeCount(typeof(Expression)) > 0)
            {
                List<Expression> list = new List<Expression>(members.TypeCount(typeof(Expression)));

                foreach (Expression e in members.EnumerateKeys<Expression>())
                {
                    if (members[e].IsConstant(1))
                    {
                        list.Add(e);
                    }
                    else
                    {
                        return null;
                    }
                }

                Expression ans = list[0];
                for (int i = 1; i < list.Count; i++)
                {
                    ans = Expression.Distribute(ans, list[i]);
                    //ans.Distribute(list[i]);
                }

                members.RemoveType(typeof(Expression));

                ans = Expression.Distribute(ans, this);
                //ans = ans.Distribute(this);
                
                return ans.Copy();
            }
            else
            {
                return null;
            }
        }

        public static explicit operator Term(Expression e)
        {
            Term factored;

            if (ToTerm(e, out factored))
            {
                return factored;
            }

            factored = new Term();
            factored.multiply(e);
            return factored;
        }

        public static bool ToTerm(Expression e, out Term ans)
        {
            List<Term> terms = new List<Term>();
            foreach(Term t in e)
            {
                terms.Add(t);
            }

            if (terms.Count == 0)
            {
                ans = new Term(0);
            }
            else if (terms.Count == 1)
            {
                ans = terms[0];
            }
            else
            {
                ans = new Term();

                int gcd = 0;
                for (int i = 0; i < terms.Count; i++)
                {
                    if (!terms[i].Coefficient.IsInt())
                    {
                        gcd = 1;
                        break;
                    }
                    else
                    {
                        gcd = (int)Math.GCD(gcd, terms[i].Coefficient);
                    }
                }

                int lcm = (int)terms[0].Denominator;
                for (int i = 1; i < terms.Count; i++)
                {
                    lcm = lcm * (int)terms[i].Denominator / (int)Math.GCD(lcm, terms[i].Denominator);
                }

                ans.setCoefficient(gcd, lcm);

                foreach (Pair pair in terms.Last().members.KeyValuePairs())
                {
                    double max = double.MaxValue;
                    foreach (Term t in terms)
                    {
                        //Operand exp = null;
                        //if (t.members.ContainsKey(pair.Key) && (exp = t.members[pair.Key]).IsConstant((d) => d.IsInt()))
                        double d;
                        if (t.members.ContainsKey(pair.Key) && t.members[pair.Key].IsConstant(out d) && d.IsInt())
                        {
                            max = System.Math.Min(max, d);
                            //max = System.Math.Min(max, exp.value.Terms[0].Numerator);
                        }
                        else
                        {
                            max = double.MaxValue;
                            break;
                        }
                    }

                    if (!(max is double.MaxValue))
                    {
                        ans.multiply(pair.Key is Expression ? (pair.Key as Expression).Copy() : pair.Key, max);
                    }
                }

                double factor;
                //The only thing that factors out is a 1
                if (ans.IsConstant(out factor) && factor == 1) // ans.members.Count > 0 || ans.Coefficient != 1)
                {
                    ans.multiply(e);
                    return false;
                }
                else
                {
                    //e.Distribute(ans.Copy().Exponentiate(-1));
                    Term copy = ans.Copy();
                    copy.Exponentiate(-1);
                    e = Expression.Distribute(e, copy);
                }

                ans.multiply(e);
            }

            return true;

            /*//e = e.Multiply(ans.Exponentiate(-1));
            //ans.multiply(e);
            //return ans;

            Term term = null;// e.Terms.Last().Copy() as Term;
            for (int i = 0; i < e.Terms.Count - 1; i++)
            {
                //term = GCD(term, e.Terms[i]);
            }

            //Cancel the gcd from the expression - not necessary if the gcd is 1 term (because then the gcd is the expressions only term)
            print.log("making term from expression", e, e.Terms.Count, term);
            if (term.members.Count > 0)
            {
                //gcd.exponentiate(-1);
                //e = e.Multiply(gcd);
                //gcd.exponentiate(-1);
            }
            //term.Multiply((Term)e);

            //return term;*/
        }

        public Term Copy()
        {
            Term t = new Term();
            t.coefficient = new double[2] { coefficient[0], coefficient[1] };
            
            foreach (Pair pair in members.KeyValuePairs())
            {
                t.members.Add((pair.Key as Expression)?.Copy() ?? pair.Key, pair.Value.Copy());
            }
            return t;
        }

        internal void Multiply(Term other)
        {
            Print.Log("multiplying terms " + this + " and " + other);

            double constant;
            if (IsConstant(out constant) && constant == 1)
            {
                Clone(other);
                return;
            }
            else if (constant == 0 || (other.IsConstant(out constant) && constant == 0))
            {
                members = new GroupedDictionary<Operand>();
                coefficient = new double[] { 0, 1 };
                return;
            }
            else if (constant == 1)
            {
                return;
            }

            foreach (Pair pair in other.members.KeyValuePairs())
            {
                multiply(pair.Key, pair.Value);
            }
            
            setCoefficient(safelyMultiply(Numerator, other.Numerator), safelyMultiply(Denominator, other.Denominator));
        }

        /******************************* EXPONENTIATION *******************************/

        internal void Exponentiate(Operand exponent)
        {
            Print.Log("exponentiating " + this, exponent);

            double baseConstant;
            double expConstant;

            bool constantBase = IsConstant(out baseConstant);
            bool constantExp = exponent.IsConstant(out expConstant);

            //0 ^ something
            if (constantBase && baseConstant == 0)
            {
                //0 in the denominator is undefined
                if (exponent.IsNegative)
                {
                    throw new DivideByZeroException();
                }
                //0 ^ anything is 0
                else
                {
                    members = new GroupedDictionary<Operand>();
                    coefficient = new double[] { 0, 1 };
                    return;
                }
            }
            //1 ^ anything or anything ^ 0 is 1
            else if ((constantBase && baseConstant == 1) || (constantExp && expConstant == 0))
            {
                members = new GroupedDictionary<Operand>();
                coefficient = new double[] { 1, 1 };
                return;
            }
            //anything ^ 1 is itself
            else if (constantExp && expConstant == 1)
            {
                return;
            }

            Term t = new Term();

            for (int i = 0; i < 2; i++)
            {
                if (coefficient[i] != 1)
                {
                    if (constantExp && Math.CanExponentiate(coefficient[i], System.Math.Abs(expConstant)))
                    {
                        int sign = 1;
                        if (expConstant > 0 && expConstant < 1 && 1 / expConstant % 2 == 1 && coefficient[i] < 0)
                        {
                            sign = -1;
                            coefficient[i] *= -1;
                        }

                        double temp = sign * System.Math.Pow(coefficient[i], System.Math.Abs(expConstant));

                        if (double.IsNaN(temp))
                        {
                            throw new Exception("Imaginary answer");
                        }

                        t.coefficient[(i == 0 ^ expConstant > 0).ToInt()] = System.Math.Round(temp, 14);
                    }
                    else
                    {
                        Operand p = exponent.Copy();
                        if (i == 1)
                        {
                            p.Multiply(-1);
                        }
                        t.multiply(coefficient[i], p);
                    }
                }
            }

            foreach (Pair pair in members.KeyValuePairs())
            {
                pair.Value.Multiply(exponent);
                t.multiply(pair.Key, pair.Value);
            }

            t.setCoefficient(t.coefficient[0], t.coefficient[1]);
            Clone(t);
        }

        /*internal void exponentiate(Operand exponent)
        {
            print.log("exponentiating " + this, exponent);

            double baseConstant;
            double expConstant;

            bool constantBase = IsConstant(out baseConstant);
            bool constantExp = exponent.IsConstant(out expConstant);

            //0 ^ something
            if (constantBase && baseConstant == 0)
            {
                //0 in the denominator is undefined
                if (exponent.IsNegative)
                {
                    throw new DivideByZeroException();
                }
                //0 ^ anything is 0
                else
                {
                    members = new GroupedDictionary<Operand>();
                    coefficient = new double[] { 0, 1 };
                    return;
                }
            }
            //1 ^ anything or anything ^ 0 is 1
            else if ((constantBase && baseConstant == 1) || (constantExp && expConstant == 0))
            {
                members = new GroupedDictionary<Operand>();
                coefficient = new double[] { 1, 1 };
                return;
            }
            //anything ^ 1 is itself
            else if (constantExp && expConstant == 1)
            {
                return;
            }

            double[] newCoefficient = new double[2];
            for (int i = 0; i < 2; i++)
            {
                if (coefficient[i] != 1)
                {
                    if (constantExp && Math.CanExponentiate(coefficient[i], System.Math.Abs(expConstant)))
                    {
                        //Handle negative constants to odd fraction powers (ie (-4)^(1/3))
                        int sign = 1;
                        if (expConstant > 0 && expConstant < 1 && 1 / expConstant % 2 == 1 && coefficient[i] < 0)
                        {
                            sign = -1;
                            coefficient[i] *= -1;
                        }

                        double temp = sign * System.Math.Pow(coefficient[i], System.Math.Abs(expConstant));

                        if (double.IsNaN(temp))
                        {
                            throw new Exception("Imaginary answer");
                        }

                        newCoefficient[(i == 0 ^ expConstant > 0).ToInt()] = System.Math.Round(temp, 14);
                    }
                    else
                    {
                        Operand p = exponent.Copy();
                        if (i == 1)
                        {
                            p.Multiply(-1);
                        }
                        multiply(coefficient[i], p);
                    }
                }
            }

            foreach (Pair pair in members.KeyValuePairs())
            {
                pair.Value.Multiply(exponent);
            }

            setCoefficient(newCoefficient[0], newCoefficient[1]);
        }*/

        private void Clone(Term t)
        {
            members = t.members;
            coefficient = t.coefficient;
        }

        public bool TryAdd(Term t)
        {
            Print.Log("adding terms " + this + " and " + t);

            double constant;
            if (IsConstant(out constant) && constant == 0)
            {
                Clone(t);
                return true;
            }
            else if (t.IsConstant(out constant) && constant == 0)
            {
                return true;
            }

            bool isLike = members.Count - constantKeyValues().Count == t.members.Count - t.constantKeyValues().Count;
            bool isFraction = false;

            foreach (Pair pair in members.KeyValuePairs())
            {
                double d;
                if (isLike && !(pair.Key is double && pair.Value.IsConstant(out d) && Math.CanExponentiate((double)pair.Key, d)))
                {
                    isLike = t.members.ContainsKey(pair.Key) && pair.Value.ToString() == t.members[pair.Key].ToString();
                }
                isFraction = isFraction || pair.Value.IsNegative;
            }

            if (!isLike && !isFraction)
            {
                foreach (Pair pair in t.members.KeyValuePairs())
                {
                    isFraction = isFraction || pair.Value.IsNegative;
                }
            }

            //Like and not fraction -- add numerator constants
            //Like and fraction -- add constants as fraction
            //Not like and not fraction -- new expression
            //Not like and fraction -- add everything as fraction

            if (isLike)
            {
                double d1 = Numerator * t.Denominator;// safelyMultiply(Numerator, t.Denominator);
                double d2 = t.Numerator * Denominator;// safelyMultiply(t.Numerator, Denominator);
                double d3 = Denominator * t.Denominator;// safelyMultiply(Denominator, t.Denominator);

                for (int i = 0; i < 2; i++)
                {
                    Term temp = i == 0 ? this : t;
                    IEnumerable<KeyValuePair<double, double>> collection = temp.constantKeyValues();

                    foreach (KeyValuePair<double, double> pair in collection)
                    {
                        double d = System.Math.Pow(pair.Key, System.Math.Abs(pair.Value));

                        if (i == 0 ^ pair.Value > 0)
                        {
                            d2 *= d;
                            //d2 = safelyMultiply(d2, d);
                        }
                        else
                        {
                            d1 *= d;
                            //d1 = safelyMultiply(d1, d);
                        }

                        if (pair.Value < 0)
                        {
                            d3 *= d;
                            //d3 = safelyMultiply(d3, d);
                        }

                        temp.members.Remove(pair.Key);
                    }
                }

                setCoefficient(d1 + d2, d3);
            }
            else if (isFraction && nonConstantKeyCount > 0 && t.nonConstantKeyCount > 0)
            {
                Term[] numerator = new Term[] { new Term(), new Term() };
                Term denominator = new Term();

                for (int i = 0; i < 2; i++)
                {
                    Term term = i == 0 ? this : t;

                    foreach (Pair pair in term.members.KeyValuePairs())
                    {
                        if (pair.Value.IsNegative)
                        {
                            denominator.multiply(pair.Key is Expression ? (pair.Key as Expression).Copy() : pair.Key, pair.Value.Copy());

                            pair.Value.Multiply(-1);
                            numerator[1 - i].multiply(pair.Key, pair.Value);
                        }
                        else
                        {
                            numerator[i].multiply(pair.Key, pair.Value);
                        }
                    }
                }
                
                numerator[0].Numerator = numerator[0].safelyMultiply(Numerator, t.Denominator);
                numerator[1].Numerator = numerator[1].safelyMultiply(Denominator, t.Numerator);
                denominator.Denominator = denominator.safelyMultiply(Denominator, t.Denominator);

                Operand o = numerator[0];
                o.Add(numerator[1]);

                Term top = o.TermForm;
                top.Multiply(denominator);

                Clone(top);
            }
            else
            {
                return false;
            }

            return true;
        }

        /******************************* MULTIPLICATION *******************************/
        private double safelyMultiply(double d1, double d2)
        {
            if (d1 * d2 >= MAX_COEFFICIENT)
            {
                return (d1.IsInt() && d1 % 10 == 0 ? trimZeroes(d1.ToString()) : d1) * (d2.IsInt() && d2 % 10 == 0 ? trimZeroes(d2.ToString()) : d2);
            }
            else
            {
                return d1 * d2;
            }
        }

        private double trimZeroes(string num)
        {
            if (num == "0")
            {
                return 0;
            }

            int power = 0;
            while (num.Length - 1 - power >=0 && num[num.Length - 1 - power] == '0')
            {
                power++;
            }

            double d = double.Parse(num.Substring(0, num.Length - power));

            //if (d.IsInt() && num.Last() != '.' && power > 0)
            if (!num.Contains("."))
            {
                multiply(10, power);
            }
            
            return d;
        }

        private void setCoefficient(double numerator = double.NaN, double denominator = double.NaN)
        {
            for (int i = 0; i < 2; i++)
            {
                double d = i == 0 ? numerator : denominator;

                if (double.IsNaN(d))
                {
                    continue;
                }

                string num = d.ToString();

                //Fix doubles with 'E' representation
                if (num.Contains("E"))
                {
                    d = double.Parse(num.Substring(0, num.IndexOf("E")));
                    double power = (i * -2 + 1) * double.Parse(num.Substring(num.IndexOf("E") + 1));

                    multiply(TEN, power);
                }

                if (d != 0)
                {
                    double powerOf10 = 1;
                    bool scientificNotation = members.ContainsKey(TEN) && members[TEN].IsConstant(out powerOf10);
                    int places = (int)System.Math.Floor(System.Math.Log10(System.Math.Abs(d)));

                    //If the number is currently being represented in scientific notation, make sure it needs to be
                    if (scientificNotation)
                    {
                        powerOf10 *= i * -2 + 1;

                        //Multiply the power of 10 into the coefficient if it won't be too large
                        if (powerOf10 + places >= 0 && powerOf10 + places < 15)
                        {
                            d *= System.Math.Pow(10, powerOf10);
                            members.Remove(TEN);
                            scientificNotation = false;
                        }
                    }

                    //If we can't get rid of the power of 10 or the coefficient is too small to be rounded, make sure we have correctly formatted scientific notation
                    if ((scientificNotation && !d.IsInt() && places != 0) || (System.Math.Abs(d) < 0.1))
                    {
                        d /= System.Math.Pow(10, places);
                        multiply(TEN, places * (i * -2 + 1));
                    }
                }

                coefficient[i] = d;
            }

            if (coefficient[0].IsInt() && coefficient[1].IsInt() && coefficient[0] != 1 && coefficient[1] != 1)
            {
                int gcd = 1;

                //Divide out the GCD
                if ((gcd *= (int)Math.GCD(coefficient[0], coefficient[1])) != 1)
                {
                    coefficient[0] /= gcd;
                    coefficient[1] /= gcd;
                }
            }
            
            checkForHashedCoefficients();
        }

        private void checkForHashedCoefficients()
        {
            for (int i = 0; i < 2; i++)
            {
                if (members.ContainsKey(coefficient[i]))
                {
                    members[coefficient[i]].Add(i * -2 + 1);
                    coefficient[i] = 1;
                }
            }
        }
        
        private void multiply(object key) => multiply(key, 1);

        private void multiply(object key, Operand value)
        {
            if (key is int)
            {
                key = (double)(int)key;
            }

            if (key is double && (double)key == 1)
            {
                return;
            }
            
            if (!members.ContainsKey(key))
            {
                members.Add(key, value);
            }
            else
            {
                members[key].Add(value);
            }

            checkForHashedCoefficients();

            double power;
            if (members[key].IsConstant(out power))
            {
                //Anything ^ 0 is 1
                if (power == 0)
                {
                    members.Remove(key);
                }
                else if (key is double && (double)key != 10 && Math.CanExponentiate((double)key, power))
                {
                    members.Remove(key);

                    Term t = new Term((double)key);
                    t.Exponentiate(power);
                    Multiply(t);
                }
            }
        }

        /******************************* ADDITION *******************************/

        private bool IsLike(Term other)
        {
            if (other.members.Count != members.Count)
            {
                return false;
            }

            foreach (Type t in Order)
            {
                if (members.TypeCount(t) != other.members.TypeCount(t))
                {
                    return false;
                }
            }

            foreach (Pair pair in other.members.KeyValuePairs())
            {
                Operand exponent;
                if (!members.TryGetValue(pair.Key, out exponent) || !exponent.Equals(pair.Value))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode() => ToString().GetHashCode();

        /*public override bool Equals(object obj)
        {
            //print.log("term comparing " + this + " to " + obj);
            if (obj is Expression)
            {
                return (obj as Expression).Equals(this);
            }

            Term other = obj as Term;

            if (other == null)
            {
                return false;
            }

            string c1 = Coefficient.ToString();
            string c2 = other.Coefficient.ToString();
            int i = 0;
            for (; i < System.Math.Min(c1.Length, c2.Length) - 1; i++)
            {
                if (c1[i] != c2[i])
                {
                    string check = c1[i] == '0' ? c1 : c2;

                    for (int j = i; j < check.Length; j++)
                    {
                        if (check[j] != '0' && check[j] != '.')
                        {
                            return false;
                        }
                    }

                    i--;
                    break;
                }
            }

            return System.Math.Abs(c1[i] - c2[i]) <= 1 && IsLike(other);
        }*/

        private static readonly List<Type> Order = new List<Type>() {
            typeof(double),
            typeof(char),
            typeof(TrigFunction),
            typeof(Function),
            typeof(Expression)
        };

        private int Compare(KeyValuePair<Operand, object> a, KeyValuePair<Operand, object> b)
        {
            int compare = a.Key.CompareTo(b.Key);

            if (compare != 0)
            {
                return compare;
            }

            int index1 = Order.IndexOf(a.Value.GetType());
            int index2 = Order.IndexOf(b.Value.GetType());

            if (index1 != index2)
            {
                return index1.CompareTo(index2);
            }

            return a.Value is char ? ((char)b.Value).CompareTo((char)a.Value) : (a.Value as dynamic).CompareTo(b.Value as dynamic);
        }

        private IEnumerable<KeyValuePair<Operand, object>> SortByExponent()
        {
            SortedSet<KeyValuePair<Operand, object>> sorted = new SortedSet<KeyValuePair<Operand, object>>(
                Comparer<KeyValuePair<Operand, object>>.Create((a, b) => Compare(b, a))
                );

            foreach(Pair pair in members.KeyValuePairs())
            {
                sorted.Add(new KeyValuePair<Operand, object>(pair.Value, pair.Key));
            }

            return sorted;
        }

        public int CompareTo(Term other)
        {
            IEnumerator<KeyValuePair<Operand, object>> itr1 = SortByExponent().GetEnumerator();
            IEnumerator<KeyValuePair<Operand, object>> itr2 = other.SortByExponent().GetEnumerator();
            
            do
            {
                bool b1 = itr1.MoveNext();
                bool b2 = itr2.MoveNext();
                
                if (!b1 && !b2)
                {
                    return Coefficient.CompareTo(other.Coefficient);
                }
                else if (!b1)
                {
                    return -1;
                }
                else if (!b2)
                {
                    return 1;
                }
                
                int compare = Compare(itr1.Current, itr2.Current);// itr1.Current.Key.CompareTo(itr2.Current.Key);
                
                if (compare != 0)
                {
                    return compare;
                }
            }
            while (true);
        }

        public override bool Equals(object obj)
        {
            //print.log("comparing term " + this + " to " + obj);

            Term other = obj as Term ?? (Term)(obj as Expression);

            if (other == null)
            {
                return false;
            }
            
            if (System.Math.Round(Coefficient, 2).ToString() != System.Math.Round(other.Coefficient, 2).ToString())
            {
                return false;
            }

            return IsLike(other);
        }

        public override string ToString()
        {
            if (Coefficient == 0)
            {
                return "0";
            }

            //Sort coefficients into numerator and denominator
            double[] coefficients = new double[2] { coefficient[0], coefficient[1] };

            //Sort all of the variables into numerator and denominator
            string[] variables = new string[2] { "", "" };
            IEnumerator<KeyValuePair<Operand, object>> itr = SortByExponent().GetEnumerator();
            //foreach (Pair pair in members.KeyValuePairs())
            while(itr.MoveNext())
            {
                Pair pair = new Pair(itr.Current.Value, itr.Current.Key);

                string exponent = pair.Value.ToString();
                //Constant values (ie 10^24) should be displayed before any variables
                bool front = pair.Key is double && pair.Value.IsConstant();
                //Should be displayed in the denominator if the exponent is negative, unless the only coefficient is in the numerator (because we want to keep constants together)
                bool denominator = pair.Value.IsNegative && !(pair.Key is double && coefficients[0] != 1 && coefficients[1] == 1);
                bool oneInExponent = exponent == "1" || exponent == "-1";

                //Remove any negative signs from values exponents going in the denominator
                if (denominator)
                {
                    exponent = exponent.Substring(1);
                }
                //If there's no numerator coefficient but there is one in the denominator, display in the denominator instead (because we want to keep constants together)
                else if (pair.Key is double && coefficients[0] == 1 && coefficients[1] != 1)
                {
                    exponent = "-" + exponent;
                    denominator = true;
                }

                string temp = (pair.Key is double ? "*" : "") + pair.Key.ToString();
                //No need to display an exponent if it's 1
                if (!oneInExponent)
                {
                    temp += "^(" + exponent + ")";
                }

                //Sort into numerator/denominator and put constants at the front
                variables[denominator.ToInt()] = front ? temp + variables[denominator.ToInt()] : variables[denominator.ToInt()] + temp;
            }

            /* Create the final fraction - everything above bar goes in index 0, everything below in index 1
             * Display as follows:
             *      No denominator -> coefficients[0] + variables[0]
             *      Denominator is constant -> (coefficients[0] / coefficients[1]) + variables[0]
             *      Denominator contains variables -> (coefficients[0] + variables[0]) / (coefficients[1] + variables[1])
             */

            string[] fraction = new string[2] { "",  variables[1] + ")" };
            for (int i = 0; i < 2; i++)
            {
                double d = System.Math.Round(System.Math.Abs(coefficients[i]), Math.DecimalPlaces);
                fraction[i] = (d == 1 ? "" : d.ToString()) + fraction[i];
            }

            //Decide where numerator variables go (see cases) 
            fraction[(variables[1] == "" && coefficients[1] != 1).ToInt()] += variables[0];
            
            //If nothing is above the bar, show a 1
            if (fraction[0] == "")
            {
                fraction[0] = "1";
            }
            
            for (int i = 0; i < 2; i++)
            {
                fraction[i] = fraction[i].TrimStart('*');
            }

            string sign = Coefficient < 0 ? "-" : "";
            return sign + (fraction[1] == ")" ? fraction[0] : "(" + fraction[0] + ")/(" + fraction[1]);
        }

        public class Simplifier : ISimplifier<Term>
        {
            public bool HasExactForm => hasExactForm || vs.HasExactForm;

            public Variable.Simplifier vs;
            public TrigFunction.Simplifier tfs;
            private Function.Simplifier fs;
            private Operand.Simplifier os;
            private Expression.Simplifier es;

            private Numbers numbers;
            private bool hasExactForm = false;

            public Simplifier(Variable.Simplifier vs, TrigFunction.Simplifier tfs, Operand.Simplifier os, Numbers numbers)
            {
                this.vs = vs;
                this.tfs = tfs;
                fs = new Function.Simplifier(os);
                this.os = os;
                this.es = new Expression.Simplifier(this);
                this.numbers = numbers;
            }

            public Operand Simplify(Term t)
            {
                Print.Log("simplifying " + t);
                Term b = new Term(1);
                b.coefficient[0] = t.coefficient[0];
                b.coefficient[1] = t.coefficient[1];
                Operand ans = b;

                foreach (Pair pair in t.members.KeyValuePairs())
                {
                    Operand o = Simplify(pair.Key as dynamic);
                    o.Exponentiate(os.Simplify(pair.Value));
                    ans.Multiply(o);
                }

                Term a = ans.TermForm;
                bool wholeCoefficients = a.coefficient[0].IsInt() && a.coefficient[1].IsInt();
                if (wholeCoefficients && a.coefficient[1] != 1)
                {
                    hasExactForm = true;
                }

                //Simplify fractions
                if (numbers == Numbers.Decimal || !wholeCoefficients)
                {
                    double d = a.coefficient[1];
                    a.coefficient[1] = 1;
                    a.Numerator = a.coefficient[0] / d;
                }

                Print.Log("simplified " + t + " to " + ans);
                return ans;
            }
            
            private Operand Simplify(char c) => vs?.Simplify(c) ?? new Term(c);
            private Operand Simplify(TrigFunction tf) => tfs?.Simplify(tf) ?? new Term(tf);
            private Operand Simplify(Function f) => fs?.Simplify(f) ?? new Term(f);
            private Operand Simplify(Expression e) => es?.Simplify(e.Copy()) ?? e.Copy();
        }
    }
}
 