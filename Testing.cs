using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;

namespace Crunch.Engine
{
    public class MathTest
    {
        public IReadOnlyList<string> Questions => questions;
        public IReadOnlyList<Operand> Answers => answers;

        private List<string> questions = new List<string>();
        private List<Operand> answers = new List<Operand>();

        public void Add(string question, string answer = "")
        {
            questions.Add(question);
            answers.Add(Parse(answer));
        }

        private Operand Parse(string s)
        {
            var b = Testing.Debug;
            Testing.Debug = false;

            Operand o = Math.Evaluate(s);

            if (o != null)
            {
                o = o.Format(Polynomials.Factored, Numbers.Decimal, Trigonometry.Degrees);// ?? o.Format(Polynomials.Factored, Numbers.Decimal, Trigonometry.Degrees);
            }

            Testing.Debug = b;
            return o;
        }

        public bool CheckQuestion(int questionNumber, string answer)
        {
            Operand ans = Parse(answer);
            Operand key = answers[questionNumber];

            return (ans == null && key == null) || ans.value.Equals(key.value);
        }

    }

    public static class Testing
    {
        public static bool Active = false;
        public static bool DisplayCorrectAnswers = false;
        public static bool Debug = true;
        private static MathTest testcases = new MathTest();

        public static MathTest Test()
        {
            BlackBoxTests();
            WhiteBoxTests();

            Active = Active && testcases.Questions.Count > 0;
            return testcases;
        }

        private static void BlackBoxTests()
        {
            bool physics = false;
            bool other = false;

            //physics = true;
            //other = true;

            if (physics)
            {
                testcases.Add("(0m/s)*(32.8s)+ 0.5*(3.20m/s^2)*(32.8s)^2", "1721.344m");
                testcases.Add("(110 m)/(13.57 s^2)", "8.106m/s^2");
                testcases.Add("(18.5 m/s)*(2.47 s)+ 0.5*(11.2 m/s^2)*(2.47 s)^2", "79.86m");
                testcases.Add("(0.5kg)(-9.8ms^2)(-1.5m)", "7.35kgm^2s^2");
                testcases.Add("(50)(10)(cos30)", "433.013");
                testcases.Add("(900 m^2/s^2)/ (16.0 m/s^2)", "56.25m");
                testcases.Add("(2.84*10^-19J)/(6.626*10^(-34)J*s)", "428614548747359/s");
            }


            if (other)
            {
                testcases.Add("3+476576/9878-56^2876");
                testcases.Add("6-(3/2+4^(7-5))/(8^2*4)+2^(2+3)");
                testcases.Add("8/8/8+2^2^2");
                testcases.Add("6+(8-3)*3/(9/5)+2^2");
            }
        }

        private static void WhiteBoxTests()
        { 
            bool mixed = false;
            bool factor = false;
            bool zeroes = false;
            bool nonConstantExponents = false;
            bool bigNumbers = false;
            bool trig = false;
            bool negative = false;
            bool simplify = false;
            bool division = false;
            bool multiplication = false;
            bool exponents = false;
            bool addition = false;
            bool cancel = false;
            bool advanced = false;
            bool parentheses = false;

            //factor = true;
            //mixed = true;
            //zeroes = true;
            //nonConstantExponents = true;
            //negative = true;
            //simplify = true;

            //trig = true;
            //bigNumbers = true;
            //advanced = true;

            //multiplication = true;
            //addition = true;
            //exponents = true;

            //division = true;
            //cancel = true;

            //parentheses = true;

            //testcases.Add("(x^2)^3");
            //testcases.Add("x^2^3");
            //testcases.Add("log_(2)(8)");

            if (parentheses)
            {
                testcases.Add("(5+3)*5/9");
                testcases.Add("(5+3/4)*5");
                testcases.Add("4*(5+3/4)*5");
                testcases.Add("5/9*(5+3/4)");
                testcases.Add("(5/4*(3+4))");
                testcases.Add("(5*(3/4+4))");
                testcases.Add("(5*(3/4+4)+4)");
                testcases.Add("(5+3)+5)");
                testcases.Add("5+3)+4/5");
                testcases.Add("5+3)+4/5)*5");
                testcases.Add("5+3)+4/5)*5/5/3");
                testcases.Add("(5+3/4)+3)");
                testcases.Add("5+3/4)+4/4/4");
                testcases.Add("5+(3+4");
                testcases.Add("5+(3/4+4");
                testcases.Add("5/4+(3+4");
                testcases.Add("5+(3+(4+4/3)+2");
                testcases.Add("5+(3+(4+3)+2/5");

                testcases.Add("3+(2+4^2)");
                testcases.Add("3+(2+4^2^2^2)");
                testcases.Add("3+(5+4^2^2^2)");
                testcases.Add("3+(5/7+4^2^2^2)");
            }

            if (mixed)
            {
                testcases.Add("7^2/(sin30)");
                testcases.Add("7^2/(sin^-1(0.5))");
                testcases.Add("(sin)^(1/2)30");
                testcases.Add("(sin30)/5");
                testcases.Add("((sin30)/4)^2");
                testcases.Add("2^(sin30+4)");
                testcases.Add("2^(5/(sin30))");
                testcases.Add("7^2/(2^(sin30))");
                testcases.Add("sin(7^2/(sin30))");
                testcases.Add("(7/(sinx))^2");
            }

            if (factor)
            {
                testcases.Add("(5/2x+5/2)/x");
                testcases.Add("(1/2x+1/4)/x");
                testcases.Add("(5/4x+5/6)/x");
                testcases.Add("(5/4x+25/6)/x");
                testcases.Add("(5/4x+25/6)/(3/2x)");
                testcases.Add("(5/4x+25/6)/(3/2x+7/9)");
            }

            if (zeroes)
            {
                testcases.Add("0*8", "0");
                testcases.Add("8*0", "0");
                testcases.Add("0/1", "0");
                testcases.Add("1/0", "");
                testcases.Add("0/x", "0");
                testcases.Add("x/0", "");
                testcases.Add("0^6", "0");
                testcases.Add("0^x", "0");
                testcases.Add("(7t*5*0*x)^5", "0");
                testcases.Add("(7t*5*x)^0", "1");
                testcases.Add("5xy/z*8e*0", "0");
                testcases.Add("(5xy)/(z*8e*0)", "");
                testcases.Add("5xy/z*0*8e", "0");
                testcases.Add("5xy/z*0*8e*0", "0");
                testcases.Add("5xy/z*8e/0", "");
                testcases.Add("0*5xyakldjlf", "0");
                testcases.Add("5+7*0", "5");
                testcases.Add("5x+7*0", "5x");
                testcases.Add("7*0+5x", "5x");
                testcases.Add("5+7x*0", "5");
                testcases.Add("(x+0)+(x^2+4x+0)", "5x+x^2");
                testcases.Add("(x+9+0)+(x^2+4x+0)", "5x+9+x^2");
            }

            if (nonConstantExponents)
            {
                testcases.Add("7*7^x");
                testcases.Add("7^x*7");
                testcases.Add("7/5*7^x*5^x");
                testcases.Add("7^(x+1)/7^x*4");
                testcases.Add("(4*7^(x+1))/7^x");
                testcases.Add("(4*7^(x+2))/7^x");
                testcases.Add("(4*7^(x+24))/7^x");
                testcases.Add("(5/9)^(1/x)");
            }

            if (bigNumbers)
            {
                testcases.Add("12^12");
                testcases.Add("(12^12)^2");
                testcases.Add("12^12*38289");
                testcases.Add("4^2^2^2^2+3/5");
                testcases.Add("333333333333333333");
                testcases.Add("2000000000000000/30000000000000000");
                testcases.Add("2/3000000000000000");
                testcases.Add("2/15000000000000000");
                testcases.Add("2/15*10^24");
                testcases.Add("7^24");
                testcases.Add("20^18");
                testcases.Add("7^39572047502");
                testcases.Add("2305027405^39572047502");
                testcases.Add("10^-24");
                testcases.Add("3*10^-24");
                testcases.Add("1/(10^24)");
                testcases.Add("3/(10^24)");
                testcases.Add("1/(2*10^24)");
                testcases.Add("(10^24)/3");
                testcases.Add("(10^24)/(3x)");
                testcases.Add("(10^-24)/3");
                testcases.Add("3/(10^-24)");
                testcases.Add("(7^24)/(10^19)");
                testcases.Add("(10^19)/(7^24)");
                testcases.Add("1.91581231380566/32838582938");
                testcases.Add("(7^24)*1/32838582938");
                testcases.Add("(7^-24)*32838582938");
                testcases.Add("5*10^24*10^-19");
                testcases.Add("(4*10^16)^(1/2)");
                testcases.Add("5*7^24");
                testcases.Add("5x*10^24");
                testcases.Add("5*10^15*10^24");
                testcases.Add("7^23*10^24*5");
                testcases.Add("(7^24)/34");
                testcases.Add("(34*10^24)/23");
                testcases.Add("(3.4*10^24)/23");
                testcases.Add("2*5*10^24");
                testcases.Add("2*(5*10^24)");
                testcases.Add("(5*10^24)*2");
                testcases.Add("(7*10^24)^24");
                testcases.Add("7^(24*10^24)");
            }

            if (trig)
            {
                testcases.Add("sin(30)");
                testcases.Add("sin30");
                testcases.Add("sin^2(30)");
                testcases.Add("sin^-1(0.5)");
                testcases.Add("sin^(6-7)(0.5)");
                testcases.Add("sin(sin^-1(0.5))");
                testcases.Add("2sinx+4sinx");
                testcases.Add("2sinx+4siny");
                testcases.Add("2sinx+4sin^-1x");
                testcases.Add("2sin^-1x+4sin^-1x");
                testcases.Add("sin5/4");
                testcases.Add("si(30)");
                testcases.Add("s(30)");
                testcases.Add("in(30)");
                testcases.Add("sin(30)+cos(30)");
                testcases.Add("sin(cos(60))");
                testcases.Add("sin(cos60+3)");
                testcases.Add("cossin30");
                testcases.Add("sincos30");
                testcases.Add("6sin30");
                testcases.Add("5/4sin30");
                testcases.Add("sin30cos30");
                testcases.Add("esin30");
                testcases.Add("e^2sin30+cos30e^2");
                testcases.Add("4^(sin30)");
                testcases.Add("4^(sin30+cos30)");
                testcases.Add("(x+1)^(sin30+cos30)");
                testcases.Add("5(x+sin30)");
            }

            if (negative)
            {
                testcases.Add("x-5");
                testcases.Add("-9*6");
                testcases.Add("-9-6");
                testcases.Add("-6*x");
                testcases.Add("-1*x");
                testcases.Add("-1");
                testcases.Add("-7");
                testcases.Add("6+-9");
                testcases.Add("6*-(1+2)");
                testcases.Add("-(1+2)");
                testcases.Add("6/-9");
                testcases.Add("(6+6)-9");
                testcases.Add("2*(6+6)-9");
                testcases.Add("(-5)");
                testcases.Add("2^-3");
                testcases.Add("2^-1^2");
                testcases.Add("-1--4/5+2^-2+4/-5+5*-6");
                testcases.Add("x^2+-6*x^2");
                testcases.Add("x^2+-1*x");
                testcases.Add("x^2--1*x");
            }

            if (simplify)
            {
                testcases.Add("e");
                testcases.Add("e*e");
                testcases.Add("e*π");
                testcases.Add("e+e");
                testcases.Add("e+π");
                testcases.Add("e^2");
                testcases.Add("e^x");
                testcases.Add("e^2+π");
                testcases.Add("e^π+3");
                testcases.Add("e^(2+π)");
                testcases.Add("eπ+2");
                testcases.Add("5.3e");
                testcases.Add("5/4e^(5.2/4)");
                testcases.Add("5xe^2+4x^3e");
                testcases.Add("5x^3e^2+4x^3e");
            }

            if (division)
            {
                testcases.Add("5/8");
                testcases.Add("6/8");
                testcases.Add("8/2");
                testcases.Add("(-5)/8");
                testcases.Add("(-6)/8");
                testcases.Add("(-8)/2");
                testcases.Add("5/(-8)");
                testcases.Add("6/(-8)");
                testcases.Add("8/(-2)");
                testcases.Add("(-5)/(-8)");
                testcases.Add("(-6)/(-8)");
                testcases.Add("(-8)/(-2)");
                testcases.Add("8/3/7");
                testcases.Add("8/(3/7)");
                testcases.Add("9/2/7/5");
                testcases.Add("(9/2)/(7/5)");

                testcases.Add("5*8/3");
                testcases.Add("5*8/3.5");
                testcases.Add("5*8.5/3");
                testcases.Add("5.5*8/3");
                testcases.Add("5.4*8/3");
                testcases.Add("3.5/5*2");
                testcases.Add("5/e");
                testcases.Add("e/3");
                testcases.Add("e/e");
                testcases.Add("e/π");
            }

            if (multiplication)
            {
                testcases.Add("6*8", "48");
                testcases.Add("6*8/5", "48/5");
                testcases.Add("x*6", "6x");
                testcases.Add("x*x", "x^2");
                testcases.Add("x^2*x", "x^3");
                testcases.Add("x*y", "xy");
                testcases.Add("yyy", "y^3");
                testcases.Add("6x*3", "18x");
                testcases.Add("6x*3x", "18x^2");
                testcases.Add("6x*y", "6xy");
                testcases.Add("6x*3y", "18xy");
                testcases.Add("6x*1/2y*z^2", "3xyz^2");
                testcases.Add("6x^2*y*5x", "30x^3y");
                testcases.Add("6x^2*3x^5*y^7", "18x^7y^7");
                testcases.Add("6x*y/z", "6xy/z");
                testcases.Add("x*1/z", "x/z");
                testcases.Add("(x+1)5", "5(x+1)");
                testcases.Add("5(x+1)", "5(x+1)");
                testcases.Add("(x+1)*x", "x(x+1)");
                testcases.Add("(x+1)*6x", "6x(x+1)");
                testcases.Add("(x+1)*(x+2)", "(x+1)(x+2)");
                testcases.Add("(x+1)(x+2)", "(x+1)(x+2)");
                testcases.Add("(x^3+x+2)*(x^2+x+3)", "(x^3+x+2)(x^2+x+3)");
                testcases.Add("(x+1)^2*(x+1)", "(x+1)^3");
                testcases.Add("(x+1)^y*(x+1)^2y", "(x+1)^(y+2)y");
                testcases.Add("(x+1)*5/6", "5/6(x+1)");
                testcases.Add("(x+1)*y/z", "((x+1)y)/z");
            }

            if (addition)
            {
                testcases.Add("1+1");
                testcases.Add("1+-1");
                testcases.Add("-1+1");
                testcases.Add("-1+-1");
                testcases.Add("5+8");
                testcases.Add("5+8/3");
                testcases.Add("5/3+8/9");
                testcases.Add("5/3+8/7");
                testcases.Add("5/x+2/x");
                testcases.Add("(5y)/(2x)+(2y)/(3x)");
                testcases.Add("5+x/y");
                testcases.Add("5+x");
                testcases.Add("5/2+x");
                testcases.Add("5+x^2");
                testcases.Add("1+10^x");
                testcases.Add("10^-31+1");
                testcases.Add("10^31+1");
                testcases.Add("5+6x^2");
                testcases.Add("5+(x+6)");
                testcases.Add("5+(7/3+x^2)");
                testcases.Add("5+(x^2+x)");
                testcases.Add("x+x");
                testcases.Add("x+y");
                testcases.Add("x+(x^2+x)");
                testcases.Add("x+y/z");
                testcases.Add("5x+x");
                testcases.Add("5*10^20x+7^24x");
                testcases.Add("5x+7^24x");
                testcases.Add("5x+8x");
                testcases.Add("5x+8y");
                testcases.Add("5xy^2+8/3y^2x");
                testcases.Add("(x+1)+(y^2+4x+2)");
                testcases.Add("x/y+y/z");
                testcases.Add("x/y+z/y");
                testcases.Add("(x+1)/2+(x+1)/2");
                testcases.Add("(x+1)/2+(1-x)/2");
                testcases.Add("(x+1)/2+(-x-1)/2");
                testcases.Add("(x+1)/2+(-x)/2");
            }

            if (exponents)
            {
                testcases.Add("4^2");
                testcases.Add("4^-2");
                testcases.Add("4^-2*3^2");
                testcases.Add("(64)^(1/2)");
                testcases.Add("(-64)^(1/2)");
                testcases.Add("(64)^(1/3)");
                testcases.Add("(-64)^(1/3)");
                testcases.Add("5^(1/1)");
            }

            if (cancel)
            {
                testcases.Add("6/x");
                testcases.Add("6/(6x)");
                testcases.Add("6/(5x^2)");
                testcases.Add("6/(5x+3y)");
                testcases.Add("x/6");
                testcases.Add("x/y");
                testcases.Add("x/x");
                testcases.Add("x/(6x)");
                testcases.Add("3/(6x)");
                testcases.Add("x/(5x+3x^2)");
                testcases.Add("x/(6x+3x^2)");
                testcases.Add("(6x)/6");
                testcases.Add("(6x)/x");
                testcases.Add("(6x)/(6x)");
                testcases.Add("(6x)/(5x)");
                testcases.Add("(6x)/(6y)");
                testcases.Add("(6x)/(5y)");
                testcases.Add("(6x)/(5x+3y)");
                testcases.Add("(6x)/(5x+3x^2)");
                testcases.Add("(6x+3)/6");
                testcases.Add("(6x+3)/x");
                testcases.Add("(6x+3)/(3x)");
                testcases.Add("(6x+3)/(6x+3)");
                testcases.Add("(6x+3)/(6x+2)");
            }

            if (advanced)
            {
                testcases.Add("(e+π)/(π+e)");
                testcases.Add("(x+y)/(y+x)");
                testcases.Add("((5*4^(x+1))/4^x)");
                testcases.Add("(5^(x+y))/(5^x+6*5^(x+y))");
                testcases.Add("(5^(x+1))/(5^x+9*5^(x+1))");
                testcases.Add("(x^(3+x))/(x^(2+x)+6x^4)");
            }

            //print.log(";lakjsdflk;jasld;kfj;alskdfj", ((string)new Text()) == null);
            /*print.log(Crunch.Parse.Math("^3234+4^(6-85^"));
            print.log(Crunch.Parse.Math("(3)+(9^(4*(8)+1^5"));
            print.log(Crunch.Parse.Math("((1^3)-1^9)+(43^2*(6-9))-8+(234^4*(7))"));
            print.log(Crunch.Parse.Math("3^1^2^3^4-95)+7)+(4*(6-9))-8+(4*17)"));
            print.log(Crunch.Parse.Math("(4+3()2+1^8^0)(4)+(6)"));
            print.log(Crunch.Parse.Math("()4+()*8()"));
            throw new Exception();*/

            Active = !zeroes && !mixed && !exponents;
        }
    }
}
