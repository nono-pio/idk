namespace Rings.io;

public interface IParser<Element> {
    Element parse(string str);
}
