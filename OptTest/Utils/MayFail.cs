namespace OptTest.Utils;

public class MayFail<T>
{
    public bool IsFailed { get; }
    public T Value => _Value!;
    private T? _Value { get; }

    public MayFail(T value)
    {
        _Value = value;
        IsFailed = false;
    }

    public MayFail()
    {
        _Value = default;
        IsFailed = true;
    }
}