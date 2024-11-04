using System.Text.Json;

namespace WebApplication1.apis;

public class MathErrorController
{

    private static readonly List<MathError> Errors = new();

    public static void AddError(MathError error)
    {
        Errors.Add(error);
    }

    public static List<MathError> GetErrors()
    {
        return Errors;
    }
    
}


public record MathError(string endpoint, JsonElement input, JsonElement result);