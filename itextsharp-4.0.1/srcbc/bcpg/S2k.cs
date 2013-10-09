using System;
using System.IO;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Bcpg
{
	/// <remarks>The string to key specifier class.</remarks>
    public class S2k
        : BcpgObject
    {
        private const int ExpBias = 6;

        public const int Simple = 0;
        public const int Salted = 1;
        public const int SaltedAndIterated = 3;
        public const int GnuDummyS2K = 101;

        internal int type;
        internal HashAlgorithmTag algorithm;
        internal byte[] iv;
        internal int itCount = -1;
        internal int protectionMode = -1;

        internal S2k(
            Stream inputStream)
        {
			//Stream dIn = inputStream;
			BinaryReader dIn = new BinaryReader(inputStream);

            type = dIn.ReadByte();
            algorithm = (HashAlgorithmTag) dIn.ReadByte();

            //
            // if this happens we have a dummy-S2k packet.
            //
            if (type != GnuDummyS2K)
            {
                if (type != 0)
                {
					iv = dIn.ReadBytes(8);

					if (iv.Length < 8)
					{
						throw new EndOfStreamException();
					}
				}

				if (type == 3)
                {
                    itCount = dIn.ReadByte();
                }
            }
            else
            {
                dIn.ReadByte(); // G
                dIn.ReadByte(); // N
                dIn.ReadByte(); // U
                protectionMode = dIn.ReadByte(); // protection mode
            }
        }

        public S2k(
            HashAlgorithmTag algorithm)
        {
            this.type = 0;
            this.algorithm = algorithm;
        }

        public S2k(
            HashAlgorithmTag algorithm,
            byte[] iv)
        {
            this.type = 1;
            this.algorithm = algorithm;
            this.iv = iv;
        }

        public S2k(
            HashAlgorithmTag algorithm,
            byte[] iv,
            int itCount)
        {
            this.type = 3;
            this.algorithm = algorithm;
            this.iv = iv;
            this.itCount = itCount;
        }

        public int Type
        {
			get { return type; }
        }

		/// <summary>The hash algorithm.</summary>
        public HashAlgorithmTag HashAlgorithm
        {
			get { return algorithm; }
		}

		/// <summary>The IV for the key generation algorithm.</summary>
        public byte[] GetIV()
        {
            return Arrays.Clone(iv);
        }

		[Obsolete("Use 'IterationCount' property instead")]
        public long GetIterationCount()
        {
            return IterationCount;
        }

		/// <summary>The iteration count</summary>
		public long IterationCount
		{
			get { return (16 + (itCount & 15)) << ((itCount >> 4) + ExpBias); }
		}

		/// <summary>The protection mode - only if GnuDummyS2K</summary>
        public int ProtectionMode
        {
			get { return protectionMode; }
        }

        public override void Encode(
            BcpgOutputStream bcpgOut)
        {
            bcpgOut.WriteByte((byte) type);
            bcpgOut.WriteByte((byte) algorithm);

            if (type != GnuDummyS2K)
            {
                if (type != 0)
                {
                    bcpgOut.Write(iv);
                }

                if (type == 3)
                {
                    bcpgOut.WriteByte((byte) itCount);
                }
            }
            else
            {
                bcpgOut.WriteByte((byte) 'G');
                bcpgOut.WriteByte((byte) 'N');
                bcpgOut.WriteByte((byte) 'U');
                bcpgOut.WriteByte((byte) protectionMode);
            }
        }
    }
}
