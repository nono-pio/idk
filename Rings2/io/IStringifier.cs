using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Poly;
using Cc.Redberry.Rings.Poly.Multivar;
using Cc.Redberry.Rings.Poly.Univar;

using System.Collections.ObjectModel;


namespace Cc.Redberry.Rings.Io
{
    /// <summary>
    /// Defines #stringify(Stringifiable) method
    /// </summary>
    /// <remarks>@since2.4</remarks>
    public interface IStringifier<Element>
    {
        /// <summary>
        /// Stringify stringifiable object
        /// </summary>
        string Stringify(Element el)
        {
            if (el is Stringifiable<Element>)
                return ((Stringifiable<Element>)el).ToString(this);
            else
                return el.ToString();
        }


        /// <summary>
        /// Stringify stringifiable object
        /// </summary>
        string Stringify(Collection<Element> c)
        {
            return string.Join(',', c.Select(this.Stringify));
        }


        /// <summary>
        /// Get stringifier for the specified ring of some underlying elements, should never give null (use dummy() for
        /// absent stringifier)
        /// </summary>
        IStringifier<UnderlyingElement> Substringifier<UnderlyingElement>(Ring<UnderlyingElement> ring);

        /// <summary>
        /// Get string binding of corresponding element
        /// </summary>
        string GetBinding(Element el)
        {
            return GetBinding(el, null);
        }

        /// <summary>
        /// Get string binding of corresponding element
        /// </summary>
        /// <param name="defaultStr">default string</param>
        string GetBinding(Element el, string defaultStr)
        {
            return GetBindings().GetValueOrDefault(el, defaultStr);
        }


        /// <summary>
        /// Map of bindings
        /// </summary>
        Dictionary<Element, string> GetBindings();

        //////////////////////////////////////////////////////Factory//////////////////////////////////////////////////////
        /// <summary>
        /// Dummy stringifier
        /// </summary>
        static IStringifier<Element> DUMMY = new DummyIStringifier<Element>();
        
        sealed class DummyIStringifier<E> : IStringifier<E> {
            public IStringifier<U> Substringifier<U>(Ring<U> ring) {
                return this as IStringifier<U>;
            }

            public Dictionary<E, string> GetBindings() {
                return new Dictionary<E, string>();
            }
        };
        
        /// <summary>
        /// Dummy stringifier
        /// </summary>
        static IStringifier<E> Dummy<E>()
        {
            return (IStringifier<E>)DUMMY;
        }

        
        /// <summary>
        /// Simple map-based stringifier
        /// </summary>
        sealed class SimpleStringifier<E> : IStringifier<E>
        {
            public readonly Dictionary<E, string> bindings = new Dictionary<E, string>();
            public readonly Dictionary<object, object> substringifiers = new Dictionary<object, object>();
            public IStringifier<U> Substringifier<U>(Ring<U> ring)
            {
                return (IStringifier<U>)substringifiers.GetValueOrDefault(ring, IStringifier<E>.Dummy<U>());
            }

            public Dictionary<E, string> GetBindings()
            {
                return bindings;
            }
        }
        
        
        /// <summary>
        /// Create simple stringifier
        /// </summary>
        static IStringifier<E> MkStringifier<E>(Dictionary<E, string> bindings)
        {
            SimpleStringifier<E> r = new SimpleStringifier<E>();
            foreach (var (k, v) in bindings)
                r.bindings.Add(k, v);
            return r;
        }

        /// <summary>
        /// Create simple stringifier for polynomials with given variables
        /// </summary>
        static IStringifier<Poly> MkPolyStringifier<Poly extends IPolynomial<Poly>>(IPolynomialRing<Poly> ring, params string[] variables)
        {
            Dictionary<Poly, string> bindings = new Dictionary<Poly, string>();
            for (int i = 0; i < ring.NVariables(); ++i)
                bindings.Add(ring.Variable(i), variables[i]);
            return MkStringifier(bindings);
        }

       
        /// <summary>
        /// Create simple stringifier for polynomials with given variables
        /// </summary>
        static IStringifier<Poly> MkPolyStringifier<Poly extends IPolynomial<Poly>>(Poly factory, params string[] variables)
        {
            Dictionary<Poly, string> bindings = new HashMap();
            if (factory is IUnivariatePolynomial)
                bindings.Put((Poly)((IUnivariatePolynomial)factory).CreateMonomial(1), variables[0]);
            else
            {
                AMultivariatePolynomial mf = (AMultivariatePolynomial)factory;
                for (int i = 0; i < mf.nVariables; ++i)
                    bindings.Put((Poly)mf.CreateMonomial(i, 1), variables[i]);
            }

            return MkStringifier(bindings);
        }

       
        /// <summary>
        /// Enclose with math parenthesis if needed (e.g. a+b in (a+b)*c should be enclosed)
        /// </summary>
        static string EncloseMathParenthesisInSumIfNeeded(string cf)
        {
            if (NeedParenthesisInSum(cf))
                return "(" + cf + ")";
            else
                return cf;
        }

        /// <summary>
        /// If required to enclose with math parenthesis (e.g. a+b in (a+b)*c should be enclosed)
        /// </summary>
        static bool NeedParenthesisInSum(string cf)
        {
            if (cf.StartsWith("-") && !HasPlusMinus(1, cf))
                return false;
            return HasPlusMinus(0, cf);
        }

       
        /// <summary>
        /// If required to enclose with math parenthesis (e.g. a+b in (a+b)*c should be enclosed)
        /// </summary>
        static bool HasPlusMinus(int start, string cf)
        {

            // has +- on a zero bracket level
            int level = 0;
            for (int i = start; i < cf.Length; ++i)
            {
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

        /// <summary>
        /// If required to enclose with math parenthesis (e.g. a+b in (a+b)*c should be enclosed)
        /// </summary>
        // has +- on a zero bracket level
        static bool HasMulDivPlusMinus(int start, string cf)
        {

            // has +- on a zero bracket level
            int level = 0;
            for (int i = start; i < cf.Length; ++i)
            {
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

        /// <summary>
        /// Sequence of strings "a", "b", "c" etc.
        /// </summary>
        /// <param name="nVars">number of variable</param>
        static String[] DefaultVars(int nVars)
        {
            if (nVars == 1)
                return new string[]
                {
                    "x"
                };
            if (nVars == 2)
                return new string[]
                {
                    "x",
                    "y"
                };
            if (nVars == 3)
                return new string[]
                {
                    "x",
                    "y",
                    "z"
                };
            string[] vars = new string[nVars];
            for (int i = 1; i <= nVars; i++)
                vars[i - 1] = "x" + i;
            return vars;
        }

        /// <summary>
        /// Sequence of strings "a", "b", "c" etc.
        /// </summary>
        /// <param name="nVars">number of variable</param>
        static string DefaultVar(int i, int nVars)
        {
            if (nVars == 1)
                return "x";
            if (nVars == 2)
                return i == 0 ? "x" : "y";
            if (nVars == 3)
                return i == 0 ? "x" : i == 1 ? "y" : "z";
            return "x" + (i + 1);
        }
        
        static string DefaultVar()
        {
            return DefaultVar(0, 1);
        }
    }
}