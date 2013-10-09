using System;
// MASC 20070307.
// CF does not support serialization yet
#if !NETCF
using System.Runtime.Serialization;
#endif

namespace Org.BouncyCastle.Crypto
{
    /**
     * this exception is thrown if a buffer that is meant to have output
     * copied into it turns out to be too short, or if we've been given
     * insufficient input. In general this exception will Get thrown rather
     * than an ArrayOutOfBounds exception.
     */
    [SerializableAttribute]
    public class DataLengthException
		: CryptoException
	{
        /**
        * base constructor.
		*/
        public DataLengthException()
        {
        }

		/**
         * create a DataLengthException with the given message.
         *
         * @param message the message to be carried with the exception.
         */
        public DataLengthException(
            string message)
			: base(message)
        {
		}

		public DataLengthException(
            string		message,
            Exception	exception)
			: base(message, exception)
        {
        }

#if !NETCF
		protected DataLengthException(
            SerializationInfo	info,
            StreamingContext	context)
			: base(info, context)
		{
		}
#endif
	}
}
