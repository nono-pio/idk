namespace Rings.io;

public interface Stringifiable<E> {
    string? ToString(IStringifier<E> stringifier) {
        return this.ToString();
    }
}
