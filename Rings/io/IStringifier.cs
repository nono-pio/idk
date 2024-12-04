namespace Rings.io;


public interface IStringifier<Element> { 
    string? stringify(Element el) {
        if (el is Stringifiable<Element>)
            return ((Stringifiable<Element>) el).ToString(this);
        else
            return el.ToString();
    }
    
    string stringify(IEnumerable<Element> c) {
        return string.Join(',', c.Select(stringify));
    }

    IStringifier<UnderlyingElement> substringifier<UnderlyingElement>(Ring<UnderlyingElement> ring);
    
    string getBinding(Element el) {
        return getBinding(el, null);
    }
    
    string getBinding(Element el, string defaultStr) {
        return getBindings().GetValueOrDefault(el, defaultStr);
    }
    
    Dictionary<Element, string> getBindings();

    //////////////////////////////////////////////////////Factory//////////////////////////////////////////////////////

    public class DummyStringifier<Element> : IStringifier<Element>
    {
        public IStringifier<UnderlyingElement> substringifier<UnderlyingElement>(Ring<UnderlyingElement> ring)
        {
            return (IStringifier<UnderlyingElement>) this;
        }
        
        public Dictionary<Element, string> getBindings() {
            return new ();
        }
    }

    /** Dummy stringifier */
    public static IStringifier<Element> DUMMY = new DummyStringifier<Element>();

    /** Dummy stringifier */
    static IStringifier<E> dummy<E>() {
        return (IStringifier<E>) DUMMY;
    }

    /**
     * Simple map-based stringifier
     */
    sealed class Simplestringifier<E> : IStringifier<E> {
        public readonly Dictionary<E, string> bindings = new Dictionary<E, string>();
        public readonly Dictionary<Ring<E>, IStringifier<E>> substringifiers = new Dictionary<Ring<E>, IStringifier<E>>();

        public  IStringifier<U> substringifier<U>(Ring<U> ring) {
            return (IStringifier<U>) substringifiers.GetValueOrDefault(ring, IStringifier<U>.dummy<U>());
        }

        public Dictionary<E, string> getBindings() {
            return bindings;
        }
    }

    /** Create simple stringifier */
    static  IStringifier<E> mkstringifier<E>(Dictionary<E, string> bindings) {
        Simplestringifier<E> r = new ();
        r.bindings.putAll(bindings);
        return r;
    }

    /** Create simple stringifier for polynomials with given variables */
    static IStringifier<Poly> mkPolystringifier<Poly>(IPolynomialRing<Poly> ring, params string[] variables) where Poly : IPolynomial<Poly> {
        Dictionary<Poly, string> bindings = new ();
        for (int i = 0; i < ring.nVariables(); ++i)
            bindings.Add(ring.variable(i), variables[i]);
        return mkstringifier(bindings);
    }

    /** Create simple stringifier for polynomials with given variables */
    static IStringifier<Poly> mkPolystringifier<Poly>(Poly factory, params string[] variables) where Poly : IPolynomial<Poly> {
        Dictionary<Poly, string> bindings = new ();
        if (factory is IUnivariatePolynomial)
            bindings.Add((Poly) ((IUnivariatePolynomial) factory).createMonomial(1), variables[0]);
        else {
            AMultivariatePolynomial mf = (AMultivariatePolynomial) factory;
            for (int i = 0; i < mf.nVariables; ++i)
                bindings.Add((Poly) mf.createMonomial(i, 1), variables[i]);
        }
        return mkstringifier(bindings);
    }

    /**
     * Enclose with math parenthesis if needed (e.g. a+b in (a+b)*c should be enclosed)
     */
    static string encloseMathParenthesisInSumIfNeeded(string cf) {
        if (needParenthesisInSum(cf))
            return "(" + cf + ")";
        else
            return cf;
    }

    /**
     * If required to enclose with math parenthesis (e.g. a+b in (a+b)*c should be enclosed)
     */
    static bool needParenthesisInSum(string cf) {
        if (cf.StartsWith('-') && !hasPlusMinus(1, cf))
            return false;
        return hasPlusMinus(0, cf);
    }

    static bool hasPlusMinus(int start, string cf) {
        // has +- on a zero bracket level
        int level = 0;
        for (int i = start; i < cf.Length; ++i) {
            char c = cf[i];
            if (c == '(')
                ++level;
            else if (c == ')')
                --level;
            else if ((c == '+' || c == '-') && level == 0)
                return true;
        }
        return false;
    }

    static bool hasMulDivPlusMinus(int start, string cf) {
        // has +- on a zero bracket level
        int level = 0;
        for (int i = start; i < cf.Length; ++i) {
            char c = cf[i];
            if (c == '(')
                ++level;
            else if (c == ')')
                --level;
            else if (level == 0 && (c == '+' || c == '-' || c == '*' || c == '/'))
                return true;
        }
        return false;
    }

    /**
     * Sequence of strings "a", "b", "c" etc.
     *
     * @param nVars number of variable
     */
    static string[] defaultVars(int nVars) {
        if (nVars == 1)
            return new string[]{"x"};
        if (nVars == 2)
            return new string[]{"x", "y"};
        if (nVars == 3)
            return new string[]{"x", "y", "z"};

        string[] vars = new string[nVars];
        for (int i = 1; i <= nVars; i++)
            vars[i - 1] = "x" + i;
        return vars;
    }

    static string defaultVar(int i, int nVars) {
        if (nVars == 1)
            return "x";
        if (nVars == 2)
            return i == 0 ? "x" : "y";
        if (nVars == 3)
            return i == 0 ? "x" : i == 1 ? "y" : "z";

        return "x" + (i + 1);
    }

    static string defaultVar() {
        return defaultVar(0, 1);
    }
}
