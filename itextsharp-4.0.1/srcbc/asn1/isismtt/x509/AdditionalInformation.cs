using System;

using Org.BouncyCastle.Asn1.X500;

namespace Org.BouncyCastle.Asn1.IsisMtt.X509
{
	/**
	* Some other information of non-restrictive nature regarding the usage of this
	* certificate.
	* 
	* <pre>
	*    AdditionalInformationSyntax ::= DirectoryString (SIZE(1..2048))
	* </pre>
	*/
	public class AdditionalInformation
		: Asn1Encodable
	{
		private readonly DirectoryString information;

		public static AdditionalInformation GetInstance(
			object obj)
		{
			if (obj == null || obj is AdditionalInformation)
			{
				return (AdditionalInformation) obj;
			}

			if (obj is IAsn1String)
			{
				return new AdditionalInformation(DirectoryString.GetInstance(obj));
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		private AdditionalInformation(
			DirectoryString information)
		{
			this.information = information;
		}

		/**
		* Constructor from a given details.
		*
		* @param information The describtion of the information.
		*/
		public AdditionalInformation(
			string information)
		{
			this.information = new DirectoryString(information);
		}

		/**
		* Produce an object suitable for an Asn1OutputStream.
		* <p/>
		* Returns:
		* <p/>
		* <pre>
		*   AdditionalInformationSyntax ::= DirectoryString (SIZE(1..2048))
		* </pre>
		*
		* @return an Asn1Object
		*/
		public override Asn1Object ToAsn1Object()
		{
			return information.ToAsn1Object();
		}
	}
}
