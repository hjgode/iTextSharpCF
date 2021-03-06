using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Org.BouncyCastle.Math
{
	// TODO Probably need a custom serialization here
	[Serializable]
	public class BigInteger
	{
		// The primes b/w 2 and ~2^10
		/*
				3   5   7   11  13  17  19  23  29
			31  37  41  43  47  53  59  61  67  71
			73  79  83  89  97  101 103 107 109 113
			127 131 137 139 149 151 157 163 167 173
			179 181 191 193 197 199 211 223 227 229
			233 239 241 251 257 263 269 271 277 281
			283 293 307 311 313 317 331 337 347 349
			353 359 367 373 379 383 389 397 401 409
			419 421 431 433 439 443 449 457 461 463
			467 479 487 491 499 503 509 521 523 541
			547 557 563 569 571 577 587 593 599 601
			607 613 617 619 631 641 643 647 653 659
			661 673 677 683 691 701 709 719 727 733
			739 743 751 757 761 769 773 787 797 809
			811 821 823 827 829 839 853 857 859 863
			877 881 883 887 907 911 919 929 937 941
			947 953 967 971 977 983 991 997
			1009 1013 1019 1021 1031
		*/

		// Each list has a product < 2^31
		private static readonly int[][] primeLists = new int[][]
		{
			new int[]{ 3, 5, 7, 11, 13, 17, 19, 23 },
			new int[]{ 29, 31, 37, 41, 43 },
			new int[]{ 47, 53, 59, 61, 67 },
			new int[]{ 71, 73, 79, 83 },
			new int[]{ 89, 97, 101, 103 },

			new int[]{ 107, 109, 113, 127 },
			new int[]{ 131, 137, 139, 149 },
			new int[]{ 151, 157, 163, 167 },
			new int[]{ 173, 179, 181, 191 },
			new int[]{ 193, 197, 199, 211 },

			new int[]{ 223, 227, 229 },
			new int[]{ 233, 239, 241 },
			new int[]{ 251, 257, 263 },
			new int[]{ 269, 271, 277 },
			new int[]{ 281, 283, 293 },

			new int[]{ 307, 311, 313 },
			new int[]{ 317, 331, 337 },
			new int[]{ 347, 349, 353 },
			new int[]{ 359, 367, 373 },
			new int[]{ 379, 383, 389 },

			new int[]{ 397, 401, 409 },
			new int[]{ 419, 421, 431 },
			new int[]{ 433, 439, 443 },
			new int[]{ 449, 457, 461 },
			new int[]{ 463, 467, 479 },

			new int[]{ 487, 491, 499 },
			new int[]{ 503, 509, 521 },
			new int[]{ 523, 541, 547 },
			new int[]{ 557, 563, 569 },
			new int[]{ 571, 577, 587 },

			new int[]{ 593, 599, 601 },
			new int[]{ 607, 613, 617 },
			new int[]{ 619, 631, 641 },
			new int[]{ 643, 647, 653 },
			new int[]{ 659, 661, 673 },

			new int[]{ 677, 683, 691 },
			new int[]{ 701, 709, 719 },
			new int[]{ 727, 733, 739 },
			new int[]{ 743, 751, 757 },
			new int[]{ 761, 769, 773 },

			new int[]{ 787, 797, 809 },
			new int[]{ 811, 821, 823 },
			new int[]{ 827, 829, 839 },
			new int[]{ 853, 857, 859 },
			new int[]{ 863, 877, 881 },

			new int[]{ 883, 887, 907 },
			new int[]{ 911, 919, 929 },
			new int[]{ 937, 941, 947 },
			new int[]{ 953, 967, 971 },
			new int[]{ 977, 983, 991 },

			new int[]{ 997, 1009, 1013 },
			new int[]{ 1019, 1021, 1031 },
		};

		private static int[] primeProducts;
//		private static BigInteger[] PrimeProducts;

		static BigInteger()
		{
			primeProducts = new int[primeLists.Length];
//			PrimeProducts = new BigInteger[primeLists.Length];

			for (int i = 0; i < primeLists.Length; ++i)
			{
				int[] primeList = primeLists[i];
				int product = 1;
				for (int j = 0; j < primeList.Length; ++j)
				{
					product *= primeList[j];
				}
				primeProducts[i] = product;
//				PrimeProducts[i] = BigInteger.ValueOf(product);
			}
		}




		private const long IMASK = 0xffffffffL;
		private static readonly ulong UIMASK = (ulong)IMASK;

		private static readonly int[] ZeroMagnitude = new int[0];

		public static readonly BigInteger Zero = new BigInteger(0, ZeroMagnitude, false);
		public static readonly BigInteger One = createUValueOf(1);
		public static readonly BigInteger Two = createUValueOf(2);
		public static readonly BigInteger Three = createUValueOf(3);
		public static readonly BigInteger Ten = createUValueOf(10);

		private int sign; // -1 means -ve; +1 means +ve; 0 means 0;
		private int[] magnitude; // array of ints with [0] being the most significant
		private int nBits = -1; // cache BitCount() value
		private int nBitLength = -1; // cache calcBitLength() value
		private long mQuote = -1L; // -m^(-1) mod b, b = 2^32 (see Montgomery mult.)

		private static readonly int chunk10 = 19;
		private static readonly BigInteger radix10 = ValueOf(10);
		private static readonly BigInteger radix10E = radix10.Pow(chunk10);

		private static readonly int chunk16 = 16;
		private static readonly BigInteger radix16 = ValueOf(16);
		private static readonly BigInteger radix16E = radix16.Pow(chunk16);

		private static readonly Random RandomSource = new Random();

		private BigInteger()
		{
		}

		private BigInteger(
			int		signum,
			int[]	mag,
			bool	checkMag)
		{
			if (checkMag)
			{
				int i = 0;
				while (i < mag.Length && mag[i] == 0)
				{
					++i;
				}

				if (i == mag.Length)
				{
//					this.sign = 0;
					this.magnitude = ZeroMagnitude;
				}
				else
				{
					this.sign = signum;

					if (i == 0)
					{
						this.magnitude = mag;
					}
					else
					{
						// strip leading 0 words
						this.magnitude = new int[mag.Length - i];
						Array.Copy(mag, i, this.magnitude, 0, this.magnitude.Length);
					}
				}
			}
			else
			{
				this.sign = signum;
				this.magnitude = mag;
			}
		}

		public BigInteger(
			string value)
			: this(value, 10)
		{
		}

		public BigInteger(
			string value,
			int radix)
		{
			if (value.Length == 0)
			{
				throw new FormatException("Zero length BigInteger");
			}

			NumberStyles style;
			int chunk;
			BigInteger r;
			BigInteger rE;
			switch (radix)
			{
				case 10:
					// This style seems to handle spaces and minus sign already (our processing redundant?)
					style = NumberStyles.Integer;
					chunk = chunk10;
					r = radix10;
					rE = radix10E;
					break;
				case 16:
					// TODO Should this be HexNumber?
					style = NumberStyles.AllowHexSpecifier;
					chunk = chunk16;
					r = radix16;
					rE = radix16E;
					break;
				default:
					throw new FormatException("Only base 10 or 16 allowed");
			}


			int index = 0;
			sign = 1;

			if (value[0] == '-')
			{
				if (value.Length == 1)
				{
					throw new FormatException("Zero length BigInteger");
				}

				sign = -1;
				index = 1;
			}

			// strip leading zeros from the string value
			while (index < value.Length && Int32.Parse(value[index].ToString(), style) == 0)
			{
				index++;
			}

			if (index >= value.Length)
			{
				// zero value - we're done
				sign = 0;
				magnitude = ZeroMagnitude;
				return;
			}

			//////
			// could we work out the max number of ints required to store
			// value.Length digits in the given base, then allocate that
			// storage in one hit?, then Generate the magnitude in one hit too?
			//////

			BigInteger b = Zero;


			int next = index + chunk;

			if (next <= value.Length)
			{
				do
				{
					string s = value.Substring(index, chunk);
					ulong i = UInt64.Parse(s, style);
					BigInteger bi = createUValueOf(i);

					if (radix == 16)
					{
						b = b.ShiftLeft(chunk << 2);
					}
					else
					{
						b = b.Multiply(rE);
					}

					b = b.Add(bi);

					index = next;
					next += chunk;
				}
				while (next <= value.Length);
			}

			if (index < value.Length)
			{
				string s = value.Substring(index);
				ulong i = UInt64.Parse(s, style);
				BigInteger bi = createUValueOf(i);

				if (b.sign > 0)
				{
					if (radix == 16)
					{
						b = b.ShiftLeft(s.Length << 2);
					}
					else
					{
						b = b.Multiply(r.Pow(s.Length));
					}

					b = b.Add(bi);
				}
				else
				{
					b = bi;
				}
			}

			// Note: This is the previous (slower) algorithm
			//			while (index < value.Length)
			//            {
			//				char c = value[index];
			//				string s = c.ToString();
			//				int i = Int32.Parse(s, style);
			//
			//                b = b.Multiply(r).Add(ValueOf(i));
			//                index++;
			//            }


			magnitude = b.magnitude;
		}

		public BigInteger(
			byte[] bytes)
		{
			if (bytes.Length == 0)
			{
				throw new FormatException("Zero length BigInteger");
			}

			if ((sbyte)bytes[0] < 0)
			{
				this.sign = -1;

				int iBval;
				// strip leading sign bytes
				for (iBval = 0; iBval < bytes.Length && ((sbyte)bytes[iBval] == -1); iBval++)
				{
				}

				if (iBval >= bytes.Length)
				{
					this.magnitude = One.magnitude;
				}
				else
				{
					int numBytes = bytes.Length - iBval;
					byte[] inverse = new byte[numBytes];

					int index = 0;
					while (index < numBytes)
					{
						inverse[index++] = (byte)~bytes[iBval++];
					}

					Debug.Assert(iBval == bytes.Length);

					while (inverse[--index] == byte.MaxValue)
					{
						inverse[index] = byte.MinValue;
					}

					inverse[index]++;

					this.magnitude = MakeMagnitude(inverse);
				}
			}
			else
			{
				// strip leading zero bytes and return magnitude bytes
				this.magnitude = MakeMagnitude(bytes);
				this.sign = this.magnitude.Length > 0 ? 1 : 0;
			}
		}

		private static int[] MakeMagnitude(
			byte[] bytes)
		{
			int firstSignificant;

			// strip leading zeros
			for (firstSignificant = 0; firstSignificant < bytes.Length
				&& bytes[firstSignificant] == 0; firstSignificant++)
			{
			}

			if (firstSignificant >= bytes.Length)
			{
				return ZeroMagnitude;
			}

			int nInts = (bytes.Length - firstSignificant + 3) / BytesPerInt;
			int bCount = (bytes.Length - firstSignificant) % BytesPerInt;
			if (bCount == 0)
			{
				bCount = BytesPerInt;
			}

			if (nInts < 1)
			{
				return ZeroMagnitude;
			}

			int[] mag = new int[nInts];

			int v = 0;
			int magnitudeIndex = 0;
			for (int i = firstSignificant; i < bytes.Length; ++i)
			{
				v <<= 8;
				v |= bytes[i] & 0xff;
				bCount--;
				if (bCount <= 0)
				{
					mag[magnitudeIndex] = v;
					magnitudeIndex++;
					bCount = BytesPerInt;
					v = 0;
				}
			}

			if (magnitudeIndex < mag.Length)
			{
				mag[magnitudeIndex] = v;
			}

			return mag;
		}

		public BigInteger(
			int		sign,
			byte[]	value)
		{
			if (sign < -1 || sign > 1)
			{
				throw new FormatException("Invalid sign value");
			}

			if (sign == 0)
			{
//				this.sign = 0;
				this.magnitude = ZeroMagnitude;
				return;
			}

			// copy bytes
			this.magnitude = MakeMagnitude(value);
			this.sign = this.magnitude.Length < 1 ? 0 : sign;
		}

		public BigInteger(
			int		sizeInBits,
			Random	random)
		{
			if (sizeInBits < 0)
			{
				throw new ArgumentException("sizeInBits must be non-negative");
			}

			this.nBits = -1;
			this.nBitLength = -1;

			if (sizeInBits == 0)
			{
//				this.sign = 0;
				this.magnitude = ZeroMagnitude;
				return;
			}

			int nBytes = (sizeInBits + BitsPerByte - 1) / BitsPerByte;

			byte[] b = new byte[nBytes];

			random.NextBytes(b);

			// strip off any excess bits in the MSB
			b[0] &= rndMask[BitsPerByte * nBytes - sizeInBits];

			this.magnitude = MakeMagnitude(b);
			this.sign = this.magnitude.Length < 1 ? 0 : 1;
		}

		private const int BitsPerByte = 8;
		private const int BytesPerInt = 4;

		private static readonly byte[] rndMask = { 255, 127, 63, 31, 15, 7, 3, 1 };

		public BigInteger(
			int		bitLength,
			int		certainty,
			Random	random)
		{
			if (bitLength < 2)
			{
				throw new ArithmeticException("bitLength < 2");
			}

			this.sign = 1;
			this.nBitLength = bitLength;

			if (bitLength == 2)
			{
				this.magnitude = random.Next(2) == 0
					?	Two.magnitude
					:	Three.magnitude;
				return;
			}

			int nBytes = (bitLength + 7) / BitsPerByte;
			int xBits = BitsPerByte * nBytes - bitLength;
			byte mask = rndMask[xBits];

			byte[] b = new byte[nBytes];

			for (;;)
			{
				random.NextBytes(b);

				// strip off any excess bits in the MSB
				b[0] &= mask;

				// ensure the leading bit is 1 (to meet the strength requirement)
				b[0] |= (byte)(1 << (7 - xBits));

				// ensure the trailing bit is 1 (i.e. must be odd)
				b[nBytes - 1] |= 1;

				this.magnitude = MakeMagnitude(b);
				this.nBits = -1;
				this.mQuote = -1L;

				if (certainty < 1)
					break;

				if (CheckProbablePrime(certainty, random))
					break;

				if (bitLength > 32)
				{
					for (int rep = 0; rep < 10000; ++rep)
					{
						this.magnitude[this.magnitude.Length - 1] ^= ((random.Next() + 1) << 1);
						this.mQuote = -1L;

						if (CheckProbablePrime(certainty, random))
							return;
					}
				}
			}
		}

		public BigInteger Abs()
		{
			return sign >= 0 ? this : Negate();
		}

		/**
		 * return a = a + b - b preserved.
		 */
		private static int[] AddMagnitudes(
			int[] a,
			int[] b)
		{
			int tI = a.Length - 1;
			int vI = b.Length - 1;
			long m = 0;

			while (vI >= 0)
			{
				m += ((long)(uint)a[tI] + (long)(uint)b[vI--]);
				a[tI--] = (int)m;
				m = (long)((ulong)m >> 32);
			}

			if (m != 0)
			{
				while (tI >= 0 && ++a[tI--] == 0)
				{
				}
			}

			return a;
		}

		public BigInteger Add(
			BigInteger value)
		{
			if (this.sign == 0)
				return value;

			if (this.sign != value.sign)
			{
				if (value.sign == 0)
					return this;

				if (value.sign < 0)
					return Subtract(value.Negate());

				return value.Subtract(Negate());
			}

			return AddToMagnitude(value.magnitude);
		}

		private BigInteger AddToMagnitude(
			int[] magToAdd)
		{
			int[] big, small;
			if (this.magnitude.Length < magToAdd.Length)
			{
				big = magToAdd;
				small = this.magnitude;
			}
			else
			{
				big = this.magnitude;
				small = magToAdd;
			}

			// Conservatively avoid over-allocation when no overflow possible
			uint limit = uint.MaxValue;
			if (big.Length == small.Length)
				limit -= (uint) small[0];

			bool possibleOverflow = (uint) big[0] >= limit;

			int[] bigCopy;
			if (possibleOverflow)
			{
				bigCopy = new int[big.Length + 1];
				big.CopyTo(bigCopy, 1);
			}
			else
			{
				bigCopy = (int[]) big.Clone();
			}

			bigCopy = AddMagnitudes(bigCopy, small);

			return new BigInteger(this.sign, bigCopy, possibleOverflow);
		}

		public BigInteger And(
			BigInteger value)
		{
			if (this.sign == 0 || value.sign == 0)
			{
				return Zero;
			}

			int[] aMag = this.sign > 0
				? this.magnitude
				: Add(One).magnitude;

			int[] bMag = value.sign > 0
				? value.magnitude
				: value.Add(One).magnitude;

			bool resultNeg = sign < 0 && value.sign < 0;
			int resultLength = System.Math.Max(aMag.Length, bMag.Length);
			int[] resultMag = new int[resultLength];

			int aStart = resultMag.Length - aMag.Length;
			int bStart = resultMag.Length - bMag.Length;

			for (int i = 0; i < resultMag.Length; ++i)
			{
				int aWord = i >= aStart ? aMag[i - aStart] : 0;
				int bWord = i >= bStart ? bMag[i - bStart] : 0;

				if (this.sign < 0)
				{
					aWord = ~aWord;
				}

				if (value.sign < 0)
				{
					bWord = ~bWord;
				}

				resultMag[i] = aWord & bWord;

				if (resultNeg)
				{
					resultMag[i] = ~resultMag[i];
				}
			}

			BigInteger result = new BigInteger(1, resultMag, true);

			// TODO Optimise this case
			if (resultNeg)
			{
				result = result.Not();
			}

			return result;
		}

		public BigInteger AndNot(
			BigInteger value)
		{
			return And(value.Not());
		}

		public int BitCount
		{
			get
			{
				if (nBits == -1)
				{
					nBits = 0;
					for (int i = 0; i < magnitude.Length; i++)
					{
						nBits += bitCounts[magnitude[i] & 0xff];
						nBits += bitCounts[(magnitude[i] >> 8) & 0xff];
						nBits += bitCounts[(magnitude[i] >> 16) & 0xff];
						nBits += bitCounts[(magnitude[i] >> 24) & 0xff];
					}
				}

				return nBits;
			}
		}

		private readonly static byte[] bitCounts =
		{
			0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 1,
			2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4,
			4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3,
			4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5,
			3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 1, 2, 2, 3, 2,
			3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3,
			3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6,
			7, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6,
			5, 6, 6, 7, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 4, 5, 5, 6, 5, 6, 6, 7, 5,
			6, 6, 7, 6, 7, 7, 8
		};

		private int calcBitLength(
			int		indx,
			int[]	mag)
		{
			if (mag.Length == 0)
				return 0;

			while (indx != mag.Length && mag[indx] == 0)
			{
				indx++;
			}

			if (indx == mag.Length)
				return 0;

			// bit length for everything after the first int
			int bitLength = 32 * ((mag.Length - indx) - 1);

			// and determine bitlength of first int
			bitLength += BitLen(mag[indx]);

			if (sign < 0)
			{
				// Check if magnitude is a power of two
				bool pow2 = ((bitCounts[mag[indx] & 0xff])
					+ (bitCounts[(mag[indx] >> 8) & 0xff])
					+ (bitCounts[(mag[indx] >> 16) & 0xff]) + (bitCounts[(mag[indx] >> 24) & 0xff])) == 1;

				for (int i = indx + 1; i < mag.Length && pow2; i++)
				{
					pow2 = (mag[i] == 0);
				}

				bitLength -= (pow2 ? 1 : 0);
			}

			return bitLength;
		}

		public int BitLength
		{
			get
			{
				if (nBitLength == -1)
				{
					nBitLength = sign == 0
						? 0
						: calcBitLength(0, magnitude);
				}

				return nBitLength;
			}
		}

		//
		// BitLen(value) is the number of bits in value.
		//
		private static int BitLen(
			int w)
		{
			// Binary search - decision tree (5 tests, rarely 6)
			return (w < 1 << 15 ? (w < 1 << 7
				? (w < 1 << 3 ? (w < 1 << 1
				? (w < 1 << 0 ? (w < 0 ? 32 : 0) : 1)
				: (w < 1 << 2 ? 2 : 3)) : (w < 1 << 5
				? (w < 1 << 4 ? 4 : 5)
				: (w < 1 << 6 ? 6 : 7)))
				: (w < 1 << 11
				? (w < 1 << 9 ? (w < 1 << 8 ? 8 : 9) : (w < 1 << 10 ? 10 : 11))
				: (w < 1 << 13 ? (w < 1 << 12 ? 12 : 13) : (w < 1 << 14 ? 14 : 15)))) : (w < 1 << 23 ? (w < 1 << 19
				? (w < 1 << 17 ? (w < 1 << 16 ? 16 : 17) : (w < 1 << 18 ? 18 : 19))
				: (w < 1 << 21 ? (w < 1 << 20 ? 20 : 21) : (w < 1 << 22 ? 22 : 23))) : (w < 1 << 27
				? (w < 1 << 25 ? (w < 1 << 24 ? 24 : 25) : (w < 1 << 26 ? 26 : 27))
				: (w < 1 << 29 ? (w < 1 << 28 ? 28 : 29) : (w < 1 << 30 ? 30 : 31)))));
		}

//		private readonly static byte[] bitLengths =
//		{
//			0, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4,
//			5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
//			6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
//			7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
//			7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 8, 8, 8, 8,
//			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
//			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
//			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
//			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
//			8, 8, 8, 8, 8, 8, 8, 8
//		};

		public int CompareTo(
			object obj)
		{
			return CompareTo((BigInteger)obj);
		}

		/**
		 * unsigned comparison on two arrays - note the arrays may
		 * start with leading zeros.
		 */
		private static int CompareTo(
			int		xIndx,
			int[]	x,
			int		yIndx,
			int[]	y)
		{
			while (xIndx != x.Length && x[xIndx] == 0)
			{
				xIndx++;
			}

			while (yIndx != y.Length && y[yIndx] == 0)
			{
				yIndx++;
			}

			int diff = (x.Length - y.Length) - (xIndx - yIndx);

			if (diff < 0)
				return -1;

			if (diff > 0)
				return 1;

			// lengths of magnitudes the same, test the magnitude values

			while (xIndx < x.Length)
			{
				uint v1 = (uint)x[xIndx++];
				uint v2 = (uint)y[yIndx++];

				if (v1 != v2)
					return v1 < v2 ? - 1: 1;
			}

			return 0;
		}

		public int CompareTo(
			BigInteger value)
		{
			return sign < value.sign ? -1
				: sign > value.sign ? 1
				: sign == 0 ? 0
				: sign * CompareTo(0, magnitude, 0, value.magnitude);
		}

		/**
		 * return z = x / y - done in place (z value preserved, x contains the
		 * remainder)
		 */
		private int[] Divide(
			int[]	x,
			int[]	y)
		{
			int xStart = 0;
			while (xStart < x.Length && x[xStart] == 0)
			{
				xStart++;
			}

			int yStart = 0;
			while (yStart < y.Length && y[yStart] == 0)
			{
				yStart++;
			}

			int xyCmp = CompareTo(xStart, x, yStart, y);
			int[] count;

			if (xyCmp > 0)
			{
				int[] c;
				int cBitLength = calcBitLength(yStart, y);
				int firstShift = calcBitLength(xStart, x) - cBitLength;

				int cStart;
				if (firstShift > 1)
				{
					// TODO Take another look at this block...
					c = ShiftLeft(y, firstShift - 1);
					count = ShiftLeft(One.magnitude, firstShift - 1);
					if (firstShift % 32 == 0)
					{
						// Special case where the shift is the size of an int.
						int[] countSpecial = new int[firstShift / 32 + 1];
						Array.Copy(count, 0, countSpecial, 1, countSpecial.Length - 1);
						countSpecial[0] = 0;
						count = countSpecial;
					}
					cStart = 0;
					cBitLength += (firstShift - 1);
				}
				else
				{
					count = new int[] { 1 };
					c = (int[]) y.Clone();
					cStart = yStart;
				}

				Subtract(xStart, x, cStart, c);

				int[] iCount = (int[])count.Clone();
				int iCountStart = 0;

				for (;;)
				{
					while (xStart < x.Length && x[xStart] == 0)
					{
						xStart++;
					}

					while (cStart < c.Length && c[cStart] == 0)
					{
						cStart++;
					}

					while (CompareTo(xStart, x, cStart, c) >= 0)
					{
						Subtract(xStart, x, cStart, c);

						while (xStart < x.Length && x[xStart] == 0)
						{
							xStart++;
						}

						AddMagnitudes(count, iCount);
					}

					xyCmp = CompareTo(xStart, x, yStart, y);

					if (xyCmp > 0)
					{
						int secondShift = cBitLength - calcBitLength(xStart, x);

						if (secondShift < 2)
						{
							c = ShiftRightOneInPlace(cStart, c);
							--cBitLength;
							iCount = ShiftRightOneInPlace(iCountStart, iCount);
						}
						else
						{
							c = ShiftRightInPlace(cStart, c, secondShift);
							cBitLength -= secondShift;
							iCount = ShiftRightInPlace(iCountStart, iCount, secondShift);
						}

						if (iCount[iCountStart] == 0)
						{
							iCountStart++;
						}
					}
					else
					{
						if (xyCmp == 0)
						{
							AddMagnitudes(count, One.magnitude);
							Array.Clear(x, xStart, x.Length - xStart);
						}

						break;
					}
				}
			}
			else
			{
				count = new int[1];

				if (xyCmp == 0)
				{
					count[0] = 1;
					Array.Clear(x, 0, x.Length);
				}
			}

			return count;
		}

		public BigInteger Divide(
			BigInteger value)
		{
			if (value.sign == 0)
				throw new ArithmeticException("Division by zero error");

			if (sign == 0)
				return Zero;

//			if (value.Abs().Equals(One))
			if (value.magnitude.Length == 1 && value.magnitude[0] == 1)
			{
				return value.sign > 0 ? this : Negate();
			}

			int[] mag = (int[]) this.magnitude.Clone();

			return new BigInteger(this.sign * value.sign, Divide(mag, value.magnitude), true);
		}

		public BigInteger[] DivideAndRemainder(
			BigInteger value)
		{
			if (value.sign == 0)
				throw new ArithmeticException("Division by zero error");

			BigInteger[] biggies = new BigInteger[2];

			if (sign == 0)
			{
				biggies[0] = Zero;
				biggies[1] = Zero;
			}
//			else if (value.Abs().Equals(One)) // TODO Optimise this test?
			else if (value.magnitude.Length == 1 && value.magnitude[0] == 1)
			{
				biggies[0] = value.sign > 0 ? this : Negate();
				biggies[1] = Zero;
			}
			else
			{
				int[] remainder = (int[]) this.magnitude.Clone();
				int[] quotient = Divide(remainder, value.magnitude);

				biggies[0] = new BigInteger(this.sign * value.sign, quotient, true);
				biggies[1] = new BigInteger(this.sign, remainder, true);
			}

			return biggies;
		}

		public override bool Equals(
			object obj)
		{
			if (obj == this)
				return true;

			BigInteger biggie = obj as BigInteger;
			if (biggie == null)
				return false;

			if (biggie.sign != sign || biggie.magnitude.Length != magnitude.Length)
				return false;

			for (int i = 0; i < magnitude.Length; i++)
			{
				if (biggie.magnitude[i] != magnitude[i])
				{
					return false;
				}
			}

			return true;
		}

		public BigInteger Gcd(
			BigInteger value)
		{
			if (value.sign == 0)
				return Abs();

			if (sign == 0)
				return value.Abs();

			BigInteger r;
			BigInteger u = this;
			BigInteger v = value;

			while (v.sign != 0)
			{
				r = u.Mod(v);
				u = v;
				v = r;
			}

			return u;
		}

		public override int GetHashCode()
		{
			int hc = magnitude.Length;
			if (magnitude.Length > 0)
			{
				hc ^= magnitude[0];

				if (magnitude.Length > 1)
				{
					hc ^= magnitude[magnitude.Length - 1];
				}
			}

			return sign < 0 ? ~hc : hc;
		}

		// TODO Make public?
		private BigInteger Inc()
		{
			if (this.sign == 0)
				return One;

			if (this.sign < 0)
				return new BigInteger(-1, doSubBigLil(this.magnitude, One.magnitude), true);

			return AddToMagnitude(One.magnitude);
		}

		public int IntValue
		{
			get
			{
				return sign == 0 ? 0
					: sign > 0 ? magnitude[magnitude.Length - 1]
					: -magnitude[magnitude.Length - 1];
			}
		}

		/**
		 * return whether or not a BigInteger is probably prime with a
		 * probability of 1 - (1/2)**certainty.
		 * <p>From Knuth Vol 2, pg 395.</p>
		 */
		public bool IsProbablePrime(
			int certainty)
		{
			if (certainty <= 0)
				return true;

			BigInteger n = Abs();

			if (!n.TestBit(0))
				return n.Equals(Two);

			if (n.Equals(One))
				return false;

			return n.CheckProbablePrime(certainty, RandomSource);
		}

		private bool CheckProbablePrime(
			int		certainty,
			Random	random)
		{
			Debug.Assert(certainty > 0);
			Debug.Assert(CompareTo(Two) > 0);
			Debug.Assert(TestBit(0));


			// Try to reduce the penalty for really small numbers
			int numLists = System.Math.Min(BitLength - 2, primeLists.Length);

			for (int i = 0; i < numLists; ++i)
			{
				int test = Remainder(primeProducts[i]);

				int[] primeList = primeLists[i];
				for (int j = 0; j < primeList.Length; ++j)
				{
					int prime = primeList[j];
					int qRem = test % prime;
					if (qRem == 0)
					{
						// We may find small numbers in the list
						return BitLength < 16 && IntValue == prime;
					}
				}
			}


			// TODO Special case for < 10^16 (RabinMiller fixed list)
//			if (BitLength < 30)
//			{
//				RabinMiller against 2, 3, 5, 7, 11, 13, 23 is sufficient
//			}


			// TODO Is it worth trying to create a hybrid of these two?
			return RabinMillerTest(certainty, random);
//			return SolovayStrassenTest(certainty, random);

//			bool rbTest = RabinMillerTest(certainty, random);
//			bool ssTest = SolovayStrassenTest(certainty, random);
//
//			Debug.Assert(rbTest == ssTest);
//
//			return rbTest;
		}

		internal bool RabinMillerTest(
			int		certainty,
			Random	random)
		{
			Debug.Assert(certainty > 0);
			Debug.Assert(CompareTo(Two) > 0);
			Debug.Assert(TestBit(0));

			// let n = 1 + d . 2^s
			BigInteger n = this;
			int bitLengthOfN = n.BitLength;
			BigInteger nMinusOne = n.Subtract(One);
			int k = nMinusOne.GetLowestSetBit();
			BigInteger q = nMinusOne.ShiftRight(k);

			Debug.Assert(k >= 1);

			do
			{
				// TODO Make a method for random BigIntegers in range 0 < x < n)
				// - Method can be optimized by only replacing examined bits at each trial
				BigInteger x;
				do
				{
					x = new BigInteger(bitLengthOfN, random);
				}
				// NB: Spec says 0 < x < n, but 1 is trivial
				while (x.CompareTo(One) <= 0 || x.CompareTo(n) >= 0);

				BigInteger y = x.ModPow(q, n);

				if (!y.Equals(One))
				{
					// y already = x.ModPow(d << 0, n)
					int r = 0;

					while (!y.Equals(nMinusOne))
					{
						if (++r == k)
							return false;

						// y becomes x.ModPow(d << r, n)
						y = y.ModPow(Two, n);

						// TODO Confirm whether y.Equals(One) is worth testing here
					}
				}

				certainty -= 2; // composites pass for only 1/4 possible 'x'
			}
			while (certainty > 0);

			return true;
		}

//		private bool SolovayStrassenTest(
//			int		certainty,
//			Random	random)
//		{
//			Debug.Assert(certainty > 0);
//			Debug.Assert(CompareTo(Two) > 0);
//			Debug.Assert(TestBit(0));
//
//			BigInteger n = this;
//			BigInteger nMinusOne = n.Subtract(One);
//			BigInteger e = nMinusOne.ShiftRight(1);
//
//			do
//			{
//				BigInteger a;
//				do
//				{
//					a = new BigInteger(nBitLength, random);
//				}
//				// NB: Spec says 0 < x < n, but 1 is trivial
//				while (a.CompareTo(One) <= 0 || a.CompareTo(n) >= 0);
//
//
//				// TODO Check this is redundant given the way Jacobi() works?
////				if (!a.Gcd(n).Equals(One))
////					return false;
//
//				int x = Jacobi(a, n);
//
//				if (x == 0)
//					return false;
//
//				BigInteger check = a.ModPow(e, n);
//
//				if (x == 1 && !check.Equals(One))
//					return false;
//
//				if (x == -1 && !check.Equals(nMinusOne))
//					return false;
//
//				--certainty;
//			}
//			while (certainty > 0);
//
//			return true;
//		}
//
//		private static int Jacobi(
//			BigInteger	a,
//			BigInteger	b)
//		{
//			Debug.Assert(a.sign >= 0);
//			Debug.Assert(b.sign > 0);
//			Debug.Assert(b.TestBit(0));
//			Debug.Assert(a.CompareTo(b) < 0);
//
//			int totalS = 1;
//			for (;;)
//			{
//				if (a.sign == 0)
//					return 0;
//
//				if (a.Equals(One))
//					break;
//
//				int e = a.GetLowestSetBit();
//
//				int bLsw = b.magnitude[b.magnitude.Length - 1];
//				if ((e & 1) != 0 && ((bLsw & 7) == 3 || (bLsw & 7) == 5))
//					totalS = -totalS;
//
//				// TODO Confirm this is faster than later a1.Equals(One) test
//				if (a.BitLength == e + 1)
//					break;
//				BigInteger a1 = a.ShiftRight(e);
////				if (a1.Equals(One))
////					break;
//
//				int a1Lsw = a1.magnitude[a1.magnitude.Length - 1];
//				if ((bLsw & 3) == 3 && (a1Lsw & 3) == 3)
//					totalS = -totalS;
//
////				a = b.Mod(a1);
//				a = b.Remainder(a1);
//				b = a1;
//			}
//			return totalS;
//		}

		public long LongValue
		{
			get
			{
				if (sign == 0)
					return 0;

				long v;
				if (magnitude.Length > 1)
				{
					v = ((long)magnitude[magnitude.Length - 2] << 32)
						| (magnitude[magnitude.Length - 1] & IMASK);
				}
				else
				{
					v = (magnitude[magnitude.Length - 1] & IMASK);
				}

				return sign < 0 ? -v : v;
			}
		}

		public BigInteger Max(
			BigInteger value)
		{
			return CompareTo(value) > 0 ? this : value;
		}

		public BigInteger Min(
			BigInteger value)
		{
			return CompareTo(value) < 0 ? this : value;
		}

		public BigInteger Mod(
			BigInteger m)
		{
			if (m.sign < 1)
				throw new ArithmeticException("Modulus must be positive");

			BigInteger biggie = Remainder(m);

			return (biggie.sign >= 0 ? biggie : biggie.Add(m));
		}

		public BigInteger ModInverse(
			BigInteger m)
		{
			if (m.sign < 1)
				throw new ArithmeticException("Modulus must be positive");

			BigInteger x = new BigInteger();
			BigInteger gcd = ExtEuclid(this, m, x, null);

			if (!gcd.Equals(One))
				throw new ArithmeticException("Numbers not relatively prime.");

			if (x.sign < 0)
			{
				x.sign = 1;
				//x = m.Subtract(x);
				x.magnitude = doSubBigLil(m.magnitude, x.magnitude);
			}

			return x;
		}

		/**
		 * Calculate the numbers u1, u2, and u3 such that:
		 *
		 * u1 * a + u2 * b = u3
		 *
		 * where u3 is the greatest common divider of a and b.
		 * a and b using the extended Euclid algorithm (refer p. 323
		 * of The Art of Computer Programming vol 2, 2nd ed).
		 * This also seems to have the side effect of calculating
		 * some form of multiplicative inverse.
		 *
		 * @param a    First number to calculate gcd for
		 * @param b    Second number to calculate gcd for
		 * @param u1Out      the return object for the u1 value
		 * @param u2Out      the return object for the u2 value
		 * @return     The greatest common divisor of a and b
		 */
		private static BigInteger ExtEuclid(
			BigInteger	a,
			BigInteger	b,
			BigInteger	u1Out,
			BigInteger	u2Out)
		{
			BigInteger u1 = One;
			BigInteger u3 = a;
			BigInteger v1 = Zero;
			BigInteger v3 = b;

			while (v3.sign > 0)
			{
				BigInteger[] q = u3.DivideAndRemainder(v3);

				BigInteger tn = u1.Subtract(v1.Multiply(q[0]));
				u1 = v1;
				v1 = tn;

				tn = q[1];
				u3 = v3;
				v3 = tn;
			}

			if (u1Out != null)
			{
				u1Out.sign = u1.sign;
				u1Out.magnitude = u1.magnitude;
			}

			if (u2Out != null)
			{
				BigInteger res = u3.Subtract(u1.Multiply(a)).Divide(b);
				u2Out.sign = res.sign;
				u2Out.magnitude = res.magnitude;
			}

			return u3;
		}

		private static void ZeroOut(
			int[] x)
		{
			Array.Clear(x, 0, x.Length);
		}

		public BigInteger ModPow(
			BigInteger exponent,
			BigInteger m)
		{
			if (m.sign < 1)
				throw new ArithmeticException("Modulus must be positive");

			if (m.Equals(One))
				return Zero;

			if (exponent.sign == 0)
				return One;

			if (sign == 0)
				return Zero;

			int[] zVal = null;
			int[] yAccum = null;
			int[] yVal;

			// Montgomery exponentiation is only possible if the modulus is odd,
			// but AFAIK, this is always the case for crypto algo's
			bool useMonty = ((m.magnitude[m.magnitude.Length - 1] & 1) == 1);
			long mQ = 0;
			if (useMonty)
			{
				mQ = m.GetMQuote();

				// tmp = this * R mod m
				BigInteger tmp = ShiftLeft(32 * m.magnitude.Length).Mod(m);
				zVal = tmp.magnitude;

				useMonty = (zVal.Length <= m.magnitude.Length);

				if (useMonty)
				{
					yAccum = new int[m.magnitude.Length + 1];
					if (zVal.Length < m.magnitude.Length)
					{
						int[] longZ = new int[m.magnitude.Length];
						zVal.CopyTo(longZ, longZ.Length - zVal.Length);
						zVal = longZ;
					}
				}
			}

			if (!useMonty)
			{
				if (magnitude.Length <= m.magnitude.Length)
				{
					//zAccum = new int[m.magnitude.Length * 2];
					zVal = new int[m.magnitude.Length];
					magnitude.CopyTo(zVal, zVal.Length - magnitude.Length);
				}
				else
				{
					//
					// in normal practice we'll never see this...
					//
					BigInteger tmp = Remainder(m);

					//zAccum = new int[m.magnitude.Length * 2];
					zVal = new int[m.magnitude.Length];
					tmp.magnitude.CopyTo(zVal, zVal.Length - tmp.magnitude.Length);
				}

				yAccum = new int[m.magnitude.Length * 2];
			}

			yVal = new int[m.magnitude.Length];

			//
			// from LSW to MSW
			//
			for (int i = 0; i < exponent.magnitude.Length; i++)
			{
				int v = exponent.magnitude[i];
				int bits = 0;

				if (i == 0)
				{
					while (v > 0)
					{
						v <<= 1;
						bits++;
					}

					//
					// first time in initialise y
					//
					zVal.CopyTo(yVal, 0);

					v <<= 1;
					bits++;
				}

				while (v != 0)
				{
					if (useMonty)
					{
						// Montgomery square algo doesn't exist, and a normal
						// square followed by a Montgomery reduction proved to
						// be almost as heavy as a Montgomery mulitply.
						MultiplyMonty(yAccum, yVal, yVal, m.magnitude, mQ);
					}
					else
					{
						Square(yAccum, yVal);
						Remainder(yAccum, m.magnitude);
						Array.Copy(yAccum, yAccum.Length - yVal.Length, yVal, 0, yVal.Length);
						ZeroOut(yAccum);
					}
					bits++;

					if (v < 0)
					{
						if (useMonty)
						{
							MultiplyMonty(yAccum, yVal, zVal, m.magnitude, mQ);
						}
						else
						{
							Multiply(yAccum, yVal, zVal);
							Remainder(yAccum, m.magnitude);
							Array.Copy(yAccum, yAccum.Length - yVal.Length, yVal, 0,
								yVal.Length);
							ZeroOut(yAccum);
						}
					}

					v <<= 1;
				}

				while (bits < 32)
				{
					if (useMonty)
					{
						MultiplyMonty(yAccum, yVal, yVal, m.magnitude, mQ);
					}
					else
					{
						Square(yAccum, yVal);
						Remainder(yAccum, m.magnitude);
						Array.Copy(yAccum, yAccum.Length - yVal.Length, yVal, 0, yVal.Length);
						ZeroOut(yAccum);
					}
					bits++;
				}
			}

			if (useMonty)
			{
				// Return y * R^(-1) mod m by doing y * 1 * R^(-1) mod m
				ZeroOut(zVal);
				zVal[zVal.Length - 1] = 1;
				MultiplyMonty(yAccum, yVal, zVal, m.magnitude, mQ);
			}

			return new BigInteger(1, yVal, true);
		}

		/**
		 * return w with w = x * x - w is assumed to have enough space.
		 */
		private static int[] Square(
			int[]	w,
			int[]	x)
		{
			if (w.Length != 2 * x.Length)
				throw new ArgumentException("no I don't think so...");

			long u1, u2, c;

			for (int i = x.Length - 1; i != 0; i--)
			{
				long v = (x[i] & IMASK);

				u1 = v * v;
				u2 = (long)((ulong)u1 >> 32);
				u1 = u1 & IMASK;

				u1 += (w[2 * i + 1] & IMASK);

				w[2 * i + 1] = (int)u1;
				c = u2 + (u1 >> 32);

				for (int j = i - 1; j >= 0; j--)
				{
					u1 = (x[j] & IMASK) * v;
					u2 = (long)((ulong)u1 >> 31); // multiply by 2!
					u1 = (u1 & 0x7fffffff) << 1; // multiply by 2!
					u1 += (w[i + j + 1] & IMASK) + c;

					w[i + j + 1] = (int)u1;
					c = u2 + (long)((ulong)u1 >> 32);
				}
				c += w[i] & IMASK;
				w[i] = (int)c;
				w[i - 1] = (int)(c >> 32);
			}

			u1 = (x[0] & IMASK);
			u1 = u1 * u1;
			u2 = (long)((ulong)u1 >> 32);
			u1 = u1 & IMASK;

			u1 += (w[1] & IMASK);

			w[1] = (int)u1;
			w[0] = (int)(u2 + (u1 >> 32) + w[0]);

			return w;
		}

		/**
		 * return x with x = y * z - x is assumed to have enough space.
		 */
		private static int[] Multiply(
			int[]	x,
			int[]	y,
			int[]	z)
		{
			for (int i = z.Length - 1; i >= 0; i--)
			{
				long a = z[i] & IMASK;
				long value = 0;

				for (int j = y.Length - 1; j >= 0; j--)
				{
					value += a * (y[j] & IMASK) + (x[i + j + 1] & IMASK);

					x[i + j + 1] = (int)value;

					value = (long)((ulong)value >> 32);
				}

				x[i] = (int)value;
			}

			return x;
		}

		private static long FastExtEuclid(
			long	a,
			long	b,
			long[]	uOut)
		{
			long res;

			long u1 = 1;
			long u3 = a;
			long v1 = 0;
			long v3 = b;

			while (v3 > 0)
			{
				long q, tn;

				q = u3 / v3;

				tn = u1 - (v1 * q);
				u1 = v1;
				v1 = tn;

				tn = u3 - (v3 * q);
				u3 = v3;
				v3 = tn;
			}

			uOut[0] = u1;

			res = (u3 - (u1 * a)) / b;
			uOut[1] = res;

			return u3;
		}

		private static long FastModInverse(
			long	v,
			long	m)
		{
			if (m < 0)
			{
				throw new ArithmeticException("Modulus must be positive");
			}

			long[] x = new long[2];

			long gcd = FastExtEuclid(v, m, x);

			if (gcd != 1)
			{
				throw new ArithmeticException("Numbers not relatively prime.");
			}

			if (x[0] < 0)
			{
				x[0] += m;
			}

			return x[0];
		}

//		private static BigInteger MQuoteB = One.ShiftLeft(32);
//		private static BigInteger MQuoteBSub1 = MQuoteB.Subtract(One);

		/**
		 * Calculate mQuote = -m^(-1) mod b with b = 2^32 (32 = word size)
		 */
		private long GetMQuote()
		{
			Debug.Assert(this.sign > 0);

			if (mQuote != -1)
			{
				return mQuote; // already calculated
			}

			if (magnitude.Length == 0 || (magnitude[magnitude.Length - 1] & 1) == 0)
			{
				return -1; // not for even numbers
			}

			long v = (((~this.magnitude[this.magnitude.Length - 1]) | 1) & 0xffffffffL);
			mQuote = FastModInverse(v, 0x100000000L);

			return mQuote;

//			//return this.Negate().Mod(MQuoteB).ModInverse(MQuoteB).LongValue;
//
//			BigInteger mod = MQuoteB.Subtract(this.And(MQuoteBSub1));
//			//return mod.ModInverse(MQuoteB).LongValue;
//
//			BigInteger x = new BigInteger();
//			ExtEuclid(mod, MQuoteB, x, null);
//			mQuote = x.LongValue;
//
//			return mQuote;
		}

		/**
		 * Montgomery multiplication: a = x * y * R^(-1) mod m
		 * <br/>
		 * Based algorithm 14.36 of Handbook of Applied Cryptography.
		 * <br/>
		 * <li> m, x, y should have length n </li>
		 * <li> a should have length (n + 1) </li>
		 * <li> b = 2^32, R = b^n </li>
		 * <br/>
		 * The result is put in x
		 * <br/>
		 * NOTE: the indices of x, y, m, a different in HAC and in Java
		 */
		private static void MultiplyMonty(
			int[]	a,
			int[]	x,
			int[]	y,
			int[]	m,
			long	mQuote)
			// mQuote = -m^(-1) mod b
		{
			if (m.Length == 1)
			{
				x[0] = (int)MultiplyMontyNIsOne((uint)x[0], (uint)y[0], (uint)m[0], (ulong)mQuote);
				return;
			}

			int n = m.Length;
			int nMinus1 = n - 1;
			long y_0 = y[n - 1] & IMASK;

			// 1. a = 0 (Notation: a = (a_{n} a_{n-1} ... a_{0})_{b} )
			Array.Clear(a, 0, n + 1);

			// 2. for i from 0 to (n - 1) do the following:
			for (int i = n; i > 0; i--)
			{

				long x_i = x[i - 1] & IMASK;

				// 2.1 u = ((a[0] + (x[i] * y[0]) * mQuote) mod b
				long u = ((((a[n] & IMASK) + ((x_i * y_0) & IMASK)) & IMASK) * mQuote) & IMASK;

				// 2.2 a = (a + x_i * y + u * m) / b
				long prod1 = x_i * y_0;
				long prod2 = u * (m[n - 1] & IMASK);
				long tmp = (a[n] & IMASK) + (prod1 & IMASK) + (prod2 & IMASK);
				long carry = (long)((ulong)prod1 >> 32) + (long)((ulong)prod2 >> 32) + (long)((ulong)tmp >> 32);
				for (int j = nMinus1; j > 0; j--)
				{
					prod1 = x_i * (y[j - 1] & IMASK);
					prod2 = u * (m[j - 1] & IMASK);
					tmp = (a[j] & IMASK) + (prod1 & IMASK) + (prod2 & IMASK) + (carry & IMASK);
					carry = (long)((ulong)carry >> 32) + (long)((ulong)prod1 >> 32) +
						(long)((ulong)prod2 >> 32) + (long)((ulong)tmp >> 32);
					a[j + 1] = (int)tmp; // division by b
				}
				carry += (a[0] & IMASK);
				a[1] = (int)carry;
				a[0] = (int)((ulong)carry >> 32); // OJO!!!!!
			}

			// 3. if x >= m the x = x - m
			if (CompareTo(0, a, 0, m) >= 0)
			{
				Subtract(0, a, 0, m);
			}

			// put the result in x
			Array.Copy(a, 1, x, 0, n);
		}

		private static uint MultiplyMontyNIsOne(
			uint	x,
			uint	y,
			uint	m,
			ulong	mQuote)
		{
			ulong um = m;
			ulong prod1 = (ulong)x * (ulong)y;
			ulong u = (prod1 * mQuote) & UIMASK;
			ulong prod2 = u * um;
			ulong tmp = (prod1 & UIMASK) + (prod2 & UIMASK);
			ulong carry = (prod1 >> 32) + (prod2 >> 32) + (tmp >> 32);

			if (carry > um)
			{
				carry -= um;
			}

			return (uint)(carry & UIMASK);
		}

		public BigInteger Multiply(
			BigInteger value)
		{
			if (sign == 0 || value.sign == 0)
				return Zero;

			int[] res = new int[magnitude.Length + value.magnitude.Length];

			return new BigInteger(sign * value.sign, Multiply(res, magnitude, value.magnitude), true);
		}

		public BigInteger Negate()
		{
			return sign == 0 ? this : new BigInteger(-sign, magnitude, false);
		}

		public BigInteger NextProbablePrime()
		{
			if (sign < 0)
				throw new ArithmeticException("Cannot be called on value < 0");

			if (CompareTo(Two) < 0)
				return Two;

			BigInteger n = Inc().SetBit(0);

			while (!n.CheckProbablePrime(100, RandomSource))
			{
				n = n.Add(Two);
			}

			return n;
		}

		public BigInteger Not()
		{
			return Inc().Negate();
		}

		public BigInteger Pow(int exp)
		{
			if (exp < 0)
			{
				throw new ArithmeticException("Negative exponent");
			}

			if (exp == 0)
			{
				return One;
			}

			if (sign == 0 || Equals(One))
			{
				return this;
			}

			BigInteger y = One;
			BigInteger z = this;

			for (;;)
			{
				if ((exp & 0x1) == 1)
				{
					y = y.Multiply(z);
				}
				exp >>= 1;
				if (exp == 0) break;
				z = z.Multiply(z);
			}

			return y;
		}

		public static BigInteger ProbablePrime(
			int bitLength,
			Random random)
		{
			return new BigInteger(bitLength, 100, random);
		}

		private int Remainder(
			int m)
		{
			Debug.Assert(m > 0);

			long acc = 0;
			for (int pos = 0; pos < magnitude.Length; ++pos)
			{
				long posVal = (uint) magnitude[pos];
				acc = (acc << 32 | posVal) % m;
			}

			return (int) acc;
		}

		/**
		 * return x = x % y - done in place (y value preserved)
		 */
		private int[] Remainder(
			int[] x,
			int[] y)
		{
			int xStart = 0;
			while (xStart < x.Length && x[xStart] == 0)
			{
				xStart++;
			}

			int yStart = 0;
			while (yStart < y.Length && y[yStart] == 0)
			{
				yStart++;
			}

			int xyCmp = CompareTo(xStart, x, yStart, y);

			if (xyCmp > 0)
			{
				int[] c;
				int cBitLength = calcBitLength(yStart, y);
				int firstShift = calcBitLength(xStart, x) - cBitLength;

				int cStart;
				if (--firstShift > 0)
				{
					c = ShiftLeft(y, firstShift);
					cBitLength += firstShift;
					cStart = 0;
				}
				else
				{
					c = (int[])y.Clone();
					cStart = yStart;
				}

				Subtract(xStart, x, cStart, c);

				for (;;)
				{
					while (xStart < x.Length && x[xStart] == 0)
					{
						xStart++;
					}

					while (cStart < c.Length && c[cStart] == 0)
					{
						cStart++;
					}

					while (CompareTo(xStart, x, cStart, c) >= 0)
					{
						Subtract(xStart, x, cStart, c);

						while (xStart < x.Length && x[xStart] == 0)
						{
							xStart++;
						}
					}

					xyCmp = CompareTo(xStart, x, yStart, y);

					if (xyCmp > 0)
					{
						int secondShift = cBitLength - calcBitLength(xStart, x);

						if (secondShift < 2)
						{
							c = ShiftRightOneInPlace(cStart, c);
							--cBitLength;
						}
						else
						{
							c = ShiftRightInPlace(cStart, c, secondShift);
							cBitLength -= secondShift;
						}
					}
					else
					{
						if (xyCmp == 0)
						{
							Array.Clear(x, xStart, x.Length - xStart);
						}

						break;
					}
				}
			}
			else if (xyCmp == 0)
			{
				Array.Clear(x, 0, x.Length);
			}

			return x;
		}

		public BigInteger Remainder(
			BigInteger value)
		{
			if (value.sign == 0)
				throw new ArithmeticException("Division by zero error");

			if (this.sign == 0)
				return Zero;

			// For small values, use fast remainder method
			if (value.magnitude.Length == 1)
			{
				int val = value.magnitude[0];

				if (val > 0)
				{
					if (val == 1)
						return Zero;

					// TODO Make this func work on uint, and handle val == 1?
					int rem = Remainder(val);

					return rem == 0
						?	Zero
						:	new BigInteger(sign, new int[]{ rem }, false);
				}
			}

			if (CompareTo(0, magnitude, 0, value.magnitude) < 0)
				return this;

			int[] result = (int[]) this.magnitude.Clone();

			return new BigInteger(sign, Remainder(result, value.magnitude), true);
		}

		/**
		 * do a left shift - this returns a new array.
		 */
		private static int[] ShiftLeft(
			int[] mag,
			int n)
		{
			int nInts = (int)((uint)n >> 5);
			int nBits = n & 0x1f;
			int magLen = mag.Length;
			int[] newMag;

			if (nBits == 0)
			{
				newMag = new int[magLen + nInts];
				mag.CopyTo(newMag, 0);
			}
			else
			{
				int i = 0;
				int nBits2 = 32 - nBits;
				int highBits = (int)((uint)mag[0] >> nBits2);

				if (highBits != 0)
				{
					newMag = new int[magLen + nInts + 1];
					newMag[i++] = highBits;
				}
				else
				{
					newMag = new int[magLen + nInts];
				}

				int m = mag[0];
				for (int j = 0; j < magLen - 1; j++)
				{
					int next = mag[j + 1];

					newMag[i++] = (m << nBits) | (int)((uint)next >> nBits2);
					m = next;
				}

				newMag[i] = mag[magLen - 1] << nBits;
			}

			return newMag;
		}

		public BigInteger ShiftLeft(
			int n)
		{
			if (sign == 0 || magnitude.Length == 0)
				return Zero;

			if (n == 0)
				return this;

			if (n < 0)
				return ShiftRight(-n);

			return new BigInteger(sign, ShiftLeft(magnitude, n), true);
		}

		/**
		 * do a right shift - this does it in place.
		 */
		private static int[] ShiftRightInPlace(
			int start,
			int[] mag,
			int n)
		{
			int nInts = (int)((uint)n >> 5) + start;
			int nBits = n & 0x1f;
			int magLen = mag.Length;

			if (nInts != start)
			{
				int delta = (nInts - start);

				for (int i = magLen - 1; i >= nInts; i--)
				{
					mag[i] = mag[i - delta];
				}
				for (int i = nInts - 1; i >= start; i--)
				{
					mag[i] = 0;
				}
			}

			if (nBits != 0)
			{
				int nBits2 = 32 - nBits;
				int m = mag[magLen - 1];

				for (int i = magLen - 1; i >= nInts + 1; i--)
				{
					int next = mag[i - 1];

					mag[i] = (int)((uint)m >> nBits) | (next << nBits2);
					m = next;
				}

				mag[nInts] = (int)((uint)mag[nInts] >> nBits);
			}

			return mag;
		}

		/**
		 * do a right shift by one - this does it in place.
		 */
		private static int[] ShiftRightOneInPlace(
			int     start,
			int[]   mag)
		{
			int i = mag.Length;
			int m = mag[i - 1];

            while (--i > start)
			{
				int next = mag[i - 1];
				mag[i] = ((int)((uint)m >> 1)) | (next << 31);
				m = next;
			}

            mag[start] = (int)((uint)mag[start] >> 1);

            return mag;
		}

        public BigInteger ShiftRight(
			int n)
		{
			if (n == 0)
				return this;

			if (n < 0)
				return ShiftLeft(-n);

			if (n >= BitLength)
				return (this.sign < 0 ? One.Negate() : Zero);

//			int[] res = (int[]) this.magnitude.Clone();
//
//			res = ShiftRightInPlace(0, res, n);
//
//			return new BigInteger(this.sign, res, true);

			int resultLength = (BitLength - n + 31) >> 5;
			int[] res = new int[resultLength];

			int numInts = n >> 5;
			int numBits = n & 31;

			if (numBits == 0)
			{
				Array.Copy(this.magnitude, 0, res, 0, res.Length);
			}
			else
			{
				int numBits2 = 32 - numBits;

				int magPos = this.magnitude.Length - 1 - numInts;
				for (int i = resultLength - 1; i >= 0; --i)
				{
					res[i] = (int)((uint) this.magnitude[magPos--] >> numBits);

					if (magPos >= 0)
					{
						res[i] |= this.magnitude[magPos] << numBits2;
					}
				}
			}

			Debug.Assert(res[0] != 0);

			return new BigInteger(this.sign, res, false);
		}

		public int SignValue
		{
			get { return sign; }
		}

		/**
		 * returns x = x - y - we assume x is >= y
		 */
		private static int[] Subtract(
			int		xStart,
			int[]	x,
			int		yStart,
			int[]	y)
		{
			Debug.Assert(x.Length - xStart >= y.Length - yStart);

			int iT = x.Length - 1;
			int iV = y.Length - 1;
			long m;
			int borrow = 0;

			do
			{
				m = (x[iT] & IMASK) - (y[iV--] & IMASK) + borrow;

				x[iT--] = (int) m;

				borrow = (m < 0) ? -1 : 0;
			}
			while (iV >= yStart);

			while (iT >= xStart)
			{
				m = (x[iT] & IMASK) + borrow;
				x[iT--] = (int)m;

				if (m >= 0)
					break;

				borrow = -1;
			}

			return x;
		}

		public BigInteger Subtract(
			BigInteger value)
		{
			if (value.sign == 0)
				return this;

			if (this.sign == 0)
				return value.Negate();

			if (this.sign != value.sign)
				return Add(value.Negate());

			int compare = CompareTo(0, magnitude, 0, value.magnitude);
			if (compare == 0)
				return Zero;

			BigInteger bigun, lilun;
			if (compare < 0)
			{
				bigun = value;
				lilun = this;
			}
			else
			{
				bigun = this;
				lilun = value;
			}

			return new BigInteger(this.sign * compare, doSubBigLil(bigun.magnitude, lilun.magnitude), true);
		}

		private static int[] doSubBigLil(
			int[]	bigMag,
			int[]	lilMag)
		{
			int[] res = (int[]) bigMag.Clone();

			return Subtract(0, res, 0, lilMag);
		}

		public byte[] ToByteArray()
		{
			return ToByteArray(true);
		}

		public byte[] ToByteArrayUnsigned()
		{
			return ToByteArray(false);
		}

		private byte[] ToByteArray(
			bool includeSignBit)
		{
			int bitLength = BitLength;
			byte[] bytes = new byte[bitLength / BitsPerByte + 1];

			int bytesCopied = 4;
			int mag = 0;
			int ofs = magnitude.Length - 1;
			int carry = 1;
			long lMag;
			for (int i = bytes.Length - 1; i >= 0; i--)
			{
				if (bytesCopied == 4 && ofs >= 0)
				{
					if (sign < 0)
					{
						// we are dealing with a +ve number and we want a -ve one, so
						// invert the magnitude ints and add 1 (propagating the carry)
						// to make a 2's complement -ve number
						lMag = ~magnitude[ofs--] & IMASK;
						lMag += carry;
						if ((lMag & ~IMASK) != 0)
							carry = 1;
						else
							carry = 0;
						mag = (int)(lMag & IMASK);
					}
					else
					{
						mag = magnitude[ofs--];
					}
					bytesCopied = 1;
				}
				else
				{
					mag = (int)((uint)mag >> 8);
					bytesCopied++;
				}

				bytes[i] = (byte)mag;
			}

			// TODO Optimise the unsigned case
			if (!includeSignBit && bytes[0] == 0)
			{
				byte[] tmp = new byte[bytes.Length - 1];
				Array.Copy(bytes, 1, tmp, 0, tmp.Length);
				bytes = tmp;
			}

			return bytes;
		}

		public override string ToString()
		{
			return ToString(10);
		}

		public string ToString(
			int radix)
		{
			// TODO Make this method work for other radices (ideally 2 <= radix <= 16)

			switch (radix)
			{
				case 2:
				case 10:
				case 16:
					break;
				default:
					throw new FormatException("Only base 10 or 16 are allowed");
			}

			// NB: Can only happen to internally managed instances
			if (magnitude == null)
				return "null";

			if (sign == 0)
				return "0";

			Debug.Assert(magnitude.Length > 0);

			StringBuilder sb = new StringBuilder();

			if (radix == 16)
			{
				sb.Append(magnitude[0].ToString("x"));

				for (int i = 1; i < magnitude.Length; i++)
				{
					sb.Append(magnitude[i].ToString("x8"));
				}
			}
			else if (radix == 2)
			{
				for (int i = BitLength - 1; i >= 0; --i)
				{
					sb.Append(TestBit(i) ? '1' : '0');
				}
			}
			else
			{
				// This is algorithm 1a from chapter 4.4 in Seminumerical Algorithms, slow but it works
				Stack S = new Stack();
				BigInteger bs = ValueOf(radix);

				// The sign is handled separatly.
				// Notice however that for this to work, radix 16 _MUST_ be a special case,
				// unless we want to enter a recursion well. In their infinite wisdom, why did not
				// the Sun engineers made a c'tor for BigIntegers taking a BigInteger as parameter?
				// (Answer: Becuase Sun's BigIntger is clonable, something bouncycastle's isn't.)
				BigInteger u = new BigInteger(Abs().ToString(16), 16);
				BigInteger b;

				while (u.sign != 0)
				{
					b = u.Mod(bs);
					if (b.sign == 0)
					{
						S.Push("0");
					}
					else
					{
						// see how to interact with different bases
						S.Push(b.magnitude[0].ToString("d"));
					}
					u = u.Divide(bs);
				}

				// Then pop the stack
				while (S.Count != 0)
				{
					sb.Append((string) S.Pop());
				}
			}

			string s = sb.ToString();

			Debug.Assert(s.Length > 0);

			// Strip leading zeros. (We know this number is not all zeroes though)
			if (s[0] == '0')
			{
				int nonZeroPos = 0;
				while (s[++nonZeroPos] == '0') {}

				s = s.Substring(nonZeroPos);
			}

			if (sign == -1)
			{
				s = "-" + s;
			}

			return s;
		}

		private static BigInteger createUValueOf(
			ulong value)
		{
			int msw = (int)(value >> 32);
			int lsw = (int)value;

			if (msw != 0)
				return new BigInteger(1, new int[] { msw, lsw }, false);

			if (lsw != 0)
				return new BigInteger(1, new int[] { lsw }, false);

			return Zero;
		}

		private static BigInteger createValueOf(
			long value)
		{
			if (value < 0)
			{
				if (value == long.MinValue)
					return createValueOf(~value).Not();

				return createValueOf(-value).Negate();
			}

			return createUValueOf((ulong)value);

//			// store value into a byte array
//			byte[] b = new byte[8];
//			for (int i = 0; i < 8; i++)
//			{
//				b[7 - i] = (byte)value;
//				value >>= 8;
//			}
//
//			return new BigInteger(b);
		}

		public static BigInteger ValueOf(
			long value)
		{
			switch (value)
			{
				case 0:
					return Zero;
				case 1:
					return One;
				case 2:
					return Two;
				case 3:
					return Three;
				case 10:
					return Ten;
			}

			return createValueOf(value);
		}

		public int GetLowestSetBit()
		{
			if (this.sign == 0)
				return -1;

			int w = magnitude.Length - 1;

			while (w >= 0)
			{
				if (magnitude[w] != 0)
					break;

				w--;
			}

			int b = 31;

			while (b > 0)
			{
				if ((uint)(magnitude[w] << b) == 0x80000000)
					break;

				b--;
			}

			return (((magnitude.Length - 1) - w) * 32 + (31 - b));
		}

		public bool TestBit(
			int n)
		{
			if (n < 0)
				throw new ArithmeticException("Bit position must not be negative");

			if (sign < 0)
				return !Not().TestBit(n);

			int wordNum = n / 32;
			if (wordNum >= magnitude.Length)
				return false;

			int word = magnitude[magnitude.Length - 1 - wordNum];
			return ((word >> (n % 32)) & 1) > 0;
		}

		public BigInteger Or(
			BigInteger value)
		{
			if (this.sign == 0)
				return value;

			if (value.sign == 0)
				return this;

			int[] aMag = this.sign > 0
				? this.magnitude
				: Add(One).magnitude;

			int[] bMag = value.sign > 0
				? value.magnitude
				: value.Add(One).magnitude;

			bool resultNeg = sign < 0 || value.sign < 0;
			int resultLength = System.Math.Max(aMag.Length, bMag.Length);
			int[] resultMag = new int[resultLength];

			int aStart = resultMag.Length - aMag.Length;
			int bStart = resultMag.Length - bMag.Length;

			for (int i = 0; i < resultMag.Length; ++i)
			{
				int aWord = i >= aStart ? aMag[i - aStart] : 0;
				int bWord = i >= bStart ? bMag[i - bStart] : 0;

				if (this.sign < 0)
				{
					aWord = ~aWord;
				}

				if (value.sign < 0)
				{
					bWord = ~bWord;
				}

				resultMag[i] = aWord | bWord;

				if (resultNeg)
				{
					resultMag[i] = ~resultMag[i];
				}
			}

			BigInteger result = new BigInteger(1, resultMag, true);

			// TODO Optimise this case
			if (resultNeg)
			{
				result = result.Not();
			}

			return result;
		}

		public BigInteger Xor(
			BigInteger value)
		{
			if (this.sign == 0)
				return value;

			if (value.sign == 0)
				return this;

			int[] aMag = this.sign > 0
				? this.magnitude
				: Add(One).magnitude;

			int[] bMag = value.sign > 0
				? value.magnitude
				: value.Add(One).magnitude;

			bool resultNeg = (sign < 0 && value.sign >= 0) || (sign >= 0 && value.sign < 0);
			int resultLength = System.Math.Max(aMag.Length, bMag.Length);
			int[] resultMag = new int[resultLength];

			int aStart = resultMag.Length - aMag.Length;
			int bStart = resultMag.Length - bMag.Length;

			for (int i = 0; i < resultMag.Length; ++i)
			{
				int aWord = i >= aStart ? aMag[i - aStart] : 0;
				int bWord = i >= bStart ? bMag[i - bStart] : 0;

				if (this.sign < 0)
				{
					aWord = ~aWord;
				}

				if (value.sign < 0)
				{
					bWord = ~bWord;
				}

				resultMag[i] = aWord ^ bWord;

				if (resultNeg)
				{
					resultMag[i] = ~resultMag[i];
				}
			}

			BigInteger result = new BigInteger(1, resultMag, true);

			// TODO Optimise this case
			if (resultNeg)
			{
				result = result.Not();
			}

			return result;
		}

		public BigInteger SetBit(
			int n)
		{
			if (n < 0)
				throw new ArithmeticException("Bit address less than zero");

			if (TestBit(n))
				return this;

			if (n < (BitLength - 1))
			{
				int[] mag = (int[]) this.magnitude.Clone();
				mag[mag.Length - 1 - (n >> 5)] ^= (1 << (n & 31)); // Flip 0 bit to 1
				//mag[mag.Length - 1 - (n / 32)] |= (1 << (n % 32));
				BigInteger result = new BigInteger(this.sign, mag, false);
				result.nBitLength = nBitLength;
				return result;
			}

			return Or(One.ShiftLeft(n));
		}

		public BigInteger ClearBit(
			int n)
		{
			if (n < 0)
				throw new ArithmeticException("Bit address less than zero");

			if (!TestBit(n))
				return this;

			if (n < (BitLength - 1))
			{
				int[] mag = (int[]) this.magnitude.Clone();
				mag[mag.Length - 1 - (n >> 5)] ^= (1 << (n & 31)); // Flip 1 bit to 0
				//mag[mag.Length - 1 - (n / 32)] &= ~(1 << (n % 32));
				BigInteger result = new BigInteger(this.sign, mag, false);
				result.nBitLength = nBitLength;
				return result;
			}

			return AndNot(One.ShiftLeft(n));
		}

		public BigInteger FlipBit(
			int n)
		{
			if (n < 0)
				throw new ArithmeticException("Bit address less than zero");

			if (n < (BitLength - 1))
			{
				int[] mag = (int[]) this.magnitude.Clone();
				mag[mag.Length - 1 - (n >> 5)] ^= (1 << (n & 31)); // Flip bit
//				mag[mag.Length - 1 - (n / 32)] ^= (1 << (n % 32));
				BigInteger result = new BigInteger(this.sign, mag, false);
				result.nBitLength = nBitLength;
				return result;
			}

			return Xor(One.ShiftLeft(n));
		}
	}
}
