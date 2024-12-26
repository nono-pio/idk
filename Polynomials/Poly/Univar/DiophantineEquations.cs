namespace Polynomials.Poly.Univar;

public static class DiophantineEquations
{
    public static UnivariatePolynomial<E>[] MonicExtendedEuclid<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        var xgcd = UnivariateGCD.PolynomialExtendedGCD(a, b);
        if (xgcd[0].IsOne())
            return xgcd;

        //normalize: x * a + y * b = 1
        xgcd[2].DivideByLC(xgcd[0]);
        xgcd[1].DivideByLC(xgcd[0]);
        xgcd[0].Monic();
        return xgcd;
    }


    public sealed class DiophantineSolver<E>
    {
        readonly UnivariatePolynomial<E>[] factors;


        readonly UnivariatePolynomial<E>[] solution;


        readonly UnivariatePolynomial<E> gcd;


        public DiophantineSolver(UnivariatePolynomial<E>[] factors)
        {
            this.factors = factors;
            this.solution = new UnivariatePolynomial<E>[factors.Length];
            var prev = factors[0];
            solution[0] = factors[0].CreateOne();
            for (int i = 1; i < factors.Length; i++)
            {
                var xgcd = MonicExtendedEuclid(prev, factors[i]);
                for (int j = 0; j < i; j++)
                    solution[j].Multiply(xgcd[1]);
                solution[i] = xgcd[2];
                prev = xgcd[0];
            }

            gcd = prev;
        }


        public UnivariatePolynomial<E>[] Solve(UnivariatePolynomial<E> rhs)
        {
            rhs = UnivariateDivision.DivideOrNull(rhs, gcd, true);
            if (rhs == null)
                throw new ArgumentException("Not solvable.");
            UnivariatePolynomial<E>[] solution = new UnivariatePolynomial<E>[this.solution.Length];
            for (int i = 0; i < solution.Length; i++)
                solution[i] = this.solution[i].Clone().Multiply(rhs);
            return solution;
        }
    }
}