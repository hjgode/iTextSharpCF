using System;
using iTextSharp.text;
using iTextSharp.text.rtf.document;
using iTextSharp.text.rtf.field;
using iTextSharp.text.rtf.graphic;
using iTextSharp.text.rtf.list;
using TB = iTextSharp.text.rtf.table;
using iTextSharp.text.rtf.text;
/*
 * $Id: RtfMapper.cs,v 1.1 2005/07/04 22:51:35 psoares33 Exp $
 * $Name:  $
 *
 * Copyright 2003, 2004 by Mark Hall
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

namespace iTextSharp.text.rtf {

    /**
    * The RtfMapper provides mappings between com.lowagie.text.* classes
    * and the corresponding com.lowagie.text.rtf.** classes.
    * 
    * @version $Version:$
    * @author Mark Hall (mhall@edu.uni-klu.ac.at)
    */
    public class RtfMapper {

        /**
        * The RtfDocument this RtfMapper belongs to
        */
        RtfDocument rtfDoc;
        
        /**
        * Constructs a RtfMapper for a RtfDocument
        * 
        * @param doc The RtfDocument this RtfMapper belongs to
        */
        public RtfMapper(RtfDocument doc) {
            this.rtfDoc = doc;
        }
        
        /**
        * Takes an Element subclass and returns the correct IRtfBasicElement
        * subclass, that wraps the Element subclass.
        * 
        * @param element The Element to wrap
        * @return A IRtfBasicElement wrapping the Element
        * @throws DocumentException
        */
        public IRtfBasicElement MapElement(IElement element) {
            IRtfBasicElement rtfElement = null;

            if (element is IRtfBasicElement) {
                rtfElement = (IRtfBasicElement) element;
                rtfElement.SetRtfDocument(this.rtfDoc);
                return rtfElement;
            }
            switch (element.Type) {
                case Element.CHUNK:
                    if (((Chunk) element).GetImage() != null) {
                        rtfElement = new RtfImage(rtfDoc, ((Chunk) element).GetImage());
                    } else if (((Chunk) element).HasAttributes() && ((Chunk) element).Attributes.ContainsKey(Chunk.NEWPAGE)) {
                        rtfElement = new RtfNewPage(rtfDoc);
                    } else {
                        rtfElement = new RtfChunk(rtfDoc, (Chunk) element);
                    }
                    break;
                case Element.PHRASE:
                    rtfElement = new RtfPhrase(rtfDoc, (Phrase) element);
                    break;
                case Element.PARAGRAPH:
                    rtfElement = new RtfParagraph(rtfDoc, (Paragraph) element);
                    break;
                case Element.ANCHOR:
                    rtfElement = new RtfAnchor(rtfDoc, (Anchor) element);
                    break;
                case Element.ANNOTATION:
                    rtfElement = new RtfAnnotation(rtfDoc, (Annotation) element);
                    break;
                case Element.IMGRAW:
                case Element.IMGTEMPLATE:
                case Element.JPEG:
                    rtfElement = new RtfImage(rtfDoc, (Image) element);
                    break;
                case Element.AUTHOR: 
                case Element.SUBJECT:
                case Element.KEYWORDS:
                case Element.TITLE:
                case Element.PRODUCER:
                case Element.CREATIONDATE:
                    rtfElement = new RtfInfoElement(rtfDoc, (Meta) element);
                    break;
                case Element.LIST:
                    rtfElement = new RtfList(rtfDoc, (List) element);
                    break;
                case Element.LISTITEM:
                    rtfElement = new RtfListItem(rtfDoc, (ListItem) element);
                    break;
                case Element.SECTION:
                    rtfElement = new RtfSection(rtfDoc, (Section) element);
                    break;
                case Element.CHAPTER:
                    rtfElement = new RtfChapter(rtfDoc, (Chapter) element);
                    break;
                case Element.TABLE:
                    try {
                        rtfElement = new TB.RtfTable(rtfDoc, (Table) element);
                    }
                    catch (InvalidCastException) {
                        rtfElement = new TB.RtfTable(rtfDoc, ((SimpleTable) element).CreateTable());
                    }
                    break;
            }
            
            return rtfElement;
        }
    }
}