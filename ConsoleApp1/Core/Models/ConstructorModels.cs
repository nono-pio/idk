namespace ConsoleApp1.Core.Models;

public static class ConstructorModels
{
    // <-- Polynome Expression -->
    
    public static Poly PolyZero => new Poly(Zero);
    public static Poly PolyOne => new Poly(1.Expr());

}