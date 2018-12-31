using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crunch
{
    public static partial class Math
    {
        public static int DecimalPlaces = 3;
        public static double ImplicitLogarithmBase = 10;

        public static bool CanExponentiate(double b, double p) => !double.IsInfinity(System.Math.Pow(b, p));

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

        public static bool EqualDecimals(string c1, string c2)
        {
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

                    return System.Math.Abs(c1[i - 1] - c2[i - 1]) <= 1;
                }
            }

            return true;
        }
    }
}
