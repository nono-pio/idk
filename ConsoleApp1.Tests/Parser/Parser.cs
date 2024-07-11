﻿using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleApp1.Tests.Parser;

[TestClass]
[TestSubject(typeof(Parser))]
public class Parser
{
    [TestMethod]
    public void GetIntTest()
    {
        var test = ConsoleApp1.Parser.Parser.GetInt("123");
        Assert.IsNotNull(test);
        Assert.AreEqual(123L, test.Value.Int);
        Assert.AreEqual(3, test.Value.Length);
        
        test = ConsoleApp1.Parser.Parser.GetInt("1234");
        Assert.IsNotNull(test);
        Assert.AreEqual(1234L, test.Value.Int);
        Assert.AreEqual(4, test.Value.Length);
        
        test = ConsoleApp1.Parser.Parser.GetInt("-123");
        Assert.IsNotNull(test);
        Assert.AreEqual(-123L, test.Value.Int);
        Assert.AreEqual(4, test.Value.Length);
        
        test = ConsoleApp1.Parser.Parser.GetInt("");
        Assert.IsNull(test);
    }
}