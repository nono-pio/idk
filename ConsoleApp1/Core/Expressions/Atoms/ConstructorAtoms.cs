using ConsoleApp1.Core.Classes;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Expressions.Atoms;

public static class ConstructorAtoms
{
    // <-- Number Expression -->
    public static Expr Zero => Num(0);
    public static Expr Un => Num(1);
    public static Expr Deux => Num(2);

    public static Number Num(double num) => new Number(num);
    public static Number Num(float num) => new Number(num);
    public static Number Num(long num) => new Number(num);
    public static Number Num(int num) => new Number(num);
    public static Number Num(NumberStruct num) => new Number(num);
    public static Number Num(long p, long q) => new Number(new NumberStruct(p, q));

    // Var: Name, real, natural, integer, rational, complex, positive, negative, domain, dependencies, default value
    

    public static Variable Var(string name, 
        bool real = true, bool natural = false, bool integer = false, bool rational = false, bool complex = false, 
        bool positive = false, bool negative = false, 
        Set? domain = null, 
        List<Variable>? dependencies = null, 
        Expr? value = null)
    {
        Set? domain_ = domain;
        if (real)
            domain_ = Set.R;
        if (natural)
            domain_ = Set.N;
        if (integer)
            domain_ = Set.Z;
        if (rational)
            domain_ = Set.Q;
        if (complex)
            throw new NotImplementedException();

        if (positive)
            domain_ = domain_?.Positive;
        if (negative)
            domain_ = domain_?.Negative;
        
        return new Variable(name, domain: domain_, dependency: dependencies, value: value);
    }
}