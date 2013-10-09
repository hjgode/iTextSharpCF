using System;
using System.Text;
using System.Collections;
using System.util;

using iTextSharp.text.pdf;
using iTextSharp.text.html;

/*
 * $Id: Chunk.cs,v 1.11 2007/01/24 17:42:16 psoares33 Exp $
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
    /// This is the smallest significant part of text that can be added to a document.
    /// </summary>
    /// <remarks>
    /// Most elements can be divided in one or more Chunks.
    /// A chunk is a string with a certain Font.
    /// all other layoutparameters should be defined in the object to which
    /// this chunk of text is added.
    /// </remarks>
    /// <example>
    /// <code>
    /// <strong>Chunk chunk = new Chunk("Hello world", FontFactory.GetFont(FontFactory.COURIER, 20, Font.ITALIC, new Color(255, 0, 0)));</strong>
    /// document.Add(chunk);
    /// </code>
    /// </example>
    public class Chunk : IElement {

        // public static membervariables
    
        public const string OBJECT_REPLACEMENT_CHARACTER = "\ufffc";

        ///<summary> This is a Chunk containing a newline. </summary>
        public static readonly Chunk NEWLINE = new Chunk("\n");

    /** This is a Chunk containing a newpage. */
        public static readonly Chunk NEXTPAGE = new Chunk("");
        static Chunk() {
            NEXTPAGE.SetNewPage();
        }

        ///<summary> Key for sub/basescript. </summary>
        public const string SUBSUPSCRIPT = "SUBSUPSCRIPT";

        ///<summary> Key for underline. </summary>
        public const string UNDERLINE = "UNDERLINE";

        ///<summary> Key for strikethru. </summary>
        public const string STRIKETHRU = "STRIKETHRU";

        ///<summary> Key for color. </summary>
        public const string COLOR = "COLOR";

        ///<summary> Key for encoding. </summary>
        public const string ENCODING = "ENCODING";

        ///<summary> Key for remote goto. </summary>
        public const string REMOTEGOTO = "REMOTEGOTO";

        ///<summary> Key for local goto. </summary>
        public const string LOCALGOTO = "LOCALGOTO";

        ///<summary> Key for local destination. </summary>
        public const string LOCALDESTINATION = "LOCALDESTINATION";

        ///<summary> Key for image. </summary>
        public const string IMAGE = "IMAGE";

        ///<summary> Key for generic tag. </summary>
        public const string GENERICTAG = "GENERICTAG";

        ///<summary> Key for newpage. </summary>
        public const string NEWPAGE = "NEWPAGE";

        ///<summary> Key for split character. </summary>
        public const string SPLITCHARACTER = "SPLITCHARACTER";

        ///<summary> Key for Action. </summary>
        public const string ACTION = "ACTION";

        ///<summary> Key for background. </summary>
        public const string BACKGROUND = "BACKGROUND";

        ///<summary> Key for annotation. </summary>
        public const string PDFANNOTATION = "PDFANNOTATION";

        ///<summary> Key for hyphenation. </summary>
        public const string HYPHENATION = "HYPHENATION";

        ///<summary> Key for text skewing. </summary>
        public const string SKEW = "SKEW";

        ///<summary> Key for text rendering mode.</summary>
        public const string TEXTRENDERMODE = "TEXTRENDERMODE";

        /** Key for text horizontal scaling. */
        public const string HSCALE = "HSCALE";



        // member variables

        ///<summary> This is the content of this chunk of text. </summary>
        protected StringBuilder content = null;

        ///<summary> This is the Font of this chunk of text. </summary>
        protected Font font = null;

        ///<summary> Contains some of the attributes for this Chunk. </summary>
        protected Hashtable attributes = null;

        // constructors

        /// <summary>
        /// Empty constructor.
        /// </summary>
        /// <overloads>
        /// Has six overloads.
        /// </overloads>
        protected Chunk() {
        }

        /**
        * A <CODE>Chunk</CODE> copy constructor.
        * @param ck the <CODE>Chunk</CODE> to be copied
        */    
        public Chunk(Chunk ck) {
            if (ck.content != null) {
                content = new StringBuilder(ck.content.ToString());
            }
            if (ck.font != null) {
                font = new Font(ck.font);
            }
            if (ck.attributes != null) {
                attributes = new Hashtable(ck.attributes);
            }
        }
        
        /// <summary>
        /// Constructs a chunk of text with a certain content and a certain Font.
        /// </summary>
        /// <param name="content">the content</param>
        /// <param name="font">the font</param>
        public Chunk(string content, Font font) {
            this.content = new StringBuilder(content);
            this.font = font;
        }

        /// <summary>
        /// Constructs a chunk of text with a certain content, without specifying a Font.
        /// </summary>
        /// <param name="content">the content</param>
        public Chunk(string content) : this(content, new Font()) {}

        /**
        * Constructs a chunk of text with a char and a certain <CODE>Font</CODE>.
        *
        * @param    c        the content
        * @param    font        the font
        */
        public Chunk(char c, Font font) {
            this.content = new StringBuilder();
            this.content.Append(c);
            this.font = font;
        }
            
        /**
        * Constructs a chunk of text with a char, without specifying a <CODE>Font</CODE>.
        *
        * @param    c        the content
        */
        public Chunk(char c) : this(c, new Font()) {
        }

        /// <summary>
        /// Constructs a chunk containing an Image.
        /// </summary>
        /// <param name="image">the image</param>
        /// <param name="offsetX">the image offset in the x direction</param>
        /// <param name="offsetY">the image offset in the y direction</param>
        public Chunk(Image image, float offsetX, float offsetY) : this(OBJECT_REPLACEMENT_CHARACTER, new Font()) {
            Image copyImage = Image.GetInstance(image);
            copyImage.SetAbsolutePosition(float.NaN, float.NaN);
            SetAttribute(IMAGE, new Object[]{copyImage, offsetX, offsetY, false});
        }

        /// <summary>
        /// Constructs a chunk containing an Image.
        /// </summary>
        /// <param name="image">the image</param>
        /// <param name="offsetX">the image offset in the x direction</param>
        /// <param name="offsetY">the image offset in the y direction</param>
        /// <param name="changeLeading">true if the leading has to be adapted to the image</param>
        public Chunk(Image image, float offsetX, float offsetY, bool changeLeading) : this(OBJECT_REPLACEMENT_CHARACTER, new Font()) {
            SetAttribute(IMAGE, new Object[]{image, offsetX, offsetY, changeLeading});
        }

        /// <summary>
        /// Returns a Chunk that has been constructed taking in account
        /// the value of some attributes.
        /// </summary>
        /// <param name="attributes">some attributes</param>
        public Chunk(Properties attributes) : this("", FontFactory.GetFont(attributes)) {
            string value;
            if ((value = attributes.Remove(ElementTags.ITEXT)) != null) {
                Append(value);
            }
            if ((value = attributes.Remove(ElementTags.LOCALGOTO)) != null) {
                SetLocalGoto(value);
            }
            if ((value = attributes.Remove(ElementTags.REMOTEGOTO)) != null) {
                String destination = attributes.Remove(ElementTags.DESTINATION);
                String page = attributes.Remove(ElementTags.PAGE);
                if (page != null) {
                    SetRemoteGoto(value, int.Parse(page));
                }
                else if (destination != null) {
                    SetRemoteGoto(value, destination);
                }
            }
            if ((value = attributes.Remove(ElementTags.LOCALDESTINATION)) != null) {
                SetLocalDestination(value);
            }
            if ((value = attributes.Remove(ElementTags.SUBSUPSCRIPT)) != null) {
                SetTextRise(float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo));
            }
            if ((value = attributes.Remove(Markup.CSS_KEY_VERTICALALIGN)) != null && value.EndsWith("%")) {
                float p = float.Parse(value.Substring(0, value.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo) / 100f;
                SetTextRise(p * font.Size);
            }
            if ((value = attributes.Remove(ElementTags.GENERICTAG)) != null) {
                SetGenericTag(value);
            }
            if ((value = attributes.Remove(ElementTags.BACKGROUNDCOLOR)) != null) {
                SetBackground(Markup.DecodeColor(value));
            }
        }

        // implementation of the Element-methods

        /// <summary>
        /// Processes the element by adding it (or the different parts) to an
        /// IElementListener.
        /// </summary>
        /// <param name="listener">an IElementListener</param>
        /// <returns>true if the element was processed successfully</returns>
        public bool Process(IElementListener listener) {
            try {
                return listener.Add(this);
            }
            catch (DocumentException de) {
                de.GetType();
                return false;
            }
        }

        /// <summary>
        /// Gets the type of the text element.
        /// </summary>
        /// <value>a type</value>
        public int Type {
            get {
                return Element.CHUNK;
            }
        }

        /// <summary>
        /// Gets all the chunks in this element.
        /// </summary>
        /// <value>an ArrayList</value>
        public ArrayList Chunks {
            get {
                ArrayList tmp = new ArrayList();
                tmp.Add(this);
                return tmp;
            }
        }

        // methods

        /// <summary>
        /// appends some text to this Chunk.
        /// </summary>
        /// <param name="str">a string</param>
        /// <returns>a StringBuilder</returns>
        public StringBuilder Append(string str) {
            return content.Append(str);
        }

        // methods to retrieve information

        /// <summary>
        /// Get/set the font of this Chunk.
        /// </summary>
        /// <value>a Font</value>
        public virtual Font Font {
            get {
                return font;
            }

            set {
                this.font = value;
            }
        }


        /// <summary>
        /// Returns the content of this Chunk.
        /// </summary>
        /// <value>a string</value>
        public virtual string Content {
            get {
                return content.ToString();
            }
        }

        public override string ToString() {
            return content.ToString();
        }


        /// <summary>
        /// Checks is this Chunk is empty.
        /// </summary>
        /// <returns>false if the Chunk contains other characters than space.</returns>
        public virtual bool IsEmpty() {
            return (content.ToString().Trim().Length == 0) && (content.ToString().IndexOf("\n") == -1) && (attributes == null);
        }

        /**
        * Gets the width of the Chunk in points.
        * @return a width in points
        */
        public float GetWidthPoint() {
            if (GetImage() != null) {
                return GetImage().ScaledWidth;
            }
            return font.GetCalculatedBaseFont(true).GetWidthPoint(Content, font.CalculatedSize) * HorizontalScaling;
        }
    
        /// <summary>
        /// Sets the text displacement relative to the baseline. Positive values rise the text,
        /// negative values lower the text.
        /// </summary>
        /// <remarks>
        /// It can be used to implement sub/basescript.
        /// </remarks>
        /// <param name="rise">the displacement in points</param>
        /// <returns>this Chunk</returns>
        public Chunk SetTextRise(float rise) {
            return SetAttribute(SUBSUPSCRIPT, rise);
        }

        /**
        * Gets the text displacement relatiev to the baseline.
        * @return a displacement in points
        */
        public float TextRise {
            get {
                if (attributes.ContainsKey(SUBSUPSCRIPT)) {
                    return (float)attributes[SUBSUPSCRIPT];
                }
                return 0.0f;
            }
        }

        /** Sets the text rendering mode. It can outline text, simulate bold and make
        * text invisible.
        * @param mode the text rendering mode. It can be <CODE>PdfContentByte.TEXT_RENDER_MODE_FILL</CODE>,
        * <CODE>PdfContentByte.TEXT_RENDER_MODE_STROKE</CODE>, <CODE>PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE</CODE>
        * and <CODE>PdfContentByte.TEXT_RENDER_MODE_INVISIBLE</CODE>.
        * @param strokeWidth the stroke line width for the modes <CODE>PdfContentByte.TEXT_RENDER_MODE_STROKE</CODE> and
        * <CODE>PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE</CODE>.
        * @param strokeColor the stroke color or <CODE>null</CODE> to follow the text color
        * @return this <CODE>Chunk</CODE>
        */    
        public Chunk SetTextRenderMode(int mode, float strokeWidth, Color strokeColor) {
            return SetAttribute(TEXTRENDERMODE, new Object[]{mode, strokeWidth, strokeColor});
        }

        /**
        * Skews the text to simulate italic and other effects.
        * Try <CODE>alpha=0</CODE> and <CODE>beta=12</CODE>.
        * @param alpha the first angle in degrees
        * @param beta the second angle in degrees
        * @return this <CODE>Chunk</CODE>
        */    
        public Chunk SetSkew(float alpha, float beta) {
            alpha = (float)Math.Tan(alpha * Math.PI / 180);
            beta = (float)Math.Tan(beta * Math.PI / 180);
            return SetAttribute(SKEW, new float[]{alpha, beta});
        }

        /**
        * Sets the text horizontal scaling. A value of 1 is normal and a value of 0.5f
        * shrinks the text to half it's width.
        * @param scale the horizontal scaling factor
        * @return this <CODE>Chunk</CODE>
        */    
        public Chunk SetHorizontalScaling(float scale) {
            return SetAttribute(HSCALE, scale);
        }
        
        /**
        * Gets the horizontal scaling.
        * @return a percentage in float
        */
        public float HorizontalScaling {
            get {
                if (attributes == null) return 1f;
                Object f = attributes[HSCALE];
                if (f == null) return 1f;
                return (float)f;
            }
        }
        
        /// <summary>
        /// Sets an action for this Chunk.
        /// </summary>
        /// <param name="action">the action</param>
        /// <returns>this Chunk</returns>
        public Chunk SetAction(PdfAction action) {
            return SetAttribute(ACTION, action);
        }

        /// <summary>
        /// Sets an anchor for this Chunk.
        /// </summary>
        /// <param name="url">the Uri to link to</param>
        /// <returns>this Chunk</returns>
        public Chunk SetAnchor(Uri url) {
            return SetAttribute(ACTION, new PdfAction(url));
        }

        /// <summary>
        /// Sets an anchor for this Chunk.
        /// </summary>
        /// <param name="url">the url to link to</param>
        /// <returns>this Chunk</returns>
        public Chunk SetAnchor(string url) {
            return SetAttribute(ACTION, new PdfAction(url));
        }

        /// <summary>
        /// Sets a local goto for this Chunk.
        /// </summary>
        /// <remarks>
        /// There must be a local destination matching the name.
        /// </remarks>
        /// <param name="name">the name of the destination to go to</param>
        /// <returns>this Chunk</returns>
        public Chunk SetLocalGoto(string name) {
            return SetAttribute(LOCALGOTO, name);
        }

        /// <summary>
        /// Sets the color of the background Chunk.
        /// </summary>
        /// <param name="color">the color of the background</param>
        /// <returns>this Chunk</returns>
        public Chunk SetBackground(Color color) {
            return SetBackground(color, 0, 0, 0, 0);
        }

        /** Sets the color and the size of the background <CODE>Chunk</CODE>.
        * @param color the color of the background
        * @param extraLeft increase the size of the rectangle in the left
        * @param extraBottom increase the size of the rectangle in the bottom
        * @param extraRight increase the size of the rectangle in the right
        * @param extraTop increase the size of the rectangle in the top
        * @return this <CODE>Chunk</CODE>
        */
        public Chunk SetBackground(Color color, float extraLeft, float extraBottom, float extraRight, float extraTop) {
            return SetAttribute(BACKGROUND, new Object[]{color, new float[]{extraLeft, extraBottom, extraRight, extraTop}});
        }

        /**
        * Sets an horizontal line that can be an underline or a strikethrough.
        * Actually, the line can be anywhere vertically and has always the
        * <CODE>Chunk</CODE> width. Multiple call to this method will
        * produce multiple lines.
        * @param thickness the absolute thickness of the line
        * @param yPosition the absolute y position relative to the baseline
        * @return this <CODE>Chunk</CODE>
        */    
        public Chunk SetUnderline(float thickness, float yPosition) {
            return SetUnderline(null, thickness, 0f, yPosition, 0f, PdfContentByte.LINE_CAP_BUTT);
        }

        /**
        * Sets an horizontal line that can be an underline or a strikethrough.
        * Actually, the line can be anywhere vertically and has always the
        * <CODE>Chunk</CODE> width. Multiple call to this method will
        * produce multiple lines.
        * @param color the color of the line or <CODE>null</CODE> to follow
        * the text color
        * @param thickness the absolute thickness of the line
        * @param thicknessMul the thickness multiplication factor with the font size
        * @param yPosition the absolute y position relative to the baseline
        * @param yPositionMul the position multiplication factor with the font size
        * @param cap the end line cap. Allowed values are
        * PdfContentByte.LINE_CAP_BUTT, PdfContentByte.LINE_CAP_ROUND and
        * PdfContentByte.LINE_CAP_PROJECTING_SQUARE
        * @return this <CODE>Chunk</CODE>
        */    
        public Chunk SetUnderline(Color color, float thickness, float thicknessMul, float yPosition, float yPositionMul, int cap) {
            if (attributes == null)
                attributes = new Hashtable();
            Object[] obj = {color, new float[]{thickness, thicknessMul, yPosition, yPositionMul, (float)cap}};
            Object[][] unders = AddToArray((Object[][])attributes[UNDERLINE], obj);
            return SetAttribute(UNDERLINE, unders);
        }
        
        /**
        * Utility method to extend an array.
        * @param original the original array or <CODE>null</CODE>
        * @param item the item to be added to the array
        * @return a new array with the item appended
        */    
        public static Object[][] AddToArray(Object[][] original, Object[] item) {
            if (original == null) {
                original = new Object[1][];
                original[0] = item;
                return original;
            }
            else {
                Object[][] original2 = new Object[original.Length + 1][];
                Array.Copy(original, 0, original2, 0, original.Length);
                original2[original.Length] = item;
                return original2;
            }
        }

        /// <summary>
        /// Sets a generic annotation to this Chunk.
        /// </summary>
        /// <param name="annotation">the annotation</param>
        /// <returns>this Chunk</returns>
        public Chunk SetAnnotation(PdfAnnotation annotation) {
            return SetAttribute(PDFANNOTATION, annotation);
        }

        /// <summary>
        /// sets the hyphenation engine to this Chunk.
        /// </summary>
        /// <param name="hyphenation">the hyphenation engine</param>
        /// <returns>this Chunk</returns>
        public Chunk SetHyphenation(IHyphenationEvent hyphenation) {
            return SetAttribute(HYPHENATION, hyphenation);
        }

        /// <summary>
        /// Sets a goto for a remote destination for this Chunk.
        /// </summary>
        /// <param name="filename">the file name of the destination document</param>
        /// <param name="name">the name of the destination to go to</param>
        /// <returns>this Chunk</returns>
        public Chunk SetRemoteGoto(string filename, string name) {
            return SetAttribute(REMOTEGOTO, new Object[]{filename, name});
        }

        /// <summary>
        /// Sets a goto for a remote destination for this Chunk.
        /// </summary>
        /// <param name="filename">the file name of the destination document</param>
        /// <param name="page">the page of the destination to go to. First page is 1</param>
        /// <returns>this Chunk</returns>
        public Chunk SetRemoteGoto(string filename, int page) {
            return SetAttribute(REMOTEGOTO, new Object[]{filename, page});
        }

        /// <summary>
        /// Sets a local destination for this Chunk.
        /// </summary>
        /// <param name="name">the name for this destination</param>
        /// <returns>this Chunk</returns>
        public Chunk SetLocalDestination(string name) {
            return SetAttribute(LOCALDESTINATION, name);
        }

        /// <summary>
        /// Sets the generic tag Chunk.
        /// </summary>
        /// <remarks>
        /// The text for this tag can be retrieved with PdfPageEvent.
        /// </remarks>
        /// <param name="text">the text for the tag</param>
        /// <returns>this Chunk</returns>
        public Chunk SetGenericTag(string text) {
            return SetAttribute(GENERICTAG, text);
        }

        /// <summary>
        /// Sets the split characters.
        /// </summary>
        /// <param name="splitCharacter">the SplitCharacter interface</param>
        /// <returns>this Chunk</returns>
        public Chunk SetSplitCharacter(ISplitCharacter splitCharacter) {
            return SetAttribute(SPLITCHARACTER, splitCharacter);
        }

        /// <summary>
        /// Sets a new page tag..
        /// </summary>
        /// <returns>this Chunk</returns>
        public Chunk SetNewPage() {
            return SetAttribute(NEWPAGE, null);
        }

        /// <summary>
        /// Sets an arbitrary attribute.
        /// </summary>
        /// <param name="name">the key for the attribute</param>
        /// <param name="obj">the value of the attribute</param>
        /// <returns>this Chunk</returns>
        private Chunk SetAttribute(string name, Object obj) {
            if (attributes == null)
                attributes = new Hashtable();
            attributes[name] = obj;
            return this;
        }

        /// <summary>
        /// Gets the attributes for this Chunk.
        /// </summary>
        /// <remarks>
        /// It may be null.
        /// </remarks>
        /// <value>a Hashtable</value>
        public Hashtable Attributes {
            get {
                return attributes;
            }
        }

        /// <summary>
        /// Checks the attributes of this Chunk.
        /// </summary>
        /// <returns>false if there aren't any.</returns>
        public bool HasAttributes() {
            return attributes != null;
        }

        /// <summary>
        /// Returns the image.
        /// </summary>
        /// <value>an Image</value>
        public Image GetImage() {
            if (attributes == null) return null;
            Object[] obj = (Object[])attributes[Chunk.IMAGE];
            if (obj == null)
                return null;
            else {
                return (Image)obj[0];
            }
        }

        /// <summary>
        /// Checks if a given tag corresponds with this object.
        /// </summary>
        /// <param name="tag">the given tag</param>
        /// <returns>true if the tag corresponds</returns>
        public static bool IsTag(string tag) {
            return ElementTags.CHUNK.Equals(tag);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static ICollection GetKeySet(Properties table) {
            return (table == null) ? new Properties().Keys : table.Keys;
        }
    }
}
