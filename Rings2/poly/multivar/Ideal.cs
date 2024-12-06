using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Io;
using Cc.Redberry.Rings.Poly;
using Cc.Redberry.Rings.Poly.Multivar;
using Java.Io;
using Java;
using Java.Util.Stream;
using Cc.Redberry.Rings.Poly.Multivar.MonomialOrder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Poly.Multivar.RoundingMode;
using static Cc.Redberry.Rings.Poly.Multivar.Associativity;
using static Cc.Redberry.Rings.Poly.Multivar.Operator;
using static Cc.Redberry.Rings.Poly.Multivar.TokenType;
using static Cc.Redberry.Rings.Poly.Multivar.SystemInfo;

namespace Cc.Redberry.Rings.Poly.Multivar
{
    /// <summary>
    /// Ideal represented by its Groebner basis.
    /// </summary>
    /// <remarks>@since2.3</remarks>
    public sealed class Ideal<Term, Poly> : Stringifiable<Poly>, Serializable
    {
        /// <summary>
        /// list of original generators
        /// </summary>
        private readonly IList<Poly> originalGenerators;
        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        public readonly Comparator<DegreeVector> ordering;
        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        private readonly Poly factory;
        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        private readonly IList<Poly> groebnerBasis;
        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        private readonly MultivariateRing<Poly> ring;
        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        private Ideal(IList<Poly> originalGenerators, IList<Poly> groebnerBasis)
        {
            this.originalGenerators = Collections.UnmodifiableList(originalGenerators);
            this.factory = groebnerBasis[0].CreateZero();
            this.groebnerBasis = groebnerBasis;
            this.ordering = factory.ordering;
            this.ring = Rings.MultivariateRing(factory);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        private Ideal(IList<Poly> groebnerBasis) : this(groebnerBasis, groebnerBasis)
        {
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        public Comparator<DegreeVector> GetMonomialOrder()
        {
            return ordering;
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        public Ideal<Term, Poly> ChangeOrder(Comparator<DegreeVector> newMonomialOrder)
        {
            if (ordering == newMonomialOrder)
                return this;
            if (IsGradedOrder(ordering) || !IsGradedOrder(newMonomialOrder))
                return new Ideal(originalGenerators, HilbertConvertBasis(groebnerBasis, newMonomialOrder));
            return Create(originalGenerators, newMonomialOrder);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        private static Poly SetOrdering<Poly extends AMultivariatePolynomial<?, Poly>>(Poly poly, Comparator<DegreeVector> monomialOrder)
        {
            return poly.ordering == monomialOrder ? poly : poly.SetOrdering(monomialOrder);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        private Poly SetOrdering(Poly poly)
        {
            return SetOrdering(poly, ordering);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        private Poly Mod0(Poly poly)
        {
            return MultivariateDivision.PseudoRemainder(SetOrdering(poly), groebnerBasis);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        public Poly NormalForm(Poly poly)
        {
            Comparator<DegreeVector> originalOrder = poly.ordering;
            return SetOrdering(Mod0(poly), originalOrder);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        public IList<Poly> GetOriginalGenerators()
        {
            return originalGenerators;
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        public IList<Poly> GetGroebnerBasis()
        {
            return Collections.UnmodifiableList(groebnerBasis);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        public int NBasisGenerators()
        {
            return groebnerBasis.Count;
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        public Poly GetBasisGenerator(int i)
        {
            return groebnerBasis[i];
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        public bool IsTrivial()
        {
            return NBasisGenerators() == 1 && GetBasisGenerator(0).IsConstant() && !GetBasisGenerator(0).IsZero();
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        public bool IsProper()
        {
            return !IsTrivial();
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        public bool IsEmpty()
        {
            return NBasisGenerators() == 1 && GetBasisGenerator(0).IsZero();
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        public bool IsPrincipal()
        {
            return NBasisGenerators() == 1;
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        public bool IsHomogeneous()
        {
            return IsHomogeneousIdeal(groebnerBasis);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        public bool IsMonomial()
        {
            return IsMonomialIdeal(groebnerBasis);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        public bool IsMaximal()
        {
            return (factory.IsOverZ() || factory.IsOverField()) && Dimension() == 0 && groebnerBasis.Count == factory.nVariables && groebnerBasis.Stream().AllMatch(AMultivariatePolynomial.IsLinearExactly());
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        public Ideal<Term, Poly> LtIdeal()
        {
            if (IsMonomial())
                return this;
            return new Ideal(groebnerBasis.Stream().Map(AMultivariatePolynomial.LtAsPoly()).Collect(Collectors.ToList()));
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        public bool Contains(Poly poly)
        {
            return Mod0(poly).IsZero();
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        public bool Contains(Ideal<Term, Poly> oth)
        {
            return Quotient(oth).IsTrivial();
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        private HilbertSeries hilbertSeries = null;
        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        public HilbertSeries HilbertSeries()
        {
            lock (this)
            {
                if (hilbertSeries == null)
                {
                    if (IsHomogeneous() || IsGradedOrder(ordering))
                        hilbertSeries = HilbertSeriesOfLeadingTermsSet(groebnerBasis);
                    else

                        // use original generators to construct basis when current ordering is "hard"
                        hilbertSeries = HilbertSeriesOfLeadingTermsSet(GroebnerBasisWithOptimizedGradedOrder(originalGenerators));
                }

                return hilbertSeries;
            }
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        public int Dimension()
        {
            return HilbertSeries().Dimension();
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        public int Degree()
        {
            return HilbertSeries().Degree();
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        public bool ContainsProduct(Ideal<Term, Poly> a, Ideal<Term, Poly> b)
        {
            if (a.NBasisGenerators() > b.NBasisGenerators())
                return ContainsProduct(b, a);
            return Quotient(a).Contains(b);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        public bool RadicalContains(Poly poly)
        {

            // adjoin new variable to all generators (convert to F[X][y])
            IList<Poly> yGenerators = groebnerBasis.Stream().Map(AMultivariatePolynomial.JoinNewVariable()).Collect(Collectors.ToList());
            Poly yPoly = poly.JoinNewVariable();

            // add 1 - y*poly
            yGenerators.Add(yPoly.CreateOne().Subtract(yPoly.CreateMonomial(yPoly.nVariables - 1, 1).Multiply(yPoly)));
            return Create(yGenerators).IsTrivial();
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        public Ideal<Term, Poly> Union(Poly oth)
        {
            factory.AssertSameCoefficientRingWith(oth);
            if (oth.IsZero())
                return this;
            if (oth.IsOne())
                return Trivial(factory);
            IList<Poly> l = new List(groebnerBasis);
            l.Add(oth);
            return Create(l, ordering);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        public Ideal<Term, Poly> Union(Ideal<Term, Poly> oth)
        {
            AssertSameDomain(oth);
            if (IsEmpty() || oth.IsTrivial())
                return oth;
            if (oth.IsEmpty() || IsTrivial())
                return this;
            IList<Poly> l = new List();
            l.AddAll(groebnerBasis);
            l.AddAll(oth.groebnerBasis);
            return Create(l, ordering);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        public Ideal<Term, Poly> Multiply(Ideal<Term, Poly> oth)
        {
            AssertSameDomain(oth);
            if (IsTrivial() || oth.IsEmpty())
                return oth;
            if (oth.IsTrivial() || this.IsEmpty())
                return this;
            IList<Poly> generators = new List();
            foreach (Poly a in groebnerBasis)
                foreach (Poly b in oth.groebnerBasis)
                    generators.Add(a.Clone().Multiply(b));
            return Create(generators, ordering);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        public Ideal<Term, Poly> Square()
        {
            return Multiply(this);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        public Ideal<Term, Poly> Pow(int exponent)
        {
            if (exponent < 0)
                throw new ArgumentException();
            if (exponent == 1)
                return this;
            Ideal<Term, Poly> result = Trivial(factory);
            Ideal<Term, Poly> k2p = this;
            for (;;)
            {
                if ((exponent & 1) != 0)
                    result = result.Multiply(k2p);
                exponent >>= 1;
                if (exponent == 0)
                    return result;
                k2p = k2p.Multiply(k2p);
            }
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        public Ideal<Term, Poly> Multiply(Poly oth)
        {
            factory.AssertSameCoefficientRingWith(oth);
            if (IsTrivial())
                return Create(Collections.SingletonList(oth), ordering);
            if (oth.IsZero())
                return Trivial(oth, ordering);
            if (oth.IsOne() || this.IsEmpty())
                return this;
            return new Ideal(Canonicalize(groebnerBasis.Stream().Map((p) => p.Clone().Multiply(oth)).Collect(Collectors.ToList())));
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        public Ideal<Term, Poly> Intersection(Ideal<Term, Poly> oth)
        {
            AssertSameDomain(oth);
            if (IsTrivial() || oth.IsEmpty())
                return oth;
            if (oth.IsTrivial() || this.IsEmpty())
                return this;
            if (IsPrincipal() && oth.IsPrincipal())

                // intersection of principal ideals is easy
                return Create(Collections.SingletonList(ring.Lcm(GetBasisGenerator(0), oth.GetBasisGenerator(0))), ordering);

            // we compute (t * I + (1 - t) * J) ∩ R[X]
            Poly t = factory.InsertVariable(0).CreateMonomial(0, 1);
            IList<Poly> tGenerators = new List();
            foreach (Poly gI in this.groebnerBasis)
                tGenerators.Add(gI.InsertVariable(0).Multiply(t));
            Poly omt = t.Clone().Negate().Increment(); // 1 - t
            foreach (Poly gJ in oth.groebnerBasis)
                tGenerators.Add(gJ.InsertVariable(0).Multiply(omt));

            // elimination
            IList<Poly> result = GroebnerMethods.Eliminate(tGenerators, 0).Stream().Map((p) => p.DropVariable(0)).Map((p) => p.SetOrdering(ordering)).Collect(Collectors.ToList());
            return Create(result, ordering);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        // intersection of principal ideals is easy
        // we compute (t * I + (1 - t) * J) ∩ R[X]
        // 1 - t
        // elimination
        // <- restore order!
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        public Ideal<Term, Poly> Quotient(Poly oth)
        {
            if (oth.IsZero())
                return Trivial(factory);
            if (oth.IsConstant())
                return this;
            return Create(Intersection(Create(oth)).groebnerBasis.Stream().Map((p) => ring.Quotient(p, oth)).Collect(Collectors.ToList()));
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        // intersection of principal ideals is easy
        // we compute (t * I + (1 - t) * J) ∩ R[X]
        // 1 - t
        // elimination
        // <- restore order!
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        public Ideal<Term, Poly> Quotient(Ideal<Term, Poly> oth)
        {
            if (oth.IsEmpty())
                return Trivial(factory);
            if (oth.IsTrivial())
                return this;
            return oth.groebnerBasis.Stream().Map(this.Quotient()).Reduce(Trivial(factory), Ideal.Intersection());
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        // intersection of principal ideals is easy
        // we compute (t * I + (1 - t) * J) ∩ R[X]
        // 1 - t
        // elimination
        // <- restore order!
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        Ideal<Term, Poly> InsertVariable(int variable)
        {
            return new Ideal(groebnerBasis.Stream().Map((p) => p.InsertVariable(variable)).Collect(Collectors.ToList()));
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        // intersection of principal ideals is easy
        // we compute (t * I + (1 - t) * J) ∩ R[X]
        // 1 - t
        // elimination
        // <- restore order!
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        private void AssertSameDomain(Ideal<Term, Poly> oth)
        {
            factory.AssertSameCoefficientRingWith(oth.factory);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        // intersection of principal ideals is easy
        // we compute (t * I + (1 - t) * J) ∩ R[X]
        // 1 - t
        // elimination
        // <- restore order!
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        public bool Equals(object o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            Ideal<?, ?> ideal = (Ideal<?, ?>)o;
            return ordering.Equals(ideal.ordering) && groebnerBasis.Equals(ideal.groebnerBasis);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        // intersection of principal ideals is easy
        // we compute (t * I + (1 - t) * J) ∩ R[X]
        // 1 - t
        // elimination
        // <- restore order!
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        public int GetHashCode()
        {
            return groebnerBasis.GetHashCode();
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        // intersection of principal ideals is easy
        // we compute (t * I + (1 - t) * J) ∩ R[X]
        // 1 - t
        // elimination
        // <- restore order!
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        public string ToString(IStringifier<Poly> stringifier)
        {
            return "<" + groebnerBasis.Stream().Map(stringifier.Stringify()).Collect(Collectors.Joining(", ")) + ">";
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        // intersection of principal ideals is easy
        // we compute (t * I + (1 - t) * J) ∩ R[X]
        // 1 - t
        // elimination
        // <- restore order!
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        public string ToString()
        {
            return ToString(IStringifier.Dummy());
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        // intersection of principal ideals is easy
        // we compute (t * I + (1 - t) * J) ∩ R[X]
        // 1 - t
        // elimination
        // <- restore order!
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        public static Ideal<Term, Poly> Create<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators)
        {
            return Create(generators, GREVLEX);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        // intersection of principal ideals is easy
        // we compute (t * I + (1 - t) * J) ∩ R[X]
        // 1 - t
        // elimination
        // <- restore order!
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        public static Ideal<Term, Poly> Create<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(params Poly[] generators)
        {
            return Create(Arrays.AsList(generators));
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        // intersection of principal ideals is easy
        // we compute (t * I + (1 - t) * J) ∩ R[X]
        // 1 - t
        // elimination
        // <- restore order!
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to specified {@code monomialOrder} will
        /// be used.
        /// </summary>
        /// <param name="monomialOrder">monomial order for unique Groebner basis of the ideal</param>
        public static Ideal<Term, Poly> Create<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, Comparator<DegreeVector> monomialOrder)
        {
            return new Ideal(generators, GroebnerBasis(generators, monomialOrder));
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        // intersection of principal ideals is easy
        // we compute (t * I + (1 - t) * J) ∩ R[X]
        // 1 - t
        // elimination
        // <- restore order!
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to specified {@code monomialOrder} will
        /// be used.
        /// </summary>
        /// <param name="monomialOrder">monomial order for unique Groebner basis of the ideal</param>
        /// <summary>
        /// Creates trivial ideal (ideal = ring)
        /// </summary>
        public static Ideal<Term, Poly> Trivial<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(Poly factory)
        {
            return Trivial(factory, GREVLEX);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        // intersection of principal ideals is easy
        // we compute (t * I + (1 - t) * J) ∩ R[X]
        // 1 - t
        // elimination
        // <- restore order!
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to specified {@code monomialOrder} will
        /// be used.
        /// </summary>
        /// <param name="monomialOrder">monomial order for unique Groebner basis of the ideal</param>
        /// <summary>
        /// Creates trivial ideal (ideal = ring)
        /// </summary>
        /// <summary>
        /// Creates trivial ideal (ideal = ring)
        /// </summary>
        public static Ideal<Term, Poly> Trivial<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(Poly factory, Comparator<DegreeVector> monomialOrder)
        {
            return new Ideal(Collections.SingletonList(factory.CreateOne().SetOrdering(monomialOrder)));
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        // intersection of principal ideals is easy
        // we compute (t * I + (1 - t) * J) ∩ R[X]
        // 1 - t
        // elimination
        // <- restore order!
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to specified {@code monomialOrder} will
        /// be used.
        /// </summary>
        /// <param name="monomialOrder">monomial order for unique Groebner basis of the ideal</param>
        /// <summary>
        /// Creates trivial ideal (ideal = ring)
        /// </summary>
        /// <summary>
        /// Creates trivial ideal (ideal = ring)
        /// </summary>
        /// <summary>
        /// Creates empty ideal
        /// </summary>
        public static Ideal<Term, Poly> Empty<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(Poly factory)
        {
            return Empty(factory, GREVLEX);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        // intersection of principal ideals is easy
        // we compute (t * I + (1 - t) * J) ∩ R[X]
        // 1 - t
        // elimination
        // <- restore order!
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to specified {@code monomialOrder} will
        /// be used.
        /// </summary>
        /// <param name="monomialOrder">monomial order for unique Groebner basis of the ideal</param>
        /// <summary>
        /// Creates trivial ideal (ideal = ring)
        /// </summary>
        /// <summary>
        /// Creates trivial ideal (ideal = ring)
        /// </summary>
        /// <summary>
        /// Creates empty ideal
        /// </summary>
        /// <summary>
        /// Creates empty ideal
        /// </summary>
        public static Ideal<Term, Poly> Empty<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(Poly factory, Comparator<DegreeVector> monomialOrder)
        {
            return new Ideal(Collections.SingletonList(factory.CreateZero().SetOrdering(monomialOrder)));
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        // intersection of principal ideals is easy
        // we compute (t * I + (1 - t) * J) ∩ R[X]
        // 1 - t
        // elimination
        // <- restore order!
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to specified {@code monomialOrder} will
        /// be used.
        /// </summary>
        /// <param name="monomialOrder">monomial order for unique Groebner basis of the ideal</param>
        /// <summary>
        /// Creates trivial ideal (ideal = ring)
        /// </summary>
        /// <summary>
        /// Creates trivial ideal (ideal = ring)
        /// </summary>
        /// <summary>
        /// Creates empty ideal
        /// </summary>
        /// <summary>
        /// Creates empty ideal
        /// </summary>
        /// <summary>
        /// Shortcut for parse
        /// </summary>
        public static Ideal<Monomial<E>, MultivariatePolynomial<E>> Parse<E>(string[] generators, Ring<E> field, string[] variables)
        {
            return Parse(generators, field, GREVLEX, variables);
        }

        /// <summary>
        /// list of original generators
        /// </summary>
        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        // use original generators to construct basis when current ordering is "hard"
        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        // adjoin new variable to all generators (convert to F[X][y])
        // add 1 - y*poly
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns squared ideal
        /// </summary>
        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        // intersection of principal ideals is easy
        // we compute (t * I + (1 - t) * J) ∩ R[X]
        // 1 - t
        // elimination
        // <- restore order!
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to specified {@code monomialOrder} will
        /// be used.
        /// </summary>
        /// <param name="monomialOrder">monomial order for unique Groebner basis of the ideal</param>
        /// <summary>
        /// Creates trivial ideal (ideal = ring)
        /// </summary>
        /// <summary>
        /// Creates trivial ideal (ideal = ring)
        /// </summary>
        /// <summary>
        /// Creates empty ideal
        /// </summary>
        /// <summary>
        /// Creates empty ideal
        /// </summary>
        /// <summary>
        /// Shortcut for parse
        /// </summary>
        /// <summary>
        /// Shortcut for parse
        /// </summary>
        public static Ideal<Monomial<E>, MultivariatePolynomial<E>> Parse<E>(string[] generators, Ring<E> field, Comparator<DegreeVector> monomialOrder, string[] variables)
        {
            return Create(Arrays.Stream(generators).Map((p) => MultivariatePolynomial.Parse(p, field, monomialOrder, variables)).Collect(Collectors.ToList()), monomialOrder);
        }
    }
}