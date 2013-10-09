using System;
using System.IO;
using System.Collections;
using iTextSharp.text;
/**
 * $Id: RtfRow.cs,v 1.2 2006/09/25 09:29:51 psoares33 Exp $
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
    * Code added by c
    */
    public class RtfRow {
        /** Table border solid */
        public static byte[] tableBorder = DocWriter.GetISOBytes("brdrs");
        /** Table border width */
        public static byte[] tableBorderWidth = DocWriter.GetISOBytes("brdrw");
        /** Table border color */
        public static byte[] tableBorderColor = DocWriter.GetISOBytes("brdrcf");

        /** Table row defaults */
        private static byte[] rowBegin = DocWriter.GetISOBytes("trowd");
        /** End of table row */
        private static byte[] rowEnd = DocWriter.GetISOBytes("row");
        /** Table row autofit */
        private static byte[] rowAutofit = DocWriter.GetISOBytes("trautofit1");
        private static byte[] graphLeft = DocWriter.GetISOBytes("trgaph");
        /** Row border left */
        private static byte[] rowBorderLeft = DocWriter.GetISOBytes("trbrdrl");
        /** Row border right */
        private static byte[] rowBorderRight = DocWriter.GetISOBytes("trbrdrr");
        /** Row border top */
        private static byte[] rowBorderTop = DocWriter.GetISOBytes("trbrdrt");
        /** Row border bottom */
        private static byte[] rowBorderBottom = DocWriter.GetISOBytes("trbrdrb");
        /** Row border horiz inline */
        private static byte[] rowBorderInlineHorizontal = DocWriter.GetISOBytes("trbrdrh");
        /** Row border bottom */
        private static byte[] rowBorderInlineVertical = DocWriter.GetISOBytes("trbrdrv");
        /** Default cell spacing left */
        private static byte[] rowSpacingLeft = DocWriter.GetISOBytes("trspdl");
        /** Default cell spacing right */
        private static byte[] rowSpacingRight = DocWriter.GetISOBytes("trspdr");
        /** Default cell spacing top */
        private static byte[] rowSpacingTop = DocWriter.GetISOBytes("trspdt");
        /** Default cell spacing bottom */
        private static byte[] rowSpacingBottom = DocWriter.GetISOBytes("trspdb");
        /** Default cell spacing format left */
        private static byte[] rowSpacingLeftStyle = DocWriter.GetISOBytes("trspdfl3");
        /** Default cell spacing format right */
        private static byte[] rowSpacingRightStyle = DocWriter.GetISOBytes("trspdfr3");
        /** Default cell spacing format top */
        private static byte[] rowSpacingTopStyle = DocWriter.GetISOBytes("trspdft3");
        /** Default cell spacing format bottom */
        private static byte[] rowSpacingBottomStyle = DocWriter.GetISOBytes("trspdfb3");
        /** Default cell padding left */
        private static byte[] rowPaddingLeft = DocWriter.GetISOBytes("trpaddl");
        /** Default cell padding right */
        private static byte[] rowPaddingRight = DocWriter.GetISOBytes("trpaddr");
        /** Default cell padding format left */
        private static byte[] rowPaddingLeftStyle = DocWriter.GetISOBytes("trpaddfl3");
        /** Default cell padding format right */
        private static byte[] rowPaddingRightStyle = DocWriter.GetISOBytes("trpaddfr3");
        /** Row width format */
        private static byte[] rowWidthStyle = DocWriter.GetISOBytes("trftsWidth3");
        /** Row width */
        private static byte[] rowWidth = DocWriter.GetISOBytes("trwWidth");
        /**
        * Table row header. This row should appear at the top of every
        * page the current table appears on.
        */
        private static byte[] rowHeader = DocWriter.GetISOBytes("trhdr");
        /**
        * Table row keep together. This row cannot be split by a page break.
        * This property is assumed to be off unless the control word is
        * present.
        */
        private static byte[] rowKeep = DocWriter.GetISOBytes("trkeep");
        /** Table alignment left */
        private static byte[] rowAlignLeft = DocWriter.GetISOBytes("trql");
        /** Table alignment center */
        private static byte[] rowAlignCenter = DocWriter.GetISOBytes("trqc");
        /** Table alignment right */
        private static byte[] rowAlignRight = DocWriter.GetISOBytes("trqr");

        /** List of <code>RtfCell</code>s in this <code>RtfRow</code> */
        private ArrayList cells = new ArrayList();
        /** The <code>RtfWriter</code> to which this <code>RtfRow</code> belongs */
        private RtfWriter writer = null;
        /** The <coce>RtfTable</code> to which this <code>RtfRow</code> belongs */
        private RtfTable mainTable = null;

        /** The width of this <code>RtfRow</code> (in percent) */
        private int width = 100;
        /** The default cellpadding of <code>RtfCells</code> in this
        * <code>RtfRow</code> */
        private int cellpadding = 115;
        /** The default cellspacing of <code>RtfCells</code> in this
        * <code>RtfRow</code> */
        private int cellspacing = 14;
        /** The borders of this <code>RtfRow</code> */
        private int borders = 0;
        /** The border color of this <code>RtfRow</code> */
        private Color borderColor = null;
        /** The border width of this <code>RtfRow</code> */
        private float borderWidth = 0;

        /** Original Row */
        private Row origRow = null;

        /**
        * Create a new <code>RtfRow</code>.
        *
        * @param writer The <code>RtfWriter</code> that this <code>RtfRow</code> belongs to
        * @param mainTable The <code>RtfTable</code> that created this
        * <code>RtfRow</code>
        */
        public RtfRow(RtfWriter writer, RtfTable mainTable) : base() {
            this.writer = writer;
            this.mainTable = mainTable;
        }

        /**
        * Pregenerate the <code>RtfCell</code>s in this <code>RtfRow</code>.
        *
        * @param columns The number of <code>RtfCell</code>s to be generated.
        */
        public void PregenerateRows(int columns) {
            for (int i = 0; i < columns; i++) {
                RtfCell rtfCell = new RtfCell(writer, mainTable);
                cells.Add(rtfCell);
            }
        }

        /**
        * Import a <code>Row</code>.
        * <P>
        * All the parameters are taken from the <code>RtfTable</code> which contains
        * this <code>RtfRow</code> and they do exactely what they say
        * @param row
        * @param propWidths in percent
        * @param tableWidth in percent
        * @param pageWidth
        * @param cellpadding
        * @param cellspacing
        * @param borders
        * @param borderColor
        * @param borderWidth
        * @param y
        * @return true if importing the row succeeded
        */
        public bool ImportRow(Row row, float[] propWidths, int tableWidth, int pageWidth, int cellpadding,
                                int cellspacing, int borders, Color borderColor, float borderWidth,
                                int y) {
            // the width of this row is the absolute witdh, calculated from the
            // proportional with of the table and the total width of the page
            this.origRow = row;
            this.width = pageWidth * tableWidth / 100;
            this.cellpadding = cellpadding;
            this.cellspacing = cellspacing;
            this.borders = borders;
            this.borderColor = borderColor;
            this.borderWidth = borderWidth;

            if (this.borderWidth > 2) this.borderWidth = 2;

            int cellLeft = 0;
            for (int i = 0; i < row.Columns; i++) {
                IElement cell = (IElement) row.GetCell(i);

                // cellWidth is an absolute argument
                // it's based on the absolute of this row and the proportional
                // width of this column
                int cellWidth = (int) (width * propWidths[i] / 100);
                if (cell != null) {
                    if (cell.Type == Element.CELL) {
                        RtfCell rtfCell = (RtfCell) cells[i];
                        cellLeft = rtfCell.ImportCell((Cell) cell, cellLeft, cellWidth, i, y, cellpadding);
                    }
                } else {
                    RtfCell rtfCell = (RtfCell) cells[i];
                    cellLeft = rtfCell.ImportCell(null, cellLeft, cellWidth, i, y, cellpadding);
                }
            }

            // recalculate the cell right border and the cumulative width
            // on col spanning cells.
            // col + row spanning cells are also handled by this loop, because the real cell of
            // the upper left corner in such an col, row matrix is copied as first cell
            // in each row in this matrix
            int columns = row.Columns;
            for (int i = 0; i < columns; i++) {
                RtfCell firstCell = (RtfCell) cells[i];
                Cell cell = firstCell.GetStore();
                int cols = 0;
                if (cell != null) {
                    cols = cell.Colspan;
                }
                if (cols > 1) {
                    RtfCell lastCell = (RtfCell) cells[i + cols - 1];
                    firstCell.SetCellRight(lastCell.GetCellRight());
                    int width = firstCell.GetCellWidth();
                    for (int j = i + 1; j < i + cols; j++) {
                        RtfCell cCell = (RtfCell) cells[j];
                        width += cCell.GetCellWidth();
                    }
                    firstCell.SetCellWidth(width);
                    i += cols - 1;
                }
            }
            return true;
        }

        /**
        * Write the <code>RtfRow</code> to the specified <code>Stream</code>.
        *
        * @param os The <code>Stream</code> to which this <code>RtfRow</code>
        * should be written to.
        * @param rowNum The <code>index</code> of this row in the containing table.
        * @param table The <code>Table</code> which contains the original <code>Row</code>.
        * @return true if writing the row succeeded
        * @throws DocumentException
        * @throws IOException
        */
        public bool WriteRow(MemoryStream os, int rowNum, Table table) {
            os.WriteByte(RtfWriter.escape);
            os.Write(rowBegin, 0, rowBegin.Length);
            os.WriteByte((byte) '\n');
            os.WriteByte(RtfWriter.escape);
            os.Write(rowWidthStyle, 0, rowWidthStyle.Length);
            os.WriteByte(RtfWriter.escape);
            os.Write(rowWidth, 0, rowWidth.Length);
            WriteInt(os, width);
    //        os.Write(RtfWriter.escape);
    //        os.Write(rowAutofit);
            if (mainTable.GetOriginalTable().HasToFitPageCells()) {
                os.WriteByte(RtfWriter.escape);
                os.Write(rowKeep, 0, rowKeep.Length);
            }
            // check if this row is a header row
            if (rowNum < table.FirstDataRow) {
                os.WriteByte(RtfWriter.escape);
                os.Write(rowHeader, 0, rowHeader.Length);
            }
            os.WriteByte(RtfWriter.escape);
            switch (this.origRow.HorizontalAlignment) {
                case Element.ALIGN_LEFT:
                    os.Write(rowAlignLeft, 0, rowAlignLeft.Length);
                    break;
                case Element.ALIGN_CENTER:
                    os.Write(rowAlignCenter, 0, rowAlignCenter.Length);
                    break;
                case Element.ALIGN_RIGHT:
                    os.Write(rowAlignRight, 0, rowAlignRight.Length);
                    break;
                default :
                    os.Write(rowAlignLeft, 0, rowAlignLeft.Length);
                    break;
            }
            os.WriteByte(RtfWriter.escape);
            os.Write(graphLeft, 0, graphLeft.Length);
            WriteInt(os, 10);
            if (((borders & Rectangle.LEFT_BORDER) == Rectangle.LEFT_BORDER) && (borderWidth > 0)) {
                WriteBorder(os, rowBorderLeft);
            }
            if (((borders & Rectangle.TOP_BORDER) == Rectangle.TOP_BORDER) && (borderWidth > 0)) {
                WriteBorder(os, rowBorderTop);
            }
            if (((borders & Rectangle.BOTTOM_BORDER) == Rectangle.BOTTOM_BORDER) && (borderWidth > 0)) {
                WriteBorder(os, rowBorderBottom);
            }
            if (((borders & Rectangle.RIGHT_BORDER) == Rectangle.RIGHT_BORDER) && (borderWidth > 0)) {
                WriteBorder(os, rowBorderRight);
            }
            if (((borders & Rectangle.BOX) == Rectangle.BOX) && (borderWidth > 0)) {
                WriteBorder(os, rowBorderInlineHorizontal);
                WriteBorder(os, rowBorderInlineVertical);
            }

            if (cellspacing > 0) {
                os.WriteByte(RtfWriter.escape);
                os.Write(rowSpacingLeft, 0, rowSpacingLeft.Length);
                WriteInt(os, cellspacing / 2);
                os.WriteByte(RtfWriter.escape);
                os.Write(rowSpacingLeftStyle, 0, rowSpacingLeftStyle.Length);
                os.WriteByte(RtfWriter.escape);
                os.Write(rowSpacingTop, 0, rowSpacingTop.Length);
                WriteInt(os, cellspacing / 2);
                os.WriteByte(RtfWriter.escape);
                os.Write(rowSpacingTopStyle, 0, rowSpacingTopStyle.Length);
                os.WriteByte(RtfWriter.escape);
                os.Write(rowSpacingBottom, 0, rowSpacingBottom.Length);
                WriteInt(os, cellspacing / 2);
                os.WriteByte(RtfWriter.escape);
                os.Write(rowSpacingBottomStyle, 0, rowSpacingBottomStyle.Length);
                os.WriteByte(RtfWriter.escape);
                os.Write(rowSpacingRight, 0, rowSpacingRight.Length);
                WriteInt(os, cellspacing / 2);
                os.WriteByte(RtfWriter.escape);
                os.Write(rowSpacingRightStyle, 0, rowSpacingRightStyle.Length);
            }
            os.WriteByte(RtfWriter.escape);
            os.Write(rowPaddingLeft, 0, rowPaddingLeft.Length);
            WriteInt(os, cellpadding / 2);
            os.WriteByte(RtfWriter.escape);
            os.Write(rowPaddingRight, 0, rowPaddingRight.Length);
            WriteInt(os, cellpadding / 2);
            os.WriteByte(RtfWriter.escape);
            os.Write(rowPaddingLeftStyle, 0, rowPaddingLeftStyle.Length);
            os.WriteByte(RtfWriter.escape);
            os.Write(rowPaddingRightStyle, 0, rowPaddingRightStyle.Length);
            os.WriteByte((byte) '\n');

            foreach (RtfCell cell in cells)
                cell.WriteCellSettings(os);
            os.WriteByte(RtfWriter.escape);
            byte[] t = DocWriter.GetISOBytes("intbl");
            os.Write(t, 0, t.Length);

            foreach (RtfCell cell in cells)
                cell.WriteCellContent(os);
            os.WriteByte(RtfWriter.delimiter);
            os.WriteByte(RtfWriter.escape);
            os.Write(rowEnd, 0, rowEnd.Length);
            return true;
        }


        private void WriteBorder(MemoryStream os, byte[] borderType) {
            // horizontal and vertical, top, left, bottom, right
            os.WriteByte(RtfWriter.escape);
            os.Write(borderType, 0, borderType.Length);
            // line style
            os.WriteByte(RtfWriter.escape);
            os.Write(RtfRow.tableBorder, 0, RtfRow.tableBorder.Length);
            // borderwidth
            os.WriteByte(RtfWriter.escape);
            os.Write(RtfRow.tableBorderWidth, 0, RtfRow.tableBorderWidth.Length);
            WriteInt(os, (int) (borderWidth * RtfWriter.TWIPSFACTOR));
            // border color
            os.WriteByte(RtfWriter.escape);
            os.Write(RtfRow.tableBorderColor, 0, RtfRow.tableBorderColor.Length);
            if (borderColor == null) {
                WriteInt(os, writer.AddColor(new Color(0, 0, 0)));
            } else {
                WriteInt(os, writer.AddColor(borderColor));
            }
            os.WriteByte((byte) '\n');
        }


        /**
        * <code>RtfTable</code>s call this method from their own SetMerge() to
        * specify that a certain other cell is to be merged with it.
        *
        * @param x The column position of the cell to be merged
        * @param mergeType The merge type specifies the kind of merge to be applied
        * (MERGE_HORIZ_PREV, MERGE_VERT_PREV, MERGE_BOTH_PREV)
        * @param mergeCell The <code>RtfCell</code> that the cell at x and y is to
        * be merged with
        */
        public void SetMerge(int x, int mergeType, RtfCell mergeCell) {
            RtfCell cell = (RtfCell) cells[x];
            cell.SetMerge(mergeType, mergeCell);
        }

        /*
        * Write an Integer to the Outputstream.
        *
        * @param out The <code>Stream</code> to be written to.
        * @param i The int to be written.
        */
        private void WriteInt(MemoryStream outp, int i) {
            byte[] t = DocWriter.GetISOBytes(i.ToString());
            outp.Write(t, 0, t.Length);
        }
    }
}