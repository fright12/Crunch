using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crunch
{
    internal class Variable
    {
        public static Dictionary<char, Operand> Knowns = new Dictionary<char, Operand>()
        {
            { 'e', System.Math.E },
            { 'π', System.Math.PI }
        };

        private string Name;

        public Variable(string name)
        {
            Name = name;
        }

        public static implicit operator Variable(string name) => new Variable(name);

        internal class Simplifier : ISimplifier<char>
        {
            public HashSet<char> Unknowns = new HashSet<char>();
            public bool HasExactForm = false;

            private Dictionary<char, Operand> Subsitutions;
            private HashSet<char> Simplifying;
            private Operand.Simplifier os;
            private Numbers n;

            public Simplifier(Operand.Simplifier os, Dictionary<char, Operand> substitutions, Numbers n)
            {
                Subsitutions = substitutions ?? new Dictionary<char, Operand>();
                Simplifying = new HashSet<char>();
                this.os = os;
                this.n = n;
            }

            public Operand Simplify(char c)
            {
                if (Simplifying.Contains(c))
                {
                    Simplifying.Clear();
                    System.Diagnostics.Debug.WriteLine("Warning: circular dependency detected, " + c + " depends on itself");
                }
                else if (Knowns.ContainsKey(c))
                {
                    HasExactForm = true;

                    if (n == Numbers.Decimal)
                    {
                        return os.Simplify(Knowns[c]);
                    }
                }
                else
                {
                    Unknowns.Add(c);

                    if (Subsitutions != null && Subsitutions.ContainsKey(c))
                    {
                        Simplifying.Add(c);
                        Operand ans = os.Simplify(Subsitutions[c]);

                        if (Simplifying.Count != 0)
                        {
                            Simplifying.Remove(c);
                            return ans;
                        }
                    }
                }

                return new Term(c);
            }
        }
    }
}
