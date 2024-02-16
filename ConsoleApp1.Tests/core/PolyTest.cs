using System;
using ConsoleApp1.core;
using ConsoleApp1.utils;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleApp1.Tests.core;

[TestClass]
[TestSubject(typeof(Poly))]
public class PolyTest
{

    public int RandomInt(int min = Int32.MinValue, int max = Int32.MaxValue)
    {
        var rand = new Random();
        return rand.Next(min, max);
    }
    
    public Poly RandomPolyInZ(int minDeg=0, int maxDeg=15)
    {
        var rand = new Random();
        int length = rand.Next(minDeg, maxDeg);
        Expr[] coefs = new Expr[length];
        for (int i = 0; i < length; i++)
        {
            coefs[i] = RandomInt().Expr();
        }
        
        return new Poly(coefs);
    }
    
    [TestMethod]
    public void Equals()
    {
        // Poly Zero
        Assert.IsTrue(PolyZero.Equals(PolyZero));
        // Random Poly
        Assert.IsTrue(new Poly(Un, Deux).Equals(new Poly(Un, Deux)));
        // Dif Coef
        Assert.IsFalse(new Poly(Un, Un).Equals(new Poly(Un, Deux)));
        // Dif Deg
        Assert.IsFalse(new Poly(Un).Equals(new Poly(Un, Deux)));
    }
}