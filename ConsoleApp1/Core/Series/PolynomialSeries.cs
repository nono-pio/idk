using ConsoleApp1.Core.Expressions.Atoms;

namespace ConsoleApp1.Core.Series;

// Define as c_i where c_0 + c_1*x + c_2*x^2 + ... + c_n*x^n is the series (n --> oo)
public class PolynomialSeries
{
    public Variable VarIndex;
    public Expr Coefficients;
    
    public PolynomialSeries(Variable varIndex, Expr coefficients)
    {
        VarIndex = varIndex;
        Coefficients = coefficients;
    }
}