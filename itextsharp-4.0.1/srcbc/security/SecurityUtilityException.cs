using System;
// MASC 20070307.
// CF does not support serialization yet
#if !NETCF
using System.Runtime.Serialization;
#endif

namespace Org.BouncyCastle.Security
{
    [SerializableAttribute]
    public class SecurityUtilityException : Exception
    {
        /**
            * base constructor.
            */
        public SecurityUtilityException()
        {
        }

        /**
         * create a SecurityUtilityException with the given message.
         *
         * @param message the message to be carried with the exception.
         */
        public SecurityUtilityException(
            string  message) : base(message)
        {
        }

		public SecurityUtilityException(
            string message,
            Exception exception) : base(message, exception)
        {

        }

#if !NETCF
        protected SecurityUtilityException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
#endif

    }

}
