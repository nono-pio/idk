using OptTest.Utils;

namespace OptTest;

public interface CoercibleTo<Self, OutputForm>
{
    public OutputForm Coerce(Self a);
}

public interface CoercibleFrom<Self, OutputForm>
{
    public Self Coerce(OutputForm form);
}

public interface ConvertibleTo<Self, OutputForm>
{
    public OutputForm Convert(Self a);
}

public interface ConvertibleFrom<Self, OutputForm>
{
    public Self Convert(OutputForm form);
}

public interface RetractableTo<Self, OutputForm> : CoercibleFrom<Self, OutputForm>
{
    public MayFail<OutputForm> RetractIfCan(Self a);

    public OutputForm Retract(Self a)
    {
        var result = RetractIfCan(a);
        if (result.IsFailed)
            throw new InvalidOperationException();
        
        return result.Value;
    }
}

public interface RetractableFrom<Self, OutputForm> : CoercibleTo<Self, OutputForm>
{
    public MayFail<Self> RetractIfCan(OutputForm form);

    public Self Retract(OutputForm form)
    {
        var result = RetractIfCan(form);
        if (result.IsFailed)
            throw new InvalidOperationException();
        
        return result.Value;
    }
}