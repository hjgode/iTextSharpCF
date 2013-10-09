using System;
using System.IO;
using System.util;
using iTextSharp.text;
/**
 * $Id: RtfCell.cs,v 1.3 2005/12/09 12:34:52 psoares33 Exp $
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
    * Do not use it directly
    * 
    * ONLY FOR USE WITH THE RtfWriter NOT with the RtfWriter2.
    *
    * Parts of this Class were contributed by Steffen Stundzig. Many thanks for the
    * improvements.
    * Updates by Benoit WIART <b.wiart@proxiad.com>
    */
    public class RtfCell {
        /** Constants for merging Cells */

        /** A possible value for merging */
        private const int MERGE_HORIZ_FIRST = 1;
        /** A possible value for merging */
        private const int MERGE_VERT_FIRST = 2;
        /** A possible value for merging */
        private const int MERGE_BOTH_FIRST = 3;
        /** A possible value for merging */
        private const int MERGE_HORIZ_PREV = 4;
        /** A possible value for merging */
        private const int MERGE_VERT_PREV = 5;
        /** A possible value for merging */
        private const int MERGE_BOTH_PREV = 6;

        /**
        * RTF Tags
        */

        /** First cell to merge with - Horizontal */
        private static byte[] cellMergeFirst = DocWriter.GetISOBytes("clmgf");
        /** First cell to merge with - Vertical */
        private static byte[] cellVMergeFirst = DocWriter.GetISOBytes("clvmgf");
        /** Merge cell with previous horizontal cell */
        private static byte[] cellMergePrev = DocWriter.GetISOBytes("clmrg");
        /** Merge cell with previous vertical cell */
        private static byte[] cellVMergePrev = DocWriter.GetISOBytes("clvmrg");
        /** Cell content vertical alignment bottom */
        private static byte[] cellVerticalAlignBottom = DocWriter.GetISOBytes("clvertalb");
        /** Cell content vertical alignment center */
        private static byte[] cellVerticalAlignCenter = DocWriter.GetISOBytes("clvertalc");
        /** Cell content vertical alignment top */
        private static byte[] cellVerticalAlignTop = DocWriter.GetISOBytes("clvertalt");
        /** Cell border left */
        private static byte[] cellBorderLeft = DocWriter.GetISOBytes("clbrdrl");
        /** Cell border right */
        private static byte[] cellBorderRight = DocWriter.GetISOBytes("clbrdrr");
        /** Cell border top */
        private static byte[] cellBorderTop = DocWriter.GetISOBytes("clbrdrt");
        /** Cell border bottom */
        private static byte[] cellBorderBottom = DocWriter.GetISOBytes("clbrdrb");
        /** Cell background color */
        private static byte[] cellBackgroundColor = DocWriter.GetISOBytes("clcbpat");
        /** Cell width format */
        private static byte[] cellWidthStyle = DocWriter.GetISOBytes("clftsWidth3");
        /** Cell width */
        private static byte[] cellWidthTag = DocWriter.GetISOBytes("clwWidth");
        /** Cell right border position */
        private static byte[] cellRightBorder = DocWriter.GetISOBytes("cellx");
        /** Cell is part of table */
        protected internal static byte[] cellInTable = DocWriter.GetISOBytes("intbl");
        /** End of cell */
        private static byte[] cellEnd = DocWriter.GetISOBytes("cell");

        /** padding top */
        private static byte[] cellPaddingTop = DocWriter.GetISOBytes("clpadt");
        /** padding top unit */
        private static byte[] cellPaddingTopUnit = DocWriter.GetISOBytes("clpadft3");
        /** padding bottom */
        private static byte[] cellPaddingBottom = DocWriter.GetISOBytes("clpadb");
        /** padding bottom unit */
        private static byte[] cellPaddingBottomUnit = DocWriter.GetISOBytes("clpadfb3");
        /** padding left */
        private static byte[] cellPaddingLeft = DocWriter.GetISOBytes("clpadl");
        /** padding left unit */
        private static byte[] cellPaddingLeftUnit = DocWriter.GetISOBytes("clpadfl3");
        /** padding right */
        private static byte[] cellPaddingRight = DocWriter.GetISOBytes("clpadr");
        /** padding right unit */
        private static byte[] cellPaddingRightUnit = DocWriter.GetISOBytes("clpadfr3");

        /** The <code>RtfWriter</code> to which this <code>RtfCell</code> belongs. */
        private RtfWriter writer = null;
        /** The <code>RtfTable</code> to which this <code>RtfCell</code> belongs. */
        private RtfTable mainTable = null;

        /** Cell width */
        private int cellWidth = 0;
        /** Cell right border position */
        private int cellRight = 0;
        /** <code>Cell</code> containing the actual data */
        private Cell store = null;
        /** Is this an empty cell */
        private bool emptyCell = true;
        /** Type of merging to do */
        private int mergeType = 0;
        /** cell padding, because the table only renders the left and right cell padding
        * and not the top and bottom one
        */
        private int cellpadding = 0;

        /**
        * Create a new <code>RtfCell</code>.
        *
        * @param writer The <code>RtfWriter</code> that this <code>RtfCell</code> belongs to
        * @param mainTable The <code>RtfTable</code> that created the
        * <code>RtfRow</code> that created the <code>RtfCell</code> :-)
        */
        public RtfCell(RtfWriter writer, RtfTable mainTable) : base(){
            this.writer = writer;
            this.mainTable = mainTable;
        }

        /**
        * Import a <code>Cell</code>.
        * <P>
        * @param cell The <code>Cell</code> containing the data for this
        * <code>RtfCell</code>
        * @param cellLeft The position of the left border
        * @param cellWidth The default width of a cell
        * @param x The column index of this <code>RtfCell</code>
        * @param y The row index of this <code>RtfCell</code>
        * @param cellpadding the cellpadding
        * @return the position of the right side of the cell
        */
        public int ImportCell(Cell cell, int cellLeft, int cellWidth, int x, int y, int cellpadding) {
            this.cellpadding = cellpadding;

            // set this value in any case
            this.cellWidth = cellWidth;
            if (cell == null) {
                cellRight = cellLeft + cellWidth;
                return cellRight;
            }
            if (cell.CellWidth != null && !cell.CellWidth.Equals("")) {

                this.cellWidth = (int)(int.Parse(cell.CellWidth) * RtfWriter.TWIPSFACTOR);
            }
            cellRight = cellLeft + this.cellWidth;
            store = cell;
            emptyCell = false;
            if (cell.Colspan > 1) {
                if (cell.Rowspan > 1) {
                    mergeType = MERGE_BOTH_FIRST;
                    for (int i = y; i < y + cell.Rowspan; i++) {
                        if (i > y) mainTable.SetMerge(x, i, MERGE_VERT_PREV, this);
                        for (int j = x + 1; j < x + cell.Colspan; j++) {
                            mainTable.SetMerge(j, i, MERGE_BOTH_PREV, this);
                        }
                    }
                } else {
                    mergeType = MERGE_HORIZ_FIRST;
                    for (int i = x + 1; i < x + cell.Colspan; i++) {
                        mainTable.SetMerge(i, y, MERGE_HORIZ_PREV, this);
                    }
                }
            } else if (cell.Rowspan > 1) {
                mergeType = MERGE_VERT_FIRST;
                for (int i = y + 1; i < y + cell.Rowspan; i++) {
                    mainTable.SetMerge(x, i, MERGE_VERT_PREV, this);
                }
            }
            return cellRight;
        }

        /**
        * Write the properties of the <code>RtfCell</code>.
        *
        * @param os The <code>Stream</code> to which to write the properties
        * of the <code>RtfCell</code> to.
        * @return true if writing the cell settings succeeded
        * @throws DocumentException
        */
        public bool WriteCellSettings(MemoryStream os) {
            try {
                float lWidth, tWidth, rWidth, bWidth;
                byte[] lStyle, tStyle, rStyle, bStyle;

                if (store is RtfTableCell) {
                    RtfTableCell c = (RtfTableCell) store;
                    lWidth = c.LeftBorderWidth;
                    tWidth = c.TopBorderWidth;
                    rWidth = c.RightBorderWidth;
                    bWidth = c.BottomBorderWidth;
                    lStyle = RtfTableCell.GetStyleControlWord(c.LeftBorderStyle);
                    tStyle = RtfTableCell.GetStyleControlWord(c.TopBorderStyle);
                    rStyle = RtfTableCell.GetStyleControlWord(c.RightBorderStyle);
                    bStyle = RtfTableCell.GetStyleControlWord(c.BottomBorderStyle);
                } else {
                    lWidth = tWidth = rWidth = bWidth = store.BorderWidth;
                    lStyle = tStyle = rStyle = bStyle = RtfRow.tableBorder;
                }

                if (mergeType == MERGE_HORIZ_PREV || mergeType == MERGE_BOTH_PREV) {
                    return true;
                }
                switch (mergeType) {
                    case MERGE_VERT_FIRST:
                        os.WriteByte(RtfWriter.escape);
                        os.Write(cellVMergeFirst, 0, cellVMergeFirst.Length);
                        break;
                    case MERGE_BOTH_FIRST:
                        os.WriteByte(RtfWriter.escape);
                        os.Write(cellVMergeFirst, 0, cellVMergeFirst.Length);
                        break;
                    case MERGE_HORIZ_PREV:
                        os.WriteByte(RtfWriter.escape);
                        os.Write(cellMergePrev, 0, cellMergePrev.Length);
                        break;
                    case MERGE_VERT_PREV:
                        os.WriteByte(RtfWriter.escape);
                        os.Write(cellVMergePrev, 0, cellVMergePrev.Length);
                        break;
                    case MERGE_BOTH_PREV:
                        os.WriteByte(RtfWriter.escape);
                        os.Write(cellMergeFirst, 0, cellMergeFirst.Length);
                        break;
                }
                switch (store.VerticalAlignment) {
                    case Element.ALIGN_BOTTOM:
                        os.WriteByte(RtfWriter.escape);
                        os.Write(cellVerticalAlignBottom, 0, cellVerticalAlignBottom.Length);
                        break;
                    case Element.ALIGN_CENTER:
                    case Element.ALIGN_MIDDLE:
                        os.WriteByte(RtfWriter.escape);
                        os.Write(cellVerticalAlignCenter, 0, cellVerticalAlignCenter.Length);
                        break;
                    case Element.ALIGN_TOP:
                        os.WriteByte(RtfWriter.escape);
                        os.Write(cellVerticalAlignTop, 0, cellVerticalAlignTop.Length);
                        break;
                }

                if (((store.Border & Rectangle.LEFT_BORDER) == Rectangle.LEFT_BORDER) &&
                        (lWidth > 0)) {
                    os.WriteByte(RtfWriter.escape);
                    os.Write(cellBorderLeft, 0, cellBorderLeft.Length);
                    os.WriteByte(RtfWriter.escape);
                    os.Write(lStyle, 0, lStyle.Length);
                    os.WriteByte(RtfWriter.escape);
                    os.Write(RtfRow.tableBorderWidth, 0, RtfRow.tableBorderWidth.Length);
                    WriteInt(os, (int) (lWidth * RtfWriter.TWIPSFACTOR));
                    os.WriteByte(RtfWriter.escape);
                    os.Write(RtfRow.tableBorderColor, 0, RtfRow.tableBorderColor.Length);
                    if (store.BorderColor == null)
                        WriteInt(os, writer.AddColor(new
                                Color(0, 0, 0)));
                    else
                        WriteInt(os, writer.AddColor(store.BorderColor));
                    os.WriteByte((byte) '\n');
                }
                if (((store.Border & Rectangle.TOP_BORDER) == Rectangle.TOP_BORDER) && (tWidth > 0)) {
                    os.WriteByte(RtfWriter.escape);
                    os.Write(cellBorderTop, 0, cellBorderTop.Length);
                    os.WriteByte(RtfWriter.escape);
                    os.Write(tStyle, 0, tStyle.Length);
                    os.WriteByte(RtfWriter.escape);
                    os.Write(RtfRow.tableBorderWidth, 0, RtfRow.tableBorderWidth.Length);
                    WriteInt(os, (int) (tWidth * RtfWriter.TWIPSFACTOR));
                    os.WriteByte(RtfWriter.escape);
                    os.Write(RtfRow.tableBorderColor, 0, RtfRow.tableBorderColor.Length);
                    if (store.BorderColor == null)
                        WriteInt(os, writer.AddColor(new
                                Color(0, 0, 0)));
                    else
                        WriteInt(os, writer.AddColor(store.BorderColor));
                    os.WriteByte((byte) '\n');
                }
                if (((store.Border & Rectangle.BOTTOM_BORDER) == Rectangle.BOTTOM_BORDER) &&
                        (bWidth > 0)) {
                    os.WriteByte(RtfWriter.escape);
                    os.Write(cellBorderBottom, 0, cellBorderBottom.Length);
                    os.WriteByte(RtfWriter.escape);
                    os.Write(bStyle, 0, bStyle.Length);
                    os.WriteByte(RtfWriter.escape);
                    os.Write(RtfRow.tableBorderWidth, 0, RtfRow.tableBorderWidth.Length);
                    WriteInt(os, (int) (bWidth * RtfWriter.TWIPSFACTOR));
                    os.WriteByte(RtfWriter.escape);
                    os.Write(RtfRow.tableBorderColor, 0, RtfRow.tableBorderColor.Length);
                    if (store.BorderColor == null)
                        WriteInt(os, writer.AddColor(new
                                Color(0, 0, 0)));
                    else
                        WriteInt(os, writer.AddColor(store.BorderColor));
                    os.WriteByte((byte) '\n');
                }
                if (((store.Border & Rectangle.RIGHT_BORDER) == Rectangle.RIGHT_BORDER) &&
                        (rWidth > 0)) {
                    os.WriteByte(RtfWriter.escape);
                    os.Write(cellBorderRight, 0, cellBorderRight.Length);
                    os.WriteByte(RtfWriter.escape);
                    os.Write(rStyle, 0, rStyle.Length);
                    os.WriteByte(RtfWriter.escape);
                    os.Write(RtfRow.tableBorderWidth, 0, RtfRow.tableBorderWidth.Length);
                    WriteInt(os, (int) (rWidth * RtfWriter.TWIPSFACTOR));
                    os.WriteByte(RtfWriter.escape);
                    os.Write(RtfRow.tableBorderColor, 0, RtfRow.tableBorderColor.Length);
                    if (store.BorderColor == null)
                        WriteInt(os, writer.AddColor(new Color(0, 0, 0)));
                    else
                        WriteInt(os, writer.AddColor(store.BorderColor));
                    os.WriteByte((byte) '\n');
                }
                os.WriteByte(RtfWriter.escape);
                os.Write(cellBackgroundColor, 0, cellBackgroundColor.Length);
                if (store.BackgroundColor == null) {
                    WriteInt(os, writer.AddColor(new Color(255, 255, 255)));
                } else if (store.BackgroundColor != null) {
                    WriteInt(os, writer.AddColor(store.BackgroundColor));
                }
                os.WriteByte((byte) '\n');
                os.WriteByte(RtfWriter.escape);
                os.Write(cellWidthStyle, 0, cellWidthStyle.Length);
                os.WriteByte((byte) '\n');
                os.WriteByte(RtfWriter.escape);
                os.Write(cellWidthTag, 0, cellWidthTag.Length);
                WriteInt(os, cellWidth);
                os.WriteByte((byte) '\n');
                if (cellpadding > 0) {
                    // values
                    os.WriteByte(RtfWriter.escape);
                    os.Write(cellPaddingLeft, 0, cellPaddingLeft.Length);
                    WriteInt(os, cellpadding / 2);
                    os.WriteByte(RtfWriter.escape);
                    os.Write(cellPaddingTop, 0, cellPaddingTop.Length);
                    WriteInt(os, cellpadding / 2);
                    os.WriteByte(RtfWriter.escape);
                    os.Write(cellPaddingRight, 0, cellPaddingRight.Length);
                    WriteInt(os, cellpadding / 2);
                    os.WriteByte(RtfWriter.escape);
                    os.Write(cellPaddingBottom, 0, cellPaddingBottom.Length);
                    WriteInt(os, cellpadding / 2);
                    // unit
                    os.WriteByte(RtfWriter.escape);
                    os.Write(cellPaddingLeftUnit, 0, cellPaddingLeftUnit.Length);
                    os.WriteByte(RtfWriter.escape);
                    os.Write(cellPaddingTopUnit, 0, cellPaddingTopUnit.Length);
                    os.WriteByte(RtfWriter.escape);
                    os.Write(cellPaddingRightUnit, 0, cellPaddingRightUnit.Length);
                    os.WriteByte(RtfWriter.escape);
                    os.Write(cellPaddingBottomUnit, 0, cellPaddingBottomUnit.Length);
                }
                os.WriteByte(RtfWriter.escape);
                os.Write(cellRightBorder, 0, cellRightBorder.Length);
                WriteInt(os, cellRight);
            } catch (IOException) {
                return false;
            }
            return true;
        }

        /**
        * Write the content of the <code>RtfCell</code>.
        *
        * @param os The <code>Stream</code> to which to write the content of
        * the <code>RtfCell</code> to.
        * @return true if writing the cell content succeeded
        * @throws DocumentException
        */
        public bool WriteCellContent(MemoryStream os) {
            try {
                if (mergeType == MERGE_HORIZ_PREV || mergeType == MERGE_BOTH_PREV) {
                    return true;
                }
                
                if (!emptyCell) {
                    Paragraph container = null;
                    ListIterator cellIterator = new ListIterator(store.Elements);
                    while (cellIterator.HasNext()) {
                        IElement element = (IElement)cellIterator.Next();
                        // should we wrap it in a paragraph
                        if (!(element is Paragraph)) {
                            if (container != null) {
                                container.Add(element);
                            } else {
                                container = new Paragraph();
                                container.Alignment = store.HorizontalAlignment;
                                container.Add(element);
                            }
                        } else {
                            if (container != null) {
                                writer.AddElement(container, os);
                                container =null;
                                container =null;
                            }
                            
                            
                            // if horizontal alignment is undefined overwrite
                            // with that of enclosing cell
                            if (element is Paragraph && ((Paragraph) element).Alignment == Element.ALIGN_UNDEFINED) {
                                ((Paragraph) element).Alignment = store.HorizontalAlignment;
                            }
                            writer.AddElement(element, os);
                            if (element.Type == Element.PARAGRAPH && cellIterator.HasNext()) {
                                os.WriteByte(RtfWriter.escape);
                                os.Write(RtfWriter.paragraph, 0, RtfWriter.paragraph.Length);
                            }
                        }
                    }
                    if (container != null) {
                        writer.AddElement(container, os);
                        container =null;
                    }
                } else {
                    os.WriteByte(RtfWriter.escape);
                    os.Write(RtfWriter.paragraphDefaults, 0, RtfWriter.paragraphDefaults.Length);
                    os.WriteByte(RtfWriter.escape);
                    os.Write(cellInTable, 0, cellInTable.Length);
                }
                os.WriteByte(RtfWriter.escape);
                os.Write(cellEnd, 0, cellEnd.Length);
            } catch (IOException ) {
                return false;
            }
            return true;
        }

        /**
        * Sets the merge type and the <code>RtfCell</code> with which this
        * <code>RtfCell</code> is to be merged.
        *
        * @param mergeType The merge type specifies the kind of merge to be applied
        * (MERGE_HORIZ_PREV, MERGE_VERT_PREV, MERGE_BOTH_PREV)
        * @param mergeCell The <code>RtfCell</code> that the cell at x and y is to
        * be merged with
        */
        public void SetMerge(int mergeType, RtfCell mergeCell) {
            this.mergeType = mergeType;
            store = mergeCell.GetStore();
        }

        /**
        * Get the <code>Cell</code> with the actual content.
        *
        * @return <code>Cell</code> which is contained in the <code>RtfCell</code>
        */
        public Cell GetStore() {
            return store;
        }

        /**
        * Get the with of this <code>RtfCell</code>
        *
        * @return Width of the current <code>RtfCell</code>
        */
        public int GetCellWidth() {
            return cellWidth;
        }

        /**
        * sets the width of the cell
        * @param value a width
        */
        public void SetCellWidth(int value) {
            cellWidth = value;
        }

        /**
        * Get the position of the right border of this <code>RtfCell</code>.
        * @return position of the right border
        */
        public int GetCellRight() {
            return cellRight;
        }


        /**
        * Sets the right position of the cell
        * @param value a cell position
        */
        public void SetCellRight(int value) {
            cellRight = value;
        }

        /**
        * Write an Integer to the Outputstream.
        *
        * @param outp The <code>Stream</code> to be written to.
        * @param i The int to be written.
        * @throws IOException
        */
        private void WriteInt(MemoryStream outp, int i) {
            byte[] t = DocWriter.GetISOBytes(i.ToString());
            outp.Write(t, 0, t.Length);
        }
    }
}