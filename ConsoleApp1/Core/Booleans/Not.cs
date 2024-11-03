using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Booleans;

public class Not(Boolean value) : Boolean
{
    public Boolean Value = value;
    
    public override bool? GetValue() => !Value.GetValue();

    public override Set SolveFor(Variable x) => Complement(Value.SolveFor(x));

    public override string ToString() => $"\u00ac({Value})";
}