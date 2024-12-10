

namespace Cc.Redberry.Rings.Util
{
    /// <summary>
    /// </summary>
    public interface SerializableFunction<in T, out R>
    {
        R Apply(T t);
        SerializableFunction<T, V> AndThen<V>(SerializableFunction<R, V> after)
        {
            return (T t) => after.Apply(Apply(t));
        }

        static SerializableFunction<T, T> Identity<T>()
        {
            return (t) => t;
        }
    }
}