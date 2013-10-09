using System;
using System.IO;
using System.Collections;
using iTextSharp.text;
using iTextSharp.text.rtf;
using iTextSharp.text.rtf.document;
using iTextSharp.text.rtf.graphic;
using ST = iTextSharp.text.rtf.style;
/*
 * $Id: RtfParagraph.cs,v 1.3 2005/12/26 09:57:28 psoares33 Exp $
 * $Name:  $
 *
 * Copyright 2001, 2002, 2003, 2004 by Mark Hall
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
 * LGPL license (the ?GNU LIBRARY GENERAL PUBLIC LICENSE?), in which case the
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

namespace iTextSharp.text.rtf.text {

    /**
    * The RtfParagraph is an extension of the RtfPhrase that adds alignment and
    * indentation properties. It wraps a Paragraph.
    * 
    * @version $Version:$
    * @author Mark Hall (mhall@edu.uni-klu.ac.at)
    */
    public class RtfParagraph : RtfPhrase {

        /**
        * Constant for the end of a paragraph
        */
        public static byte[] PARAGRAPH = DocWriter.GetISOBytes("\\par");
        /**
        * An optional RtfParagraphStyle to use for styling.
        */
        private ST.RtfParagraphStyle paragraphStyle = null;
        
        /**
        * Constructs a RtfParagraph belonging to a RtfDocument based on a Paragraph.
        * 
        * @param doc The RtfDocument this RtfParagraph belongs to
        * @param paragraph The Paragraph that this RtfParagraph is based on
        */
        public RtfParagraph(RtfDocument doc, Paragraph paragraph) : base(doc) {
            ST.RtfFont baseFont = null;
            if (paragraph.Font is ST.RtfParagraphStyle) {
                this.paragraphStyle = this.document.GetDocumentHeader().GetRtfParagraphStyle(((ST.RtfParagraphStyle) paragraph.Font).GetStyleName());
                baseFont = this.paragraphStyle;
            } else {
                baseFont = new ST.RtfFont(this.document, paragraph.Font);
                this.paragraphStyle = new ST.RtfParagraphStyle(this.document, this.document.GetDocumentHeader().GetRtfParagraphStyle("Normal"));
                this.paragraphStyle.SetAlignment(paragraph.Alignment);
                this.paragraphStyle.SetIndentLeft((int) (paragraph.IndentationLeft * RtfElement.TWIPS_FACTOR));
                this.paragraphStyle.SetIndentRight((int) (paragraph.IndentationRight * RtfElement.TWIPS_FACTOR));
                this.paragraphStyle.SetSpacingBefore((int) (paragraph.SpacingBefore * RtfElement.TWIPS_FACTOR));
                this.paragraphStyle.SetSpacingAfter((int) (paragraph.SpacingAfter * RtfElement.TWIPS_FACTOR));
                if (paragraph.LeadingDefined) {
                    this.paragraphStyle.SetLineLeading((int) (paragraph.Leading * RtfElement.TWIPS_FACTOR));
                }
                this.paragraphStyle.SetKeepTogether(paragraph.KeepTogether);
            }
            
            for (int i = 0; i < paragraph.Count; i++) {
                IElement chunk = (IElement) paragraph[i];
                if (chunk is Chunk) {
                    ((Chunk) chunk).Font = baseFont.Difference(((Chunk) chunk).Font);
                } else if (chunk is RtfImage) {
                    ((RtfImage) chunks[i]).SetAlignment(this.paragraphStyle.GetAlignment());
                }
                try {
                    chunks.Add(doc.GetMapper().MapElement(chunk));
                } catch (DocumentException) {
                }
            }
        }
        
        /**
        * Set whether this RtfParagraph must stay on the same page as the next one.
        *  
        * @param keepTogetherWithNext Whether this RtfParagraph must keep together with the next.
        */
        public void SetKeepTogetherWithNext(bool keepTogetherWithNext) {
            this.paragraphStyle.SetKeepTogetherWithNext(keepTogetherWithNext);
        }
        
        /**
        * Writes the content of this RtfParagraph. First paragraph specific data is written
        * and then the RtfChunks of this RtfParagraph are added.
        * 
        * @return The content of this RtfParagraph
        */
        public override byte[] Write() {
            MemoryStream result = new MemoryStream();
            byte[] t;
            try {
                result.Write(PARAGRAPH_DEFAULTS, 0, PARAGRAPH_DEFAULTS.Length);
                if (inTable) {
                    result.Write(IN_TABLE, 0, IN_TABLE.Length);
                }
                if(this.paragraphStyle != null) {
                    result.Write(t = this.paragraphStyle.WriteBegin(), 0, t.Length);
                }
                for (int i = 0; i < chunks.Count; i++) {
                    result.Write(t = ((IRtfBasicElement) chunks[i]).Write(), 0, t.Length);
                }
                if(this.paragraphStyle != null) {
                    result.Write(t = this.paragraphStyle.WriteEnd(), 0, t.Length);
                }
                if (!inTable) {
                    result.Write(PARAGRAPH, 0, PARAGRAPH.Length);
                }
                if(this.document.GetDocumentSettings().IsOutputDebugLineBreaks()) {
                    result.WriteByte((byte)'\n');
                }
            } catch (IOException) {
            }
            return result.ToArray();
        }

        /**
        * Gets the left indentation of this RtfParagraph.
        * 
        * @return The left indentation.
        */
        public int GetIndentLeft() {
            return this.paragraphStyle.GetIndentLeft();
        }
        
        /**
        * Sets the left indentation of this RtfParagraph.
        * 
        * @param indentLeft The left indentation to use.
        */
        public void SetIndentLeft(int indentLeft) {
            this.paragraphStyle.SetIndentLeft(indentLeft);
        }
        
        /**
        * Gets the right indentation of this RtfParagraph.
        * 
        * @return The right indentation.
        */
        public int GetIndentRight()  {
            return this.paragraphStyle.GetIndentRight();
        }
        
        /**
        * Sets the right indentation of this RtfParagraph.
        * 
        * @param indentRight The right indentation to use.
        */
        public void SetIndentRight(int indentRight) {
            this.paragraphStyle.SetIndentRight(indentRight);
        }
    }
}