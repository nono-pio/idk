using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Booleans;

public class BooleanValue(bool value) : Boolean
{
    public bool Value = value;

    public override Set SolveFor(Variable x) => Value ? R : EmptySet;

    public override bool? GetValue() => Value;
    
    public override string ToString() => Value.ToString();
}