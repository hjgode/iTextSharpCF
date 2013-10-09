using System;
using System.Collections;
using System.util;

using iTextSharp.text.html;
using iTextSharp.text.pdf;

/*
 * $Id: Table.cs,v 1.13 2007/01/24 17:42:17 psoares33 Exp $
 * $Name:  $
 *
 * Copyright 1999, 2000, 2001, 2002 by Bruno Lowagie.
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
 * LGPL license (the "GNU LIBRARY GENERAL PUBLIC LICENSE"), in which case the
 * provisions of LGPL are applicable instead of those above.  If you wish to
 * allow use of your version of this file only under the terms of the LGPL
 * License and not to allow others to use your version of this file under
 * the MPL, indicate your decision by deleting the provisions above and
 * replace them with the notice and other provisions required by the LGPL.
 * If you do not delete the provisions above, a recipient may use your version
 * of this file under either the MPL or the GNU LIBRARY GENERAL PUBLIC LICENSE
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the MPL as stated above or under the terms of the GNU
 * Library General Public License as published by the Free Software Foundation;
 * either version 2 of the License, or any later version.
 *
 * This library is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU LIBRARY GENERAL PUBLIC LICENSE for more
 * details.
 *
 * If you didn't download this code from the following link, you should check if
 * you aren't using an obsolete version:
 * http://www.lowagie.com/iText/
 *
 * Some methods in this class were contributed by Geert Poels, Kris Jespers and
 * Steve Ogryzek. Check the CVS repository.
 */

namespace iTextSharp.text {
    /// <summary>
    /// A Table is a Rectangle that contains Cells,
    /// ordered in some kind of matrix.
    /// </summary>
    /// <remarks>
    /// Tables that span multiple pages are cut into different parts automatically.
    /// If you want a table header to be repeated on every page, you may not forget to
    /// mark the end of the header section by using the method EndHeaders().
    /// <P/>
    /// The matrix of a table is not necessarily an m x n-matrix. It can contain holes
    /// or cells that are bigger than the unit. Believe me or not, but it took some serious
    /// thinking to make this as userfriendly as possible. I hope you wil find the result
    /// quite simple (I love simple solutions, especially for complex problems).
    /// </remarks>
    /// <example>
    /// <code>
    /// // Remark: You MUST know the number of columns when constructing a Table.
    /// //         The number of rows is not important.
    /// <STRONG>Table table = new Table(3);</STRONG>
    /// <STRONG>table.SetBorderWidth(1);</STRONG>
    /// <STRONG>table.SetBorderColor(new Color(0, 0, 255));</STRONG>
    /// <STRONG>table.SetPadding(5);</STRONG>
    /// <STRONG>table.SetSpacing(5);</STRONG>
    /// Cell cell = new Cell("header");
    /// cell.SetHeader(true);
    /// cell.SetColspan(3);
    /// <STRONG>table.AddCell(cell);</STRONG>
    /// <STRONG>table.EndHeaders();</STRONG>
    /// cell = new Cell("example cell with colspan 1 and rowspan 2");
    /// cell.SetRowspan(2);
    /// cell.SetBorderColor(new Color(255, 0, 0));
    /// <STRONG>table.AddCell(cell);</STRONG>
    /// <STRONG>table.AddCell("1.1");</STRONG>
    /// <STRONG>table.AddCell("2.1");</STRONG>
    /// <STRONG>table.AddCell("1.2");</STRONG>
    /// <STRONG>table.AddCell("2.2");</STRONG>
    /// <STRONG>table.AddCell("cell test1");</STRONG>
    /// cell = new Cell("big cell");
    /// cell.SetRowspan(2);
    /// cell.SetColspan(2);
    /// <STRONG>table.AddCell(cell);</STRONG>
    /// <STRONG>table.AddCell("cell test2");</STRONG>
    /// </code>
    /// 
    /// The result of this code is a table:
    ///     <TABLE ALIGN="Center" BORDER="1" BORDERCOLOR="#0000ff" CELLPADDING="5" CELLSPACING="5">
    ///            <TR ALIGN="Left" VALIGN="Left">
    ///                    <TH ALIGN="Left" COLSPAN="3" VALIGN="Left">
    ///                            header
    ///                    </TH>
    ///            </TR>
    ///            <TR ALIGN="Left" VALIGN="Left">
    ///                    <TD ALIGN="Left" BORDERCOLOR="#ff0000" ROWSPAN="2" VALIGN="Left">
    ///                            example cell with colspan 1 and rowspan 2
    ///                    </TD>
    ///                    <TD ALIGN="Left" VALIGN="Left">
    ///                            1.1
    ///                    </TD>
    ///                    <TD ALIGN="Left" VALIGN="Left">
    ///                            2.1
    ///                    </TD>
    ///            </TR>
    ///            <TR ALIGN="Left" VALIGN="Left">
    ///                    <TD ALIGN="Left" VALIGN="Left">
    ///                            1.2
    ///                    </TD>
    ///                    <TD ALIGN="Left" VALIGN="Left">
    ///                            2.2
    ///                    </TD>
    ///            </TR>
    ///            <TR ALIGN="Left" VALIGN="Left">
    ///                    <TD ALIGN="Left" VALIGN="Left">
    ///                            cell test1
    ///                    </TD>
    ///                    <TD ALIGN="Left" COLSPAN="2" ROWSPAN="2" VALIGN="Left">
    ///                            big cell
    ///                    </TD>
    ///            </TR>
    ///            <TR ALIGN="Left" VALIGN="Left">
    ///                    <TD ALIGN="Left" VALIGN="Left">
    ///                            cell test2
    ///                    </TD>
    ///            </TR>
    ///    </TABLE>
    /// </example>
    /// <seealso cref="T:iTextSharp.text.Rectangle"/>
    /// <seealso cref="T:iTextSharp.text.Element"/>
    /// <seealso cref="T:iTextSharp.text.Row"/>
    /// <seealso cref="T:iTextSharp.text.Cell"/>
    public class Table : Rectangle, IElement {

        // membervariables

        // these variables contain the data of the table

        ///<summary> This is the number of columns in the Table. </summary>
        private int columns;

        // this is the current Position in the table
        private System.Drawing.Point curPosition = new System.Drawing.Point(0, 0);

        ///<summary> This is the list of Rows. </summary>
        private ArrayList rows = new ArrayList();

        // these variables contain the layout of the table

        ///<summary> This Empty Cell contains the DEFAULT layout of each Cell added with the method AddCell(string content). </summary>
        private Cell defaultLayout = new Cell(true);

        ///<summary> This is the number of the last row of the table headers. </summary>
        private int lastHeaderRow = -1;

        ///<summary> This is the horizontal Element. </summary>
        private int alignment = Element.ALIGN_CENTER;

        ///<summary> This is cellpadding. </summary>
        private float cellpadding;

        ///<summary> This is cellspacing. </summary>
        private float cellspacing;

        ///<summary> This is the width of the table (in percent of the available space). </summary>
        private float widthPercentage = 80;

        // member variable added by Evelyne De Cordier
        ///<summary> This is the width of the table (in pixels). </summary>
        private string absWidth = "";

        ///<summary> This is an array containing the widths (in percentages) of every column. </summary>
        private float[] widths;

        ///<summary> Boolean to track errors (some checks will be performed) </summary>
        bool mDebug = false;

        ///<summary> bool to track if a table was inserted (to avoid unnecessary computations afterwards) </summary>
        bool mTableInserted = false;

        /// <summary>
        /// Boolean to automatically fill empty cells before a table is rendered
        /// (takes CPU so may be set to false in case of certainty)
        /// </summary>
        bool mAutoFillEmptyCells = false;

        ///<summary> If true this table may not be split over two pages. </summary>
        bool tableFitsPage = false;

        ///<summary> If true cells may not be split over two pages. </summary>
        bool cellsFitPage = false;

        ///<summary> This is the offset of the table. </summary>
        float offset = float.NaN;

        ///<summary> contains the attributes that are added to each odd (or even) row </summary>
        protected Hashtable alternatingRowAttributes = null;

        /** if you want to generate tables the old way, set this value to false. */
        protected bool convert2pdfptable = false;

        // constructors

        /// <summary>
        /// Constructs a Table with a certain number of columns.
        /// </summary>
        /// <param name="columns">The number of columns in the table</param>
        /// <overloads>
        /// Has three overloads
        /// </overloads>
        public Table(int columns) : this(columns, 1) {}

        /// <summary>
        /// Constructs a Table with a certain number of columns
        /// and a certain number of Rows.
        /// </summary>
        /// <param name="columns">The number of columns in the table</param>
        /// <param name="rows">The number of rows</param>
        /// <overloads>
        /// Has three overloads
        /// </overloads>
        public Table(int columns, int rows) : base(0, 0, 0, 0) {
            Border = BOX;
            BorderWidth = 1;
            defaultLayout.Border = BOX;

            // a table should have at least 1 column
            if (columns <= 0) {
                throw new BadElementException("A table should have at least 1 column.");
            }
            this.columns = columns;

            // a certain number of rows are created
            for (int i = 0; i < rows; i++) {
                this.rows.Add(new Row(columns));
            }
            curPosition = new System.Drawing.Point(0, 0);

            // the DEFAULT widths are calculated
            widths = new float[columns];
            float width = 100f / columns;
            for (int i = 0; i < columns; i++) {
                widths[i] = width;
            }
        }

        /// <summary>
        /// Returns a Table that has been constructed taking in account
        /// the value of some <VAR>attributes</VAR>.
        /// </summary>
        /// <param name="attributes">some attributes</param>
        /// <overloads>
        /// Has three overloads
        /// </overloads>
        public Table(Properties attributes) : base(0, 0, 0, 0) {
            Border = BOX;
            BorderWidth = 1;
            defaultLayout.Border = BOX;

            string value = attributes.Remove(ElementTags.COLUMNS);
            if (value == null) {
                columns = 1;
            }
            else {
                columns = int.Parse(value);
                if (columns <= 0) {
                    columns = 1;
                }
            }

            rows.Add(new Row(columns));
            curPosition.X = 0;

            if ((value = attributes.Remove(ElementTags.OFFSET)) != null) {
                Offset = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            if ((value = attributes.Remove(ElementTags.LASTHEADERROW)) != null) {
                LastHeaderRow = int.Parse(value);
            }
            if ((value = attributes.Remove(ElementTags.ALIGN)) != null) {
                SetAlignment(value);
            }
            if ((value = attributes.Remove(ElementTags.CELLSPACING)) != null) {
                Spacing = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            if ((value = attributes.Remove(ElementTags.CELLPADDING)) != null) {
                Padding = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            if ((value = attributes.Remove(ElementTags.OFFSET)) != null) {
                Offset = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            if ((value = attributes.Remove(ElementTags.WIDTH)) != null) {
                if (value.EndsWith("%"))
                    this.WidthPercentage = float.Parse(value.Substring(0, value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
                else
                    AbsWidth = value;
            }
            widths = new float[columns];
            for (int i = 0; i < columns; i++) {
                widths[i] = 0;
            }
            if ((value = attributes.Remove(ElementTags.WIDTHS)) != null) {
                StringTokenizer widthTokens = new StringTokenizer(value, ";");
                int i = 0;
                while (widthTokens.HasMoreTokens()) {
                    value = widthTokens.NextToken();
                    widths[i] = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
                    i++;
                }
                columns = i;
            }
            if ((value = attributes.Remove(ElementTags.TABLEFITSPAGE)) != null) {
                tableFitsPage = bool.Parse(value);
            }
            if ((value = attributes.Remove(ElementTags.CELLSFITPAGE)) != null) {
                cellsFitPage = bool.Parse(value);
            }
            if ((value = attributes.Remove(ElementTags.CONVERT2PDFP)) != null) {
                convert2pdfptable = bool.Parse(value);
            }
            if ((value = attributes.Remove(ElementTags.BORDERWIDTH)) != null) {
                BorderWidth = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            int border = 0;
            if ((value = attributes.Remove(ElementTags.LEFT)) != null) {
                if (bool.Parse(value)) border |= Rectangle.LEFT_BORDER;
            }
            if ((value = attributes.Remove(ElementTags.RIGHT)) != null) {
                if (bool.Parse(value)) border |= Rectangle.RIGHT_BORDER;
            }
            if ((value = attributes.Remove(ElementTags.TOP)) != null) {
                if (bool.Parse(value)) border |= Rectangle.TOP_BORDER;
            }
            if ((value = attributes.Remove(ElementTags.BOTTOM)) != null) {
                if (bool.Parse(value)) border |= Rectangle.BOTTOM_BORDER;
            }
            Border = border;
            string r = attributes.Remove(ElementTags.RED);
            string g = attributes.Remove(ElementTags.GREEN);
            string b = attributes.Remove(ElementTags.BLUE);
            if (r != null || g != null || b != null) {
                int red = 0;
                int green = 0;
                int blue = 0;
                if (r != null) red = int.Parse(r);
                if (g != null) green = int.Parse(g);
                if (b != null) blue = int.Parse(b);
                BorderColor = new Color(red, green, blue);
            }
            else if ((value = attributes[ElementTags.BORDERCOLOR]) != null) {
                BorderColor = Markup.DecodeColor(value);
            }
            r = attributes.Remove(ElementTags.BGRED);
            g = attributes.Remove(ElementTags.BGGREEN);
            b = attributes.Remove(ElementTags.BGBLUE);
            if (r != null || g != null || b != null) {
                int red = 0;
                int green = 0;
                int blue = 0;
                if (r != null) red = int.Parse(r);
                if (g != null) green = int.Parse(g);
                if (b != null) blue = int.Parse(b);
                BackgroundColor = new Color(red, green, blue);
            }
            else if ((value = attributes.Remove(ElementTags.BACKGROUNDCOLOR)) != null) {
                BackgroundColor = Markup.DecodeColor(value);
            }
            if ((value = attributes.Remove(ElementTags.GRAYFILL)) != null) {
                GrayFill = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
        }

        // implementation of the Element-methods

        /// <summary>
        /// Processes the element by adding it (or the different parts) to an
        /// IElementListener.
        /// </summary>
        /// <param name="listener">an IElementListener</param>
        /// <returns>true if the element was processed successfully</returns>
        public override bool Process(IElementListener listener) {
            try {
                return listener.Add(this);
            }
            catch (DocumentException de) {
                de.GetType();
                return false;
            }
        }

        /// <summary>
        /// Performs extra checks when executing table code (currently only when cells are added).
        /// </summary>
        /// <value>new value</value>
        public bool Debug {
            set {
                mDebug = value;
            }
        }

        /**
        * Sets the default layout of the Table to
        * the provided Cell
        * @param value a cell with all the defaults
        */
        public Cell DefaultLayout {
            set {
                defaultLayout = value;
            }
        }

        /// <summary>
        /// Enables/disables automatic insertion of empty cells before table is rendered. (default = false)
        /// </summary>
        /// <remarks>
        /// As some people may want to create a table, fill only a couple of the cells and don't bother with
        /// investigating which empty ones need to be added, this default behaviour may be very welcome.
        /// Disabling is recommended to increase speed. (empty cells should be added through extra code then)
        /// </remarks>
        /// <value>enable/disable autofill</value>
        public bool AutoFillEmptyCells {
            set {
                mAutoFillEmptyCells = value;
            }
        }

        /// <summary>
        /// Allows you to control when a page break occurs.
        /// </summary>
        /// <remarks>
        /// When a table doesn't fit a page, it is split in two parts.
        /// If you want to avoid this, you should set the <VAR>tableFitsPage</VAR> value to true.
        /// </remarks>
        /// <value>a value</value>
        public bool TableFitsPage {
            set {
                this.tableFitsPage = value;
                if (value) CellsFitPage = true;
            }
        }

        /// <summary>
        /// Allows you to control when a page break occurs.
        /// </summary>
        /// <remarks>
        /// When a cell doesn't fit a page, it is split in two parts.
        /// If you want to avoid this, you should set the <VAR>cellsFitPage</VAR> value to true.
        /// </remarks>
        /// <value>a value</value>
        public bool CellsFitPage {
            set {
                this.cellsFitPage = value;
            }
        }

        /// <summary>
        /// Checks if this Table has to fit a page.
        /// </summary>
        /// <returns>true if the table may not be split</returns>
        public bool HasToFitPageTable() {
            return tableFitsPage;
        }

        /// <summary>
        /// Checks if the cells of this Table have to fit a page.
        /// </summary>
        /// <returns>true if the cells may not be split</returns>
        public bool HasToFitPageCells() {
            return cellsFitPage;
        }

        /// <summary>
        /// Get/set the offset of this table.
        /// </summary>
        /// <value>the space between this table and the previous element.</value>
        public float Offset {
            get {
                return offset;
            }

            set {
                this.offset = value;
            }
        }

        /// <summary>
        /// Gets the type of the text element.
        /// </summary>
        /// <value>a type</value>
        public override int Type {
            get {
                return Element.TABLE;
            }
        }

        /**
         * Gets all the chunks in this element.
         *
         * @return  an ArrayList
         */

        //    public ArrayList Chunks {
        //      get {
        //          return new ArrayList();
        //      }
        //    }

        // methods to add content to the table

        /// <summary>
        /// Adds a Cell to the Table at a certain row and column.
        /// </summary>
        /// <param name="aCell">The Cell to add</param>
        /// <param name="row">The row where the Cell will be added</param>
        /// <param name="column">The column where the Cell will be added</param>
        public void AddCell(Cell aCell, int row, int column) {
            AddCell(aCell, new System.Drawing.Point(row,column));
        }

        /// <summary>
        /// Adds a Cell to the Table at a certain location.
        /// </summary>
        /// <param name="aCell">The Cell to add</param>
        /// <param name="aLocation">The location where the Cell will be added</param>
        public void AddCell(Cell aCell, object aLocation) {
            System.Drawing.Point p;
            if (aCell == null) throw new Exception("addCell - cell has null-value");
            if (aLocation == null)
                throw new Exception("addCell - point has null-value");
            else
                p = (System.Drawing.Point)aLocation;

            if (aCell.IsTable()) {
                IEnumerator i = aCell.Elements.GetEnumerator();
                i.MoveNext();
                InsertTable((Table)i.Current, p);
            }
            if (mDebug == true) {
                if (p.X < 0) throw new BadElementException("row coordinate of location must be >= 0");
                if ((p.Y <= 0) && (p.Y > columns)) throw new BadElementException("column coordinate of location must be >= 0 and < nr of columns");
                if (!IsValidLocation(aCell, p)) throw new BadElementException("Adding a cell at the location (" + p.X + "," + p.Y + ") with a colspan of " + aCell.Colspan + " and a rowspan of " + aCell.Rowspan + " is illegal (beyond boundaries/overlapping).");
            }
            if (aCell.Border == UNDEFINED) aCell.Border = defaultLayout.Border;
            aCell.Fill();
            PlaceCell(rows, aCell, p);
            CurrentLocationToNextValidPosition = p;
        }


        /// <summary>
        /// Adds a Cell to the Table.
        /// </summary>
        /// <param name="cell">a Cell</param>
        public void AddCell(Cell cell) {
            AddCell(cell, curPosition);
        }

        /// <summary>
        /// Adds a Cell to the Table.
        /// </summary>
        /// <remarks>
        /// This is a shortcut for AddCell(Cell cell).
        /// The Phrase will be converted to a Cell.
        /// </remarks>
        /// <param name="content">a Phrase</param>
        public void AddCell(Phrase content) {
            AddCell(content, curPosition);
        }

        /// <summary>
        /// Adds a Cell to the Table.
        /// </summary>
        /// <param name="content">a Phrase</param>
        /// <param name="location">a System.Drawing.Point</param>
        public void AddCell(Phrase content, System.Drawing.Point location) {
            Cell cell = new Cell(content);
            cell.Border = defaultLayout.Border;
            cell.BorderWidth = defaultLayout.BorderWidth;
            cell.BorderColor = defaultLayout.BorderColor;
            cell.BackgroundColor = defaultLayout.BackgroundColor;
            cell.HorizontalAlignment = defaultLayout.HorizontalAlignment;
            cell.VerticalAlignment = defaultLayout.VerticalAlignment;
            cell.Colspan = defaultLayout.Colspan;
            cell.Rowspan = defaultLayout.Rowspan;
            AddCell(cell, location);
        }

        /// <summary>
        /// Adds a Cell to the Table.
        /// </summary>
        /// <remarks>
        /// This is a shortcut for AddCell(Cell cell).
        /// The string will be converted to a Cell.
        /// </remarks>
        /// <param name="content">a string</param>
        public void AddCell(string content) {
            AddCell(new Phrase(content), curPosition);
        }

        /// <summary>
        /// Adds a Cell to the Table.
        /// </summary>
        /// <remarks>
        /// This is a shortcut for AddCell(Cell cell, System.Drawing.Point location).
        /// The string will be converted to a Cell.
        /// </remarks>
        /// <param name="content">a string</param>
        /// <param name="location">a point</param>
        public void AddCell(string content, System.Drawing.Point location) {
            AddCell(new Phrase(content), location);
        }

        /// <summary>
        /// To put a table within the existing table at the current position
        /// generateTable will of course re-arrange the widths of the columns.
        /// </summary>
        /// <param name="aTable">the table you want to insert</param>
        public void InsertTable(Table aTable) {
            if (aTable == null) throw new Exception("insertTable - table has null-value");
            InsertTable(aTable, curPosition);
        }

        /// <summary>
        /// To put a table within the existing table at the given position
        /// generateTable will of course re-arrange the widths of the columns.
        /// </summary>
        /// <param name="aTable">The Table to add</param>
        /// <param name="row">The row where the Cell will be added</param>
        /// <param name="column">The column where the Cell will be added</param>
        public void InsertTable(Table aTable, int row, int column) {
            if (aTable == null) throw new Exception("insertTable - table has null-value");
            InsertTable(aTable, new System.Drawing.Point(row, column));
        }

        /// <summary>
        /// To put a table within the existing table at the given position
        /// generateTable will of course re-arrange the widths of the columns.
        /// </summary>
        /// <param name="aTable">the table you want to insert</param>
        /// <param name="aLocation">a System.Drawing.Point</param>
        public void InsertTable(Table aTable, object aLocation) {
            System.Drawing.Point p;
            if (aTable == null) throw new Exception("insertTable - table has null-value");
            if (aLocation == null) 
                throw new Exception("insertTable - point has null-value");
            else
                p = (System.Drawing.Point)aLocation;

            mTableInserted = true;
            aTable.Complete();
            if (mDebug == true) {
                if (p.Y > columns) Console.Error.WriteLine("insertTable -- wrong Columnposition("+ p.Y + ") of location; max =" + columns);
            }
            int rowCount = p.X + 1 - rows.Count;
            int i = 0;
            if ( rowCount > 0 ) {   //create new rows ?
                for (; i < rowCount; i++) {
                    rows.Add(new Row(columns));
                }
            }

            ((Row) rows[p.X]).SetElement(aTable,p.Y);

            CurrentLocationToNextValidPosition = p;
        }

        /// <summary>
        /// Will fill empty cells with valid blank Cells
        /// </summary>
        public void Complete() {
            if (mTableInserted == true) {
                MergeInsertedTables();  // integrate tables in the table
                mTableInserted = false;
            }
            if (mAutoFillEmptyCells == true) {
                FillEmptyMatrixCells();
            }
            if (alternatingRowAttributes != null) {
                Properties even = new Properties();
                Properties odd = new Properties();
                string name;
                string[] value;
                foreach (object itm in alternatingRowAttributes.Keys) {
                    name = itm.ToString();
                    value = (string[])alternatingRowAttributes[name];
                    even.Add(name, value[0]);
                    odd.Add(name, value[1]);
                }
            }
        }

        /// <summary>
        /// Changes the border in the default layout of the Cells
        /// added with method AddCell(string content).
        /// </summary>
        /// <value>the new border value</value>
        public int DefaultCellBorder {
            set {
                defaultLayout.Border = value;
            }
        }

        /// <summary>
        /// Changes the width of the borders in the default layout of the Cells
        /// added with method AddCell(string content).
        /// </summary>
        /// <value>the new width</value>
        public float DefaultCellBorderWidth {
            set {
                defaultLayout.BorderWidth = value;
            }
        }

        /// <summary>
        /// Changes the bordercolor in the default layout of the Cells
        /// added with method AddCell(string content).
        /// </summary>
        public Color DefaultCellBorderColor {
            set {
                defaultLayout.BorderColor = value;
            }
        }

        /// <summary>
        /// Changes the backgroundcolor in the default layout of the Cells
        /// added with method AddCell(string content).
        /// </summary>
        /// <value>the new color</value>
        public Color DefaultCellBackgroundColor {
            set {
                defaultLayout.BackgroundColor = value;
            }
        }

        /// <summary>
        /// Changes the grayfill in the default layout of the Cells
        /// added with method AddCell(string content).
        /// </summary>
        /// <value>the new value</value>
        public float DefaultCellGrayFill {
            set {
                if (value >= 0 && value <= 1) {
                    defaultLayout.GrayFill = value;
                }
            }
        }

        /// <summary>
        /// Changes the horizontalalignment in the default layout of the Cells
        /// added with method AddCell(string content).
        /// </summary>
        /// <value>the new alignment value</value>
        public int DefaultHorizontalAlignment {
            set {
                defaultLayout.HorizontalAlignment = value;
            }
        }

        /// <summary>
        /// Changes the verticalAlignment in the default layout of the Cells
        /// added with method AddCell(string content).
        /// </summary>
        /// <value>the new alignment value</value>
        public int DefaultVerticalAlignment {
            set {
                defaultLayout.VerticalAlignment = value;
            }
        }

        /// <summary>
        /// Changes the rowspan in the default layout of the Cells
        /// added with method AddCell(string content).
        /// </summary>
        /// <value>the new rowspan value</value>
        public int DefaultRowspan {
            set {
                defaultLayout.Rowspan = value;
            }
        }

        /// <summary>
        /// Changes the colspan in the default layout of the Cells
        /// added with method AddCell(string content).
        /// </summary>
        /// <value>the new colspan value</value>
        public int DefaultColspan {
            set {
                defaultLayout.Colspan = value;
            }
        }

        // methods

        /// <summary>
        /// Sets the unset cell properties to be the table defaults.
        /// </summary>
        /// <param name="aCell">The cell to set to table defaults as necessary.</param>
        private void AssumeTableDefaults(Cell aCell) {

            if (aCell.Border == Rectangle.UNDEFINED) {
                aCell.Border = defaultLayout.Border;
            }
            if (aCell.BorderWidth == Rectangle.UNDEFINED) {
                aCell.BorderWidth = defaultLayout.BorderWidth;
            }
            if (aCell.BorderColor == null) {
                aCell.BorderColor = defaultLayout.BorderColor;
            }
            if (aCell.BackgroundColor == null) {
                aCell.BackgroundColor = defaultLayout.BackgroundColor;
            }
            if (aCell.HorizontalAlignment == Element.ALIGN_UNDEFINED) {
                aCell.HorizontalAlignment = defaultLayout.HorizontalAlignment;
            }
            if (aCell.VerticalAlignment == Element.ALIGN_UNDEFINED) {
                aCell.VerticalAlignment = defaultLayout.VerticalAlignment;
            }
        }

        /// <summary>
        /// Deletes a column in this table.
        /// </summary>
        /// <param name="column">the number of the column that has to be deleted</param>
        public void DeleteColumn(int column) {
            float[] newWidths = new float[--columns];
            System.Array.Copy(widths, 0, newWidths, 0, column);
            System.Array.Copy(widths, column + 1, newWidths, column, columns - column);
            Widths = newWidths;
            System.Array.Copy(widths, 0, newWidths, 0, columns);
            widths = newWidths;
            Row row;
            int size = rows.Count;
            for (int i = 0; i < size; i++) {
                row = (Row) rows[i];
                row.DeleteColumn(column);
                rows[i] = row;
            }
            if (column == columns) {
                curPosition.X++;
                curPosition.Y = 0;
            }
        }

        /// <summary>
        /// Deletes a row.
        /// </summary>
        /// <param name="row">the number of the row to delete</param>
        /// <returns>true if the row was deleted; false if not</returns>
        public bool DeleteRow(int row) {
            if (row < 0 || row >= rows.Count) {
                return false;
            }
            rows.RemoveAt(row);
            curPosition.X--;
            return true;
        }

        /**
        * Deletes all rows in this table.
        * (contributed by dperezcar@fcc.es)
        */
        
        public void DeleteAllRows() {
            rows.Clear();
            rows.Add(new Row(columns));
            curPosition.X = 0;
            curPosition.Y = 0;
            lastHeaderRow = -1;
        }

        /// <summary>
        /// Deletes the last row in this table.
        /// </summary>
        /// <returns>true if the row was deleted; false if not</returns>
        public bool DeleteLastRow() {
            return DeleteRow(rows.Count - 1);
        }

        /// <summary>
        /// Marks the last row of the table headers.
        /// </summary>
        /// <returns>the number of the last row of the table headers</returns>
        public int EndHeaders() {
            /* patch sep 8 2001 Francesco De Milato */
            lastHeaderRow = curPosition.X - 1;
            return lastHeaderRow;
        }

        // methods to set the membervariables

        /// <summary>
        /// Sets the horizontal Element.
        /// </summary>
        /// <value>the new value</value>
        public int LastHeaderRow {
            set {
                lastHeaderRow = value;
            }
            get {
                return lastHeaderRow;
            }
        }

        /// <summary>
        /// Sets the alignment of this paragraph.
        /// </summary>
        /// <param name="alignment">the new alignment as a string</param>
        public void SetAlignment(string alignment) {
            if (Util.EqualsIgnoreCase(alignment, ElementTags.ALIGN_LEFT)) {
                this.alignment = Element.ALIGN_LEFT;
                return;
            }
            if (Util.EqualsIgnoreCase(alignment, ElementTags.ALIGN_RIGHT)) {
                this.alignment = Element.ALIGN_RIGHT;
                return;
            }
            this.alignment = Element.ALIGN_CENTER;
        }

        /// <summary>
        /// Sets the cellpadding.
        /// </summary>
        /// <value>the new value</value>
        public float SpaceInsideCell {
            set {
                cellpadding = value;
            }
        }

        /// <summary>
        /// Sets the cellspacing.
        /// </summary>
        /// <value>the new value</value>
        public float SpaceBetweenCells {
            set {
                cellspacing = value;
            }
        }

        /// <summary>
        /// Sets the cellpadding.
        /// </summary>
        /// <value>the new value</value>
        public float Padding {
            set {
                cellpadding = value;
            }
        }

        /// <summary>
        /// Sets the cellspacing.
        /// </summary>
        /// <value>the new value</value>
        public float Spacing {
            set {
                cellspacing = value;
            }
        }

        /// <summary>
        /// Sets the widths of the different columns (percentages).
        /// </summary>
        /// <remarks>
        /// You can give up relative values of borderwidths.
        /// The sum of these values will be considered 100%.
        /// The values will be recalculated as percentages of this sum.
        /// </remarks>
        /// <example>
        /// <BLOCKQUOTE><PRE>
        /// float[] widths = {2, 1, 1};
        /// <STRONG>table.SetWidths(widths)</STRONG>
        /// </PRE></BLOCKQUOTE>
        /// 
        /// The widths will be: a width of 50% for the first column,
        /// 25% for the second and third column.
        /// </example>
        /// <value>an array with values</value>
        public float[] Widths {
            set {
                if (value.Length != columns) {
                    throw new BadElementException("Wrong number of columns.");
                }

                // The sum of all values is 100%
                float hundredPercent = 0;
                for (int i = 0; i < columns; i++) {
                    hundredPercent += value[i];
                }

                // The different percentages are calculated
                float width;
                this.widths[columns - 1] = 100;
                for (int i = 0; i < columns - 1; i++) {
                    width = (100.0f * value[i]) / hundredPercent;
                    this.widths[i] = width;
                    this.widths[columns - 1] -= width;
                }
            }
        }

        /// <summary>
        /// Sets the widths of the different columns (percentages).
        /// </summary>
        /// <remarks>
        /// You can give up relative values of borderwidths.
        /// The sum of these values will be considered 100%.
        /// The values will be recalculated as percentages of this sum.
        /// </remarks>
        /// <param name="widths">an array with values</param>
        public void SetWidths(int[] widths) {
            float[] tb = new float[widths.Length];
            for (int k = 0; k < widths.Length; ++k)
                tb[k] = widths[k];
            this.Widths = tb;
        }
        // methods to retrieve the membervariables

        /// <summary>
        /// Gets the number of columns.
        /// </summary>
        /// <value>a value</value>
        public int Columns {
            get {
                return columns;
            }
        }

        /// <summary>
        /// Gets the number of rows in this Table.
        /// </summary>
        /// <value>the number of rows in this Table</value>
        public int Size {
            get {
                return rows.Count;
            }
        }

        /// <summary>
        /// Gets the proportional widths of the columns in this Table.
        /// </summary>
        /// <value>the proportional widths of the columns in this Table</value>
        public float[] ProportionalWidths {
            get {
                return widths;
            }
        }

        /// <summary>
        /// Gets an Iterator of all the Rows.
        /// </summary>
        /// <returns>an IEnumerator</returns>
        public IEnumerator GetEnumerator() {
            return rows.GetEnumerator();
        }

        /// <summary>
        /// Get/set the horizontal Element.
        /// </summary>
        /// <value>a value</value>
        public int Alignment{
            get {
                return alignment;
            }

            set {
                alignment = value;
            }
        }

        /// <summary>
        /// Get/set the cellpadding.
        /// </summary>
        /// <value>the cellpadding</value>
        public float Cellpadding {
            get {
                return cellpadding;
            }

            set {
                this.cellpadding = value;
            }
        }

        /// <summary>
        /// Get/set the cellspacing.
        /// </summary>
        /// <value>the cellspacing</value>
        public float Cellspacing {
            get {
                return cellspacing;
            }

            set {
                this.cellspacing = value;
            }
        }

        /// <summary>
        /// Get/set the table width (a percentage).
        /// </summary>
        /// <value>the table width (a percentage)</value>
        public float WidthPercentage {
            get {
                return widthPercentage;
            }

            set {
                this.widthPercentage = value;
            }
        }

        public override float Width {
            set {
                this.widthPercentage = value;
            }
        }

        /// <summary>
        /// Get/set the table width (in pixels).
        /// </summary>
        /// <value>the table width (in pixels)</value>
        public string AbsWidth {
            get {
                return absWidth;
            }

            set {
                this.absWidth = value;
            }
        }

        /// <summary>
        /// Gets the first number of the row that doesn't contain headers.
        /// </summary>
        /// <returns>a rownumber</returns>
        public int FirstDataRow {
            get {
                return lastHeaderRow + 1;
            }
        }

        /// <summary>
        /// Gets the dimension of this table
        /// </summary>
        /// <value>the dimension</value>
        public System.Drawing.Dimension Dimension {
            get {
                return new System.Drawing.Dimension(columns, rows.Count);
            }
        }

        /// <summary>
        /// returns the element at the position row, column
        ///           (Cast to Cell or Table)
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns>an object</returns>
        public object GetElement(int row, int column) {
            return ((Row) rows[row]).GetCell(column);
        }

        /// <summary>
        /// Integrates all added tables and recalculates column widths.
        /// </summary>
        private void MergeInsertedTables() {
            int i=0, j=0;
            float [] lNewWidths = null;
            int [] lDummyWidths = new int[columns];     // to keep track in how many new cols this one will be split
            float[][] lDummyColumnWidths = new float[columns][]; // bugfix Tony Copping
            int [] lDummyHeights = new int[rows.Count]; // to keep track in how many new rows this one will be split
            ArrayList newRows = null;
            bool isTable=false;
            int lTotalRows  = 0, lTotalColumns      = 0;
            int lNewMaxRows = 0, lNewMaxColumns     = 0;
            
            Table lDummyTable = null;
            
            // first we'll add new columns when needed
            // check one column at a time, find maximum needed nr of cols
            // Search internal tables and find one with max columns
            for (j=0; j < columns; j++) {
                lNewMaxColumns = 1; // value to hold in how many columns the current one will be split
                float [] tmpWidths = null;
                for (i=0; i < rows.Count; i++) {
                    if (((Row) rows[i]).GetCell(j) is Table) {
                        isTable=true;
                        lDummyTable = ((Table) ((Row) rows[i]).GetCell(j));
                        if ( tmpWidths == null) {
                            tmpWidths = lDummyTable.widths;
                            lNewMaxColumns=tmpWidths.Length;
                        }
                        else {
                            int cols = lDummyTable.Dimension.width;
                            float [] tmpWidthsN = new float[ cols * tmpWidths.Length];
                            float tpW=0, btW=0, totW=0;
                            int tpI=0, btI=0, totI=0;
                            tpW+=tmpWidths[0];
                            btW+=lDummyTable.widths[0];
                            while ( tpI<tmpWidths.Length && btI<cols) {
                                if ( btW>tpW) {
                                    tmpWidthsN[totI] = tpW-totW;
                                    tpI++;
                                    if (tpI<tmpWidths.Length) {
                                        tpW+=tmpWidths[tpI];
                                    }
                                }
                                else {
                                    tmpWidthsN[totI] = btW-totW;
                                    btI++;
                                    if (Math.Abs(btW - tpW) < 0.0001) {
                                        tpI++;
                                        if (tpI<tmpWidths.Length) {
                                            tpW+=tmpWidths[tpI];
                                        }
                                    }
                                    if (btI<cols) {
                                        btW+=lDummyTable.widths[btI];
                                    }
                                }
                                totW+=tmpWidthsN[totI];
                                totI++;
                            }
                            tmpWidths = new float[totI];
                            Array.Copy(tmpWidthsN, 0, tmpWidths, 0, totI);
                            lNewMaxColumns=totI;
                        }
                    }
                }
                lDummyColumnWidths[j] = tmpWidths;
                lTotalColumns += lNewMaxColumns;
                lDummyWidths [j] = lNewMaxColumns;
            }
            
            // next we'll add new rows when needed
            for (i=0; i < rows.Count; i++) {
                lNewMaxRows = 1;    // holds value in how many rows the current one will be split
                for (j=0; j < columns; j++) {
                    if (((Row) rows[i]).GetCell(j) is Table ) {
                        isTable=true;
                        lDummyTable = (Table) ((Row) rows[i]).GetCell(j);
                        if ( lDummyTable.Dimension.height > lNewMaxRows ) {
                            lNewMaxRows = lDummyTable.Dimension.height;
                        }
                    }
                }
                lTotalRows += lNewMaxRows;
                lDummyHeights [i] = lNewMaxRows;
            }
            
            if ( (lTotalColumns != columns) || (lTotalRows != rows.Count) || isTable)    // NO ADJUSTMENT
            {
                // ** WIDTH
                // set correct width for new columns
                // divide width over new nr of columns
                // Take new max columns of internal table and work out widths for each col
                lNewWidths = new float [lTotalColumns];
                int lDummy = 0;
                for (int tel=0; tel < widths.Length;tel++) {
                    if ( lDummyWidths[tel] != 1) {
                        // divide
                        for (int tel2 = 0; tel2 < lDummyWidths[tel]; tel2++) {
                            lNewWidths[lDummy] = widths[tel] * lDummyColumnWidths[tel][tel2] / 100f; // bugfix Tony Copping
                            lDummy++;
                        }
                    }
                    else {
                        lNewWidths[lDummy] = widths[tel];
                        lDummy++;
                    }
                }
                
                // ** FILL OUR NEW TABLE
                // generate new table
                // set new widths
                // copy old values
                newRows = new ArrayList(lTotalRows);
                for (i = 0; i < lTotalRows; i++) {
                    newRows.Add(new Row(lTotalColumns));
                }
                int lDummyRow = 0, lDummyColumn = 0;        // to remember where we are in the new, larger table
                Object lDummyElement = null;
                for (i=0; i < rows.Count; i++) {
                    lDummyColumn = 0;
                    lNewMaxRows = 1;
                    for (j=0; j < columns; j++) {
                        if (((Row) rows[i]).GetCell(j) is Table )       // copy values from embedded table
                        {
                            lDummyTable = (Table) ((Row) rows[i]).GetCell(j);
                            
                            // Work out where columns in table table correspond to columns in current table
                            int[] colMap = new int[lDummyTable.widths.Length+1];
                            int cb=0, ct=0;
                            
                            for ( ; cb<lDummyTable.widths.Length;cb++) {
                                colMap[cb] = lDummyColumn+ct;
                                
                                float wb;
                                wb = lDummyTable.widths[cb];
                                
                                float wt=0;
                                while ( ct<lDummyWidths[j]) {
                                    wt+=lDummyColumnWidths[j][ct++];
                                    if (Math.Abs(ConvertWidth(wb) - ConvertWidth(wt)) < 0.0001) break;
                                }
                            }
                            colMap[cb] = lDummyColumn+ct;
                            
                            // need to change this to work out how many cols to span
                            for (int k=0; k < lDummyTable.Dimension.height; k++) {
                                for (int l=0; l < lDummyTable.Dimension.width; l++) {
                                    lDummyElement = lDummyTable.GetElement(k,l);
                                    if (lDummyElement != null) {
                                        int col=lDummyColumn+l;
                                        
                                        if (lDummyElement is Cell) {
                                            Cell lDummyC = (Cell)lDummyElement;
                                            // Find col to add cell in and set col span
                                            col = colMap[l];
                                            int ot = colMap[l+lDummyC.Colspan];
                                            
                                            lDummyC.Colspan = ot-col;
                                        }
                                        
                                        ((Row) newRows[k + lDummyRow]).AddElement(lDummyElement,col);  // use addElement to set reserved status ok in row
                                    }
                                }
                            }
                        }
                        else        // copy others values
                        {
                            Object aElement = GetElement(i,j);
                            
                            if (aElement is Cell) {
                                
                                // adjust spans for cell
                                ((Cell) aElement).Rowspan = ((Cell) ((Row) rows[i]).GetCell(j)).Rowspan + lDummyHeights[i] - 1;
                                ((Cell) aElement).Colspan = ((Cell) ((Row) rows[i]).GetCell(j)).Colspan + lDummyWidths[j] - 1;
                                
                                // most likely this cell covers a larger area because of the row/cols splits : define not-to-be-filled cells
                                PlaceCell(newRows,((Cell) aElement), new System.Drawing.Point(lDummyRow,lDummyColumn));
                            }
                        }
                        lDummyColumn += lDummyWidths[j];
                    }
                    lDummyRow += lDummyHeights[i];
                }
                
                // Set our new matrix
                columns     = lTotalColumns;
                rows = newRows;
                this.widths = lNewWidths;
            }
        }

        /// <summary>
        /// Integrates all added tables and recalculates column widths.
        /// </summary>
        private void FillEmptyMatrixCells() {
            for (int i=0; i < rows.Count; i++) {
                for (int j=0; j < columns; j++) {
                    if ( ((Row) rows[i]).IsReserved(j) == false) {
                        AddCell(defaultLayout, new System.Drawing.Point(i, j));
                    }
                }
            }
        }

        /// <summary>
        /// check if Cell 'fits' the table.
        /// </summary>
        /// <remarks>
        /// <UL><LI/>rowspan/colspan not beyond borders
        ///     <LI/>spanned cell don't overlap existing cells</UL>
        /// </remarks>
        /// <param name="aCell">the cell that has to be checked</param>
        /// <param name="aLocation">the location where the cell has to be placed</param>
        /// <returns></returns>
        private bool IsValidLocation(Cell aCell, System.Drawing.Point aLocation) {
            // rowspan not beyond last column
            if ( aLocation.X < rows.Count ) {        // if false : new location is already at new, not-yet-created area so no check
                if ((aLocation.Y + aCell.Colspan) > columns) {
                    return false;
                }

                int difx = ((rows.Count - aLocation.X) >  aCell.Rowspan) ? aCell.Rowspan : rows.Count - aLocation.X;
                int dify = ((columns - aLocation.Y) >  aCell.Colspan) ? aCell.Colspan : columns - aLocation.Y;
                // no other content at cells targetted by rowspan/colspan
                for (int i=aLocation.X; i < (aLocation.X + difx); i++) {
                    for (int j=aLocation.Y; j < (aLocation.Y + dify); j++) {
                        if ( ((Row) rows[i]).IsReserved(j) == true ) {
                            return false;
                        }
                    }
                }
            }
            else {
                if ((aLocation.Y + aCell.Colspan) > columns) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Inserts a Cell in a cell-array and reserves cells defined by row-/colspan.
        /// </summary>
        /// <param name="someRows">some rows</param>
        /// <param name="aCell">the cell that has to be inserted</param>
        /// <param name="aPosition">the position where the cell has to be placed</param>
        private void PlaceCell(ArrayList someRows, Cell aCell, System.Drawing.Point aPosition) {
            int i;
            Row row = null;
            int lColumns = ((Row) someRows[0]).Columns;
            int rowCount = aPosition.X + aCell.Rowspan - someRows.Count;
            AssumeTableDefaults(aCell);
            if ( (aPosition.X + aCell.Rowspan) > someRows.Count ) {        //create new rows ?
                for (i = 0; i < rowCount; i++) {
                    row = new Row(lColumns);
                    someRows.Add(row);
                }
            }

            // reserve cell in rows below
            for (i = aPosition.X + 1; i < (aPosition.X  + aCell.Rowspan); i++) {
                if ( !((Row) someRows[i]).Reserve(aPosition.Y, aCell.Colspan)) {

                    // should be impossible to come here :-)
                    throw new Exception("addCell - error in reserve");
                }
            }
            row = (Row) someRows[aPosition.X];
            row.AddElement(aCell, aPosition.Y);

        }

        /// <summary>
        /// Gives you the posibility to add columns.
        /// </summary>
        /// <param name="aColumns">the number of columns to add</param>
        public void AddColumns(int aColumns) {
            ArrayList newRows = new ArrayList(rows.Count);

            int newColumns = columns + aColumns;
            Row row;
            for (int i = 0; i < rows.Count; i++) {
                row = new Row(newColumns);
                for (int j = 0; j < columns; j++) {
                    row.SetElement(((Row) rows[i]).GetCell(j) ,j);
                }
                for (int j = columns; j < newColumns && i < curPosition.X; j++) {
                    row.SetElement(defaultLayout, j);
                }
                newRows.Add(row);
            }

            // applied 1 column-fix; last column needs to have a width of 0
            float [] newWidths = new float[newColumns];
            System.Array.Copy(widths, 0, newWidths, 0, columns);
            for (int j = columns; j < newColumns ; j++) {
                newWidths[j] = 0;
            }
            columns = newColumns;
            widths = newWidths;
            rows = newRows;
        }

        /// <summary>
        /// Gets an array with the positions of the borders between every column.
        /// </summary>
        /// <remarks>
        /// This method translates the widths expressed in percentages into the
        /// x-coordinate of the borders of the columns on a real document.
        /// </remarks>
        /// <param name="left">this is the position of the first border at the left (cellpadding not included)</param>
        /// <param name="totalWidth">
        /// this is the space between the first border at the left
        /// and the last border at the right (cellpadding not included)
        /// </param>
        /// <returns>an array with borderpositions</returns>
        public float[] GetWidths(float left, float totalWidth) {
            // for x columns, there are x+1 borders
            float[] w = new float[columns + 1];
            // the border at the left is calculated
            switch (alignment) {
                case Element.ALIGN_LEFT:
                    w[0] = left;
                    break;
                case Element.ALIGN_RIGHT:
                    w[0] = left + (totalWidth * (100 - widthPercentage)) / 100;
                    break;
                case Element.ALIGN_CENTER:
                default:
                    w[0] = left + (totalWidth * (100 - widthPercentage)) / 200;
                    break;
            }
            // the total available width is changed
            totalWidth = (totalWidth * widthPercentage) / 100;
            // the inner borders are calculated
            for (int i = 1; i < columns; i++) {
                w[i] = w[i - 1] + (widths[i - 1] * totalWidth / 100);
            }
            // the border at the right is calculated
            w[columns] = w[0] + totalWidth;
            return w;
        }

        /// <summary>
        /// Sets current col/row to Valid(empty) pos after addCell/Table
        /// </summary>
        /// <value>a System.Drawing.Point</value>
        private System.Drawing.Point CurrentLocationToNextValidPosition {
            set {
                // set latest location to next valid position
                int i, j;
                i = value.X;
                j = value.Y;
                do {
                    if ( (j + 1)  == columns ) {    // goto next row
                        i++;
                        j = 0;
                    }
                    else {
                        j++;
                    }
                }
                while (
                    (i < rows.Count) && (j < columns) && (((Row) rows[i]).IsReserved(j) == true)
                    );
                curPosition = new System.Drawing.Point(i, j);
            }
        }

        /// <summary>
        /// Checks if a given tag corresponds with this object.
        /// </summary>
        /// <param name="tag">the given tag</param>
        /// <returns>true if the tag corresponds</returns>
        public static bool IsTag(string tag) {
            return ElementTags.TABLE.Equals(tag);
        }

        /// <summary>
        /// Allows clients to set up alternating attributes for each Row in the Table.
        /// </summary>
        /// <remarks>
        /// This code was contributed by Matt Benson.
        /// </remarks>
        /// <param name="name">the name of the attribute</param>
        /// <param name="value0">the value of the attribute for even rows</param>
        /// <param name="value1">the value of the attribute for odd rows</param>
        public void SetAlternatingRowAttribute(string name, string value0, string value1) {
            if (value0 == null || value1 == null) {
                throw new Exception("MarkupTable#setAlternatingRowAttribute(): null values are not permitted.");
            }
            if (alternatingRowAttributes == null) alternatingRowAttributes = new Hashtable();

            // we could always use new Arrays but this is big enough
            string[] value = (string[])(alternatingRowAttributes[name]);
            value = (value == null) ? new string[2] : value;
            value[0] = value0;
            value[1] = value1;
            alternatingRowAttributes.Add(name, value);
        }

        private void ErrorDimensions() {
            throw new Exception("Dimensions of a Table can't be calculated. See the FAQ.");
        }

        /**
        * Returns the next row 0-based index where a new cell would be added.
        * (contributed by dperezcar@fcc.es)
        * @return x coordinate for the next row
        */
        public int NextRow {
            get {
                return curPosition.X;
            }
        }
        
        /**
        * Returns the next column 0-based index where a new cell would be added.
        * (contributed by dperezcar@fcc.es)
        * @return y coordinate for the next row
        */
        public int NextColumn {
            get {
                return curPosition.Y;
            }
        }
        
        private static double ConvertWidth( double val) {
            return Math.Round(val, 2);
        }
    
        public override float Bottom {
            get {
                ErrorDimensions();
                return 0;
            }
            set {
                ErrorDimensions();
            }
        }
    
        public override float Left {
            get {
                ErrorDimensions();
                return 0;
            }
            set {
                ErrorDimensions();
            }
        }
    
        public override float Right {
            get {
                ErrorDimensions();
                return 0;
            }
            set {
                ErrorDimensions();
            }
        }
    
        public override float Top {
            get {
                ErrorDimensions();
                return 0;
            }
            set {
                ErrorDimensions();
            }
        }
    
        public override float GetBottom(float margin) {
            ErrorDimensions();
            return 0;
        }
    
        public override float GetLeft(float margin) {
            ErrorDimensions();
            return 0;
        }
    
        public override float GetRight(float margin) {
            ErrorDimensions();
            return 0;
        }
    
        public override float GetTop(float margin) {
            ErrorDimensions();
            return 0;
        }

        /**
        * Create a PdfPTable based on this Table object.
        * @return a PdfPTable object
        * @throws BadElementException
        */
        public PdfPTable CreatePdfPTable() {
            if (!convert2pdfptable) {
                throw new BadElementException("No error, just an old style table");
            }
            AutoFillEmptyCells = true;
            Complete();
            PdfPTable pdfptable = new PdfPTable(widths);
            pdfptable.TableEvent = SimpleTable.GetDimensionlessInstance(this, cellspacing);
            pdfptable.HeaderRows = lastHeaderRow + 1;
            pdfptable.SplitLate = cellsFitPage;
            pdfptable.KeepTogether = tableFitsPage;
            if (!float.IsNaN(offset)) {
                pdfptable.SpacingBefore = offset;
            }
            pdfptable.HorizontalAlignment = alignment;
            if (absWidth.Length > 0) {
                try {
                    pdfptable.TotalWidth = float.Parse(absWidth, System.Globalization.NumberFormatInfo.InvariantInfo);
                }
                catch  {
                    try {
                        pdfptable.TotalWidth = (float)int.Parse(absWidth);
                    }
                    catch  {
                        pdfptable.WidthPercentage = widthPercentage;
                    }
                }
            }
            else {
                pdfptable.WidthPercentage = widthPercentage;
            }
            foreach (Row row in this) {
                IElement cell;
                PdfPCell pcell;
                for (int i = 0; i < row.Columns; i++) {
                    if ((cell = (IElement)row.GetCell(i)) != null) {
                        if (cell is Table) {
                            pcell = new PdfPCell(((Table)cell).CreatePdfPTable());
                        }
                        else if (cell is Cell) {
                            pcell = ((Cell)cell).CreatePdfPCell();
                            pcell.Padding = cellpadding + cellspacing / 2f;
                            pcell.CellEvent = SimpleCell.GetDimensionlessInstance((Cell)cell, cellspacing);
                        }
                        else {
                            pcell = new PdfPCell();
                        }
                        pdfptable.AddCell(pcell);
                    }
                }
            }
            return pdfptable;
        }
        
        /**
        * If set to true, iText will try to convert the Table to a PdfPTable.
        * @param convert2pdfptable true if you want iText to try to convert the Table to a PdfPTable
        */
        public bool Convert2pdfptable {
            get {
                return convert2pdfptable;
            }
            set {
                this.convert2pdfptable = value;
            }
        }
    }
}
