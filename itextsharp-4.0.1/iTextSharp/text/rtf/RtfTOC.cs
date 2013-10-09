using System;
using System.IO;
using iTextSharp.text;
/**
 * $Id: RtfTOC.cs,v 1.1 2005/07/04 22:51:35 psoares33 Exp $
 *
 * Copyright 2002 by 
 * <a href="http://www.smb-tec.com">SMB</a> 
 * <a href="mailto:Steffen.Stundzig@smb-tec.com">Steffen.Stundzig@smb-tec.com</a>
 *
 * The contents of this file are subject to the Mozilla Public License Version 1.1
 * (the "License"); you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the License.
 *
 * The Original Code is 'iText, a free JAVA-PDF library'.
 *
 * The Initial Developer of the Original Code is Bruno Lowagie. Portions created by
 * the Initial Developer are Copyright (C) 1999, 2000, 2001, 2002 by Bruno Lowagie.
 * All Rights Reserved.
 * Co-Developer of the code is Paulo Soares. Portions created by the Co-Developer
 * are Copyright (C) 2000, 2001, 2002 by Paulo Soares. All Rights Reserved.
 *
 * Contributor(s): all the names of the contributors are added in the source code
 * where applicable.
 *
 * Alternatively, the contents of this file may be used under the terms of the
 * LGPL license (the “GNU LIBRARY GENERAL PUBLIC LICENSE”), in which case the
 * provisions of LGPL are applicable instead of those above.  If you wish to
 * allow use of your version of this file only under the terms of the LGPL
 * License and not to allow others to use your version of this file under
 * the MPL, indicate your decision by deleting the provisions above and
 * replace them with the notice and other provisions required by the LGPL.
 * If you do not delete the provisions above, a recipient may use your version
 * of this file under either the MPL or the GNU LIBRARY GENERAL PUBLIC LICENSE.
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the MPL as stated above or under the terms of the GNU
 * Library General Public License as published by the Free Software Foundation;
 * either version 2 of the License, or any later version.
 *
 * This library is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU Library general Public License for more
 * details.
 *
 * If you didn't download this code from the following link, you should check if
 * you aren't using an obsolete version:
 * http://www.lowagie.com/iText/
 */

namespace iTextSharp.text.rtf {

    /**
    * This class can be used to insert a table of contents into 
    * the RTF document.
    * Therefore the field TOC is used. It works great in Word 2000. 
    * StarOffice doesn't support such fields. Other word version
    * are not tested yet.
    * 
    * ONLY FOR USE WITH THE RtfWriter NOT with the RtfWriter2.
    *
    * This class is based on the RtfWriter-package from Mark Hall.
    * @author <a href="mailto:Steffen.Stundzig@smb-tec.com">Steffen.Stundzig@smb-tec.com</a> 
    * @version $Revision: 1.1 $Date: 2005/07/04 22:51:35 $
    */
    public class RtfTOC : Chunk, IRtfField {


        private String      defaultText = "Klicken Sie mit der rechten Maustaste auf diesen Text, um das Inhaltsverzeichnis zu aktualisieren!";

        private bool     addTOCAsTOCEntry = false;

        private Font        entryFont = null;
        private String      entryName = null;


        /**
        * @param tocName the headline of the table of contents
        * @param tocFont the font for the headline
        */
        public RtfTOC( String tocName, Font tocFont ) : base(tocName, tocFont) {
        }

        /**
        * @see com.lowagie.text.rtf.RtfField#write(com.lowagie.text.rtf.RtfWriter, java.io.Stream)
        */
        public void Write( RtfWriter writer, Stream outp ) {

            writer.WriteInitialFontSignature( outp, this );
            byte[] t = DocWriter.GetISOBytes(RtfWriter.FilterSpecialChar( Content, true ));
            outp.Write(t, 0, t.Length);
            writer.WriteFinishingFontSignature( outp, this );
            
            if (addTOCAsTOCEntry) {
                RtfTOCEntry entry = new RtfTOCEntry( entryName, entryFont );
                entry.HideText();
                writer.Add( entry );
            }

            // line break after headline
            outp.WriteByte(RtfWriter.escape);
            outp.Write( RtfWriter.paragraph , 0,  RtfWriter.paragraph .Length);
            outp.WriteByte(RtfWriter.delimiter);

            // toc field entry
            outp.WriteByte(RtfWriter.openGroup);
            outp.WriteByte(RtfWriter.escape);
            outp.Write( RtfWriter.field , 0,  RtfWriter.field .Length);
            // field initialization stuff
            outp.WriteByte(RtfWriter.openGroup);        
            outp.WriteByte(RtfWriter.escape);
            outp.Write( RtfWriter.fieldContent , 0,  RtfWriter.fieldContent .Length);
            outp.WriteByte(RtfWriter.delimiter);
            t = DocWriter.GetISOBytes("TOC");
            outp.Write(t, 0, t.Length);
            // create the TOC based on the 'toc entries'
            outp.WriteByte(RtfWriter.delimiter);
            outp.WriteByte(RtfWriter.escape);        
            outp.WriteByte(RtfWriter.escape);        
            outp.WriteByte( (byte)'f' );
            outp.WriteByte(RtfWriter.delimiter);
            // create Hyperlink TOC Entrie 
            outp.WriteByte(RtfWriter.escape);        
            outp.WriteByte(RtfWriter.escape);        
            outp.WriteByte( (byte)'h');
            outp.WriteByte(RtfWriter.delimiter);
            // create the TOC based on the paragraph level
            outp.WriteByte(RtfWriter.delimiter);
            outp.WriteByte(RtfWriter.escape);        
            outp.WriteByte(RtfWriter.escape);        
            outp.WriteByte( (byte)'u' );
            outp.WriteByte(RtfWriter.delimiter);
            // create the TOC based on the paragraph headlines 1-5
            outp.WriteByte(RtfWriter.delimiter);
            outp.WriteByte(RtfWriter.escape);        
            outp.WriteByte(RtfWriter.escape);        
            outp.WriteByte( (byte)'o' );
            outp.WriteByte(RtfWriter.delimiter);
            t = DocWriter.GetISOBytes("\"1-5\"");
            outp.Write(t, 0, t.Length);
            outp.WriteByte(RtfWriter.delimiter);
            outp.WriteByte(RtfWriter.closeGroup);

            // field default result stuff
            outp.WriteByte(RtfWriter.openGroup);        
            outp.WriteByte(RtfWriter.escape);
            outp.Write( RtfWriter.fieldDisplay , 0,  RtfWriter.fieldDisplay .Length);
            outp.WriteByte(RtfWriter.delimiter);
            t = DocWriter.GetISOBytes(defaultText);
            outp.Write(t, 0, t.Length);
            outp.WriteByte(RtfWriter.delimiter);
            outp.WriteByte(RtfWriter.closeGroup);
            outp.WriteByte(RtfWriter.closeGroup);
        }

        
        /**
        * Add a toc entry
        * @param entryName the name of the entry
        * @param entryFont the font to be used for the entry
        */
        public void AddTOCAsTOCEntry( String entryName, Font entryFont ) {
            this.addTOCAsTOCEntry = true;
            this.entryFont = entryFont;
            this.entryName = entryName;        
        }

        
        /**
        * Sets the default text of the Table of Contents
        * @param text the default text
        */
        public void SetDefaultText( String text ) {
            this.defaultText = text;
        }
    }
}