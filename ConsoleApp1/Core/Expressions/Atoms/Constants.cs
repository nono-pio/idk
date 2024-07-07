using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Expressions.Atoms;

public static class Constants
{
    public static Variable PI = Variable.CreateConstant(Symbols.pi, Math.PI);
    public static Variable E = Variable.CreateConstant("e", Math.E);
    //public static Variable I = Variable.CreateConstant("i", Variable.CreateConstant Complex(0, 1));
    
    public static Variable Infinity = Variable.CreateConstant(Symbols.Infinity, double.PositiveInfinity);
    public static Variable NegativeInfinity = Variable.CreateConstant(Symbols.NegativeInfinity, double.NegativeInfinity);
    
    public static Variable NaN = Variable.CreateConstant("NaN", double.NaN);
    
    public static Variable True = Variable.CreateConstant("True", 1);
    public static Variable False = Variable.CreateConstant("False", 0);
    
}