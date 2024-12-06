using Org.Apache.Commons.Math3.Exception;
using Org.Apache.Commons.Math3.Stat.Descriptive;
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
    /// <remarks>@since1.0</remarks>
    public sealed class TimeUnits
    {
        private TimeUnits()
        {
        }

        public static string NanosecondsToString(long nano)
        {
            string pf = "ns";
            if (nano / 1000 > 1)
            {
                pf = "us";
                nano /= 1000;
            }

            if (nano / 1000 > 1)
            {
                pf = "ms";
                nano /= 1000;
            }

            if (nano / 1000 > 1)
            {
                pf = "s";
                nano /= 1000;
            }

            return nano + pf;
        }

        public static string StatisticsNanotime(DescriptiveStatistics stats)
        {
            return StatisticsNanotime(stats, false);
        }

        public static string StatisticsNanotime(DescriptiveStatistics stats, bool median)
        {
            return NanosecondsToString((long)(median ? stats.GetPercentile(0.5) : stats.GetMean())) + " ± " + NanosecondsToString((long)stats.GetStandardDeviation());
        }

        private static string Ns(double nano)
        {
            return NanosecondsToString((long)nano);
        }

        public static string StatisticsNanotimeFull(DescriptiveStatistics stats)
        {
            StringBuilder outBuffer = new StringBuilder();
            string endl = "\n";
            outBuffer.Append("DescriptiveStatistics:").Append(endl);
            outBuffer.Append("n: ").Append(stats.GetN()).Append(endl);
            outBuffer.Append("min: ").Append(Ns(stats.GetMin())).Append(endl);
            outBuffer.Append("max: ").Append(Ns(stats.GetMax())).Append(endl);
            outBuffer.Append("mean: ").Append(Ns(stats.GetMean())).Append(endl);
            outBuffer.Append("std dev: ").Append(Ns(stats.GetStandardDeviation())).Append(endl);
            try
            {

                // No catch for MIAE because actual parameter is valid below
                outBuffer.Append("median: ").Append(Ns(stats.GetPercentile(50))).Append(endl);
            }
            catch (MathIllegalStateException ex)
            {
                outBuffer.Append("median: unavailable").Append(endl);
            }

            outBuffer.Append("skewness: ").Append(Ns(stats.GetSkewness())).Append(endl);
            outBuffer.Append("kurtosis: ").Append(Ns(stats.GetKurtosis())).Append(endl);
            return outBuffer.ToString();
        }
    }
}