using System;
using System.Text;
using System.Collections;

namespace iTextSharp.text.pdf
{
    /// <summary>
    /// Summary description for ICC_Profile.
    /// </summary>
    public class ICC_Profile
    {
        protected byte[] data;
        protected int numComponents;
        private static Hashtable cstags = new Hashtable();
        
        protected ICC_Profile() {
        }
        
        public static ICC_Profile GetInstance(byte[] data) {
            if (data.Length < 128)
                throw new ArgumentException("Invalid ICC profile");
            ICC_Profile icc = new ICC_Profile();
            icc.data = data;
            object cs = cstags[Encoding.ASCII.GetString(data, 16, 4)];
            icc.numComponents = (cs == null ? 0 : (int)cs);
            return icc;
        }
        
        public byte[] Data {
            get {
                return data;
            }
        }
        
        public int NumComponents {
            get {
                return numComponents;
            }
        }

        static ICC_Profile() {
            cstags["XYZ "] = 3;
            cstags["Lab "] = 3;
            cstags["Luv "] = 3;
            cstags["YCbr"] = 3;
            cstags["Yxy "] = 3;
            cstags["RGB "] = 3;
            cstags["GRAY"] = 1;
            cstags["HSV "] = 3;
            cstags["HLS "] = 3;
            cstags["CMYK"] = 4;
            cstags["CMY "] = 3;
            cstags["2CLR"] = 2;
            cstags["3CLR"] = 3;
            cstags["4CLR"] = 4;
            cstags["5CLR"] = 5;
            cstags["6CLR"] = 6;
            cstags["7CLR"] = 7;
            cstags["8CLR"] = 8;
            cstags["9CLR"] = 9;
            cstags["ACLR"] = 10;
            cstags["BCLR"] = 11;
            cstags["CCLR"] = 12;
            cstags["DCLR"] = 13;
            cstags["ECLR"] = 14;
            cstags["FCLR"] = 15;
        }
    }
}
