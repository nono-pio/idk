using OptTest.Basics;

namespace OptTest.Utils;

public static class Util
{
    public static T Reduce<T>(Func<T, T, T> f, T[] list, T identity, T absorbant) where T : SetCategory<T>
    {
        var result = identity;
        foreach (var item in list)
        {
            if (result == absorbant)
                break;
            result = f(result, item);
        }
        
        return result;
    }
}