using Java;
using Java.Util;
using Java.Util.Zip;
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
    public sealed class ZipUtil
    {
        private ZipUtil()
        {
        }

        /// <summary>
        /// Compress object to a string
        /// </summary>
        public static string Compress(object @object)
        {
            try
            {
                using (ByteArrayOutputStream outBytes = new ByteArrayOutputStream())
                using (GZIPOutputStream zipped = new GZIPOutputStream(outBytes))
                using (ObjectOutputStream outSer = new ObjectOutputStream(zipped))
                {
                    outSer.WriteObject(@object);
                    zipped.Finish();
                    return Base64.GetEncoder().EncodeToString(outBytes.ToByteArray());
                }
            }
            catch (IOException e)
            {
                throw new Exception(e);
            }
        }

        /// <summary>
        /// Decompress object from its string code obtained via {@link ZipUtil#compress(Object)}
        /// </summary>
        public static T Uncompress<T>(string @object)
        {
            byte[] decoded = Base64.GetDecoder().Decode(@object);
            try
            {
                using (ByteArrayInputStream inBytes = new ByteArrayInputStream(decoded))
                using (ObjectInputStream inSer = new ObjectInputStream(new GZIPInputStream(inBytes)))
                {
                    return (T)inSer.ReadObject();
                }
            }
            catch (IOException e)
            {
                throw new Exception(e);
            }
            catch (ClassNotFoundException e)
            {
                throw new Exception(e);
            }
        }
    }
}