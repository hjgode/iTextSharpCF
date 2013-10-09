using System;
// MASC 20070307.
// CF does not support serialization yet
#if !NETCF
using System.Runtime.Serialization;
#endif

namespace Org.BouncyCastle.Crypto
{
    [SerializableAttribute]
    public abstract class CryptoException
		: Exception
    {
        protected CryptoException()
        {
        }

		protected CryptoException(
            string message)
			: base(message)
        {
        }

		protected CryptoException(
            string		message,
            Exception	exception)
			: base(message, exception)
        {
        }

#if !NETCF
		protected CryptoException(
            SerializationInfo	info,
            StreamingContext	context)
			: base(info, context)
        {
        }
#endif
    }
}
