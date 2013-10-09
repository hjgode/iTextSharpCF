using System;
using System.Collections;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.rtf.document;
/**
 * $Id: RtfParser.cs,v 1.2 2006/08/28 20:22:35 psoares33 Exp $
 * $Name:  $
 *
 * Copyright 2006 by Mark Hall
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
 * the Initial Developer are Copyright (C) 1999-2006 by Bruno Lowagie.
 * All Rights Reserved.
 * Co-Developer of the code is Paulo Soares. Portions created by the Co-Developer
 * are Copyright (C) 2000-2006 by Paulo Soares. All Rights Reserved.
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

namespace iTextSharp.text.rtf.direct {

    /**
    * The RtfParser allows the importing of RTF documents or
    * RTF document fragments. The RTF document or fragment is tokenised,
    * font and color definitions corrected and then added to
    * the document being written.
    * 
    * @version $Revision: 1.2 $
    * @author Mark Hall (mhall@edu.uni-klu.ac.at)
    */
    public class RtfParser {
        /**
        * Currently the RTF document header is being parsed.
        */
        private const int PARSER_IN_HEADER = 0;
        /**
        * Currently the RTF font table is being parsed.
        */
        private const int PARSER_IN_FONT_TABLE = 1;
        /**
        * Currently the RTF color table is being parsed.
        */
        private const int PARSER_IN_COLOR_TABLE = 2;
        /**
        * Currently the RTF info group is being parsed.
        */
        private const int PARSER_IN_INFO_GROUP = 4;
        /**
        * Currently the RTF document content is being parsed.
        */
        private const int PARSER_IN_DOCUMENT = 8;
        
        /**
        * The RtfDocument to add the RTF document or fragment to.
        */
        private RtfDocument rtfDoc = null;
        /**
        * The RtfTokeniser to use for tokenising the RTF document or fragment.
        */
        private RtfTokeniser tokeniser = null;
        /**
        * The RtfImportHeader to store imported font and color mappings in.
        */
        private RtfImportHeader importHeader = null;
        /**
        * The RtfFontTableParser to use for parsing the font table.
        */
        private RtfFontTableParser fontTableParser = null;
        /**
        * The RtfColorTableParser to use for parsing the color table.
        */
        private RtfColorTableParser colorTableParser = null;
        /**
        * The current parser state.
        */
        private int state = PARSER_IN_HEADER;
        
        /**
        * Imports a complete RTF document.
        * 
        * @param reader The Reader to read the RTF document from.
        * @param rtfDoc The RtfDocument to add the imported document to.
        * @throws IOException On I/O errors.
        * @throws DocumentException On document writing errors.
        */
        public void ImportRtfDocument(TextReader reader, RtfDocument rtfDoc) {
            this.rtfDoc = rtfDoc;
            this.state = PARSER_IN_HEADER;
            this.importHeader = new RtfImportHeader(this.rtfDoc);
            this.fontTableParser = new RtfFontTableParser(this.importHeader);
            this.colorTableParser = new RtfColorTableParser(this.importHeader);
            this.tokeniser = new RtfTokeniser(this, 0);
            this.tokeniser.Tokenise(reader);
        }
        
        /**
        * Imports an RTF fragment.
        * 
        * @param reader The Reader to read the RTF fragment from.
        * @param rtfDoc The RTF document to add the RTF fragment to.
        * @param importMappings The RtfImportMappings defining font and color mappings for the fragment.
        * @throws IOException On I/O errors.
        * @throws DocumentException On document writing errors.
        */
        public void ImportRtfFragment(TextReader reader, RtfDocument rtfDoc, RtfImportMappings importMappings) {
            this.rtfDoc = rtfDoc;
            this.state = PARSER_IN_DOCUMENT;
            this.importHeader = new RtfImportHeader(this.rtfDoc);
            this.fontTableParser = new RtfFontTableParser(this.importHeader);
            this.colorTableParser = new RtfColorTableParser(this.importHeader);
            HandleImportMappings(importMappings);
            this.tokeniser = new RtfTokeniser(this, 1);
            this.tokeniser.Tokenise(reader);
        }

        /**
        * Imports the mappings defined in the RtfImportMappings into the
        * RtfImportHeader of this RtfParser.
        * 
        * @param importMappings The RtfImportMappings to import.
        */
        private void HandleImportMappings(RtfImportMappings importMappings) {
            foreach (String fontNr in importMappings.GetFontMappings().Keys) {
                this.importHeader.ImportFont(fontNr, (String) importMappings.GetFontMappings()[fontNr]);
            }
            foreach (String colorNr in importMappings.GetColorMappings().Keys) {
                this.importHeader.ImportColor(colorNr, (Color) importMappings.GetColorMappings()[colorNr]);
            }
        }
        
        /**
        * Handles open group tokens.
        * 
        * @param groupLevel The current group nesting level.
        * @throws DocumentException On document writing errors.
        */
        public void HandleOpenGroup(int groupLevel) {
            if (this.state == PARSER_IN_DOCUMENT) {
                this.rtfDoc.Add(new RtfDirectContent("{"));
            }
        }
        
        /**
        * Handles close group tokens. Depending on what is currently
        * being parsed the parse state may change.
        * 
        * @param groupLevel The current group nesting level.
        * @throws DocumentException On document writing errors.
        */
        public void HandleCloseGroup(int groupLevel) {
            if (this.state == PARSER_IN_DOCUMENT && groupLevel > 1) {
                this.rtfDoc.Add(new RtfDirectContent("}"));
            } else if (this.state == PARSER_IN_INFO_GROUP && groupLevel == 2) {
                this.state = PARSER_IN_DOCUMENT;
            } else if (this.state == PARSER_IN_FONT_TABLE) {
                this.fontTableParser.HandleCloseGroup(groupLevel);
                if (groupLevel == 2) {
                    this.state = PARSER_IN_HEADER;
                }
            } else if (this.state == PARSER_IN_COLOR_TABLE) {
                this.state = PARSER_IN_HEADER;
            }
        }
        
        /**
        * Handles single control character tokens.
        * 
        * @param ctrlCharacter The control character to handle.
        * @param groupLevel The current group nesting level.
        * @throws DocumentException On document writing errors.
        */
        public void HandleCtrlCharacter(String ctrlCharacter, int groupLevel) {
            if (this.state == PARSER_IN_DOCUMENT) {
                this.rtfDoc.Add(new RtfDirectContent(ctrlCharacter));
            }
        }
        
        /**
        * Handles control word tokens. Depending on the current
        * state a control word can lead to a state change. When
        * parsing the actual document contents, The font number,
        * color number and background color number are remapped.
        * 
        * @param ctrlWord The control word to handle.
        * @param groupLevel The current group nesting level.
        * @throws DocumentException On document writing errors.
        */
        public void HandleCtrlWord(String ctrlWord, int groupLevel) {
            if (this.state == PARSER_IN_DOCUMENT) {
                if (RtfColorTableParser.StringMatches(ctrlWord, "\\f")) {
                    ctrlWord = "\\f" + this.importHeader.MapFontNr(ctrlWord.Substring(2));
                } else if (RtfColorTableParser.StringMatches(ctrlWord, "\\cf")) {
                    ctrlWord = "\\cf" + this.importHeader.MapColorNr(ctrlWord.Substring(3));
                } else if (RtfColorTableParser.StringMatches(ctrlWord, "\\cb")) {
                    ctrlWord = "\\cb" + this.importHeader.MapColorNr(ctrlWord.Substring(3));
                }
                this.rtfDoc.Add(new RtfDirectContent(ctrlWord));
            } else if (this.state == PARSER_IN_FONT_TABLE) {
                this.fontTableParser.HandleCtrlWord(ctrlWord, groupLevel);
            } else if (this.state == PARSER_IN_COLOR_TABLE) {
                this.colorTableParser.HandleCtrlWord(ctrlWord, groupLevel);
            } else if (this.state == PARSER_IN_HEADER) {
                if (ctrlWord.Equals("\\info")) {
                    this.state = PARSER_IN_INFO_GROUP;
                } else if (ctrlWord.Equals("\\fonttbl")) {
                    this.state = PARSER_IN_FONT_TABLE;
                } else if (ctrlWord.Equals("\\colortbl")) {
                    this.state = PARSER_IN_COLOR_TABLE;
                }
            }
        }
        
        /**
        * Handles text tokens. These are either handed on to the
        * RtfColorTableParser or RtfFontTableParser or added directly
        * to the document.
        * 
        * @param text The text token to handle.
        * @param groupLevel The current group nesting level.
        * @throws DocumentException On document writing errors.
        */
        public void HandleText(String text, int groupLevel) {
            if (this.state == PARSER_IN_DOCUMENT) {
                this.rtfDoc.Add(new RtfDirectContent(text));
            } else if (this.state == PARSER_IN_FONT_TABLE) {
                this.fontTableParser.HandleText(text, groupLevel);
            } else if (this.state == PARSER_IN_COLOR_TABLE) {
                this.colorTableParser.HandleText(text, groupLevel);
            }
        }
    }
}