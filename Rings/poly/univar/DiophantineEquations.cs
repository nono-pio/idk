namespace Rings.poly.univar;


public static class DiophantineEquations {

    /** runs xgcd for coprime polynomials ensuring that gcd is 1 (not another constant) */
    public static 
    Poly[] monicExtendedEuclid<Poly>(Poly a, Poly b)  where Poly : IUnivariatePolynomial<Poly> {
        Poly[] xgcd = UnivariateGCD.PolynomialExtendedGCD(a, b);
        if (xgcd[0].isOne())
            return xgcd;

        //normalize: x * a + y * b = 1
        xgcd[2].divideByLC(xgcd[0]);
        xgcd[1].divideByLC(xgcd[0]);
        xgcd[0].monic();

        return xgcd;
    }

    /**
     * Solves a1 * x1 + a2 * x2 + ... = rhs for given univariate and rhs and unknown x_i
     */
    public sealed class DiophantineSolver<Poly> where Poly : IUnivariatePolynomial<Poly> {
        /** the given factors */
        readonly Poly[] factors;
        readonly Poly[] solution;
        readonly Poly gcd;

        public DiophantineSolver(Poly[] factors) {
            this.factors = factors;
            this.solution = new Poly[factors.Length];

            Poly prev = factors[0];
            solution[0] = factors[0].createOne();

            for (int i = 1; i < factors.Length; i++) {
                Poly[] xgcd = monicExtendedEuclid(prev, factors[i]);
                for (int j = 0; j < i; j++)
                    solution[j].multiply(xgcd[1]);
                solution[i] = xgcd[2];
                prev = xgcd[0];
            }
            gcd = prev;
        }

        public Poly[] solve(Poly rhs) {
            rhs = UnivariateDivision.divideOrNull(rhs, gcd, true);
            if (rhs == null)
                throw new ArgumentException("Not solvable.");
            Poly[] solution = new Poly[this.solution.Length];
            for (int i = 0; i < solution.Length; i++)
                solution[i] = this.solution[i].clone().multiply(rhs);
            return solution;
        }
    }
}
