using System;

namespace iTextSharp.text.pdf {

    /**
     * A <CODE>PdfICCBased</CODE> defines a ColorSpace
     *
     * @see        PdfStream
     */

    public class PdfICCBased : PdfStream {
    
        protected int NumberOfComponents;
    
        internal PdfICCBased(ICC_Profile profile) {
            NumberOfComponents = profile.NumComponents;
            switch (NumberOfComponents) {
                case 1:
                    Put(PdfName.ALTERNATE, PdfName.DEVICEGRAY);
                    break;
                case 3:
                    Put(PdfName.ALTERNATE, PdfName.DEVICERGB);
                    break;
                case 4:
                    Put(PdfName.ALTERNATE, PdfName.DEVICECMYK);
                    break;
                default:
                    throw new PdfException(NumberOfComponents + " Component(s) is not supported in iText");
            }
            Put(PdfName.N, new PdfNumber(NumberOfComponents));
            bytes = profile.Data;
            FlateCompress();
        }
    }
}