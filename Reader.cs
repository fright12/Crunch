using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using System.Extensions;
using Parse;

namespace Crunch
{
    using Operator = Operator<Token>;

    public class Reader
    {
        public readonly Parse.Reader Parser;
        public Trie<Tuple<Operator<Token>, int>> Operations;

        public Reader(params KeyValuePair<string, Operator<Token>>[][] operations)
        {
            Operations = new Trie<Tuple<Operator<Token>, int>>();
            for (int i = 0; i < operations.Length; i++)
            {
                foreach (KeyValuePair<string, Operator<Token>> kvp in operations[i])
                {
                    Operations.Add(kvp.Key, new Tuple<Operator<Token>, int>(kvp.Value, i));
                }
            }
            
            Parser = new Parse.Reader(Juxtapose);
        }

        public Token Parse(string input)
        {
            Lexer<Operand> lexer = new Lexer<Operand>(Operations, Tokenize);
            IEnumerable<Token> tokenStream = lexer.TokenStream(input);
            return Parser.Parse(tokenStream);
        }

        protected IEnumerable<Token.Operand<Operand>> Tokenize(IEnumerable<char> operand)
        {
            IEnumerator<char> itr = operand.GetEnumerator();
            string number = "";

            while (itr.MoveNext())
            {
                if (Machine.StringClassification.IsNumber(itr.Current.ToString()))
                {
                    number += itr.Current;
                }
                else
                {
                    if (number.Length > 0)
                    {
                        yield return new Token.Operand<Operand>(new Operand(Term.Parse(number)));
                        number = "";
                    }

                    yield return new Token.Operand<Operand>(new Operand(Term.Parse(itr.Current.ToString())));
                }
            }
            
            if (number.Length > 0)
            {
                yield return new Token.Operand<Operand>(new Operand(Term.Parse(number)));
            }
        }

        protected Token Juxtapose(IEnumerable<Token> expression)
        {
            Operand ans = 1;

            foreach (Token t in expression)
            {
                ans.Multiply((Operand)t.Value);
            }

            return new Token.Operand<Operand>(ans);
        }

        private static bool Sequence(IEnumerator<Token> itr, params Func<Token, bool>[] comparers)
        {
            int i = 0;
            while (i < comparers.Length && itr.MoveNext())
            {
                if (!comparers[i++](itr.Current))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsString(Token o, string target) => o.Value.ToString().Equals(target);

        public static Operator Trig(string name, Func<double, double> normal, Func<double, double> inverse)
        {
            UnaryOperator<Token> trig = new UnaryOperator<Token>(null, null);

            Action<IEditEnumerator<Token>> next = (itr) =>
            {
                trig.OperateFunc = (o) => new Token.Operand<Operand>(new Operand(new Term(new TrigFunction(name, (Operand)o[0].Value, (d, units) => normal(d * (units == Trigonometry.Degrees ? System.Math.PI / 180 : 1))))));

                if (itr.MoveNext() && IsString(itr.Current, "^"))
                {
                    int remove = 0;

                    if (Sequence(itr.Copy(), (o) => IsString(o, "-"), (o) => IsString(o, "1"), (o) => o.IsOpeningToken()))
                    {
                        remove = 3;
                    }
                    else if (Sequence(itr.Copy(), (o) => o.IsOpeningToken(), (o) => IsString(o, "-"), (o) => IsString(o, "1"), (o) => !o.IsOpeningToken()))
                    {
                        remove = 5;
                    }
                    
                    if (remove == 0)
                    {
                        itr.MoveNext();
                        itr.Add(0, Math.MathReader.Parser.ParseOperand(itr));
                        itr.MoveNext();
                    }
                    else
                    {
                        itr.Move(remove);
                        for (int i = 0; i < remove; i++)
                        {
                            itr.Remove(-1);
                        }
                        
                        trig.OperateFunc = (o) => new Token.Operand<Operand>(new Operand(new Term(new TrigFunction("arc" + name, (Operand)o[0].Value, (d, units) => inverse(d) * (units == Trigonometry.Degrees ? 180 / System.Math.PI : 1)))));
                    }
                }
            };

            trig.Targets = new Action<IEditEnumerator<Token>>[] { next };
            return trig;
        }

        public class BinaryOperator : BinaryOperator<Token>
        {
            public BinaryOperator(Action<Operand, Operand> operation, Action<IEditEnumerator<Token>> prev = null, Action<IEditEnumerator<Token>> next = null, bool juxtapose = false) : base(
                (o1, o2) =>
                {
                    var _o1 = (Operand)o1.Value;
                    operation(_o1, (Operand)o2.Value);
                    return new Token.Operand<Operand>(_o1);
                },
                (itr) => Move(prev ?? Prev, itr, juxtapose ? -1 : 0),
                (itr) => Move(next ?? Next, itr, juxtapose ? 1 : 0))
            { }

            private static void Move(Action<IEditEnumerator<Token>> mover, IEditEnumerator<Token> itr, int juxtapose)
            {
                mover(itr);

                if (juxtapose != 0)
                {
                    // itr is pointing at the thing we ultimately want to return - we need to move off it to make sure it's juxtaposed too
                    itr.Move(-juxtapose);
                    // Juxtapose will delete the thing we were just on - add the result where it was
                    itr.Add(-juxtapose, Math.MathReader.Juxtapose(Math.MathReader.Parser.CollectOperands(itr, (ProcessingOrder)juxtapose)));
                    // Move back to where we were (which is now the juxtaposed value)
                    itr.Move(-juxtapose);
                }
            }

            public static void Prev(IEditEnumerator itr) => itr.MovePrev();

            public static void Next(IEditEnumerator<Token> itr)
            {
                // Is the next thing a minus sign?
                if (itr.MoveNext() && IsString(itr.Current, "-"))
                {
                    // Move off the negative sign (to the thing after)
                    itr.MoveNext();
                    // Delete the negative sign
                    itr.Remove(-1);

                    // Negate the value after
                    Operand next = (Operand)Math.MathReader.Parser.ParseOperand(itr).Value;
                    next.Multiply(-1);
                    // Replace with the negated value
                    itr.Add(0, new Token.Operand<Operand>(next));
                }
            }
        }
    }
}