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
            public bool HasExactForm = false;

            public HashSet<char> Unknowns = new HashSet<char>();
            private Dictionary<char, Operand> Subsitutions;

            private Operand.Simplifier os;
            private Numbers n;

            public Simplifier(Operand.Simplifier os, Dictionary<char, Operand> substitutions, Numbers n)
            {
                Subsitutions = substitutions ?? new Dictionary<char, Operand>();
                this.os = os;
                this.n = n;
            }

            public Operand Simplify(char c)
            {
                if (Knowns.ContainsKey(c))
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
                        return os.Simplify(Subsitutions[c]);
                    }
                }

                return new Term(c);
            }
        }
    }
}
