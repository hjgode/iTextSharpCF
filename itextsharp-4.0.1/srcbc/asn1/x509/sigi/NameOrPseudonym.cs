using System;
using System.Collections;

using Org.BouncyCastle.Asn1.X500;

namespace Org.BouncyCastle.Asn1.X509.SigI
{
	/**
	* Structure for a name or pseudonym.
	* 
	* <pre>
	*       NameOrPseudonym ::= CHOICE {
	*     	   surAndGivenName SEQUENCE {
	*     	     surName DirectoryString,
	*     	     givenName SEQUENCE OF DirectoryString 
	*         },
	*     	   pseudonym DirectoryString 
	*       }
	* </pre>
	* 
	* @see org.bouncycastle.asn1.x509.sigi.PersonalData
	* 
	*/
	public class NameOrPseudonym
		: Asn1Encodable
		//, Asn1Choice
	{
		private readonly DirectoryString	pseudonym;
		private readonly DirectoryString	surname;
		private readonly ArrayList			givenName;

		public static NameOrPseudonym GetInstance(
			object obj)
		{
			if (obj == null || obj is NameOrPseudonym)
			{
				return (NameOrPseudonym)obj;
			}

			if (obj is IAsn1String)
			{
				return new NameOrPseudonym(DirectoryString.GetInstance(obj));
			}

			if (obj is Asn1Sequence)
			{
				return new NameOrPseudonym((Asn1Sequence) obj);
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		/**
		* Constructor from DERString.
		* <p/>
		* The sequence is of type NameOrPseudonym:
		* <p/>
		* <pre>
		*       NameOrPseudonym ::= CHOICE {
		*     	   surAndGivenName SEQUENCE {
		*     	     surName DirectoryString,
		*     	     givenName SEQUENCE OF DirectoryString
		*         },
		*     	   pseudonym DirectoryString
		*       }
		* </pre>
		* @param pseudonym pseudonym value to use.
		*/
		public NameOrPseudonym(
			DirectoryString pseudonym)
		{
			this.pseudonym = pseudonym;
		}

		/**
		* Constructor from Asn1Sequence.
		* <p/>
		* The sequence is of type NameOrPseudonym:
		* <p/>
		* <pre>
		*       NameOrPseudonym ::= CHOICE {
		*     	   surAndGivenName SEQUENCE {
		*     	     surName DirectoryString,
		*     	     givenName SEQUENCE OF DirectoryString
		*         },
		*     	   pseudonym DirectoryString
		*       }
		* </pre>
		*
		* @param seq The ASN.1 sequence.
		*/
		private NameOrPseudonym(
			Asn1Sequence seq)
		{
			if (seq.Count != 2)
				throw new ArgumentException("Bad sequence size: " + seq.Count);

			if (!(seq[0] is IAsn1String))
				throw new ArgumentException("Bad object encountered: " + seq[0].GetType().Name);

			surname = DirectoryString.GetInstance(seq[0]);
			givenName = new ArrayList();

			Asn1Sequence s = Asn1Sequence.GetInstance(seq[1]);

			foreach (object o in s)
			{
				if (!(o is IAsn1String))
					throw new ArgumentException("Bad object encountered: " + o.GetType().Name);

				givenName.Add(DirectoryString.GetInstance(o));
			}
		}

		/**
		* Constructor from a given details.
		*
		* @param pseudonym The pseudonym.
		*/
		public NameOrPseudonym(
			string pseudonym)
		{
			this.pseudonym = new DirectoryString(pseudonym);
		}

		/**
		* Constructor from a given details.
		*
		* @param surname   The surname.
		* @param givenName An IEnumerable of strings of the given name
		*/
		public NameOrPseudonym(
			string		surname,
			IEnumerable	givenName)
		{
			this.surname = new DirectoryString(surname);
			this.givenName = new ArrayList();

			foreach (string s in givenName)
			{
				this.givenName.Add(new DirectoryString(s));
			}
		}

		/**
		* Produce an object suitable for an Asn1OutputStream.
		* <p/>
		* Returns:
		* <p/>
		* <pre>
		*       NameOrPseudonym ::= CHOICE {
		*     	   surAndGivenName SEQUENCE {
		*     	     surName DirectoryString,
		*     	     givenName SEQUENCE OF DirectoryString
		*         },
		*     	   pseudonym DirectoryString
		*       }
		* </pre>
		*
		* @return an Asn1Object
		*/
		public override Asn1Object ToAsn1Object()
		{
			if (pseudonym != null)
			{
				return pseudonym.ToAsn1Object();
			}

			Asn1EncodableVector vec = new Asn1EncodableVector();
			foreach (object obj in givenName)
			{
				vec.Add(new DerUtf8String(obj.ToString()));
			}

			return new DerSequence(surname, new DerSequence(vec));
		}
	}
}
