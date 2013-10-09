using System;
using System.IO;
using System.Net;
using iTextSharp.text;
using iTextSharp.text.rtf;
using iTextSharp.text.rtf.document;
using iTextSharp.text.rtf.text;
using iTextSharp.text.rtf.style;
using iTextSharp.text.pdf.codec.wmf;
/*
 * $Id: RtfImage.cs,v 1.4 2005/12/26 09:57:29 psoares33 Exp $
 * $Name:  $
 *
 * Copyright 2001, 2002, 2003, 2004 by Mark Hall
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

namespace iTextSharp.text.rtf.graphic {

    /**
    * The RtfImage contains one image. Supported image types are jpeg, png, wmf, bmp.
    * 
    * @version $Version:$
    * @author Mark Hall (mhall@edu.uni-klu.ac.at)
    * @author Paulo Soares
    */
    public class RtfImage : RtfElement {
        
        /**
        * Constant for the shape/picture group
        */
        private static byte[] PICTURE_GROUP = DocWriter.GetISOBytes("\\*\\shppict");
        /**
        * Constant for a picture
        */
        private static byte[] PICTURE = DocWriter.GetISOBytes("\\pict");
        /**
        * Constant for a jpeg image
        */
        private static byte[] PICTURE_JPEG = DocWriter.GetISOBytes("\\jpegblip");
        /**
        * Constant for a png image
        */
        private static byte[] PICTURE_PNG = DocWriter.GetISOBytes("\\pngblip");
        /**
        * Constant for a bmp image
        */
        private static byte[] PICTURE_BMP = DocWriter.GetISOBytes("\\dibitmap0");
        /**
        * Constant for a wmf image
        */
        private static byte[] PICTURE_WMF = DocWriter.GetISOBytes("\\wmetafile8");
        /**
        * Constant for the picture width
        */
        private static byte[] PICTURE_WIDTH = DocWriter.GetISOBytes("\\picw");
        /**
        * Constant for the picture height
        */
        private static byte[] PICTURE_HEIGHT = DocWriter.GetISOBytes("\\pich");
        /**
        * Constant for the picture width scale
        */
        private static byte[] PICTURE_SCALED_WIDTH = DocWriter.GetISOBytes("\\picwgoal");
        /**
        * Constant for the picture height scale
        */
        private static byte[] PICTURE_SCALED_HEIGHT = DocWriter.GetISOBytes("\\pichgoal");
        
        /**
        * The type of image this is.
        */
        private int imageType = Image.ORIGINAL_NONE;
        /**
        * The actual image. Already formated for direct inclusion in the rtf document
        */
        private byte[] image = new byte[0];
        /**
        * The alignment of this picture
        */
        private int alignment = Element.ALIGN_LEFT;
        /**
        * The width of this picture
        */
        private float width = 0;
        /**
        * The height of this picutre
        */
        private float height = 0;
        /**
        * The intended display width of this picture
        */
        private float plainWidth = 0;
        /**
        * The intended display height of this picture
        */
        private float plainHeight = 0;
        /**
        * Whether this RtfImage is a top level element and should
        * be an extra paragraph.
        */
        private bool topLevelElement = false;
        
        /**
        * Constructs a RtfImage for an Image.
        * 
        * @param doc The RtfDocument this RtfImage belongs to
        * @param image The Image that this RtfImage wraps
        * @throws DocumentException If an error occured accessing the image content
        */
        public RtfImage(RtfDocument doc, Image image) : base(doc) {
            imageType = image.OriginalType;
            if (!(imageType == Image.ORIGINAL_JPEG || imageType == Image.ORIGINAL_BMP
                    || imageType == Image.ORIGINAL_PNG || imageType == Image.ORIGINAL_WMF)) {
                throw new DocumentException("Only BMP, PNG, WMF and JPEG images are supported by the RTF Writer");
            }
            alignment = image.Alignment;
            width = image.Width;
            height = image.Height;
            plainWidth = image.PlainWidth;
            plainHeight = image.PlainHeight;
            this.image = GetImage(image);
        }
        
        /**
        * Extracts the image data from the Image. The data is formated for direct inclusion
        * in a rtf document
        * 
        * @param image The Image for which to extract the content
        * @return The image data formated for the rtf document
        * @throws DocumentException If an error occurs accessing the image content
        */
        private byte[] GetImage(Image image) {
            MemoryStream imageTemp = new MemoryStream();
            try {
                Stream imageIn;
                if (imageType == Image.ORIGINAL_BMP) {
                    imageIn = new MemoryStream(MetaDo.WrapBMP(image));
                } else {
                    if (image.OriginalData == null) {
#if !NETCF
                        imageIn = WebRequest.Create(image.Url).GetResponse().GetResponseStream();
#else
                        imageIn=new FileStream(image.Url.LocalPath, FileMode.Open);
#endif
                    } else {
                        imageIn = new MemoryStream(image.OriginalData);
                    }
                    if (imageType == Image.ORIGINAL_WMF) { //remove the placeable header
                        Image.Skip(imageIn, 22);
                    }
                }
                int buffer = 0;
                int count = 0;
                while ((buffer = imageIn.ReadByte()) != -1) {
                    String helperStr = buffer.ToString("X2");
                    byte[] t = DocWriter.GetISOBytes(helperStr);
                    imageTemp.Write(t, 0, t.Length);
                    count++;
                    if (count == 64) {
                        imageTemp.WriteByte((byte) '\n');
                        count = 0;
                    }
                }
            } catch (IOException ioe) {
                throw new DocumentException(ioe.ToString());
            }
            return imageTemp.ToArray();
        }
        
        /**
        * Writes the RtfImage content
        * 
        * @return the RtfImage content
        */
        public override byte[] Write() {
            MemoryStream result = new MemoryStream();
            byte[] t;
            try {
                if (this.topLevelElement) {
                    result.Write(RtfParagraph.PARAGRAPH_DEFAULTS, 0, RtfParagraph.PARAGRAPH_DEFAULTS.Length);
                }
                switch (alignment) {
                    case Element.ALIGN_LEFT:
                        result.Write(RtfParagraphStyle.ALIGN_LEFT, 0, RtfParagraphStyle.ALIGN_LEFT.Length);
                        break;
                    case Element.ALIGN_RIGHT:
                        result.Write(RtfParagraphStyle.ALIGN_RIGHT, 0, RtfParagraphStyle.ALIGN_RIGHT.Length);
                        break;
                    case Element.ALIGN_CENTER:
                        result.Write(RtfParagraphStyle.ALIGN_CENTER, 0, RtfParagraphStyle.ALIGN_CENTER.Length);
                        break;
                    case Element.ALIGN_JUSTIFIED:
                        result.Write(RtfParagraphStyle.ALIGN_JUSTIFY, 0, RtfParagraphStyle.ALIGN_JUSTIFY.Length);
                        break;
                }
                result.Write(OPEN_GROUP, 0, OPEN_GROUP.Length);
                result.Write(PICTURE_GROUP, 0, PICTURE_GROUP.Length);
                result.Write(OPEN_GROUP, 0, OPEN_GROUP.Length);
                result.Write(PICTURE, 0, PICTURE.Length);
                switch (imageType) {
                    case Image.ORIGINAL_JPEG:
                        result.Write(PICTURE_JPEG, 0, PICTURE_JPEG.Length);
                        break;
                    case Image.ORIGINAL_PNG:
                        result.Write(PICTURE_PNG, 0, PICTURE_PNG.Length);
                        break;
                    case Image.ORIGINAL_WMF:
                    case Image.ORIGINAL_BMP:
                        result.Write(PICTURE_WMF, 0, PICTURE_WMF.Length);
                        break;
                }
                result.Write(PICTURE_WIDTH, 0, PICTURE_WIDTH.Length);
                result.Write(t = IntToByteArray((int) width), 0, t.Length);
                result.Write(PICTURE_HEIGHT, 0, PICTURE_HEIGHT.Length);
                result.Write(t = IntToByteArray((int) height), 0, t.Length);
                if (width != plainWidth || this.imageType == Image.ORIGINAL_BMP) {
                    result.Write(PICTURE_SCALED_WIDTH, 0, PICTURE_SCALED_WIDTH.Length);
                    result.Write(t = IntToByteArray((int) (plainWidth * RtfElement.TWIPS_FACTOR)), 0, t.Length);
                }
                if (height != plainHeight || this.imageType == Image.ORIGINAL_BMP) {
                    result.Write(PICTURE_SCALED_HEIGHT, 0, PICTURE_SCALED_HEIGHT.Length);
                    result.Write(t = IntToByteArray((int) (plainHeight * RtfElement.TWIPS_FACTOR)), 0, t.Length);
                }
                result.Write(DELIMITER, 0, DELIMITER.Length);
                result.WriteByte((byte) '\n');
                result.Write(image, 0, image.Length);
                result.Write(CLOSE_GROUP, 0, CLOSE_GROUP.Length);
                result.Write(CLOSE_GROUP, 0, CLOSE_GROUP.Length);
                if (this.topLevelElement) {
                    result.Write(RtfParagraph.PARAGRAPH, 0, RtfParagraph.PARAGRAPH.Length);
                }
                result.WriteByte((byte) '\n');
            } catch (IOException) {
            }
            return result.ToArray();
        }
        
        /**
        * Sets the alignment of this RtfImage. Uses the alignments from com.lowagie.text.Element.
        * 
        * @param alignment The alignment to use.
        */
        public void SetAlignment(int alignment) {
            this.alignment = alignment;
        }

        /**
        * Set whether this RtfImage should behave like a top level element
        * and enclose itself in a paragraph.
        * 
        * @param topLevelElement Whether to behave like a top level element.
        */
        public void SetTopLevelElement(bool topLevelElement) {
            this.topLevelElement = topLevelElement;
        }
    }
}