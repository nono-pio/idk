using System.Numerics;
using Polynomials.Poly.Multivar;
using Polynomials.Poly.Univar;
using Polynomials.Utils;

namespace Polynomials.Poly;

public interface IMultipleFieldExtension;
public class MultipleFieldExtension<E> : ImageRing<UnivariatePolynomial<E>, MultivariatePolynomial<E>>, IMultipleFieldExtension
{
    readonly MultivariatePolynomial<E> mFactory;


    readonly UnivariatePolynomial<E> sFactory;


    readonly MultipleFieldExtension<E>[] tower;


    readonly UnivariatePolynomial<MultivariatePolynomial<E>>[] minimalPolynomialsOfGenerators;


    readonly MultivariatePolynomial<E> primitiveElement;


    readonly SimpleFieldExtension<E> simpleExtension;


    readonly UnivariatePolynomial<E>[] generatorsReps;


    public MultipleFieldExtension(MultipleFieldExtension<E>[] tower,
        UnivariatePolynomial<MultivariatePolynomial<E>>[] minimalPolynomialsOfGenerators,
        MultivariatePolynomial<E> primitiveElement,
        UnivariatePolynomial<E>[] generatorsReps,
        SimpleFieldExtension<E> simpleExtension)
        : base(simpleExtension,
            new MultipleToSimple(primitiveElement, simpleExtension, generatorsReps).Apply,
            new SimpleToMultiple(primitiveElement, simpleExtension, generatorsReps, tower,
                minimalPolynomialsOfGenerators).Apply)
    {
        this.tower = tower;
        this.minimalPolynomialsOfGenerators = minimalPolynomialsOfGenerators;
        this.primitiveElement = primitiveElement;
        this.simpleExtension = simpleExtension;
        this.generatorsReps = generatorsReps;
        this.mFactory = primitiveElement.CreateOne();
        this.sFactory = simpleExtension.Factory().CreateOne();
    }


    public int NVariables()
    {
        return mFactory.nVariables;
    }


    public MultivariatePolynomial<E> Factory()
    {
        return mFactory;
    }


    public MultivariatePolynomial<E> Variable(int variable)
    {
        return mFactory.CreateMonomial(variable, 1);
    }


    public virtual UnivariatePolynomial<E> GetUnivariateFactory()
    {
        return sFactory;
    }


    public virtual MultivariatePolynomial<E> GetPrimitiveElement()
    {
        return primitiveElement.Clone();
    }


    public virtual int Degree()
    {
        return simpleExtension.Degree();
    }


    public virtual SimpleFieldExtension<E> GetSimpleExtension()
    {
        return simpleExtension;
    }


    public virtual UnivariatePolynomial<MultivariatePolynomial<E>> GetGeneratorMinimalPoly(int iGenerator)
    {
        return minimalPolynomialsOfGenerators[iGenerator].Clone();
    }


    public virtual MultipleFieldExtension<E> GetSubExtension(int i)
    {
        return tower[i];
    }


    public virtual UnivariatePolynomial<E> GetGeneratorRep(int iGenerator)
    {
        return generatorsReps[iGenerator].Clone();
    }


    public virtual UnivariatePolynomial<E>[] GetGeneratorReps()
    {
        return (UnivariatePolynomial<E>[])generatorsReps.Clone();
    }


    public virtual MultipleFieldExtension<E> JoinAlgebraicElement(
        UnivariatePolynomial<MultivariatePolynomial<E>> algebraicElement)
    {
        UnivariatePolynomial<UnivariatePolynomial<E>> minimalPoly =
            algebraicElement.MapCoefficients(this.simpleExtension, Inverse);
        MultipleFieldExtension<E> ext = MkMultipleExtension0(this.simpleExtension.minimalPoly, minimalPoly);
        MultipleFieldExtension<E>[] tower = [..this.tower, this];
        UnivariatePolynomial<MultivariatePolynomial<E>>[] minPolys = [..this.minimalPolynomialsOfGenerators, algebraicElement];
        MultivariatePolynomial<E> primitiveElement = ext.primitiveElement.InsertVariable(1, NVariables() - 1)
            .Composition(0, this.primitiveElement.JoinNewVariable());
        SimpleFieldExtension<E> simpleExtension = ext.simpleExtension;
        UnivariatePolynomial<E>[] generatorsReps = this.generatorsReps
            .Select((rep) => rep.Composition(simpleExtension, ext.generatorsReps[0]))
            .Append(ext.generatorsReps[1])
            .ToArray();
        return new MultipleFieldExtension<E>(tower, minPolys, primitiveElement, generatorsReps, simpleExtension);
    }


    public virtual MultipleFieldExtension<E> JoinAlgebraicElement(UnivariatePolynomial<E> minimalPoly)
    {
        return JoinAlgebraicElement(minimalPoly.MapCoefficientsAsPolys(this, this.Image));
    }


    public virtual MultipleFieldExtension<E> JoinRedundantElement(MultivariatePolynomial<E> element)
    {
        UnivariatePolynomial<MultivariatePolynomial<E>> minimalPoly =
            UnivariatePolynomial<MultivariatePolynomial<E>>.Create(this, Negate(element), GetOne());
        MultipleFieldExtension<E>[] tower = [..this.tower, this];
        UnivariatePolynomial<MultivariatePolynomial<E>>[] minPolys =
            [..this.minimalPolynomialsOfGenerators, minimalPoly];
        MultivariatePolynomial<E> primitiveElement = this.primitiveElement.JoinNewVariable();
        SimpleFieldExtension<E> simpleExtension = this.simpleExtension;
        UnivariatePolynomial<E>[] generatorsReps = [..this.generatorsReps, Inverse(element)];
        return new MultipleFieldExtension<E>(tower, minPolys, primitiveElement, generatorsReps, simpleExtension);
    }


    public override MultivariatePolynomial<E> ValueOfLong(long val)
    {
        return mFactory.CreateConstant(mFactory.ring.ValueOfLong(val));
    }


    public override MultivariatePolynomial<E> ValueOfBigInteger(BigInteger val)
    {
        return mFactory.CreateOne().MultiplyByBigInteger(val);
    }


    public override MultivariatePolynomial<E> GetZero()
    {
        return mFactory.CreateZero();
    }


    public override MultivariatePolynomial<E> GetOne()
    {
        return mFactory.CreateOne();
    }


    public override MultivariatePolynomial<E> Copy(MultivariatePolynomial<E> element)
    {
        return element.Clone();
    }


    public override bool IsZero(MultivariatePolynomial<E> element)
    {
        return element.IsZero();
    }


    public override bool IsOne(MultivariatePolynomial<E> element)
    {
        return element.IsOne();
    }


    public override bool IsUnit(MultivariatePolynomial<E> element)
    {
        return IsField() ? !IsZero(element) : (IsOne(element) || IsMinusOne(element));
    }


    public override MultivariatePolynomial<E> Gcd(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        return IsField() ? a.Clone() : GetOne();
    }


    public override MultivariatePolynomial<E>[] ExtendedGCD(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        return base.ExtendedGCD(a, b);
    }


    public override MultivariatePolynomial<E> Lcm(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        return base.Lcm(a, b);
    }


    public override MultivariatePolynomial<E> Gcd(params MultivariatePolynomial<E>[] elements)
    {
        return base.Gcd(elements);
    }


    public override MultivariatePolynomial<E> Gcd(IEnumerable<MultivariatePolynomial<E>> elements)
    {
        return base.Gcd(elements);
    }


    public override int Signum(MultivariatePolynomial<E> element)
    {
        return element.SignumOfLC();
    }


    public override MultivariatePolynomial<E> Factorial(long num)
    {
        return base.Factorial(num);
    }


    public override bool Equals(object o)
    {
        if (this == o)
            return true;
        if (o == null || GetType() != o.GetType())
            return false;
        if (!base.Equals(o))
            return false;
        MultipleFieldExtension<E> that = (MultipleFieldExtension<E>)o;
        return Enumerable.SequenceEqual(minimalPolynomialsOfGenerators, that.minimalPolynomialsOfGenerators);
    }


    public override int GetHashCode()
    {
        int result = base.GetHashCode();
        result = 31 * result + minimalPolynomialsOfGenerators.GetHashCode();
        return result;
    }


    class MappingFunc
    {
        protected readonly MultivariatePolynomial<E> primitiveElement;


        protected readonly SimpleFieldExtension<E> simpleExtension;


        protected readonly UnivariatePolynomial<E>[] generatorsReps;


        public MappingFunc(MultivariatePolynomial<E> primitiveElement, SimpleFieldExtension<E> simpleExtension,
            UnivariatePolynomial<E>[] generatorsReps)
        {
            this.primitiveElement = primitiveElement;
            this.simpleExtension = simpleExtension;
            this.generatorsReps = generatorsReps;
        }


        public virtual bool Equals(object o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            MappingFunc that = (MappingFunc)o;
            return Equals(primitiveElement, that.primitiveElement) &&
                   Equals(generatorsReps, that.generatorsReps);
        }


        public override int GetHashCode()
        {
            int result = primitiveElement.GetHashCode();
            result = 31 * result + generatorsReps.GetHashCode();
            return result;
        }
    }


    sealed class SimpleToMultiple : MappingFunc
    {
        readonly MultipleFieldExtension<E>[] tower;
        readonly UnivariatePolynomial<MultivariatePolynomial<E>>[] minimalPolynomials;

        public SimpleToMultiple(MultivariatePolynomial<E> primitiveElement, SimpleFieldExtension<E> simpleExtension,
            UnivariatePolynomial<E>[] generatorsReps, MultipleFieldExtension<E>[] tower,
            UnivariatePolynomial<MultivariatePolynomial<E>>[] minimalPolynomials) : base(primitiveElement,
            simpleExtension, generatorsReps)
        {
            this.tower = tower;
            this.minimalPolynomials = minimalPolynomials;
        }

        public MultivariatePolynomial<E> Apply(UnivariatePolynomial<E> sPoly)
        {
            MultivariatePolynomial<E> r = (MultivariatePolynomial<E>)sPoly.Composition(this.primitiveElement);
            if (r.nVariables > 1)
            {
                MultipleFieldExtension<E> prevExt = tower[tower.Length - 1];
                int variable = r.nVariables - 1;
                r = MultivariatePolynomial<E>.AsMultivariate(
                    UnivariateDivision.Remainder(r.AsUnivariateEliminate(variable).SetRingUnsafe(prevExt),
                        minimalPolynomials[variable], false), variable, true);
            }


            //            for (int i = minimalPolynomials.length - 1; i >= 0; --i) {
            //                MonomialOrder.EliminationOrder order = new MonomialOrder.EliminationOrder(MonomialOrder.LEX, i);
            //                mPoly gen = i == 0
            //                        ? AMultivariatePolynomial.asMultivariate(minimalPolynomials[i], i + 1, true).dropVariable(0).joinNewVariables(minimalPolynomials.length - i - 1)
            //                        : AMultivariatePolynomial.asMultivariate(minimalPolynomials[i], i, true).joinNewVariables(minimalPolynomials.length - i - 1);
            //                r = r.setOrdering(order);
            //                gen = gen.setOrdering(order);
            //                r = MultivariateDivision.remainder(r, gen);
            //            }
            return r;
        }
    }


    sealed class MultipleToSimple : MappingFunc
    {
        public MultipleToSimple(MultivariatePolynomial<E> primitiveElement, SimpleFieldExtension<E> simpleExtension,
            UnivariatePolynomial<E>[] generatorsReps) :
            base(primitiveElement, simpleExtension, generatorsReps)
        {
        }

        public UnivariatePolynomial<E> Apply(MultivariatePolynomial<E> mPoly)
        {
            return mPoly.Composition(this.simpleExtension, this.generatorsReps);
        }
    }


    public static MultipleFieldExtension<E> MkMultipleExtension(UnivariatePolynomial<E> a)
    {
        return MkMultipleExtension(Rings.SimpleFieldExtension(a));
    }


    public static MultipleFieldExtension<E> MkMultipleExtension(
        SimpleFieldExtension<E> ext)
    {
        var m = ext.GetMinimalPolynomial();
        return new MultipleFieldExtension<E>(new MultipleFieldExtension<E>[0],
        [
            m.MapCoefficientsAsPolys(Rings.MultivariateRing(m.AsMultivariate()),
                    (cf) => cf.AsMultivariate())
        ], m.AsMultivariate().CreateMonomial(0, 1), [m.CreateMonomial(1)], ext);
    }


    private static MultipleFieldExtension<E> MkMultipleExtension0(UnivariatePolynomial<E> a,
        UnivariatePolynomial<UnivariatePolynomial<E>> b)
    {
        MultivariatePolynomial<E> ma = a.AsMultivariate().InsertVariable(0), factory = ma.CreateOne();
        for (int s = 0;; ++s)
        {
            // prepare a(x) and b(x, alpha) to compute resultant
            MultivariatePolynomial<E> mb = AsBivariate(b);

            // compute b(x - s*alpha, alpha)
            if (s != 0)
                mb = mb.Composition(0, factory.CreateMonomial(0, 1).Subtract(factory.CreateMonomial(1, 1).Multiply(s)));

            // compute h(x) = Res(a(x), b(x - s*alpha, alpha), alpha)
            UnivariatePolynomial<E> primitiveElement = MultivariateResultants.Resultant(ma, mb, 1).AsUnivariate();
            if (!UnivariateSquareFreeFactorization.IsSquareFree(primitiveElement))
                continue;

            // h(x) is the minimal polynomial of primitive element
            SimpleFieldExtension<E> extension = Rings.SimpleFieldExtension(primitiveElement);

            // compute gcd( a(X), b(gamma - s*X, X) )
            UnivariatePolynomial<UnivariatePolynomial<E>> aE = a.MapCoefficientsAsPolys(extension, extension.ValueOf),
                bE = mb.MapCoefficients(extension, extension.factory.CreateConstant)
                    .Composition(aE.CreateConstant(extension.Generator()), aE.CreateMonomial(1)),
                gcd = UnivariateGCD.PolynomialGCD(aE, bE).Monic();

            // representations
            UnivariatePolynomial<E> aRep = extension.Negate(gcd.Cc());
            UnivariatePolynomial<E> bRep = extension.Subtract(extension.Generator(), aRep.Clone().Multiply(s));

            // second extension
            MultipleFieldExtension<E> result =
                new MultipleFieldExtension<E>(null, null,
                    factory.CreateMonomial(1, 1).Add(factory.CreateMonomial(0, 1).Multiply(s)),
                    [aRep, bRep], extension);
            return result;
        }
    }


    private static MultivariatePolynomial<E> AsBivariate(
        UnivariatePolynomial<UnivariatePolynomial<E>> b)
    {

            return MultivariatePolynomial<E>.AsNormalMultivariate(b.AsMultivariate(), 1);
    }

   

    public static MultipleFieldExtension<E> MkMultipleExtension(params UnivariatePolynomial<E>[]
        minimalPolynomials)
    {
        MultipleFieldExtension<E> ext = MkMultipleExtension(minimalPolynomials[0]);
        for (int i = 1; i < minimalPolynomials.Length; ++i)
            ext = ext.JoinAlgebraicElement(minimalPolynomials[i]);
        return ext;
    }


    public static MultipleFieldExtension<E> MkSplittingField(UnivariatePolynomial<E> poly)
    {
        // basic extension
        MultipleFieldExtension<E> extension = MkMultipleExtension(poly);
        List<UnivariatePolynomial<MultivariatePolynomial<E>>> nonLinearFactors = [];
        nonLinearFactors.Add(poly.MapCoefficientsAsPolys(extension, extension.Image));
        bool first = true;
        while (nonLinearFactors.Count != 0)
        {
            UnivariatePolynomial<MultivariatePolynomial<E>> factor = nonLinearFactors.Pop(nonLinearFactors.Count - 1);
            MultipleFieldExtension<E> nextExt;
            if (first)
            {
                nextExt = extension;
                first = false;
            }
            else
            {
                nextExt = extension.JoinAlgebraicElement(factor);
                MultipleFieldExtension<E> _nextExt = nextExt;
                nonLinearFactors = nonLinearFactors.Select((f) =>
                    f.MapCoefficients(_nextExt, c => c.JoinNewVariable())).ToList();
                factor = factor.MapCoefficients(nextExt, c => c.JoinNewVariable());
            }

            factor = UnivariateDivision.DivideExact(factor,
                UnivariatePolynomial<MultivariatePolynomial<E>>.Create(nextExt, nextExt.Negate(nextExt.Variable(nextExt.NVariables() - 1)),
                    nextExt.GetOne()), false);
            List<UnivariatePolynomial<MultivariatePolynomial<E>>> factors = UnivariateFactorization.Factor(factor).Factors;
            for (int i = 0; i < factors.Count; i++)
            {
                UnivariatePolynomial<MultivariatePolynomial<E>> f = factors[i];
                if (f.Degree() > 1)
                    nonLinearFactors.Add(f);
                else
                {
                    nextExt = nextExt.JoinRedundantElement(nextExt.Negate(nextExt.DivideExact(f.Cc(), f.Lc())));
                    for (int j = i + 1; j < factors.Count; ++j)
                        factors[j] = factors[j].MapCoefficients(nextExt, c => c.JoinNewVariable());
                }
            }

            extension = nextExt;
        }

        return extension;
    }
}