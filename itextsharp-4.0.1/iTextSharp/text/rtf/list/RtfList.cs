using System;
using System.IO;
using System.Collections;
using iTextSharp.text;
using iTextSharp.text.rtf;
using iTextSharp.text.rtf.document;
using ST = iTextSharp.text.rtf.style;
using iTextSharp.text.rtf.text;
/*
 * $Id: RtfList.cs,v 1.6 2006/04/05 14:32:54 psoares33 Exp $
 * $Name:  $
 *
 * Copyright 2001, 2002, 2003, 2004, 2005 by Mark Hall
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

namespace iTextSharp.text.rtf.list {

    /**
    * The RtfList stores one List. It also provides the methods to write the
    * list declaration and the list data.
    *  
    * @version $Version:$
    * @author Mark Hall (mhall@edu.uni-klu.ac.at)
    */
    public class RtfList : RtfElement, IRtfExtendedElement {

        /**
        * Constant for list level
        */
        private static byte[] LIST_LEVEL = DocWriter.GetISOBytes("\\listlevel");
        /**
        * Constant for list level style old
        */
        private static byte[] LIST_LEVEL_TYPE = DocWriter.GetISOBytes("\\levelnfc");
        /**
        * Constant for list level style new
        */
        private static byte[] LIST_LEVEL_TYPE_NEW = DocWriter.GetISOBytes("\\levelnfcn");
        /**
        * Constant for list level alignment old
        */
        private static byte[] LIST_LEVEL_ALIGNMENT = DocWriter.GetISOBytes("\\leveljc");
        /**
        * Constant for list level alignment new
        */
        private static byte[] LIST_LEVEL_ALIGNMENT_NEW = DocWriter.GetISOBytes("\\leveljcn");
        /**
        * Constant for list level start at
        */
        private static byte[] LIST_LEVEL_START_AT = DocWriter.GetISOBytes("\\levelstartat");
        /**
        * Constant for list level text
        */
        private static byte[] LIST_LEVEL_TEXT = DocWriter.GetISOBytes("\\leveltext");
        /**
        * Constant for the beginning of the list level numbered style
        */
        private static byte[] LIST_LEVEL_STYLE_NUMBERED_BEGIN = DocWriter.GetISOBytes("\\\'02\\\'");
        /**
        * Constant for the end of the list level numbered style
        */
        private static byte[] LIST_LEVEL_STYLE_NUMBERED_END = DocWriter.GetISOBytes(".;");
        /**
        * Constant for the list level bulleted style
        */
        private static byte[] LIST_LEVEL_STYLE_BULLETED = DocWriter.GetISOBytes("\\\'01\\u-3913 ?;");
        /**
        * Constant for the beginning of the list level numbers
        */
        private static byte[] LIST_LEVEL_NUMBERS_BEGIN = DocWriter.GetISOBytes("\\levelnumbers");
        /**
        * Constant for the list level numbers
        */
        private static byte[] LIST_LEVEL_NUMBERS_NUMBERED = DocWriter.GetISOBytes("\\\'01");
        /**
        * Constant for the end of the list level numbers
        */
        private static byte[] LIST_LEVEL_NUMBERS_END = DocWriter.GetISOBytes(";");
        /**
        * Constant for the first indentation
        */
        private static byte[] LIST_LEVEL_FIRST_INDENT = DocWriter.GetISOBytes("\\fi");
        /**
        * Constant for the symbol indentation
        */
        private static byte[] LIST_LEVEL_SYMBOL_INDENT = DocWriter.GetISOBytes("\\tx");
        /**
        * Constant for the list level value
        */
        private static byte[] LIST_LEVEL_NUMBER = DocWriter.GetISOBytes("\\ilvl");
        /**
        * Constant for a tab character
        */
        private static byte[] TAB = DocWriter.GetISOBytes("\\tab");
        /**
        * Constant for the old list text
        */
        private static byte[] LIST_TEXT = DocWriter.GetISOBytes("\\listtext");
        /**
        * Constant for the old list number end
        */
        private static byte[] LIST_NUMBER_END = DocWriter.GetISOBytes(".");
        /**
        * Constant for the old bulleted list 
        */
        private static byte[] LIST_BULLET = DocWriter.GetISOBytes("\\\'b7");
        
        /**
        * The subitems of this RtfList
        */
        private ArrayList items;
        /**
        * The level of this RtfList
        */
        private int listLevel = 0;
        /**
        * The first indentation of this RtfList
        */
        private int firstIndent = 0;
        /**
        * The left indentation of this RtfList
        */
        private int leftIndent = 0;
        /**
        * The right indentation of this RtfList
        */
        private int rightIndent = 0;
        /**
        * The symbol indentation of this RtfList
        */
        private int symbolIndent = 0;
        /**
        * The list number of this RtfList
        */
        private int listNumber = 1;
        /**
        * Whether this RtfList is numbered
        */
        private bool numbered = true;
        /**
        * The RtfFont for numbered lists
        */
        private ST.RtfFont fontNumber;
        /**
        * The RtfFont for bulleted lists
        */
        private ST.RtfFont fontBullet;
        /**
        * The alignment of this RtfList
        */
        private int alignment = Element.ALIGN_LEFT;

        /**
        * The parent List in multi-level lists.
        */
        private RtfList parentList = null;
        
        /**
        * Constructs a new RtfList for the specified List.
        * 
        * @param doc The RtfDocument this RtfList belongs to
        * @param list The List this RtfList is based on
        */
        public RtfList(RtfDocument doc, List list) : base(doc) {
            
            this.listNumber = document.GetDocumentHeader().GetListNumber(this);
            
            this.items = new ArrayList();
            if (list.SymbolIndent > 0 && list.IndentationLeft > 0) {
                this.firstIndent = (int) (list.SymbolIndent * RtfElement.TWIPS_FACTOR * -1);
                this.leftIndent = (int) ((list.IndentationLeft + list.SymbolIndent) * RtfElement.TWIPS_FACTOR);
            } else if (list.SymbolIndent > 0) {
                this.firstIndent = (int) (list.SymbolIndent * RtfElement.TWIPS_FACTOR * -1);
                this.leftIndent = (int) (list.SymbolIndent * RtfElement.TWIPS_FACTOR);
            } else if (list.IndentationLeft > 0) {
                this.firstIndent = 0;
                this.leftIndent = (int) (list.IndentationLeft * RtfElement.TWIPS_FACTOR);
            } else {
                this.firstIndent = 0;
                this.leftIndent = 0;
            }
            this.rightIndent = (int) (list.IndentationRight * RtfElement.TWIPS_FACTOR);
            this.symbolIndent = (int) ((list.SymbolIndent + list.IndentationLeft) * RtfElement.TWIPS_FACTOR);
            this.numbered = list.IsNumbered();
            
            for (int i = 0; i < list.Items.Count; i++) {
                try {
                    IElement element = (IElement) list.Items[i];
                    if (element.Type == Element.CHUNK) {
                        element = new ListItem((Chunk) element);
                    }
                    if (element is ListItem) {
                        this.alignment = ((ListItem) element).Alignment;
                    }
                    IRtfBasicElement rtfElement = doc.GetMapper().MapElement(element);
                    if (rtfElement is RtfList) {
                        ((RtfList) rtfElement).SetListNumber(listNumber);
                        ((RtfList) rtfElement).SetListLevel(listLevel + 1);
                        ((RtfList) rtfElement).SetParent(this);
                    } else if (rtfElement is RtfListItem) {
                        ((RtfListItem) rtfElement).SetParent(this);
                        ((RtfListItem) rtfElement).InheritListSettings(listNumber, listLevel + 1);
                    }
                    items.Add(rtfElement);
                } catch (DocumentException ) {
                }
            }
            if (this.listLevel == 0) {
                CorrectIndentation();
            }
            
            fontNumber = new ST.RtfFont(document, new Font(Font.TIMES_ROMAN, 10, Font.NORMAL, new Color(0, 0, 0)));
            fontBullet = new ST.RtfFont(document, new Font(Font.SYMBOL, 10, Font.NORMAL, new Color(0, 0, 0)));
        }
        
        private byte[] WriteIndentations() {
            MemoryStream result = new MemoryStream();
            try {
                byte[] t;
                result.Write(LIST_LEVEL_FIRST_INDENT, 0, LIST_LEVEL_FIRST_INDENT.Length);
                result.Write(t = IntToByteArray(firstIndent), 0, t.Length);
                result.Write(ST.RtfParagraphStyle.INDENT_LEFT, 0, ST.RtfParagraphStyle.INDENT_LEFT.Length);
                result.Write(t = IntToByteArray(leftIndent), 0, t.Length);
                result.Write(ST.RtfParagraphStyle.INDENT_RIGHT, 0, ST.RtfParagraphStyle.INDENT_RIGHT.Length);
                result.Write(t = IntToByteArray(rightIndent), 0, t.Length);
            } catch (IOException) {
            }
            return result.ToArray();
        }
        
        /**
        * Writes the definition part of this list level
        * 
        * @return A byte array containing the definition of this list level
        */
        public byte[] WriteDefinition() {
            MemoryStream result = new MemoryStream();
            try {
                byte[] t;
                result.Write(OPEN_GROUP, 0, OPEN_GROUP.Length);
                result.Write(LIST_LEVEL, 0, LIST_LEVEL.Length);
                result.Write(LIST_LEVEL_TYPE, 0, LIST_LEVEL_TYPE.Length);
                if (numbered) {
                    result.Write(t = IntToByteArray(0), 0, t.Length);
                } else {
                    result.Write(t = IntToByteArray(23), 0, t.Length);
                }
                result.Write(LIST_LEVEL_TYPE_NEW, 0, LIST_LEVEL_TYPE_NEW.Length);
                if (numbered) {
                    result.Write(t = IntToByteArray(0), 0, t.Length);
                } else {
                    result.Write(t = IntToByteArray(23), 0, t.Length);
                }
                result.Write(LIST_LEVEL_ALIGNMENT, 0, LIST_LEVEL_ALIGNMENT.Length);
                result.Write(t = IntToByteArray(0), 0, t.Length);
                result.Write(LIST_LEVEL_ALIGNMENT_NEW, 0, LIST_LEVEL_ALIGNMENT_NEW.Length);
                result.Write(t = IntToByteArray(0), 0, t.Length);
                result.Write(LIST_LEVEL_START_AT, 0, LIST_LEVEL_START_AT.Length);
                result.Write(t = IntToByteArray(1), 0, t.Length);
                result.Write(OPEN_GROUP, 0, OPEN_GROUP.Length);
                result.Write(LIST_LEVEL_TEXT, 0, LIST_LEVEL_TEXT.Length);
                if (numbered) {
                    result.Write(LIST_LEVEL_STYLE_NUMBERED_BEGIN, 0, LIST_LEVEL_STYLE_NUMBERED_BEGIN.Length);
                    if (listLevel < 10) {
                        result.Write(t = IntToByteArray(0), 0, t.Length);
                    }
                    result.Write(t = IntToByteArray(listLevel), 0, t.Length);
                    result.Write(LIST_LEVEL_STYLE_NUMBERED_END, 0, LIST_LEVEL_STYLE_NUMBERED_END.Length);
                } else {
                    result.Write(LIST_LEVEL_STYLE_BULLETED, 0, LIST_LEVEL_STYLE_BULLETED.Length);
                }
                result.Write(CLOSE_GROUP, 0, CLOSE_GROUP.Length);
                result.Write(OPEN_GROUP, 0, OPEN_GROUP.Length);
                result.Write(LIST_LEVEL_NUMBERS_BEGIN, 0, LIST_LEVEL_NUMBERS_BEGIN.Length);
                if (numbered) {
                    result.Write(LIST_LEVEL_NUMBERS_NUMBERED, 0, LIST_LEVEL_NUMBERS_NUMBERED.Length);
                }
                result.Write(LIST_LEVEL_NUMBERS_END, 0, LIST_LEVEL_NUMBERS_END.Length);
                result.Write(CLOSE_GROUP, 0, CLOSE_GROUP.Length);
                result.Write(ST.RtfFontList.FONT_NUMBER, 0, ST.RtfFontList.FONT_NUMBER.Length);
                if (numbered) {
                    result.Write(t = IntToByteArray(fontNumber.GetFontNumber()), 0, t.Length);
                } else {
                    result.Write(t = IntToByteArray(fontBullet.GetFontNumber()), 0, t.Length);
                }
                result.Write(t = WriteIndentations(), 0, t.Length);
                result.Write(LIST_LEVEL_SYMBOL_INDENT, 0, LIST_LEVEL_SYMBOL_INDENT.Length);
                result.Write(t = IntToByteArray(this.leftIndent), 0, t.Length);
                result.Write(CLOSE_GROUP, 0, CLOSE_GROUP.Length);
                result.WriteByte((byte)'\n');
                for (int i = 0; i < items.Count; i++) {
                    RtfElement rtfElement = (RtfElement) items[i];
                    if (rtfElement is RtfList) {
                        result.Write(t = ((RtfList) rtfElement).WriteDefinition(), 0, t.Length);
                        break;
                    } else if (rtfElement is RtfListItem) {
                        byte[] data = ((RtfListItem) rtfElement).WriteDefinition();
                        if (data.Length > 0) {
                            result.Write(data, 0, data.Length);
                            break;
                        }
                    }
                }
            } catch (IOException) {
            }
            return result.ToArray();
        }

        /**
        * Writes the initialisation part of the RtfList
        * 
        * @return A byte array containing the initialisation part
        */
        protected internal byte[] WriteListBeginning() {
            MemoryStream result = new MemoryStream();
            try {
                byte[] t;
                result.Write(RtfParagraph.PARAGRAPH_DEFAULTS, 0, RtfParagraph.PARAGRAPH_DEFAULTS.Length);
                if (this.inTable) {
                    result.Write(RtfParagraph.IN_TABLE, 0, RtfParagraph.IN_TABLE.Length);
                }
                switch (this.alignment) {
                    case Element.ALIGN_LEFT:
                        result.Write(ST.RtfParagraphStyle.ALIGN_LEFT, 0, ST.RtfParagraphStyle.ALIGN_LEFT.Length);
                        break;
                    case Element.ALIGN_RIGHT:
                        result.Write(ST.RtfParagraphStyle.ALIGN_RIGHT, 0, ST.RtfParagraphStyle.ALIGN_RIGHT.Length);
                        break;
                    case Element.ALIGN_CENTER:
                        result.Write(ST.RtfParagraphStyle.ALIGN_CENTER, 0, ST.RtfParagraphStyle.ALIGN_CENTER.Length);
                        break;
                    case Element.ALIGN_JUSTIFIED:
                    case Element.ALIGN_JUSTIFIED_ALL:
                        result.Write(ST.RtfParagraphStyle.ALIGN_JUSTIFY, 0, ST.RtfParagraphStyle.ALIGN_JUSTIFY.Length);
                        break;
                }
                result.Write(t = WriteIndentations(), 0, t.Length);
                result.Write(ST.RtfFont.FONT_SIZE, 0, ST.RtfFont.FONT_SIZE.Length);
                result.Write(t = IntToByteArray(fontNumber.GetFontSize() * 2), 0, t.Length);
                if (this.symbolIndent > 0) { // TODO This is a slight hack. Replace with a call to tab support when implemented.
                    result.Write(t = DocWriter.GetISOBytes("\\tx"), 0, t.Length);
                    result.Write(t = IntToByteArray(this.leftIndent), 0, t.Length);
                }
            } catch (IOException) {
            }
            return result.ToArray();
        }

        /**
        * Writes only the list number and list level number.
        * 
        * @return The list number and list level number of this RtfList.
        */
        protected byte[] WriteListNumbers() {
            MemoryStream result = new MemoryStream();
            try {
                byte[] t;
                result.Write(RtfListTable.LIST_NUMBER, 0, RtfListTable.LIST_NUMBER.Length);
                result.Write(t = IntToByteArray(listNumber), 0, t.Length);
                if (listLevel > 0) {
                    result.Write(LIST_LEVEL_NUMBER, 0, LIST_LEVEL_NUMBER.Length);
                    result.Write(t = IntToByteArray(listLevel), 0, t.Length);
                }
            } catch (IOException) {
            }
            return result.ToArray();
        }
        
        /**
        * Writes the content of the RtfList
        * 
        * @return A byte array containing the actual content of the RtfList
        */
        public override byte[] Write()  {
            MemoryStream result = new MemoryStream();
            try {
                byte[] t;
                result.Write(t = WriteListBeginning(), 0, t.Length);
                result.Write(t = WriteListNumbers(), 0, t.Length);
                result.Write(OPEN_GROUP, 0, OPEN_GROUP.Length);
                int itemNr = 0;
                for (int i = 0; i < items.Count; i++) {
                    RtfElement rtfElement = (RtfElement) items[i];
                    if (rtfElement is RtfListItem) {
                        itemNr++;
                        result.Write(OPEN_GROUP, 0, OPEN_GROUP.Length);
                        result.Write(LIST_TEXT, 0, LIST_TEXT.Length);
                        result.Write(RtfParagraph.PARAGRAPH_DEFAULTS, 0, RtfParagraph.PARAGRAPH_DEFAULTS.Length);
                        if (this.inTable) {
                            result.Write(RtfParagraph.IN_TABLE, 0, RtfParagraph.IN_TABLE.Length);
                        }
                        result.Write(ST.RtfFontList.FONT_NUMBER, 0, ST.RtfFontList.FONT_NUMBER.Length);
                        if (numbered) {
                            result.Write(t = IntToByteArray(fontNumber.GetFontNumber()), 0, t.Length);
                        } else {
                            result.Write(t = IntToByteArray(fontBullet.GetFontNumber()), 0, t.Length);
                        }
                        result.Write(t = WriteIndentations(), 0, t.Length);
                        result.Write(DELIMITER, 0, DELIMITER.Length);
                        if (numbered) {
                            result.Write(t = this.IntToByteArray(itemNr), 0, t.Length);
                            result.Write(LIST_NUMBER_END, 0, LIST_NUMBER_END.Length);
                        } else {
                            result.Write(LIST_BULLET, 0, LIST_BULLET.Length);
                        }
                        result.Write(TAB, 0, TAB.Length);
                        result.Write(CLOSE_GROUP, 0, CLOSE_GROUP.Length);
                        result.Write(t = rtfElement.Write(), 0, t.Length);
                        result.Write(RtfParagraph.PARAGRAPH, 0, RtfParagraph.PARAGRAPH.Length);
                        if(((RtfListItem) rtfElement).IsContainsInnerList()) {
                            result.Write(t = WriteListNumbers(), 0, t.Length);
                        }
                        result.WriteByte((byte)'\n');
                    } else if (rtfElement is RtfList) {
                        result.Write(t = rtfElement.Write(), 0, t.Length);
                        result.Write(t = WriteListBeginning(), 0, t.Length);
                        result.WriteByte((byte)'\n');
                    }
                }
                result.Write(CLOSE_GROUP, 0, CLOSE_GROUP.Length);
                if (!this.inTable) {
                    result.Write(RtfParagraph.PARAGRAPH_DEFAULTS, 0, RtfParagraph.PARAGRAPH_DEFAULTS.Length);
                }
            } catch (IOException) {
            }
            return result.ToArray();
        }
        
        
        /**
        * Gets the list level of this RtfList
        * 
        * @return Returns the list level.
        */
        public int GetListLevel() {
            return listLevel;
        }
        
        /**
        * Sets the list level of this RtfList. A list level > 0 will
        * unregister this RtfList from the RtfListTable
        * 
        * @param listLevel The list level to set.
        */
        public void SetListLevel(int listLevel) {
            this.listLevel = listLevel;
            if (this.listLevel != 0) {
                document.GetDocumentHeader().FreeListNumber(this);
                for (int i = 0; i < this.items.Count; i++) {
                    if (this.items[i] is RtfList) {
                        ((RtfList) this.items[i]).SetListNumber(this.listNumber);
                        ((RtfList) this.items[i]).SetListLevel(this.listLevel + 1);
                    }
                }
            } else {
                this.listNumber = document.GetDocumentHeader().GetListNumber(this);
            }
        }
        
        /**
        * Sets the parent RtfList of this RtfList
        * 
        * @param parent The parent RtfList to use.
        */
        protected internal void SetParent(RtfList parent) {
            this.parentList = parent;
        }

        /**
        * Gets the id of this list
        * 
        * @return Returns the list number.
        */
        public int GetListNumber() {
            return listNumber;
        }
        
        /**
        * Sets the id of this list
        * 
        * @param listNumber The list number to set.
        */
        public void SetListNumber(int listNumber) {
            this.listNumber = listNumber;
        }
        
        /**
        * Sets whether this RtfList is in a table. Sets the correct inTable setting for all
        * child elements.
        * 
        * @param inTable <code>True</code> if this RtfList is in a table, <code>false</code> otherwise
        */
        public override void SetInTable(bool inTable) {
            base.SetInTable(inTable);
            for (int i = 0; i < this.items.Count; i++) {
                ((IRtfBasicElement) this.items[i]).SetInTable(inTable);
            }
        }
        
        /**
        * Sets whether this RtfList is in a header. Sets the correct inTable setting for all
        * child elements.
        * 
        * @param inHeader <code>True</code> if this RtfList is in a header, <code>false</code> otherwise
        */
        public override void SetInHeader(bool inHeader) {
            base.SetInHeader(inHeader);
            for (int i = 0; i < this.items.Count; i++) {
                ((IRtfBasicElement) this.items[i]).SetInHeader(inHeader);
            }
        }

        /**
        * Correct the indentation of this RtfList by adding left/first line indentation
        * from the parent RtfList. Also calls correctIndentation on all child RtfLists.
        */
        protected internal void CorrectIndentation() {
            if (this.parentList != null) {
                this.leftIndent = this.leftIndent + this.parentList.GetLeftIndent() + this.parentList.GetFirstIndent();
            }
            for (int i = 0; i < this.items.Count; i++) {
                if (this.items[i] is RtfList) {
                    ((RtfList) this.items[i]).CorrectIndentation();
                } else if(this.items[i] is RtfListItem) {
                    ((RtfListItem) this.items[i]).CorrectIndentation();
                }
            }
        }

        /**
        * Get the left indentation of this RtfList.
        * 
        * @return The left indentation.
        */
        private int GetLeftIndent() {
            return this.leftIndent;
        }
        
        /**
        * Get the first line indentation of this RtfList.
        * 
        * @return The first line indentation.
        */
        private int GetFirstIndent() {
            return this.firstIndent;
        }
    }
}