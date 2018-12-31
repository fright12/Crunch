using System;
using System.Collections.Generic;
using System.Text;

namespace Crunch.Engine
{
    /*internal class Variable
    {
        public static readonly Dictionary<char, double> Knowns = new Dictionary<char, double>()
            {
                { 'e', System.Math.E },
                { 'π', System.Math.PI }
            };

        public Term Value => value;
        public char Name => name;

        private char name;
        private Term value;

        public Variable(char name, Term value = null)
        {
            this.name = name;
            this.value = Knowns.ContainsKey(name) ? new Term(Knowns[name].ToString()) : value;
        }

        public override int GetHashCode() => Name.GetHashCode();
        public override bool Equals(object obj) => obj.GetType() == GetType() && obj.GetHashCode() == GetHashCode();
        public override string ToString() => Name.ToString();
    }*/
}
