using System;
using System.IO;
using iTextSharp.text;
/**
 * $Id: RtfTOCEntry.cs,v 1.1 2005/07/04 22:51:35 psoares33 Exp $
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
 * This class can be used to insert entries for a table of contents into 
 * the RTF document.
 * 
 * ONLY FOR USE WITH THE RtfWriter NOT with the RtfWriter2.
 *
 * This class is based on the RtfWriter-package from Mark Hall.
 * @author <a href="mailto:Steffen.Stundzig@smb-tec.com">Steffen.Stundzig@smb-tec.com</a> 
 * @version $Revision: 1.1 $Date: 2005/07/04 22:51:35 $
 */
public class RtfTOCEntry : Chunk, IRtfField {


    private bool         hideText = false;

    private bool         hidePageNumber = false;    

    private String    entryName;

    private Font      entryFont;    

    private Font      contentFont;    


    /**
     * Constructs an entry for the Table of Contents
     * @param content the content of the entry
     * @param contentFont the font
     */
    public RtfTOCEntry( String content, Font contentFont ) : this( content, contentFont, content, contentFont ){
//        Super( content, font );
//        this.entryName = content;
//        printEntryNameAsText = true;
    }


    /**
     * Constructs an entry for the Table of Contents
     * @param content the content of the entry
     * @param contentFont the font
     * @param entryName name of the entry
     * @param entryFont font of the entryname
     */
    public RtfTOCEntry( String content, Font contentFont, String entryName, Font entryFont ) : base( content, contentFont ) {
        // hide the text of the entry, because it is printed  
        this.entryName = entryName;
        this.entryFont = entryFont;
        this.contentFont = contentFont;
    }

    /**
     * @see com.lowagie.text.rtf.RtfField#write(com.lowagie.text.rtf.RtfWriter, java.io.Stream)
     */
    public void Write( RtfWriter writer, Stream outp ) {

        if (!hideText) {
            writer.WriteInitialFontSignature( outp, new Chunk("", contentFont) );
            byte[] t = DocWriter.GetISOBytes(RtfWriter.FilterSpecialChar( Content, true ));
            outp.Write(t, 0, t.Length);
            writer.WriteFinishingFontSignature( outp, new Chunk("", contentFont) );
        }

        if (!entryFont.Equals( contentFont )) {
            writer.WriteInitialFontSignature(outp, new Chunk("", entryFont) );
            WriteField( outp );
            writer.WriteFinishingFontSignature(outp, new Chunk("", entryFont) );
        } else {
            writer.WriteInitialFontSignature(outp, new Chunk("", contentFont) );
            WriteField( outp );
            writer.WriteFinishingFontSignature(outp, new Chunk("", contentFont) );
        }
    }

    
    private void WriteField( Stream outp ) {
        
        // always hide the toc entry
        outp.WriteByte(RtfWriter.openGroup);
        outp.WriteByte(RtfWriter.escape);
        outp.WriteByte( (byte)'v' );

        // tc field entry
        outp.WriteByte(RtfWriter.openGroup);
        outp.WriteByte(RtfWriter.escape);
        byte[] t;
        if (!hidePageNumber) {
            t = DocWriter.GetISOBytes("tc");
            outp.Write(t, 0, t.Length);
        } else {
            t = DocWriter.GetISOBytes("tcn");
            outp.Write(t, 0, t.Length);
        }    
        outp.WriteByte(RtfWriter.delimiter);
        t = DocWriter.GetISOBytes(RtfWriter.FilterSpecialChar( entryName, true ));
        outp.Write(t, 0, t.Length);
        outp.WriteByte(RtfWriter.delimiter);
        outp.WriteByte(RtfWriter.closeGroup);        

        outp.WriteByte(RtfWriter.closeGroup);        
    }

    /**
     * sets the hideText value to true 
     */
    public void HideText() {
        hideText = true;
    }

    /**
     * sets the hidePageNumber value to true 
     */
    public void HidePageNumber() {
        hidePageNumber = true;
    }
}
}