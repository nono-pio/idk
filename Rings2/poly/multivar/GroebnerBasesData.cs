using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Bigint;
using Java.Util;
using Cc.Redberry.Rings.Rings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Poly.Multivar.RoundingMode;
using static Cc.Redberry.Rings.Poly.Multivar.Associativity;
using static Cc.Redberry.Rings.Poly.Multivar.Operator;
using static Cc.Redberry.Rings.Poly.Multivar.TokenType;
using static Cc.Redberry.Rings.Poly.Multivar.SystemInfo;

namespace Cc.Redberry.Rings.Poly.Multivar
{
    /// <summary>
    /// Collection of special ideals
    /// </summary>
    /// <remarks>@since2.3</remarks>
    public sealed class GroebnerBasesData
    {
        /* **************************************** Katsura **************************************************** */
        public static IList<MultivariatePolynomial<Rational<BigInteger>>> Katsura2()
        {
            string[] vars = new[]
            {
                "u0",
                "u1",
                "u2"
            };
            return Arrays.AsList(MultivariatePolynomial.Parse("u2*u2 + u1*u1 + u0*u0 + u1*u1 + u2*u2 - u0", Q, vars), MultivariatePolynomial.Parse("u2*0 + u1*u2 + u0*u1 + u1*u0 + u2*u1 - u1", Q, vars), MultivariatePolynomial.Parse("u2 + u1 + u0 + u1 + u2 - 1", Q, vars));
        }

        public static IList<MultivariatePolynomial<Rational<BigInteger>>> Katsura3()
        {
            string[] vars = new[]
            {
                "u0",
                "u1",
                "u2",
                "u3"
            };
            return Arrays.AsList(MultivariatePolynomial.Parse("u3*u3 + u2*u2 + u1*u1 + u0*u0 + u1*u1 + u2*u2 + u3*u3 - u0", Q, vars), MultivariatePolynomial.Parse("u3*0 + u2*u3 + u1*u2 + u0*u1 + u1*u0 + u2*u1 + u3*u2 - u1", Q, vars), MultivariatePolynomial.Parse("u3*0 + u2*0 + u1*u3 + u0*u2 + u1*u1 + u2*u0 + u3*u1 - u2", Q, vars), MultivariatePolynomial.Parse("u3 + u2 + u1 + u0 + u1 + u2 + u3 - 1", Q, vars));
        }

        public static IList<MultivariatePolynomial<Rational<BigInteger>>> Katsura4()
        {
            string[] vars = new[]
            {
                "u0",
                "u1",
                "u2",
                "u3",
                "u4"
            };
            return Arrays.AsList(MultivariatePolynomial.Parse("u4*u4 + u3*u3 + u2*u2 + u1*u1 + u0*u0 + u1*u1 + u2*u2 + u3*u3 + u4*u4 - u0", Q, vars), MultivariatePolynomial.Parse("u4*0 + u3*u4 + u2*u3 + u1*u2 + u0*u1 + u1*u0 + u2*u1 + u3*u2 + u4*u3 - u1", Q, vars), MultivariatePolynomial.Parse("u4*0 + u3*0 + u2*u4 + u1*u3 + u0*u2 + u1*u1 + u2*u0 + u3*u1 + u4*u2 - u2", Q, vars), MultivariatePolynomial.Parse("u4*0 + u3*0 + u2*0 + u1*u4 + u0*u3 + u1*u2 + u2*u1 + u3*u0 + u4*u1 - u3", Q, vars), MultivariatePolynomial.Parse("u4 + u3 + u2 + u1 + u0 + u1 + u2 + u3 + u4 - 1", Q, vars));
        }

        public static IList<MultivariatePolynomial<Rational<BigInteger>>> Katsura5()
        {
            string[] vars = new[]
            {
                "u0",
                "u1",
                "u2",
                "u3",
                "u4",
                "u5"
            };
            return Arrays.AsList(MultivariatePolynomial.Parse("u5*u5 + u4*u4 + u3*u3 + u2*u2 + u1*u1 + u0*u0 + u1*u1 + u2*u2 + u3*u3 + u4*u4 + u5*u5 - u0", Q, vars), MultivariatePolynomial.Parse("u5*0 + u4*u5 + u3*u4 + u2*u3 + u1*u2 + u0*u1 + u1*u0 + u2*u1 + u3*u2 + u4*u3 + u5*u4 - u1", Q, vars), MultivariatePolynomial.Parse("u5*0 + u4*0 + u3*u5 + u2*u4 + u1*u3 + u0*u2 + u1*u1 + u2*u0 + u3*u1 + u4*u2 + u5*u3 - u2", Q, vars), MultivariatePolynomial.Parse("u5*0 + u4*0 + u3*0 + u2*u5 + u1*u4 + u0*u3 + u1*u2 + u2*u1 + u3*u0 + u4*u1 + u5*u2 - u3", Q, vars), MultivariatePolynomial.Parse("u5*0 + u4*0 + u3*0 + u2*0 + u1*u5 + u0*u4 + u1*u3 + u2*u2 + u3*u1 + u4*u0 + u5*u1 - u4", Q, vars), MultivariatePolynomial.Parse("u5 + u4 + u3 + u2 + u1 + u0 + u1 + u2 + u3 + u4 + u5 - 1", Q, vars));
        }

        public static IList<MultivariatePolynomial<Rational<BigInteger>>> Katsura6()
        {
            string[] vars = new[]
            {
                "u0",
                "u1",
                "u2",
                "u3",
                "u4",
                "u5",
                "u6"
            };
            return Arrays.AsList(MultivariatePolynomial.Parse("u6*u6 + u5*u5 + u4*u4 + u3*u3 + u2*u2 + u1*u1 + u0*u0 + u1*u1 + u2*u2 + u3*u3 + u4*u4 + u5*u5 + u6*u6 - u0", Q, vars), MultivariatePolynomial.Parse("u6*0 + u5*u6 + u4*u5 + u3*u4 + u2*u3 + u1*u2 + u0*u1 + u1*u0 + u2*u1 + u3*u2 + u4*u3 + u5*u4 + u6*u5 - u1", Q, vars), MultivariatePolynomial.Parse("u6*0 + u5*0 + u4*u6 + u3*u5 + u2*u4 + u1*u3 + u0*u2 + u1*u1 + u2*u0 + u3*u1 + u4*u2 + u5*u3 + u6*u4 - u2", Q, vars), MultivariatePolynomial.Parse("u6*0 + u5*0 + u4*0 + u3*u6 + u2*u5 + u1*u4 + u0*u3 + u1*u2 + u2*u1 + u3*u0 + u4*u1 + u5*u2 + u6*u3 - u3", Q, vars), MultivariatePolynomial.Parse("u6*0 + u5*0 + u4*0 + u3*0 + u2*u6 + u1*u5 + u0*u4 + u1*u3 + u2*u2 + u3*u1 + u4*u0 + u5*u1 + u6*u2 - u4", Q, vars), MultivariatePolynomial.Parse("u6*0 + u5*0 + u4*0 + u3*0 + u2*0 + u1*u6 + u0*u5 + u1*u4 + u2*u3 + u3*u2 + u4*u1 + u5*u0 + u6*u1 - u5", Q, vars), MultivariatePolynomial.Parse("u6 + u5 + u4 + u3 + u2 + u1 + u0 + u1 + u2 + u3 + u4 + u5 + u6 - 1", Q, vars));
        }

        public static IList<MultivariatePolynomial<Rational<BigInteger>>> Katsura7()
        {
            string[] vars = new[]
            {
                "u0",
                "u1",
                "u2",
                "u3",
                "u4",
                "u5",
                "u6",
                "u7"
            };
            return Arrays.AsList(MultivariatePolynomial.Parse("u7*u7 + u6*u6 + u5*u5 + u4*u4 + u3*u3 + u2*u2 + u1*u1 + u0*u0 + u1*u1 + u2*u2 + u3*u3 + u4*u4 + u5*u5 + u6*u6 + u7*u7 - u0", Q, vars), MultivariatePolynomial.Parse("u7*0 + u6*u7 + u5*u6 + u4*u5 + u3*u4 + u2*u3 + u1*u2 + u0*u1 + u1*u0 + u2*u1 + u3*u2 + u4*u3 + u5*u4 + u6*u5 + u7*u6 - u1", Q, vars), MultivariatePolynomial.Parse("u7*0 + u6*0 + u5*u7 + u4*u6 + u3*u5 + u2*u4 + u1*u3 + u0*u2 + u1*u1 + u2*u0 + u3*u1 + u4*u2 + u5*u3 + u6*u4 + u7*u5 - u2", Q, vars), MultivariatePolynomial.Parse("u7*0 + u6*0 + u5*0 + u4*u7 + u3*u6 + u2*u5 + u1*u4 + u0*u3 + u1*u2 + u2*u1 + u3*u0 + u4*u1 + u5*u2 + u6*u3 + u7*u4 - u3", Q, vars), MultivariatePolynomial.Parse("u7*0 + u6*0 + u5*0 + u4*0 + u3*u7 + u2*u6 + u1*u5 + u0*u4 + u1*u3 + u2*u2 + u3*u1 + u4*u0 + u5*u1 + u6*u2 + u7*u3 - u4", Q, vars), MultivariatePolynomial.Parse("u7*0 + u6*0 + u5*0 + u4*0 + u3*0 + u2*u7 + u1*u6 + u0*u5 + u1*u4 + u2*u3 + u3*u2 + u4*u1 + u5*u0 + u6*u1 + u7*u2 - u5", Q, vars), MultivariatePolynomial.Parse("u7*0 + u6*0 + u5*0 + u4*0 + u3*0 + u2*0 + u1*u7 + u0*u6 + u1*u5 + u2*u4 + u3*u3 + u4*u2 + u5*u1 + u6*u0 + u7*u1 - u6", Q, vars), MultivariatePolynomial.Parse("u7 + u6 + u5 + u4 + u3 + u2 + u1 + u0 + u1 + u2 + u3 + u4 + u5 + u6 + u7 - 1", Q, vars));
        }

        public static IList<MultivariatePolynomial<Rational<BigInteger>>> Katsura8()
        {
            string[] vars = new[]
            {
                "u0",
                "u1",
                "u2",
                "u3",
                "u4",
                "u5",
                "u6",
                "u7",
                "u8"
            };
            return Arrays.AsList(MultivariatePolynomial.Parse("u8*u8 + u7*u7 + u6*u6 + u5*u5 + u4*u4 + u3*u3 + u2*u2 + u1*u1 + u0*u0 + u1*u1 + u2*u2 + u3*u3 + u4*u4 + u5*u5 + u6*u6 + u7*u7 + u8*u8 - u0", Q, vars), MultivariatePolynomial.Parse("u8*0 + u7*u8 + u6*u7 + u5*u6 + u4*u5 + u3*u4 + u2*u3 + u1*u2 + u0*u1 + u1*u0 + u2*u1 + u3*u2 + u4*u3 + u5*u4 + u6*u5 + u7*u6 + u8*u7 - u1", Q, vars), MultivariatePolynomial.Parse("u8*0 + u7*0 + u6*u8 + u5*u7 + u4*u6 + u3*u5 + u2*u4 + u1*u3 + u0*u2 + u1*u1 + u2*u0 + u3*u1 + u4*u2 + u5*u3 + u6*u4 + u7*u5 + u8*u6 - u2", Q, vars), MultivariatePolynomial.Parse("u8*0 + u7*0 + u6*0 + u5*u8 + u4*u7 + u3*u6 + u2*u5 + u1*u4 + u0*u3 + u1*u2 + u2*u1 + u3*u0 + u4*u1 + u5*u2 + u6*u3 + u7*u4 + u8*u5 - u3", Q, vars), MultivariatePolynomial.Parse("u8*0 + u7*0 + u6*0 + u5*0 + u4*u8 + u3*u7 + u2*u6 + u1*u5 + u0*u4 + u1*u3 + u2*u2 + u3*u1 + u4*u0 + u5*u1 + u6*u2 + u7*u3 + u8*u4 - u4", Q, vars), MultivariatePolynomial.Parse("u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*u8 + u2*u7 + u1*u6 + u0*u5 + u1*u4 + u2*u3 + u3*u2 + u4*u1 + u5*u0 + u6*u1 + u7*u2 + u8*u3 - u5", Q, vars), MultivariatePolynomial.Parse("u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*0 + u2*u8 + u1*u7 + u0*u6 + u1*u5 + u2*u4 + u3*u3 + u4*u2 + u5*u1 + u6*u0 + u7*u1 + u8*u2 - u6", Q, vars), MultivariatePolynomial.Parse("u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*0 + u2*0 + u1*u8 + u0*u7 + u1*u6 + u2*u5 + u3*u4 + u4*u3 + u5*u2 + u6*u1 + u7*u0 + u8*u1 - u7", Q, vars), MultivariatePolynomial.Parse("u8 + u7 + u6 + u5 + u4 + u3 + u2 + u1 + u0 + u1 + u2 + u3 + u4 + u5 + u6 + u7 + u8 - 1", Q, vars));
        }

        public static IList<MultivariatePolynomial<Rational<BigInteger>>> Katsura9()
        {
            string[] vars = new[]
            {
                "u0",
                "u1",
                "u2",
                "u3",
                "u4",
                "u5",
                "u6",
                "u7",
                "u8",
                "u9"
            };
            return Arrays.AsList(MultivariatePolynomial.Parse("u9*u9 + u8*u8 + u7*u7 + u6*u6 + u5*u5 + u4*u4 + u3*u3 + u2*u2 + u1*u1 + u0*u0 + u1*u1 + u2*u2 + u3*u3 + u4*u4 + u5*u5 + u6*u6 + u7*u7 + u8*u8 + u9*u9 - u0", Q, vars), MultivariatePolynomial.Parse("u9*0 + u8*u9 + u7*u8 + u6*u7 + u5*u6 + u4*u5 + u3*u4 + u2*u3 + u1*u2 + u0*u1 + u1*u0 + u2*u1 + u3*u2 + u4*u3 + u5*u4 + u6*u5 + u7*u6 + u8*u7 + u9*u8 - u1", Q, vars), MultivariatePolynomial.Parse("u9*0 + u8*0 + u7*u9 + u6*u8 + u5*u7 + u4*u6 + u3*u5 + u2*u4 + u1*u3 + u0*u2 + u1*u1 + u2*u0 + u3*u1 + u4*u2 + u5*u3 + u6*u4 + u7*u5 + u8*u6 + u9*u7 - u2", Q, vars), MultivariatePolynomial.Parse("u9*0 + u8*0 + u7*0 + u6*u9 + u5*u8 + u4*u7 + u3*u6 + u2*u5 + u1*u4 + u0*u3 + u1*u2 + u2*u1 + u3*u0 + u4*u1 + u5*u2 + u6*u3 + u7*u4 + u8*u5 + u9*u6 - u3", Q, vars), MultivariatePolynomial.Parse("u9*0 + u8*0 + u7*0 + u6*0 + u5*u9 + u4*u8 + u3*u7 + u2*u6 + u1*u5 + u0*u4 + u1*u3 + u2*u2 + u3*u1 + u4*u0 + u5*u1 + u6*u2 + u7*u3 + u8*u4 + u9*u5 - u4", Q, vars), MultivariatePolynomial.Parse("u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*u9 + u3*u8 + u2*u7 + u1*u6 + u0*u5 + u1*u4 + u2*u3 + u3*u2 + u4*u1 + u5*u0 + u6*u1 + u7*u2 + u8*u3 + u9*u4 - u5", Q, vars), MultivariatePolynomial.Parse("u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*u9 + u2*u8 + u1*u7 + u0*u6 + u1*u5 + u2*u4 + u3*u3 + u4*u2 + u5*u1 + u6*u0 + u7*u1 + u8*u2 + u9*u3 - u6", Q, vars), MultivariatePolynomial.Parse("u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*0 + u2*u9 + u1*u8 + u0*u7 + u1*u6 + u2*u5 + u3*u4 + u4*u3 + u5*u2 + u6*u1 + u7*u0 + u8*u1 + u9*u2 - u7", Q, vars), MultivariatePolynomial.Parse("u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*0 + u2*0 + u1*u9 + u0*u8 + u1*u7 + u2*u6 + u3*u5 + u4*u4 + u5*u3 + u6*u2 + u7*u1 + u8*u0 + u9*u1 - u8", Q, vars), MultivariatePolynomial.Parse("u9 + u8 + u7 + u6 + u5 + u4 + u3 + u2 + u1 + u0 + u1 + u2 + u3 + u4 + u5 + u6 + u7 + u8 + u9 - 1", Q, vars));
        }

        public static IList<MultivariatePolynomial<Rational<BigInteger>>> Katsura10()
        {
            string[] vars = new[]
            {
                "u0",
                "u1",
                "u2",
                "u3",
                "u4",
                "u5",
                "u6",
                "u7",
                "u8",
                "u9",
                "u10"
            };
            return Arrays.AsList(MultivariatePolynomial.Parse("u10*u10 + u9*u9 + u8*u8 + u7*u7 + u6*u6 + u5*u5 + u4*u4 + u3*u3 + u2*u2 + u1*u1 + u0*u0 + u1*u1 + u2*u2 + u3*u3 + u4*u4 + u5*u5 + u6*u6 + u7*u7 + u8*u8 + u9*u9 + u10*u10 - u0", Q, vars), MultivariatePolynomial.Parse("u10*0 + u9*u10 + u8*u9 + u7*u8 + u6*u7 + u5*u6 + u4*u5 + u3*u4 + u2*u3 + u1*u2 + u0*u1 + u1*u0 + u2*u1 + u3*u2 + u4*u3 + u5*u4 + u6*u5 + u7*u6 + u8*u7 + u9*u8 + u10*u9 - u1", Q, vars), MultivariatePolynomial.Parse("u10*0 + u9*0 + u8*u10 + u7*u9 + u6*u8 + u5*u7 + u4*u6 + u3*u5 + u2*u4 + u1*u3 + u0*u2 + u1*u1 + u2*u0 + u3*u1 + u4*u2 + u5*u3 + u6*u4 + u7*u5 + u8*u6 + u9*u7 + u10*u8 - u2", Q, vars), MultivariatePolynomial.Parse("u10*0 + u9*0 + u8*0 + u7*u10 + u6*u9 + u5*u8 + u4*u7 + u3*u6 + u2*u5 + u1*u4 + u0*u3 + u1*u2 + u2*u1 + u3*u0 + u4*u1 + u5*u2 + u6*u3 + u7*u4 + u8*u5 + u9*u6 + u10*u7 - u3", Q, vars), MultivariatePolynomial.Parse("u10*0 + u9*0 + u8*0 + u7*0 + u6*u10 + u5*u9 + u4*u8 + u3*u7 + u2*u6 + u1*u5 + u0*u4 + u1*u3 + u2*u2 + u3*u1 + u4*u0 + u5*u1 + u6*u2 + u7*u3 + u8*u4 + u9*u5 + u10*u6 - u4", Q, vars), MultivariatePolynomial.Parse("u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*u10 + u4*u9 + u3*u8 + u2*u7 + u1*u6 + u0*u5 + u1*u4 + u2*u3 + u3*u2 + u4*u1 + u5*u0 + u6*u1 + u7*u2 + u8*u3 + u9*u4 + u10*u5 - u5", Q, vars), MultivariatePolynomial.Parse("u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*u10 + u3*u9 + u2*u8 + u1*u7 + u0*u6 + u1*u5 + u2*u4 + u3*u3 + u4*u2 + u5*u1 + u6*u0 + u7*u1 + u8*u2 + u9*u3 + u10*u4 - u6", Q, vars), MultivariatePolynomial.Parse("u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*u10 + u2*u9 + u1*u8 + u0*u7 + u1*u6 + u2*u5 + u3*u4 + u4*u3 + u5*u2 + u6*u1 + u7*u0 + u8*u1 + u9*u2 + u10*u3 - u7", Q, vars), MultivariatePolynomial.Parse("u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*0 + u2*u10 + u1*u9 + u0*u8 + u1*u7 + u2*u6 + u3*u5 + u4*u4 + u5*u3 + u6*u2 + u7*u1 + u8*u0 + u9*u1 + u10*u2 - u8", Q, vars), MultivariatePolynomial.Parse("u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*0 + u2*0 + u1*u10 + u0*u9 + u1*u8 + u2*u7 + u3*u6 + u4*u5 + u5*u4 + u6*u3 + u7*u2 + u8*u1 + u9*u0 + u10*u1 - u9", Q, vars), MultivariatePolynomial.Parse("u10 + u9 + u8 + u7 + u6 + u5 + u4 + u3 + u2 + u1 + u0 + u1 + u2 + u3 + u4 + u5 + u6 + u7 + u8 + u9 + u10 - 1", Q, vars));
        }

        public static IList<MultivariatePolynomial<Rational<BigInteger>>> Katsura11()
        {
            string[] vars = new[]
            {
                "u0",
                "u1",
                "u2",
                "u3",
                "u4",
                "u5",
                "u6",
                "u7",
                "u8",
                "u9",
                "u10",
                "u11"
            };
            return Arrays.AsList(MultivariatePolynomial.Parse("u11*u11 + u10*u10 + u9*u9 + u8*u8 + u7*u7 + u6*u6 + u5*u5 + u4*u4 + u3*u3 + u2*u2 + u1*u1 + u0*u0 + u1*u1 + u2*u2 + u3*u3 + u4*u4 + u5*u5 + u6*u6 + u7*u7 + u8*u8 + u9*u9 + u10*u10 + u11*u11 - u0", Q, vars), MultivariatePolynomial.Parse("u11*0 + u10*u11 + u9*u10 + u8*u9 + u7*u8 + u6*u7 + u5*u6 + u4*u5 + u3*u4 + u2*u3 + u1*u2 + u0*u1 + u1*u0 + u2*u1 + u3*u2 + u4*u3 + u5*u4 + u6*u5 + u7*u6 + u8*u7 + u9*u8 + u10*u9 + u11*u10 - u1", Q, vars), MultivariatePolynomial.Parse("u11*0 + u10*0 + u9*u11 + u8*u10 + u7*u9 + u6*u8 + u5*u7 + u4*u6 + u3*u5 + u2*u4 + u1*u3 + u0*u2 + u1*u1 + u2*u0 + u3*u1 + u4*u2 + u5*u3 + u6*u4 + u7*u5 + u8*u6 + u9*u7 + u10*u8 + u11*u9 - u2", Q, vars), MultivariatePolynomial.Parse("u11*0 + u10*0 + u9*0 + u8*u11 + u7*u10 + u6*u9 + u5*u8 + u4*u7 + u3*u6 + u2*u5 + u1*u4 + u0*u3 + u1*u2 + u2*u1 + u3*u0 + u4*u1 + u5*u2 + u6*u3 + u7*u4 + u8*u5 + u9*u6 + u10*u7 + u11*u8 - u3", Q, vars), MultivariatePolynomial.Parse("u11*0 + u10*0 + u9*0 + u8*0 + u7*u11 + u6*u10 + u5*u9 + u4*u8 + u3*u7 + u2*u6 + u1*u5 + u0*u4 + u1*u3 + u2*u2 + u3*u1 + u4*u0 + u5*u1 + u6*u2 + u7*u3 + u8*u4 + u9*u5 + u10*u6 + u11*u7 - u4", Q, vars), MultivariatePolynomial.Parse("u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*u11 + u5*u10 + u4*u9 + u3*u8 + u2*u7 + u1*u6 + u0*u5 + u1*u4 + u2*u3 + u3*u2 + u4*u1 + u5*u0 + u6*u1 + u7*u2 + u8*u3 + u9*u4 + u10*u5 + u11*u6 - u5", Q, vars), MultivariatePolynomial.Parse("u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*u11 + u4*u10 + u3*u9 + u2*u8 + u1*u7 + u0*u6 + u1*u5 + u2*u4 + u3*u3 + u4*u2 + u5*u1 + u6*u0 + u7*u1 + u8*u2 + u9*u3 + u10*u4 + u11*u5 - u6", Q, vars), MultivariatePolynomial.Parse("u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*u11 + u3*u10 + u2*u9 + u1*u8 + u0*u7 + u1*u6 + u2*u5 + u3*u4 + u4*u3 + u5*u2 + u6*u1 + u7*u0 + u8*u1 + u9*u2 + u10*u3 + u11*u4 - u7", Q, vars), MultivariatePolynomial.Parse("u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*u11 + u2*u10 + u1*u9 + u0*u8 + u1*u7 + u2*u6 + u3*u5 + u4*u4 + u5*u3 + u6*u2 + u7*u1 + u8*u0 + u9*u1 + u10*u2 + u11*u3 - u8", Q, vars), MultivariatePolynomial.Parse("u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*0 + u2*u11 + u1*u10 + u0*u9 + u1*u8 + u2*u7 + u3*u6 + u4*u5 + u5*u4 + u6*u3 + u7*u2 + u8*u1 + u9*u0 + u10*u1 + u11*u2 - u9", Q, vars), MultivariatePolynomial.Parse("u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*0 + u2*0 + u1*u11 + u0*u10 + u1*u9 + u2*u8 + u3*u7 + u4*u6 + u5*u5 + u6*u4 + u7*u3 + u8*u2 + u9*u1 + u10*u0 + u11*u1 - u10", Q, vars), MultivariatePolynomial.Parse("u11 + u10 + u9 + u8 + u7 + u6 + u5 + u4 + u3 + u2 + u1 + u0 + u1 + u2 + u3 + u4 + u5 + u6 + u7 + u8 + u9 + u10 + u11 - 1", Q, vars));
        }

        public static IList<MultivariatePolynomial<Rational<BigInteger>>> Katsura12()
        {
            string[] vars = new[]
            {
                "u0",
                "u1",
                "u2",
                "u3",
                "u4",
                "u5",
                "u6",
                "u7",
                "u8",
                "u9",
                "u10",
                "u11",
                "u12"
            };
            return Arrays.AsList(MultivariatePolynomial.Parse("u12*u12 + u11*u11 + u10*u10 + u9*u9 + u8*u8 + u7*u7 + u6*u6 + u5*u5 + u4*u4 + u3*u3 + u2*u2 + u1*u1 + u0*u0 + u1*u1 + u2*u2 + u3*u3 + u4*u4 + u5*u5 + u6*u6 + u7*u7 + u8*u8 + u9*u9 + u10*u10 + u11*u11 + u12*u12 - u0", Q, vars), MultivariatePolynomial.Parse("u12*0 + u11*u12 + u10*u11 + u9*u10 + u8*u9 + u7*u8 + u6*u7 + u5*u6 + u4*u5 + u3*u4 + u2*u3 + u1*u2 + u0*u1 + u1*u0 + u2*u1 + u3*u2 + u4*u3 + u5*u4 + u6*u5 + u7*u6 + u8*u7 + u9*u8 + u10*u9 + u11*u10 + u12*u11 - u1", Q, vars), MultivariatePolynomial.Parse("u12*0 + u11*0 + u10*u12 + u9*u11 + u8*u10 + u7*u9 + u6*u8 + u5*u7 + u4*u6 + u3*u5 + u2*u4 + u1*u3 + u0*u2 + u1*u1 + u2*u0 + u3*u1 + u4*u2 + u5*u3 + u6*u4 + u7*u5 + u8*u6 + u9*u7 + u10*u8 + u11*u9 + u12*u10 - u2", Q, vars), MultivariatePolynomial.Parse("u12*0 + u11*0 + u10*0 + u9*u12 + u8*u11 + u7*u10 + u6*u9 + u5*u8 + u4*u7 + u3*u6 + u2*u5 + u1*u4 + u0*u3 + u1*u2 + u2*u1 + u3*u0 + u4*u1 + u5*u2 + u6*u3 + u7*u4 + u8*u5 + u9*u6 + u10*u7 + u11*u8 + u12*u9 - u3", Q, vars), MultivariatePolynomial.Parse("u12*0 + u11*0 + u10*0 + u9*0 + u8*u12 + u7*u11 + u6*u10 + u5*u9 + u4*u8 + u3*u7 + u2*u6 + u1*u5 + u0*u4 + u1*u3 + u2*u2 + u3*u1 + u4*u0 + u5*u1 + u6*u2 + u7*u3 + u8*u4 + u9*u5 + u10*u6 + u11*u7 + u12*u8 - u4", Q, vars), MultivariatePolynomial.Parse("u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*u12 + u6*u11 + u5*u10 + u4*u9 + u3*u8 + u2*u7 + u1*u6 + u0*u5 + u1*u4 + u2*u3 + u3*u2 + u4*u1 + u5*u0 + u6*u1 + u7*u2 + u8*u3 + u9*u4 + u10*u5 + u11*u6 + u12*u7 - u5", Q, vars), MultivariatePolynomial.Parse("u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*u12 + u5*u11 + u4*u10 + u3*u9 + u2*u8 + u1*u7 + u0*u6 + u1*u5 + u2*u4 + u3*u3 + u4*u2 + u5*u1 + u6*u0 + u7*u1 + u8*u2 + u9*u3 + u10*u4 + u11*u5 + u12*u6 - u6", Q, vars), MultivariatePolynomial.Parse("u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*u12 + u4*u11 + u3*u10 + u2*u9 + u1*u8 + u0*u7 + u1*u6 + u2*u5 + u3*u4 + u4*u3 + u5*u2 + u6*u1 + u7*u0 + u8*u1 + u9*u2 + u10*u3 + u11*u4 + u12*u5 - u7", Q, vars), MultivariatePolynomial.Parse("u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*u12 + u3*u11 + u2*u10 + u1*u9 + u0*u8 + u1*u7 + u2*u6 + u3*u5 + u4*u4 + u5*u3 + u6*u2 + u7*u1 + u8*u0 + u9*u1 + u10*u2 + u11*u3 + u12*u4 - u8", Q, vars), MultivariatePolynomial.Parse("u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*u12 + u2*u11 + u1*u10 + u0*u9 + u1*u8 + u2*u7 + u3*u6 + u4*u5 + u5*u4 + u6*u3 + u7*u2 + u8*u1 + u9*u0 + u10*u1 + u11*u2 + u12*u3 - u9", Q, vars), MultivariatePolynomial.Parse("u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*0 + u2*u12 + u1*u11 + u0*u10 + u1*u9 + u2*u8 + u3*u7 + u4*u6 + u5*u5 + u6*u4 + u7*u3 + u8*u2 + u9*u1 + u10*u0 + u11*u1 + u12*u2 - u10", Q, vars), MultivariatePolynomial.Parse("u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*0 + u2*0 + u1*u12 + u0*u11 + u1*u10 + u2*u9 + u3*u8 + u4*u7 + u5*u6 + u6*u5 + u7*u4 + u8*u3 + u9*u2 + u10*u1 + u11*u0 + u12*u1 - u11", Q, vars), MultivariatePolynomial.Parse("u12 + u11 + u10 + u9 + u8 + u7 + u6 + u5 + u4 + u3 + u2 + u1 + u0 + u1 + u2 + u3 + u4 + u5 + u6 + u7 + u8 + u9 + u10 + u11 + u12 - 1", Q, vars));
        }

        public static IList<MultivariatePolynomial<Rational<BigInteger>>> Katsura13()
        {
            string[] vars = new[]
            {
                "u0",
                "u1",
                "u2",
                "u3",
                "u4",
                "u5",
                "u6",
                "u7",
                "u8",
                "u9",
                "u10",
                "u11",
                "u12",
                "u13"
            };
            return Arrays.AsList(MultivariatePolynomial.Parse("u13*u13 + u12*u12 + u11*u11 + u10*u10 + u9*u9 + u8*u8 + u7*u7 + u6*u6 + u5*u5 + u4*u4 + u3*u3 + u2*u2 + u1*u1 + u0*u0 + u1*u1 + u2*u2 + u3*u3 + u4*u4 + u5*u5 + u6*u6 + u7*u7 + u8*u8 + u9*u9 + u10*u10 + u11*u11 + u12*u12 + u13*u13 - u0", Q, vars), MultivariatePolynomial.Parse("u13*0 + u12*u13 + u11*u12 + u10*u11 + u9*u10 + u8*u9 + u7*u8 + u6*u7 + u5*u6 + u4*u5 + u3*u4 + u2*u3 + u1*u2 + u0*u1 + u1*u0 + u2*u1 + u3*u2 + u4*u3 + u5*u4 + u6*u5 + u7*u6 + u8*u7 + u9*u8 + u10*u9 + u11*u10 + u12*u11 + u13*u12 - u1", Q, vars), MultivariatePolynomial.Parse("u13*0 + u12*0 + u11*u13 + u10*u12 + u9*u11 + u8*u10 + u7*u9 + u6*u8 + u5*u7 + u4*u6 + u3*u5 + u2*u4 + u1*u3 + u0*u2 + u1*u1 + u2*u0 + u3*u1 + u4*u2 + u5*u3 + u6*u4 + u7*u5 + u8*u6 + u9*u7 + u10*u8 + u11*u9 + u12*u10 + u13*u11 - u2", Q, vars), MultivariatePolynomial.Parse("u13*0 + u12*0 + u11*0 + u10*u13 + u9*u12 + u8*u11 + u7*u10 + u6*u9 + u5*u8 + u4*u7 + u3*u6 + u2*u5 + u1*u4 + u0*u3 + u1*u2 + u2*u1 + u3*u0 + u4*u1 + u5*u2 + u6*u3 + u7*u4 + u8*u5 + u9*u6 + u10*u7 + u11*u8 + u12*u9 + u13*u10 - u3", Q, vars), MultivariatePolynomial.Parse("u13*0 + u12*0 + u11*0 + u10*0 + u9*u13 + u8*u12 + u7*u11 + u6*u10 + u5*u9 + u4*u8 + u3*u7 + u2*u6 + u1*u5 + u0*u4 + u1*u3 + u2*u2 + u3*u1 + u4*u0 + u5*u1 + u6*u2 + u7*u3 + u8*u4 + u9*u5 + u10*u6 + u11*u7 + u12*u8 + u13*u9 - u4", Q, vars), MultivariatePolynomial.Parse("u13*0 + u12*0 + u11*0 + u10*0 + u9*0 + u8*u13 + u7*u12 + u6*u11 + u5*u10 + u4*u9 + u3*u8 + u2*u7 + u1*u6 + u0*u5 + u1*u4 + u2*u3 + u3*u2 + u4*u1 + u5*u0 + u6*u1 + u7*u2 + u8*u3 + u9*u4 + u10*u5 + u11*u6 + u12*u7 + u13*u8 - u5", Q, vars), MultivariatePolynomial.Parse("u13*0 + u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*u13 + u6*u12 + u5*u11 + u4*u10 + u3*u9 + u2*u8 + u1*u7 + u0*u6 + u1*u5 + u2*u4 + u3*u3 + u4*u2 + u5*u1 + u6*u0 + u7*u1 + u8*u2 + u9*u3 + u10*u4 + u11*u5 + u12*u6 + u13*u7 - u6", Q, vars), MultivariatePolynomial.Parse("u13*0 + u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*u13 + u5*u12 + u4*u11 + u3*u10 + u2*u9 + u1*u8 + u0*u7 + u1*u6 + u2*u5 + u3*u4 + u4*u3 + u5*u2 + u6*u1 + u7*u0 + u8*u1 + u9*u2 + u10*u3 + u11*u4 + u12*u5 + u13*u6 - u7", Q, vars), MultivariatePolynomial.Parse("u13*0 + u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*u13 + u4*u12 + u3*u11 + u2*u10 + u1*u9 + u0*u8 + u1*u7 + u2*u6 + u3*u5 + u4*u4 + u5*u3 + u6*u2 + u7*u1 + u8*u0 + u9*u1 + u10*u2 + u11*u3 + u12*u4 + u13*u5 - u8", Q, vars), MultivariatePolynomial.Parse("u13*0 + u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*u13 + u3*u12 + u2*u11 + u1*u10 + u0*u9 + u1*u8 + u2*u7 + u3*u6 + u4*u5 + u5*u4 + u6*u3 + u7*u2 + u8*u1 + u9*u0 + u10*u1 + u11*u2 + u12*u3 + u13*u4 - u9", Q, vars), MultivariatePolynomial.Parse("u13*0 + u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*u13 + u2*u12 + u1*u11 + u0*u10 + u1*u9 + u2*u8 + u3*u7 + u4*u6 + u5*u5 + u6*u4 + u7*u3 + u8*u2 + u9*u1 + u10*u0 + u11*u1 + u12*u2 + u13*u3 - u10", Q, vars), MultivariatePolynomial.Parse("u13*0 + u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*0 + u2*u13 + u1*u12 + u0*u11 + u1*u10 + u2*u9 + u3*u8 + u4*u7 + u5*u6 + u6*u5 + u7*u4 + u8*u3 + u9*u2 + u10*u1 + u11*u0 + u12*u1 + u13*u2 - u11", Q, vars), MultivariatePolynomial.Parse("u13*0 + u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*0 + u2*0 + u1*u13 + u0*u12 + u1*u11 + u2*u10 + u3*u9 + u4*u8 + u5*u7 + u6*u6 + u7*u5 + u8*u4 + u9*u3 + u10*u2 + u11*u1 + u12*u0 + u13*u1 - u12", Q, vars), MultivariatePolynomial.Parse("u13 + u12 + u11 + u10 + u9 + u8 + u7 + u6 + u5 + u4 + u3 + u2 + u1 + u0 + u1 + u2 + u3 + u4 + u5 + u6 + u7 + u8 + u9 + u10 + u11 + u12 + u13 - 1", Q, vars));
        }

        public static IList<MultivariatePolynomial<Rational<BigInteger>>> Katsura14()
        {
            string[] vars = new[]
            {
                "u0",
                "u1",
                "u2",
                "u3",
                "u4",
                "u5",
                "u6",
                "u7",
                "u8",
                "u9",
                "u10",
                "u11",
                "u12",
                "u13",
                "u14"
            };
            return Arrays.AsList(MultivariatePolynomial.Parse("u14*u14 + u13*u13 + u12*u12 + u11*u11 + u10*u10 + u9*u9 + u8*u8 + u7*u7 + u6*u6 + u5*u5 + u4*u4 + u3*u3 + u2*u2 + u1*u1 + u0*u0 + u1*u1 + u2*u2 + u3*u3 + u4*u4 + u5*u5 + u6*u6 + u7*u7 + u8*u8 + u9*u9 + u10*u10 + u11*u11 + u12*u12 + u13*u13 + u14*u14 - u0", Q, vars), MultivariatePolynomial.Parse("u14*0 + u13*u14 + u12*u13 + u11*u12 + u10*u11 + u9*u10 + u8*u9 + u7*u8 + u6*u7 + u5*u6 + u4*u5 + u3*u4 + u2*u3 + u1*u2 + u0*u1 + u1*u0 + u2*u1 + u3*u2 + u4*u3 + u5*u4 + u6*u5 + u7*u6 + u8*u7 + u9*u8 + u10*u9 + u11*u10 + u12*u11 + u13*u12 + u14*u13 - u1", Q, vars), MultivariatePolynomial.Parse("u14*0 + u13*0 + u12*u14 + u11*u13 + u10*u12 + u9*u11 + u8*u10 + u7*u9 + u6*u8 + u5*u7 + u4*u6 + u3*u5 + u2*u4 + u1*u3 + u0*u2 + u1*u1 + u2*u0 + u3*u1 + u4*u2 + u5*u3 + u6*u4 + u7*u5 + u8*u6 + u9*u7 + u10*u8 + u11*u9 + u12*u10 + u13*u11 + u14*u12 - u2", Q, vars), MultivariatePolynomial.Parse("u14*0 + u13*0 + u12*0 + u11*u14 + u10*u13 + u9*u12 + u8*u11 + u7*u10 + u6*u9 + u5*u8 + u4*u7 + u3*u6 + u2*u5 + u1*u4 + u0*u3 + u1*u2 + u2*u1 + u3*u0 + u4*u1 + u5*u2 + u6*u3 + u7*u4 + u8*u5 + u9*u6 + u10*u7 + u11*u8 + u12*u9 + u13*u10 + u14*u11 - u3", Q, vars), MultivariatePolynomial.Parse("u14*0 + u13*0 + u12*0 + u11*0 + u10*u14 + u9*u13 + u8*u12 + u7*u11 + u6*u10 + u5*u9 + u4*u8 + u3*u7 + u2*u6 + u1*u5 + u0*u4 + u1*u3 + u2*u2 + u3*u1 + u4*u0 + u5*u1 + u6*u2 + u7*u3 + u8*u4 + u9*u5 + u10*u6 + u11*u7 + u12*u8 + u13*u9 + u14*u10 - u4", Q, vars), MultivariatePolynomial.Parse("u14*0 + u13*0 + u12*0 + u11*0 + u10*0 + u9*u14 + u8*u13 + u7*u12 + u6*u11 + u5*u10 + u4*u9 + u3*u8 + u2*u7 + u1*u6 + u0*u5 + u1*u4 + u2*u3 + u3*u2 + u4*u1 + u5*u0 + u6*u1 + u7*u2 + u8*u3 + u9*u4 + u10*u5 + u11*u6 + u12*u7 + u13*u8 + u14*u9 - u5", Q, vars), MultivariatePolynomial.Parse("u14*0 + u13*0 + u12*0 + u11*0 + u10*0 + u9*0 + u8*u14 + u7*u13 + u6*u12 + u5*u11 + u4*u10 + u3*u9 + u2*u8 + u1*u7 + u0*u6 + u1*u5 + u2*u4 + u3*u3 + u4*u2 + u5*u1 + u6*u0 + u7*u1 + u8*u2 + u9*u3 + u10*u4 + u11*u5 + u12*u6 + u13*u7 + u14*u8 - u6", Q, vars), MultivariatePolynomial.Parse("u14*0 + u13*0 + u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*u14 + u6*u13 + u5*u12 + u4*u11 + u3*u10 + u2*u9 + u1*u8 + u0*u7 + u1*u6 + u2*u5 + u3*u4 + u4*u3 + u5*u2 + u6*u1 + u7*u0 + u8*u1 + u9*u2 + u10*u3 + u11*u4 + u12*u5 + u13*u6 + u14*u7 - u7", Q, vars), MultivariatePolynomial.Parse("u14*0 + u13*0 + u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*u14 + u5*u13 + u4*u12 + u3*u11 + u2*u10 + u1*u9 + u0*u8 + u1*u7 + u2*u6 + u3*u5 + u4*u4 + u5*u3 + u6*u2 + u7*u1 + u8*u0 + u9*u1 + u10*u2 + u11*u3 + u12*u4 + u13*u5 + u14*u6 - u8", Q, vars), MultivariatePolynomial.Parse("u14*0 + u13*0 + u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*u14 + u4*u13 + u3*u12 + u2*u11 + u1*u10 + u0*u9 + u1*u8 + u2*u7 + u3*u6 + u4*u5 + u5*u4 + u6*u3 + u7*u2 + u8*u1 + u9*u0 + u10*u1 + u11*u2 + u12*u3 + u13*u4 + u14*u5 - u9", Q, vars), MultivariatePolynomial.Parse("u14*0 + u13*0 + u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*u14 + u3*u13 + u2*u12 + u1*u11 + u0*u10 + u1*u9 + u2*u8 + u3*u7 + u4*u6 + u5*u5 + u6*u4 + u7*u3 + u8*u2 + u9*u1 + u10*u0 + u11*u1 + u12*u2 + u13*u3 + u14*u4 - u10", Q, vars), MultivariatePolynomial.Parse("u14*0 + u13*0 + u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*u14 + u2*u13 + u1*u12 + u0*u11 + u1*u10 + u2*u9 + u3*u8 + u4*u7 + u5*u6 + u6*u5 + u7*u4 + u8*u3 + u9*u2 + u10*u1 + u11*u0 + u12*u1 + u13*u2 + u14*u3 - u11", Q, vars), MultivariatePolynomial.Parse("u14*0 + u13*0 + u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*0 + u2*u14 + u1*u13 + u0*u12 + u1*u11 + u2*u10 + u3*u9 + u4*u8 + u5*u7 + u6*u6 + u7*u5 + u8*u4 + u9*u3 + u10*u2 + u11*u1 + u12*u0 + u13*u1 + u14*u2 - u12", Q, vars), MultivariatePolynomial.Parse("u14*0 + u13*0 + u12*0 + u11*0 + u10*0 + u9*0 + u8*0 + u7*0 + u6*0 + u5*0 + u4*0 + u3*0 + u2*0 + u1*u14 + u0*u13 + u1*u12 + u2*u11 + u3*u10 + u4*u9 + u5*u8 + u6*u7 + u7*u6 + u8*u5 + u9*u4 + u10*u3 + u11*u2 + u12*u1 + u13*u0 + u14*u1 - u13", Q, vars), MultivariatePolynomial.Parse("u14 + u13 + u12 + u11 + u10 + u9 + u8 + u7 + u6 + u5 + u4 + u3 + u2 + u1 + u0 + u1 + u2 + u3 + u4 + u5 + u6 + u7 + u8 + u9 + u10 + u11 + u12 + u13 + u14 - 1", Q, vars));
        }

        public static IList<MultivariatePolynomial<Rational<BigInteger>>> Cyclic(int n)
        {
            MultivariatePolynomial<Rational<BigInteger>> factory = MultivariatePolynomial.Zero(n, Q, MonomialOrder.GREVLEX);
            MultivariatePolynomial<Rational<BigInteger>>[] vars = new MultivariatePolynomial[n];
            for (int i = 0; i < n; ++i)
                vars[i] = factory.CreateLinear(i, Q.GetZero(), Q.GetOne());
            IList<MultivariatePolynomial<Rational<BigInteger>>> polynomials = new List();

            // x1 + x2 + ... + xN
            polynomials.Add(factory.CreateZero().Add(vars));
            for (int nProd = 2; nProd < n; ++nProd)
            {
                MultivariatePolynomial<Rational<BigInteger>> poly = factory.CreateZero();
                for (int cycle = 0; cycle < n; ++cycle)
                {
                    MultivariatePolynomial<Rational<BigInteger>> prod = factory.CreateOne();
                    for (int j = 0; j < nProd; ++j)
                        prod.Multiply(vars[cycle + j >= n ? (cycle + j - n) : cycle + j]);
                    poly.Add(prod);
                }

                polynomials.Add(poly);
            }

            polynomials.Add(factory.CreateOne().Multiply(vars).Decrement());
            return polynomials;
        }

        public static int MIN_KATSURA = 2, MAX_KATSURA = 14;
        public static IList<MultivariatePolynomial<Rational<BigInteger>>> Katsura(int i)
        {
            switch (i)
            {
                case 2:
                    return Katsura2();
                case 3:
                    return Katsura3();
                case 4:
                    return Katsura4();
                case 5:
                    return Katsura5();
                case 6:
                    return Katsura6();
                case 7:
                    return Katsura7();
                case 8:
                    return Katsura8();
                case 9:
                    return Katsura9();
                case 10:
                    return Katsura10();
                case 11:
                    return Katsura11();
                case 12:
                    return Katsura12();
                case 13:
                    return Katsura13();
                case 14:
                    return Katsura14();
                default:
                    throw new ArgumentException();
                    break;
            }
        }
    }
}