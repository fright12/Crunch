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
            Operand o = Math.Evaluate(s);

            if (o != null)
            {
                o = o.Format(Polynomials.Factored, Numbers.Decimal, Trigonometry.Degrees);
            }

            return o;
        }

        public bool CheckQuestion(int questionNumber, string answer)
        {
            Operand given = Parse(answer);
            Operand key = answers[questionNumber];

            return (given == null && key == null) || given.Equals(key);
        }

    }

    public static class Testing
    {
        public static bool Active = false;
        public static bool DisplayCorrectAnswers = false;
        public static bool ShowWork = false;
        public static bool Debug = false;
        private static MathTest testcases = new MathTest();

        public static MathTest Test()
        {
            bool RUN_ALL_TESTS = false;
            //RUN_ALL_TESTS = true;

            BlackBoxTests(RUN_ALL_TESTS);
            WhiteBoxTests(RUN_ALL_TESTS);

            Active = Active && testcases.Questions.Count > 0;
            return testcases;
        }

        private static void BlackBoxTests(bool runAll = false)
        {
            bool RUN_ALL_TESTS = runAll;
            //RUN_ALL_TESTS = true;

            bool physics = false;
            bool other = false;

            //physics = true;
            //other = true;

            if (physics || RUN_ALL_TESTS)
            {
                testcases.Add("(0m/s)*(32.8s)+ 0.5*(3.20m/s^2)*(32.8s)^2", "1720m");
                testcases.Add("(110 m)/(13.57 s^2)", "8.10m/s^2");
                testcases.Add("(18.5 m/s)*(2.47 s)+ 0.5*(11.2 m/s^2)*(2.47 s)^2", "79.8m");
                testcases.Add("(0.5kg)(-9.8ms^2)(-1.5m)", "7.35kgm^2s^2");
                testcases.Add("(50)(10)(cos30)", "433");
                testcases.Add("(900 m^2/s^2)/ (16.0 m/s^2)", "56.3m");
                testcases.Add("(2.84*10^-19J)/(6.626*10^(-34)J*s)", "4.28*10^14/s");
            }


            if (other || RUN_ALL_TESTS)
            {
                testcases.Add("3+476576/9878-56^2876");
                testcases.Add("6-(3/2+4^(7-5))/(8^2*4)+2^(2+3)");
                testcases.Add("8/8/8+2^2^2");
                testcases.Add("6+(8-3)*3/(9/5)+2^2");
            }
        }

        private static void WhiteBoxTests(bool runAll = false)
        {
            bool RUN_ALL_TESTS = runAll;
            //RUN_ALL_TESTS = true;

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

            //Debug = true;
            //print.log("\n\n\n\n" + Math.Evaluate("64^(1/3)").value.Equals(Math.Evaluate("4").value) + "\n\n\n\n");
            //throw new Exception();

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
            //testcases.Add("(x+0)+(x^2+4x+0)", "x^2+5x");
            //testcases.Add("(x+9+0)+(x^2+4x+0)", "x^2+5x+9");
            //testcases.Add("1+x+x^2+x^3+x^4+x^5+x^6", "x^6+x^5+x^4+x^3+x^2+x+1");

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

            if (mixed || RUN_ALL_TESTS)
            {
                testcases.Add("7^2/(sin30)", "98");
                //testcases.Add("7^2/(sin^-1(0.5))");
                //testcases.Add("(sin)^(1/2)30");
                testcases.Add("(sin30)/5", "1/10");
                testcases.Add("((sin30)/4)^2", "1/64");
                testcases.Add("2^(sin30+4)", "22.627");
                testcases.Add("2^(5/(sin30))", "1024");
                testcases.Add("7^2/(2^(sin30))", "34.648");
                testcases.Add("sin(7^2/(sin30))", "0.99");
                testcases.Add("(7/(sinx))^2", "(7/(sinx))^2");
            }

            if (factor || RUN_ALL_TESTS)
            {
                testcases.Add("(5/2x+5/2)/x", "(5(x+1))/(2x)");
                testcases.Add("(1/2x+1/4)/x", "(2x+1)/(4x)");
                testcases.Add("(5/4x+5/6)/x", "(5(3x+2))/(12x)");
                testcases.Add("(5/4x+25/6)/x", "(5(3x+10))/(12x)");
                testcases.Add("(5/4x+25/6)/(3/2x)", "(5(3x+10))/(18x)");
                testcases.Add("(5/4x+25/6)/(3/2x+7/9)", "(15(3x+10))/(2(27x+14))");
            }

            if (zeroes || RUN_ALL_TESTS)
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
                testcases.Add("(x+0)+(x^2+4x+0)", "x^2+5x");
                testcases.Add("(x+9+0)+(x^2+4x+0)", "x^2+5x+9");
            }

            if (nonConstantExponents || RUN_ALL_TESTS)
            {
                testcases.Add("7*7^x", "7^(x+1)");
                testcases.Add("7^x*7", "7^(x+1)");
                testcases.Add("7/5*7^x*5^x", "7^(x+1)*5^(x-1)");
                testcases.Add("7^(x+1)/7^x*4", "28");
                testcases.Add("(4*7^(x+1))/7^x", "28");
                testcases.Add("(4*7^(x+2))/7^x", "196");
                testcases.Add("(4*7^(x+24))/7^x", "7.663*10^20");
                testcases.Add("(5/9)^(1/x)", "(5/9)^(1/x)");
            }

            if (bigNumbers || RUN_ALL_TESTS)
            {
                testcases.Add("12^12", "8.916*10^12");
                testcases.Add("(12^12)^2", "7.95*10^25");
                testcases.Add("12^12*38289", "3.414*10^17");
                testcases.Add("4^2^2^2^2+3/5", "4^65536+3/5");
                testcases.Add("333333333333333333", "3.333*10^17");
                testcases.Add("2000000000000000/30000000000000000", "1/15");
                testcases.Add("2/3000000000000000", "6.666*10^-16");
                testcases.Add("2/15000000000000000", "1.333*10^-16");
                testcases.Add("2/15*10^24", "1.333*10^23");
                testcases.Add("7^24", "1.916*10^20");
                testcases.Add("20^18", "2.621*10^23");
                testcases.Add("7^39572047502", "7^39572047502");
                testcases.Add("2305027405^39572047502", "2305027405^39572047502");
                testcases.Add("10^-24", "10^-24");
                testcases.Add("3*10^-24", "3*10^-24");
                testcases.Add("1/(10^24)", "10^-24");
                testcases.Add("3/(10^24)", "3*10^-24");
                testcases.Add("1/(2*10^24)", "5*10^-25");
                testcases.Add("(10^24)/3", "3.333*10^23");
                testcases.Add("(10^24)/(3x)", "(3.333*10^23)/x");
                testcases.Add("(10^-24)/3", "3.333*10^-25");
                testcases.Add("3/(10^-24)", "3*10^24");
                testcases.Add("(7^24)/(10^19)", "19.158");
                testcases.Add("(10^19)/(7^24)", "0.0522");
                testcases.Add("1.91581231380566/32838582938", "5.834*10^-11");
                testcases.Add("(7^24)*1/32838582938", "5.834*10^9");
                testcases.Add("(7^-24)*32838582938", "1.714*10^-10");
                testcases.Add("5*10^24*10^-19", "50000");
                testcases.Add("(4*10^16)^(1/2)", "2*10^8");
                testcases.Add("5*7^24", "9.58*10^20");
                testcases.Add("5x*10^24", "5x*10^24");
                testcases.Add("5*10^15*10^24", "5*10^39");
                testcases.Add("7^23*10^24*5", "1.368*10^44");
                testcases.Add("(7^24)/34", "5.635*10^18");
                testcases.Add("(34*10^24)/23", "1.478*10^24");
                testcases.Add("(3.4*10^24)/23", "1.478*10^23");
                testcases.Add("2*5*10^24", "10^25");
                testcases.Add("2*(5*10^24)", "10^25");
                testcases.Add("(5*10^24)*2", "10^25");
                testcases.Add("(7*10^24)^24", "1.916*10^596");
                testcases.Add("7^(24*10^24)", "7^(24*10^24)");
            }

            if (trig || RUN_ALL_TESTS)
            {
                testcases.Add("sin(30)", "0.5");
                testcases.Add("sin30", "0.5");
                testcases.Add("sin^2(30)", "0.25");
                /*testcases.Add("sin^-1(0.5)", "30");
                testcases.Add("sin^(6-7)(0.5)", "30");
                testcases.Add("sin(sin^-1(0.5))", "0.5");
                testcases.Add("2sinx+4sinx", "6sinx");
                testcases.Add("2sinx+4siny", "2sinx+4siny");
                testcases.Add("2sinx+4sin^-1x", "2sinx+4sin^-1x");
                testcases.Add("2sin^-1x+4sin^-1x", "6sin^-1x");*/
                testcases.Add("sin5/4", "0.0218");
                testcases.Add("si(30)", "30si");
                testcases.Add("s(30)", "30s");
                testcases.Add("in(30)", "30in");
                testcases.Add("sin(30)+cos(30)", "1.366");
                testcases.Add("sin(cos(60))", "0.00873");
                testcases.Add("sin(cos60+3)", "0.061");
                testcases.Add("cossin30", "1");
                testcases.Add("sincos30", "0.0151");
                testcases.Add("6sin30", "3");
                testcases.Add("5/4sin30", "5/8");
                testcases.Add("sin30cos30", "3^(1/2)/4");
                testcases.Add("esin30", "e/2");
                testcases.Add("e^2sin30+cos30e^2", "10.094");
                testcases.Add("4^(sin30)", "2");
                testcases.Add("4^(sin30+cos30)", "6.644");
                testcases.Add("(x+1)^(sin30+cos30)", "(x+1)^1.366");
                testcases.Add("5(x+sin30)", "5(x+0.5)");
            }

            if (negative || RUN_ALL_TESTS)
            {
                testcases.Add("x-5", "x-5");
                testcases.Add("-9*6", "-54");
                testcases.Add("-9-6", "-15");
                testcases.Add("-6*x", "-6x");
                testcases.Add("-1*x", "-x");
                testcases.Add("-1", "-1");
                testcases.Add("-7", "-7");
                testcases.Add("6+-9", "-3");
                testcases.Add("6*-(1+2)", "-18");
                testcases.Add("-(1+2)", "-3");
                testcases.Add("6/-9", "-2/3");
                testcases.Add("(6+6)-9", "3");
                testcases.Add("2*(6+6)-9", "15");
                testcases.Add("(-5)", "-5");
                testcases.Add("2^-3", "1/8");
                testcases.Add("2^-1^2", "1/2");
                testcases.Add("-1--4/5+2^-2+4/-5+5*-6", "-123/4");
                testcases.Add("x^2+-6*x^2", "-5x^2");
                testcases.Add("x^2+-1*x", "x^2-x");
                testcases.Add("x^2--1*x", "x^2+x");
            }

            if (simplify || RUN_ALL_TESTS)
            {
                testcases.Add("e", "2.718");
                testcases.Add("e*e", "7.389");
                testcases.Add("e*π", "8.54");
                testcases.Add("e+e", "5.437");
                testcases.Add("e+π", "5.86");
                testcases.Add("e^2", "7.389");
                testcases.Add("e^x", "e^x");
                testcases.Add("e^2+π", "10.531");
                testcases.Add("e^π+3", "26.141");
                testcases.Add("e^(2+π)", "170.988");
                testcases.Add("eπ+2", "10.540");
                testcases.Add("5.3e", "14.407");
                testcases.Add("5/4e^(5.2/4)", "4.587");
                testcases.Add("5xe^2+4x^3e", "10.873x^3+36.945x");
                testcases.Add("5x^3e^2+4x^3e", "47.818x^3");
            }

            if (division || RUN_ALL_TESTS)
            {
                testcases.Add("5/8", "5/8");
                testcases.Add("6/8", "3/4");
                testcases.Add("8/2", "4");
                testcases.Add("(-5)/8", "(-5)/8");
                testcases.Add("(-6)/8", "-3/4");
                testcases.Add("(-8)/2", "-4");
                testcases.Add("5/(-8)", "-5/8");
                testcases.Add("6/(-8)", "-3/4");
                testcases.Add("8/(-2)", "-4");
                testcases.Add("(-5)/(-8)", "5/8");
                testcases.Add("(-6)/(-8)", "3/4");
                testcases.Add("(-8)/(-2)", "4");
                testcases.Add("8/3/7", "8/21");
                testcases.Add("8/(3/7)", "56/3");
                testcases.Add("9/2/7/5", "9/70");
                testcases.Add("(9/2)/(7/5)", "45/14");

                testcases.Add("5*8/3", "40/3");
                testcases.Add("5*8/3.5", "11.429");
                testcases.Add("5*8.5/3", "14.167");
                testcases.Add("5.5*8/3", "44/3");
                testcases.Add("5.4*8/3", "14.4");
                testcases.Add("3.5/5*2", "7/5");
                testcases.Add("5/e", "1.839");
                testcases.Add("e/3", "0.906");
                testcases.Add("e/e", "1");
                testcases.Add("e/π", "0.865");
            }

            if (multiplication || RUN_ALL_TESTS)
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

            if (addition || RUN_ALL_TESTS)
            {
                testcases.Add("1+1", "2");
                testcases.Add("1+-1", "0");
                testcases.Add("-1+1", "0");
                testcases.Add("-1+-1", "-2");
                testcases.Add("5+8", "13");
                testcases.Add("5+8/3", "23/3");
                testcases.Add("5/3+8/9", "23/9");
                testcases.Add("5/3+8/7", "59/21");
                testcases.Add("5/x+2/x", "7/x");
                testcases.Add("(5y)/(2x)+(2y)/(3x)", "19y/(6x)");
                testcases.Add("5+x/y", "5+x/y");
                testcases.Add("5+x", "5+x");
                testcases.Add("5/2+x", "5/2+x");
                testcases.Add("5+x^2", "5+x^2");
                testcases.Add("1+10^x", "1+10^x");
                testcases.Add("10^-31+1", "1");
                testcases.Add("10^31+1", "10^31");
                testcases.Add("5+6x^2", "5+6x^2");
                testcases.Add("5+(x+6)", "x+11");
                testcases.Add("5+(7/3+x^2)", "22/3+x^2");
                testcases.Add("5+(x^2+x)", "x^2+x+5");
                testcases.Add("x+x", "2x");
                testcases.Add("x+y", "x+y");
                testcases.Add("x+(x^2+x)", "x^2+2x");
                testcases.Add("x+y/z", "x+y/z");
                testcases.Add("5x+x", "6x");
                testcases.Add("5*10^20x+7^24x", "6.916*10^20x");
                testcases.Add("5x+7^24x", "1.916*10^20x");
                testcases.Add("5x+8x", "13x");
                testcases.Add("5x+8y", "5x+8y");
                testcases.Add("5xy^2+8/3y^2x", "23/3y^2x");
                testcases.Add("(x+1)+(y^2+4x+2)", "y^2+5x+3");
                testcases.Add("1+x+x^2+x^3+x^4+x^5+x^6", "x^6+x^5+x^4+x^3+x^2+x+1");
                testcases.Add("x/y+y/z", "(xz+y^2)/(yz)");
                testcases.Add("x/y+z/y", "(x+z)/y");
                testcases.Add("(x+1)/2+(x+1)/2", "x+1");
                testcases.Add("(x+1)/2+(1-x)/2", "1");
                testcases.Add("(x+1)/2+(-x-1)/2", "0");
                testcases.Add("(x+1)/2+(-x)/2", "1/2");
            }

            if (exponents || RUN_ALL_TESTS)
            {
                testcases.Add("4^2", "16");
                testcases.Add("4^-2", "1/16");
                testcases.Add("4^-2*3^2", "9/16");
                testcases.Add("(64)^(1/2)", "8");
                testcases.Add("(-64)^(1/2)", "");
                testcases.Add("(64)^(1/3)", "4");
                testcases.Add("(-64)^(1/3)", "-4");
                testcases.Add("5^(1/1)", "5");
            }

            if (cancel || RUN_ALL_TESTS)
            {
                testcases.Add("6/x", "6/x");
                testcases.Add("6/(6x)", "1/x");
                testcases.Add("6/(5x^2)", "6/(5x^2)");
                testcases.Add("6/(5x+3y)", "6/(5x+3y)");
                testcases.Add("x/6", "x/6");
                testcases.Add("x/y", "x/y");
                testcases.Add("x/x", "1");
                testcases.Add("x/(6x)", "1/6");
                testcases.Add("3/(6x)", "1/(2x)");
                testcases.Add("x/(5x+3x^2)", "1/(5+3x)");
                testcases.Add("3x/(6x+3x^2)", "1/(2+x)");
                testcases.Add("(6x)/6", "x");
                testcases.Add("(6x)/x", "6");
                testcases.Add("(6x)/(6x)", "1");
                testcases.Add("(6x)/(5x)", "6/5");
                testcases.Add("(6x)/(6y)", "x/y");
                testcases.Add("(6x)/(5y)", "(6x)/(5y)");
                testcases.Add("(6x)/(5x+3y)", "(6x)/(5x+3y)");
                testcases.Add("(6x)/(5x+3x^2)", "6/(5+3x)");
                testcases.Add("(6x+3)/6", "(2x+1)/2");
                testcases.Add("(6x+3)/x", "(6x+3)/x");
                testcases.Add("(6x+3)/(3x)", "(2x+1)/x");
                testcases.Add("(6x+3)/(6x+3)", "1");
                testcases.Add("(6x+3)/(6x+2)", "(6x+3)/(6x+2)");
            }

            if (advanced || RUN_ALL_TESTS)
            {
                testcases.Add("(e+π)/(π+e)", "1");
                testcases.Add("(x+y)/(y+x)", "1");
                testcases.Add("((5*4^(x+1))/4^x)", "20");
                testcases.Add("(5^(x+y))/(5^x+6*5^(x+y))", "(5^y)/(1+6*5^y)");
                testcases.Add("(5^(x+1))/(5^x+9*5^(x+1))", "5/46");
                testcases.Add("(x^(3+x))/(x^(2+x)+6x^4)", "(x^(x+1))/(x^x+6x^2");
            }

            //print.log(";lakjsdflk;jasld;kfj;alskdfj", ((string)new Text()) == null);
            /*print.log(Crunch.Parse.Math("^3234+4^(6-85^"));
            print.log(Crunch.Parse.Math("(3)+(9^(4*(8)+1^5"));
            print.log(Crunch.Parse.Math("((1^3)-1^9)+(43^2*(6-9))-8+(234^4*(7))"));
            print.log(Crunch.Parse.Math("3^1^2^3^4-95)+7)+(4*(6-9))-8+(4*17)"));
            print.log(Crunch.Parse.Math("(4+3()2+1^8^0)(4)+(6)"));
            print.log(Crunch.Parse.Math("()4+()*8()"));
            throw new Exception();*/

            Active = !zeroes && !mixed && !exponents && !RUN_ALL_TESTS;
        }
    }
}
