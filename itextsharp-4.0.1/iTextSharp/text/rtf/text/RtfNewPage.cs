using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.rtf;
using iTextSharp.text.rtf.document;
/*
 * Created on Aug 12, 2004
 *
 * To change the template for this generated file go to
 * Window - Preferences - Java - Code Generation - Code and Comments
 */
namespace iTextSharp.text.rtf.text {


    /**
    * The RtfNewPage creates a new page. INTERNAL CLASS
    * 
    * @version $Version:$
    * @author Mark Hall (mhall@edu.uni-klu.ac.at)
    */
    public class RtfNewPage : RtfElement {

        /**
        * Constant for a new page
        */
        private static byte[] NEW_PAGE = DocWriter.GetISOBytes("\\page");
        
        /**
        * Constructs a RtfNewPage
        * 
        * @param doc The RtfDocument this RtfNewPage belongs to
        */
        public RtfNewPage(RtfDocument doc) : base(doc) {
        }
        
        /**
        * Writes a new page
        * 
        * @return A byte array with the new page set
        */
        public override byte[] Write() {
            MemoryStream result = new MemoryStream();
            try {
                result.Write(NEW_PAGE, 0, NEW_PAGE.Length);
                result.Write(RtfParagraph.PARAGRAPH_DEFAULTS, 0, RtfParagraph.PARAGRAPH_DEFAULTS.Length);
            } catch (IOException) {
            }
            return result.ToArray();
        }
    }
}