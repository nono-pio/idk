namespace ConsoleApp1.core.expr.atoms;

public class Variable : Atom
{
    public readonly string Name;

    public Variable(string name)
    {
        Name = name;
    }

    public override Expr Derivee(string variable)
    {
        return Name == variable ? Un : Zero;
    }

    // TODO
    public override Expr Inverse(Expr y, int argIndex)
    {
        throw new NotImplementedException();
    }

    public override object[] GetArgs()
    {
        return new object[] { Name };
    }

    public override int CompareSelf(Atom expr)
    {
        return string.Compare(Name, ((Variable)expr).Name, StringComparison.Ordinal);
    }

    // <-- Display -->

    public override string ToLatex()
    {
        return Name;
    }


    public override string ToString()
    {
        return Name;
    }
}