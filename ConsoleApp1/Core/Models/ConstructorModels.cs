namespace ConsoleApp1.Core.Models;

public static class ConstructorModels
{
    // <-- Polynome Expression -->
    
    public static Poly PolyZero => new Poly(Zero);
    public static Poly PolyOne => new Poly(1.Expr());

    public static PolyRational PolyRationalZero => new PolyRational(PolyZero, PolyOne);
    public static PolyRational PolyRationalOne => new PolyRational(PolyOne, PolyOne);
}