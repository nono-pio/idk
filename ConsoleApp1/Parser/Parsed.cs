namespace ConsoleApp1.Parser;

public struct Parsed<T>
{
    public T Value;
    public int Length;
    
    public bool IsNull => Length == 0;
    
    public static Parsed<T> Null => new Parsed<T>(default, 0);
    public Parsed(T value, int length)
    {
        Value = value;
        Length = length;
    }

    public Parsed<R> Map<R>(Func<T, R> map)
    {
        if (IsNull)
            return Parsed<R>.Null;

        return new(map(Value), Length);
    }
}