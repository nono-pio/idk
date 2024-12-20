
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Poly.Univar;
using Cc.Redberry.Rings.Util;

namespace Cc.Redberry.Rings.Linear
{
    /// <summary>
    /// Solver for quadratic linear system
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class LinearSolver
    {

        /// <summary>
        /// Transpose square matrix
        /// </summary>
        public static void TransposeSquare<T>(T[,] matrix)
        {
            for (var i = 0; i < matrix.Length; ++i)
            {
                for (var j = 0; j < i; ++j)
                {
                    (matrix[i,j], matrix[j,i]) = (matrix[j,i], matrix[i,j]);
                }
            }
        }

        /// <summary>
        /// Gives the row echelon form of the matrix
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="matrix">the matrix</param>
        /// <returns>the number of free variables</returns>
        public static int RowEchelonForm<E>(Ring<E> ring, E[, ] matrix)
        {
            return RowEchelonForm(ring, matrix, null, false, false);
        }

        /// <summary>
        /// Gives the row echelon form of the matrix
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="matrix">the matrix</param>
        /// <param name="reduce">whether to calculate reduced row echelon form</param>
        /// <returns>the number of free variables</returns>
        public static int RowEchelonForm<E>(Ring<E> ring, E[, ] matrix, bool reduce)
        {
            return RowEchelonForm(ring, matrix, null, reduce, false);
        }

        /// <summary>
        /// Gives the row echelon form of the linear system {@code lhs.x = rhs}.
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="lhs">the lhs of the system</param>
        /// <param name="rhs">the rhs of the system</param>
        /// <returns>the number of free variables</returns>
        public static int RowEchelonForm<E>(Ring<E> ring, E[, ] lhs, E[] rhs)
        {
            return RowEchelonForm(ring, lhs, rhs, false, false);
        }

        /// <summary>
        /// Gives the row echelon form of the linear system {@code lhs.x = rhs}.
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="lhs">the lhs of the system</param>
        /// <param name="rhs">the rhs of the system</param>
        /// <param name="reduce">whether to calculate reduced row echelon form</param>
        /// <param name="breakOnUnderDetermined">whether to return immediately if it was detected that system is under determined</param>
        /// <returns>the number of free variables</returns>
        public static int RowEchelonForm<E>(Ring<E> ring, E[, ] lhs, E[]? rhs, bool reduce, bool breakOnUnderDetermined)
        {
            if (rhs != null && lhs.GetLength(0) != rhs.Length)
                throw new ArgumentException("lhs.length != rhs.length");
            if (lhs.Length == 0)
                return 0;
            var nRows = lhs.GetLength(0);
            var nColumns = lhs.GetLength(1);

            //number of zero columns
            var nZeroColumns = 0;
            for (int iColumn = 0, to = Math.Min(nRows, nColumns); iColumn < to; ++iColumn)
            {

                // find pivot row and swap
                var row = iColumn - nZeroColumns;
                var max = row;
                if (ring.IsZero(lhs[row,iColumn]))
                {
                    for (var iRow = row + 1; iRow < nRows; ++iRow)
                        if (!ring.IsZero(lhs[iRow,iColumn]))
                        {
                            max = iRow;
                            break;
                        }

                    ArraysUtil.Swap(lhs, row, max);
                    if (rhs != null)
                        ArraysUtil.Swap(rhs, row, max);
                }


                // singular
                if (ring.IsZero(lhs[row,iColumn]))
                {
                    if (breakOnUnderDetermined)
                        return 1;

                    //nothing to do on this column
                    ++nZeroColumns;
                    to = Math.Min(nRows + nZeroColumns, nColumns);
                    continue;
                }


                // pivot within A and b
                for (var iRow = row + 1; iRow < nRows; ++iRow)
                {
                    var alpha = ring.DivideExact(lhs[iRow,iColumn], lhs[row,iColumn]);
                    if (rhs != null)
                        rhs[iRow] = ring.Subtract(rhs[iRow], ring.Multiply(alpha, rhs[row]));
                    if (!ring.IsZero(alpha))
                        for (var iCol = iColumn; iCol < nColumns; ++iCol)
                            lhs[iRow,iCol] = ring.Subtract(lhs[iRow,iCol], ring.Multiply(alpha, lhs[row,iCol]));
                }
            }

            if (reduce)
                ReducedRowEchelonForm(ring, lhs, rhs);
            return nZeroColumns;
        }

        /// <summary>
        /// Gives the reduced row echelon form of the linear system {@code lhs.x = rhs} from a given row echelon form.
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="lhs">the lhs of the system in the row echelon form</param>
        /// <param name="rhs">the rhs of the system</param>
        public static void ReducedRowEchelonForm<E>(Ring<E> ring, E[, ] lhs, E[]? rhs)
        {
            var nRows = lhs.GetLength(0);
            var nColumns = lhs.GetLength(1);

            //number of zero columns
            var nZeroColumns = 0;
            for (int iColumn = 0, to = Math.Min(nRows, nColumns); iColumn < to; ++iColumn)
            {

                // find pivot row and swap
                var iRow = iColumn - nZeroColumns;
                if (ring.IsZero(lhs[iRow,iColumn]))
                {
                    ++nZeroColumns;
                    to = Math.Min(nRows + nZeroColumns, nColumns);
                    continue;
                }


                // scale current row
                E[] row = lhs.GetRow(iRow);
                var val = row[iColumn];
                var valInv = ring.Reciprocal(val);
                for (var i = iColumn; i < nColumns; i++)
                    row[i] = ring.Multiply(valInv, row[i]);
                if (rhs != null)
                    rhs[iRow] = ring.Multiply(valInv, rhs[iRow]);

                // scale all rows before
                for (var i = 0; i < iRow; i++)
                {
                    E[] pRow = lhs.GetRow(i);
                    var v = pRow[iColumn];
                    if (ring.IsZero(v))
                        continue;
                    for (var j = iColumn; j < nColumns; ++j)
                        pRow[j] = ring.Subtract(pRow[j], ring.Multiply(v, row[j]));
                    if (rhs != null)
                        rhs[i] = ring.Subtract(rhs[i], ring.Multiply(v, rhs[iColumn]));
                }
            }
        }

        /// <summary>
        /// Solves linear system {@code lhs.x = rhs} and reduces lhs to row echelon form.
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="lhs">the lhs of the system (will be reduced to row echelon form)</param>
        /// <param name="rhs">the rhs of the system</param>
        /// <returns>the solution</returns>
        /// <exception cref="ArithmeticException">if the system is SystemInfo.Inconsistent or under-determined</exception>
        public static E[] Solve<E>(Ring<E> ring, E[, ] lhs, E[] rhs)
        {
            var nUnknowns = lhs.GetLength(1);
            if (nUnknowns == 0)
                return [];
            E[] result = new E[nUnknowns];
            var info = Solve(ring, lhs, rhs, result);
            if (info != SystemInfo.Consistent)
                throw new ArithmeticException("singular or under-determined matrix");
            return result;
        }

        /// <summary>
        /// Info about linear system
        /// </summary>
        public enum SystemInfo
        {
            /// <summary>
            /// Under-determined system
            /// </summary>
            UnderDetermined,
            /// <summary>
            /// SystemInfo.Inconsistent system
            /// </summary>
            Inconsistent,
            /// <summary>
            /// Consistent system
            /// </summary>
            Consistent
        }

        /// <summary>
        /// Solves linear system {@code lhs.x = rhs} and reduces the lhs to row echelon form. The result is stored in {@code
        /// result} (which should be of the enough length).
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="lhs">the lhs of the system (will be reduced to row echelon form)</param>
        /// <param name="rhs">the rhs of the system</param>
        /// <param name="result">where to place the result</param>
        /// <returns>system information (SystemInfo.Inconsistent, under-determined or consistent)</returns>
        public static SystemInfo Solve<E>(Ring<E> ring, E[, ] lhs, E[] rhs, E[] result)
        {
            return Solve(ring, lhs, rhs, result, false);
        }

        /// <summary>
        /// Solves linear system {@code lhs.x = rhs} and reduces the lhs to row echelon form. The result is stored in {@code
        /// result} (which should be of the enough length).
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="lhs">the lhs of the system (will be reduced to row echelon form)</param>
        /// <param name="rhs">the rhs of the system</param>
        /// <param name="result">where to place the result</param>
        /// <param name="solveIfUnderDetermined">give some solution even if the system is under determined</param>
        /// <returns>system information (SystemInfo.Inconsistent, under-determined or consistent)</returns>
        public static SystemInfo Solve<E>(Ring<E> ring, E[, ] lhs, E[] rhs, E[] result, bool solveIfUnderDetermined)
        {
            if (lhs.GetLength(0) != rhs.Length)
                throw new ArgumentException("lhs.length != rhs.length");
            if (rhs.Length == 0)
                return SystemInfo.Consistent;
            if (rhs.Length == 1)
            {
                if (lhs.GetLength(1) == 1)
                {
                    result[0] = ring.DivideExact(rhs[0], lhs[0,0]);
                    return SystemInfo.Consistent;
                }

                if (solveIfUnderDetermined)
                {
                    ring.FillZeros(result);
                    if (ring.IsZero(rhs[0]))
                        return SystemInfo.Consistent;
                    for (var i = 0; i < result.Length; ++i)
                        if (!ring.IsZero(lhs[0,i]))
                        {
                            result[i] = ring.DivideExact(rhs[0], lhs[0,i]);
                            return SystemInfo.Consistent;
                        }

                    return SystemInfo.Inconsistent;
                }

                if (lhs.GetLength(1) > 1)
                    return SystemInfo.UnderDetermined;
                return SystemInfo.Inconsistent;
            }

            var nUnderDetermined = RowEchelonForm(ring, lhs, rhs, false, !solveIfUnderDetermined);
            if (!solveIfUnderDetermined && nUnderDetermined > 0)

                // under-determined system
                return SystemInfo.UnderDetermined;
            var nRows = rhs.Length;
            var nColumns = lhs.GetLength(1);
            if (!solveIfUnderDetermined && nColumns > nRows)

                // under-determined system
                return SystemInfo.UnderDetermined;
            if (nRows > nColumns)

                // over-determined system
                // check that all rhs are zero
                for (var i = nColumns; i < nRows; ++i)
                    if (!ring.IsZero(rhs[i]))

                        // SystemInfo.Inconsistent system
                        return SystemInfo.Inconsistent;
            if (nRows > nColumns)
                for (var i = nColumns + 1; i < nRows; ++i)
                    if (!ring.IsZero(rhs[i]))
                        return SystemInfo.Inconsistent;
            ring.FillZeros(result);

            // back substitution in case of determined system
            if (nUnderDetermined == 0 && nColumns <= nRows)
            {
                for (var i = nColumns - 1; i >= 0; i--)
                {
                    var sum = ring.GetZero();
                    for (var j = i + 1; j < nColumns; j++)
                        sum = ring.Add(sum, ring.Multiply(lhs[i,j], result[j]));
                    result[i] = ring.DivideExact(ring.Subtract(rhs[i], sum), lhs[i,i]);
                }

                return SystemInfo.Consistent;
            }


            // back substitution in case of SystemInfo.UnderDetermined system
            TIntList nzColumns = new TIntList(), nzRows = new TIntList();

            //number of zero columns
            var nZeroColumns = 0;
            var iRow = 0;
            for (int iColumn = 0, to = Math.Min(nRows, nColumns); iColumn < to; ++iColumn)
            {

                // find pivot row and swap
                iRow = iColumn - nZeroColumns;
                if (ring.IsZero(lhs[iRow,iColumn]))
                {
                    if (iColumn == (nColumns - 1) && !ring.IsZero(rhs[iRow]))
                        return SystemInfo.Inconsistent;
                    ++nZeroColumns;
                    to = Math.Min(nRows + nZeroColumns, nColumns);
                    continue;
                }


                // scale current row
                E[] row = lhs.GetRow(iRow);
                var val = row[iColumn];
                var valInv = ring.Reciprocal(val);
                for (var i = iColumn; i < nColumns; i++)
                    row[i] = ring.Multiply(valInv, row[i]);
                rhs[iRow] = ring.Multiply(valInv, rhs[iRow]);

                // scale all rows before
                for (var i = 0; i < iRow; i++)
                {
                    E[] pRow = lhs.GetRow(i);
                    var v = pRow[iColumn];
                    if (ring.IsZero(v))
                        continue;
                    for (var j = iColumn; j < nColumns; ++j)
                        pRow[j] = ring.Subtract(pRow[j], ring.Multiply(v, row[j]));
                    rhs[i] = ring.Subtract(rhs[i], ring.Multiply(v, rhs[iRow]));
                }

                if (!ring.IsZero(rhs[iRow]) && ring.IsZero(lhs[iRow,iColumn]))
                    return SystemInfo.Inconsistent;
                nzColumns.Add(iColumn);
                nzRows.Add(iRow);
            }

            ++iRow;
            if (iRow < nRows)
                for (; iRow < nRows; ++iRow)
                    if (!ring.IsZero(rhs[iRow]))
                        return SystemInfo.Inconsistent;
            for (var i = 0; i < nzColumns.Count; ++i)
                result[nzColumns[i]] = rhs[nzRows[i]];
            return SystemInfo.Consistent;
        }

        /// <summary>
        /// Solves linear system {@code lhs.x = rhs} and stores the result in {@code result} (which should be of the enough
        /// length).
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="lhs">the lhs of the system</param>
        /// <param name="rhs">the rhs of the system</param>
        /// <param name="result">where to place the result</param>
        /// <returns>system information (SystemInfo.Inconsistent, under-determined or consistent)</returns>
        public static SystemInfo Solve<E>(Ring<E> ring, List<E[]> lhs, List<E> rhs, E[] result)
        {
            return Solve(ring, lhs.ToArray().AsArray2D(), rhs.ToArray(), result);
        }

        /// <summary>
        /// Solves Vandermonde linear system (that is with i-th equation of the form {@code row[i]^0 * x0 +  row[i]^1 * x1 +
        /// ... row[i]^N * xN = rhs[i] }).
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="row">the Vandermonde coefficients</param>
        /// <param name="rhs">the rhs of the system</param>
        /// <returns>the solution</returns>
        /// <exception cref="ArithmeticException">if the system is SystemInfo.Inconsistent or under-determined</exception>
        public static E[] SolveVandermonde<E>(Ring<E> ring, E[] row, E[] rhs)
        {
            var result = new E[rhs.Length];
            var info = SolveVandermonde(ring, row, rhs, result);
            if (info != SystemInfo.Consistent)
                throw new ArithmeticException("singular or under-determined matrix");
            return result;
        }

        /// <summary>
        /// Solves transposed Vandermonde linear system (that is with i-th equation of the form {@code row[0]^i * x0 +
        /// row[1]^i * x1 + ... row[N]^i * xN = rhs[i] }).
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="row">the Vandermonde coefficients</param>
        /// <param name="rhs">the rhs of the system</param>
        /// <returns>the solution</returns>
        /// <exception cref="ArithmeticException">if the system is SystemInfo.Inconsistent or under-determined</exception>
        public static E[] SolveVandermondeT<E>(Ring<E> ring, E[] row, E[] rhs)
        {
            var result = new E[rhs.Length];
            var info = SolveVandermondeT(ring, row, rhs, result);
            if (info != SystemInfo.Consistent)
                throw new ArithmeticException("singular or under-determined matrix");
            return result;
        }

        /// <summary>
        /// Solves Vandermonde linear system (that is with i-th equation of the form {@code row[i]^0 * x0 +  row[i]^1 * x1 +
        /// ... row[i]^N * xN = rhs[i] }) and stores the result in {@code result} (which should be of the enough length).
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="row">the Vandermonde coefficients</param>
        /// <param name="rhs">the rhs of the system</param>
        /// <param name="result">where to place the result</param>
        /// <returns>system information (SystemInfo.Inconsistent, under-determined or consistent)</returns>
        public static SystemInfo SolveVandermonde<E>(Ring<E> ring, E[] row, E[] rhs, E[] result)
        {
            if (row.Length != rhs.Length)
                throw new ArgumentException("not a square Vandermonde matrix");
            if (rhs.Length == 0)
                return SystemInfo.Consistent;
            if (rhs.Length == 1)
            {
                result[0] = rhs[0];
                return SystemInfo.Consistent;
            }

            var lins = new UnivariatePolynomial<E>[row.Length];
            UnivariatePolynomial<E> master = UnivariatePolynomial<E>.One(ring);
            for (var i = 0; i < row.Length; ++i)
            {
                lins[i] = master.CreateLinear(ring.Negate(row[i]), ring.GetOne());
                master = master.Multiply(lins[i]);
            }

            for (var i = 0; i < result.Length; i++)
                result[i] = ring.GetZero();
            for (var i = 0; i < row.Length; i++)
            {
                UnivariatePolynomial<E> quot = UnivariateDivision.DivideAndRemainder(master, lins[i], true)[0];
                var cf = quot.Evaluate(row[i]);
                if (ring.IsZero(cf))
                    return SystemInfo.UnderDetermined;
                quot = quot.DivideOrNull(cf);
                if (quot == null)
                    throw new ArgumentException();
                for (var j = 0; j < row.Length; ++j)
                    result[j] = ring.Add(result[j], ring.Multiply(rhs[i], quot[j]));
            }

            return SystemInfo.Consistent;
        }

        /// <summary>
        /// Solves transposed Vandermonde linear system (that is with i-th equation of the form {@code row[0]^i * x0 +
        /// row[1]^i * x1 + ... row[N]^i * xN = rhs[i] }) and stores the result in {@code result} (which should be of the
        /// enough length).
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="row">the Vandermonde coefficients</param>
        /// <param name="rhs">the rhs of the system</param>
        /// <param name="result">where to place the result</param>
        /// <returns>system information (SystemInfo.Inconsistent, under-determined or consistent)</returns>
        public static SystemInfo SolveVandermondeT<E>(Ring<E> ring, E[] row, E[] rhs, E[] result)
        {
            if (row.Length != rhs.Length)
                throw new ArgumentException("not a square Vandermonde matrix");
            if (rhs.Length == 0)
                return SystemInfo.Consistent;
            if (rhs.Length == 1)
            {
                result[0] = rhs[0];
                return SystemInfo.Consistent;
            }

            var lins = new UnivariatePolynomial<E>[row.Length];
            UnivariatePolynomial<E> master = UnivariatePolynomial<E>.One(ring);
            for (var i = 0; i < row.Length; ++i)
            {
                lins[i] = master.CreateLinear(ring.Negate(row[i]), ring.GetOne());
                master = master.Multiply(lins[i]);
            }

            for (var i = 0; i < row.Length; i++)
            {
                UnivariatePolynomial<E> quot = UnivariateDivision.DivideAndRemainder(master, lins[i], true)[0];
                var cf = quot.Evaluate(row[i]);
                if (ring.IsZero(cf))
                    return SystemInfo.UnderDetermined;
                quot = quot.DivideOrNull(cf);
                if (quot == null)
                    throw new ArgumentException();
                result[i] = ring.GetZero();
                for (var j = 0; j < row.Length; ++j)
                    result[i] = ring.Add(result[i], ring.Multiply(rhs[j], quot[j]));
            }

            return SystemInfo.Consistent;
        }

        /* ========================================= Machine numbers ============================================ */
        /// <summary>
        /// Gives the row echelon form of the matrix
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="matrix">the matrix</param>
        /// <returns>the number of free variables</returns>
        public static int RowEchelonForm(IntegersZp64 ring, long[, ] matrix)
        {
            return RowEchelonForm(ring, matrix, false);
        }

        /// <summary>
        /// Gives the row echelon form of the matrix
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="matrix">the matrix</param>
        /// <param name="reduce">whether to calculate reduced row echelon form</param>
        /// <returns>the number of free variables</returns>
        public static int RowEchelonForm(IntegersZp64 ring, long[, ] matrix, bool reduce)
        {
            return RowEchelonForm(ring, matrix, null, reduce, false);
        }

        /// <summary>
        /// Gives the row echelon form of the linear system {@code lhs.x = rhs} (rhs may be null).
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="lhs">the lhs of the system</param>
        /// <param name="rhs">the rhs of the system (may be null)</param>
        /// <returns>the number of free variables</returns>
        public static int RowEchelonForm(IntegersZp64 ring, long[, ] lhs, long[] rhs)
        {
            return RowEchelonForm(ring, lhs, rhs, false, false);
        }

        /// <summary>
        /// Gives the row echelon form of the linear system {@code lhs.x = rhs} (rhs may be null).
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="lhs">the lhs of the system</param>
        /// <param name="rhs">the rhs of the system (may be null)</param>
        /// <param name="reduce">whether to calculate reduced row echelon form</param>
        /// <param name="breakOnUnderDetermined">whether to return immediately if it was detected that system is under determined</param>
        /// <returns>the number of free variables</returns>
        public static int RowEchelonForm(IntegersZp64 ring, long[, ] lhs, long[] rhs, bool reduce, bool breakOnUnderDetermined)
        {
            if (rhs != null && lhs.GetLength(0) != rhs.Length)
                throw new ArgumentException("lhs.length != rhs.length");
            if (lhs.GetLength(0) == 0)
                return 0;
            var nRows = lhs.GetLength(0);
            var nColumns = lhs.GetLength(1);

            //number of zero columns
            var nZeroColumns = 0;
            for (int iColumn = 0, to = Math.Min(nRows, nColumns); iColumn < to; ++iColumn)
            {

                // find pivot row and swap
                var row = iColumn - nZeroColumns;
                var nonZero = row;
                if (lhs[row,iColumn] == 0)
                {
                    for (var iRow = row + 1; iRow < nRows; ++iRow)
                        if (lhs[iRow,iColumn] != 0)
                        {
                            nonZero = iRow;
                            break;
                        }

                    ArraysUtil.Swap(lhs, row, nonZero);
                    if (rhs != null)
                        ArraysUtil.Swap(rhs, row, nonZero);
                }


                // singular
                if (lhs[row,iColumn] == 0)
                {
                    if (breakOnUnderDetermined)
                        return 1;

                    //nothing to do on this column
                    ++nZeroColumns;
                    to = Math.Min(nRows + nZeroColumns, nColumns);
                    continue;
                }


                // pivot within A and b
                for (var iRow = row + 1; iRow < nRows; ++iRow)
                {
                    var alpha = ring.Divide(lhs[iRow,iColumn], lhs[row,iColumn]);
                    if (rhs != null)
                        rhs[iRow] = ring.Subtract(rhs[iRow], ring.Multiply(alpha, rhs[row]));
                    if (alpha != 0)
                        for (var iCol = iColumn; iCol < nColumns; ++iCol)
                            lhs[iRow,iCol] = ring.Subtract(lhs[iRow,iCol], ring.Multiply(alpha, lhs[row,iCol]));
                }
            }

            if (reduce)
                ReducedRowEchelonForm(ring, lhs, rhs);
            return nZeroColumns;
        }

        /// <summary>
        /// Gives the reduced row echelon form of the linear system {@code lhs.x = rhs} from a given row echelon form.
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="lhs">the lhs of the system in the row echelon form</param>
        /// <param name="rhs">the rhs of the system</param>
        public static void ReducedRowEchelonForm(IntegersZp64 ring, long[, ] lhs, long[]? rhs)
        {
            var nRows = lhs.GetLength(0);
            var nColumns = lhs.GetLength(1);

            //number of zero columns
            var nZeroColumns = 0;
            for (int iColumn = 0, to = Math.Min(nRows, nColumns); iColumn < to; ++iColumn)
            {

                // find pivot row and swap
                var iRow = iColumn - nZeroColumns;
                if (lhs[iRow,iColumn] == 0)
                {
                    ++nZeroColumns;
                    to = Math.Min(nRows + nZeroColumns, nColumns);
                    continue;
                }


                // scale current row
                var row = lhs.GetRow(iRow);
                var val = row[iColumn];
                var valInv = ring.Reciprocal(val);
                for (var i = iColumn; i < nColumns; i++)
                    row[i] = ring.Multiply(valInv, row[i]);
                if (rhs != null)
                    rhs[iRow] = ring.Multiply(valInv, rhs[iRow]);

                // scale all rows before
                for (var i = 0; i < iRow; i++)
                {
                    var pRow = lhs.GetRow(i);
                    var v = pRow[iColumn];
                    if (v == 0)
                        continue;
                    for (var j = iColumn; j < nColumns; ++j)
                        pRow[j] = ring.Subtract(pRow[j], ring.Multiply(v, row[j]));
                    if (rhs != null)
                        rhs[i] = ring.Subtract(rhs[i], ring.Multiply(v, rhs[iRow]));
                }
            }
        }

        /// <summary>
        /// Solves linear system {@code lhs.x = rhs} and reduces the lhs to row echelon form.
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="lhs">the lhs of the system  (will be reduced to row echelon form)</param>
        /// <param name="rhs">the rhs of the system</param>
        /// <returns>the solution</returns>
        /// <exception cref="ArithmeticException">if the system is SystemInfo.Inconsistent or under-determined</exception>
        public static long[] Solve(IntegersZp64 ring, long[, ] lhs, long[] rhs)
        {
            var nUnknowns = lhs.GetLength(1);
            if (nUnknowns == 0)
                return new long[0];
            var result = new long[nUnknowns];
            var info = Solve(ring, lhs, rhs, result);
            if (info != SystemInfo.Consistent)
                throw new ArithmeticException("singular or under-determined matrix");
            return result;
        }

        /// <summary>
        /// Solves linear system {@code lhs.x = rhs} and reduces the lhs to row echelon form. The result is stored in {@code
        /// result} (which should be of the enough length).
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="lhs">the lhs of the system  (will be reduced to row echelon form)</param>
        /// <param name="rhs">the rhs of the system</param>
        /// <param name="result">where to place the result</param>
        /// <returns>system information (SystemInfo.Inconsistent, under-determined or consistent)</returns>
        public static SystemInfo Solve(IntegersZp64 ring, long[, ] lhs, long[] rhs, long[] result)
        {
            return Solve(ring, lhs, rhs, result, false);
        }

        /// <summary>
        /// Solves linear system {@code lhs.x = rhs} and reduces the lhs to row echelon form. The result is stored in {@code
        /// result} (which should be of the enough length and filled with zeros).
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="lhs">the lhs of the system  (will be reduced to row echelon form)</param>
        /// <param name="rhs">the rhs of the system</param>
        /// <param name="result">where to place the result</param>
        /// <param name="solveIfUnderDetermined">give some solution even if the system is under determined</param>
        /// <returns>system information (SystemInfo.Inconsistent, under-determined or consistent)</returns>
        public static SystemInfo Solve(IntegersZp64 ring, long[, ] lhs, long[] rhs, long[] result, bool solveIfUnderDetermined)
        {
            if (lhs.GetLength(0) != rhs.Length)
                throw new ArgumentException("lhs.length != rhs.length");
            if (rhs.Length == 0)
                return SystemInfo.Consistent;
            if (rhs.Length == 1)
            {
                if (lhs.GetLength(1) == 1)
                {
                    result[0] = ring.Divide(rhs[0], lhs[0,0]);
                    return SystemInfo.Consistent;
                }

                if (solveIfUnderDetermined)
                {
                    if (rhs[0] == 0)
                        return SystemInfo.Consistent;
                    for (var i = 0; i < result.Length; ++i)
                        if (lhs[0,i] != 0)
                        {
                            result[i] = ring.Divide(rhs[0], lhs[0,i]);
                            return SystemInfo.Consistent;
                        }

                    return SystemInfo.Inconsistent;
                }

                if (lhs.GetLength(1) > 1)
                    return SystemInfo.UnderDetermined;
                return SystemInfo.Inconsistent;
            }

            var nUnderDetermined = RowEchelonForm(ring, lhs, rhs, false, !solveIfUnderDetermined);
            if (!solveIfUnderDetermined && nUnderDetermined > 0)

                // under-determined system
                return SystemInfo.UnderDetermined;
            var nRows = rhs.Length;
            var nColumns = lhs.GetLength(1);
            if (!solveIfUnderDetermined && nColumns > nRows)

                // under-determined system
                return SystemInfo.UnderDetermined;
            if (nRows > nColumns)

                // over-determined system
                // check that all rhs are zero
                for (var i = nColumns; i < nRows; ++i)
                    if (rhs[i] != 0)

                        // SystemInfo.Inconsistent system
                        return SystemInfo.Inconsistent;
            if (nRows > nColumns)
                for (var i = nColumns + 1; i < nRows; ++i)
                    if (rhs[i] != 0)
                        return SystemInfo.Inconsistent;

            // back substitution in case of determined system
            if (nUnderDetermined == 0 && nColumns <= nRows)
            {
                for (var i = nColumns - 1; i >= 0; i--)
                {
                    long sum = 0;
                    for (var j = i + 1; j < nColumns; j++)
                        sum = ring.Add(sum, ring.Multiply(lhs[i,j], result[j]));
                    result[i] = ring.Divide(ring.Subtract(rhs[i], sum), lhs[i,i]);
                }

                return SystemInfo.Consistent;
            }


            // back substitution in case of SystemInfo.UnderDetermined system
            TIntList nzColumns = new TIntList(), nzRows = new TIntList();

            //number of zero columns
            var nZeroColumns = 0;
            var iRow = 0;
            for (int iColumn = 0, to = Math.Min(nRows, nColumns); iColumn < to; ++iColumn)
            {

                // find pivot row and swap
                iRow = iColumn - nZeroColumns;
                if (lhs[iRow,iColumn] == 0)
                {
                    if (iColumn == (nColumns - 1) && rhs[iRow] != 0)
                        return SystemInfo.Inconsistent;
                    ++nZeroColumns;
                    to = Math.Min(nRows + nZeroColumns, nColumns);
                    continue;
                }


                // scale current row
                long[] row = lhs.GetRow(iRow);
                var val = row[iColumn];
                var valInv = ring.Reciprocal(val);
                for (var i = iColumn; i < nColumns; i++)
                    row[i] = ring.Multiply(valInv, row[i]);
                rhs[iRow] = ring.Multiply(valInv, rhs[iRow]);

                // scale all rows before
                for (var i = 0; i < iRow; i++)
                {
                    long[] pRow = lhs.GetRow(i);
                    var v = pRow[iColumn];
                    if (v == 0)
                        continue;
                    for (var j = iColumn; j < nColumns; ++j)
                        pRow[j] = ring.Subtract(pRow[j], ring.Multiply(v, row[j]));
                    rhs[i] = ring.Subtract(rhs[i], ring.Multiply(v, rhs[iRow]));
                }

                if (rhs[iRow] != 0 && lhs[iRow,iColumn] == 0)
                    return SystemInfo.Inconsistent;
                nzColumns.Add(iColumn);
                nzRows.Add(iRow);
            }

            ++iRow;
            if (iRow < nRows)
                for (; iRow < nRows; ++iRow)
                    if (rhs[iRow] != 0)
                        return SystemInfo.Inconsistent;
            for (var i = 0; i < nzColumns.Count; ++i)
                result[nzColumns[i]] = rhs[nzRows[i]];
            return SystemInfo.Consistent;
        }

        /// <summary>
        /// Solves linear system {@code lhs.x = rhs} and stores the result in {@code result} (which should be of the enough
        /// length).
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="lhs">the lhs of the system</param>
        /// <param name="rhs">the rhs of the system</param>
        /// <param name="result">where to place the result</param>
        /// <returns>system information (SystemInfo.Inconsistent, under-determined or consistent)</returns>
        public static SystemInfo Solve(IntegersZp64 ring, List<long[]> lhs, TLongList rhs, long[] result)
        {
            return Solve(ring, lhs.ToArray().AsArray2D(), rhs.ToArray(), result);
        }

        /// <summary>
        /// Solves Vandermonde linear system (that is with i-th equation of the form {@code row[i]^0 * x0 +  row[i]^1 * x1 +
        /// ... row[i]^N * xN = rhs[i] }).
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="row">the Vandermonde coefficients</param>
        /// <param name="rhs">the rhs of the system</param>
        /// <returns>the solution</returns>
        /// <exception cref="ArithmeticException">if the system is SystemInfo.Inconsistent or under-determined</exception>
        public static long[] SolveVandermonde(IntegersZp64 ring, long[] row, long[] rhs)
        {
            var result = new long[rhs.Length];
            var info = SolveVandermonde(ring, row, rhs, result);
            if (info != SystemInfo.Consistent)
                throw new ArithmeticException("singular or under-determined matrix");
            return result;
        }

        /// <summary>
        /// Solves transposed Vandermonde linear system (that is with i-th equation of the form {@code row[0]^i * x0 +
        /// row[1]^i * x1 + ... row[N]^i * xN = rhs[i] }).
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="row">the Vandermonde coefficients</param>
        /// <param name="rhs">the rhs of the system</param>
        /// <returns>the solution</returns>
        /// <exception cref="ArithmeticException">if the system is SystemInfo.Inconsistent or under-determined</exception>
        public static long[] SolveVandermondeT(IntegersZp64 ring, long[] row, long[] rhs)
        {
            var result = new long[rhs.Length];
            var info = SolveVandermondeT(ring, row, rhs, result);
            if (info != SystemInfo.Consistent)
                throw new ArithmeticException("singular or under-determined matrix");
            return result;
        }

        /// <summary>
        /// Solves Vandermonde linear system (that is with i-th equation of the form {@code row[i]^0 * x0 +  row[i]^1 * x1 +
        /// ... row[i]^N * xN = rhs[i] }) and stores the result in {@code result} (which should be of the enough length).
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="row">the Vandermonde coefficients</param>
        /// <param name="rhs">the rhs of the system</param>
        /// <param name="result">where to place the result</param>
        /// <returns>system information (SystemInfo.Inconsistent, under-determined or consistent)</returns>
        public static SystemInfo SolveVandermonde(IntegersZp64 ring, long[] row, long[] rhs, long[] result)
        {
            if (row.Length != rhs.Length)
                throw new ArgumentException("not a square Vandermonde matrix");
            if (rhs.Length == 0)
                return SystemInfo.Consistent;
            if (rhs.Length == 1)
            {
                result[0] = rhs[0];
                return SystemInfo.Consistent;
            }

            var lins = new UnivariatePolynomialZp64[row.Length];
            var master = UnivariatePolynomialZp64.One(ring);
            for (var i = 0; i < row.Length; ++i)
            {
                lins[i] = master.CreateLinear(ring.Negate(row[i]), 1);
                master = master.Multiply(lins[i]);
            }

            for (var i = 0; i < result.Length; i++)
                result[i] = 0;
            for (var i = 0; i < row.Length; i++)
            {
                var quot = UnivariateDivision.DivideAndRemainder(master, lins[i], true)[0];
                var cf = quot.Evaluate(row[i]);
                if (cf == 0)
                    return SystemInfo.UnderDetermined;
                quot = quot.Divide(cf);
                for (var j = 0; j < row.Length; ++j)
                    result[j] = ring.Add(result[j], ring.Multiply(rhs[i], quot[j]));
            }

            return SystemInfo.Consistent;
        }

        /// <summary>
        /// Solves transposed Vandermonde linear system (that is with i-th equation of the form {@code row[0]^i * x0 +
        /// row[1]^i * x1 + ... row[N]^i * xN = rhs[i] }) and stores the result in {@code result} (which should be of the
        /// enough length).
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="row">the Vandermonde coefficients</param>
        /// <param name="rhs">the rhs of the system</param>
        /// <param name="result">where to place the result</param>
        /// <returns>system information (SystemInfo.Inconsistent, under-determined or consistent)</returns>
        public static SystemInfo SolveVandermondeT(IntegersZp64 ring, long[] row, long[] rhs, long[] result)
        {
            if (row.Length != rhs.Length)
                throw new ArgumentException("not a square Vandermonde matrix");
            if (rhs.Length == 0)
                return SystemInfo.Consistent;
            if (rhs.Length == 1)
            {
                result[0] = rhs[0];
                return SystemInfo.Consistent;
            }

            var lins = new UnivariatePolynomialZp64[row.Length];
            var master = UnivariatePolynomialZp64.One(ring);
            for (var i = 0; i < row.Length; ++i)
            {
                lins[i] = master.CreateLinear(ring.Negate(row[i]), 1);
                master = master.Multiply(lins[i]);
            }

            for (var i = 0; i < row.Length; i++)
            {
                var quot = UnivariateDivision.DivideAndRemainder(master, lins[i], true)[0];
                var cf = quot.Evaluate(row[i]);
                if (cf == 0)
                    return SystemInfo.UnderDetermined;
                quot = quot.Divide(cf);
                if (quot == null)
                    throw new ArgumentException();
                result[i] = 0;
                for (var j = 0; j < row.Length; ++j)
                    result[i] = ring.Add(result[i], ring.Multiply(rhs[j], quot[j]));
            }

            return SystemInfo.Consistent;
        }
    }
}