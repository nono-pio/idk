

namespace Cc.Redberry.Rings.Io
{
    /// <summary>
    /// Elements that could be stringified with the help of IStringifier
    /// </summary>
    public interface Stringifiable<E>
    {
        /// <summary>
        /// convert this to string with the use of stringifier
        /// </summary>
        string ToString(IStringifier<E> stringifier)
        {
            return this.ToString();
        }
    }
}