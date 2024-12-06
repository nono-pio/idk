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
    /// </summary>
    public interface SerializableFunction<T, R> : Serializable
    {
        R Apply(T t);
        SerializableFunction<T, V> AndThen<V>(SerializableFunction<TWildcardTodoR, TWildcardTodoV> after)
        {
            return (T t) => after.Apply(Apply(t));
        }

        static SerializableFunction<T, T> Identity<T>()
        {
            return (t) => t;
        }
    }
}