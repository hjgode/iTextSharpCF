using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.Collections;
using System.Text;

namespace Org.BouncyCastle.Asn1.Utilities
{
    public sealed class Asn1Dump
    {
        private Asn1Dump()
        {
        }

        private const string  TAB = "    ";

        /**
         * dump a Der object as a formatted string with indentation
         *
         * @param obj the Asn1Object to be dumped out.
         */
        private static string AsString(
            string		indent,
            Asn1Object	obj)
        {
            if (obj is Asn1Sequence)
            {
                StringBuilder    Buffer = new StringBuilder();
                string          tab = indent + TAB;

                Buffer.Append(indent);
                if (obj is DerSequence)
                {
                    Buffer.Append("DER Sequence");
                }
                else if (obj is BerSequence)
                {
                    Buffer.Append("BER Sequence");
                }
                else
                {
                    Buffer.Append("Sequence");
                }

                Buffer.Append(
// MASC 20070308. CF compatibility patch
#if !NETCF
					Environment.NewLine
#else
					EnvironmentEx.NewLine
#endif
				);

				foreach (object o in ((Asn1Sequence)obj))
				{
                    if (o == null || o.Equals(DerNull.Instance))
                    {
                        Buffer.Append(tab);
                        Buffer.Append("Null");
                        Buffer.Append(
// MASC 20070308. CF compatibility patch
#if !NETCF
							Environment.NewLine
#else
							EnvironmentEx.NewLine
#endif					
						);
                    }
                    else if (o is Asn1Object)
                    {
                        Buffer.Append(AsString(tab, (Asn1Object)o));
                    }
                    else
                    {
                        Buffer.Append(AsString(tab, ((Asn1Encodable)o).ToAsn1Object()));
                    }
                }
                return Buffer.ToString();
            }
            else if (obj is DerTaggedObject)
            {
                StringBuilder Buffer = new StringBuilder();
                string tab = indent + TAB;

				Buffer.Append(indent);
                if (obj is BerTaggedObject)
                {
                    Buffer.Append("BER Tagged [");
                }
                else
                {
                    Buffer.Append("Tagged [");
                }

				DerTaggedObject o = (DerTaggedObject)obj;

				Buffer.Append(((int)o.TagNo).ToString());
                Buffer.Append(']');

				if (!o.IsExplicit())
                {
                    Buffer.Append(" IMPLICIT ");
                }

				Buffer.Append(
// MASC 20070308. CF compatibility patch
#if !NETCF
					Environment.NewLine
#else
					EnvironmentEx.NewLine
#endif					
				);

				if (o.IsEmpty())
                {
                    Buffer.Append(tab);
                    Buffer.Append("EMPTY");
                    Buffer.Append(
// MASC 20070308. CF compatibility patch
#if !NETCF
						Environment.NewLine
#else
						EnvironmentEx.NewLine
#endif					
					);
                }
                else
                {
                    Buffer.Append(AsString(tab, o.GetObject()));
                }

				return Buffer.ToString();
            }
            else if (obj is BerSet)
            {
                StringBuilder Buffer = new StringBuilder();
                string tab = indent + TAB;

				Buffer.Append(indent);
                Buffer.Append("BER Set");
                Buffer.Append(
// MASC 20070308. CF compatibility patch
#if !NETCF
					Environment.NewLine
#else
					EnvironmentEx.NewLine
#endif					
				);

				foreach (object o in ((Asn1Set)obj))
				{
                    if (o == null)
                    {
                        Buffer.Append(tab);
                        Buffer.Append("Null");
                        Buffer.Append(
// MASC 20070308. CF compatibility patch
#if !NETCF
							Environment.NewLine
#else
							EnvironmentEx.NewLine
#endif					
						);
                    }
                    else if (o is Asn1Object)
                    {
                        Buffer.Append(AsString(tab, (Asn1Object)o));
                    }
                    else
                    {
                        Buffer.Append(AsString(tab, ((Asn1Encodable)o).ToAsn1Object()));
                    }
                }
                return Buffer.ToString();
            }
            else if (obj is DerSet)
            {
                StringBuilder Buffer = new StringBuilder();
                string tab = indent + TAB;

				Buffer.Append(indent);
                Buffer.Append("DER Set");
                Buffer.Append(
// MASC 20070308. CF compatibility patch
#if !NETCF
					Environment.NewLine
#else
					EnvironmentEx.NewLine
#endif					
				);

				foreach (object o in ((Asn1Set)obj))
				{
                    if (o == null)
                    {
                        Buffer.Append(tab);
                        Buffer.Append("Null");
                        Buffer.Append(
// MASC 20070308. CF compatibility patch
#if !NETCF
							Environment.NewLine
#else
							EnvironmentEx.NewLine
#endif					
						);
                    }
                    else if (o is Asn1Object)
                    {
                        Buffer.Append(AsString(tab, (Asn1Object)o));
                    }
                    else
                    {
                        Buffer.Append(AsString(tab, ((Asn1Encodable)o).ToAsn1Object()));
                    }
                }

				return Buffer.ToString();
            }
            else if (obj is DerObjectIdentifier)
            {
                return indent + "ObjectIdentifier(" + ((DerObjectIdentifier)obj).Id + ")" + 
// MASC 20070308. CF compatibility patch
#if !NETCF
					Environment.NewLine
#else
					EnvironmentEx.NewLine
#endif					
				;
            }
            else if (obj is DerBoolean)
            {
                return indent + "Boolean(" + ((DerBoolean)obj).IsTrue + ")" + 
// MASC 20070308. CF compatibility patch
#if !NETCF
					Environment.NewLine
#else
					EnvironmentEx.NewLine
#endif					
				;
            }
            else if (obj is DerInteger)
            {
                return indent + "Integer(" + ((DerInteger)obj).Value + ")" + 
// MASC 20070308. CF compatibility patch
#if !NETCF
					Environment.NewLine
#else
					EnvironmentEx.NewLine
#endif					
				;
            }
            else if (obj is DerOctetString)
            {
                return indent + obj.ToString() + "[" + ((Asn1OctetString)obj).GetOctets().Length + "] " + 
// MASC 20070308. CF compatibility patch
#if !NETCF
					Environment.NewLine
#else
					EnvironmentEx.NewLine
#endif					
				;
            }
            else if (obj is DerIA5String)
            {
                return indent + "IA5String(" + ((DerIA5String)obj).GetString() + ") " + 
// MASC 20070308. CF compatibility patch
#if !NETCF
					Environment.NewLine
#else
					EnvironmentEx.NewLine
#endif					
				;
            }
            else if (obj is DerPrintableString)
            {
                return indent + "PrintableString(" + ((DerPrintableString)obj).GetString() + ") " + 
// MASC 20070308. CF compatibility patch
#if !NETCF
					Environment.NewLine
#else
					EnvironmentEx.NewLine
#endif					
				;
            }
            else if (obj is DerVisibleString)
            {
                return indent + "VisibleString(" + ((DerVisibleString)obj).GetString() + ") " + 
// MASC 20070308. CF compatibility patch
#if !NETCF
					Environment.NewLine
#else
					EnvironmentEx.NewLine
#endif					
				;            
			}
            else if (obj is DerBmpString)
            {
                return indent + "BMPString(" + ((DerBmpString)obj).GetString() + ") " + 
// MASC 20070308. CF compatibility patch
#if !NETCF
					Environment.NewLine
#else
					EnvironmentEx.NewLine
#endif					
				;            
			}
            else if (obj is DerT61String)
            {
                return indent + "T61String(" + ((DerT61String)obj).GetString() + ") " + 
// MASC 20070308. CF compatibility patch
#if !NETCF
					Environment.NewLine
#else
					EnvironmentEx.NewLine
#endif					
				;            
			}
            else if (obj is DerUtcTime)
            {
                return indent + "UTCTime(" + ((DerUtcTime)obj).TimeString + ") " + 
// MASC 20070308. CF compatibility patch
#if !NETCF
					Environment.NewLine
#else
					EnvironmentEx.NewLine
#endif					
				;           
			}
            else if (obj is DerUnknownTag)
            {
				// MASC 20070308. CF compatibility patch
				byte[] hex = Hex.Encode(((DerUnknownTag)obj).GetData());
                return indent + "Unknown " + ((int)((DerUnknownTag)obj).Tag).ToString("X") + " "
                    + Encoding.ASCII.GetString(hex,0,hex.Length) + 
// MASC 20070308. CF compatibility patch
#if !NETCF
						Environment.NewLine
#else
						EnvironmentEx.NewLine
#endif					
				;            
			}
            else
            {
                return indent + obj.ToString() + 
// MASC 20070308. CF compatibility patch
#if !NETCF
					Environment.NewLine
#else
					EnvironmentEx.NewLine
#endif					
				;            
			}
        }

        /**
         * dump out a Der object as a formatted string
         *
         * @param obj the Asn1Object to be dumped out.
         */
        public static string DumpAsString(
            object   obj)
        {
            if (obj is Asn1Object)
            {
                return AsString("", (Asn1Object)obj);
            }
            else if (obj is Asn1Encodable)
            {
                return AsString("", ((Asn1Encodable)obj).ToAsn1Object());
            }

            return "unknown object type " + obj.ToString();
        }
    }
}
