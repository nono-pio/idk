namespace Cc.Redberry.Rings.Util;

public readonly struct Nullable<T>
{
    public bool IsNull { get; }
    public T Value { get; }
    
    public Nullable(T value)
    {
        if (value is null)
        {
            IsNull = true;
            Value = value;
            return;
        }
        
        IsNull = false;
        Value = value;
    }
    
    public Nullable()
    {
        IsNull = true;
        Value = default(T);
    }
    
    public static implicit operator Nullable<T>(T value) => new Nullable<T>(value);
    public static Nullable<T> Null = new Nullable<T>();
}