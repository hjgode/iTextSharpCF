using System;
using System.Collections;
using System.IO;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	/// <remarks>General class to handle a PGP secret key object.</remarks>
    public class PgpSecretKey
    {
        private SecretKeyPacket secret;
        private TrustPacket     trust;
        private ArrayList       keySigs;
        private ArrayList       ids;
        private ArrayList       idTrusts;
        private ArrayList       idSigs;
        private PgpPublicKey    pub;
        private ArrayList       subSigs;

		/// <summary>Copy constructor - master key.</summary>
        private PgpSecretKey(
            SecretKeyPacket secret,
            TrustPacket     trust,
            ArrayList       keySigs,
            ArrayList       ids,
            ArrayList       idTrusts,
            ArrayList       idSigs,
            PgpPublicKey    pub)
        {
            this.secret = secret;
            this.trust = trust;
            this.keySigs = keySigs;
            this.ids = ids;
            this.idTrusts = idTrusts;
            this.idSigs = idSigs;
            this.pub = pub;
        }

		/// <summary>Copy constructor - subkey.</summary>
        private PgpSecretKey(
            SecretKeyPacket secret,
            TrustPacket     trust,
            ArrayList       subSigs,
            PgpPublicKey    pub)
        {
            this.secret = secret;
            this.trust = trust;
            this.subSigs = subSigs;
            this.pub = pub;
        }

		internal PgpSecretKey(
            SecretKeyPacket	secret,
            TrustPacket		trust,
            ArrayList		keySigs,
            ArrayList		ids,
            ArrayList		idTrusts,
            ArrayList		idSigs)
        {
            this.secret = secret;
            this.trust = trust;
            this.keySigs = keySigs;
            this.ids = ids;
            this.idTrusts = idTrusts;
            this.idSigs = idSigs;
            this.pub = new PgpPublicKey(secret.PublicKeyPacket, trust, keySigs, ids, idTrusts, idSigs);
        }

		internal PgpSecretKey(
            SecretKeyPacket	secret,
            TrustPacket		trust,
            ArrayList		subSigs)
        {
            this.secret = secret;
            this.trust = trust;
            this.subSigs = subSigs;
            this.pub = new PgpPublicKey(secret.PublicKeyPacket, trust, subSigs);
        }

		/// <summary>Create a subkey</summary>
        internal PgpSecretKey(
            PgpKeyPair					keyPair,
            TrustPacket					trust,
            ArrayList					subSigs,
            SymmetricKeyAlgorithmTag	encAlgorithm,
            char[]						passPhrase,
			bool						useSHA1,
			SecureRandom				rand)
            : this(keyPair, encAlgorithm, passPhrase, useSHA1, rand)
		{
			this.secret = new SecretSubkeyPacket(
				secret.PublicKeyPacket,
				secret.EncAlgorithm,
				secret.S2kUsage,
				secret.S2k,
				secret.GetIV(),
				secret.GetSecretKeyData());

			this.trust = trust;
            this.subSigs = subSigs;
            this.pub = new PgpPublicKey(keyPair.PublicKey, trust, subSigs);
        }

		internal PgpSecretKey(
            PgpKeyPair					keyPair,
            SymmetricKeyAlgorithmTag	encAlgorithm,
            char[]						passPhrase,
			bool						useSHA1,
			SecureRandom				rand)
        {
			PublicKeyPacket pubPk = keyPair.PublicKey.publicPk;

			BcpgObject secKey;
			switch (keyPair.PublicKey.Algorithm)
            {
				case PublicKeyAlgorithmTag.RsaEncrypt:
				case PublicKeyAlgorithmTag.RsaSign:
				case PublicKeyAlgorithmTag.RsaGeneral:
					RsaPrivateCrtKeyParameters rsK = (RsaPrivateCrtKeyParameters) keyPair.PrivateKey.Key;
					secKey = new RsaSecretBcpgKey(rsK.Exponent, rsK.P, rsK.Q);
					break;
				case PublicKeyAlgorithmTag.Dsa:
					DsaPrivateKeyParameters dsK = (DsaPrivateKeyParameters) keyPair.PrivateKey.Key;
					secKey = new DsaSecretBcpgKey(dsK.X);
					break;
				case PublicKeyAlgorithmTag.ElGamalEncrypt:
				case PublicKeyAlgorithmTag.ElGamalGeneral:
					ElGamalPrivateKeyParameters esK = (ElGamalPrivateKeyParameters) keyPair.PrivateKey.Key;
					secKey = new ElGamalSecretBcpgKey(esK.X);
					break;
				default:
					throw new PgpException("unknown key class");
            }

			string cName = PgpUtilities.GetSymmetricCipherName(encAlgorithm);

			IBufferedCipher c = null;
            if (cName != null)
            {
                try
                {
                    c = CipherUtilities.GetCipher(cName + "/CFB/NoPadding");
                }
                catch (Exception e)
                {
                    throw new PgpException("Exception creating cipher", e);
                }
            }

			try
            {
                MemoryStream bOut = new MemoryStream();
                BcpgOutputStream pOut = new BcpgOutputStream(bOut);

				pOut.WriteObject(secKey);

				byte[] keyData = bOut.ToArray();
				byte[] checksumBytes = Checksum(useSHA1, keyData, keyData.Length);

				pOut.Write(checksumBytes);

				byte[] bOutData = bOut.ToArray();

				if (c != null)
                {
                    byte[] iv = new byte[8];
                    rand.NextBytes(iv);

					S2k s2k = new S2k(HashAlgorithmTag.Sha1, iv, 0x60);
                    KeyParameter key = PgpUtilities.MakeKeyFromPassPhrase(encAlgorithm, s2k, passPhrase);

					iv = new byte[c.GetBlockSize()];
                    rand.NextBytes(iv);
                    c.Init(true, new ParametersWithIV(key, iv));

					byte[] encData = c.DoFinal(bOutData);

					int usage = useSHA1
						?	SecretKeyPacket.UsageSha1
						:	SecretKeyPacket.UsageChecksum;

					this.secret = new SecretKeyPacket(pubPk, encAlgorithm, usage, s2k, iv, encData);
				}
                else
                {
                    this.secret = new SecretKeyPacket(pubPk, encAlgorithm, null, null, bOutData);
                }

				this.trust = null;
            }
            catch (PgpException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new PgpException("Exception encrypting key", e);
            }

			this.keySigs = new ArrayList();
        }

		public PgpSecretKey(
            int							certificationLevel,
            PgpKeyPair					keyPair,
            string						id,
            SymmetricKeyAlgorithmTag	encAlgorithm,
            char[]						passPhrase,
            PgpSignatureSubpacketVector	hashedPackets,
            PgpSignatureSubpacketVector	unhashedPackets,
            SecureRandom				rand)
			: this(certificationLevel, keyPair, id, encAlgorithm, passPhrase, false, hashedPackets, unhashedPackets, rand)
		{
		}

		public PgpSecretKey(
			int							certificationLevel,
			PgpKeyPair					keyPair,
			string						id,
			SymmetricKeyAlgorithmTag	encAlgorithm,
			char[]						passPhrase,
			bool						useSHA1,
			PgpSignatureSubpacketVector	hashedPackets,
			PgpSignatureSubpacketVector	unhashedPackets,
			SecureRandom				rand)
			: this(keyPair, encAlgorithm, passPhrase, useSHA1, rand)
		{
			try
            {
                this.trust = null;
                this.ids = new ArrayList();
                ids.Add(id);

				this.idTrusts = new ArrayList();
                idTrusts.Add(null);

				this.idSigs = new ArrayList();

				PgpSignatureGenerator sGen = new PgpSignatureGenerator(
					keyPair.PublicKey.Algorithm, HashAlgorithmTag.Sha1);

				//
                // Generate the certification
                //
                sGen.InitSign(certificationLevel, keyPair.PrivateKey);

				sGen.SetHashedSubpackets(hashedPackets);
                sGen.SetUnhashedSubpackets(unhashedPackets);

				PgpSignature certification = sGen.GenerateCertification(id, keyPair.PublicKey);
                this.pub = PgpPublicKey.AddCertification(keyPair.PublicKey, id, certification);

				ArrayList sigList = new ArrayList();
                sigList.Add(certification);
                idSigs.Add(sigList);
            }
            catch (PgpException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new PgpException("Exception encrypting key", e);
            }
        }

		public PgpSecretKey(
            int							certificationLevel,
            PublicKeyAlgorithmTag		algorithm,
            AsymmetricKeyParameter		pubKey,
            AsymmetricKeyParameter		privKey,
            DateTime					time,
            string						id,
            SymmetricKeyAlgorithmTag	encAlgorithm,
            char[]						passPhrase,
            PgpSignatureSubpacketVector	hashedPackets,
            PgpSignatureSubpacketVector	unhashedPackets,
            SecureRandom				rand)
            : this(certificationLevel,
                new PgpKeyPair(algorithm, pubKey, privKey, time),
                id, encAlgorithm, passPhrase, hashedPackets, unhashedPackets, rand)
        {
        }

		public PgpSecretKey(
			int							certificationLevel,
			PublicKeyAlgorithmTag		algorithm,
			AsymmetricKeyParameter		pubKey,
			AsymmetricKeyParameter		privKey,
			DateTime					time,
			string						id,
			SymmetricKeyAlgorithmTag	encAlgorithm,
			char[]						passPhrase,
			bool						useSHA1,
			PgpSignatureSubpacketVector	hashedPackets,
			PgpSignatureSubpacketVector	unhashedPackets,
			SecureRandom				rand)
			: this(certificationLevel, new PgpKeyPair(algorithm, pubKey, privKey, time), id, encAlgorithm, passPhrase, useSHA1, hashedPackets, unhashedPackets, rand)
		{
		}

		/// <summary>True, if this key is marked as suitable for signature generation.</summary>
        public bool IsSigningKey
        {
			get
			{
				switch (pub.Algorithm)
				{
					case PublicKeyAlgorithmTag.RsaGeneral:
					case PublicKeyAlgorithmTag.RsaSign:
					case PublicKeyAlgorithmTag.Dsa:
					case PublicKeyAlgorithmTag.ECDsa:
					case PublicKeyAlgorithmTag.ElGamalGeneral:
						return true;
					default:
						return false;
				}
			}
        }

		/// <summary>True, if this is a master key.</summary>
        public bool IsMasterKey
		{
			get { return subSigs == null; }
        }

		/// <summary>The algorithm the key is encrypted with.</summary>
        public SymmetricKeyAlgorithmTag KeyEncryptionAlgorithm
        {
			get { return secret.EncAlgorithm; }
        }

		/// <summary>The key ID of the public key associated with this key.</summary>
        public long KeyId
        {
            get { return pub.KeyId; }
        }

		/// <summary>The public key associated with this key.</summary>
        public PgpPublicKey PublicKey
        {
			get { return pub; }
        }

		/// <summary>Allows enumeration of any user IDs associated with the key.</summary>
		/// <returns>An <c>IEnumerable</c> of <c>string</c> objects.</returns>
        public IEnumerable UserIds
        {
			get { return pub.GetUserIds(); }
        }

		/// <summary>Allows enumeration of any user attribute vectors associated with the key.</summary>
		/// <returns>An <c>IEnumerable</c> of <c>string</c> objects.</returns>
        public IEnumerable UserAttributes
        {
			get { return pub.GetUserAttributes(); }
        }

		private byte[] ExtractKeyData(
            char[] passPhrase)
        {
            SymmetricKeyAlgorithmTag alg = secret.EncAlgorithm;
            string cName = PgpUtilities.GetSymmetricCipherName(alg);
            IBufferedCipher c = null;
            if (cName != null)
            {
                try
                {
                    c = CipherUtilities.GetCipher(cName + "/CFB/NoPadding");
                }
                catch (Exception e)
                {
                    throw new PgpException("Exception creating cipher", e);
                }
            }

			byte[] encData = secret.GetSecretKeyData();

			try
            {
				byte[] data;
				if (c != null)
                {
					// TODO Factor this block out as 'encryptData'
					try
                    {
                        if (secret.PublicKeyPacket.Version == 4)
                        {
                            KeyParameter key = PgpUtilities.MakeKeyFromPassPhrase(secret.EncAlgorithm, secret.S2k, passPhrase);

							c.Init(false, new ParametersWithIV(key, secret.GetIV()));

							data = c.DoFinal(encData);

							bool useSHA1 = secret.S2kUsage == SecretKeyPacket.UsageSha1;
							byte[] check = Checksum(useSHA1, data, (useSHA1) ? data.Length - 20 : data.Length - 2);

							for (int i = 0; i != check.Length; i++)
							{
								if (check[i] != data[data.Length - check.Length + i])
								{
									throw new PgpException("Checksum mismatch at " + i + " of " + check.Length);
								}
							}
						}
                        else // version 2 or 3, RSA only.
                        {
                            KeyParameter key = PgpUtilities.MakeKeyFromPassPhrase(secret.EncAlgorithm, secret.S2k, passPhrase);

							data = new byte[encData.Length];

							byte[] iv = secret.GetIV();

							//
                            // read in the four numbers
                            //
                            int pos = 0;

							for (int i = 0; i != 4; i++)
                            {
                                c.Init(false, new ParametersWithIV(key, iv));

								int encLen = (((encData[pos] << 8) | (encData[pos + 1] & 0xff)) + 7) / 8;

								data[pos] = encData[pos];
								data[pos + 1] = encData[pos + 1];

								c.DoFinal(encData, pos + 2, encLen, data, pos + 2);
								pos += 2 + encLen;

								if (i != 3)
                                {
                                    Array.Copy(encData, pos - iv.Length, iv, 0, iv.Length);
                                }
                            }
                            //
                            // verify Checksum
                            //

							int cs = ((encData[pos] << 8) & 0xff00) | (encData[pos + 1] & 0xff);
                            int calcCs = 0;
                            for (int j=0; j < data.Length-2; j++)
                            {
                                calcCs += data[j] & 0xff;
                            }

							calcCs &= 0xffff;
                            if (calcCs != cs)
                            {
                                throw new PgpException("Checksum mismatch: passphrase wrong, expected "
									+ cs.ToString("X")
									+ " found " + calcCs.ToString("X"));
                            }
                        }
                    }
                    catch (PgpException e)
                    {
                        throw e;
                    }
                    catch (Exception e)
                    {
                        throw new PgpException("Exception decrypting key", e);
                    }
                }
                else
                {
                    data = encData;
                }
                return data;
            }
            catch (PgpException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new PgpException("Exception constructing key", e);
            }
        }

		/// <summary>Extract a <c>PgpPrivateKey</c> from this secret key's encrypted contents.</summary>
        public PgpPrivateKey ExtractPrivateKey(
            char[] passPhrase)
        {
            if (secret.GetSecretKeyData() == null)
            {
                return null;
            }

			PublicKeyPacket pubPk = secret.PublicKeyPacket;
            try
            {
                byte[] data = ExtractKeyData(passPhrase);
                BcpgInputStream bcpgIn = BcpgInputStream.Wrap(new MemoryStream(data, false));
                AsymmetricKeyParameter privateKey;
                switch (pubPk.Algorithm)
                {
                case PublicKeyAlgorithmTag.RsaEncrypt:
                case PublicKeyAlgorithmTag.RsaGeneral:
                case PublicKeyAlgorithmTag.RsaSign:
                    RsaPublicBcpgKey rsaPub = (RsaPublicBcpgKey)pubPk.Key;
                    RsaSecretBcpgKey rsaPriv = new RsaSecretBcpgKey(bcpgIn);
                    RsaPrivateCrtKeyParameters rsaPrivSpec = new RsaPrivateCrtKeyParameters(
                        rsaPriv.Modulus,
                        rsaPub.PublicExponent,
                        rsaPriv.PrivateExponent,
                        rsaPriv.PrimeP,
                        rsaPriv.PrimeQ,
                        rsaPriv.PrimeExponentP,
                        rsaPriv.PrimeExponentQ,
                        rsaPriv.CrtCoefficient);
                    privateKey = rsaPrivSpec;
                    break;
                case PublicKeyAlgorithmTag.Dsa:
                    DsaPublicBcpgKey dsaPub = (DsaPublicBcpgKey)pubPk.Key;
                    DsaSecretBcpgKey dsaPriv = new DsaSecretBcpgKey(bcpgIn);
                    DsaParameters dsaParams = new DsaParameters(dsaPub.P, dsaPub.Q, dsaPub.G);
                    privateKey = new DsaPrivateKeyParameters(dsaPriv.X, dsaParams);
                    break;
                case PublicKeyAlgorithmTag.ElGamalEncrypt:
                case PublicKeyAlgorithmTag.ElGamalGeneral:
                    ElGamalPublicBcpgKey elPub = (ElGamalPublicBcpgKey)pubPk.Key;
                    ElGamalSecretBcpgKey elPriv = new ElGamalSecretBcpgKey(bcpgIn);
                    ElGamalParameters elParams = new ElGamalParameters(elPub.P, elPub.G);
                    privateKey = new ElGamalPrivateKeyParameters(elPriv.X, elParams);
                    break;
                default:
                    throw new PgpException("unknown public key algorithm encountered");
                }

				return new PgpPrivateKey(privateKey, KeyId);
            }
            catch (PgpException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new PgpException("Exception constructing key", e);
            }
        }

		private static byte[] Checksum(
			bool	useSHA1,
			byte[]	bytes,
			int		length)
		{
			if (useSHA1)
			{
				try
				{
					IDigest dig = DigestUtilities.GetDigest("SHA1");
					dig.BlockUpdate(bytes, 0, length);
					return DigestUtilities.DoFinal(dig);
				}
				//catch (NoSuchAlgorithmException e)
				catch (Exception e)
				{
					throw new PgpException("Can't find SHA-1", e);
				}
			}
			else
			{
				int Checksum = 0;
				for (int i = 0; i != length; i++)
				{
					Checksum += bytes[i];
				}

				return new byte[] { (byte)(Checksum >> 8), (byte)Checksum };
			}
		}

		public byte[] GetEncoded()
        {
            MemoryStream bOut = new MemoryStream();
            Encode(bOut);
            return bOut.ToArray();
        }

		public void Encode(
            Stream outStr)
        {
            BcpgOutputStream bcpgOut = BcpgOutputStream.Wrap(outStr);

			bcpgOut.WritePacket(secret);
            if (trust != null)
            {
                bcpgOut.WritePacket(trust);
            }

			if (subSigs == null) // is not a sub key
            {
				foreach (PgpSignature keySig in keySigs)
				{
					keySig.Encode(bcpgOut);
                }

				for (int i = 0; i != ids.Count; i++)
                {
                    if (ids[i] is string)
                    {
                        string id = (string) ids[i];

                        bcpgOut.WritePacket(new UserIdPacket(id));
                    }
                    else
                    {
                        PgpUserAttributeSubpacketVector v = (PgpUserAttributeSubpacketVector)ids[i];
                        bcpgOut.WritePacket(new UserAttributePacket(v.ToSubpacketArray()));
                    }

					if (idTrusts[i] != null)
                    {
                        bcpgOut.WritePacket((ContainedPacket)idTrusts[i]);
                    }

					foreach (PgpSignature sig in (ArrayList) idSigs[i])
					{
						sig.Encode(bcpgOut);
                    }
                }
            }
            else
            {
				foreach (PgpSignature subSig in subSigs)
				{
					subSig.Encode(bcpgOut);
                }
            }

			// TODO Check that this is right/necessary
			//bcpgOut.Finish();
        }

		/// <summary>
		/// Return a copy of the passed in secret key, encrypted using a new password
		/// and the passed in algorithm.
		/// </summary>
		/// <param name="key">The PgpSecretKey to be copied.</param>
		/// <param name="oldPassPhrase">The current password for the key.</param>
		/// <param name="newPassPhrase">The new password for the key.</param>
		/// <param name="newEncAlgorithm">The algorithm to be used for the encryption.</param>
		/// <param name="rand">Source of randomness.</param>
        public static PgpSecretKey CopyWithNewPassword(
            PgpSecretKey				key,
            char[]						oldPassPhrase,
            char[]						newPassPhrase,
            SymmetricKeyAlgorithmTag	newEncAlgorithm,
            SecureRandom				rand)
        {
            byte[]	rawKeyData = key.ExtractKeyData(oldPassPhrase);
			int		s2kUsage = key.secret.S2kUsage;
			byte[]	iv = null;
            S2k		s2k = null;
            byte[]	keyData;

			if (newEncAlgorithm == SymmetricKeyAlgorithmTag.Null)
            {
				s2kUsage = SecretKeyPacket.UsageNone;
				if (key.secret.S2kUsage == SecretKeyPacket.UsageSha1)   // SHA-1 hash, need to rewrite Checksum
				{
					keyData = new byte[rawKeyData.Length - 18];

					Array.Copy(rawKeyData, 0, keyData, 0, keyData.Length - 2);

					byte[] check = Checksum(false, keyData, keyData.Length - 2);

					keyData[keyData.Length - 2] = check[0];
					keyData[keyData.Length - 1] = check[1];
				}
				else
				{
					keyData = rawKeyData;
				}
			}
            else
            {
                IBufferedCipher c;
                try
                {
					string cName = PgpUtilities.GetSymmetricCipherName(newEncAlgorithm);
					c = CipherUtilities.GetCipher(cName + "/CFB/NoPadding");
                }
                catch (Exception e)
                {
                    throw new PgpException("Exception creating cipher", e);
                }

				iv = new byte[8];
                rand.NextBytes(iv);
                s2k = new S2k(HashAlgorithmTag.Sha1, iv, 0x60);
                try
                {
                    KeyParameter sKey = PgpUtilities.MakeKeyFromPassPhrase(newEncAlgorithm, s2k, newPassPhrase);
                    iv = new byte[c.GetBlockSize()];
                    rand.NextBytes(iv);
                    c.Init(true, new ParametersWithIV(sKey, iv));

					keyData = c.DoFinal(rawKeyData);
                }
                catch (PgpException e)
                {
                    throw e;
                }
                catch (Exception e)
                {
                    throw new PgpException("Exception encrypting key", e);
                }
            }

			SecretKeyPacket secret;
            if (key.secret is SecretSubkeyPacket)
            {
                secret = new SecretSubkeyPacket(key.secret.PublicKeyPacket,
					newEncAlgorithm, s2kUsage, s2k, iv, keyData);
            }
            else
            {
                secret = new SecretKeyPacket(key.secret.PublicKeyPacket,
	                newEncAlgorithm, s2kUsage, s2k, iv, keyData);
            }

			if (key.subSigs == null)
            {
                return new PgpSecretKey(secret, key.trust, key.keySigs, key.ids,
                    key.idTrusts, key.idSigs, key.pub);
            }

			return new PgpSecretKey(secret, key.trust, key.subSigs, key.pub);
        }
    }
}
