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
using Cc.Redberry.Rings.Bigint;
using Org.Apache.Commons.Math3.Random;
using Java.Lang.Reflect;
using Java.Util;
using Java.Util.Function;
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
    /// This class contains additional methods for manipulating arrays (such as sorting and searching). For all quick sort
    /// methods the base code was taken from jdk6 {@link Arrays} class.
    /// </summary>
    /// <remarks>
    /// @authorDmitry Bolotin
    /// @authorStanislav Poslavsky
    /// @seeArrays
    /// </remarks>
    public static class ArraysUtil
    {

        public static readonly Comparator<object> HASH_COMPARATOR = (o1, o2) => Integer.Compare(o1.GetHashCode(), o2.GetHashCode());
        /// <summary>
        /// Lexicographic order
        /// </summary>
        public static readonly Comparator<int[]> COMPARATOR = (int[] a, int[] b) =>
        {
            if (a.Length != b.Length)
                throw new ArgumentException();
            for (int i = 0; i < a.Length; ++i)
            {
                int c = Integer.Compare(a[i], b[i]);
                if (c != 0)
                    return c;
            }

            return 0;
        };
        /// <summary>
        /// Lexicographic order
        /// </summary>
        public static readonly Comparator<long[]> COMPARATOR_LONG = (long[] a, long[] b) =>
        {
            if (a.Length != b.Length)
                throw new ArgumentException();
            for (int i = 0; i < a.Length; ++i)
            {
                int c = Long.Compare(a[i], b[i]);
                if (c != 0)
                    return c;
            }

            return 0;
        };
        /// <summary>
        /// Lexicographic order
        /// </summary>
        public static readonly Comparator<Comparable[]> COMPARATOR_GENERIC = (Comparable[] a, Comparable[] b) =>
        {
            if (a.Length != b.Length)
                throw new ArgumentException();
            for (int i = 0; i < a.Length; ++i)
            {
                int c = a[i].CompareTo(b[i]);
                if (c != 0)
                    return c;
            }

            return 0;
        };
        public static int[] Flatten(int[, ] array)
        {
            int len = 0;
            foreach (int[] e in array)
                len += e.Length;
            int[] result = new int[len];
            int pointer = 0;
            foreach (int[] e in array)
            {
                System.Arraycopy(e, 0, result, pointer, e.Length);
                pointer += e.Length;
            }

            return result;
        }

        public static int[] ArrayOf(int val, int len)
        {
            int[] r = new int[len];
            Arrays.Fill(r, val);
            return r;
        }

        public static long[] ArrayOf(long val, int len)
        {
            long[] r = new long[len];
            Arrays.Fill(r, val);
            return r;
        }

        public static char[] ArrayOf(char val, int len)
        {
            char[] r = new char[len];
            Arrays.Fill(r, val);
            return r;
        }

        public static T[] ArrayOf<T>(T val, int len)
        {
            T[] r = (T[])Array.NewInstance(val.GetType(), len);
            Arrays.Fill(r, val);
            return r;
        }

        public static int[] Negate(int[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
                arr[i] = -arr[i];
            return arr;
        }

        public static long[] Negate(long[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
                arr[i] = -arr[i];
            return arr;
        }

        public static BigInteger[] Negate(BigInteger[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
                arr[i] = arr[i].Negate();
            return arr;
        }

        public static string ToString(long[] a, int from, int to)
        {
            if (a == null)
                return "null";
            int iMax = to - 1;
            if (iMax == -1)
                return "[]";
            StringBuilder b = new StringBuilder();
            b.Append('[');
            for (int i = from;; i++)
            {
                b.Append(a[i]);
                if (i == iMax)
                    return b.Append(']').ToString();
                b.Append(", ");
            }
        }

        public static string ToString<T>(T[] a, int from, int to)
        {
            if (a == null)
                return "null";
            int iMax = to - 1;
            if (iMax == -1)
                return "[]";
            StringBuilder b = new StringBuilder();
            b.Append('[');
            for (int i = from;; i++)
            {
                b.Append(a[i]);
                if (i == iMax)
                    return b.Append(']').ToString();
                b.Append(", ");
            }
        }

        public static string ToString<T>(T[] a, int from, int to, Function<T, string> stringer)
        {
            if (a == null)
                return "null";
            int iMax = to - 1;
            if (iMax == -1)
                return "[]";
            StringBuilder b = new StringBuilder();
            b.Append('[');
            for (int i = from;; i++)
            {
                b.Append(stringer.Apply(a[i]));
                if (i == iMax)
                    return b.Append(']').ToString();
                b.Append(", ");
            }
        }

        public static void Shuffle(int[] array, RandomGenerator rnd)
        {
            for (int i = 0; i < 2 * array.Length; ++i)
                Swap(array, rnd.NextInt(array.Length), rnd.NextInt(array.Length));
        }

        /// <summary>
        /// Sort array & return array with removed repetitive values.
        /// </summary>
        /// <param name="values">input array (this method will quickSort this array)</param>
        /// <returns>sorted array of distinct values</returns>
        public static int[] GetSortedDistinct(int[] values)
        {
            if (values.Length == 0)
                return values;
            Arrays.Sort(values);
            int shift = 0;
            int i = 0;
            while (i + shift + 1 < values.Length)
                if (values[i + shift] == values[i + shift + 1])
                    ++shift;
                else
                {
                    values[i] = values[i + shift];
                    ++i;
                }

            values[i] = values[i + shift];
            return Arrays.CopyOf(values, i + 1);
        }

        /// <summary>
        /// Sort array & return array with removed repetitive values.
        /// </summary>
        /// <param name="values">input array (this method will quickSort this array)</param>
        /// <returns>sorted array of distinct values</returns>
        public static BigInteger[] GetSortedDistinct(BigInteger[] values)
        {
            if (values.Length == 0)
                return values;
            Arrays.Sort(values);
            int shift = 0;
            int i = 0;
            while (i + shift + 1 < values.Length)
                if (values[i + shift].Equals(values[i + shift + 1]))
                    ++shift;
                else
                {
                    values[i] = values[i + shift];
                    ++i;
                }

            values[i] = values[i + shift];
            return Arrays.CopyOf(values, i + 1);
        }

        /// <summary>
        /// Sort array & return array with removed repetitive values.
        /// </summary>
        /// <param name="values">input array (this method will quickSort this array)</param>
        /// <returns>sorted array of distinct values</returns>
        public static long[] GetSortedDistinct(long[] values)
        {
            if (values.Length == 0)
                return values;
            Arrays.Sort(values);
            int shift = 0;
            int i = 0;
            while (i + shift + 1 < values.Length)
                if (values[i + shift] == values[i + shift + 1])
                    ++shift;
                else
                {
                    values[i] = values[i + shift];
                    ++i;
                }

            values[i] = values[i + shift];
            return Arrays.CopyOf(values, i + 1);
        }

        /// <summary>
        /// Return the set difference B - A for int sets A and B.<br/> Sets A and B must be represented as two sorted int
        /// arrays.<br/> Repetitive values in A or B not allowed.
        /// </summary>
        /// <param name="main">sorted array of distinct values (set B)</param>
        /// <param name="delete">sorted array of distinct values (set A)</param>
        /// <returns>the set of elements in B but not in A</returns>
        public static int[] IntSetDifference(int[] main, int[] delete)
        {
            int bPointer = 0, aPointer = 0;
            int counter = 0;
            while (aPointer < delete.Length && bPointer < main.Length)
                if (delete[aPointer] == main[bPointer])
                {
                    aPointer++;
                    bPointer++;
                }
                else if (delete[aPointer] < main[bPointer])
                    aPointer++;
                else if (delete[aPointer] > main[bPointer])
                {
                    counter++;
                    bPointer++;
                }

            counter += main.Length - bPointer;
            int[] result = new int[counter];
            counter = 0;
            aPointer = 0;
            bPointer = 0;
            while (aPointer < delete.Length && bPointer < main.Length)
                if (delete[aPointer] == main[bPointer])
                {
                    aPointer++;
                    bPointer++;
                }
                else if (delete[aPointer] < main[bPointer])
                    aPointer++;
                else if (delete[aPointer] > main[bPointer])
                    result[counter++] = main[bPointer++];
            System.Arraycopy(main, bPointer, result, counter, main.Length - bPointer);
            return result;
        }

        /// <summary>
        /// Return the union B + A for integer sets A and B.<br/> Sets A and B must be represented as two sorted integer
        /// arrays.<br/> Repetitive values in A or B not allowed.
        /// </summary>
        /// <param name="a">sorted array of distinct values. (set A)</param>
        /// <param name="b">sorted array of distinct values. (set B)</param>
        /// <returns>the set of elements from B and from A</returns>
        public static int[] IntSetUnion(int[] a, int[] b)
        {
            int bPointer = 0, aPointer = 0;
            int counter = 0;
            while (aPointer < a.Length && bPointer < b.Length)
                if (a[aPointer] == b[bPointer])
                {
                    aPointer++;
                    bPointer++;
                    counter++;
                }
                else if (a[aPointer] < b[bPointer])
                {
                    aPointer++;
                    counter++;
                }
                else if (a[aPointer] > b[bPointer])
                {
                    counter++;
                    bPointer++;
                }

            counter += (a.Length - aPointer) + (b.Length - bPointer); //Assert aPoiner==a.length || bPointer==b.length
            int[] result = new int[counter];
            counter = 0;
            aPointer = 0;
            bPointer = 0;
            while (aPointer < a.Length && bPointer < b.Length)
                if (a[aPointer] == b[bPointer])
                {
                    result[counter++] = b[bPointer];
                    aPointer++;
                    bPointer++;
                }
                else if (a[aPointer] < b[bPointer])
                    result[counter++] = a[aPointer++];
                else if (a[aPointer] > b[bPointer])
                    result[counter++] = b[bPointer++];
            if (aPointer == a.Length)
                System.Arraycopy(b, bPointer, result, counter, b.Length - bPointer);
            else
                System.Arraycopy(a, aPointer, result, counter, a.Length - aPointer);
            return result;
        }

        public static int[] Insert(int[] array, int position, int value)
        {
            int[] newArray = new int[array.Length + 1];
            System.Arraycopy(array, 0, newArray, 0, position);
            System.Arraycopy(array, position, newArray, position + 1, array.Length - position);
            newArray[position] = value;
            return newArray;
        }

        public static int[] Insert(int[] array, int position, int value, int length)
        {
            if (length == 0)
                return array.Clone();
            int[] newArray = new int[array.Length + length];
            System.Arraycopy(array, 0, newArray, 0, position);
            System.Arraycopy(array, position, newArray, position + length, array.Length - position);
            Arrays.Fill(newArray, position, position + length, value);
            return newArray;
        }

        public static long[] Insert(long[] array, int position, long value)
        {
            long[] newArray = new long[array.Length + 1];
            System.Arraycopy(array, 0, newArray, 0, position);
            System.Arraycopy(array, position, newArray, position + 1, array.Length - position);
            newArray[position] = value;
            return newArray;
        }

        public static T[] Insert<T>(T[] array, int position, T value)
        {
            T[] newArray = (T[])Array.NewInstance(value.GetType(), array.Length + 1);
            System.Arraycopy(array, 0, newArray, 0, position);
            System.Arraycopy(array, position, newArray, position + 1, array.Length - position);
            newArray[position] = value;
            return newArray;
        }

        public static void Reverse(int[] array, int from, int to)
        {
            for (int i = 0; i < (to - from) / 2; ++i)
                Swap(array, from + i, to - 1 - i);
        }

        public static void Reverse(long[] array, int from, int to)
        {
            for (int i = 0; i < (to - from) / 2; ++i)
                Swap(array, from + i, to - 1 - i);
        }

        public static void Reverse<T>(T[] array, int from, int to)
        {
            for (int i = 0; i < (to - from) / 2; ++i)
                Swap(array, from + i, to - 1 - i);
        }

        public static void Reverse<T>(T[] array)
        {
            Reverse(array, 0, array.Length);
        }

        public static void Reverse(int[] array)
        {
            Reverse(array, 0, array.Length);
        }

        public static void Reverse(long[] array)
        {
            Reverse(array, 0, array.Length);
        }

        public static int[] Short2int(short[] a)
        {
            int[] r = new int[a.Length];
            for (int i = 0; i < a.Length; ++i)
                r[i] = a[i];
            return r;
        }

        public static short[] Int2short(int[] a)
        {
            short[] r = new short[a.Length];
            for (int i = 0; i < a.Length; ++i)
                r[i] = (short)a[i];
            return r;
        }

        public static int[] Byte2int(byte[] a)
        {
            int[] r = new int[a.Length];
            for (int i = 0; i < a.Length; ++i)
                r[i] = a[i];
            return r;
        }

        public static short[] Byte2short(byte[] a)
        {
            short[] r = new short[a.Length];
            for (int i = 0; i < a.Length; ++i)
                r[i] = a[i];
            return r;
        }

        public static byte[] Int2byte(int[] a)
        {
            byte[] r = new byte[a.Length];
            for (int i = 0; i < a.Length; ++i)
                r[i] = (byte)a[i];
            return r;
        }

        public static int Max(int[] array)
        {
            int a = Integer.MIN_VALUE;
            foreach (int i in array)
                a = Math.Max(a, i);
            return a;
        }

        public static long Max(long[] array)
        {
            long a = Long.MIN_VALUE;
            foreach (long i in array)
                a = Math.Max(a, i);
            return a;
        }

        public static int Max(int[] array, int from, int to)
        {
            int a = Integer.MIN_VALUE;
            for (int i = from; i < to; i++)
                a = Math.Max(a, array[i]);
            return a;
        }

        public static int[] Max(int[] a, int[] b)
        {
            int[] r = new int[a.Length];
            for (int i = 0; i < a.Length; i++)
                r[i] = Math.Max(a[i], b[i]);
            return r;
        }

        public static int Min(int[] array)
        {
            int a = Integer.MAX_VALUE;
            foreach (int i in array)
                a = Math.Min(a, i);
            return a;
        }

        public static long Min(long[] array)
        {
            long a = Long.MAX_VALUE;
            foreach (long i in array)
                a = Math.Min(a, i);
            return a;
        }

        public static int Min(int[] array, int from, int to)
        {
            int a = Integer.MAX_VALUE;
            for (int i = from; i < to; i++)
                a = Math.Min(a, array[i]);
            return a;
        }

        public static int[] Min(int[] a, int[] b)
        {
            int[] r = new int[a.Length];
            for (int i = 0; i < a.Length; i++)
                r[i] = Math.Min(a[i], b[i]);
            return r;
        }

        public static int FirstIndexOf(int element, int[] array)
        {
            for (int i = 0; i < array.Length; i++)
                if (array[i] == element)
                    return i;
            return -1;
        }

        public static int FirstIndexOf(object element, object[] array)
        {
            for (int i = 0; i < array.Length; i++)
                if (element.Equals(array[i]))
                    return i;
            return -1;
        }

        public static int IndexOfMax(int[] array)
        {
            int index = 0, max = array[index];
            for (int i = 1; i < array.Length; i++)
                if (array[i] > max)
                {
                    max = array[i];
                    index = i;
                }

            return index;
        }

        public static int[] Sequence(int size)
        {
            return Sequence(0, size);
        }

        public static int[] Sequence(int from, int to)
        {
            int[] ret = new int[to - from];
            for (int i = ret.length - 1; i >= 0; --i)
                ret[i] = from + i;
            return ret;
        }

        public static int[][] DeepClone(int[, ] input)
        {
            int[, ] res = new int[input.Length];
            for (int i = res.length - 1; i >= 0; --i)
                res[i] = input[i].Clone();
            return res;
        }

        public static Object[][] DeepClone(object[, ] input)
        {
            object[, ] res = new object[input.Length];
            for (int i = res.length - 1; i >= 0; --i)
                res[i] = input[i].Clone();
            return res;
        }

        public static int Sum(int[] array)
        {
            return Sum(array, 0, array.Length);
        }

        public static int[] Sum(int[] a, int[] b)
        {
            int[] c = new int[a.Length];
            for (int i = 0; i < c.Length; i++)
                c[i] = a[i] + b[i];
            return c;
        }

        public static int[] Multiply(int[] a, int[] b)
        {
            int[] c = new int[a.Length];
            for (int i = 0; i < c.Length; i++)
                c[i] = a[i] * b[i];
            return c;
        }

        public static int[] Subtract(int[] a, int[] b)
        {
            int[] c = new int[a.Length];
            for (int i = 0; i < c.Length; i++)
                c[i] = a[i] - b[i];
            return c;
        }

        public static int Sum(int[] array, int from)
        {
            return Sum(array, from, array.Length);
        }

        public static int Sum(int[] array, int from, int to)
        {
            int s = 0;
            for (int i = from; i < to; ++i)
                s += array[i];
            return s;
        }

        public static int Multiply(int[] array, int from, int to)
        {
            int s = 1;
            for (int i = from; i < to; ++i)
                s *= array[i];
            return s;
        }

        public static double MultiplyToDouble(int[] array, int from, int to)
        {
            double s = 1;
            for (int i = from; i < to; ++i)
                s *= array[i];
            return s;
        }

        public static double MultiplyToDouble(int[] array)
        {
            return MultiplyToDouble(array, 0, array.Length);
        }

        public static double SumToDouble(int[] array, int from, int to)
        {
            double s = 0;
            for (int i = from; i < to; ++i)
                s += array[i];
            return s;
        }

        public static double SumToDouble(int[] array)
        {
            return SumToDouble(array, 0, array.Length);
        }

        public static int Or(long[] array)
        {
            return Or(array, 0, array.Length);
        }

        public static int Or(long[] array, int from)
        {
            return Or(array, from, array.Length);
        }

        public static int Or(long[] array, int from, int to)
        {
            int s = 0;
            for (int i = from; i < to; ++i)
                s |= array[i];
            return s;
        }

        /// <summary>
        /// This method is similar to {@link #bijection(Comparable[], Comparable[])}  }, but uses specified {@code
        /// comparator}.
        /// </summary>
        /// <param name="from">from array</param>
        /// <param name="to">to array</param>
        /// <param name="comparator">comparator</param>
        /// <returns>a bijective mapping from {@code from}-array to {@code to}-array and {@code null} if no mapping exist</returns>
        public static int[] Bijection<T>(T[] from, T[] to, Comparator<TWildcardTodoT> comparator)
        {

            //TODO refactor with sorting !!!!
            if (from.Length != to.Length)
                return null;
            int length = from.Length;
            int[] bijection = new int[length];
            Arrays.Fill(bijection, -1);
            int i, j;
            OUT:
                for (i = 0; i < length; ++i)
                {
                    for (j = 0; j < length; ++j)
                        if (bijection[j] == -1 && comparator.Compare(from[i], to[j]) == 0)
                        {
                            bijection[j] = i;
                            continue;
                        }

                    return null;
                }

            return bijection;
        }

        /// <summary>
        /// Creates a bijective mapping between two arrays and returns the resulting bijection as array. Method returns null,
        /// if no mapping found. <p/>
        /// <p>Example: <blockquote><pre>
        ///      Integer from[] = {1,2,1,4};
        ///      Integer to[] = {2,4,1,1};
        ///      int[] bijection = bijection(from,to);
        /// </pre></blockquote>
        /// <p/> <p> The resulting bijection will be {@code [2, 0, 3, 1]}
        /// </summary>
        /// <param name="from">from array</param>
        /// <param name="to">to array</param>
        /// <returns>a bijective mapping from {@code from}-array to {@code to}-array and {@code null} if no mapping exist</returns>
        public static int[] Bijection<T

        static extends Comparable<?
        static super T>>

        static (T[] from, T[] to)
        {

            //TODO refactor with sorting !!!!
            if (from.Length != to.Length)
                return null;
            int length = from.Length;
            int[] bijection = new int[length];
            Arrays.Fill(bijection, -1);
            int i, j;
            OUT:
                for (i = 0; i < length; ++i)
                {
                    for (j = 0; j < length; ++j)
                        if (bijection[j] == -1 && from[i].CompareTo(to[j]) == 0)
                        {
                            bijection[j] = i;
                            continue;
                        }

                    return null;
                }

            return bijection;
        }

        public static int[] AddAll(int[] array1, params int[] array2)
        {
            int[] r = new int[array1.Length + array2.Length];
            System.Arraycopy(array1, 0, r, 0, array1.Length);
            System.Arraycopy(array2, 0, r, array1.Length, array2.Length);
            return r;
        }

        public static long[] AddAll(long[] array1, params long[] array2)
        {
            long[] r = new long[array1.Length + array2.Length];
            System.Arraycopy(array1, 0, r, 0, array1.Length);
            System.Arraycopy(array2, 0, r, array1.Length, array2.Length);
            return r;
        }

        public static int[] AddAll(params int[] arrays)
        {
            if (arrays.Length == 0)
                return new int[0];
            int i, length = 0;
            for (i = 0; i < arrays.Length; ++i)
                length += arrays[i].Length;
            if (length == 0)
                return new int[0];
            int[] r = new int[length];
            int pointer = 0;
            for (i = 0; i < arrays.Length; ++i)
            {
                System.Arraycopy(arrays[i], 0, r, pointer, arrays[i].Length);
                pointer += arrays[i].Length;
            }

            return r;
        }

        public static int[] Remove(int[] array, int i)
        {
            if (i >= array.Length)
                throw new ArrayIndexOutOfBoundsException();
            if (array.Length == 1)
            {
                return new int[0];
            }
            else if (array.Length == 2)
                return new int[]
                {
                    array[1 ^ i]
                };
            int[] newArray = new int[array.Length - 1];
            System.Arraycopy(array, 0, newArray, 0, i);
            if (i != array.Length - 1)
                System.Arraycopy(array, i + 1, newArray, i, array.Length - i - 1);
            return newArray;
        }

        public static long[] Remove(long[] array, int i)
        {
            if (i >= array.Length)
                throw new ArrayIndexOutOfBoundsException();
            if (array.Length == 1)
            {
                return new long[0];
            }
            else if (array.Length == 2)
                return new long[]
                {
                    array[1 ^ i]
                };
            long[] newArray = new long[array.Length - 1];
            System.Arraycopy(array, 0, newArray, 0, i);
            if (i != array.Length - 1)
                System.Arraycopy(array, i + 1, newArray, i, array.Length - i - 1);
            return newArray;
        }

        public static T[] Remove<T>(T[] array, int i)
        {
            T[] r = (T[])Array.NewInstance(array.GetType().GetComponentType(), array.Length - 1);
            System.Arraycopy(array, 0, r, 0, i);
            if (i < array.Length - 1)
                System.Arraycopy(array, i + 1, r, i, array.Length - i - 1);
            return r;
        }

        /// <summary>
        /// This code is taken from Apache Commons Lang ArrayUtils. <p/> <p>Adds all the elements of the given arrays into a
        /// new array.</p> <p>The new array contains all of the element of {@code array1} followed by all of the elements
        /// {@code array2}. When an array is returned, it is always a new array.</p> <p/>
        /// <pre>
        /// ArrayUtils.addAll(null, null)     = null
        /// ArrayUtils.addAll(array1, null)   = cloned copy of array1
        /// ArrayUtils.addAll(null, array2)   = cloned copy of array2
        /// ArrayUtils.addAll([], [])         = []
        /// ArrayUtils.addAll([null], [null]) = [null, null]
        /// ArrayUtils.addAll(["a", "b", "c"], ["1", "2", "3"]) = ["a", "b", "c", "1", "2", "3"]
        /// </pre>
        /// </summary>
        /// <param name="<T>">the component type of the array</param>
        /// <param name="array1">the first array whose elements are added to the new array, may be {@code null}</param>
        /// <param name="array2">the second array whose elements are added to the new array, may be {@code null}</param>
        /// <returns>The new array, {@code null} if both arrays are {@code null}. The type of the new array is the type of the
        ///         first array, unless the first array is null, in which case the type is the same as the second array.</returns>
        /// <exception cref="IllegalArgumentException">if the array types are incompatible</exception>
        /// <remarks>@since2.1</remarks>
        public static T[] AddAll<T>(T[] array1, params T[] array2)
        {
            if (array1 == null)
                return array2.Clone();
            else if (array2 == null)
                return array1.Clone();
            Class<TWildcardTodo> type1 = array1.GetType().GetComponentType();
            T[] joinedArray = (T[])Array.NewInstance(type1, array1.Length + array2.Length);
            System.Arraycopy(array1, 0, joinedArray, 0, array1.Length);
            try
            {
                System.Arraycopy(array2, 0, joinedArray, array1.Length, array2.Length);
            }
            catch (ArrayStoreException ase)
            {

                // Check if problem was due to incompatible types
                /*
                 * We do this here, rather than before the copy because: - it would
                 * be a wasted check most of the time - safer, in case check turns
                 * out to be too strict
                 */
                Class<TWildcardTodo> type2 = array2.GetType().GetComponentType();
                if (!type1.IsAssignableFrom(type2))
                    throw new ArgumentException("Cannot store " + type2.GetName() + " in an array of " + type1.GetName(), ase);
                throw ase; // No, so rethrow original
            }

            return joinedArray;
        }

        /// <summary>
        /// Removes elements at specified {@code positions} in specified {@code array}. This method preserve the relative
        /// order of elements in specified {@code array}.
        /// </summary>
        /// <param name="array">array of elements</param>
        /// <param name="positions">positions of elements that should be removed</param>
        /// <param name="<T>">generic type</param>
        /// <returns>new array with removed elements at specified positions</returns>
        /// <exception cref="ArrayIndexOutOfBoundsException">if some position larger then array length</exception>
        public static T[] Remove<T>(T[] array, int[] positions)
        {
            if (array == null)
                throw new NullReferenceException();
            int[] p = GetSortedDistinct(positions);
            if (p.Length == 0)
                return array;
            int size = p.Length, pointer = 0, s = array.Length;
            for (; pointer < size; ++pointer)
                if (p[pointer] >= s)
                    throw new ArrayIndexOutOfBoundsException();
            Class<TWildcardTodo> type = array.GetType().GetComponentType();
            T[] r = (T[])Array.NewInstance(type, array.Length - p.Length);
            pointer = 0;
            int i = -1;
            for (int j = 0; j < s; ++j)
            {
                if (pointer < size - 1 && j > p[pointer])
                    ++pointer;
                if (j == p[pointer])
                    continue;
                else
                    r[++i] = array[j];
            }

            return r;
        }

        /// <summary>
        /// Removes elements at specified {@code positions} in specified {@code array}. This method preserve the relative
        /// order of elements in specified {@code array}.
        /// </summary>
        /// <param name="array">array of elements</param>
        /// <param name="positions">positions of elements that should be removed</param>
        /// <returns>new array with removed elements at specified positions</returns>
        /// <exception cref="ArrayIndexOutOfBoundsException">if some position larger then array length</exception>
        public static int[] Remove(int[] array, int[] positions)
        {
            if (array == null)
                throw new NullReferenceException();
            int[] p = GetSortedDistinct(positions.Clone());
            if (p.Length == 0)
                return array;
            int size = p.Length, pointer = 0, s = array.Length;
            for (; pointer < size; ++pointer)
                if (p[pointer] >= s)
                    throw new ArrayIndexOutOfBoundsException();
            int[] r = new int[array.Length - p.Length];
            pointer = 0;
            int i = -1;
            for (int j = 0; j < s; ++j)
            {
                if (pointer < size - 1 && j > p[pointer])
                    ++pointer;
                if (j == p[pointer])
                    continue;
                else
                    r[++i] = array[j];
            }

            return r;
        }

        /// <summary>
        /// Removes elements at specified {@code positions} in specified {@code array}. This method preserve the relative
        /// order of elements in specified {@code array}.
        /// </summary>
        /// <param name="array">array of elements</param>
        /// <param name="positions">positions of elements that should be removed</param>
        /// <returns>new array with removed elements at specified positions</returns>
        /// <exception cref="ArrayIndexOutOfBoundsException">if some position larger then array length</exception>
        public static long[] Remove(long[] array, int[] positions)
        {
            if (array == null)
                throw new NullReferenceException();
            int[] p = GetSortedDistinct(positions.Clone());
            if (p.Length == 0)
                return array;
            int size = p.Length, pointer = 0, s = array.Length;
            for (; pointer < size; ++pointer)
                if (p[pointer] >= s)
                    throw new ArrayIndexOutOfBoundsException();
            long[] r = new long[array.Length - p.Length];
            pointer = 0;
            int i = -1;
            for (int j = 0; j < s; ++j)
            {
                if (pointer < size - 1 && j > p[pointer])
                    ++pointer;
                if (j == p[pointer])
                    continue;
                else
                    r[++i] = array[j];
            }

            return r;
        }

        /// <summary>
        /// Selects elements from specified {@code array} at specified {@code positions}. The resulting array preserves the
        /// relative order of elements in specified {@code array}.
        /// </summary>
        /// <param name="array">array of elements</param>
        /// <param name="positions">of elements that should be picked out</param>
        /// <param name="<T>">generic type</param>
        /// <returns>the array of elements that picked out from specified positions in specified array</returns>
        public static T[] Select<T>(T[] array, int[] positions)
        {
            if (array == null)
                throw new NullReferenceException();
            int[] p = GetSortedDistinct(positions);
            Class<TWildcardTodo> type = array.GetType().GetComponentType();
            T[] r = (T[])Array.NewInstance(type, p.Length);
            int i = -1;
            foreach (int j in p)
                r[++i] = array[j];
            return r;
        }

        /// <summary>
        /// Selects elements from specified {@code array} at specified {@code positions}. The resulting array preserves the
        /// relative order of elements in specified {@code array}.
        /// </summary>
        /// <param name="array">array of elements</param>
        /// <param name="positions">of elements that should be picked out</param>
        /// <returns>the array of elements that picked out from specified positions in specified array</returns>
        public static int[] Select(int[] array, int[] positions)
        {
            if (array == null)
                throw new NullReferenceException();
            int[] p = GetSortedDistinct(positions);
            int[] r = new int[p.Length];
            int i = -1;
            foreach (int j in p)
                r[++i] = array[j];
            return r;
        }

        /// <summary>
        /// Converts {@code Set<Integer>} to {@code int[]}
        /// </summary>
        /// <param name="set">a {@link Set} of {@link Integer}</param>
        /// <returns>{@code int[]}</returns>
        public static int[] ToArray(HashSet<int> set)
        {
            int i = -1;
            int[] a = new int[set.Count];
            foreach (int ii in set)
                a[++i] = ii;
            return a;
        }

        /// <summary>
        /// This is the same method to {@link Arrays#binarySearch(int[], int) }. The differs is in the returned value. If key
        /// not found, this method returns the position of the first element, witch is closest to key (i.e. if
        /// Arrays.binarySearch returns {@code -low-1}, this method returns {@code low}).
        /// </summary>
        /// <param name="a">the array to be searched</param>
        /// <param name="key">the value to be searched for</param>
        /// <returns>index of the search key, if it is contained in the array; otherwise, <tt><i>insertion point</i></tt>.
        ///         The
        ///         <i>insertion point</i> is defined as the point at which the key would be inserted into the array: the
        ///         index of the first element greater than the key, or <tt>a.length</tt> if all elements in the array are
        ///         less than the specified key.</returns>
        public static int BinarySearch1(int[] a, int key)
        {
            return BinarySearch1(a, 0, a.Length, key);
        }

        /// <summary>
        /// This is the same method to {@link Arrays#binarySearch(int[], int, int, int) }. The differs is in the returned
        /// value. If key not found, this method returns the position of the first element, witch is closest to key (i.e. if
        /// Arrays.binarySearch returns {@code -low-1}, this method returns {@code low}).
        /// </summary>
        /// <param name="a">the array to be searched</param>
        /// <param name="key">the value to be searched for</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be searched</param>
        /// <param name="toIndex">the index of the last element (exclusive) to be searched</param>
        /// <returns>index of the search key, if it is contained in the array; otherwise, <tt><i>insertion point</i></tt>.
        ///         The
        ///         <i>insertion point</i> is defined as the point at which the key would be inserted into the array: the
        ///         index of the first element greater than the key, or <tt>toIndex</tt> if all elements in the array are
        ///         less than the specified key.</returns>
        public static int BinarySearch1(int[] a, int fromIndex, int toIndex, int key)
        {
            int low = fromIndex;
            int high = toIndex - 1;
            while (low <= high)
            {
                int mid = (low + high) >>> 1;
                int midVal = a[mid];
                if (midVal < key)
                    low = mid + 1;
                else if (midVal > key)
                    high = mid - 1;
                else
                {
                    while (mid > 0 && a[mid - 1] == a[mid])
                        --mid;
                    return mid; // key found
                }
            }

            if (low >= a.Length)
                return low;
            while (low > 0 && a[low - 1] == a[low])
                --low;
            return low; // key not found.
        }

        /// <summary>
        /// Returns commutative hash code of the data
        /// </summary>
        /// <param name="data">array</param>
        /// <returns>commutative hash</returns>
        public static int CommutativeHashCode<T>(T[] data)
        {
            return CommutativeHashCode(data, 0, data.Length);
        }

        /// <summary>
        /// Returns commutative hash code of the data
        /// </summary>
        /// <param name="data">array</param>
        /// <returns>commutative hash</returns>
        public static int CommutativeHashCode<T>(T[] data, int from, int to)
        {
            int r = 17;
            for (int i = from; i < to; i++)
            {
                int h = data[i].GetHashCode();
                r *= h + 29 ^ h;
            }

            return r;
        }

        /// <summary>
        /// Returns commutative hash code of the data
        /// </summary>
        /// <param name="data">array</param>
        /// <returns>commutative hash</returns>
        public static int CommutativeHashCode(int[] data)
        {
            return CommutativeHashCode(data, 0, data.Length);
        }

        /// <summary>
        /// Returns commutative hash code of the data
        /// </summary>
        /// <param name="data">array</param>
        /// <returns>commutative hash</returns>
        public static int CommutativeHashCode(int[] data, int from, int to)
        {
            int r = 17;
            for (int i = from; i < to; i++)
                r *= data[i] + 29 ^ data[i];
            return r;
        }

        /// <summary>
        /// Sorts the specified array of ints into ascending order using insertion sort algorithm and simultaneously permutes
        /// the {@code coSort} ints array in the same way as the target array. This sort guarantee O(n^2) performance in the
        /// worst case and O(n) in the best case (nearly sorted input). <p/> <p> This sort is the best choice for small
        /// arrays with elements number < 100. <p/> <p>This sort is guaranteed to be <i>stable</i>: equal elements will not
        /// be reordered as a result of the sort; <i>adaptive</i>: performance adapts to the initial order of elements and
        /// <i>in-place</i>: requires constant amount of additional space.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if coSort length less then target length.</exception>
        public static void InsertionSort(int[] target, int[] coSort)
        {
            InsertionSort(target, 0, target.Length, coSort);
        }

        /// <summary>
        /// Sorts the specified array of ints into ascending order using insertion sort algorithm and simultaneously permutes
        /// the {@code coSort} ints array in the same way as the target array. This sort guarantee O(n^2) performance in the
        /// worst case and O(n) in the best case (nearly sorted input). The range to be sorted extends from index
        /// <tt>fromIndex</tt>, inclusive, to index <tt>toIndex</tt>, exclusive. (If <tt>fromIndex==toIndex</tt>, the range
        /// to be sorted is empty.) <p/> <p> This sort is the best choice for small arrays with elements number < 100. <p/>
        /// <p>This sort is guaranteed to be <i>stable</i>: equal elements will not be reordered as a result of the sort;
        /// <i>adaptive</i>: performance adapts to the initial order of elements and <i>in-place</i>: requires constant
        /// amount of additional space.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="toIndex">the index of the last element (exclusive) to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array, during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if <tt>fromIndex &gt; toIndex</tt></exception>
        /// <exception cref="ArrayIndexOutOfBoundsException">if <tt>fromIndex &lt; 0</tt> or <tt>toIndex &gt; target.length</tt> or
        ///                                        <tt>toIndex &gt; coSort.length</tt></exception>
        public static void InsertionSort(int[] target, int fromIndex, int toIndex, int[] coSort)
        {
            RangeCheck(target.Length, fromIndex, toIndex);
            RangeCheck(coSort.Length, fromIndex, toIndex);
            int i, key, j, keyC;
            for (i = fromIndex + 1; i < toIndex; i++)
            {
                key = target[i];
                keyC = coSort[i];
                for (j = i; j > fromIndex && target[j - 1] > key; j--)
                {
                    target[j] = target[j - 1];
                    coSort[j] = coSort[j - 1];
                }

                target[j] = key;
                coSort[j] = keyC;
            }
        }

        /// <summary>
        /// Sorts the specified array of ints into ascending order using insertion sort algorithm and simultaneously permutes
        /// the {@code coSort} longs array in the same way as the specified target array. This sort guarantee O(n^2)
        /// performance in the worst case and O(n) in the best case (nearly sorted input). <p/> <p> This sort is the best
        /// choice for small arrays with elements number < 100. <p/> <p>This sort is guaranteed to be <i>stable</i>: equal
        /// elements will not be reordered as a result of the sort; <i>adaptive</i>: performance adapts to the initial order
        /// of elements and <i>in-place</i>: requires constant amount of additional space.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if coSort length less then target length.</exception>
        public static void InsertionSort(int[] target, long[] coSort)
        {
            InsertionSort(target, 0, target.Length, coSort);
        }

        /// <summary>
        /// Sorts the specified array of ints into ascending order using insertion sort algorithm and simultaneously permutes
        /// the {@code coSort} ints array in the same way as the target array. This sort guarantee O(n^2) performance in the
        /// worst case and O(n) in the best case (nearly sorted input). The range to be sorted extends from index
        /// <tt>fromIndex</tt>, inclusive, to index <tt>toIndex</tt>, exclusive. (If <tt>fromIndex==toIndex</tt>, the range
        /// to be sorted is empty.) <p/> <p> This sort is the best choice for small arrays with elements number < 100. <p/>
        /// <p>This sort is guaranteed to be <i>stable</i>: equal elements will not be reordered as a result of the sort;
        /// <i>adaptive</i>: performance adapts to the initial order of elements and <i>in-place</i>: requires constant
        /// amount of additional space.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="toIndex">the index of the last element (exclusive) to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the specified target array, during sorting
        ///                  procedure</param>
        /// <exception cref="IllegalArgumentException">if <tt>fromIndex &gt; toIndex</tt></exception>
        /// <exception cref="ArrayIndexOutOfBoundsException">if <tt>fromIndex &lt; 0</tt> or <tt>toIndex &gt; target.length</tt> or
        ///                                        <tt>toIndex &gt; coSort.length</tt></exception>
        public static void InsertionSort(int[] target, int fromIndex, int toIndex, long[] coSort)
        {
            RangeCheck(target.Length, fromIndex, toIndex);
            RangeCheck(coSort.Length, fromIndex, toIndex);
            int i, key, j;
            long keyC;
            for (i = fromIndex + 1; i < toIndex; i++)
            {
                key = target[i];
                keyC = coSort[i];
                for (j = i; j > fromIndex && target[j - 1] > key; j--)
                {
                    target[j] = target[j - 1];
                    coSort[j] = coSort[j - 1];
                }

                target[j] = key;
                coSort[j] = keyC;
            }
        }

        /// <summary>
        /// Sorts the specified target array of objects into ascending order, according to the natural ordering of its
        /// elements using insertion sort algorithm and simultaneously permutes the {@code coSort} objects array in the same
        /// way then specified target array. This sort guarantee O(n^2) performance in the worst case and O(n) in the best
        /// case (nearly sorted input). <p/> <p> This sort is the best choice for small arrays with elements number < 100.
        /// <p/> <p>This sort is guaranteed to be <i>stable</i>: equal elements will not be reordered as a result of the
        /// sort; <i>adaptive</i>: performance adapts to the initial order of elements and <i>in-place</i>: requires constant
        /// amount of additional space.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if coSort length less then target length.</exception>
        public static void InsertionSort<T

        static extends Comparable<T>>(T[] target, object[] coSort)
        {
            InsertionSort(target, 0, target.Length, coSort);
        }

        /// <summary>
        /// Sorts the specified target array of objects into ascending order, according to the natural ordering of its
        /// elements using insertion sort algorithm and simultaneously permutes the {@code coSort} objects array in the same
        /// way then specified target array. This sort guarantee O(n^2) performance in the worst case and O(n) in the best
        /// case (nearly sorted input). The range to be sorted extends from index <tt>fromIndex</tt>, inclusive, to index
        /// <tt>toIndex</tt>, exclusive. (If <tt>fromIndex==toIndex</tt>, the range to be sorted is empty.) <p/> <p> This
        /// sort is the best choice for small arrays with elements number < 100. <p/> <p>This sort is guaranteed to be
        /// <i>stable</i>: equal elements will not be reordered as a result of the sort; <i>adaptive</i>: performance adapts
        /// to the initial order of elements and <i>in-place</i>: requires constant amount of additional space.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="toIndex">the index of the last element (exclusive) to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array, during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if <tt>fromIndex &gt; toIndex</tt></exception>
        /// <exception cref="ArrayIndexOutOfBoundsException">if <tt>fromIndex &lt; 0</tt> or <tt>toIndex &gt; target.length</tt> or
        ///                                        <tt>toIndex &gt; coSort.length</tt></exception>
        public static void InsertionSort<T

        static extends Comparable<T>>(T[] target, int fromIndex, int toIndex, object[] coSort)
        {
            int i, j;
            T key;
            object keyC;
            for (i = fromIndex + 1; i < toIndex; i++)
            {
                key = target[i];
                keyC = coSort[i];
                for (j = i; j > fromIndex && target[j - 1].CompareTo(key) > 0; j--)
                {
                    target[j] = target[j - 1];
                    coSort[j] = coSort[j - 1];
                }

                target[j] = key;
                coSort[j] = keyC;
            }
        }

        /// <summary>
        /// Sorts the specified target array of objects into ascending order, according to the natural ordering of its
        /// elements using insertion sort algorithm and simultaneously permutes the {@code coSort} objects array in the same
        /// way then specified target array. This sort guarantee O(n^2) performance in the worst case and O(n) in the best
        /// case (nearly sorted input). <p/> <p> This sort is the best choice for small arrays with elements number < 100.
        /// <p/> <p>This sort is guaranteed to be <i>stable</i>: equal elements will not be reordered as a result of the
        /// sort; <i>adaptive</i>: performance adapts to the initial order of elements and <i>in-place</i>: requires constant
        /// amount of additional space.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if coSort length less then target length.</exception>
        public static void InsertionSort<T

        static extends Comparable<T>>(T[] target, int[] coSort)
        {
            InsertionSort(target, 0, target.Length, coSort);
        }

        /// <summary>
        /// Sorts the specified target array of objects into ascending order, according to the natural ordering of its
        /// elements using insertion sort algorithm and simultaneously permutes the {@code coSort} objects array in the same
        /// way then specified target array. This sort guarantee O(n^2) performance in the worst case and O(n) in the best
        /// case (nearly sorted input). The range to be sorted extends from index <tt>fromIndex</tt>, inclusive, to index
        /// <tt>toIndex</tt>, exclusive. (If <tt>fromIndex==toIndex</tt>, the range to be sorted is empty.) <p/> <p> This
        /// sort is the best choice for small arrays with elements number < 100. <p/> <p>This sort is guaranteed to be
        /// <i>stable</i>: equal elements will not be reordered as a result of the sort; <i>adaptive</i>: performance adapts
        /// to the initial order of elements and <i>in-place</i>: requires constant amount of additional space.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="toIndex">the index of the last element (exclusive) to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array, during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if <tt>fromIndex &gt; toIndex</tt></exception>
        /// <exception cref="ArrayIndexOutOfBoundsException">if <tt>fromIndex &lt; 0</tt> or <tt>toIndex &gt; target.length</tt> or
        ///                                        <tt>toIndex &gt; coSort.length</tt></exception>
        public static void InsertionSort<T

        static extends Comparable<T>>(T[] target, int fromIndex, int toIndex, int[] coSort)
        {
            InsertionSort(target, fromIndex, toIndex, coSort, T.CompareTo());
        }

        /// <summary>
        /// Sorts the specified target array of objects into ascending order, according to the natural ordering of its
        /// elements using insertion sort algorithm and simultaneously permutes the {@code coSort} objects array in the same
        /// way then specified target array. This sort guarantee O(n^2) performance in the worst case and O(n) in the best
        /// case (nearly sorted input). <p/> <p> This sort is the best choice for small arrays with elements number < 100.
        /// <p/> <p>This sort is guaranteed to be <i>stable</i>: equal elements will not be reordered as a result of the
        /// sort; <i>adaptive</i>: performance adapts to the initial order of elements and <i>in-place</i>: requires constant
        /// amount of additional space.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if coSort length less then target length.</exception>
        public static void InsertionSort<T>(T[] target, int[] coSort, Comparator<T> comparator)
        {
            InsertionSort(target, 0, target.Length, coSort, comparator);
        }

        /// <summary>
        /// Sorts the specified target array of objects into ascending order, according to the natural ordering of its
        /// elements using insertion sort algorithm and simultaneously permutes the {@code coSort} objects array in the same
        /// way then specified target array. This sort guarantee O(n^2) performance in the worst case and O(n) in the best
        /// case (nearly sorted input). The range to be sorted extends from index <tt>fromIndex</tt>, inclusive, to index
        /// <tt>toIndex</tt>, exclusive. (If <tt>fromIndex==toIndex</tt>, the range to be sorted is empty.) <p/> <p> This
        /// sort is the best choice for small arrays with elements number < 100. <p/> <p>This sort is guaranteed to be
        /// <i>stable</i>: equal elements will not be reordered as a result of the sort; <i>adaptive</i>: performance adapts
        /// to the initial order of elements and <i>in-place</i>: requires constant amount of additional space.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="toIndex">the index of the last element (exclusive) to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array, during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if <tt>fromIndex &gt; toIndex</tt></exception>
        /// <exception cref="ArrayIndexOutOfBoundsException">if <tt>fromIndex &lt; 0</tt> or <tt>toIndex &gt; target.length</tt> or
        ///                                        <tt>toIndex &gt; coSort.length</tt></exception>
        public static void InsertionSort<T>(T[] target, int fromIndex, int toIndex, int[] coSort, Comparator<T> comparator)
        {
            int i, j;
            T key;
            int keyC;
            for (i = fromIndex + 1; i < toIndex; i++)
            {
                key = target[i];
                keyC = coSort[i];
                for (j = i; j > fromIndex && comparator.Compare(target[j - 1], key) > 0; j--)
                {
                    target[j] = target[j - 1];
                    coSort[j] = coSort[j - 1];
                }

                target[j] = key;
                coSort[j] = keyC;
            }
        }

        /// <summary>
        /// Sorts the specified array of ints into ascending order using TimSort algorithm and simultaneously permutes the
        /// {@code coSort} ints array in the same way as the target array. <p/> <p> NOTE: using of this method is very good
        /// for large arrays with more then 100 elements, in other case using of insertion sort is highly recommended. <p/>
        /// <p>This sort is guaranteed to be <i>stable</i>: equal elements will not be reordered as a result of the sort.
        /// <p/> <p> The code was taken from {@link Arrays#sort(java.lang.Object[]) } and adapted for integers. For more
        /// information look there.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array during sorting procedure</param>
        /// <exception cref="ClassCastException">if the array contains elements that are not <i>mutually comparable</i> (for example,
        ///                            strings and integers)</exception>
        /// <remarks>@seeArrays#sort(java.lang.Object[])</remarks>
        public static void TimSort(int[] target, int[] coSort)
        {
            IntTimSort.Sort(target, coSort);
        }

        /// <summary>
        /// Sorts the specified array of ints into ascending order using stable sort algorithm and simultaneously permutes
        /// the {@code coSort} ints array in the same way as the target array. If length of specified array is less than 100
        /// - insertion sort algorithm performed, otherwise - TimSort. <p/> <p>This sort is guaranteed to be <i>stable</i>:
        /// equal elements will not be reordered as a result of the sort.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="cosort">the array which will be permuted in the same way as the target array during sorting procedure</param>
        /// <exception cref="ClassCastException">if the array contains elements that are not <i>mutually comparable</i> (for
        ///                                  example, strings and integers)</exception>
        /// <exception cref="IllegalArgumentException">if coSort length less then target length.</exception>
        /// <exception cref="IllegalArgumentException">if target == coSort (as references).</exception>
        /// <remarks>
        /// @see#insertionSort(int[], int[])
        /// @see#timSort(int[], int[])
        /// </remarks>
        public static void StableSort(int[] target, int[] cosort)
        {
            if (target.Length > 100)
                ArraysUtil.TimSort(target, cosort);
            else
                ArraysUtil.InsertionSort(target, cosort);
        }

        /// <summary>
        /// Sorts the specified array and returns the resulting permutation
        /// </summary>
        /// <param name="target">int array</param>
        /// <returns>sorting permutation</returns>
        public static int[] QuickSortP(int[] target)
        {
            int[] permutation = new int[target.Length];
            for (int i = 1; i < target.Length; ++i)
                permutation[i] = i;
            QuickSort(target, 0, target.Length, permutation);
            return permutation;
        }

        // =================  QUICKSORT INT[] INT[] =================
        /// <summary>
        /// Sorts the specified target array of ints into ascending numerical order and simultaneously permutes the {@code
        /// coSort} ints array in the same way as the target array. <p/> The code was taken from the jdk6 Arrays class. <p/>
        /// The sorting algorithm is a tuned quicksort, adapted from Jon L. Bentley and M. Douglas McIlroy's "Engineering a
        /// Sort Function", Software-Practice and Experience, Vol. 23(11) P. 1249-1265 (November 1993). This algorithm offers
        /// n*log(n) performance on many data sets that cause other quicksorts to degrade to quadratic performance. <p/>
        /// <p><b>NOTE: remember this is unstable sort algorithm, so additional combinatorics of the {@code coSort} array
        /// can
        /// be perfomed. Use this method only if you are sure, in what you are doing. If not - use stable sort methods like
        /// an insertion sort or Tim sort.</b> <p/> <p><b>NOTE:</b> The method throws {@code IllegalArgumentException} if
        /// {@code target == coSort}, because in this case no sorting will be perfomed.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if coSort length less then target length.</exception>
        /// <exception cref="IllegalArgumentException">if target == coSort (as references).</exception>
        public static void QuickSort(int[] target, int[] coSort)
        {
            QuickSort(target, 0, target.Length, coSort);
        }

        /// <summary>
        /// Sorts the specified range of the specified target array of ints into ascending numerical order and simultaneously
        /// permutes the {@code coSort} ints array in the same way as the target array. The range to be sorted extends from
        /// index <tt>fromIndex</tt>, inclusive, to index <tt>toIndex</tt>, exclusive. (If <tt>fromIndex==toIndex</tt>, the
        /// range to be sorted is empty.)<p> <p/> The code was taken from the jdk6 Arrays class. <p/> The sorting algorithm
        /// is a tuned quicksort, adapted from Jon L. Bentley and M. Douglas McIlroy's "Engineering a Sort Function",
        /// Software-Practice and Experience, Vol. 23(11) P. 1249-1265 (November 1993). This algorithm offers n*log(n)
        /// performance on many data sets that cause other quicksorts to degrade to quadratic performance. <p/> <p><b>NOTE:
        /// remember this is unstable sort algorithm, so additional combinatorics of the {@code coSort} array can be
        /// perfomed. Use this method only if you are sure, in what you are doing. If not - use stable sort methods like an
        /// insertion sort or Tim sort.</b> <p/> <p><b>NOTE:</b> The method throws {@code IllegalArgumentException} if {@code
        /// target == coSort}, because in this case no sorting will be perfomed.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="toIndex">the index of the last element (exclusive) to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array, during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if <tt>fromIndex &gt; toIndex</tt></exception>
        /// <exception cref="ArrayIndexOutOfBoundsException">if <tt>fromIndex &lt; 0</tt> or <tt>toIndex &gt; target.length</tt> or
        ///                                        <tt>toIndex &gt; coSort.length</tt></exception>
        /// <exception cref="IllegalArgumentException">if target == coSort (as references).</exception>
        public static void QuickSort(int[] target, int fromIndex, int toIndex, int[] coSort)
        {
            RangeCheck(target.Length, fromIndex, toIndex);
            RangeCheck(coSort.Length, fromIndex, toIndex);
            QuickSort1(target, fromIndex, toIndex - fromIndex, coSort);
        }

        /// <summary>
        /// This method is the same as {@link #quickSort(int[], int, int, int[]) }, but without range checking and toIndex ->
        /// length (see params). Throws {@code IllegalArgumentException} if {@code target == coSort}, because in this case no
        /// sorting will be perfomed . <p/> <p><b>NOTE: this is unstable sort algorithm, so additional combinatorics of the
        /// {@code coSort} array can be perfomed. Use this method only if you are sure, in what you are doing. If not - use
        /// stable sort methods like an insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="length">the length of the sorting subarray.</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array, during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if target == coSort (as references).</exception>
        public static void QuickSort1(int[] target, int fromIndex, int length, int[] coSort)
        {
            if (target == coSort)
                throw new ArgumentException("Target reference == coSort reference.");
            QuickSort2(target, fromIndex, length, coSort);
        }

        private static void QuickSort2(int[] target, int fromIndex, int length, int[] coSort)
        {

            // Insertion quickSort on smallest arrays
            if (length < 7)
            {
                for (int i = fromIndex; i < length + fromIndex; i++)
                    for (int j = i; j > fromIndex && target[j - 1] > target[j]; j--)
                        Swap(target, j, j - 1, coSort);
                return;
            }


            // Choose a partition element, v
            int m = fromIndex + (length >> 1); // Small arrays, middle element
            if (length > 7)
            {
                int l = fromIndex;
                int n = fromIndex + length - 1;
                if (length > 40)
                {

                    // Big arrays, pseudomedian of 9
                    int s = length / 8;
                    l = Med3(target, l, l + s, l + 2 * s);
                    m = Med3(target, m - s, m, m + s);
                    n = Med3(target, n - 2 * s, n - s, n);
                }

                m = Med3(target, l, m, n); // Mid-size, med of 3
            }

            int v = target[m];

            // Establish Invariant: v* (<v)* (>v)* v*
            int a = fromIndex, b = a, c = fromIndex + length - 1, d = c;
            while (true)
            {
                while (b <= c && target[b] <= v)
                {
                    if (target[b] == v)
                        Swap(target, a++, b, coSort);
                    b++;
                }

                while (c >= b && target[c] >= v)
                {
                    if (target[c] == v)
                        Swap(target, c, d--, coSort);
                    c--;
                }

                if (b > c)
                    break;
                Swap(target, b++, c--, coSort);
            }


            // Swap partition elements back to middle
            int s, n = fromIndex + length;
            s = Math.Min(a - fromIndex, b - a);
            Vecswap(target, fromIndex, b - s, s, coSort);
            s = Math.Min(d - c, n - d - 1);
            Vecswap(target, b, n - s, s, coSort);

            // Recursively quickSort non-partition-elements
            if ((s = b - a) > 1)
                QuickSort2(target, fromIndex, s, coSort);
            if ((s = d - c) > 1)
                QuickSort2(target, n - s, s, coSort);
        }

        private static void Swap(int[] x, int a, int b, int[] coSort)
        {
            Swap(x, a, b);
            Swap(coSort, a, b);
        }

        /// <summary>
        /// Swaps x[a] with x[b].
        /// </summary>
        public static void Swap<T>(T[] x, int a, int b)
        {
            (x[a], x[b]) = (x[b], x[a]);
        }

        private static void Vecswap(int[] x, int a, int b, int n, int[] coSort)
        {
            for (int i = 0; i < n; i++, a++, b++)
                Swap(x, a, b, coSort);
        }

        /// <summary>
        /// Returns the index of the median of the three indexed integers.
        /// </summary>
        private static int Med3(int[] x, int a, int b, int c)
        {
            return (x[a] < x[b] ? (x[b] < x[c] ? b : x[a] < x[c] ? c : a) : (x[b] > x[c] ? b : x[a] > x[c] ? c : a));
        }

        // =================  QUICKSORT LONG[] LONG[] =================
        /// <summary>
        /// Sorts the specified target array of ints into ascending numerical order and simultaneously permutes the {@code
        /// coSort} longs array in the same way as the target array. <p/> The code was taken from the jdk6 Arrays class. <p/>
        /// The sorting algorithm is a tuned quicksort, adapted from Jon L. Bentley and M. Douglas McIlroy's "Engineering a
        /// Sort Function", Software-Practice and Experience, Vol. 23(11) P. 1249-1265 (November 1993). This algorithm offers
        /// n*log(n) performance on many data sets that cause other quicksorts to degrade to quadratic performance. <p/>
        /// <p/>
        /// <p><b>NOTE: this is unstable sort algorithm, so additional combinatorics of the {@code coSort} array can be
        /// perfomed. Use this method only if you are sure, in what you are doing. If not - use stable sort methods like an
        /// insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if coSort length less then target length.</exception>
        public static void QuickSort(long[] target, long[] coSort)
        {
            QuickSort1(target, 0, target.Length, coSort);
        }

        /// <summary>
        /// Sorts the specified range of the specified target array of ints into ascending numerical order and simultaneously
        /// permutes the {@code coSort} longs array in the same way as the target array. The range to be sorted extends from
        /// index <tt>fromIndex</tt>, inclusive, to index <tt>toIndex</tt>, exclusive. (If <tt>fromIndex==toIndex</tt>, the
        /// range to be sorted is empty.)<p> <p/> The code was taken from the jdk6 Arrays class. <p/> The sorting algorithm
        /// is a tuned quicksort, adapted from Jon L. Bentley and M. Douglas McIlroy's "Engineering a Sort Function",
        /// Software-Practice and Experience, Vol. 23(11) P. 1249-1265 (November 1993). This algorithm offers n*log(n)
        /// performance on many data sets that cause other quicksorts to degrade to quadratic performance. <p/> <p/>
        /// <p><b>NOTE: this is unstable sort algorithm, so additional combinatorics of the {@code coSort} array can be
        /// performed. Use this method only if you are sure, in what you are doing. If not - use stable sort methods like an
        /// insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="toIndex">the index of the last element (exclusive) to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array, during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if <tt>fromIndex &gt; toIndex</tt></exception>
        /// <exception cref="ArrayIndexOutOfBoundsException">if <tt>fromIndex &lt; 0</tt> or <tt>toIndex &gt; target.length</tt> or
        ///                                        <tt>toIndex &gt; coSort.length</tt></exception>
        public static void QuickSort(long[] target, int fromIndex, int toIndex, long[] coSort)
        {
            RangeCheck(target.Length, fromIndex, toIndex);
            RangeCheck(coSort.Length, fromIndex, toIndex);
            QuickSort1(target, fromIndex, toIndex - fromIndex, coSort);
        }

        /// <summary>
        /// This method is the same as {@link #quickSort(int[], int, int, long[])  ) }, but without range checking. <p/>
        /// <p><b>NOTE: this is unstable sort algorithm, so additional combinatorics of the {@code coSort} array can be
        /// performed. Use this method only if you are sure, in what you are doing. If not - use stable sort methods like an
        /// insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="length">the length of the sorting subarray.</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array, during sorting procedure</param>
        public static void QuickSort1(long[] target, int fromIndex, int length, long[] coSort)
        {

            // Insertion quickSort on smallest arrays
            if (length < 7)
            {
                for (int i = fromIndex; i < length + fromIndex; i++)
                    for (int j = i; j > fromIndex && target[j - 1] > target[j]; j--)
                        Swap(target, j, j - 1, coSort);
                return;
            }


            // Choose a partition element, v
            int m = fromIndex + (length >> 1); // Small arrays, middle element
            if (length > 7)
            {
                int l = fromIndex;
                int n = fromIndex + length - 1;
                if (length > 40)
                {

                    // Big arrays, pseudomedian of 9
                    int s = length / 8;
                    l = Med3(target, l, l + s, l + 2 * s);
                    m = Med3(target, m - s, m, m + s);
                    n = Med3(target, n - 2 * s, n - s, n);
                }

                m = Med3(target, l, m, n); // Mid-size, med of 3
            }

            long v = target[m];

            // Establish Invariant: v* (<v)* (>v)* v*
            int a = fromIndex, b = a, c = fromIndex + length - 1, d = c;
            while (true)
            {
                while (b <= c && target[b] <= v)
                {
                    if (target[b] == v)
                        Swap(target, a++, b, coSort);
                    b++;
                }

                while (c >= b && target[c] >= v)
                {
                    if (target[c] == v)
                        Swap(target, c, d--, coSort);
                    c--;
                }

                if (b > c)
                    break;
                Swap(target, b++, c--, coSort);
            }


            // Swap partition elements back to middle
            int s, n = fromIndex + length;
            s = Math.Min(a - fromIndex, b - a);
            Vecswap(target, fromIndex, b - s, s, coSort);
            s = Math.Min(d - c, n - d - 1);
            Vecswap(target, b, n - s, s, coSort);

            // Recursively quickSort non-partition-elements
            if ((s = b - a) > 1)
                QuickSort1(target, fromIndex, s, coSort);
            if ((s = d - c) > 1)
                QuickSort1(target, n - s, s, coSort);
        }

        private static void Swap(long[] x, int a, int b, long[] coSort)
        {
            Swap(x, a, b);
            Swap(coSort, a, b);
        }

        private static void Vecswap(long[] x, int a, int b, int n, long[] coSort)
        {
            for (int i = 0; i < n; i++, a++, b++)
                Swap(x, a, b, coSort);
        }

        private static int Med3(long[] x, int a, int b, int c)
        {
            return (x[a] < x[b] ? (x[b] < x[c] ? b : x[a] < x[c] ? c : a) : (x[b] > x[c] ? b : x[a] > x[c] ? c : a));
        }

        // =================  QUICKSORT INT[] LONG[] =================
        /// <summary>
        /// Sorts the specified target array of ints into ascending numerical order and simultaneously permutes the {@code
        /// coSort} longs array in the same way as the target array. <p/> The code was taken from the jdk6 Arrays class. <p/>
        /// The sorting algorithm is a tuned quicksort, adapted from Jon L. Bentley and M. Douglas McIlroy's "Engineering a
        /// Sort Function", Software-Practice and Experience, Vol. 23(11) P. 1249-1265 (November 1993). This algorithm offers
        /// n*log(n) performance on many data sets that cause other quicksorts to degrade to quadratic performance. <p/>
        /// <p/>
        /// <p><b>NOTE: this is unstable sort algorithm, so additional combinatorics of the {@code coSort} array can be
        /// perfomed. Use this method only if you are sure, in what you are doing. If not - use stable sort methods like an
        /// insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if coSort length less then target length.</exception>
        public static void QuickSort(int[] target, long[] coSort)
        {
            QuickSort1(target, 0, target.Length, coSort);
        }

        /// <summary>
        /// Sorts the specified range of the specified target array of ints into ascending numerical order and simultaneously
        /// permutes the {@code coSort} longs array in the same way as the target array. The range to be sorted extends from
        /// index <tt>fromIndex</tt>, inclusive, to index <tt>toIndex</tt>, exclusive. (If <tt>fromIndex==toIndex</tt>, the
        /// range to be sorted is empty.)<p> <p/> The code was taken from the jdk6 Arrays class. <p/> The sorting algorithm
        /// is a tuned quicksort, adapted from Jon L. Bentley and M. Douglas McIlroy's "Engineering a Sort Function",
        /// Software-Practice and Experience, Vol. 23(11) P. 1249-1265 (November 1993). This algorithm offers n*log(n)
        /// performance on many data sets that cause other quicksorts to degrade to quadratic performance. <p/> <p/>
        /// <p><b>NOTE: this is unstable sort algorithm, so additional combinatorics of the {@code coSort} array can be
        /// performed. Use this method only if you are sure, in what you are doing. If not - use stable sort methods like an
        /// insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="toIndex">the index of the last element (exclusive) to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array, during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if <tt>fromIndex &gt; toIndex</tt></exception>
        /// <exception cref="ArrayIndexOutOfBoundsException">if <tt>fromIndex &lt; 0</tt> or <tt>toIndex &gt; target.length</tt> or
        ///                                        <tt>toIndex &gt; coSort.length</tt></exception>
        public static void QuickSort(int[] target, int fromIndex, int toIndex, long[] coSort)
        {
            RangeCheck(target.Length, fromIndex, toIndex);
            RangeCheck(coSort.Length, fromIndex, toIndex);
            QuickSort1(target, fromIndex, toIndex - fromIndex, coSort);
        }

        /// <summary>
        /// This method is the same as {@link #quickSort(int[], int, int, long[])  ) }, but without range checking. <p/>
        /// <p><b>NOTE: this is unstable sort algorithm, so additional combinatorics of the {@code coSort} array can be
        /// performed. Use this method only if you are sure, in what you are doing. If not - use stable sort methods like an
        /// insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="length">the length of the sorting subarray.</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array, during sorting procedure</param>
        public static void QuickSort1(int[] target, int fromIndex, int length, long[] coSort)
        {

            // Insertion quickSort on smallest arrays
            if (length < 7)
            {
                for (int i = fromIndex; i < length + fromIndex; i++)
                    for (int j = i; j > fromIndex && target[j - 1] > target[j]; j--)
                        Swap(target, j, j - 1, coSort);
                return;
            }


            // Choose a partition element, v
            int m = fromIndex + (length >> 1); // Small arrays, middle element
            if (length > 7)
            {
                int l = fromIndex;
                int n = fromIndex + length - 1;
                if (length > 40)
                {

                    // Big arrays, pseudomedian of 9
                    int s = length / 8;
                    l = Med3(target, l, l + s, l + 2 * s);
                    m = Med3(target, m - s, m, m + s);
                    n = Med3(target, n - 2 * s, n - s, n);
                }

                m = Med3(target, l, m, n); // Mid-size, med of 3
            }

            int v = target[m];

            // Establish Invariant: v* (<v)* (>v)* v*
            int a = fromIndex, b = a, c = fromIndex + length - 1, d = c;
            while (true)
            {
                while (b <= c && target[b] <= v)
                {
                    if (target[b] == v)
                        Swap(target, a++, b, coSort);
                    b++;
                }

                while (c >= b && target[c] >= v)
                {
                    if (target[c] == v)
                        Swap(target, c, d--, coSort);
                    c--;
                }

                if (b > c)
                    break;
                Swap(target, b++, c--, coSort);
            }


            // Swap partition elements back to middle
            int s, n = fromIndex + length;
            s = Math.Min(a - fromIndex, b - a);
            Vecswap(target, fromIndex, b - s, s, coSort);
            s = Math.Min(d - c, n - d - 1);
            Vecswap(target, b, n - s, s, coSort);

            // Recursively quickSort non-partition-elements
            if ((s = b - a) > 1)
                QuickSort1(target, fromIndex, s, coSort);
            if ((s = d - c) > 1)
                QuickSort1(target, n - s, s, coSort);
        }

        private static void Swap(int[] x, int a, int b, long[] coSort)
        {
            Swap(x, a, b);
            Swap(coSort, a, b);
        }

        /// <summary>
        /// Swaps x[a] with x[b].
        /// </summary>
        public static void Swap(long[] x, int a, int b)
        {
            long t = x[a];
            x[a] = x[b];
            x[b] = t;
        }

        private static void Vecswap(int[] x, int a, int b, int n, long[] coSort)
        {
            for (int i = 0; i < n; i++, a++, b++)
                Swap(x, a, b, coSort);
        }

        // =================  QUICKSORT OBJECT[] OBJECT[] =================
        /// <summary>
        /// Sorts the specified target array of objects into ascending order, according to the natural ordering of its
        /// elements and simultaneously permutes the {@code coSort} objects array in the same way then specified target
        /// array. <p/> The code was taken from the jdk6 Arrays class. <p/> The sorting algorithm is a tuned quicksort,
        /// adapted from Jon L. Bentley and M. Douglas McIlroy's "Engineering a Sort Function", Software-Practice and
        /// Experience, Vol. 23(11) P. 1249-1265 (November 1993). This algorithm offers n*log(n) performance on many data
        /// sets that cause other quicksorts to degrade to quadratic performance. <p/> <p><b>NOTE: this is unstable sort
        /// algorithm, so additional combinatorics of the {@code coSort} array can be perfomed. Use this method only if you
        /// are sure, in what you are doing. If not - use stable sort methods like an insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if <tt>fromIndex &gt; toIndex</tt></exception>
        /// <exception cref="IllegalArgumentException">if coSort length less then target length.</exception>
        /// <exception cref="IllegalArgumentException">if target == coSort (as references).</exception>
        public static void QuickSort<T

        static extends Comparable<T>>(T[] target, object[] coSort)
        {
            QuickSort(target, 0, target.Length, coSort);
        }

        /// <summary>
        /// Sorts the specified target array of objects into ascending order, according to the natural ordering of its
        /// elements and simultaneously permutes the {@code coSort} objects array in the same way then specified target
        /// array. The range to be sorted extends from index <tt>fromIndex</tt>, inclusive, to index <tt>toIndex</tt>,
        /// exclusive. (If <tt>fromIndex==toIndex</tt>, the range to be sorted is empty.)<p> <p/> The code was taken from the
        /// jdk6 Arrays class. <p/> The sorting algorithm is a tuned quicksort, adapted from Jon L. Bentley and M. Douglas
        /// McIlroy's "Engineering a Sort Function", Software-Practice and Experience, Vol. 23(11) P. 1249-1265 (November
        /// 1993). This algorithm offers n*log(n) performance on many data sets that cause other quicksorts to degrade to
        /// quadratic performance. <p/> <p><b>NOTE: this is unstable sort algorithm, so additional combinatorics of the
        /// {@code coSort} array can be perfomed. Use this method only if you are sure, in what you are doing. If not - use
        /// stable sort methods like an insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="toIndex">the index of the last element (exclusive) to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array, during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if <tt>fromIndex &gt; toIndex</tt></exception>
        /// <exception cref="ArrayIndexOutOfBoundsException">if <tt>fromIndex &lt; 0</tt> or <tt>toIndex &gt; target.length</tt> or
        ///                                        <tt>toIndex &gt; coSort.length</tt></exception>
        /// <exception cref="IllegalArgumentException">if target == coSort (as references).</exception>
        public static void QuickSort<T

        static extends Comparable<T>>(T[] target, int fromIndex, int toIndex, object[] coSort)
        {
            if (target == coSort)
                throw new ArgumentException();
            RangeCheck(target.Length, fromIndex, toIndex);
            RangeCheck(coSort.Length, fromIndex, toIndex);
            QuickSort1(target, fromIndex, toIndex - fromIndex, coSort);
        }

        /// <summary>
        /// This method is the same as {@link #quickSort(Comparable[], int, int, Object[])}, but without range checking.
        /// <p/>
        /// <p><b>NOTE: this is unstable sort algorithm, so additional combinatorics of the {@code coSort} array can be
        /// perfomed. Use this method only if you are sure, in what you are doing. If not - use stable sort methods like an
        /// insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="length">the length of the sorting subarray.</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array, during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if target == coSort (as references).</exception>
        public static void QuickSort1<T

        static extends Comparable<T>>(T[] target, int fromIndex, int length, object[] coSort)
        {

            // Insertion quickSort on smallest arrays
            if (length < 7)
            {
                for (int i = fromIndex; i < length + fromIndex; i++)
                    for (int j = i; j > fromIndex && target[j - 1].CompareTo(target[j]) > 0; j--)
                        Swap(target, j, j - 1, coSort);
                return;
            }


            // Choose a partition element, v
            int m = fromIndex + (length >> 1); // Small arrays, middle element
            if (length > 7)
            {
                int l = fromIndex;
                int n = fromIndex + length - 1;
                if (length > 40)
                {

                    // Big arrays, pseudomedian of 9
                    int s = length / 8;
                    l = Med3(target, l, l + s, l + 2 * s);
                    m = Med3(target, m - s, m, m + s);
                    n = Med3(target, n - 2 * s, n - s, n);
                }

                m = Med3(target, l, m, n); // Mid-size, med of 3
            }

            T v = target[m];

            // Establish Invariant: v* (<v)* (>v)* v*
            int a = fromIndex, b = a, c = fromIndex + length - 1, d = c;
            while (true)
            {
                while (b <= c && target[b].CompareTo(v) <= 0)
                {
                    if (target[b] == v)
                        Swap(target, a++, b, coSort);
                    b++;
                }

                while (c >= b && target[c].CompareTo(v) >= 0)
                {
                    if (target[c] == v)
                        Swap(target, c, d--, coSort);
                    c--;
                }

                if (b > c)
                    break;
                Swap(target, b++, c--, coSort);
            }


            // Swap partition elements back to middle
            int s, n = fromIndex + length;
            s = Math.Min(a - fromIndex, b - a);
            Vecswap(target, fromIndex, b - s, s, coSort);
            s = Math.Min(d - c, n - d - 1);
            Vecswap(target, b, n - s, s, coSort);

            // Recursively quickSort non-partition-elements
            if ((s = b - a) > 1)
                QuickSort1(target, fromIndex, s, coSort);
            if ((s = d - c) > 1)
                QuickSort1(target, n - s, s, coSort);
        }

        private static void Swap(object[] x, int a, int b, object[] coSort)
        {
            Swap(x, a, b);
            Swap(coSort, a, b);
        }

        private static void Vecswap(object[] x, int a, int b, int n, object[] coSort)
        {
            for (int i = 0; i < n; i++, a++, b++)
                Swap(x, a, b, coSort);
        }

        // =================  QUICKSORT OBJECT[] INT[] =================
        /// <summary>
        /// Sorts the specified target array of objects into ascending order, according to the natural ordering of its
        /// elements and simultaneously permutes the {@code coSort} objects array in the same way then specified target
        /// array. <p/> The code was taken from the jdk6 Arrays class. <p/> The sorting algorithm is a tuned quicksort,
        /// adapted from Jon L. Bentley and M. Douglas McIlroy's "Engineering a Sort Function", Software-Practice and
        /// Experience, Vol. 23(11) P. 1249-1265 (November 1993). This algorithm offers n*log(n) performance on many data
        /// sets that cause other quicksorts to degrade to quadratic performance. <p/> <p><b>NOTE: this is unstable sort
        /// algorithm, so additional combinatorics of the {@code coSort} array can be perfomed. Use this method only if you
        /// are sure, in what you are doing. If not - use stable sort methods like an insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if <tt>fromIndex &gt; toIndex</tt></exception>
        /// <exception cref="IllegalArgumentException">if coSort length less then target length.</exception>
        /// <exception cref="IllegalArgumentException">if target == coSort (as references).</exception>
        public static void QuickSort<T

        static extends Comparable<T>>(T[] target, int[] coSort)
        {
            QuickSort(target, 0, target.Length, coSort);
        }

        /// <summary>
        /// Sorts the specified target array of objects into ascending order, according to the natural ordering of its
        /// elements and simultaneously permutes the {@code coSort} objects array in the same way then specified target
        /// array. The range to be sorted extends from index <tt>fromIndex</tt>, inclusive, to index <tt>toIndex</tt>,
        /// exclusive. (If <tt>fromIndex==toIndex</tt>, the range to be sorted is empty.)<p> <p/> The code was taken from the
        /// jdk6 Arrays class. <p/> The sorting algorithm is a tuned quicksort, adapted from Jon L. Bentley and M. Douglas
        /// McIlroy's "Engineering a Sort Function", Software-Practice and Experience, Vol. 23(11) P. 1249-1265 (November
        /// 1993). This algorithm offers n*log(n) performance on many data sets that cause other quicksorts to degrade to
        /// quadratic performance. <p/> <p><b>NOTE: this is unstable sort algorithm, so additional combinatorics of the
        /// {@code coSort} array can be perfomed. Use this method only if you are sure, in what you are doing. If not - use
        /// stable sort methods like an insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="toIndex">the index of the last element (exclusive) to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array, during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if <tt>fromIndex &gt; toIndex</tt></exception>
        /// <exception cref="ArrayIndexOutOfBoundsException">if <tt>fromIndex &lt; 0</tt> or <tt>toIndex &gt; target.length</tt> or
        ///                                        <tt>toIndex &gt; coSort.length</tt></exception>
        /// <exception cref="IllegalArgumentException">if target == coSort (as references).</exception>
        public static void QuickSort<T

        static extends Comparable<T>>(T[] target, int fromIndex, int toIndex, int[] coSort)
        {
            RangeCheck(target.Length, fromIndex, toIndex);
            RangeCheck(coSort.Length, fromIndex, toIndex);
            QuickSort1(target, fromIndex, toIndex - fromIndex, coSort);
        }

        /// <summary>
        /// This method is the same as {@link #quickSort(Comparable[], int, int, Object[])}, but without range checking.
        /// <p/>
        /// <p><b>NOTE: this is unstable sort algorithm, so additional combinatorics of the {@code coSort} array can be
        /// perfomed. Use this method only if you are sure, in what you are doing. If not - use stable sort methods like an
        /// insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="length">the length of the sorting subarray.</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array, during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if target == coSort (as references).</exception>
        public static void QuickSort1<T

        static extends Comparable<T>>(T[] target, int fromIndex, int length, int[] coSort)
        {

            // Insertion quickSort on smallest arrays
            if (length < 7)
            {
                for (int i = fromIndex; i < length + fromIndex; i++)
                    for (int j = i; j > fromIndex && target[j - 1].CompareTo(target[j]) > 0; j--)
                        Swap(target, j, j - 1, coSort);
                return;
            }


            // Choose a partition element, v
            int m = fromIndex + (length >> 1); // Small arrays, middle element
            if (length > 7)
            {
                int l = fromIndex;
                int n = fromIndex + length - 1;
                if (length > 40)
                {

                    // Big arrays, pseudomedian of 9
                    int s = length / 8;
                    l = Med3(target, l, l + s, l + 2 * s);
                    m = Med3(target, m - s, m, m + s);
                    n = Med3(target, n - 2 * s, n - s, n);
                }

                m = Med3(target, l, m, n); // Mid-size, med of 3
            }

            T v = target[m];

            // Establish Invariant: v* (<v)* (>v)* v*
            int a = fromIndex, b = a, c = fromIndex + length - 1, d = c;
            while (true)
            {
                while (b <= c && target[b].CompareTo(v) <= 0)
                {
                    if (target[b] == v)
                        Swap(target, a++, b, coSort);
                    b++;
                }

                while (c >= b && target[c].CompareTo(v) >= 0)
                {
                    if (target[c] == v)
                        Swap(target, c, d--, coSort);
                    c--;
                }

                if (b > c)
                    break;
                Swap(target, b++, c--, coSort);
            }


            // Swap partition elements back to middle
            int s, n = fromIndex + length;
            s = Math.Min(a - fromIndex, b - a);
            Vecswap(target, fromIndex, b - s, s, coSort);
            s = Math.Min(d - c, n - d - 1);
            Vecswap(target, b, n - s, s, coSort);

            // Recursively quickSort non-partition-elements
            if ((s = b - a) > 1)
                QuickSort1(target, fromIndex, s, coSort);
            if ((s = d - c) > 1)
                QuickSort1(target, n - s, s, coSort);
        }

        private static void Swap(object[] x, int a, int b, int[] coSort)
        {
            Swap(x, a, b);
            Swap(coSort, a, b);
        }

        private static void Vecswap(object[] x, int a, int b, int n, int[] coSort)
        {
            for (int i = 0; i < n; i++, a++, b++)
                Swap(x, a, b, coSort);
        }

        /// <summary>
        /// Swaps x[a] with x[b].
        /// </summary>
        public static void Swap(object[] x, int a, int b)
        {
            object t = x[a];
            x[a] = x[b];
            x[b] = t;
        }

        private static int Med3<T

        static extends Comparable<T>>(T[] x, int a, int b, int c)
        {
            return (x[a].CompareTo(x[b]) < 0 ? (x[b].CompareTo(x[c]) < 0 ? b : x[a].CompareTo(x[c]) < 0 ? c : a) : (x[b].CompareTo(x[c]) > 0 ? b : x[a].CompareTo(x[c]) > 0 ? c : a));
        }

        // =================  QUICKSORT INT[] OBJECT[] =================
        /// <summary>
        /// Sorts the specified target array of ints into ascending numerical order and simultaneously permutes the {@code
        /// coSort} Objects array in the same way as the target array. <p/> The code was taken from the jdk6 Arrays class.
        /// <p/> The sorting algorithm is a tuned quicksort, adapted from Jon L. Bentley and M. Douglas McIlroy's
        /// "Engineering a Sort Function", Software-Practice and Experience, Vol. 23(11) P. 1249-1265 (November 1993). This
        /// algorithm offers n*log(n) performance on many data sets that cause other quicksorts to degrade to quadratic
        /// performance. <p/> <p/> <p><b>NOTE: this is unstable sort algorithm, so additional combinatorics of the {@code
        /// coSort} array can be perfomed. Use this method only if you are sure, in what you are doing. If not - use stable
        /// sort methods like an insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if coSort length less then target length.</exception>
        public static void QuickSort(int[] target, object[] coSort)
        {
            QuickSort1(target, 0, target.Length, coSort);
        }

        /// <summary>
        /// Sorts the specified range of the specified target array of ints into ascending numerical order and simultaneously
        /// permutes the {@code coSort} Objects array in the same way as the target array. The range to be sorted extends
        /// from index <tt>fromIndex</tt>, inclusive, to index <tt>toIndex</tt>, exclusive. (If <tt>fromIndex==toIndex</tt>,
        /// the range to be sorted is empty.)<p> <p/> The code was taken from the jdk6 Arrays class. <p/> The sorting
        /// algorithm is a tuned quicksort, adapted from Jon L. Bentley and M. Douglas McIlroy's "Engineering a Sort
        /// Function", Software-Practice and Experience, Vol. 23(11) P. 1249-1265 (November 1993). This algorithm offers
        /// n*log(n) performance on many data sets that cause other quicksorts to degrade to quadratic performance. <p/>
        /// <p/>
        /// <p><b>NOTE: this is unstable sort algorithm, so additional combinatorics of the {@code coSort} array can be
        /// perfomed. Use this method only if you are sure, in what you are doing. If not - use stable sort methods like an
        /// insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="toIndex">the index of the last element (exclusive) to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array, during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if <tt>fromIndex &gt; toIndex</tt></exception>
        /// <exception cref="ArrayIndexOutOfBoundsException">if <tt>fromIndex &lt; 0</tt> or <tt>toIndex &gt; target.length</tt> or
        ///                                        <tt>toIndex &gt; coSort.length</tt></exception>
        public static void QuickSort(int[] target, int fromIndex, int toIndex, object[] coSort)
        {
            RangeCheck(target.Length, fromIndex, toIndex);
            RangeCheck(coSort.Length, fromIndex, toIndex);
            QuickSort1(target, fromIndex, toIndex - fromIndex, coSort);
        }

        /// <summary>
        /// This method is the same as {@link #quickSort(int[], int, int, Object[])  ) }, but without range checking. <p/>
        /// <p><b>NOTE: this is unstable sort algorithm, so additional combinatorics of the {@code coSort} array can be
        /// perfomed. Use this method only if you are sure, in what you are doing. If not - use stable sort methods like an
        /// insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="length">the length of the sorting subarray.</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array, during sorting procedure</param>
        public static void QuickSort1(int[] target, int fromIndex, int length, object[] coSort)
        {

            // Insertion quickSort on smallest arrays
            if (length < 7)
            {
                for (int i = fromIndex; i < length + fromIndex; i++)
                    for (int j = i; j > fromIndex && target[j - 1] > target[j]; j--)
                        Swap(target, j, j - 1, coSort);
                return;
            }


            // Choose a partition element, v
            int m = fromIndex + (length >> 1); // Small arrays, middle element
            if (length > 7)
            {
                int l = fromIndex;
                int n = fromIndex + length - 1;
                if (length > 40)
                {

                    // Big arrays, pseudomedian of 9
                    int s = length / 8;
                    l = Med3(target, l, l + s, l + 2 * s);
                    m = Med3(target, m - s, m, m + s);
                    n = Med3(target, n - 2 * s, n - s, n);
                }

                m = Med3(target, l, m, n); // Mid-size, med of 3
            }

            int v = target[m];

            // Establish Invariant: v* (<v)* (>v)* v*
            int a = fromIndex, b = a, c = fromIndex + length - 1, d = c;
            while (true)
            {
                while (b <= c && target[b] <= v)
                {
                    if (target[b] == v)
                        Swap(target, a++, b, coSort);
                    b++;
                }

                while (c >= b && target[c] >= v)
                {
                    if (target[c] == v)
                        Swap(target, c, d--, coSort);
                    c--;
                }

                if (b > c)
                    break;
                Swap(target, b++, c--, coSort);
            }


            // Swap partition elements back to middle
            int s, n = fromIndex + length;
            s = Math.Min(a - fromIndex, b - a);
            Vecswap(target, fromIndex, b - s, s, coSort);
            s = Math.Min(d - c, n - d - 1);
            Vecswap(target, b, n - s, s, coSort);

            // Recursively quickSort non-partition-elements
            if ((s = b - a) > 1)
                QuickSort1(target, fromIndex, s, coSort);
            if ((s = d - c) > 1)
                QuickSort1(target, n - s, s, coSort);
        }

        private static void Swap(int[] x, int a, int b, object[] coSort)
        {
            Swap(x, a, b);
            Swap(coSort, a, b);
        }

        private static void Vecswap(int[] x, int a, int b, int n, object[] coSort)
        {
            for (int i = 0; i < n; i++, a++, b++)
                Swap(x, a, b, coSort);
        }

        public static int[] QuickSortP(short[] target)
        {
            int[] permutation = new int[target.Length];
            for (int i = 1; i < target.Length; ++i)
                permutation[i] = i;
            QuickSort(target, 0, target.Length, permutation);
            return permutation;
        }

        // =================  QUICKSORT SHORT[] INT[] =================
        /// <summary>
        /// Sorts the specified range of the specified target array of ints into ascending numerical order and simultaneously
        /// permutes the {@code coSort} ints array in the same way as the target array. The range to be sorted extends from
        /// index <tt>fromIndex</tt>, inclusive, to index <tt>toIndex</tt>, exclusive. (If <tt>fromIndex==toIndex</tt>, the
        /// range to be sorted is empty.)<p> <p/> The code was taken from the jdk6 Arrays class. <p/> The sorting algorithm
        /// is a tuned quicksort, adapted from Jon L. Bentley and M. Douglas McIlroy's "Engineering a Sort Function",
        /// Software-Practice and Experience, Vol. 23(11) P. 1249-1265 (November 1993). This algorithm offers n*log(n)
        /// performance on many data sets that cause other quicksorts to degrade to quadratic performance. <p/> <p><b>NOTE:
        /// remember this is unstable sort algorithm, so additional combinatorics of the {@code coSort} array can be
        /// perfomed. Use this method only if you are sure, in what you are doing. If not - use stable sort methods like an
        /// insertion sort or Tim sort.</b> <p/> <p><b>NOTE:</b> The method throws {@code IllegalArgumentException} if {@code
        /// target == coSort}, because in this case no sorting will be perfomed.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="toIndex">the index of the last element (exclusive) to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array, during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if <tt>fromIndex &gt; toIndex</tt></exception>
        /// <exception cref="ArrayIndexOutOfBoundsException">if <tt>fromIndex &lt; 0</tt> or <tt>toIndex &gt; target.length</tt> or
        ///                                        <tt>toIndex &gt; coSort.length</tt></exception>
        public static void QuickSort(short[] target, int fromIndex, int toIndex, int[] coSort)
        {
            RangeCheck(target.Length, fromIndex, toIndex);
            RangeCheck(coSort.Length, fromIndex, toIndex);
            QuickSort1(target, fromIndex, toIndex - fromIndex, coSort);
        }

        /// <summary>
        /// This method is the same as {@link #quickSort(int[], int, int, int[]) }, but without range checking and toIndex ->
        /// length (see params). Throws {@code IllegalArgumentException} if {@code target == coSort}, because in this case no
        /// sorting will be perfomed . <p/> <p><b>NOTE: this is unstable sort algorithm, so additional combinatorics of the
        /// {@code coSort} array can be perfomed. Use this method only if you are sure, in what you are doing. If not - use
        /// stable sort methods like an insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="length">the length of the sorting subarray.</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array, during sorting procedure</param>
        public static void QuickSort1(short[] target, int fromIndex, int length, int[] coSort)
        {
            QuickSort2(target, fromIndex, length, coSort);
        }

        private static void QuickSort2(short[] target, int fromIndex, int length, int[] coSort)
        {

            // Insertion quickSort on smallest arrays
            if (length < 7)
            {
                for (int i = fromIndex; i < length + fromIndex; i++)
                    for (int j = i; j > fromIndex && target[j - 1] > target[j]; j--)
                        Swap(target, j, j - 1, coSort);
                return;
            }


            // Choose a partition element, v
            int m = fromIndex + (length >> 1); // Small arrays, middle element
            if (length > 7)
            {
                int l = fromIndex;
                int n = fromIndex + length - 1;
                if (length > 40)
                {

                    // Big arrays, pseudomedian of 9
                    int s = length / 8;
                    l = Med3(target, l, l + s, l + 2 * s);
                    m = Med3(target, m - s, m, m + s);
                    n = Med3(target, n - 2 * s, n - s, n);
                }

                m = Med3(target, l, m, n); // Mid-size, med of 3
            }

            int v = target[m];

            // Establish Invariant: v* (<v)* (>v)* v*
            int a = fromIndex, b = a, c = fromIndex + length - 1, d = c;
            while (true)
            {
                while (b <= c && target[b] <= v)
                {
                    if (target[b] == v)
                        Swap(target, a++, b, coSort);
                    b++;
                }

                while (c >= b && target[c] >= v)
                {
                    if (target[c] == v)
                        Swap(target, c, d--, coSort);
                    c--;
                }

                if (b > c)
                    break;
                Swap(target, b++, c--, coSort);
            }


            // Swap partition elements back to middle
            int s, n = fromIndex + length;
            s = Math.Min(a - fromIndex, b - a);
            Vecswap(target, fromIndex, b - s, s, coSort);
            s = Math.Min(d - c, n - d - 1);
            Vecswap(target, b, n - s, s, coSort);

            // Recursively quickSort non-partition-elements
            if ((s = b - a) > 1)
                QuickSort2(target, fromIndex, s, coSort);
            if ((s = d - c) > 1)
                QuickSort2(target, n - s, s, coSort);
        }

        /// <summary>
        /// Sorts the specified target array of shorts into ascending numerical order and simultaneously permutes the {@code
        /// coSort} ints array in the same way as the target array. <p/> The code was taken from the jdk6 Arrays class. <p/>
        /// The sorting algorithm is a tuned quicksort, adapted from Jon L. Bentley and M. Douglas McIlroy's "Engineering a
        /// Sort Function", Software-Practice and Experience, Vol. 23(11) P. 1249-1265 (November 1993). This algorithm offers
        /// n*log(n) performance on many data sets that cause other quicksorts to degrade to quadratic performance. <p/>
        /// <p><b>NOTE: remember this is unstable sort algorithm, so additional combinatorics of the {@code coSort} array
        /// can
        /// be perfomed. Use this method only if you are sure, in what you are doing. If not - use stable sort methods like
        /// an insertion sort or Tim sort.</b> <p/> <p><b>NOTE:</b> The method throws {@code IllegalArgumentException} if
        /// {@code target == coSort}, because in this case no sorting will be perfomed.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="coSort">the array which will be permuted in the same way as the target array during sorting procedure</param>
        /// <exception cref="IllegalArgumentException">if coSort length less then target length.</exception>
        public static void QuickSort(short[] target, int[] coSort)
        {
            QuickSort(target, 0, target.Length, coSort);
        }

        private static void Swap(short[] x, int a, int b, int[] coSort)
        {
            Swap(x, a, b);
            Swap(coSort, a, b);
        }

        /// <summary>
        /// Swaps x[a] with x[b].
        /// </summary>
        private static void Swap(short[] x, int a, int b)
        {
            short t = x[a];
            x[a] = x[b];
            x[b] = t;
        }

        private static void Vecswap(short[] x, int a, int b, int n, int[] coSort)
        {
            for (int i = 0; i < n; i++, a++, b++)
                Swap(x, a, b, coSort);
        }

        /// <summary>
        /// Returns the index of the median of the three indexed integers.
        /// </summary>
        private static int Med3(short[] x, int a, int b, int c)
        {
            return (x[a] < x[b] ? (x[b] < x[c] ? b : x[a] < x[c] ? c : a) : (x[b] > x[c] ? b : x[a] > x[c] ? c : a));
        }

        ////////////////////////////////////// COMPARATOR /////////////////////////////////////////////////////
        /// <summary>
        /// Sorts the specified range of the specified target array of ints into order specified by {@link IntComparator}
        /// using quicksort.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="comparator">custom comparator</param>
        /// <exception cref="IllegalArgumentException">if coSort length less then target length.</exception>
        public static void QuickSort(int[] target, IntComparator comparator)
        {
            QuickSort1(target, 0, target.Length, comparator);
        }

        /// <summary>
        /// Sorts the specified range of the specified target array of ints into order specified by {@link IntComparator}
        /// using quicksort.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="toIndex">the index of the last element (exclusive) to be sorted</param>
        /// <param name="comparator">comparator</param>
        /// <exception cref="IllegalArgumentException">if <tt>fromIndex &gt; toIndex</tt></exception>
        /// <exception cref="ArrayIndexOutOfBoundsException">if <tt>fromIndex &lt; 0</tt> or <tt>toIndex &gt; target.length</tt> or
        ///                                        <tt>toIndex &gt; coSort.length</tt></exception>
        public static void QuickSort(int[] target, int fromIndex, int toIndex, IntComparator comparator)
        {
            RangeCheck(target.Length, fromIndex, toIndex);
            QuickSort1(target, fromIndex, toIndex - fromIndex, comparator);
        }

        /// <summary>
        /// Sorts the specified range of the specified target array of ints into order specified by {@link IntComparator}
        /// using quicksort.
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="length">the length of the sorting subarray.</param>
        /// <param name="comparator">comparator</param>
        public static void QuickSort1(int[] target, int fromIndex, int length, IntComparator comparator)
        {

            // Insertion quickSort on smallest arrays
            if (length < 7)
            {
                for (int i = fromIndex; i < length + fromIndex; i++)
                    for (int j = i; j > fromIndex && comparator.Compare(target[j - 1], target[j]) > 0; j--)
                        Swap(target, j, j - 1);
                return;
            }


            // Choose a partition element, v
            int m = fromIndex + (length >> 1); // Small arrays, middle element
            if (length > 7)
            {
                int l = fromIndex;
                int n = fromIndex + length - 1;
                if (length > 40)
                {

                    // Big arrays, pseudomedian of 9
                    int s = length / 8;
                    l = Med3(target, l, l + s, l + 2 * s, comparator);
                    m = Med3(target, m - s, m, m + s, comparator);
                    n = Med3(target, n - 2 * s, n - s, n, comparator);
                }

                m = Med3(target, l, m, n, comparator); // Mid-size, med of 3
            }

            int v = target[m];

            // Establish Invariant: v* (<v)* (>v)* v*
            int a = fromIndex, b = a, c = fromIndex + length - 1, d = c;
            while (true)
            {
                while (b <= c && comparator.Compare(target[b], v) <= 0)
                {
                    if (comparator.Compare(target[b], v) == 0)
                        Swap(target, a++, b);
                    b++;
                }

                while (c >= b && comparator.Compare(target[c], v) >= 0)
                {
                    if (comparator.Compare(target[c], v) == 0)
                        Swap(target, c, d--);
                    c--;
                }

                if (b > c)
                    break;
                Swap(target, b++, c--);
            }


            // Swap partition elements back to middle
            int s, n = fromIndex + length;
            s = Math.Min(a - fromIndex, b - a);
            Vecswap(target, fromIndex, b - s, s);
            s = Math.Min(d - c, n - d - 1);
            Vecswap(target, b, n - s, s);

            // Recursively quickSort non-partition-elements
            if ((s = b - a) > 1)
                QuickSort1(target, fromIndex, s, comparator);
            if ((s = d - c) > 1)
                QuickSort1(target, n - s, s, comparator);
        }

        /////////////////////////////// QUICK SORT INTCOMPARATOR COSORT ////////////////////////////////////////
        /// <summary>
        /// Sorts the specified target array of ints according to {@link IntComparator} and simultaneously permutes the
        /// {@code coSort} Objects array in the same way as the target array. <p/> The code was taken from the jdk6 Arrays
        /// class. <p/> The sorting algorithm is a tuned quicksort, adapted from Jon L. Bentley and M. Douglas McIlroy's
        /// "Engineering a Sort Function", Software-Practice and Experience, Vol. 23(11) P. 1249-1265 (November 1993). This
        /// algorithm offers n*log(n) performance on many data sets that cause other quicksorts to degrade to quadratic
        /// performance. <p/> <p/> <p><b>NOTE: this is unstable sort algorithm, so additional combinatorics of the {@code
        /// coSort} array can be perfomed. Use this method only if you are sure, in what you are doing. If not - use stable
        /// sort methods like an insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="comparator">custom comparator</param>
        /// <exception cref="IllegalArgumentException">if coSort length less then target length.</exception>
        public static void QuickSort(int[] target, int[] cosort, IntComparator comparator)
        {
            QuickSort1(target, 0, target.Length, cosort, comparator);
        }

        /// <summary>
        /// Sorts the specified range of the specified target array of ints according to {@link IntComparator} and
        /// simultaneously permutes the {@code coSort} Objects array in the same way as the target array. The range to be
        /// sorted extends from index <tt>fromIndex</tt>, inclusive, to index <tt>toIndex</tt>, exclusive. (If
        /// <tt>fromIndex==toIndex</tt>, the range to be sorted is empty.)<p> <p/> The code was taken from the jdk6 Arrays
        /// class. <p/> The sorting algorithm is a tuned quicksort, adapted from Jon L. Bentley and M. Douglas McIlroy's
        /// "Engineering a Sort Function", Software-Practice and Experience, Vol. 23(11) P. 1249-1265 (November 1993). This
        /// algorithm offers n*log(n) performance on many data sets that cause other quicksorts to degrade to quadratic
        /// performance. <p/> <p/> <p><b>NOTE: this is unstable sort algorithm, so additional combinatorics of the {@code
        /// coSort} array can be perfomed. Use this method only if you are sure, in what you are doing. If not - use stable
        /// sort methods like an insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="toIndex">the index of the last element (exclusive) to be sorted</param>
        /// <param name="comparator">comparator</param>
        /// <exception cref="IllegalArgumentException">if <tt>fromIndex &gt; toIndex</tt></exception>
        /// <exception cref="ArrayIndexOutOfBoundsException">if <tt>fromIndex &lt; 0</tt> or <tt>toIndex &gt; target.length</tt> or
        ///                                        <tt>toIndex &gt; coSort.length</tt></exception>
        public static void QuickSort(int[] target, int fromIndex, int toIndex, int[] cosort, IntComparator comparator)
        {
            RangeCheck(target.Length, fromIndex, toIndex);
            if (target == cosort)
                throw new ArgumentException("Same array references.");
            QuickSort1(target, fromIndex, toIndex - fromIndex, cosort, comparator);
        }

        /// <summary>
        /// This method is the same as {@link #quickSort(int[], int, int, Object[])  ) }, but without range checking. <p/>
        /// <p><b>NOTE: this is unstable sort algorithm, so additional combinatorics of the {@code coSort} array can be
        /// perfomed. Use this method only if you are sure, in what you are doing. If not - use stable sort methods like an
        /// insertion sort or Tim sort.</b>
        /// </summary>
        /// <param name="target">the array to be sorted</param>
        /// <param name="fromIndex">the index of the first element (inclusive) to be sorted</param>
        /// <param name="length">the length of the sorting subarray.</param>
        /// <param name="comparator">comparator</param>
        private static void QuickSort1(int[] target, int fromIndex, int length, int[] cosort, IntComparator comparator)
        {

            // Insertion quickSort on smallest arrays
            if (length < 7)
            {
                for (int i = fromIndex; i < length + fromIndex; i++)
                    for (int j = i; j > fromIndex && comparator.Compare(target[j - 1], target[j]) > 0; j--)
                        Swap(target, j, j - 1, cosort);
                return;
            }


            // Choose a partition element, v
            int m = fromIndex + (length >> 1); // Small arrays, middle element
            if (length > 7)
            {
                int l = fromIndex;
                int n = fromIndex + length - 1;
                if (length > 40)
                {

                    // Big arrays, pseudomedian of 9
                    int s = length / 8;
                    l = Med3(target, l, l + s, l + 2 * s, comparator);
                    m = Med3(target, m - s, m, m + s, comparator);
                    n = Med3(target, n - 2 * s, n - s, n, comparator);
                }

                m = Med3(target, l, m, n, comparator); // Mid-size, med of 3
            }

            int v = target[m];

            // Establish Invariant: v* (<v)* (>v)* v*
            int a = fromIndex, b = a, c = fromIndex + length - 1, d = c;
            while (true)
            {
                while (b <= c && comparator.Compare(target[b], v) <= 0)
                {
                    if (comparator.Compare(target[b], v) == 0)
                        Swap(target, a++, b, cosort);
                    b++;
                }

                while (c >= b && comparator.Compare(target[c], v) >= 0)
                {
                    if (comparator.Compare(target[c], v) == 0)
                        Swap(target, c, d--, cosort);
                    c--;
                }

                if (b > c)
                    break;
                Swap(target, b++, c--, cosort);
            }


            // Swap partition elements back to middle
            int s, n = fromIndex + length;
            s = Math.Min(a - fromIndex, b - a);
            Vecswap(target, fromIndex, b - s, s, cosort);
            s = Math.Min(d - c, n - d - 1);
            Vecswap(target, b, n - s, s, cosort);

            // Recursively quickSort non-partition-elements
            if ((s = b - a) > 1)
                QuickSort1(target, fromIndex, s, cosort, comparator);
            if ((s = d - c) > 1)
                QuickSort1(target, n - s, s, cosort, comparator);
        }

        /// <summary>
        /// Returns the index of the median of the three indexed integers.
        /// </summary>
        private static int Med3(int[] x, int a, int b, int c, IntComparator comparator)
        {
            return (comparator.Compare(x[a], x[b]) < 0 ? (comparator.Compare(x[b], x[c]) < 0 ? b : comparator.Compare(x[a], x[c]) < 0 ? c : a) : (comparator.Compare(x[b], x[c]) > 0 ? b : comparator.Compare(x[a], x[c]) > 0 ? c : a));
        }

        private static void Vecswap(int[] x, int a, int b, int n)
        {
            for (int i = 0; i < n; i++, a++, b++)
                Swap(x, a, b);
        }

        ////////////////////////////////////// UTILS ///////////////////////////////////////////////////////////
        /// <summary>
        /// Check that fromIndex and toIndex are in range, and throw an appropriate exception if they aren't.
        /// </summary>
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