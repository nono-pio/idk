using Polynomials.Poly.Univar;
using Polynomials.Utils;
using UnivariatePolynomialZp64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;

namespace Polynomials.Linear;

public static class LinearSolver
{
    public static void TransposeSquare<T>(T[,] matrix)
    {
        for (var i = 0; i < matrix.Length; ++i)
        {
            for (var j = 0; j < i; ++j)
            {
                (matrix[i, j], matrix[j, i]) = (matrix[j, i], matrix[i, j]);
            }
        }
    }


    public static int RowEchelonForm<E>(Ring<E> ring, E[,] matrix)
    {
        return RowEchelonForm(ring, matrix, null, false, false);
    }


    public static int RowEchelonForm<E>(Ring<E> ring, E[,] matrix, bool reduce)
    {
        return RowEchelonForm(ring, matrix, null, reduce, false);
    }


    public static int RowEchelonForm<E>(Ring<E> ring, E[,] lhs, E[] rhs)
    {
        return RowEchelonForm(ring, lhs, rhs, false, false);
    }


    public static int RowEchelonForm<E>(Ring<E> ring, E[,] lhs, E[]? rhs, bool reduce, bool breakOnUnderDetermined)
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
            if (ring.IsZero(lhs[row, iColumn]))
            {
                for (var iRow = row + 1; iRow < nRows; ++iRow)
                    if (!ring.IsZero(lhs[iRow, iColumn]))
                    {
                        max = iRow;
                        break;
                    }

                Utils.Utils.Swap(lhs, row, max);
                if (rhs != null)
                    Utils.Utils.Swap(rhs, row, max);
            }


            // singular
            if (ring.IsZero(lhs[row, iColumn]))
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
                var alpha = ring.DivideExact(lhs[iRow, iColumn], lhs[row, iColumn]);
                if (rhs != null)
                    rhs[iRow] = ring.Subtract(rhs[iRow], ring.Multiply(alpha, rhs[row]));
                if (!ring.IsZero(alpha))
                    for (var iCol = iColumn; iCol < nColumns; ++iCol)
                        lhs[iRow, iCol] = ring.Subtract(lhs[iRow, iCol], ring.Multiply(alpha, lhs[row, iCol]));
            }
        }

        if (reduce)
            ReducedRowEchelonForm(ring, lhs, rhs);
        return nZeroColumns;
    }


    public static void ReducedRowEchelonForm<E>(Ring<E> ring, E[,] lhs, E[]? rhs)
    {
        var nRows = lhs.GetLength(0);
        var nColumns = lhs.GetLength(1);

        //number of zero columns
        var nZeroColumns = 0;
        for (int iColumn = 0, to = Math.Min(nRows, nColumns); iColumn < to; ++iColumn)
        {
            // find pivot row and swap
            var iRow = iColumn - nZeroColumns;
            if (ring.IsZero(lhs[iRow, iColumn]))
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


    public static E[] Solve<E>(Ring<E> ring, E[,] lhs, E[] rhs)
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


    public enum SystemInfo
    {
        UnderDetermined,


        Inconsistent,


        Consistent
    }


    public static SystemInfo Solve<E>(Ring<E> ring, E[,] lhs, E[] rhs, E[] result)
    {
        return Solve(ring, lhs, rhs, result, false);
    }


    public static SystemInfo Solve<E>(Ring<E> ring, E[,] lhs, E[] rhs, E[] result, bool solveIfUnderDetermined)
    {
        if (lhs.GetLength(0) != rhs.Length)
            throw new ArgumentException("lhs.length != rhs.length");
        if (rhs.Length == 0)
            return SystemInfo.Consistent;
        if (rhs.Length == 1)
        {
            if (lhs.GetLength(1) == 1)
            {
                result[0] = ring.DivideExact(rhs[0], lhs[0, 0]);
                return SystemInfo.Consistent;
            }

            if (solveIfUnderDetermined)
            {
                ring.FillZeros(result);
                if (ring.IsZero(rhs[0]))
                    return SystemInfo.Consistent;
                for (var i = 0; i < result.Length; ++i)
                    if (!ring.IsZero(lhs[0, i]))
                    {
                        result[i] = ring.DivideExact(rhs[0], lhs[0, i]);
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
                    sum = ring.Add(sum, ring.Multiply(lhs[i, j], result[j]));
                result[i] = ring.DivideExact(ring.Subtract(rhs[i], sum), lhs[i, i]);
            }

            return SystemInfo.Consistent;
        }


        // back substitution in case of SystemInfo.UnderDetermined system
        List<int> nzColumns = new List<int>(), nzRows = new List<int>();

        //number of zero columns
        var nZeroColumns = 0;
        var iRow = 0;
        for (int iColumn = 0, to = Math.Min(nRows, nColumns); iColumn < to; ++iColumn)
        {
            // find pivot row and swap
            iRow = iColumn - nZeroColumns;
            if (ring.IsZero(lhs[iRow, iColumn]))
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

            if (!ring.IsZero(rhs[iRow]) && ring.IsZero(lhs[iRow, iColumn]))
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


    public static SystemInfo Solve<E>(Ring<E> ring, List<E[]> lhs, List<E> rhs, E[] result)
    {
        return Solve(ring, lhs.ToArray().AsArray2D(), rhs.ToArray(), result);
    }


    public static E[] SolveVandermonde<E>(Ring<E> ring, E[] row, E[] rhs)
    {
        var result = new E[rhs.Length];
        var info = SolveVandermonde(ring, row, rhs, result);
        if (info != SystemInfo.Consistent)
            throw new ArithmeticException("singular or under-determined matrix");
        return result;
    }


    public static E[] SolveVandermondeT<E>(Ring<E> ring, E[] row, E[] rhs)
    {
        var result = new E[rhs.Length];
        var info = SolveVandermondeT(ring, row, rhs, result);
        if (info != SystemInfo.Consistent)
            throw new ArithmeticException("singular or under-determined matrix");
        return result;
    }


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
            var quot = UnivariateDivision.DivideAndRemainder(master, lins[i], true)[0];
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
            var quot = UnivariateDivision.DivideAndRemainder(master, lins[i], true)[0];
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


    public static int RowEchelonForm(IntegersZp64 ring, long[,] matrix)
    {
        return RowEchelonForm(ring, matrix, false);
    }


    public static int RowEchelonForm(IntegersZp64 ring, long[,] matrix, bool reduce)
    {
        return RowEchelonForm(ring, matrix, null, reduce, false);
    }


    public static int RowEchelonForm(IntegersZp64 ring, long[,] lhs, long[] rhs)
    {
        return RowEchelonForm(ring, lhs, rhs, false, false);
    }


    public static int RowEchelonForm(IntegersZp64 ring, long[,] lhs, long[]? rhs, bool reduce,
        bool breakOnUnderDetermined)
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
            if (lhs[row, iColumn] == 0)
            {
                for (var iRow = row + 1; iRow < nRows; ++iRow)
                    if (lhs[iRow, iColumn] != 0)
                    {
                        nonZero = iRow;
                        break;
                    }

                Utils.Utils.Swap(lhs, row, nonZero);
                if (rhs != null)
                    Utils.Utils.Swap(rhs, row, nonZero);
            }


            // singular
            if (lhs[row, iColumn] == 0)
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
                var alpha = ring.Divide(lhs[iRow, iColumn], lhs[row, iColumn]);
                if (rhs != null)
                    rhs[iRow] = ring.Subtract(rhs[iRow], ring.Multiply(alpha, rhs[row]));
                if (alpha != 0)
                    for (var iCol = iColumn; iCol < nColumns; ++iCol)
                        lhs[iRow, iCol] = ring.Subtract(lhs[iRow, iCol], ring.Multiply(alpha, lhs[row, iCol]));
            }
        }

        if (reduce)
            ReducedRowEchelonForm(ring, lhs, rhs);
        return nZeroColumns;
    }


    public static void ReducedRowEchelonForm(IntegersZp64 ring, long[,] lhs, long[]? rhs)
    {
        var nRows = lhs.GetLength(0);
        var nColumns = lhs.GetLength(1);

        //number of zero columns
        var nZeroColumns = 0;
        for (int iColumn = 0, to = Math.Min(nRows, nColumns); iColumn < to; ++iColumn)
        {
            // find pivot row and swap
            var iRow = iColumn - nZeroColumns;
            if (lhs[iRow, iColumn] == 0)
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


    public static long[] Solve(IntegersZp64 ring, long[,] lhs, long[] rhs)
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


    public static SystemInfo Solve(IntegersZp64 ring, long[,] lhs, long[] rhs, long[] result)
    {
        return Solve(ring, lhs, rhs, result, false);
    }


    public static SystemInfo Solve(IntegersZp64 ring, long[,] lhs, long[] rhs, long[] result,
        bool solveIfUnderDetermined)
    {
        if (lhs.GetLength(0) != rhs.Length)
            throw new ArgumentException("lhs.length != rhs.length");
        if (rhs.Length == 0)
            return SystemInfo.Consistent;
        if (rhs.Length == 1)
        {
            if (lhs.GetLength(1) == 1)
            {
                result[0] = ring.Divide(rhs[0], lhs[0, 0]);
                return SystemInfo.Consistent;
            }

            if (solveIfUnderDetermined)
            {
                if (rhs[0] == 0)
                    return SystemInfo.Consistent;
                for (var i = 0; i < result.Length; ++i)
                    if (lhs[0, i] != 0)
                    {
                        result[i] = ring.Divide(rhs[0], lhs[0, i]);
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
                    sum = ring.Add(sum, ring.Multiply(lhs[i, j], result[j]));
                result[i] = ring.Divide(ring.Subtract(rhs[i], sum), lhs[i, i]);
            }

            return SystemInfo.Consistent;
        }


        // back substitution in case of SystemInfo.UnderDetermined system
        List<int> nzColumns = new List<int>(), nzRows = new List<int>();

        //number of zero columns
        var nZeroColumns = 0;
        var iRow = 0;
        for (int iColumn = 0, to = Math.Min(nRows, nColumns); iColumn < to; ++iColumn)
        {
            // find pivot row and swap
            iRow = iColumn - nZeroColumns;
            if (lhs[iRow, iColumn] == 0)
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

            if (rhs[iRow] != 0 && lhs[iRow, iColumn] == 0)
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


    public static SystemInfo Solve(IntegersZp64 ring, List<long[]> lhs, List<long> rhs, long[] result)
    {
        return Solve(ring, lhs.ToArray().AsArray2D(), rhs.ToArray(), result);
    }


    public static long[] SolveVandermonde(IntegersZp64 ring, long[] row, long[] rhs)
    {
        var result = new long[rhs.Length];
        var info = SolveVandermonde(ring, row, rhs, result);
        if (info != SystemInfo.Consistent)
            throw new ArithmeticException("singular or under-determined matrix");
        return result;
    }


    public static long[] SolveVandermondeT(IntegersZp64 ring, long[] row, long[] rhs)
    {
        var result = new long[rhs.Length];
        var info = SolveVandermondeT(ring, row, rhs, result);
        if (info != SystemInfo.Consistent)
            throw new ArithmeticException("singular or under-determined matrix");
        return result;
    }


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
            quot = quot.DivideExact(cf);
            for (var j = 0; j < row.Length; ++j)
                result[j] = ring.Add(result[j], ring.Multiply(rhs[i], quot[j]));
        }

        return SystemInfo.Consistent;
    }


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
            quot = quot.DivideExact(cf);
            if (quot == null)
                throw new ArgumentException();
            result[i] = 0;
            for (var j = 0; j < row.Length; ++j)
                result[i] = ring.Add(result[i], ring.Multiply(rhs[j], quot[j]));
        }

        return SystemInfo.Consistent;
    }
}
