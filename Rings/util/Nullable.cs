namespace Rings.util;

public struct Nullable<T>
{
    public T Value
    {
        get => Value;
        set
        {
            Value = value;
            IsNull = value is null;
        }
    }
    public bool IsNull;

    public Nullable(T item)
    {
        if (item is null)
            IsNull = true;

        Value = item;
    }

    public Nullable()
    {
        IsNull = true;
        Value = default(T);
    }
}