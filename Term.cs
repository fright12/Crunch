using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Extensions;

namespace Crunch.Engine
{
    using Pair = KeyValuePair<object, Expression>;

    internal class Term
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

        private GroupedDictionary<Expression> members = new GroupedDictionary<Expression>();
        private int nonConstantKeyCount => members.Count - members.TypeCount(typeof(double));
        private double[] coefficient = new double[2] { 1, 1 };

        public static readonly int digitsPrecision = 15;
        public static readonly double MaxCoefficient = System.Math.Pow(10, 15);
        public static readonly double MinCoefficient = 0.1;
        private static readonly double ten = 10;

        public static bool CanExponentiate(double b, double p) => !double.IsInfinity(System.Math.Pow(b, p));

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
            //double value = 1;
            List<KeyValuePair<double, double>> pairs = new List<KeyValuePair<double, double>>();

            foreach (double d in members.EnumerateKeys<double>())
            {
                double power;
                if (members[d].IsConstant(out power) && CanExponentiate(d, power))
                {
                    //yield return new KeyValuePair<double, double>(d, power);
                    pairs.Add(new KeyValuePair<double, double>(d, power));
                    //value *= System.Math.Pow(d, power);
                }
                /*else if (mustBeConstant)
                {
                    return double.NaN;
                }*/
            }

            return pairs;
            //value *= Coefficient;
            //return value;
        }

        public Term(double d) => Numerator = d;
        public Term(string number) => Numerator = trimZeroes(number);
        public Term(char c) => multiply(c);
        public Term(Function f) => multiply(f);
        private Term() { }

        //public static implicit operator Operand(Term t) => new Operand(t);

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
                        return this;
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
                return this;
            }
        }

        //public static implicit operator Term(double d) => new Term(d);

        public static explicit operator Term(Expression e)
        {
            Term t;
            ToTerm(e, out t);
            return t;
        }

        public static bool ToTerm(Expression e, out Term ans)
        {
            if (e.Terms.Count == 0)
            {
                ans = new Term(0);
            }
            else if (e.Terms.Count == 1)
            {
                ans = e.Terms[0];
            }
            else
            {
                ans = new Term();

                int gcd = 0;
                for (int i = 0; i < e.Terms.Count; i++)
                {
                    if (!e.Terms[i].Coefficient.IsInt())
                    {
                        gcd = 1;
                        break;
                    }
                    else
                    {
                        gcd = (int)GCD(gcd, e.Terms[i].Coefficient);
                    }
                }

                int lcm = (int)e.Terms[0].Denominator;
                for (int i = 1; i < e.Terms.Count; i++)
                {
                    lcm = lcm * (int)e.Terms[i].Denominator / (int)GCD(lcm, e.Terms[i].Denominator);
                }

                ans.setCoefficient(gcd, lcm);

                foreach (Pair pair in e.Terms.Last().members.KeyValuePairs())
                {
                    double max = double.MaxValue;
                    foreach (Term t in e.Terms)
                    {
                        Expression exp = null;
                        if (t.members.ContainsKey(pair.Key) && (exp = t.members[pair.Key]).IsConstant((d) => d.IsInt()))
                        {
                            max = System.Math.Min(max, exp.Terms[0].Numerator);
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
                    e = Expression.Distribute(e, ans.Copy().Exponentiate(-1));
                }

                ans.multiply(e);
            }

            return true;

            //e = e.Multiply(ans.Exponentiate(-1));
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

            //return term;
        }

        /*public List<char> Unknowns()
        {
            List<char> list = new List<char>();

            foreach(char c in members.EnumerateKeys<char>())
            {
                if (!Math.KnownVariables.ContainsKey(c))
                {
                    list.Add(c);
                }
            }

            foreach(Expression e in members.EnumerateKeys<Expression>())
            {
                foreach(Term t in e.Terms)
                {
                    list.AddRange(t.Unknowns());
                }
            }

            return list;
        }*/

        /*private void formatExponent(object oldBase, Expression newBase, Func<Expression, Expression> simplification)
        {
            Expression power = simplification(members[oldBase]);
            members.Remove(oldBase);
            Multiply((Term)Exponentiate(newBase, power));
        }*/

        public Term Format(Operand.Form form)
        {
            Term ans = new Term();
            ans.coefficient = new double[2] { coefficient[0], coefficient[1] };

            foreach (Pair pair in members.KeyValuePairs())
            {
                Term key = null;
                Expression value = pair.Value.Format(form);

                if (pair.Key is Expression)
                {
                    key = (Term)(pair.Key as Expression).Format(new Operand.Form(Polynomials.Expanded, form.NumberForm, form.TrigonometryForm));
                }
                else if (pair.Key is TrigFunction)
                {
                    TrigFunction tf = pair.Key as TrigFunction;
                    double d;
                    if (tf.Input.IsConstant(out d) || tf.Input.Format(new Operand.Form(Polynomials.Expanded, Numbers.Decimal, form.TrigonometryForm)).IsConstant(out d))
                    {
                        /*bool deg = form.TrigonometryForm == Trigonometry.Degrees;
                        
                        double power;
                        if (value.IsConstant(out power) && power == -1)
                        {
                            d = tf.Inverse(d);
                            key = new Term(deg ? d * 180 / System.Math.PI : d);
                            value = 1;
                        }
                        else
                        {
                            d = tf.Operation(deg ? d * System.Math.PI / 180 : d);
                            if (System.Math.Abs(d) < System.Math.Pow(10, -15))
                            {
                                d = 0;
                            }
                            key = new Term(d);
                        }*/

                        double temp = System.Math.Round(tf.Operation(form.TrigonometryForm, d), 14);
                        if (double.IsNaN(temp))
                        {
                            throw new Exception("Invalid input for trig function");
                        }

                        key = new Term(temp);
                        Operand.Instance.HasTrig = true;
                    }
                }
                else if (pair.Key is char)
                {
                    char c = (char)pair.Key;

                    if (Math.KnownVariables.ContainsKey(c))
                    {
                        if (form.NumberForm == Numbers.Decimal)
                        {
                            key = (Term)Math.KnownVariables[c].value.Format(form);
                        }

                        Operand.Instance.PossibleForms[Numbers.Exact] = true;
                    }
                    else
                    {
                        Operand.Instance.Unknowns.Add(c);

                        if (Operand.Instance.Knowns.ContainsKey(c))
                        {
                            key = (Term)Operand.Instance.Knowns[c].value.Format(form);
                        }
                    }
                }

                if (key == null)
                {
                    key = new Term(pair.Key as dynamic);
                }

                //print.log("ljl;kjasdkl;jasdf'", key, value, Exponentiate(key, value), (Term)Exponentiate(key, value));
                ans.Multiply((Term)Exponentiate(key, value));
            }

            bool wholeCoefficients = ans.coefficient[0].IsInt() && ans.coefficient[1].IsInt();
            if (wholeCoefficients && ans.coefficient[1] != 1)
            {
                Operand.Instance.PossibleForms[Numbers.Exact] = true;
            }

            //Simplify fractions
            if (form.NumberForm == Numbers.Decimal || !wholeCoefficients)
            {
                double d = ans.coefficient[1];
                ans.coefficient[1] = 1;
                ans.Numerator = ans.coefficient[0] / d;
            }

            return ans;
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

        public static Expression Multiply(params Expression[] expressions)
        {
            if (expressions.Length == 2)
            {
                print.log("multiplying expressions " + expressions[0] + " and " + expressions[1]);
            }

            foreach (Expression e in expressions)
            {
                if (e.IsConstant(0))
                {
                    return 0;
                }
            }

            return Operand.FilterIdentity(1,
                (list) =>
                {
                    Term ans = (Term)list[0];

                    for (int i = 1; i < list.Count; i++)
                    {
                        ans.Multiply((Term)list[i]);
                    }

                    return ans;
                },
                expressions);
        }

        private Term Multiply(Term other)
        {
            print.log("multiplying terms " + this + " and " + other);

            foreach (Pair pair in other.members.KeyValuePairs())
            {
                multiply(pair.Key, pair.Value);
            }
            setCoefficient(safelyMultiply(Numerator, other.Numerator), safelyMultiply(Denominator, other.Denominator));
            return this;
        }

        /******************************* EXPONENTIATION *******************************/

        public static Expression Exponentiate(Expression bse, Expression power)
        {
            double baseConstant;
            double powerConstant;

            bool constantBase = bse.IsConstant(out baseConstant);
            bool constantPower = power.IsConstant(out powerConstant);

            //0 ^ something
            if (constantBase && baseConstant == 0)
            {
                //0 in the denominator is undefined
                if (power.IsNegative)
                {
                    throw new DivideByZeroException();
                }
                //0 ^ anything is 0
                else
                {
                    return 0;
                }
            }
            //1 ^ anything or anything ^ 0 is 1
            else if ((constantBase && baseConstant == 1) || (constantPower && powerConstant == 0))
            {
                return 1;
            }
            //anything ^ 1 is itself
            else if (constantPower && powerConstant == 1)
            {
                return bse;
            }
            else
            {
                return ((Term)bse).Exponentiate(power);
            }
        }

        private Term Exponentiate(Expression power)
        {
            print.log("exponentiating " + this, power);

            double d;
            if (IsConstant(out d) && d == 0)
            //if (members.Count == 0 && Numerator == 0)
            {
                if (power.IsNegative)
                {
                    throw new DivideByZeroException();
                }
                else
                {
                    return this;
                }
            }

            Term t = new Term();
            double exp;
            bool constantExponent = power.IsConstant(out exp);

            if (constantExponent && exp == 0)
            {
                return new Term();
            }

            for (int i = 0; i < 2; i++)
            {
                if (coefficient[i] != 1)
                {
                    if (constantExponent && CanExponentiate(coefficient[i], System.Math.Abs(exp)))
                    {
                        int sign = 1;
                        if (exp > 0 && exp < 1 && 1 / exp % 2 == 1 && coefficient[i] < 0)
                        {
                            sign = -1;
                            coefficient[i] *= -1;
                        }

                        double temp = sign * System.Math.Pow(coefficient[i], System.Math.Abs(exp));

                        if (double.IsNaN(temp))
                        {
                            throw new Exception("Imaginary answer");
                        }

                        t.coefficient[(i == 0 ^ exp > 0).ToInt()] = temp;
                    }
                    else
                    {
                        if (i == 1)
                        {
                            //power.Multiply(-1);
                            power = Operand.Multiply(power, -1);
                        }
                        t.multiply(coefficient[i], power);
                    }
                }
            }

            foreach (Pair pair in members.KeyValuePairs())
            {
                t.multiply(pair.Key, Operand.Multiply(pair.Value, power));
            }

            t.setCoefficient(t.coefficient[0], t.coefficient[1]);

            return t;
        }

        public bool TryAdd(Term t, out Term ans)
        {
            print.log("adding terms " + this + " and " + t);

            bool isLike = nonConstantKeyCount == t.nonConstantKeyCount;
            bool isFraction = false;

            foreach (Pair pair in members.KeyValuePairs())
            {
                if (isLike && !(pair.Key is double && pair.Value.IsConstant()))
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
                        double d = System.Math.Pow(pair.Key, pair.Value);

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
                //setCoefficient(Numerator * t.Denominator + t.Numerator * Denominator, Denominator * t.Denominator);
                ans = this;
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

                            numerator[1 - i].multiply(pair.Key, Operand.Multiply(pair.Value, -1));// pair.Value.Multiply(-1));
                        }
                        else
                        {
                            numerator[i].multiply(pair.Key, pair.Value);
                        }
                    }
                }

                numerator[0].Numerator = numerator[0].safelyMultiply(Numerator, t.Denominator);
                numerator[1].Numerator = numerator[1].safelyMultiply(Denominator, t.Numerator);
                denominator.Numerator = denominator.safelyMultiply(Denominator, t.Denominator);
                Expression e = Operand.Add(numerator[0], numerator[1]);
                ans = ((Term)e).Multiply(denominator);
            }
            else
            {
                ans = null;
                return false;
            }
            //}

            return true;
        }

        /******************************* MULTIPLICATION *******************************/
        private double safelyMultiply(double d1, double d2)
        {
            if (d1 * d2 >= MaxCoefficient)
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

            if (power > 0)
            {
                multiply(10, power);
            }
            return double.Parse(num.Substring(0, num.Length - power));
        }

        private void setCoefficient(double numerator = double.NaN, double denominator = double.NaN)
        {
            for (int i = 0; i < 2; i++)
            {
                double d = i == 0 ? numerator : denominator;

                if (!double.IsNaN(d))
                {
                    string num = d.ToString();

                    //Fix doubles with 'E' representation
                    if (num.Contains("E"))
                    {
                        d = double.Parse(num.Substring(0, num.IndexOf("E")));
                        double power = (i * -2 + 1) * double.Parse(num.Substring(num.IndexOf("E") + 1));
                        
                        /*int precision = d.ToString().Length - 2;
                        if (precision > 0 && precision < 14)
                        {
                            d *= System.Math.Pow(10, precision);
                            power -= precision;
                        }*/
                        
                        multiply(ten, power);
                    }

                    if (d != 0)
                    {
                        Expression exp = null;
                        bool scientificNotation = members.ContainsKey(ten) && (exp = members[ten]).Terms.Count == 1 && exp.Terms[0].members.Count == 0;
                        int places = (int)System.Math.Floor(System.Math.Log10(System.Math.Abs(d)));

                        //If the number is currently being represented in scientific notation, make sure it needs to be
                        if (scientificNotation)
                        {
                            double powerOf10 = (i * -2 + 1) * exp.Terms[0].Coefficient;

                            //Multiply the power of 10 into the coefficient if it won't be too large
                            if (powerOf10 + places >= 0 && powerOf10 + places < 15)
                            {
                                d *= System.Math.Pow(10, powerOf10);
                                members.Remove(ten);
                                scientificNotation = false;
                            }
                        }

                        //If we can't get rid of the power of 10 or the coefficient is too small to be rounded, make sure we have correctly formatted scientific notation
                        if ((scientificNotation && !d.IsInt() && places != 0) || (System.Math.Abs(d) < 0.1))
                        {
                            d /= System.Math.Pow(10, places);
                            multiply(ten, places);
                        }
                    }

                    coefficient[i] = d;
                }
            }

            if (coefficient[0].IsInt() && coefficient[1].IsInt() && coefficient[0] != 1 && coefficient[1] != 1)
            {
                int gcd = 1;

                //Divide out the GCD
                if ((gcd *= (int)GCD(coefficient[0], coefficient[1])) != 1)
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
                    members[coefficient[i]] = Operand.Add(members[coefficient[i]], 1);
                    coefficient[i] = 1;
                }
            }
        }
        
        private void multiply(object key) => multiply(key, 1);

        private void multiply(object key, Expression value)
        {
            if (key is int)
            {
                key = (double)(int)key;

                /*if ((int)(double)key == (double)key)
                {
                    key = (int)(double)key;
                }
                else
                {
                    throw new Exception("Decimals cannot be the base of an exponent " + key);
                }*/
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
                members[key] = Operand.Add(members[key], value);
            }

            checkForHashedCoefficients();

            //value = members[key];
            double power;
            if (members[key].IsConstant(out power))
            {
                //Anything ^ 0 is 1
                if (power == 0)
                {
                    members.Remove(key);
                }
                else if (key is double && (double)key != 10 && CanExponentiate((double)key, power))
                {
                    members.Remove(key);
                    Multiply(new Term((double)key).Exponentiate(power));
                    //Multiply(new Term(key.ToString()).Exponentiate((value as Term).Coefficient));
                    //setCoefficient();
                }
            }
        }

        public static double GCD(double a, double b)
        {
            if (a == 0)
            {
                return b;
            }
            if (b == 0)
            {
                return a;
            }

            var sign = System.Math.Sign(a) * System.Math.Sign(b);
            a = System.Math.Abs(a);
            b = System.Math.Abs(b);

            return sign * a > b ? GCD(a % b, b) : GCD(a, b % a);
        }

        /******************************* ADDITION *******************************/

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
            foreach (Pair pair in members.KeyValuePairs())
            {
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
    }
}
 