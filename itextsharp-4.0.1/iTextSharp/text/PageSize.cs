using System;

/*
 * $Id: PageSize.cs,v 1.5 2007/02/09 15:34:38 psoares33 Exp $
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

namespace iTextSharp.text {
    /// <summary>
    /// The PageSize-object contains a number of rectangles representing the most common papersizes.
    /// </summary>
    /// <seealso cref="T:iTextSharp.text.Rectangle"/>
    public class PageSize {
    
        // membervariables
    
    /** This is the letter format */
        public static readonly Rectangle LETTER = new Rectangle(612,792);
        
    /** This is the note format */
        public static readonly Rectangle NOTE = new Rectangle(540,720);
        
    /** This is the legal format */
        public static readonly Rectangle LEGAL = new Rectangle(612,1008);
        
    /** This is the tabloid format */
        public static readonly Rectangle TABLOID = new Rectangle(792,1224);

    /** This is the executive format */
        public static readonly Rectangle EXECUTIVE = new Rectangle(522,756);

    /** This is the postcard format */
        public static readonly Rectangle POSTCARD = new Rectangle(283,416);
        
    /** This is the a0 format */
        public static readonly Rectangle A0 = new Rectangle(2384,3370);
        
    /** This is the a1 format */
        public static readonly Rectangle A1 = new Rectangle(1684,2384);
        
    /** This is the a2 format */
        public static readonly Rectangle A2 = new Rectangle(1191,1684);
        
    /** This is the a3 format */
        public static readonly Rectangle A3 = new Rectangle(842,1191);
        
    /** This is the a4 format */
        public static readonly Rectangle A4 = new Rectangle(595,842);
        
    /** This is the a5 format */
        public static readonly Rectangle A5 = new Rectangle(420,595);
        
    /** This is the a6 format */
        public static readonly Rectangle A6 = new Rectangle(297,420);
        
    /** This is the a7 format */
        public static readonly Rectangle A7 = new Rectangle(210,297);
        
    /** This is the a8 format */
        public static readonly Rectangle A8 = new Rectangle(148,210);
        
    /** This is the a9 format */
        public static readonly Rectangle A9 = new Rectangle(105,148);
        
    /** This is the a10 format */
        public static readonly Rectangle A10 = new Rectangle(73,105);
        
    /** This is the b0 format */
        public static readonly Rectangle B0 = new Rectangle(2834,4008);
        
    /** This is the b1 format */
        public static readonly Rectangle B1 = new Rectangle(2004,2834);
        
    /** This is the b2 format */
        public static readonly Rectangle B2 = new Rectangle(1417,2004);
        
    /** This is the b3 format */
        public static readonly Rectangle B3 = new Rectangle(1000,1417);
        
    /** This is the b4 format */
        public static readonly Rectangle B4 = new Rectangle(708,1000);
        
    /** This is the b5 format */
        public static readonly Rectangle B5 = new Rectangle(498,708);

    /** This is the b6 format */
        public static readonly Rectangle B6 = new Rectangle(354,498);
        
    /** This is the b7 format */
        public static readonly Rectangle B7 = new Rectangle(249,354);
        
    /** This is the b8 format */
        public static readonly Rectangle B8 = new Rectangle(175,249);

    /** This is the b9 format */
        public static readonly Rectangle B9 = new Rectangle(124,175);

    /** This is the b10 format */
        public static readonly Rectangle B10 = new Rectangle(87,124);
        
    /** This is the archE format */
        public static readonly Rectangle ARCH_E = new Rectangle(2592,3456);
        
    /** This is the archD format */
        public static readonly Rectangle ARCH_D = new Rectangle(1728,2592);
        
    /** This is the archC format */
        public static readonly Rectangle ARCH_C = new Rectangle(1296,1728);
        
    /** This is the archB format */
        public static readonly Rectangle ARCH_B = new Rectangle(864,1296);
        
    /** This is the archA format */
        public static readonly Rectangle ARCH_A = new Rectangle(648,864);
        
    /** This is the American Foolscap format */
        public static readonly Rectangle FLSA = new Rectangle(612,936);
        
    /** This is the European Foolscap format */
        public static readonly Rectangle FLSE = new Rectangle(648,936);
        
    /** This is the halfletter format */
        public static readonly Rectangle HALFLETTER = new Rectangle(396,612);
        
    /** This is the 11x17 format */
        public static readonly Rectangle _11X17 = new Rectangle(792,1224);
        
    /** This is the ISO 7810 ID-1 format (85.60 x 53.98 mm or 3.370 x 2.125 inch) */
        public static readonly Rectangle ID_1 = new Rectangle(242.65f,153);
        
    /** This is the ISO 7810 ID-2 format (A7 rotated) */
        public static readonly Rectangle ID_2 = new Rectangle(297,210);
        
    /** This is the ISO 7810 ID-3 format (B7 rotated) */
        public static readonly Rectangle ID_3 = new Rectangle(254,249);
        
    /** This is the ledger format */
        public static readonly Rectangle LEDGER = new Rectangle(1224,792);
        
    /** This is the Crown Quarto format */
        public static readonly Rectangle CROWN_QUARTO = new Rectangle(535,697);

    /** This is the Large Crown Quarto format */
        public static readonly Rectangle LARGE_CROWN_QUARTO = new Rectangle(569,731);
        
    /** This is the Demy Quarto format. */
        public static readonly Rectangle DEMY_QUARTO = new Rectangle(620,782);
        
    /** This is the Royal Quarto format. */
        public static readonly Rectangle ROYAL_QUARTO = new Rectangle(671,884);
        
    /** This is the Crown Octavo format */
        public static readonly Rectangle CROWN_OCTAVO = new Rectangle(348,527);
        
    /** This is the Large Crown Octavo format */
        public static readonly Rectangle LARGE_CROWN_OCTAVO = new Rectangle(365,561);
        
    /** This is the Demy Octavo format */
        public static readonly Rectangle DEMY_OCTAVO = new Rectangle(391,612);
        
    /** This is the Royal Octavo format. */
        public static readonly Rectangle ROYAL_OCTAVO = new Rectangle(442,663);
        
    /** This is the small paperback format. */
        public static readonly Rectangle SMALL_PAPERBACK = new Rectangle(314,504);
        
    /** This is the Pengiun small paperback format. */
        public static readonly Rectangle PENGUIN_SMALL_PAPERBACK = new Rectangle(314,513);
        
    /** This is the Penguin large paparback format. */
        public static readonly Rectangle PENGUIN_LARGE_PAPERBACK = new Rectangle(365,561);

        /**
        * This method returns a Rectangle based on a String.
        * Possible values are the the names of a constant in this class
        * (for instance "A4", "LETTER",...) or a value like "595 842"
        */
        public static Rectangle GetRectangle(String name)  {
            name = name.Trim().ToUpper();
            int pos = name.IndexOf(' ');
            if (pos == -1) {
                try {            
                    return (Rectangle)typeof(PageSize).GetField(name).GetValue(null);
                } catch (Exception) {
                    throw new ArgumentException("Can't find page size " + name);          
                }
            }
            else {
                try {
                    String width = name.Substring(0, pos);
                    String height = name.Substring(pos + 1);
                    return new Rectangle(float.Parse(width, System.Globalization.NumberFormatInfo.InvariantInfo), float.Parse(height, System.Globalization.NumberFormatInfo.InvariantInfo));
                } catch(Exception e) {
                    throw new ArgumentException(name + " is not a valid page size format: " + e.Message);
                }
            }
        }    
    }
}
