namespace Rings.linear;


public static class LinearSolver {

    /**
     * Transpose square matrix
     */
    public static void transposeSquare<T>(T[,] matrix) {
        for (int i = 0; i < matrix.GetLength(0); ++i) {
            for (int j = 0; j < i; ++j) {
                (matrix[i, j], matrix[j, i]) = (matrix[j, i], matrix[i, j]);
            }
        }
    }


    public static int rowEchelonForm<E>(Ring<E> ring, E[][] matrix) {
        return rowEchelonForm(ring, matrix, null, false, false);
    }

    /**
     * Gives the row echelon form of the matrix
     *
     * @param ring   the ring
     * @param matrix the matrix
     * @param reduce whether to calculate reduced row echelon form
     * @return the number of free variables
     */
    public static int rowEchelonForm<E>(Ring<E> ring, E[][] matrix, bool reduce) {
        return rowEchelonForm(ring, matrix, null, reduce, false);
    }

    /**
     * Gives the row echelon form of the linear system {@code lhs.x = rhs}.
     *
     * @param ring the ring
     * @param lhs  the lhs of the system
     * @param rhs  the rhs of the system
     * @return the number of free variables
     */
    public static  int rowEchelonForm<E>(Ring<E> ring, E[][] lhs, E[] rhs) {
        return rowEchelonForm(ring, lhs, rhs, false, false);
    }

    /**
     * Gives the row echelon form of the linear system {@code lhs.x = rhs}.
     *
     * @param ring                   the ring
     * @param lhs                    the lhs of the system
     * @param rhs                    the rhs of the system
     * @param reduce                 whether to calculate reduced row echelon form
     * @param breakOnUnderDetermined whether to return immediately if it was detected that system is under determined
     * @return the number of free variables
     */
    public static  int rowEchelonForm<E>(Ring<E> ring, E[][] lhs, E[] rhs, bool reduce, bool breakOnUnderDetermined) {
        if (rhs != null && lhs.Length != rhs.Length)
            throw new ArgumentException("lhs.Length != rhs.Length");

        if (lhs.Length == 0)
            return 0;

        int nRows = lhs.Length;
        int nColumns = lhs[0].Length;

        //number of zero columns
        int nZeroColumns = 0;
        for (int iColumn = 0, to = Math.Min(nRows, nColumns); iColumn < to; ++iColumn) {

            // find pivot row and swap
            int row = iColumn - nZeroColumns;
            int max = row;
            if (ring.isZero(lhs[row][iColumn])) {
                for (int iRow = row + 1; iRow < nRows; ++iRow)
                    if (!ring.isZero(lhs[iRow][iColumn])) {
                        max = iRow;
                        break;
                    }

                ArraysUtil.swap(lhs, row, max);
                if (rhs != null)
                    ArraysUtil.swap(rhs, row, max);
            }

            // singular
            if (ring.isZero(lhs[row][iColumn])) {
                if (breakOnUnderDetermined)
                    return 1;
                //nothing to do on this column
                ++nZeroColumns;
                to = Math.Min(nRows + nZeroColumns, nColumns);
                continue;
            }

            // pivot within A and b
            for (int iRow = row + 1; iRow < nRows; ++iRow) {
                E alpha = ring.divideExact(lhs[iRow][iColumn], lhs[row][iColumn]);
                if (rhs != null)
                    rhs[iRow] = ring.subtract(rhs[iRow], ring.multiply(alpha, rhs[row]));
                if (!ring.isZero(alpha))
                    for (int iCol = iColumn; iCol < nColumns; ++iCol)
                        lhs[iRow][iCol] = ring.subtract(lhs[iRow][iCol], ring.multiply(alpha, lhs[row][iCol]));
            }
        }
        if (reduce)
            reducedRowEchelonForm(ring, lhs, rhs);
        return nZeroColumns;
    }

    /**
     * Gives the reduced row echelon form of the linear system {@code lhs.x = rhs} from a given row echelon form.
     *
     * @param ring the ring
     * @param lhs  the lhs of the system in the row echelon form
     * @param rhs  the rhs of the system
     */
    public static  void reducedRowEchelonForm<E>(Ring<E> ring, E[][] lhs, E[] rhs) {
        int nRows = lhs.Length;
        int nColumns = lhs[0].Length;

        //number of zero columns
        int nZeroColumns = 0;
        for (int iColumn = 0, to = Math.Min(nRows, nColumns); iColumn < to; ++iColumn) {
            // find pivot row and swap
            int iRow = iColumn - nZeroColumns;
            if (ring.isZero(lhs[iRow][iColumn])) {
                ++nZeroColumns;
                to = Math.Min(nRows + nZeroColumns, nColumns);
                continue;
            }

            // scale current row
            E[] row = lhs[iRow];
            E val = row[iColumn];
            E valInv = ring.reciprocal(val);

            for (int i = iColumn; i < nColumns; i++)
                row[i] = ring.multiply(valInv, row[i]);
            if (rhs != null)
                rhs[iRow] = ring.multiply(valInv, rhs[iRow]);

            // scale all rows before
            for (int i = 0; i < iRow; i++) {
                E[] pRow = lhs[i];
                E v = pRow[iColumn];
                if (ring.isZero(v))
                    continue;
                for (int j = iColumn; j < nColumns; ++j)
                    pRow[j] = ring.subtract(pRow[j], ring.multiply(v, row[j]));
                if (rhs != null)
                    rhs[i] = ring.subtract(rhs[i], ring.multiply(v, rhs[iColumn]));
            }
        }
    }

    /**
     * Solves linear system {@code lhs.x = rhs} and reduces lhs to row echelon form.
     *
     * @param ring the ring
     * @param lhs  the lhs of the system (will be reduced to row echelon form)
     * @param rhs  the rhs of the system
     * @return the solution
     * @throws ArithmeticException if the system is inconsistent or under-determined
     */
    public static E[] solve<E>(Ring<E> ring, E[][] lhs, E[] rhs) {
        int nUnknowns = lhs[0].Length;
        if (nUnknowns == 0)
            return [];
        E[] result = new E[nUnknowns];
        SystemInfo info = solve(ring, lhs, rhs, result);
        if (info != SystemInfo.Consistent)
            throw new ArithmeticException("singular or under-determined matrix");
        return result;
    }

    /**
     * Info about linear system
     */
    public enum SystemInfo {
        /** Under-determined system */
        UnderDetermined,
        /** Inconsistent system */
        Inconsistent,
        /** Consistent system */
        Consistent
    }

    /**
     * Solves linear system {@code lhs.x = rhs} and reduces the lhs to row echelon form. The result is stored in {@code
     * result} (which should be of the enough Length).
     *
     * @param ring   the ring
     * @param lhs    the lhs of the system (will be reduced to row echelon form)
     * @param rhs    the rhs of the system
     * @param result where to place the result
     * @return system information (inconsistent, under-determined or consistent)
     */
    public static  SystemInfo solve<E>(Ring<E> ring, E[][] lhs, E[] rhs, E[] result) {
        return solve(ring, lhs, rhs, result, false);
    }

    /**
     * Solves linear system {@code lhs.x = rhs} and reduces the lhs to row echelon form. The result is stored in {@code
     * result} (which should be of the enough Length).
     *
     * @param ring                   the ring
     * @param lhs                    the lhs of the system (will be reduced to row echelon form)
     * @param rhs                    the rhs of the system
     * @param result                 where to place the result
     * @param solveIfUnderDetermined give some solution even if the system is under determined
     * @return system information (inconsistent, under-determined or consistent)
     */
    public static SystemInfo solve<E>(Ring<E> ring, E[][] lhs, E[] rhs, E[] result, bool solveIfUnderDetermined) {
        if (lhs.Length != rhs.Length)
            throw new ArgumentException("lhs.Length != rhs.Length");
        if (rhs.Length == 0)
            return SystemInfo.Consistent;
        if (rhs.Length == 1) {
            if (lhs[0].Length == 1) {
                result[0] = ring.divideExact(rhs[0], lhs[0][0]);
                return SystemInfo.Consistent;
            }
            if (solveIfUnderDetermined) {
                ring.fillZeros(result);
                if (ring.isZero(rhs[0]))
                    return SystemInfo.Consistent;
                
                for (int i = 0; i < result.Length; ++i)
                    if (!ring.isZero(lhs[0][i])) {
                        result[i] = ring.divideExact(rhs[0], lhs[0, i]);
                        return SystemInfo.Consistent;
                    }

                return SystemInfo.Inconsistent;
            }
            if (lhs[0].Length > 1)
                return SystemInfo.UnderDetermined;

            return SystemInfo.Inconsistent;
        }

        int nUnderDetermined = rowEchelonForm(ring, lhs, rhs, false, !solveIfUnderDetermined);
        if (!solveIfUnderDetermined && nUnderDetermined > 0)
            // under-determined system
            return SystemInfo.UnderDetermined;

        int nRows = rhs.Length;
        int nColumns = lhs[0].Length;

        if (!solveIfUnderDetermined && nColumns > nRows)
            // under-determined system
            return SystemInfo.UnderDetermined;

        if (nRows > nColumns)
            // over-determined system
            // check that all rhs are zero
            for (int i = nColumns; i < nRows; ++i)
                if (!ring.isZero(rhs[i]))
                    // inconsistent system
                    return SystemInfo.Inconsistent;

        if (nRows > nColumns)
            for (int i = nColumns + 1; i < nRows; ++i)
                if (!ring.isZero(rhs[i]))
                    return SystemInfo.Inconsistent;

        ring.fillZeros(result);
        // back substitution in case of determined system
        if (nUnderDetermined == 0 && nColumns <= nRows) {
            for (int i = nColumns - 1; i >= 0; i--) {
                E sum = ring.getZero();
                for (int j = i + 1; j < nColumns; j++)
                    sum = ring.add(sum, ring.multiply(lhs[i][j], result[j]));
                result[i] = ring.divideExact(ring.subtract(rhs[i], sum), lhs[i][i]);
            }
            return SystemInfo.Consistent;
        }

        // back substitution in case of underdetermined system
        TIntArrayList nzColumns = new TIntArrayList(), nzRows = new TIntArrayList();
        //number of zero columns
        int nZeroColumns = 0;
        int iRow = 0;
        for (int iColumn = 0, to = Math.Min(nRows, nColumns); iColumn < to; ++iColumn) {
            // find pivot row and swap
            iRow = iColumn - nZeroColumns;
            if (ring.isZero(lhs[iRow][iColumn])) {
                if (iColumn == (nColumns - 1) && !ring.isZero(rhs[iRow]))
                    return SystemInfo.Inconsistent;
                ++nZeroColumns;
                to = Math.Min(nRows + nZeroColumns, nColumns);
                continue;
            }

            // scale current row
            E[] row = lhs[iRow];
            E val = row[iColumn];
            E valInv = ring.reciprocal(val);

            for (int i = iColumn; i < nColumns; i++)
                row[i] = ring.multiply(valInv, row[i]);
            rhs[iRow] = ring.multiply(valInv, rhs[iRow]);

            // scale all rows before
            for (int i = 0; i < iRow; i++) {
                E[] pRow = lhs[i];
                E v = pRow[iColumn];
                if (ring.isZero(v))
                    continue;
                for (int j = iColumn; j < nColumns; ++j)
                    pRow[j] = ring.subtract(pRow[j], ring.multiply(v, row[j]));
                rhs[i] = ring.subtract(rhs[i], ring.multiply(v, rhs[iRow]));
            }

            if (!ring.isZero(rhs[iRow]) && ring.isZero(lhs[iRow][iColumn]))
                return SystemInfo.Inconsistent;

            nzColumns.add(iColumn);
            nzRows.add(iRow);
        }

        ++iRow;
        if (iRow < nRows)
            for (; iRow < nRows; ++iRow)
                if (!ring.isZero(rhs[iRow]))
                    return SystemInfo.Inconsistent;

        for (int i = 0; i < nzColumns.size(); ++i)
            result[nzColumns.get(i)] = rhs[nzRows.get(i)];

        return SystemInfo.Consistent;
    }

    /**
     * Solves linear system {@code lhs.x = rhs} and stores the result in {@code result} (which should be of the enough
     * Length).
     *
     * @param ring   the ring
     * @param lhs    the lhs of the system
     * @param rhs    the rhs of the system
     * @param result where to place the result
     * @return system information (inconsistent, under-determined or consistent)
     */
    public static  SystemInfo solve<E>(Ring<E> ring, List<E[]> lhs, List<E> rhs, E[] result) {
        return solve(ring, lhs.ToArray(), rhs.ToArray(), result);
    }

    /**
     * Solves Vandermonde linear system (that is with i-th equation of the form {@code row[i]^0 * x0 +  row[i]^1 * x1 +
     * ... row[i]^N * xN = rhs[i] }).
     *
     * @param ring the ring
     * @param row  the Vandermonde coefficients
     * @param rhs  the rhs of the system
     * @return the solution
     * @throws ArithmeticException if the system is inconsistent or under-determined
     */
    public static  E[] solveVandermonde<E>(Ring<E> ring, E[] row, E[] rhs) {
        E[] result = new E[rhs.Length];
        SystemInfo info = solveVandermonde(ring, row, rhs, result);
        if (info != SystemInfo.Consistent)
            throw new ArithmeticException("singular or under-determined matrix");
        return result;
    }

    /**
     * Solves transposed Vandermonde linear system (that is with i-th equation of the form {@code row[0]^i * x0 +
     * row[1]^i * x1 + ... row[N]^i * xN = rhs[i] }).
     *
     * @param ring the ring
     * @param row  the Vandermonde coefficients
     * @param rhs  the rhs of the system
     * @return the solution
     * @throws ArithmeticException if the system is inconsistent or under-determined
     */
    public static  E[] solveVandermondeT<E>(Ring<E> ring, E[] row, E[] rhs) {
        E[] result = new E[rhs.Length];
        SystemInfo info = solveVandermondeT(ring, row, rhs, result);
        if (info != SystemInfo.Consistent)
            throw new ArithmeticException("singular or under-determined matrix");
        return result;
    }

    /**
     * Solves Vandermonde linear system (that is with i-th equation of the form {@code row[i]^0 * x0 +  row[i]^1 * x1 +
     * ... row[i]^N * xN = rhs[i] }) and stores the result in {@code result} (which should be of the enough Length).
     *
     * @param ring   the ring
     * @param row    the Vandermonde coefficients
     * @param rhs    the rhs of the system
     * @param result where to place the result
     * @return system information (inconsistent, under-determined or consistent)
     */
    public static  SystemInfo solveVandermonde<E>(Ring<E> ring, E[] row, E[] rhs, E[] result) {
        if (row.Length != rhs.Length)
            throw new ArgumentException("not a square Vandermonde matrix");
        if (rhs.Length == 0)
            return SystemInfo.Consistent;
        if (rhs.Length == 1) {
            result[0] = rhs[0];
            return SystemInfo.Consistent;
        }
        
        UnivariatePolynomial<E>[] lins = new UnivariatePolynomial<E>[row.Length];
        UnivariatePolynomial<E> master = UnivariatePolynomial<E>.one(ring);
        for (int i = 0; i < row.Length; ++i) {
            lins[i] = master.createLinear(ring.negate(row[i]), ring.getOne());
            master = master.multiply(lins[i]);
        }


        for (int i = 0; i < result.Length; i++)
            result[i] = ring.getZero();

        for (int i = 0; i < row.Length; i++) {
            UnivariatePolynomial<E> quot = UnivariateDivision.divideAndRemainder(master, lins[i], true)[0];
            E cf = quot.evaluate(row[i]);
            if (ring.isZero(cf))
                return SystemInfo.UnderDetermined;
            quot = quot.divideOrNull(cf);
            if (quot == null)
                throw new ArgumentException();
            for (int j = 0; j < row.Length; ++j)
                result[j] = ring.add(result[j], ring.multiply(rhs[i], quot.get(j)));
        }
        return SystemInfo.Consistent;
    }

    /**
     * Solves transposed Vandermonde linear system (that is with i-th equation of the form {@code row[0]^i * x0 +
     * row[1]^i * x1 + ... row[N]^i * xN = rhs[i] }) and stores the result in {@code result} (which should be of the
     * enough Length).
     *
     * @param ring   the ring
     * @param row    the Vandermonde coefficients
     * @param rhs    the rhs of the system
     * @param result where to place the result
     * @return system information (inconsistent, under-determined or consistent)
     */
    public static  SystemInfo solveVandermondeT<E>(Ring<E> ring, E[] row, E[] rhs, E[] result) {
        if (row.Length != rhs.Length)
            throw new ArgumentException("not a square Vandermonde matrix");
        if (rhs.Length == 0)
            return SystemInfo.Consistent;
        if (rhs.Length == 1) {
            result[0] = rhs[0];
            return SystemInfo.Consistent;
            
        }
        UnivariatePolynomial<E>[] lins = new UnivariatePolynomial[row.Length];
        UnivariatePolynomial<E> master = UnivariatePolynomial.one(ring);
        for (int i = 0; i < row.Length; ++i) {
            lins[i] = master.createLinear(ring.negate(row[i]), ring.getOne());
            master = master.multiply(lins[i]);
        }

        for (int i = 0; i < row.Length; i++) {
            UnivariatePolynomial<E> quot = UnivariateDivision.divideAndRemainder(master, lins[i], true)[0];
            E cf = quot.evaluate(row[i]);
            if (ring.isZero(cf))
                return SystemInfo.UnderDetermined;
            quot = quot.divideOrNull(cf);
            if (quot == null)
                throw new ArgumentException();
            result[i] = ring.getZero();
            for (int j = 0; j < row.Length; ++j)
                result[i] = ring.add(result[i], ring.multiply(rhs[j], quot.get(j)));
        }
        
        return SystemInfo.Consistent;
    }


    /* ========================================= Machine numbers ============================================ */

    /**
     * Gives the row echelon form of the matrix
     *
     * @param ring   the ring
     * @param matrix the matrix
     * @return the number of free variables
     */
    public static int rowEchelonForm(IntegersZp64 ring, long[][] matrix) {
        return rowEchelonForm(ring, matrix, false);
    }

    /**
     * Gives the row echelon form of the matrix
     *
     * @param ring   the ring
     * @param matrix the matrix
     * @param reduce whether to calculate reduced row echelon form
     * @return the number of free variables
     */
    public static int rowEchelonForm(IntegersZp64 ring, long[][] matrix, bool reduce) {
        return rowEchelonForm(ring, matrix, null, reduce, false);
    }

    /**
     * Gives the row echelon form of the linear system {@code lhs.x = rhs} (rhs may be null).
     *
     * @param ring the ring
     * @param lhs  the lhs of the system
     * @param rhs  the rhs of the system (may be null)
     * @return the number of free variables
     */
    public static int rowEchelonForm(IntegersZp64 ring, long[][] lhs, long[] rhs) {
        return rowEchelonForm(ring, lhs, rhs, false, false);
    }

    /**
     * Gives the row echelon form of the linear system {@code lhs.x = rhs} (rhs may be null).
     *
     * @param ring                   the ring
     * @param lhs                    the lhs of the system
     * @param rhs                    the rhs of the system (may be null)
     * @param reduce                 whether to calculate reduced row echelon form
     * @param breakOnUnderDetermined whether to return immediately if it was detected that system is under determined
     * @return the number of free variables
     */
    public static int rowEchelonForm(IntegersZp64 ring, long[][] lhs, long[] rhs, bool reduce, bool breakOnUnderDetermined) {
        if (rhs != null && lhs.Length != rhs.Length)
            throw new ArgumentException("lhs.Length != rhs.Length");

        if (lhs.Length == 0)
            return 0;

        int nRows = lhs.Length;
        int nColumns = lhs[0].Length;

        //number of zero columns
        int nZeroColumns = 0;
        for (int iColumn = 0, to = Math.Min(nRows, nColumns); iColumn < to; ++iColumn) {

            // find pivot row and swap
            int row = iColumn - nZeroColumns;
            int nonZero = row;
            if (lhs[row][iColumn] == 0) {
                for (int iRow = row + 1; iRow < nRows; ++iRow)
                    if (lhs[iRow][iColumn] != 0) {
                        nonZero = iRow;
                        break;
                    }

                ArraysUtil.swap(lhs, row, nonZero);
                if (rhs != null)
                    ArraysUtil.swap(rhs, row, nonZero);
            }

            // singular
            if (lhs[row][iColumn] == 0) {
                if (breakOnUnderDetermined)
                    return 1;
                //nothing to do on this column
                ++nZeroColumns;
                to = Math.Min(nRows + nZeroColumns, nColumns);
                continue;
            }

            // pivot within A and b
            for (int iRow = row + 1; iRow < nRows; ++iRow) {
                long alpha = ring.divide(lhs[iRow][iColumn], lhs[row][iColumn]);
                if (rhs != null)
                    rhs[iRow] = ring.subtract(rhs[iRow], ring.multiply(alpha, rhs[row]));
                if (alpha != 0)
                    for (int iCol = iColumn; iCol < nColumns; ++iCol)
                        lhs[iRow][iCol] = ring.subtract(lhs[iRow][iCol], ring.multiply(alpha, lhs[row][iCol]));
            }
        }
        if (reduce)
            reducedRowEchelonForm(ring, lhs, rhs);
        return nZeroColumns;
    }

    /**
     * Gives the reduced row echelon form of the linear system {@code lhs.x = rhs} from a given row echelon form.
     *
     * @param ring the ring
     * @param lhs  the lhs of the system in the row echelon form
     * @param rhs  the rhs of the system
     */
    public static void reducedRowEchelonForm(IntegersZp64 ring, long[][] lhs, long[] rhs) {
        int nRows = lhs.Length;
        int nColumns = lhs[0].Length;

        //number of zero columns
        int nZeroColumns = 0;
        for (int iColumn = 0, to = Math.Min(nRows, nColumns); iColumn < to; ++iColumn) {
            // find pivot row and swap
            int iRow = iColumn - nZeroColumns;
            if (lhs[iRow][iColumn] == 0) {
                ++nZeroColumns;
                to = Math.Min(nRows + nZeroColumns, nColumns);
                continue;
            }

            // scale current row
            long[] row = lhs[iRow];
            long val = row[iColumn];
            long valInv = ring.reciprocal(val);

            for (int i = iColumn; i < nColumns; i++)
                row[i] = ring.multiply(valInv, row[i]);
            if (rhs != null)
                rhs[iRow] = ring.multiply(valInv, rhs[iRow]);

            // scale all rows before
            for (int i = 0; i < iRow; i++) {
                long[] pRow = lhs[i];
                long v = pRow[iColumn];
                if (v == 0)
                    continue;
                for (int j = iColumn; j < nColumns; ++j)
                    pRow[j] = ring.subtract(pRow[j], ring.multiply(v, row[j]));
                if (rhs != null)
                    rhs[i] = ring.subtract(rhs[i], ring.multiply(v, rhs[iRow]));
            }
        }
    }

    /**
     * Solves linear system {@code lhs.x = rhs} and reduces the lhs to row echelon form.
     *
     * @param ring the ring
     * @param lhs  the lhs of the system  (will be reduced to row echelon form)
     * @param rhs  the rhs of the system
     * @return the solution
     * @throws ArithmeticException if the system is inconsistent or under-determined
     */
    public static long[] solve(IntegersZp64 ring, long[][] lhs, long[] rhs) {
        int nUnknowns = lhs[0].Length;
        if (nUnknowns == 0)
            return new long[0];
        long[] result = new long[nUnknowns];
        SystemInfo info = solve(ring, lhs, rhs, result);
        if (info != SystemInfo.Consistent)
            throw new ArithmeticException("singular or under-determined matrix");
        return result;
    }

    /**
     * Solves linear system {@code lhs.x = rhs} and reduces the lhs to row echelon form. The result is stored in {@code
     * result} (which should be of the enough Length).
     *
     * @param ring   the ring
     * @param lhs    the lhs of the system  (will be reduced to row echelon form)
     * @param rhs    the rhs of the system
     * @param result where to place the result
     * @return system information (inconsistent, under-determined or consistent)
     */
    public static SystemInfo solve(IntegersZp64 ring, long[][] lhs, long[] rhs, long[] result) {
        return solve(ring, lhs, rhs, result, false);
    }

    /**
     * Solves linear system {@code lhs.x = rhs} and reduces the lhs to row echelon form. The result is stored in {@code
     * result} (which should be of the enough Length and filled with zeros).
     *
     * @param ring                   the ring
     * @param lhs                    the lhs of the system  (will be reduced to row echelon form)
     * @param rhs                    the rhs of the system
     * @param result                 where to place the result
     * @param solveIfUnderDetermined give some solution even if the system is under determined
     * @return system information (inconsistent, under-determined or consistent)
     */
    public static SystemInfo solve(IntegersZp64 ring, long[][] lhs, long[] rhs, long[] result, bool solveIfUnderDetermined) {
        if (lhs.Length != rhs.Length)
            throw new ArgumentException("lhs.Length != rhs.Length");
        if (rhs.Length == 0)
            return SystemInfo.Consistent;
        if (rhs.Length == 1) {
            if (lhs[0].Length == 1) {
                result[0] = ring.divide(rhs[0], lhs[0][0]);
                return SystemInfo.Consistent;
            }
            if (solveIfUnderDetermined) {
                if (rhs[0] == 0)
                    return SystemInfo.Consistent;

                for (int i = 0; i < result.Length; ++i)
                    if (lhs[0][i] != 0) {
                        result[i] = ring.divide(rhs[0], lhs[0][i]);
                        return SystemInfo.Consistent;
                    }

                return SystemInfo.Inconsistent;
            }
            if (lhs[0].Length > 1)
                return SystemInfo.UnderDetermined;

            return SystemInfo.Inconsistent;
        }

        int nUnderDetermined = rowEchelonForm(ring, lhs, rhs, false, !solveIfUnderDetermined);
        if (!solveIfUnderDetermined && nUnderDetermined > 0)
            // under-determined system
            return SystemInfo.UnderDetermined;

        int nRows = rhs.Length;
        int nColumns = lhs[0].Length;

        if (!solveIfUnderDetermined && nColumns > nRows)
            // under-determined system
            return SystemInfo.UnderDetermined;

        if (nRows > nColumns)
            // over-determined system
            // check that all rhs are zero
            for (int i = nColumns; i < nRows; ++i)
                if (rhs[i] != 0)
                    // inconsistent system
                    return SystemInfo.Inconsistent;

        if (nRows > nColumns)
            for (int i = nColumns + 1; i < nRows; ++i)
                if (rhs[i] != 0)
                    return SystemInfo.Inconsistent;

        // back substitution in case of determined system
        if (nUnderDetermined == 0 && nColumns <= nRows) {
            for (int i = nColumns - 1; i >= 0; i--) {
                long sum = 0;
                for (int j = i + 1; j < nColumns; j++)
                    sum = ring.add(sum, ring.multiply(lhs[i][j], result[j]));
                result[i] = ring.divide(ring.subtract(rhs[i], sum), lhs[i][i]);
            }
            return SystemInfo.Consistent;
        }

        // back substitution in case of underdetermined system
        TIntArrayList nzColumns = new TIntArrayList(), nzRows = new TIntArrayList();
        //number of zero columns
        int nZeroColumns = 0;
        int iRow = 0;
        for (int iColumn = 0, to = Math.Min(nRows, nColumns); iColumn < to; ++iColumn) {
            // find pivot row and swap
            iRow = iColumn - nZeroColumns;
            if (lhs[iRow][iColumn] == 0) {
                if (iColumn == (nColumns - 1) && rhs[iRow] != 0)
                    return SystemInfo.Inconsistent;
                ++nZeroColumns;
                to = Math.Min(nRows + nZeroColumns, nColumns);
                continue;
            }

            // scale current row
            long[] row = lhs[iRow];
            long val = row[iColumn];
            long valInv = ring.reciprocal(val);

            for (int i = iColumn; i < nColumns; i++)
                row[i] = ring.multiply(valInv, row[i]);
            rhs[iRow] = ring.multiply(valInv, rhs[iRow]);

            // scale all rows before
            for (int i = 0; i < iRow; i++) {
                long[] pRow = lhs[i];
                long v = pRow[iColumn];
                if (v == 0)
                    continue;
                for (int j = iColumn; j < nColumns; ++j)
                    pRow[j] = ring.subtract(pRow[j], ring.multiply(v, row[j]));
                rhs[i] = ring.subtract(rhs[i], ring.multiply(v, rhs[iRow]));
            }

            if (rhs[iRow] != 0 && lhs[iRow][iColumn] == 0)
                return SystemInfo.Inconsistent;

            nzColumns.add(iColumn);
            nzRows.add(iRow);
        }

        ++iRow;
        if (iRow < nRows)
            for (; iRow < nRows; ++iRow)
                if (rhs[iRow] != 0)
                    return SystemInfo.Inconsistent;

        for (int i = 0; i < nzColumns.size(); ++i)
            result[nzColumns.get(i)] = rhs[nzRows.get(i)];

        return SystemInfo.Consistent;
    }

    /**
     * Solves linear system {@code lhs.x = rhs} and stores the result in {@code result} (which should be of the enough
     * Length).
     *
     * @param ring   the ring
     * @param lhs    the lhs of the system
     * @param rhs    the rhs of the system
     * @param result where to place the result
     * @return system information (inconsistent, under-determined or consistent)
     */
    public static SystemInfo solve(IntegersZp64 ring, List<long[]> lhs, TLongArrayList rhs, long[] result) {
        return solve(ring, lhs.ToArray(), rhs.toArray(), result);
    }

    /**
     * Solves Vandermonde linear system (that is with i-th equation of the form {@code row[i]^0 * x0 +  row[i]^1 * x1 +
     * ... row[i]^N * xN = rhs[i] }).
     *
     * @param ring the ring
     * @param row  the Vandermonde coefficients
     * @param rhs  the rhs of the system
     * @return the solution
     * @throws ArithmeticException if the system is inconsistent or under-determined
     */
    public static long[] solveVandermonde(IntegersZp64 ring, long[] row, long[] rhs) {
        long[] result = new long[rhs.Length];
        SystemInfo info = solveVandermonde(ring, row, rhs, result);
        if (info != SystemInfo.Consistent)
            throw new ArithmeticException("singular or under-determined matrix");
        return result;
    }

    /**
     * Solves transposed Vandermonde linear system (that is with i-th equation of the form {@code row[0]^i * x0 +
     * row[1]^i * x1 + ... row[N]^i * xN = rhs[i] }).
     *
     * @param ring the ring
     * @param row  the Vandermonde coefficients
     * @param rhs  the rhs of the system
     * @return the solution
     * @throws ArithmeticException if the system is inconsistent or under-determined
     */
    public static long[] solveVandermondeT(IntegersZp64 ring, long[] row, long[] rhs) {
        long[] result = new long[rhs.Length];
        SystemInfo info = solveVandermondeT(ring, row, rhs, result);
        if (info != SystemInfo.Consistent)
            throw new ArithmeticException("singular or under-determined matrix");
        return result;
    }

    /**
     * Solves Vandermonde linear system (that is with i-th equation of the form {@code row[i]^0 * x0 +  row[i]^1 * x1 +
     * ... row[i]^N * xN = rhs[i] }) and stores the result in {@code result} (which should be of the enough Length).
     *
     * @param ring   the ring
     * @param row    the Vandermonde coefficients
     * @param rhs    the rhs of the system
     * @param result where to place the result
     * @return system information (inconsistent, under-determined or consistent)
     */
    public static SystemInfo solveVandermonde(IntegersZp64 ring, long[] row, long[] rhs, long[] result) {
        if (row.Length != rhs.Length)
            throw new ArgumentException("not a square Vandermonde matrix");
        if (rhs.Length == 0)
            return SystemInfo.Consistent;
        if (rhs.Length == 1) {
            result[0] = rhs[0];
            return SystemInfo.Consistent;
        }
        
        UnivariatePolynomialZp64[] lins = new UnivariatePolynomialZp64[row.Length];
        UnivariatePolynomialZp64 master = UnivariatePolynomialZp64.one(ring);
        for (int i = 0; i < row.Length; ++i) {
            lins[i] = master.createLinear(ring.negate(row[i]), 1L);
            master = master.multiply(lins[i]);
        }


        for (int i = 0; i < result.Length; i++)
            result[i] = 0;

        for (int i = 0; i < row.Length; i++) {
            UnivariatePolynomialZp64 quot = UnivariateDivision.divideAndRemainder(master, lins[i], true)[0];
            long cf = quot.evaluate(row[i]);
            if (cf == 0)
                return SystemInfo.UnderDetermined;
            quot = quot.divide(cf);
            for (int j = 0; j < row.Length; ++j)
                result[j] = ring.add(result[j], ring.multiply(rhs[i], quot.get(j)));
        }
        return SystemInfo.Consistent;
    }

    /**
     * Solves transposed Vandermonde linear system (that is with i-th equation of the form {@code row[0]^i * x0 +
     * row[1]^i * x1 + ... row[N]^i * xN = rhs[i] }) and stores the result in {@code result} (which should be of the
     * enough Length).
     *
     * @param ring   the ring
     * @param row    the Vandermonde coefficients
     * @param rhs    the rhs of the system
     * @param result where to place the result
     * @return system information (inconsistent, under-determined or consistent)
     */
    public static SystemInfo solveVandermondeT(IntegersZp64 ring, long[] row, long[] rhs, long[] result) {
        if (row.Length != rhs.Length)
            throw new ArgumentException("not a square Vandermonde matrix");
        if (rhs.Length == 0)
            return SystemInfo.Consistent;
        if (rhs.Length == 1) {
            result[0] = rhs[0];
            return SystemInfo.Consistent;
        }

        UnivariatePolynomialZp64[] lins = new UnivariatePolynomialZp64[row.Length];
        UnivariatePolynomialZp64 master = UnivariatePolynomialZp64.one(ring);
        for (int i = 0; i < row.Length; ++i) {
            lins[i] = master.createLinear(ring.negate(row[i]), 1L);
            master = master.multiply(lins[i]);
        }

        for (int i = 0; i < row.Length; i++) {
            UnivariatePolynomialZp64 quot = UnivariateDivision.divideAndRemainder(master, lins[i], true)[0];
            long cf = quot.evaluate(row[i]);
            if (cf == 0)
                return SystemInfo.UnderDetermined;
            quot = quot.divide(cf);
            if (quot == null)
                throw new ArgumentException();
            result[i] = 0;
            for (int j = 0; j < row.Length; ++j)
                result[i] = ring.add(result[i], ring.multiply(rhs[j], quot.get(j)));
        }
        return SystemInfo.Consistent;
    }
}
