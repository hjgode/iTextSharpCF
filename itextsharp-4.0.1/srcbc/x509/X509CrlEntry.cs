using System;
using System.Collections;
using System.IO;
using System.Text;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.X509.Extension;

namespace Org.BouncyCastle.X509
{
	/**
	 * The following extensions are listed in RFC 2459 as relevant to CRL Entries
	 *
	 * ReasonCode Hode Instruction Code Invalidity Date Certificate Issuer
	 * (critical)
	 */
	public class X509CrlEntry
		: X509ExtensionBase
	{
		private CrlEntry c;
		private bool isIndirect;
		private X509Name previousCertificateIssuer;

		public X509CrlEntry(
			CrlEntry c)
		{
			this.c = c;
		}

		/**
		* Constructor for CRLEntries of indirect CRLs. If <code>isIndirect</code>
		* is <code>false</code> {@link #getCertificateIssuer()} will always
		* return <code>null</code>, <code>previousCertificateIssuer</code> is
		* ignored. If this <code>isIndirect</code> is specified and this CrlEntry
		* has no certificate issuer CRL entry extension
		* <code>previousCertificateIssuer</code> is returned by
		* {@link #getCertificateIssuer()}.
		*
		* @param c
		*            TbsCertificateList.CrlEntry object.
		* @param isIndirect
		*            <code>true</code> if the corresponding CRL is a indirect
		*            CRL.
		* @param previousCertificateIssuer
		*            Certificate issuer of the previous CrlEntry.
		*/
		public X509CrlEntry(
			CrlEntry		c,
			bool			isIndirect,
			X509Name		previousCertificateIssuer)
		{
			this.c = c;
			this.isIndirect = isIndirect;
			this.previousCertificateIssuer = previousCertificateIssuer;
		}

		public X509Name GetCertificateIssuer()
		{
			if (!isIndirect)
			{
				return null;
			}

			Asn1OctetString ext = GetExtensionValue(X509Extensions.CertificateIssuer);
			if (ext == null)
			{
				return previousCertificateIssuer;
			}

			try
			{
				GeneralName[] names = GeneralNames.GetInstance(
					X509ExtensionUtilities.FromExtensionValue(ext)).GetNames();

				for (int i = 0; i < names.Length; i++)
				{
					if (names[i].TagNo == GeneralName.DirectoryName)
					{
						return X509Name.GetInstance(names[i].Name);
					}
				}
			}
			catch (Exception)
			{
			}

			return null;
		}

		protected override X509Extensions GetX509Extensions()
		{
			return c.Extensions;
		}

		public byte[] GetEncoded()
		{
			try
			{
				return c.GetDerEncoded();
			}
			catch (Exception e)
			{
				throw new CrlException(e.ToString());
			}
		}

		public BigInteger SerialNumber
		{
			get { return c.UserCertificate.Value; }
		}

		public DateTime RevocationDate
		{
			get { return c.RevocationDate.ToDateTime(); }
		}

		public bool HasExtensions
		{
			get { return c.Extensions != null; }
		}

		public override string ToString()
		{
			StringBuilder buf = new StringBuilder();
#if !NETCF
			string nl = Environment.NewLine;
#else
			string nl = EnvironmentEx.NewLine;
#endif

			buf.Append("      userCertificate: ").Append(this.SerialNumber).Append(nl);
			buf.Append("       revocationDate: ").Append(this.RevocationDate).Append(nl);

			X509Extensions extensions = c.Extensions;

			if (extensions != null)
			{
				IEnumerator e = extensions.ExtensionOids.GetEnumerator();
				if (e.MoveNext())
				{
					buf.Append("   crlEntryExtensions:").Append(nl);

					do
					{
						DerObjectIdentifier oid = (DerObjectIdentifier)e.Current;
						X509Extension ext = extensions.GetExtension(oid);
						buf.Append(ext);
					}
					while (e.MoveNext());
				}
			}

			return buf.ToString();
		}
	}
}
