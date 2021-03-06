using System;
using System.Collections;
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities.IO;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cms
{
    /**
    * General class for generating a CMS enveloped-data message stream.
    * <p>
    * A simple example of usage.
    * <pre>
    *      CmsEnvelopedDataStreamGenerator edGen = new CmsEnvelopedDataStreamGenerator();
    *
    *      edGen.AddKeyTransRecipient(cert);
    *
    *      MemoryStream  bOut = new MemoryStream();
    *
    *      Stream out = edGen.Open(
    *                              bOut, CMSEnvelopedDataGenerator.AES128_CBC);*
    *      out.Write(data);
    *
    *      out.Close();
    * </pre>
	* </p>
    */
    public class CmsEnvelopedDataStreamGenerator
		: CmsEnvelopedGenerator
    {
        private object	_originatorInfo = null;
        private object	_unprotectedAttributes = null;
        private int		_bufferSize;
		private bool	_berEncodeRecipientSet;

		public CmsEnvelopedDataStreamGenerator()
        {
        }

		/// <summary>Set the underlying string size for encapsulated data.</summary>
		/// <param name="bufferSize">Length of octet strings to buffer the data.</param>
        public void SetBufferSize(
            int bufferSize)
        {
            _bufferSize = bufferSize;
        }

		/// <summary>Use a BER Set to store the recipient information.</summary>
		public void SetBerEncodeRecipients(
			bool berEncodeRecipientSet)
		{
			_berEncodeRecipientSet = berEncodeRecipientSet;
		}

		private DerInteger Version
        {
			get
			{
				int version = (_originatorInfo != null || _unprotectedAttributes != null)
					?	2
					:	0;

				return new DerInteger(version);
			}
        }

		/// <summary>
		/// Generate an enveloped object that contains an CMS Enveloped Data
		/// object using the passed in key generator.
		/// </summary>
        private Stream Open(
            Stream				outStream,
            string				encryptionOid,
            CipherKeyGenerator	keyGen)
        {
			Asn1Encodable asn1Params = null;

			byte[] encKeyBytes = keyGen.GenerateKey();
			KeyParameter encKey = ParameterUtilities.CreateKeyParameter(encryptionOid, encKeyBytes);

			try
			{
				if (encryptionOid.Equals(RC2Cbc))
				{
					// mix in a bit extra...
					rand.SetSeed(DateTime.Now.Ticks);

					byte[] iv = rand.GenerateSeed(8);

					// TODO Is this detailed repeat of Java version really necessary?
					int effKeyBits = encKeyBytes.Length * 8;
					int parameterVersion;

					if (effKeyBits < 256)
					{
						parameterVersion = rc2Table[effKeyBits];
					}
					else
					{
						parameterVersion = effKeyBits;
					}

					asn1Params = new RC2CbcParameter(parameterVersion, iv);
				}
				else
				{
					asn1Params = ParameterUtilities.GenerateParameters(encryptionOid, rand);
				}
			}
			catch (SecurityUtilityException)
			{
				// No problem... no parameters generated
			}


			Asn1EncodableVector recipientInfos = new Asn1EncodableVector();

            foreach (RecipientInf recipient in recipientInfs)
            {
                try
                {
                    recipientInfos.Add(recipient.ToRecipientInfo(encKey));
                }
                catch (IOException e)
                {
                    throw new CmsException("encoding error.", e);
                }
                catch (InvalidKeyException e)
                {
                    throw new CmsException("key inappropriate for algorithm.", e);
                }
                catch (GeneralSecurityException e)
                {
                    throw new CmsException("error making encrypted content.", e);
                }
            }

			return Open(outStream, encryptionOid, encKey, asn1Params, recipientInfos);
		}

        protected Stream Open(
            Stream					outStream,
            string					encryptionOid,
            KeyParameter			encKey,
			Asn1Encodable			asn1Params,
			Asn1EncodableVector		recipientInfos)
        {
			Asn1Object asn1Object;
			ICipherParameters cipherParameters;

			if (asn1Params != null)
			{
				asn1Object = asn1Params.ToAsn1Object();
				cipherParameters = ParameterUtilities.GetCipherParameters(
					encryptionOid, encKey, asn1Object);
			}
			else
			{
				asn1Object = DerNull.Instance;
				cipherParameters = encKey;
			}


            try
            {
				AlgorithmIdentifier encAlgId = new AlgorithmIdentifier(
					new DerObjectIdentifier(encryptionOid),
					asn1Object);

				//
                // ContentInfo
                //
                BerSequenceGenerator cGen = new BerSequenceGenerator(outStream);

				cGen.AddObject(CmsObjectIdentifiers.EnvelopedData);

				//
                // Encrypted Data
                //
                BerSequenceGenerator envGen = new BerSequenceGenerator(
					cGen.GetRawOutputStream(), 0, true);

                envGen.AddObject(this.Version);

				DerSet derSet = _berEncodeRecipientSet
					?	new BerSet(recipientInfos)
					:	new DerSet(recipientInfos);

				byte[] derSetEncoding = derSet.GetEncoded();

				envGen.GetRawOutputStream().Write(derSetEncoding, 0, derSetEncoding.Length);

				IBufferedCipher cipher = CipherUtilities.GetCipher(encryptionOid);

				cipher.Init(true, cipherParameters);

				BerSequenceGenerator eiGen = new BerSequenceGenerator(
					envGen.GetRawOutputStream());

				eiGen.AddObject(PkcsObjectIdentifiers.Data);

				byte[] tmp = encAlgId.GetEncoded();
				eiGen.GetRawOutputStream().Write(tmp, 0, tmp.Length);

				BerOctetStringGenerator octGen = new BerOctetStringGenerator(
					eiGen.GetRawOutputStream(), 0, false);

				Stream octetOutputStream = _bufferSize != 0
					?	octGen.GetOctetOutputStream(new byte[_bufferSize])
					:	octGen.GetOctetOutputStream();

				CipherStream cOut = new CipherStream(octetOutputStream, null, cipher);

				return new CmsEnvelopedDataOutputStream(cOut, cGen, envGen, eiGen);
            }
			catch (SecurityUtilityException e)
			{
				throw new CmsException("couldn't create cipher.", e);
			}
			catch (InvalidKeyException e)
            {
                throw new CmsException("key invalid in message.", e);
            }
            catch (IOException e)
            {
                throw new CmsException("exception decoding algorithm parameters.", e);
            }
        }

		/**
        * generate an enveloped object that contains an CMS Enveloped Data object
        * @throws IOException
        */
        public Stream Open(
            Stream outStream,
            string encryptionOid)
        {
            try
            {
				CipherKeyGenerator keyGen = GeneratorUtilities.GetKeyGenerator(encryptionOid);

				return Open(outStream, encryptionOid, keyGen);
			}
            catch (SecurityUtilityException e)
            {
                throw new CmsException("can't find key generation algorithm.", e);
            }
        }

		/**
        * generate an enveloped object that contains an CMS Enveloped Data object
        * @throws IOException
        */
        public Stream Open(
            Stream	outStream,
            string	encryptionOid,
            int		keySize)
        {
            try
            {
				CipherKeyGenerator keyGen = GeneratorUtilities.GetKeyGenerator(encryptionOid);

				keyGen.Init(new KeyGenerationParameters(rand, keySize));

				return Open(outStream, encryptionOid, keyGen);
            }
            catch (SecurityUtilityException e)
            {
                throw new CmsException("can't find key generation algorithm.", e);
            }
        }

		private class CmsEnvelopedDataOutputStream
            : BaseOutputStream
        {
            private CipherStream			_out;
            private BerSequenceGenerator	_cGen;
            private BerSequenceGenerator	_envGen;
            private BerSequenceGenerator	_eiGen;

			public CmsEnvelopedDataOutputStream(
                CipherStream				outStream,
                BerSequenceGenerator	cGen,
                BerSequenceGenerator	envGen,
                BerSequenceGenerator	eiGen)
            {
                _out = outStream;
                _cGen = cGen;
                _envGen = envGen;
                _eiGen = eiGen;
            }

			public override void WriteByte(
                byte b)
            {
                _out.WriteByte(b);
            }

			public override void Write(
                byte[]	bytes,
                int		off,
                int		len)
            {
                _out.Write(bytes, off, len);
            }

			public override void Close()
            {
                _out.Close();
                _eiGen.Close();

				// [TODO] unprotected attributes go here

				_envGen.Close();
                _cGen.Close();
				base.Close();
			}
        }
    }
}
