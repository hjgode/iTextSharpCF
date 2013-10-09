using System;
using System.IO;
using System.Collections;
using iTextSharp.text;
/**
 * $Id: RtfTable.cs,v 1.1 2005/07/04 22:51:35 psoares33 Exp $
 * $Name:  $
 *
 * Copyright 2001, 2002 by Mark Hall
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
    * A Helper Class for the <CODE>RtfWriter</CODE>.
    * <P>
    * Do not use it directly, except if you want to write a <CODE>DocumentListener</CODE> for Rtf
    * 
    * ONLY FOR USE WITH THE RtfWriter NOT with the RtfWriter2.
    *
    * Parts of this Class were contributed by Steffen Stundzig. Many thanks for the
    * improvements.
    * Updates Benoit WIART <b.wiart@proxiad.com>
    */
    public class RtfTable {
        /** Stores the different rows. */
        private ArrayList rowsList = new ArrayList();
        /** Stores the RtfWriter, which created this RtfTable. */
        private RtfWriter writer = null;
        /** Stores the Table, which this RtfTable is based on. */
        private Table origTable = null;



        /**
        * Create a new <code>RtfTable</code>.
        *
        * @param writer The <code>RtfWriter</code> that created this Table
        */
        public RtfTable(RtfWriter writer) : base() {
            this.writer = writer;
        }

        /**
        * Import a <CODE>Table</CODE> into the <CODE>RtfTable</CODE>.
        * <P>
        * @param table A <code>Table</code> specifying the <code>Table</code> to be imported
        * @param pageWidth An <code>int</code> specifying the page width
        * @return true if importing the table succeeded
        */
        public bool ImportTable(Table table, int pageWidth) {
            origTable = table;
            // All Cells are pregenerated first, so that cell and rowspanning work

            int tableWidth = (int) table.WidthPercentage;
            int cellpadding = (int) (table.Cellpadding * RtfWriter.TWIPSFACTOR);
            int cellspacing = (int) (table.Cellspacing * RtfWriter.TWIPSFACTOR);
            float[] propWidths = table.ProportionalWidths;

            int borders = table.Border;
            Color borderColor = table.BorderColor;
            float borderWidth = table.BorderWidth;

            for (int j = 0; j < table.Size; j++) {
                RtfRow rtfRow = new RtfRow(writer, this);
                rtfRow.PregenerateRows(table.Columns);
                rowsList.Add(rtfRow);
            }
            int i = 0;
            foreach (Row row in table) {
                row.HorizontalAlignment = table.Alignment;
                RtfRow rtfRow = (RtfRow) rowsList[i];
                rtfRow.ImportRow(row, propWidths, tableWidth, pageWidth, cellpadding, cellspacing, borders, borderColor, borderWidth, i);
                i++;
            }
            return true;
        }

        /**
        * Output the content of the <CODE>RtfTable</CODE> to an Stream.
        *
        * @param os The <code>Stream</code> that the content of the <code>RtfTable</code> is to be written to
        * @return true if writing the table succeeded
        * @throws DocumentException
        * @throws IOException
        */
        public bool WriteTable(MemoryStream os) {
            
            if (!this.writer.WritingHeaderFooter()) {
                // Added by Benoit WIART <b.wiart@proxiad.com>
                // Add a new line before each table
                os.WriteByte(RtfWriter.escape);
                os.Write(RtfWriter.paragraph, 0, RtfWriter.paragraph.Length);
            }
                
            int size = rowsList.Count;
            for (int i = 0; i < size; i++) {
                RtfRow row = (RtfRow) rowsList[i];
                row.WriteRow(os, i, origTable);
                os.WriteByte((byte) '\n');
            }
            if (!writer.WritingHeaderFooter()) {
                os.WriteByte(RtfWriter.escape);
                os.Write(RtfWriter.paragraphDefaults, 0, RtfWriter.paragraphDefaults.Length);
                os.WriteByte(RtfWriter.escape);
                os.Write(RtfWriter.paragraph, 0, RtfWriter.paragraph.Length);
                switch (origTable.Alignment) {
                    case Element.ALIGN_LEFT:
                        os.WriteByte(RtfWriter.escape);
                        os.Write(RtfWriter.alignLeft, 0, RtfWriter.alignLeft.Length);
                        break;
                    case Element.ALIGN_RIGHT:
                        os.WriteByte(RtfWriter.escape);
                        os.Write(RtfWriter.alignRight, 0, RtfWriter.alignRight.Length);
                        break;
                    case Element.ALIGN_CENTER:
                        os.WriteByte(RtfWriter.escape);
                        os.Write(RtfWriter.alignCenter, 0, RtfWriter.alignCenter.Length);
                        break;
                    case Element.ALIGN_JUSTIFIED:
                    case Element.ALIGN_JUSTIFIED_ALL:
                        os.WriteByte(RtfWriter.escape);
                        os.Write(RtfWriter.alignJustify, 0, RtfWriter.alignJustify.Length);
                        break;
                }
            }
            return true;
        }

        /**
        * <code>RtfCell</code>s call this method to specify that a certain other cell is to be merged with it.
        *
        * @param x The column position of the cell to be merged
        * @param y The row position of the cell to be merged
        * @param mergeType The merge type specifies the kind of merge to be applied (MERGE_HORIZ_PREV, MERGE_VERT_PREV, MERGE_BOTH_PREV)
        * @param mergeCell The <code>RtfCell</code> that the cell at x and y is to be merged with
        */
        public void SetMerge(int x, int y, int mergeType, RtfCell mergeCell) {
            RtfRow row = (RtfRow) rowsList[y];
            row.SetMerge(x, mergeType, mergeCell);
        }

        /**
        * This method allows access to the original Table that led to this RtfTable.
        *
        * @return The Table object that is the basis of this RtfTable.
        */
        protected internal Table GetOriginalTable() {
            return origTable;
        }
    }
}