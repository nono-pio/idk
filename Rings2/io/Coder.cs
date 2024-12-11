using System.Numerics;
using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Poly;
using Cc.Redberry.Rings.Poly.Multivar;
using Cc.Redberry.Rings.Poly.Univar;
using Cc.Redberry.Rings.Util;

namespace Cc.Redberry.Rings.Io
{
    /// <summary>
    /// High-level parser and stringifier of ring elements. Uses shunting-yard algorithm for parsing.
    /// </summary>
    /// <param name="<Element>">type of resulting elements</param>
    /// <param name="<Term>">underlying polynomial terms</param>
    /// <param name="<Poly>">underlying multivariate polynomials</param>
    /// <remarks>@since2.4</remarks>
    public class Coder<Element, Term, Poly> : IParser<Element>, IStringifier<Element>
    {
        // parser stuff
        /// <summary>
        /// the base ring
        /// </summary>
        protected readonly Ring<Element> baseRing;

        /// <summary>
        /// map variableName -> Element (if it is a polynomial variable)
        /// </summary>
        protected readonly Dictionary<string, Element> eVariables;

        /// <summary>
        /// auxiliary polynomial ring
        /// </summary>
        protected readonly MultivariateRing<Poly> polyRing;
       
        /// <summary>
        /// map variableName -> variableIndex (if it is a polynomial variable)
        /// </summary>
        protected readonly Dictionary<string, int>? pVariables;
     
        /// <summary>
        /// convert polynomial to base ring elements
        /// </summary>
        protected readonly SerializableFunction<Poly, Element>? polyToElement;
       
        /// <summary>
        /// toString bindings
        /// </summary>
        protected readonly Dictionary<Element, string> bindings;
       
        /// <summary>
        /// toString bindings
        /// </summary>
        /// <summary>
        /// inner coders
        /// </summary>
        protected readonly Dictionary<Ring<_>, Coder<_, _, _>> subcoders;
       

        /// <summary>
        /// inner coders
        /// </summary>
        private Coder(Ring<Element> baseRing, Dictionary<string, Element> eVariables, MultivariateRing<Poly> polyRing, Dictionary<string, int> pVariables, SerializableFunction<Poly, Element> polyToElement)
        {
            this.baseRing = baseRing;
            this.eVariables = eVariables;
            this.polyRing = polyRing;
            this.pVariables = pVariables;
            this.polyToElement = polyToElement;

            // make sure that eVariables contain all pVariables
            if (pVariables != null)
                pVariables.ForEach((k, v) => eVariables.ComputeIfAbsent(k, (__) => polyToElement.Apply(polyRing.Variable(v))));
            this.bindings = eVariables.EntrySet().Stream().Collect(Collectors.ToMap(Map.Entry.GetValue(), Map.Entry.GetKey(), (prev, n) => n));
            this.subcoders = new Dictionary<>();
        }


        /// <summary>
        /// Add string -> element mapping
        /// </summary>
        public virtual Coder<Element, Term, Poly> Bind(string var, Element el)
        {
            bindings.Add(el, var);
            eVariables.Add(var, el);
            return this;
        }

       
        /// <summary>
        /// Add string -> element mapping
        /// </summary>
        public virtual Coder<Element, Term, Poly> BindAlias(string var, Element el)
        {
            eVariables.Add(var, el);
            return this;
        }
        
        /// <summary>
        /// Add string -> element mapping
        /// </summary>
        public virtual Coder<Element, Term, Poly> BindPolynomialVariable(string var, int index)
        {
            if (pVariables != null)
                pVariables.Add(var, index);
            if (polyToElement != null)
                eVariables.Add(var, polyToElement.Apply(polyRing.Variable(index)));
            return this;
        }

    
        /// <summary>
        /// Add stringifier of inner elements
        /// </summary>
        public virtual Coder<Element, Term, Poly> WithEncoder(Coder<_, _, _> subencoder)
        {
            this.subcoders.Add(subencoder.baseRing, subencoder);
            return this;
        }

        /// <summary>
        /// Add stringifier of inner elements
        /// </summary>
        public virtual IStringifier<K> Substringifier<K>(Ring<K> ring)
        {
            IStringifier<K> s = (IStringifier<K>) subcoders[ring];
            if (s == null)
                return IStringifier<K>.Dummy<K>();
            else
                return s;
        }

       

        /// <summary>
        /// Add stringifier of inner elements
        /// </summary>
        public virtual Dictionary<Element, string> GetBindings()
        {
            return bindings;
        }

       
      
        /// <summary>
        /// Decode element from its string representation (#parse)
        /// </summary>
        public virtual Element Decode(string @string)
        {
            return Parse(@string);
        }

        /// <summary>
        /// Encode element to its string representation (#stringify)
        /// </summary>
        public virtual string Encode(Element element)
        {
            return Stringify(element);
        }

        /// <summary>
        /// Maps this coder to a given type via mapper {@code func} which just applies to each parsed element as well as to
        /// bindings (for {@link #stringify(Object)}).
        /// </summary>
        public virtual Coder<Oth, _, _> Map<Oth>(Ring<Oth> ring, Func<Element, Oth> func)
        {
            Dictionary<string, Oth> _eVariables = eVariables.EntrySet().Stream().Collect(Collectors.ToMap(Map.Entry.GetKey(), (e) => func.Apply(e.GetValue())));
            Dictionary<Oth, string> _bindings = bindings.EntrySet().Stream().Collect(Collectors.ToMap((e) => func.Apply(e.GetKey()), Map.Entry.GetValue()));
            Coder<Element, Term, Poly> _this = this;
            return new AnonymousCoder(ring, _eVariables, null, null, null);
        }

        private sealed class AnonymousCoder : Coder
        {
            public AnonymousCoder(Coder parent)
            {
                this.parent = parent;
            }

            private readonly Coder parent;
            static AnonymousCoder()
            {
                this.subcoders.PutAll(_this.subcoders);
                this.bindings.PutAll(_bindings);
            }

            public Oth Parse(Tokenizer tokenizer)
            {
                return func.Apply(_this.Parse(tokenizer));
            }
        }

       
        ////////////////////////////////////////////////////Factory////////////////////////////////////////////////////////
        /// <summary>
        /// </summary>
        /// <param name="baseRing">the base ring</param>
        /// <param name="eVariables">variables bindings (variableString -> base ring element)</param>
        /// <param name="polyRing">auxiliary polynomial ring, to manage intermediate polynomial expressions</param>
        /// <param name="pVariables">polynomial variables bindings (variableString -> polyRing variable index)</param>
        /// <param name="polyToElement">convert from auxiliary polynomials to basering</param>
        public static Coder<Element, Term, Poly> MkCoder<Element, Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(Ring<Element> baseRing, Dictionary<string, Element> eVariables, MultivariateRing<Poly> polyRing, Dictionary<string, Poly> pVariables, SerializableFunction<Poly, Element> polyToElement)
        {
            Dictionary<string, int> iVariables = new HashMap();
            if (pVariables != null)
                foreach (Map.Entry<String, Poly> v in pVariables.EntrySet())
                {
                    Poly p = v.GetValue();
                    if (p.IsEffectiveUnivariate() && !p.IsConstant() && p.DegreeSum() == 1)
                        iVariables.Put(v.GetKey(), v.GetValue().UnivariateVariable());
                    else
                        eVariables.Put(v.GetKey(), polyToElement.Apply(p));
                }

            return new Coder(baseRing, eVariables, polyRing, iVariables, polyToElement);
        }

        
        /// <summary>
        /// Create coder for generic ring
        /// </summary>
        /// <param name="ring">the ring</param>
        public static Coder<E, ?, ?> MkCoder<E>(Ring<E> ring)
        {
            return MkCoder(ring, new HashMap());
        }

        
        /// <summary>
        /// Create coder for generic ring
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <summary>
        /// Create coder for generic rings
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="variables">map string_variable -> ring_element</param>
        public static Coder<E, ?, ?> MkCoder<E>(Ring<E> ring, Dictionary<string, E> variables)
        {
            if (ring is MultivariateRing)
                return MkMultivariateCoder((MultivariateRing)ring, (Dictionary)variables);
            if (ring is IPolynomialRing && ((IPolynomialRing)ring).Factory() is IUnivariatePolynomial)
                return MkUnivariateCoder((IPolynomialRing)ring, (Dictionary)variables);
            if (ring is MultipleFieldExtension)
                return MkMultipleExtensionCoder((MultipleFieldExtension)ring, (Dictionary)variables);
            return new Coder(ring, variables, null, null, null);
        }

        
        
        /// <summary>
        /// Create coder for generic polynomial rings
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="variables">variables</param>
        public static Coder<Poly, ?, ?> MkPolynomialCoder<Poly extends IPolynomial<Poly>>(IPolynomialRing<Poly> ring, params string[] variables)
        {
            return MkCoder(ring, MkVarsMap(ring, variables));
        }

        
        /// <summary>
        /// Create coder for multiple field extension
        /// </summary>
        /// <param name="field">multiple field extension</param>
        /// <param name="variables">string representation of generators</param>
        public static Coder<mPoly, ?, ?> MkMultipleExtensionCoder<Term extends AMonomial<Term>, mPoly extends AMultivariatePolynomial<Term, mPoly>, sPoly extends IUnivariatePolynomial<sPoly>>(MultipleFieldExtension<Term, mPoly, sPoly> field, params string[] variables)
        {
            return MkMultipleExtensionCoder(field, MkVarsMap(field, variables));
        }

        
        /// <summary>
        /// Create coder for multiple field extension
        /// </summary>
        /// <param name="field">multiple field extension</param>
        /// <param name="variables">string representation of generators</param>
        private static Dictionary<string, E> MkVarsMap<E extends IPolynomial<E>>(IPolynomialRing<E> ring, params string[] variables)
        {
            Dictionary<string, E> pVariables = new HashMap();
            for (int i = 0; i < variables.Length; ++i)
                pVariables.Put(variables[i], ring.Variable(i));
            return pVariables;
        }

        
        /// <summary>
        /// Create coder for multiple field extension
        /// </summary>
        /// <param name="field">multiple field extension</param>
        /// <param name="variables">map generator_string -> generator_as_ring_element</param>
        public static Coder<mPoly, ?, ?> MkMultipleExtensionCoder<Term extends AMonomial<Term>, mPoly extends AMultivariatePolynomial<Term, mPoly>, sPoly extends IUnivariatePolynomial<sPoly>>(MultipleFieldExtension<Term, mPoly, sPoly> field, Dictionary<string, mPoly> variables)
        {
            Dictionary<string, sPoly> sVars = new HashMap();
            foreach (Map.Entry<String, mPoly> v in variables.EntrySet())
                sVars.Put(v.GetKey(), field.Inverse(v.GetValue()));
            Coder<mPoly, ?, ?> coder = MkUnivariateCoder(field.GetSimpleExtension(), sVars).Map(field, field.imageFunc);
            coder.bindings.PutAll(variables.EntrySet().Stream().Collect(Collectors.ToMap(Map.Entry.GetValue(), Map.Entry.GetKey(), (prev, n) => n)));
            return coder;
        }

        

        /// <summary>
        /// Create coder for multivariate polynomial rings
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="variables">map string_variable -> ring_element</param>
        public static Coder<Poly, Term, Poly> MkMultivariateCoder<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(MultivariateRing<Poly> ring, Dictionary<string, Poly> variables)
        {
            return MkCoder(ring, variables, ring, variables, SerializableFunction.Identity());
        }

        
        /// <summary>
        /// Create coder for multivariate polynomial rings
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="variables">polynomial variables</param>
        public static Coder<Poly, Term, Poly> MkMultivariateCoder<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(MultivariateRing<Poly> ring, params string[] variables)
        {
            Dictionary<string, Poly> pVariables = new HashMap();
            for (int i = 0; i < variables.Length; ++i)
                pVariables.Put(variables[i], ring.Variable(i));
            return MkMultivariateCoder(ring, pVariables);
        }
        
        /// <summary>
        /// Create coder for multivariate polynomial rings
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="cfCoder">coder for coefficient ring elements</param>
        /// <param name="variables">map string_variable -> ring_element</param>
        public static Coder<MultivariatePolynomial<E>, Monomial<E>, MultivariatePolynomial<E>> MkMultivariateCoder<E>(MultivariateRing<MultivariatePolynomial<E>> ring, Coder<E, ?, ?> cfCoder, Dictionary<string, MultivariatePolynomial<E>> variables)
        {
            cfCoder.eVariables.ForEach((k, v) => variables.Put(k, ring.Factory().CreateConstant(v)));
            return MkMultivariateCoder(ring, variables).WithEncoder(cfCoder);
        }

        
        /// <summary>
        /// Create parser for multivariate polynomial rings
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="cfCoder">coder of coefficient ring elements</param>
        /// <param name="variables">polynomial variables</param>
        public static Coder<MultivariatePolynomial<E>, Monomial<E>, MultivariatePolynomial<E>> MkMultivariateCoder<E>(MultivariateRing<MultivariatePolynomial<E>> ring, Coder<E, ?, ?> cfCoder, params string[] variables)
        {
            Dictionary<string, MultivariatePolynomial<E>> eVariables = new HashMap();
            for (int i = 0; i < variables.Length; ++i)
                eVariables.Put(variables[i], ring.Variable(i));
            return MkMultivariateCoder(ring, cfCoder, eVariables);
        }


        /// <summary>
        /// Create coder for univariate polynomial rings
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="variables">map string_variable -> ring_element</param>
        public static Coder<Poly, ?, ?> MkUnivariateCoder<Poly extends IUnivariatePolynomial<Poly>>(IPolynomialRing<Poly> ring, Dictionary<string, Poly> variables)
        {
            MultivariateRing mRing = Rings.MultivariateRing(ring.Factory().AsMultivariate());
            Dictionary<string, AMultivariatePolynomial> pVariables = variables.EntrySet().Stream().Collect(Collectors.ToMap(Map.Entry.GetKey(), (e) => e.GetValue().AsMultivariate()));
            return MkCoder(ring, variables, mRing, pVariables, (p) => (Poly)p.AsUnivariate());
        }

        
        /// <summary>
        /// Create coder for univariate polynomial rings
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="variable">variable string</param>
        public static Coder<Poly, ?, ?> MkUnivariateCoder<Poly extends IUnivariatePolynomial<Poly>>(IPolynomialRing<Poly> ring, string variable)
        {
            return MkUnivariateCoder(ring, new AnonymousHashMap(this));
        }

        private sealed class AnonymousHashMap : HashMap
        {
            public AnonymousHashMap(Coder parent)
            {
                this.parent = parent;
            }

            private readonly Coder parent;
            static AnonymousHashMap()
            {
                Put(variable, ring.Variable(0));
            }
        }

        
        /// <summary>
        /// Create coder for univariate polynomial rings
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="cfCoder">coder of coefficient ring elements</param>
        /// <param name="variables">map string_variable -> ring_element</param>
        public static Coder<UnivariatePolynomial<E>, ?, ?> MkUnivariateCoder<E>(IPolynomialRing<UnivariatePolynomial<E>> ring, Coder<E, ?, ?> cfCoder, Dictionary<string, UnivariatePolynomial<E>> variables)
        {
            cfCoder.eVariables.ForEach((k, v) => variables.Put(k, ring.Factory().CreateConstant(v)));
            return MkUnivariateCoder(ring, variables).WithEncoder(cfCoder);
        }

        
        /// <summary>
        /// Create coder for univariate polynomial rings
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="cfCoder">coder of coefficient ring elements</param>
        /// <param name="variable">string variable</param>
        public static Coder<UnivariatePolynomial<E>, ?, ?> MkUnivariateCoder<E>(IPolynomialRing<UnivariatePolynomial<E>> ring, Coder<E, ?, ?> cfCoder, string variable)
        {
            HashMap<string, UnivariatePolynomial<E>> eVariables = new HashMap();
            eVariables.Put(variable, ring.Variable(0));
            return MkUnivariateCoder(ring, cfCoder, eVariables);
        }

        
        /// <summary>
        /// Create coder for rational elements
        /// </summary>
        public static Coder<Rational<E>, ?, ?> MkRationalsCoder<E>(Rationals<E> ring, Coder<E, ?, ?> elementsCoder)
        {
            return MkNestedCoder(ring, new HashMap(), elementsCoder, (e) => new Rational(ring.ring, e));
        }

        
        /// <summary>
        /// Create coder for nested rings (e.g. fractions over polynomials etc).
        /// 
        /// Example:
        /// <pre><code>
        /// // GF(17, 3) as polynomials over "t"
        /// FiniteField<UnivariatePolynomialZp64> gf = Rings.GF(17, 3);
        /// // parser of univariate polynomials over "t" from GF(17, 3)
        /// Coder<UnivariatePolynomialZp64, ?, ?> gfParser = Coder.mkUnivariateCoder(gf, mkVars(gf, "t"));
        /// 
        /// // ring GF(17, 3)[x, y, z]
        /// MultivariateRing<MultivariatePolynomial<UnivariatePolynomialZp64>> polyRing = Rings.MultivariateRing(3, gf);
        /// // parser of multivariate polynomials over GF(17, 3)
        /// Coder<MultivariatePolynomial<UnivariatePolynomialZp64>, ?, ?> polyParser = Coder.mkMultivariateCoder(polyRing,
        /// gfParser, "x", "y", "z");
        /// 
        /// // field Frac(GF(17, 3)[x, y, z])
        /// Rationals<MultivariatePolynomial<UnivariatePolynomialZp64>> fracRing = Rings.Frac(polyRing);
        /// // parser of elements in Frac(GF(17, 3)[x, y, z])
        /// Coder<Rational<MultivariatePolynomial<UnivariatePolynomialZp64>>, ?, ?> fracParser =
        ///              Coder.mkNestedCoder(
        ///                   fracRing,                        // the frac field
        ///                   new HashMap<>(),                 // variables (no any)
        ///                   polyParser,                      // parser of multivariate polynomials
        ///                   p -> new Rational<>(polyRing, p) // convert from polys to fractions
        ///              );
        /// </code></pre>
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="variables">map string_variable -> ring_element</param>
        /// <param name="innerCoder">coder for underlying ring elements</param>
        /// <param name="imageFunc">mapping from @{code I} to @{code E}</param>
        public static Coder<E, ?, ?> MkNestedCoder<E, I>(Ring<E> ring, Dictionary<string, E> variables, Coder<I, ?, ?> innerCoder, SerializableFunction<I, E> imageFunc)
        {
            if (ring is MultivariateRing && ((MultivariateRing)ring).Factory() is MultivariatePolynomial)
                return MkMultivariateCoder((MultivariateRing)ring, innerCoder, (Dictionary)variables);
            else if (ring is UnivariateRing && ((UnivariateRing)ring).Factory() is UnivariatePolynomial)
                return MkUnivariateCoder((UnivariateRing)ring, innerCoder, (Dictionary)variables);
            innerCoder.eVariables.ForEach((k, v) => variables.Put(k, imageFunc.Apply(v)));
            return new Coder(ring, variables, innerCoder.polyRing, innerCoder.pVariables, innerCoder.polyToElement == null ? null : innerCoder.polyToElement.AndThen(imageFunc)).WithEncoder(innerCoder);
        }

        
        
        ///////////////////////////////////////////////////Implementation///////////////////////////////////////////////////
        /// <summary>
        /// Parse string
        /// </summary>
        public virtual Element Parse(string @string)
        {
            return Parse(Tokenizer.MkTokenizer(@string));
        }

        
        /// <summary>
        /// Parse stream of tokens into ring element
        /// </summary>
        public virtual Element Parse(Tokenizer tokenizer)
        {

            // operators stack
            ArrayDeque<Operator> operators = new ArrayDeque();

            // operands stack
            ArrayDeque<IOperand<Poly, Element>> operands = new ArrayDeque();
            Tokenizer.TokenType previousToken = null;
            Tokenizer.Token token;
            while ((token = tokenizer.NextToken()) != END)
            {
                Tokenizer.TokenType tType = token.tokenType;

                // if this is variable, push it to stack
                if (tType == T_VARIABLE)
                    operands.Push(MkOperand(token));
                else
                {
                    Operator op = TokenToOp(token.tokenType);

                    // manage unary operations
                    if (Operator.IsPlusMinus(op) && previousToken == T_BRACKET_OPEN)
                        operands.Push(Zero); // push neutral element
                    if (Operator.IsPlusMinus(op) && Operator.IsPlusMinus(TokenToOp(previousToken)))
                    {
                        if (op == Operator.PLUS && previousToken == T_MINUS)
                        {

                            // manage --
                            tType = T_MINUS;
                            op = Operator.MINUS;
                        }
                        else if (op == Operator.MINUS && previousToken == T_MINUS)
                        {

                            // manage -+
                            tType = T_PLUS;
                            op = Operator.PLUS;
                        }

                        operands.Push(Zero); // push neutral element
                    }

                    if (op == Operator.BRACKET_CLOSE)
                    {
                        while (!operators.IsEmpty() && operators.Peek() != Operator.BRACKET_OPEN)
                            PopEvaluate(operators, operands);
                        operators.Pop(); // remove opening bracket
                    }
                    else
                    {
                        while (CanPop(op, operators))
                            PopEvaluate(operators, operands);
                        operators.Push(op);
                    }
                }

                previousToken = tType;
            }

            if (operands.Count > 1 || operators.Count > 1)
                throw new ArgumentException("Can't parse");
            return baseRing.ValueOf(operands.Pop().ToElement());
        }

        
        /// <summary>
        /// parse operand token
        /// </summary>
        private IOperand<Poly, Element> MkOperand(Tokenizer.Token operand)
        {
            if (IsInteger(operand.content))
                return new NumberOperand(new BigInteger(operand.content));
            if (operand.tokenType != T_VARIABLE)
                throw new Exception("illegal operand: " + operand);

            // if polynomial
            int iVar = pVariables == null ? null : pVariables[operand.content];
            if (iVar != null)
                return new VarOperand(pVariables[operand.content]);

            // if base ring element
            Element eVar = eVariables[operand.content];
            if (eVar != null)
                return new ElementOperand(baseRing.Copy(eVar));
            throw new Exception("illegal operand: " + operand);
        }

        
        /// <summary>
        /// whether can pop element from ops stack
        /// </summary>
        private bool CanPop(Operator op, ArrayDeque<Operator> opsStack)
        {
            if (opsStack.IsEmpty())
                return false;
            int pOp = op.priority;
            int pOpPrev = opsStack.Peek().priority;
            if (pOp < 0 || pOpPrev < 0)
                return false;
            return (op.associativity == Associativity.LeftToRight && pOp >= pOpPrev) || (op.associativity == Associativity.RightToLeft && pOp > pOpPrev);
        }

        
        /// <summary>
        /// pop two elements from operands stack and apply binary op from ops stack
        /// </summary>
        private void PopEvaluate(ArrayDeque<Operator> opsStack, ArrayDeque<IOperand<Poly, Element>> exprStack)
        {
            IOperand<Poly, Element> right = exprStack.Pop(), left = exprStack.Pop(), result;
            Operator op = opsStack.Pop();
            switch (op)
            {
                case PLUS:
                    result = left.Plus(right);
                    break;
                case MINUS:
                    result = left.Minus(right);
                    break;
                case MULTIPLY:
                    result = left.Multiply(right);
                    break;
                case DIVIDE:
                    result = left.Divide(right);
                    break;
                case POWER:
                    if (!(right is Coder.NumberOperand))
                        throw new ArgumentException("Exponents must be positive integers, but got " + right.ToElement());
                    result = left.Pow(((NumberOperand)right).number);
                    break;
                default:
                    throw new Exception();
                    break;
            }

            exprStack.Push(result);
        }

        
        //////////////////////////////////////////////////////Operands//////////////////////////////////////////////////////
        /// <summary>
        /// optimized operations with operands
        /// </summary>
        private interface IOperand<P, E> : Serializable
        {
            /// <summary>
            /// to auxiliary poly
            /// </summary>
            P ToPoly();
    
            /// <summary>
            /// to base ring element
            /// </summary>
            E ToElement();

            /// <summary>
            /// whether this operand is already converted to a base ring element
            /// </summary>
            bool InBaseRing()
            {
                return this is Coder.ElementOperand;
            }

         
            IOperand<P, E> Plus(IOperand<P, E> oth);


            IOperand<P, E> Minus(IOperand<P, E> oth);
            
            IOperand<P, E> Multiply(IOperand<P, E> oth);
            
            IOperand<P, E> Divide(IOperand<P, E> oth);
            
            IOperand<P, E> Pow(BigInteger exponent);
        }

        
      
        /// <summary>
        /// default implementation of operands algebra
        /// </summary>
        private abstract class DefaultOperandOps : IOperand<Poly, Element>
        {
            public virtual Element ToElement()
            {
                return polyToElement.Apply(ToPoly());
            }

            public virtual IOperand<Poly, Element> Plus(IOperand<Poly, Element> oth)
            {
                if (InBaseRing() || oth.InBaseRing())
                    return new ElementOperand(baseRing.AddMutable(ToElement(), oth.ToElement()));
                else
                    return new PolyOperand(polyRing.AddMutable(ToPoly(), oth.ToPoly()));
            }

            public virtual IOperand<Poly, Element> Minus(IOperand<Poly, Element> oth)
            {
                if (InBaseRing() || oth.InBaseRing())
                    return new ElementOperand(baseRing.SubtractMutable(ToElement(), oth.ToElement()));
                else
                    return new PolyOperand(polyRing.SubtractMutable(ToPoly(), oth.ToPoly()));
            }

            public virtual IOperand<Poly, Element> Divide(IOperand<Poly, Element> oth)
            {
                return new ElementOperand(baseRing.DivideExactMutable(ToElement(), oth.ToElement()));
            }

            public virtual IOperand<Poly, Element> Multiply(IOperand<Poly, Element> oth)
            {
                if (InBaseRing() || oth.InBaseRing())
                    return new ElementOperand(baseRing.MultiplyMutable(ToElement(), oth.ToElement()));
                else
                    return new PolyOperand(polyRing.MultiplyMutable(ToPoly(), oth.ToPoly()));
            }

            public virtual IOperand<Poly, Element> Pow(BigInteger exponent)
            {
                if (InBaseRing())
                    return new ElementOperand(baseRing.Pow(ToElement(), exponent));
                else
                    return new PolyOperand(polyRing.Pow(ToPoly(), exponent));
            }
        }

        
 
        /// <summary>
        /// zero operand
        /// </summary>
        private readonly NumberOperand Zero = new NumberOperand(BigInteger.ZERO);
        
    
        /// <summary>
        /// A single number
        /// </summary>
        private sealed class NumberOperand : DefaultOperandOps, IOperand<Poly, Element>
        {
            readonly BigInteger number;
            NumberOperand(BigInteger number)
            {
                this.number = number;
            }

            public override Poly ToPoly()
            {
                return polyRing.ValueOfBigInteger(number);
            }

            public override Element ToElement()
            {
                return baseRing.ValueOfBigInteger(number);
            }

            public override IOperand<Poly, Element> Plus(IOperand<Poly, Element> oth)
            {
                if (oth is Coder.NumberOperand)
                {
                    return new NumberOperand(number.Add(((NumberOperand)oth).number));
                }
                else
                    return base.Plus(oth);
            }

            public override IOperand<Poly, Element> Minus(IOperand<Poly, Element> oth)
            {
                if (oth is Coder.NumberOperand)
                {
                    return new NumberOperand(number.Subtract(((NumberOperand)oth).number));
                }
                else
                    return base.Minus(oth);
            }

            public override IOperand<Poly, Element> Multiply(IOperand<Poly, Element> oth)
            {
                if (oth is Coder.NumberOperand)
                {
                    return new NumberOperand(number.Multiply(((NumberOperand)oth).number));
                }
                else
                    return oth.Multiply(this);
            }

            public override IOperand<Poly, Element> Divide(IOperand<Poly, Element> oth)
            {
                if (oth is Coder.NumberOperand)
                {
                    BigInteger[] divRem = this.number.DivideAndRemainder(((NumberOperand)oth).number);
                    if (divRem[1].IsZero())
                        return new NumberOperand(divRem[0]);
                    else
                        return base.Divide(oth);
                }
                else
                    return base.Divide(oth);
            }

            public override IOperand<Poly, Element> Pow(BigInteger exponent)
            {
                return new NumberOperand(Rings.Z.Pow(number, exponent));
            }
        }

        
      
        /// <summary>
        /// A single variable
        /// </summary>
        private sealed class VarOperand : DefaultOperandOps, IOperand<Poly, Element>
        {
            readonly int variable;
            VarOperand(int variable)
            {
                this.variable = variable;
            }

            public override Poly ToPoly()
            {
                return polyRing.Variable(variable);
            }

            public override IOperand<Poly, Element> Multiply(IOperand<Poly, Element> oth)
            {
                if (oth is Coder.NumberOperand)
                {
                    return new MonomialOperand(polyRing.MultiplyMutable(ToPoly(), oth.ToPoly()).Lt());
                }
                else if (oth is Coder.VarOperand)
                {
                    int[] exponents = new int[polyRing.NVariables()];
                    exponents[variable] += 1;
                    exponents[((VarOperand)oth).variable] += 1;
                    return new MonomialOperand(polyRing.Factory().monomialAlgebra.Create(exponents));
                }

                return oth.Multiply(this);
            }

            public override IOperand<Poly, Element> Pow(BigInteger exponent)
            {
                if (!exponent.IsInt())
                    return base.Pow(exponent);
                int[] exponents = new int[polyRing.NVariables()];
                exponents[variable] += exponent.IntValue();
                return new MonomialOperand(polyRing.Factory().monomialAlgebra.Create(exponents));
            }
        }

        
      
        /// <summary>
        /// A single monomial (x*y^2*z etc)
        /// </summary>
        private class MonomialOperand : DefaultOperandOps, IOperand<Poly, Element>
        {
            Term term;
            MonomialOperand(Term term)
            {
                this.term = term;
            }

            public override Poly ToPoly()
            {
                return polyRing.Factory().Create(term);
            }

            public override IOperand<Poly, Element> Multiply(IOperand<Poly, Element> oth)
            {
                IMonomialAlgebra<Term> monomialAlgebra = polyRing.MonomialAlgebra();
                if (oth is Coder.NumberOperand)
                {
                    return new MonomialOperand(monomialAlgebra.Multiply(term, ((NumberOperand)oth).number));
                }
                else if (oth is Coder.VarOperand)
                {
                    int[] exponents = term.exponents;
                    exponents[((VarOperand)oth).variable] += 1;
                    return new MonomialOperand(term.ForceSetDegreeVector(exponents, term.totalDegree + 1));
                }
                else if (oth is Coder.MonomialOperand)
                {
                    Term othTerm = ((MonomialOperand)oth).term;
                    if (((long)othTerm.totalDegree) + term.totalDegree > Short.MAX_VALUE)
                        return base.Multiply(oth);
                    return new MonomialOperand(monomialAlgebra.Multiply(term, othTerm));
                }

                return oth.Multiply(this);
            }

            public override IOperand<Poly, Element> Pow(BigInteger exponent)
            {
                if (exponent.IsInt())
                {
                    int e = exponent.IntValue();
                    if (((long)term.totalDegree) * e > Short.MAX_VALUE)
                        return base.Pow(exponent);
                    IMonomialAlgebra<Term> ma = polyRing.MonomialAlgebra();
                    return new MonomialOperand(ma.Pow(term, e));
                }

                return base.Pow(exponent);
            }
        }
        
        /// <summary>
        /// A single polynomial
        /// </summary>
        private class PolyOperand : DefaultOperandOps, IOperand<Poly, Element>
        {
            readonly Poly poly;
            PolyOperand(Poly poly)
            {
                this.poly = poly;
            }

            public override Poly ToPoly()
            {
                return poly;
            }
        }

        
      
        /// <summary>
        /// Base ring element
        /// </summary>
        private class ElementOperand : DefaultOperandOps, IOperand<Poly, Element>
        {
            readonly Element element;
            ElementOperand(Element element)
            {
                this.element = element;
            }

            public override Poly ToPoly()
            {
                throw new NotSupportedException();
            }

            public override Element ToElement()
            {
                return element;
            }
        }

        
        /////////////////////////////////////////////////////Operators//////////////////////////////////////////////////////
        private enum Associativity
        {
            LeftToRight,
            RightToLeft
        }

        
        /// <summary>
        /// Operators
        /// </summary>
        private enum Operator
        {
            // dummy ops
            // // dummy ops
            // BRACKET_OPEN(null, -1)
            BRACKET_OPEN,
            // BRACKET_CLOSE(null, -1)
            BRACKET_CLOSE,
            // priority = 2
            // // priority = 2
            // POWER(Associativity.LeftToRight, 20)
            POWER,
            // priority = 3
            // // priority = 3
            // UNARY_PLUS(Associativity.RightToLeft, 30)
            UNARY_PLUS,
            // UNARY_MINUS(Associativity.RightToLeft, 30)
            UNARY_MINUS,
            // priority = 5
            // // priority = 5
            // MULTIPLY(Associativity.LeftToRight, 50)
            MULTIPLY,
            // DIVIDE(Associativity.LeftToRight, 50)
            DIVIDE,
            // priority = 6
            // // priority = 6
            // PLUS(Associativity.LeftToRight, 60)
            PLUS,
            // MINUS(Associativity.LeftToRight, 60)
            MINUS 

            // --------------------
            // TODO enum body members
            // final Associativity associativity;
            // final int priority;
            // Operator(Associativity associativity, int priority) {
            //     this.associativity = associativity;
            //     this.priority = priority;
            // }
            // static boolean isPlusMinus(Operator op) {
            //     return PLUS == op || MINUS == op;
            // }
            // static Operator toUnaryPlusMinus(Operator op) {
            //     return op == PLUS ? UNARY_PLUS : op == MINUS ? UNARY_MINUS : null;
            // }
            // --------------------
        }

        
      
        /// <summary>
        /// convert token to operator
        /// </summary>
        private static Operator TokenToOp(Tokenizer.TokenType tType)
        {
            return tokenToOp[(int) tType];
        }

        
        
        private static readonly Operator[] tokenToOp;
        
        
        static Coder()
        {
            tokenToOp = new Operator[Tokenizer.TokenType.Values.Length];
            tokenToOp[Tokenizer.TokenType.T_BRACKET_OPEN.Ordinal()] = Operator.BRACKET_OPEN;
            tokenToOp[Tokenizer.TokenType.T_BRACKET_CLOSE.Ordinal()] = Operator.BRACKET_CLOSE;
            tokenToOp[Tokenizer.TokenType.T_MULTIPLY.Ordinal()] = Operator.MULTIPLY;
            tokenToOp[Tokenizer.TokenType.T_DIVIDE.Ordinal()] = Operator.DIVIDE;
            tokenToOp[Tokenizer.TokenType.T_PLUS.Ordinal()] = Operator.PLUS;
            tokenToOp[Tokenizer.TokenType.T_MINUS.Ordinal()] = Operator.MINUS;
            tokenToOp[Tokenizer.TokenType.T_EXPONENT.Ordinal()] = Operator.POWER;
        }

        
        
        private static bool IsInteger(string? str)
        {
            if (str == null)
            {
                return false;
            }

            int length = str.Length;
            if (length == 0)
            {
                return false;
            }

            int i = 0;
            if (str[0] == '-')
            {
                if (length == 1)
                {
                    return false;
                }

                i = 1;
            }

            for (; i < length; i++)
            {
                char c = str[i];
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }

            return true;
        }
    }
}