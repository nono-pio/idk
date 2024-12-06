using Java;
using Java.Util.Function;
using Java.Util.Stream;
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
    /// A simple list wrapper
    /// </summary>
    public class ListWrapper<Poly> : AbstractList<Poly>
    {
        /// <summary>
        /// Inner list
        /// </summary>
        public readonly IList<Poly> list;
        /// <summary>
        /// Inner list
        /// </summary>
        public ListWrapper(IList<Poly> list)
        {
            this.list = list;
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override bool IsEmpty()
        {
            return list.IsEmpty();
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override bool Contains(object o)
        {
            return list.Contains(o);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override Object[] ToArray()
        {
            return list.ToArray();
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override T[] ToArray<T>(T[] a)
        {
            return list.ToArray(a);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override bool Remove(object o)
        {
            return list.Remove(o);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override bool ContainsAll(Collection<TWildcardTodo> c)
        {
            return list.ContainsAll(c);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override bool AddAll(Collection<TWildcardTodoPoly> c)
        {
            return list.AddAll(c);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override bool RemoveAll(Collection<TWildcardTodo> c)
        {
            return list.RemoveAll(c);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override bool RetainAll(Collection<TWildcardTodo> c)
        {
            return list.RetainAll(c);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override void ReplaceAll(UnaryOperator<Poly> @operator)
        {
            list.ReplaceAll(@operator);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override void Sort(Comparator<TWildcardTodoPoly> c)
        {
            list.Sort(c);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override Spliterator<Poly> Spliterator()
        {
            return list.Spliterator();
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override bool RemoveIf(Predicate<TWildcardTodoPoly> filter)
        {
            return list.RemoveIf(filter);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override Stream<Poly> Stream()
        {
            return list.Stream();
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override Stream<Poly> ParallelStream()
        {
            return list.ParallelStream();
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override void ForEach(Consumer<TWildcardTodoPoly> action)
        {
            list.ForEach(action);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override bool Add(Poly poly)
        {
            return list.Add(poly);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override Poly Set(int index, Poly element)
        {
            return list[index] = element;
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override void Add(int index, Poly element)
        {
            list.Add(index, element);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override Poly Remove(int index)
        {
            return list.Remove(index);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override int IndexOf(object o)
        {
            return list.IndexOf(o);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override int LastIndexOf(object o)
        {
            return list.LastIndexOf(o);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override void Clear()
        {
            list.Clear();
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override bool AddAll(int index, Collection<TWildcardTodoPoly> c)
        {
            return list.AddAll(index, c);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override IEnumerator<Poly> Iterator()
        {
            return list.Iterator();
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override ListIterator<Poly> ListIterator()
        {
            return list.ListIterator();
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override ListIterator<Poly> ListIterator(int index)
        {
            return list.ListIterator(index);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override IList<Poly> SubList(int fromIndex, int toIndex)
        {
            return list.SubList(fromIndex, toIndex);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override bool Equals(object o)
        {
            return list.Equals(o);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override int GetHashCode()
        {
            return list.GetHashCode();
        }

        /// <summary>
        /// Inner list
        /// </summary>
        protected override void RemoveRange(int fromIndex, int toIndex)
        {
            base.RemoveRange(fromIndex, toIndex);
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override Poly Get(int index)
        {
            return list[index];
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override int Size()
        {
            return list.Count;
        }

        /// <summary>
        /// Inner list
        /// </summary>
        public override string ToString()
        {
            return list.ToString();
        }
    }
}