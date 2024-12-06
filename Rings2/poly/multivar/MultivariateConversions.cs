using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Poly;
using Cc.Redberry.Rings.Poly.Univar;
using Cc.Redberry.Rings.Util;
using Java.Util;
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
    /// </summary>
    /// <remarks>
    /// @authorStanislav Poslavsky
    /// @since2.2
    /// </remarks>
    public sealed class MultivariateConversions
    {
        private MultivariateConversions()
        {
        }

        /// <summary>
        /// Given poly in R[x1,x2,...,xN] converts to poly in R[variables][other_variables]
        /// </summary>
        public static MultivariatePolynomial<Poly> Split<Poly extends AMultivariatePolynomial<?, Poly>>(Poly poly, params int[] variables)
        {
            return poly.AsOverMultivariateEliminate(variables);
        }

        /// <summary>
        /// Given poly in R[variables][other_variables] converts it to poly in R[x1,x2,...,xN]
        /// </summary>
        public static Poly Merge<Poly extends AMultivariatePolynomial<?, Poly>>(MultivariatePolynomial<Poly> poly, params int[] variables)
        {
            variables = variables.Clone();
            Arrays.Sort(variables);
            int[] mainVariables = ArraysUtil.IntSetDifference(ArraysUtil.Sequence(0, poly.nVariables + variables.Length), variables);
            if (poly.Cc() is MultivariatePolynomial)
                return (Poly)MultivariatePolynomial.AsNormalMultivariate((MultivariatePolynomial)poly, variables, mainVariables);
            else
                return (Poly)MultivariatePolynomialZp64.AsNormalMultivariate((MultivariatePolynomial)poly, variables, mainVariables);
        }

        /// <summary>
        /// Given poly in R[x1,x2,...,xN] converts to poly in R[variables][other_variables]
        /// </summary>
        public static MultivariateRing<MultivariatePolynomial<Poly>> Split<Poly extends AMultivariatePolynomial<?, Poly>>(IPolynomialRing<Poly> ring, params int[] variables)
        {
            return Rings.MultivariateRing(Split(ring.Factory(), variables));
        }

        /// <summary>
        /// Given poly in R[x1,x2,...,xN] converts to poly in R[variables][other_variables]
        /// </summary>
        public static MultivariateRing<Poly> Merge<Poly extends AMultivariatePolynomial<?, Poly>>(IPolynomialRing<MultivariatePolynomial<Poly>> ring, params int[] variables)
        {
            return (MultivariateRing<Poly>)Rings.MultivariateRing((AMultivariatePolynomial)Merge(ring.Factory(), variables));
        }

        /// <summary>
        /// Given poly in R[x1,x2,...,xN] converts to poly in R[other_variables][variable]
        /// </summary>
        public static UnivariatePolynomial<Poly> AsUnivariate<Poly extends AMultivariatePolynomial<?, Poly>>(Poly poly, int variable)
        {
            return poly.AsUnivariateEliminate(variable);
        }

        /// <summary>
        /// Given poly in R[variables][other_variables] converts it to poly in R[x1,x2,...,xN]
        /// </summary>
        public static Poly FromUnivariate<Poly extends AMultivariatePolynomial<?, Poly>>(UnivariatePolynomial<Poly> poly, int variable)
        {
            if (poly.Cc() is MultivariatePolynomial)
                return (Poly)MultivariatePolynomial.AsMultivariate((UnivariatePolynomial)poly, variable, true);
            else
                return (Poly)MultivariatePolynomialZp64.AsMultivariate((UnivariatePolynomial)poly, variable, true);
        }

        /// <summary>
        /// Given poly in R[x1,x2,...,xN] converts to poly in R[other_variables][variable]
        /// </summary>
        public static UnivariateRing<UnivariatePolynomial<Poly>> AsUnivariate<Poly extends AMultivariatePolynomial<?, Poly>>(IPolynomialRing<Poly> ring, int variable)
        {
            return Rings.UnivariateRing(AsUnivariate(ring.Factory(), variable));
        }

        /// <summary>
        /// Given poly in R[variables][other_variables] converts it to poly in R[x1,x2,...,xN]
        /// </summary>
        public static IPolynomialRing<Poly> FromUnivariate<Poly extends AMultivariatePolynomial<?, Poly>>(IPolynomialRing<UnivariatePolynomial<Poly>> ring, int variable)
        {
            return (IPolynomialRing<Poly>)Rings.MultivariateRing((AMultivariatePolynomial)FromUnivariate(ring.Factory(), variable));
        }
    }
}