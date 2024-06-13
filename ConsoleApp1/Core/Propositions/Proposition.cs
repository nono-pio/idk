using ConsoleApp1.Core.Propositions;

namespace ConsoleApp1.Core.Propositions;

public enum Maybe
{
    True,
    False,
    Maybe
}

static class MaybeMethods
{

    public static Maybe ToMaybe(this bool value)
    {
        return value ? Maybe.True : Maybe.False;
    }
    
    public static Maybe Not(this Maybe maybe)
    {
        return maybe switch
        {
            Maybe.True => Maybe.False,
            Maybe.False => Maybe.True,
            _ => maybe
        };
    }
    
    public static bool AsBool(this Maybe maybe)
    {
        return maybe switch
        {
            Maybe.True => true,
            Maybe.False => false,
            Maybe.Maybe => throw new Exception(),
            _ => throw new Exception()
        };
    }

    public static bool IsTrue(this Maybe maybe) => maybe == Maybe.True;
    public static bool IsFalse(this Maybe maybe) => maybe == Maybe.False;
    public static bool IsMaybe(this Maybe maybe) => maybe == Maybe.Maybe;
    
}


public abstract class Proposition
{
    public abstract Maybe GetValue();

    public bool GetValueAsBool() => GetValue().AsBool();
    
}