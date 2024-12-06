using Java.Util;
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
    public class TimeConstrained
    {
        /// <summary>
        /// Runs lambda, stopping after specified number of milliseconds
        /// </summary>
        /// <param name="lambda">code block</param>
        /// <param name="millis">time constrained in milliseconds</param>
        /// <param name="failExpr">returns this if the time constraint is not met</param>
        public static T TimeConstrained0<T>(Callable<T> lambda, long millis, T failExpr)
        {
            ExecutorService executor = Executors.NewSingleThreadExecutor();
            FutureTask<T> task = new FutureTask(lambda);
            executor.Execute(task);
            try
            {
                return task.Get(millis, TimeUnit.MILLISECONDS);
            }
            catch (TimeoutException e)
            {
                return failExpr;
            }
            finally
            {
                task.Cancel(true);
                executor.Shutdown();
            }
        }

        /// <summary>
        /// Runs lambda, stopping after specified number of milliseconds
        /// </summary>
        /// <param name="lambda">code block</param>
        /// <param name="millis">time constrained in milliseconds</param>
        /// <param name="failExpr">returns this if the time constraint is not met</param>
        public static T TimeConstrained<T>(Callable<T> lambda, long millis, T failExpr)
        {
            try
            {
                return TimeConstrained0(lambda, millis, failExpr);
            }
            catch (InterruptedException e)
            {
                throw new Exception(e);
            }
            catch (ExecutionException e)
            {
                throw new Exception(e);
            }
        }

        /// <summary>
        /// Runs lambda, stopping after specified number of milliseconds
        /// </summary>
        /// <param name="lambda">code block</param>
        /// <param name="millis">time constrained in milliseconds</param>
        public static T TimeConstrained<T>(Callable<T> lambda, long millis)
        {
            return TimeConstrained0(lambda, millis, null);
        }
    }
}