

namespace Cc.Redberry.Rings.Io
{
    /// <summary>
    /// Defines {@link #parse(String)} method
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public interface IParser<Element>
    {
        /// <summary>
        /// Parse string into {@code Element}
        /// </summary>
        Element Parse(string @string);
    }
}