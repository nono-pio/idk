namespace Polynomials.Utils;

public class Nullable<T> 
{
    public T Value { get; }
    public bool IsNull { get; }
    
    public Nullable(T value)
    {
        if (value is null)
        {
            Value = default;
            IsNull = true;
            return;
        }
        
        Value = value;
        IsNull = false;
    }
    
    public Nullable()
    {
        Value = default;
        IsNull = true;
    }
    
    public static Nullable<T> Null => new Nullable<T>();
    public static implicit operator Nullable<T>(T value) => new Nullable<T>(value);
    public static implicit operator T(Nullable<T> value) => value.IsNull ? throw new NullReferenceException(): value.Value;
}