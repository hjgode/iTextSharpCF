using System;
using System.Collections;

using Org.BouncyCastle.Asn1.X500;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.X509.SigI
{
	/**
	* Contains personal data for the otherName field in the subjectAltNames
	* extension.
	* <p/>
	* <pre>
	*     PersonalData ::= SEQUENCE {
	*       nameOrPseudonym NameOrPseudonym,
	*       nameDistinguisher [0] INTEGER OPTIONAL,
	*       dateOfBirth [1] GeneralizedTime OPTIONAL,
	*       placeOfBirth [2] DirectoryString OPTIONAL,
	*       gender [3] PrintableString OPTIONAL,
	*       postalAddress [4] DirectoryString OPTIONAL
	*       }
	* </pre>
	*
	* @see org.bouncycastle.asn1.x509.sigi.NameOrPseudonym
	* @see org.bouncycastle.asn1.x509.sigi.SigIObjectIdentifiers
	*/
	public class PersonalData
		: Asn1Encodable
	{
		private readonly NameOrPseudonym	nameOrPseudonym;
		private readonly BigInteger			nameDistiguisher;
		private readonly DerGeneralizedTime	dateOfBirth;
		private readonly DirectoryString	placeOfBirth;
		private readonly DerPrintableString	gender;
		private readonly DirectoryString	postalAddress;

		public static PersonalData GetInstance(
			object obj)
		{
			if (obj == null || obj is PersonalData)
			{
				return (PersonalData) obj;
			}

			if (obj is Asn1Sequence)
			{
				return new PersonalData((Asn1Sequence) obj);
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		/**
		* Constructor from Asn1Sequence.
		* <p/>
		* The sequence is of type NameOrPseudonym:
		* <p/>
		* <pre>
		*     PersonalData ::= SEQUENCE {
		*       nameOrPseudonym NameOrPseudonym,
		*       nameDistinguisher [0] INTEGER OPTIONAL,
		*       dateOfBirth [1] GeneralizedTime OPTIONAL,
		*       placeOfBirth [2] DirectoryString OPTIONAL,
		*       gender [3] PrintableString OPTIONAL,
		*       postalAddress [4] DirectoryString OPTIONAL
		*       }
		* </pre>
		*
		* @param seq The ASN.1 sequence.
		*/
		private PersonalData(
			Asn1Sequence seq)
		{
			if (seq.Count < 1)
				throw new ArgumentException("Bad sequence size: " + seq.Count);

			IEnumerator e = seq.GetEnumerator();
			e.MoveNext();

			nameOrPseudonym = NameOrPseudonym.GetInstance(e.Current);

			while (e.MoveNext())
			{
				Asn1TaggedObject o = Asn1TaggedObject.GetInstance(e.Current);
				int tag = o.TagNo;
				switch (tag)
				{
					case 0:
						nameDistiguisher = DerInteger.GetInstance(o, false).Value;
						break;
					case 1:
						dateOfBirth = DerGeneralizedTime.GetInstance(o, false);
						break;
					case 2:
						placeOfBirth = DirectoryString.GetInstance(o, true);
						break;
					case 3:
						gender = DerPrintableString.GetInstance(o, false);
						break;
					case 4:
						postalAddress = DirectoryString.GetInstance(o, true);
						break;
					default:
						throw new ArgumentException("Bad tag number: " + o.TagNo);
				}
			}
		}

		/**
		* Constructor from a given details.
		*
		* @param nameOrPseudonym  Name or pseudonym.
		* @param nameDistiguisher Name distinguisher.
		* @param dateOfBirth      Date of birth.
		* @param placeOfBirth     Place of birth.
		* @param gender           Gender.
		* @param postalAddress    Postal Address.
		*/
		public PersonalData(
			NameOrPseudonym		nameOrPseudonym,
			BigInteger			nameDistiguisher,
			DerGeneralizedTime	dateOfBirth,
			string				placeOfBirth,
			string				gender,
			string				postalAddress)
		{
			this.nameOrPseudonym = nameOrPseudonym;
			this.dateOfBirth = dateOfBirth;
			this.gender = new DerPrintableString(gender, true);
			this.nameDistiguisher = nameDistiguisher;
			this.postalAddress = new DirectoryString(postalAddress);
			this.placeOfBirth = new DirectoryString(placeOfBirth);
		}

		/**
		* Produce an object suitable for an Asn1OutputStream.
		* <p/>
		* Returns:
		* <p/>
		* <pre>
		*     PersonalData ::= SEQUENCE {
		*       nameOrPseudonym NameOrPseudonym,
		*       nameDistinguisher [0] INTEGER OPTIONAL,
		*       dateOfBirth [1] GeneralizedTime OPTIONAL,
		*       placeOfBirth [2] DirectoryString OPTIONAL,
		*       gender [3] PrintableString OPTIONAL,
		*       postalAddress [4] DirectoryString OPTIONAL
		*       }
		* </pre>
		*
		* @return an Asn1Object
		*/
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector vec = new Asn1EncodableVector();
			vec.Add(nameOrPseudonym);
			if (nameDistiguisher != null)
			{
				vec.Add(new DerTaggedObject(false, 0, new DerInteger(nameDistiguisher)));
			}
			if (dateOfBirth != null)
			{
				vec.Add(new DerTaggedObject(false, 1, dateOfBirth));
			}
			if (placeOfBirth != null)
			{
				vec.Add(new DerTaggedObject(true, 2, placeOfBirth));
			}
			if (gender != null)
			{
				vec.Add(new DerTaggedObject(false, 3, gender));
			}
			if (postalAddress != null)
			{
				vec.Add(new DerTaggedObject(true, 4, postalAddress));
			}
			return new DerSequence(vec);
		}
	}
}
