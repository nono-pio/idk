/*
 * Redberry: symbolic tensor computations.
 *
 * Copyright (c) 2010-2015:
 *   Stanislav Poslavsky   <stvlpos@mail.ru>
 *   Bolotin Dmitriy       <bolotin.dmitriy@gmail.com>
 *
 * This file is part of Redberry.
 *
 * Redberry is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Redberry is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Redberry. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Util.RoundingMode;
using static Cc.Redberry.Rings.Util.Associativity;
using static Cc.Redberry.Rings.Util.Operator;
using static Cc.Redberry.Rings.Util.TokenType;
using static Cc.Redberry.Rings.Util.SystemInfo;

namespace Cc.Redberry.Rings.Util
{
    /// <summary>
    /// This class copied and adapted for int[] sorting from OpenJDK7
    /// 
    /// A stable, adaptive, iterative mergesort that requires far fewer than
    /// n lg(n) comparisons when running on partially sorted arrays, while
    /// offering performance comparable to a traditional mergesort when run
    /// on random arrays.  Like all proper mergesorts, this sort is stable and
    /// runs O(n log n) time (worst case).  In the worst case, this sort requires
    /// temporary storage space for n/2 object references; in the best case,
    /// it requires only a small constant amount of space.
    /// 
    /// This implementation was adapted from Tim Peters's list sort for
    /// Python, which is described in detail here:
    /// 
    ///   http://svn.python.org/projects/python/trunk/Objects/listsort.txt
    /// 
    /// Tim's C code may be found here:
    /// 
    ///   http://svn.python.org/projects/python/trunk/Objects/listobject.c
    /// 
    /// The underlying techniques are described in this paper (and may have
    /// even earlier origins):
    /// 
    ///  "Optimistic Sorting and Information Theoretic Complexity"
    ///  Peter McIlroy
    ///  SODA (Fourth Annual ACM-SIAM Symposium on Discrete Algorithms),
    ///  pp 467-474, Austin, Texas, 25-27 January 1993.
    /// 
    /// While the API to this class consists solely of static methods, it is
    /// (privately) instantiable; a TimSort instance holds the state of an ongoing
    /// sort, assuming the input array is large enough to warrant the full-blown
    /// TimSort. Small arrays are sorted in place, using a binary insertion sort.
    /// </summary>
    /// <remarks>
    /// @authorJosh Bloch
    /// @authorDmitry Bolotin
    /// @authorStanislav Poslavsky
    /// </remarks>
    class IntTimSort
    {
        /// <summary>
        /// This is the minimum sized sequence that will be merged.  Shorter
        /// sequences will be lengthened by calling binarySort.  If the entire
        /// array is less than this length, no merges will be performed.
        /// 
        /// This constant should be a power of two.  It was 64 in Tim Peter's C
        /// implementation, but 32 was empirically determined to work better in
        /// this implementation.  In the unlikely event that you set this constant
        /// to be a number that's not a power of two, you'll need to change the
        /// {@link #minRunLength} computation.
        /// 
        /// If you decrease this constant, you must change the stackLen
        /// computation in the TimSort constructor, or you risk an
        /// ArrayOutOfBounds exception.  See listsort.txt for a discussion
        /// of the minimum stack length required as a function of the length
        /// of the array being sorted and the minimum merge sequence length.
        /// </summary>
        private static readonly int MIN_MERGE = 32;
        /// <summary>
        /// The array being sorted.
        /// </summary>
        private readonly int[] a;
        /// <summary>
        /// The array being co-sorted.
        /// </summary>
        private readonly int[] b;
        /// <summary>
        /// When we get into galloping mode, we stay there until both runs win less
        /// often than MIN_GALLOP consecutive times.
        /// </summary>
        private static readonly int MIN_GALLOP = 7;
        /// <summary>
        /// This controls when we get *into* galloping mode.  It is initialized
        /// to MIN_GALLOP.  The mergeLo and mergeHi methods nudge it higher for
        /// random data, and lower for highly structured data.
        /// </summary>
        private int minGallop = MIN_GALLOP;
        /// <summary>
        /// Maximum initial size of tmp array, which is used for merging.  The array
        /// can grow to accommodate demand.
        /// 
        /// Unlike Tim's original C version, we do not allocate this much storage
        /// when sorting smaller arrays.  This change was required for performance.
        /// </summary>
        private static readonly int INITIAL_TMP_STORAGE_LENGTH = 256;
        /// <summary>
        /// Temp storage for merges.
        /// </summary>
        private int[] tmp; // Actual runtime type will be Object[], regardless of T
        private int[] tmpB; // Actual runtime type will be Object[], regardless of T
        /// <summary>
        /// A stack of pending runs yet to be merged.  Run i starts at
        /// address base[i] and extends for len[i] elements.  It's always
        /// true (so long as the indices are in bounds) that:
        /// 
        ///     runBase[i] + runLen[i] == runBase[i + 1]
        /// 
        /// so we could cut the storage for this, but it's a minor amount,
        /// and keeping all the info explicit simplifies the code.
        /// </summary>
        private int stackSize = 0; // Number of pending runs on stack
        private readonly int[] runBase;
        private readonly int[] runLen;
        /// <summary>
        /// Creates a TimSort instance to maintain the state of an ongoing sort.
        /// </summary>
        /// <param name="a">the array to be sorted</param>
        /// <param name="c">the comparator to determine the order of the sort</param>
        private IntTimSort(int[] a, int[] b)
        {
            this.a = a;
            this.b = b;

            // Allocate temp storage (which may be increased later if necessary)
            int len = a.Length;
            int[] newArray = new int[len < 2 * INITIAL_TMP_STORAGE_LENGTH ? len >>> 1 : INITIAL_TMP_STORAGE_LENGTH];
            tmp = newArray;

            //New code
            int[] newArrayB = new int[len < 2 * INITIAL_TMP_STORAGE_LENGTH ? len >>> 1 : INITIAL_TMP_STORAGE_LENGTH];
            tmpB = newArrayB;
            /*
             * Allocate runs-to-be-merged stack (which cannot be expanded).  The
             * stack length requirements are described in listsort.txt.  The C
             * version always uses the same stack length (85), but this was
             * measured to be too expensive when sorting "mid-sized" arrays (e.g.,
             * 100 elements) in Java.  Therefore, we use smaller (but sufficiently
             * large) stack lengths for smaller arrays.  The "magic numbers" in the
             * computation below must be changed if MIN_MERGE is decreased.  See
             * the MIN_MERGE declaration above for more information.
             */
            int stackLen = (len < 120 ? 5 : len < 1542 ? 10 : len < 119151 ? 19 : 40);
            runBase = new int[stackLen];
            runLen = new int[stackLen];
        }

        /*
         * The next two methods (which are package private and static) constitute
         * the entire API of this class.  Each of these methods obeys the contract
         * of the public method with the same signature in java.util.Arrays.
         */
        static void Sort(int[] a, int[] b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException();
            Sort(a, 0, a.Length, b);
        }

        static void Sort(int[] a, int lo, int hi, int[] b)
        {
            RangeCheck(a.Length, lo, hi);
            int nRemaining = hi - lo;
            if (nRemaining < 2)
                return; // Arrays of size 0 and 1 are always sorted

            // If array is small, do a "mini-TimSort" with no merges
            if (nRemaining < MIN_MERGE)
            {
                int initRunLen = CountRunAndMakeAscending(a, lo, hi, b);
                BinarySort(a, lo, hi, lo + initRunLen, b);
                return;
            }


            /// <summary>
            /// March over the array once, left to right, finding natural runs,
            /// extending short natural runs to minRun elements, and merging runs
            /// to maintain stack invariant.
            /// </summary>
            IntTimSort ts = new IntTimSort(a, b);
            int minRun = MinRunLength(nRemaining);
            do
            {

                // Identify next run
                int runLen = CountRunAndMakeAscending(a, lo, hi, b);

                // If run is short, extend to min(minRun, nRemaining)
                if (runLen < minRun)
                {
                    int force = nRemaining <= minRun ? nRemaining : minRun;
                    BinarySort(a, lo, lo + force, lo + runLen, b);
                    runLen = force;
                }


                // Push run onto pending-run stack, and maybe merge
                ts.PushRun(lo, runLen);
                ts.MergeCollapse();

                // Advance to find next run
                lo += runLen;
                nRemaining -= runLen;
            }
            while (nRemaining != 0);
            ts.MergeForceCollapse();
        }

        /// <summary>
        /// Sorts the specified portion of the specified array using a binary
        /// insertion sort.  This is the best method for sorting small numbers
        /// of elements.  It requires O(n log n) compares, but O(n^2) data
        /// movement (worst case).
        /// 
        /// If the initial part of the specified range is already sorted,
        /// this method can take advantage of it: the method assumes that the
        /// elements from index {@code lo}, inclusive, to {@code start},
        /// exclusive are already sorted.
        /// </summary>
        /// <param name="a">the array in which a range is to be sorted</param>
        /// <param name="lo">the index of the first element in the range to be sorted</param>
        /// <param name="hi">the index after the last element in the range to be sorted</param>
        /// <param name="start">the index of the first element in the range that is
        ///        not already known to be sorted ({@code lo <= start <= hi})</param>
        /// <param name="c">comparator to used for the sort</param>
        private static void BinarySort(int[] a, int lo, int hi, int start, int[] b)
        {
            if (start == lo)
                start++;
            for (; start < hi; start++)
            {
                int pivot = a[start];
                int pivotB = b[start];

                // Set left (and right) to the index where a[start] (pivot) belongs
                int left = lo;
                int right = start;
                /*
                 * Invariants:
                 *   pivot >= all in [lo, left).
                 *   pivot <  all in [right, start).
                 */
                while (left < right)
                {
                    int mid = (left + right) >>> 1;
                    if (pivot < a[mid])
                        right = mid;
                    else
                        left = mid + 1;
                }

                /*
                 * The invariants still hold: pivot >= all in [lo, left) and
                 * pivot < all in [left, start), so pivot belongs at left.  Note
                 * that if there are elements equal to pivot, left points to the
                 * first slot after them -- that's why this sort is stable.
                 * Slide elements over to make room for pivot.
                 */
                int n = start - left; // The number of elements to move

                // Switch is just an optimization for arraycopy in default case
                switch (n)
                {
                    case 2:
                        a[left + 2] = a[left + 1];
                        b[left + 2] = b[left + 1];
                    case 1:
                        a[left + 1] = a[left];
                        b[left + 1] = b[left];
                        break;
                    default:
                        System.Arraycopy(a, left, a, left + 1, n);
                        System.Arraycopy(b, left, b, left + 1, n);
                        break;
                }

                a[left] = pivot;
                b[left] = pivotB;
            }
        }

        /// <summary>
        /// Returns the length of the run beginning at the specified position in
        /// the specified array and reverses the run if it is descending (ensuring
        /// that the run will always be ascending when the method returns).
        /// 
        /// A run is the longest ascending sequence with:
        /// 
        ///    a[lo] <= a[lo + 1] <= a[lo + 2] <= ...
        /// 
        /// or the longest descending sequence with:
        /// 
        ///    a[lo] >  a[lo + 1] >  a[lo + 2] >  ...
        /// 
        /// For its intended use in a stable mergesort, the strictness of the
        /// definition of "descending" is needed so that the call can safely
        /// reverse a descending sequence without violating stability.
        /// </summary>
        /// <param name="a">the array in which a run is to be counted and possibly reversed</param>
        /// <param name="lo">index of the first element in the run</param>
        /// <param name="hi">index after the last element that may be contained in the run.
        ///    It is required that {@code lo < hi}.</param>
        /// <param name="c">the comparator to used for the sort</param>
        /// <returns> the length of the run beginning at the specified position in
        ///          the specified array</returns>
        private static int CountRunAndMakeAscending(int[] a, int lo, int hi, int[] b)
        {
            int runHi = lo + 1;
            if (runHi == hi)
                return 1;

            // Find end of run, and reverse range if descending
            if (a[runHi++] < a[lo])
            {

                // Descending
                while (runHi < hi && a[runHi] < a[runHi - 1])
                    runHi++;
                ReverseRange(a, lo, runHi, b);
            } // Ascending
            else
                while (runHi < hi && a[runHi] >= a[runHi - 1])
                    runHi++;
            return runHi - lo;
        }

        /// <summary>
        /// Reverse the specified range of the specified array.
        /// </summary>
        /// <param name="a">the array in which a range is to be reversed</param>
        /// <param name="lo">the index of the first element in the range to be reversed</param>
        /// <param name="hi">the index after the last element in the range to be reversed</param>
        private static void ReverseRange(int[] a, int lo, int hi, int[] b)
        {
            hi--;
            while (lo < hi)
            {
                int t = a[lo];
                int e = b[lo];
                b[lo] = b[hi];
                a[lo++] = a[hi];
                b[hi] = e;
                a[hi--] = t;
            }
        }

        /// <summary>
        /// Returns the minimum acceptable run length for an array of the specified
        /// length. Natural runs shorter than this will be extended with
        /// {@link #binarySort}.
        /// 
        /// Roughly speaking, the computation is:
        /// 
        ///  If n < MIN_MERGE, return n (it's too small to bother with fancy stuff).
        ///  Else if n is an exact power of 2, return MIN_MERGE/2.
        ///  Else return an int k, MIN_MERGE/2 <= k <= MIN_MERGE, such that n/k
        ///   is close to, but strictly less than, an exact power of 2.
        /// 
        /// For the rationale, see listsort.txt.
        /// </summary>
        /// <param name="n">the length of the array to be sorted</param>
        /// <returns>the length of the minimum run to be merged</returns>
        private static int MinRunLength(int n)
        {
            int r = 0; // Becomes 1 if any 1 bits are shifted off
            while (n >= MIN_MERGE)
            {
                r |= (n & 1);
                n >>= 1;
            }

            return n + r;
        }

        /// <summary>
        /// Pushes the specified run onto the pending-run stack.
        /// </summary>
        /// <param name="runBase">index of the first element in the run</param>
        /// <param name="runLen">the number of elements in the run</param>
        private void PushRun(int runBase, int runLen)
        {
            this.runBase[stackSize] = runBase;
            this.runLen[stackSize] = runLen;
            stackSize++;
        }

        /// <summary>
        /// Examines the stack of runs waiting to be merged and merges adjacent runs
        /// until the stack invariants are reestablished:
        /// 
        ///     1. runLen[i - 3] > runLen[i - 2] + runLen[i - 1]
        ///     2. runLen[i - 2] > runLen[i - 1]
        /// 
        /// This method is called each time a new run is pushed onto the stack,
        /// so the invariants are guaranteed to hold for i < stackSize upon
        /// entry to the method.
        /// </summary>
        private void MergeCollapse()
        {
            while (stackSize > 1)
            {
                int n = stackSize - 2;
                if (n > 0 && runLen[n - 1] <= runLen[n] + runLen[n + 1])
                {
                    if (runLen[n - 1] < runLen[n + 1])
                        n--;
                    MergeAt(n);
                }
                else if (runLen[n] <= runLen[n + 1])
                    MergeAt(n);
                else
                    break; // Invariant is established
            }
        }

        /// <summary>
        /// Merges all runs on the stack until only one remains.  This method is
        /// called once, to complete the sort.
        /// </summary>
        private void MergeForceCollapse()
        {
            while (stackSize > 1)
            {
                int n = stackSize - 2;
                if (n > 0 && runLen[n - 1] < runLen[n + 1])
                    n--;
                MergeAt(n);
            }
        }

        /// <summary>
        /// Merges the two runs at stack indices i and i+1.  Run i must be
        /// the penultimate or antepenultimate run on the stack.  In other words,
        /// i must be equal to stackSize-2 or stackSize-3.
        /// </summary>
        /// <param name="i">stack index of the first of the two runs to merge</param>
        private void MergeAt(int i)
        {
            int base1 = runBase[i];
            int len1 = runLen[i];
            int base2 = runBase[i + 1];
            int len2 = runLen[i + 1];
            /*
             * Record the length of the combined runs; if i is the 3rd-last
             * run now, also slide over the last run (which isn't involved
             * in this merge).  The current run (i+1) goes away in any case.
             */
            runLen[i] = len1 + len2;
            if (i == stackSize - 3)
            {
                runBase[i + 1] = runBase[i + 2];
                runLen[i + 1] = runLen[i + 2];
            }

            stackSize--;
            /*
             * Find where the first element of run2 goes in run1. Prior elements
             * in run1 can be ignored (because they're already in place).
             */
            int k = GallopRight(a[base2], a, base1, len1, 0);
            base1 += k;
            len1 -= k;
            if (len1 == 0)
                return;
            /*
             * Find where the last element of run1 goes in run2. Subsequent elements
             * in run2 can be ignored (because they're already in place).
             */
            len2 = GallopLeft(a[base1 + len1 - 1], a, base2, len2, len2 - 1);
            if (len2 == 0)
                return;

            // Merge remaining runs, using tmp array with min(len1, len2) elements
            if (len1 <= len2)
                MergeLo(base1, len1, base2, len2);
            else
                MergeHi(base1, len1, base2, len2);
        }

        /// <summary>
        /// Locates the position at which to insert the specified key into the
        /// specified sorted range; if the range contains an element equal to key,
        /// returns the index of the leftmost equal element.
        /// </summary>
        /// <param name="key">the key whose insertion point to search for</param>
        /// <param name="a">the array in which to search</param>
        /// <param name="base">the index of the first element in the range</param>
        /// <param name="len">the length of the range; must be > 0</param>
        /// <param name="hint">the index at which to begin the search, 0 <= hint < n.
        ///     The closer hint is to the result, the faster this method will run.</param>
        /// <param name="c">the comparator used to order the range, and to search</param>
        /// <returns>the int k,  0 <= k <= n such that a[b + k - 1] < key <= a[b + k],
        ///    pretending that a[b - 1] is minus infinity and a[b + n] is infinity.
        ///    In other words, key belongs at index b + k; or in other words,
        ///    the first k elements of a should precede key, and the last n - k
        ///    should follow it.</returns>
        private static int GallopLeft(int key, int[] a, int @base, int len, int hint)
        {
            int lastOfs = 0;
            int ofs = 1;
            if (key > a[@base + hint])
            {

                // Gallop right until a[base+hint+lastOfs] < key <= a[base+hint+ofs]
                int maxOfs = len - hint;
                while (ofs < maxOfs && key > a[@base + hint + ofs])
                {
                    lastOfs = ofs;
                    ofs = (ofs << 1) + 1;
                    if (ofs <= 0)
                        ofs = maxOfs;
                }

                if (ofs > maxOfs)
                    ofs = maxOfs;

                // Make offsets relative to base
                lastOfs += hint;
                ofs += hint;
            } // key <= a[base + hint]
            else
            {

                // key <= a[base + hint]
                // Gallop left until a[base+hint-ofs] < key <= a[base+hint-lastOfs]
                int maxOfs = hint + 1;
                while (ofs < maxOfs && key <= a[@base + hint - ofs])
                {
                    lastOfs = ofs;
                    ofs = (ofs << 1) + 1;
                    if (ofs <= 0)
                        ofs = maxOfs;
                }

                if (ofs > maxOfs)
                    ofs = maxOfs;

                // Make offsets relative to base
                int tmp = lastOfs;
                lastOfs = hint - ofs;
                ofs = hint - tmp;
            }

            /*
             * Now a[base+lastOfs] < key <= a[base+ofs], so key belongs somewhere
             * to the right of lastOfs but no farther right than ofs.  Do a binary
             * search, with invariant a[base + lastOfs - 1] < key <= a[base + ofs].
             */
            lastOfs++;
            while (lastOfs < ofs)
            {
                int m = lastOfs + ((ofs - lastOfs) >>> 1);
                if (key > a[@base + m])
                    lastOfs = m + 1; // a[base + m] < key
                else
                    ofs = m; // key <= a[base + m]
            }

            return ofs;
        }

        /// <summary>
        /// Like gallopLeft, except that if the range contains an element equal to
        /// key, gallopRight returns the index after the rightmost equal element.
        /// </summary>
        /// <param name="key">the key whose insertion point to search for</param>
        /// <param name="a">the array in which to search</param>
        /// <param name="base">the index of the first element in the range</param>
        /// <param name="len">the length of the range; must be > 0</param>
        /// <param name="hint">the index at which to begin the search, 0 <= hint < n.
        ///     The closer hint is to the result, the faster this method will run.</param>
        /// <param name="c">the comparator used to order the range, and to search</param>
        /// <returns>the int k,  0 <= k <= n such that a[b + k - 1] <= key < a[b + k]</returns>
        private static int GallopRight(int key, int[] a, int @base, int len, int hint)
        {
            int ofs = 1;
            int lastOfs = 0;
            if (key < a[@base + hint])
            {

                // Gallop left until a[b+hint - ofs] <= key < a[b+hint - lastOfs]
                int maxOfs = hint + 1;
                while (ofs < maxOfs && key < a[@base + hint - ofs])
                {
                    lastOfs = ofs;
                    ofs = (ofs << 1) + 1;
                    if (ofs <= 0)
                        ofs = maxOfs;
                }

                if (ofs > maxOfs)
                    ofs = maxOfs;

                // Make offsets relative to b
                int tmp = lastOfs;
                lastOfs = hint - ofs;
                ofs = hint - tmp;
            } // a[b + hint] <= key
            else
            {

                // a[b + hint] <= key
                // Gallop right until a[b+hint + lastOfs] <= key < a[b+hint + ofs]
                int maxOfs = len - hint;
                while (ofs < maxOfs && key >= a[@base + hint + ofs])
                {
                    lastOfs = ofs;
                    ofs = (ofs << 1) + 1;
                    if (ofs <= 0)
                        ofs = maxOfs;
                }

                if (ofs > maxOfs)
                    ofs = maxOfs;

                // Make offsets relative to b
                lastOfs += hint;
                ofs += hint;
            }

            /*
             * Now a[b + lastOfs] <= key < a[b + ofs], so key belongs somewhere to
             * the right of lastOfs but no farther right than ofs.  Do a binary
             * search, with invariant a[b + lastOfs - 1] <= key < a[b + ofs].
             */
            lastOfs++;
            while (lastOfs < ofs)
            {
                int m = lastOfs + ((ofs - lastOfs) >>> 1);
                if (key < a[@base + m])
                    ofs = m; // key < a[b + m]
                else
                    lastOfs = m + 1; // a[b + m] <= key
            }

            return ofs;
        }

        /// <summary>
        /// Merges two adjacent runs in place, in a stable fashion.  The first
        /// element of the first run must be greater than the first element of the
        /// second run (a[base1] > a[base2]), and the last element of the first run
        /// (a[base1 + len1-1]) must be greater than all elements of the second run.
        /// 
        /// For performance, this method should be called only when len1 <= len2;
        /// its twin, mergeHi should be called if len1 >= len2.  (Either method
        /// may be called if len1 == len2.)
        /// </summary>
        /// <param name="base1">index of first element in first run to be merged</param>
        /// <param name="len1">length of first run to be merged (must be > 0)</param>
        /// <param name="base2">index of first element in second run to be merged
        ///        (must be aBase + aLen)</param>
        /// <param name="len2">length of second run to be merged (must be > 0)</param>
        private void MergeLo(int base1, int len1, int base2, int len2)
        {

            // Copy first run into temp array
            int[] a = this.a; // For performance
            int[] b = this.b;
            int[] tmp = EnsureCapacity(len1);
            System.Arraycopy(a, base1, tmp, 0, len1);
            int[] tmpB = this.tmpB;
            System.Arraycopy(b, base1, tmpB, 0, len1);
            int cursor1 = 0; // Indices into tmp array
            int cursor2 = base2; // Indices int a
            int dest = base1; // Indices int a

            // Move first element of second run and deal with degenerate cases
            b[dest] = b[cursor2];
            a[dest++] = a[cursor2++];
            if (--len2 == 0)
            {
                System.Arraycopy(tmp, cursor1, a, dest, len1);
                System.Arraycopy(tmpB, cursor1, b, dest, len1);
                return;
            }

            if (len1 == 1)
            {
                System.Arraycopy(a, cursor2, a, dest, len2);
                System.Arraycopy(b, cursor2, b, dest, len2);
                a[dest + len2] = tmp[cursor1]; // Last elt of run 1 to end of merge
                b[dest + len2] = tmpB[cursor1];
                return;
            }

            int minGallop = this.minGallop; //  "    "       "     "      "
            outer:
                while (true)
                {
                    int count1 = 0; // Number of times in a row that first run won
                    int count2 = 0; // Number of times in a row that second run won
                    /*
                     * Do the straightforward thing until (if ever) one run starts
                     * winning consistently.
                     */
                    do
                    {
                        if (a[cursor2] < tmp[cursor1])
                        {
                            b[dest] = b[cursor2];
                            a[dest++] = a[cursor2++];
                            count2++;
                            count1 = 0;
                            if (--len2 == 0)
                                break;
                        }
                        else
                        {
                            b[dest] = tmpB[cursor1];
                            a[dest++] = tmp[cursor1++];
                            count1++;
                            count2 = 0;
                            if (--len1 == 1)
                                break;
                        }
                    }
                    while ((count1 | count2) < minGallop);
                    /*
                     * One run is winning so consistently that galloping may be a
                     * huge win. So try that, and continue galloping until (if ever)
                     * neither run appears to be winning consistently anymore.
                     */
                    do
                    {
                        count1 = GallopRight(a[cursor2], tmp, cursor1, len1, 0);
                        if (count1 != 0)
                        {
                            System.Arraycopy(tmp, cursor1, a, dest, count1);
                            System.Arraycopy(tmpB, cursor1, b, dest, count1);
                            dest += count1;
                            cursor1 += count1;
                            len1 -= count1;
                            if (len1 <= 1)
                                break;
                        }

                        b[dest] = b[cursor2];
                        a[dest++] = a[cursor2++];
                        if (--len2 == 0)
                            break;
                        count2 = GallopLeft(tmp[cursor1], a, cursor2, len2, 0);
                        if (count2 != 0)
                        {
                            System.Arraycopy(a, cursor2, a, dest, count2);
                            System.Arraycopy(b, cursor2, b, dest, count2);
                            dest += count2;
                            cursor2 += count2;
                            len2 -= count2;
                            if (len2 == 0)
                                break;
                        }

                        b[dest] = tmpB[cursor1];
                        a[dest++] = tmp[cursor1++];
                        if (--len1 == 1)
                            break;
                        minGallop--;
                    }
                    while (count1 >= MIN_GALLOP | count2 >= MIN_GALLOP);
                    if (minGallop < 0)
                        minGallop = 0;
                    minGallop += 2; // Penalize for leaving gallop mode
                } // End of "outer" loop

            this.minGallop = minGallop < 1 ? 1 : minGallop; // Write back to field
            if (len1 == 1)
            {
                System.Arraycopy(a, cursor2, a, dest, len2);
                System.Arraycopy(b, cursor2, b, dest, len2);
                a[dest + len2] = tmp[cursor1]; //  Last elt of run 1 to end of merge
                b[dest + len2] = tmpB[cursor1]; //  Last elt of run 1 to end of merge
            }
            else if (len1 == 0)
                throw new ArgumentException("Comparison method violates its general contract!");
            else
            {
                System.Arraycopy(tmp, cursor1, a, dest, len1);
                System.Arraycopy(tmpB, cursor1, b, dest, len1);
            }
        }

        /// <summary>
        /// Like mergeLo, except that this method should be called only if
        /// len1 >= len2; mergeLo should be called if len1 <= len2.  (Either method
        /// may be called if len1 == len2.)
        /// </summary>
        /// <param name="base1">index of first element in first run to be merged</param>
        /// <param name="len1">length of first run to be merged (must be > 0)</param>
        /// <param name="base2">index of first element in second run to be merged
        ///        (must be aBase + aLen)</param>
        /// <param name="len2">length of second run to be merged (must be > 0)</param>
        private void MergeHi(int base1, int len1, int base2, int len2)
        {

            // Copy second run into temp array
            int[] a = this.a; // For performance
            int[] b = this.b; // For performance
            int[] tmp = EnsureCapacity(len2);
            int[] tmpB = this.tmpB;
            System.Arraycopy(a, base2, tmp, 0, len2);
            System.Arraycopy(b, base2, tmpB, 0, len2);
            int cursor1 = base1 + len1 - 1; // Indices into a
            int cursor2 = len2 - 1; // Indices into tmp array
            int dest = base2 + len2 - 1; // Indices into a

            // Move last element of first run and deal with degenerate cases
            b[dest] = b[cursor1];
            a[dest--] = a[cursor1--];
            if (--len1 == 0)
            {
                System.Arraycopy(tmp, 0, a, dest - (len2 - 1), len2);
                System.Arraycopy(tmpB, 0, b, dest - (len2 - 1), len2);
                return;
            }

            if (len2 == 1)
            {
                dest -= len1;
                cursor1 -= len1;
                System.Arraycopy(a, cursor1 + 1, a, dest + 1, len1);
                System.Arraycopy(b, cursor1 + 1, b, dest + 1, len1);
                a[dest] = tmp[cursor2];
                b[dest] = tmpB[cursor2];
                return;
            }

            int minGallop = this.minGallop; //  "    "       "     "      "
            outer:
                while (true)
                {
                    int count1 = 0; // Number of times in a row that first run won
                    int count2 = 0; // Number of times in a row that second run won
                    /*
                     * Do the straightforward thing until (if ever) one run
                     * appears to win consistently.
                     */
                    do
                    {
                        if (tmp[cursor2] < a[cursor1])
                        {
                            b[dest] = b[cursor1];
                            a[dest--] = a[cursor1--];
                            count1++;
                            count2 = 0;
                            if (--len1 == 0)
                                break;
                        }
                        else
                        {
                            b[dest] = tmpB[cursor2];
                            a[dest--] = tmp[cursor2--];
                            count2++;
                            count1 = 0;
                            if (--len2 == 1)
                                break;
                        }
                    }
                    while ((count1 | count2) < minGallop);
                    /*
                     * One run is winning so consistently that galloping may be a
                     * huge win. So try that, and continue galloping until (if ever)
                     * neither run appears to be winning consistently anymore.
                     */
                    do
                    {
                        count1 = len1 - GallopRight(tmp[cursor2], a, base1, len1, len1 - 1);
                        if (count1 != 0)
                        {
                            dest -= count1;
                            cursor1 -= count1;
                            len1 -= count1;
                            System.Arraycopy(a, cursor1 + 1, a, dest + 1, count1);
                            System.Arraycopy(b, cursor1 + 1, b, dest + 1, count1);
                            if (len1 == 0)
                                break;
                        }

                        b[dest] = tmpB[cursor2];
                        a[dest--] = tmp[cursor2--];
                        if (--len2 == 1)
                            break;
                        count2 = len2 - GallopLeft(a[cursor1], tmp, 0, len2, len2 - 1);
                        if (count2 != 0)
                        {
                            dest -= count2;
                            cursor2 -= count2;
                            len2 -= count2;
                            System.Arraycopy(tmpB, cursor2 + 1, b, dest + 1, count2);
                            System.Arraycopy(tmp, cursor2 + 1, a, dest + 1, count2);
                            if (len2 <= 1)
                                break;
                        }

                        b[dest] = b[cursor1];
                        a[dest--] = a[cursor1--];
                        if (--len1 == 0)
                            break;
                        minGallop--;
                    }
                    while (count1 >= MIN_GALLOP | count2 >= MIN_GALLOP);
                    if (minGallop < 0)
                        minGallop = 0;
                    minGallop += 2; // Penalize for leaving gallop mode
                } // End of "outer" loop

            this.minGallop = minGallop < 1 ? 1 : minGallop; // Write back to field
            if (len2 == 1)
            {
                dest -= len1;
                cursor1 -= len1;
                System.Arraycopy(a, cursor1 + 1, a, dest + 1, len1);
                System.Arraycopy(b, cursor1 + 1, b, dest + 1, len1);
                a[dest] = tmp[cursor2]; // Move first elt of run2 to front of merge
                b[dest] = tmpB[cursor2]; // Move first elt of run2 to front of merge
            }
            else if (len2 == 0)
                throw new ArgumentException("Comparison method violates its general contract!");
            else
            {
                System.Arraycopy(tmp, 0, a, dest - (len2 - 1), len2);
                System.Arraycopy(tmpB, 0, b, dest - (len2 - 1), len2);
            }
        }

        /// <summary>
        /// Ensures that the external array tmp has at least the specified
        /// number of elements, increasing its size if necessary.  The size
        /// increases exponentially to ensure amortized linear time complexity.
        /// </summary>
        /// <param name="minCapacity">the minimum required capacity of the tmp array</param>
        /// <returns>tmp, whether or not it grew</returns>
        private int[] EnsureCapacity(int minCapacity)
        {
            if (tmp.Length < minCapacity)
            {

                // Compute smallest power of 2 > minCapacity
                int newSize = minCapacity;
                newSize |= newSize >> 1;
                newSize |= newSize >> 2;
                newSize |= newSize >> 4;
                newSize |= newSize >> 8;
                newSize |= newSize >> 16;
                newSize++;
                if (newSize < 0)
                    newSize = minCapacity;
                else
                    newSize = Math.Min(newSize, a.Length >>> 1);
                int[] newArray = new int[newSize];
                tmp = newArray;
                int[] newArrayB = new int[newSize];
                tmpB = newArrayB;
            }

            return tmp;
        }

        /// <summary>
        /// Checks that fromIndex and toIndex are in range, and throws an
        /// appropriate exception if they aren't.
        /// </summary>
        /// <param name="arrayLen">the length of the array</param>
        /// <param name="fromIndex">the index of the first element of the range</param>
        /// <param name="toIndex">the index after the last element of the range</param>
        /// <exception cref="IllegalArgumentException">if fromIndex > toIndex</exception>
        /// <exception cref="ArrayIndexOutOfBoundsException">if fromIndex < 0
        ///         or toIndex > arrayLen</exception>
        private static void RangeCheck(int arrayLen, int fromIndex, int toIndex)
        {
            if (fromIndex > toIndex)
                throw new ArgumentException("fromIndex(" + fromIndex + ") > toIndex(" + toIndex + ")");
            if (fromIndex < 0)
                throw new ArrayIndexOutOfBoundsException(fromIndex);
            if (toIndex > arrayLen)
                throw new ArrayIndexOutOfBoundsException(toIndex);
        }
    }
}