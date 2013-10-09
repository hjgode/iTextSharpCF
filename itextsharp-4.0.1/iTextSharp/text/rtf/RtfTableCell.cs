using System;
using System.util;
using iTextSharp.text;
/*
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
    * A <code>Cell</code> with extended style attributes
    * 
    * ONLY FOR USE WITH THE RtfWriter NOT with the RtfWriter2.
    */
    public class RtfTableCell : Cell
    {
        /* Table border styles */
        
        /** Table border solid */
        public const int BORDER_UNDEFINED = 0;
        
        /** Table border solid */
        public const int BORDER_SINGLE = 1;
        
        /** Table border double thickness */
        public const int BORDER_DOUBLE_THICK = 2;
        
        /** Table border shadowed */
        public const int BORDER_SHADOWED = 3;
        
        /** Table border dotted */
        public const int BORDER_DOTTED = 4;
        
        /** Table border dashed */
        public const int BORDER_DASHED = 5;
        
        /** Table border hairline */
        public const int BORDER_HAIRLINE = 6;
        
        /** Table border double line */
        public const int BORDER_DOUBLE = 7;
        
        /** Table border dot dash line */
        public const int BORDER_DOT_DASH = 8;
        
        /** Table border dot dot dash line */
        public const int BORDER_DOT_DOT_DASH = 9;
        
        /** Table border triple line */
        public const int BORDER_TRIPLE = 10;

        /** Table border line */
        public const int BORDER_THICK_THIN = 11;
        
        /** Table border line */
        public const int BORDER_THIN_THICK = 12;
        
        /** Table border line */
        public const int BORDER_THIN_THICK_THIN = 13;
        
        /** Table border line */
        public const int BORDER_THICK_THIN_MED = 14;
        
        /** Table border line */
        public const int BORDER_THIN_THICK_MED = 15;
        
        /** Table border line */
        public const int BORDER_THIN_THICK_THIN_MED = 16;
        
        /** Table border line */
        public const int BORDER_THICK_THIN_LARGE = 17;
        
        /** Table border line */
        public const int BORDER_THIN_THICK_LARGE = 18;
        
        /** Table border line */
        public const int BORDER_THIN_THICK_THIN_LARGE = 19;
        
        /** Table border line */
        public const int BORDER_WAVY = 20;
        
        /** Table border line */
        public const int BORDER_DOUBLE_WAVY = 21;
        
        /** Table border line */
        public const int BORDER_STRIPED = 22;
        
        /** Table border line */
        public const int BORDER_EMBOSS = 23;
        
        /** Table border line */
        public const int BORDER_ENGRAVE = 24;
        
        /* Instance variables */
        private float topBorderWidth;
        private float leftBorderWidth;
        private float rightBorderWidth;
        private float bottomBorderWidth;
        private int topBorderStyle = 1;
        private int leftBorderStyle = 1;
        private int rightBorderStyle = 1;
        private int bottomBorderStyle = 1;
        
    /**
    * Constructs an empty <CODE>Cell</CODE> (for internal use only).
    *
    * @param   dummy   a dummy value
    */

        public RtfTableCell(bool dummy) : base(dummy) {
        }
        
    /**
    * Constructs a <CODE>Cell</CODE> with a certain <CODE>Element</CODE>.
    * <P>
    * if the element is a <CODE>ListItem</CODE>, <CODE>Row</CODE> or
    * <CODE>Cell</CODE>, an exception will be thrown.
    *
    * @param   element     the element
    * @throws  BadElementException when the creator was called with a <CODE>ListItem</CODE>, <CODE>Row</CODE> or <CODE>Cell</CODE>
    */
        public RtfTableCell(IElement element) : base(element){
        }
        
    /**
    * Constructs a <CODE>Cell</CODE> with a certain content.
    * <P>
    * The <CODE>String</CODE> will be converted into a <CODE>Paragraph</CODE>.
    *
    * @param   content     a <CODE>String</CODE>
    */
        public RtfTableCell(String content) : base(content) {
        }
        
    /**
    * Returns a <CODE>Cell</CODE> that has been constructed taking in account
    * the value of some <VAR>attributes</VAR>.
    *
    * @param   attributes      Some attributes
    */

        public RtfTableCell(Properties attributes) : base(attributes) {
        }
        
        /**
        * Set all four borders to <code>f</code> width
        *
        * @param f the desired width
        */
        public override float BorderWidth {
            set {
                base.BorderWidth = value;
                topBorderWidth = value;
                leftBorderWidth = value;
                rightBorderWidth = value;
                bottomBorderWidth = value;
            }
        }
        
        /**
        * Set the top border to <code>f</code> width
        *
        * @param f the desired width
        */
        public float TopBorderWidth {
            get {
                return topBorderWidth;
            }
            set {
                topBorderWidth = value;
            }
        }
        
        /**
        * Set the left border to <code>f</code> width
        *
        * @param f the desired width
        */
        public float LeftBorderWidth {
            get {
                return leftBorderWidth;
            }
            set {
                leftBorderWidth = value;
            }
        }
        
        /**
        * Set the right border to <code>f</code> width
        *
        * @param f the desired width
        */
        public float RightBorderWidth {
            get {
                return rightBorderWidth;
            }
            set {
                rightBorderWidth = value;
            }
        }
        
        /**
        * Set the bottom border to <code>f</code> width
        *
        * @param f the desired width
        */
        public float BottomBorderWidth {
            get {
                return bottomBorderWidth;
            }
            set {
                bottomBorderWidth = value;
            }
        }
        
        /**
        * Set all four borders to style defined by <code>style</code>
        *
        * @param style the desired style
        */
        public int BorderStyle {
            set {
                topBorderStyle = value;
                leftBorderStyle = value;
                rightBorderStyle = value;
                bottomBorderStyle = value;
            }
        }
        
        /**
        * Set the top border to style defined by <code>style</code>
        *
        * @param style the desired style
        */
        public int TopBorderStyle {
            get {
                return topBorderStyle;
            }
            set {
                topBorderStyle = value;
            }
        }
        
        /**
        * Set the left border to style defined by <code>style</code>
        *
        * @param style the desired style
        */
        public int LeftBorderStyle {
            get {
                return leftBorderStyle;
            }
            set {
                leftBorderStyle = value;
            }
        }
        
        /**
        * Set the right border to style defined by <code>style</code>
        *
        * @param style the desired style
        */
        public int RightBorderStyle {
            get {
                return rightBorderStyle;
            }
            set {
                rightBorderStyle = value;
            }
        }
        
        /**
        * Set the bottom border to style defined by <code>style</code>
        *
        * @param style the desired style
        */
        public int BottomBorderStyle {
            get {
                return bottomBorderStyle;
            }
            set {
                bottomBorderStyle = value;
            }
        }
        
        /**
        * Get the RTF control word for <code>style</code>
        * @param style a style value
        * @return a byte array corresponding with a style control word
        */
        protected internal static byte[] GetStyleControlWord(int style) {
            switch (style)
            {
                case BORDER_UNDEFINED               : return DocWriter.GetISOBytes("brdrs");
                case BORDER_SINGLE                  : return DocWriter.GetISOBytes("brdrs");
                case BORDER_DOUBLE_THICK            : return DocWriter.GetISOBytes("brdrth");
                case BORDER_SHADOWED                : return DocWriter.GetISOBytes("brdrsh");
                case BORDER_DOTTED                  : return DocWriter.GetISOBytes("brdrdot");
                case BORDER_DASHED                  : return DocWriter.GetISOBytes("brdrdash");
                case BORDER_HAIRLINE                : return DocWriter.GetISOBytes("brdrhair");
                case BORDER_DOUBLE                  : return DocWriter.GetISOBytes("brdrdb");
                case BORDER_DOT_DASH                : return DocWriter.GetISOBytes("brdrdashd");
                case BORDER_DOT_DOT_DASH            : return DocWriter.GetISOBytes("brdrdashdd");
                case BORDER_TRIPLE                  : return DocWriter.GetISOBytes("brdrtriple");
                case BORDER_THICK_THIN              : return DocWriter.GetISOBytes("brdrtnthsg");
                case BORDER_THIN_THICK              : return DocWriter.GetISOBytes("brdrthtnsg");
                case BORDER_THIN_THICK_THIN         : return DocWriter.GetISOBytes("brdrtnthtnsg");
                case BORDER_THICK_THIN_MED          : return DocWriter.GetISOBytes("brdrtnthmg");
                case BORDER_THIN_THICK_MED          : return DocWriter.GetISOBytes("brdrthtnmg");
                case BORDER_THIN_THICK_THIN_MED     : return DocWriter.GetISOBytes("brdrtnthtnmg");
                case BORDER_THICK_THIN_LARGE        : return DocWriter.GetISOBytes("brdrtnthlg");
                case BORDER_THIN_THICK_LARGE        : return DocWriter.GetISOBytes("brdrthtnlg");
                case BORDER_THIN_THICK_THIN_LARGE   : return DocWriter.GetISOBytes("brdrtnthtnlg");
                case BORDER_WAVY                    : return DocWriter.GetISOBytes("brdrwavy");
                case BORDER_DOUBLE_WAVY             : return DocWriter.GetISOBytes("brdrwavydb");
                case BORDER_STRIPED                 : return DocWriter.GetISOBytes("brdrdashdotstr");
                case BORDER_EMBOSS                  : return DocWriter.GetISOBytes("brdremboss");
                case BORDER_ENGRAVE                 : return DocWriter.GetISOBytes("brdrengrave");
            }
            
            return DocWriter.GetISOBytes("brdrs");
        }
    }
}