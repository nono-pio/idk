﻿using System.Numerics;
using Polynomials.Poly;

namespace Polynomials;

public static class Rings
{
    public static Random privateRandom = new Random(DateTime.Now.Nanosecond);
    public static Integers Z = new Integers();
    public static Rationals<BigInteger> Q = new Rationals<BigInteger>(Z);
    public static Integers64 Z64 = new Integers64();
    public static IntegersZp64 Zp64(long modulus) => new IntegersZp64(modulus);
    public static IntegersZp Zp(BigInteger modulus) => new IntegersZp(modulus);

    public static Ring<Poly> PolynomialRing<Poly>(Poly unit) where Poly : Polynomial<Poly>
    {
        return unit.AsRing();
    }
}