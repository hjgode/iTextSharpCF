using System;
using System.IO;
using System.Net;
using System.Text;
using System.Runtime.CompilerServices;
using System.Collections;
using System.util;
using System.Reflection;

using iTextSharp.text.pdf;
using iTextSharp.text.pdf.codec;

/*
 * $Id: Image.cs,v 1.17 2007/01/24 17:42:16 psoares33 Exp $
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
    /// An Image is the representation of a graphic element (JPEG, PNG or GIF)
    /// that has to be inserted into the document
    /// </summary>
    /// <seealso cref="T:iTextSharp.text.Element"/>
    /// <seealso cref="T:iTextSharp.text.Rectangle"/>
    public abstract class Image : Rectangle, IElement {
    
        // static membervariables (concerning the presence of borders)
    
        /// <summary> this is a kind of image Element. </summary>
        public const int DEFAULT = 0;
    
        /// <summary> this is a kind of image Element. </summary>
        public const int RIGHT_ALIGN = 2;
    
        /// <summary> this is a kind of image Element. </summary>
        public const int LEFT_ALIGN = 0;
    
        /// <summary> this is a kind of image Element. </summary>
        public const int MIDDLE_ALIGN = 1;
    
        /// <summary> this is a kind of image Element. </summary>
        public const int TEXTWRAP = 4;
    
        /// <summary> this is a kind of image Element. </summary>
        public const int UNDERLYING = 8;
    
        /// <summary> This represents a coordinate in the transformation matrix. </summary>
        public const int AX = 0;
    
        /// <summary> This represents a coordinate in the transformation matrix. </summary>
        public const int AY = 1;
    
        /// <summary> This represents a coordinate in the transformation matrix. </summary>
        public const int BX = 2;
    
        /// <summary> This represents a coordinate in the transformation matrix. </summary>
        public const int BY = 3;
    
        /// <summary> This represents a coordinate in the transformation matrix. </summary>
        public const int CX = 4;
    
        /// <summary> This represents a coordinate in the transformation matrix. </summary>
        public const int CY = 5;
    
        /// <summary> This represents a coordinate in the transformation matrix. </summary>
        public const int DX = 6;
    
        /// <summary> This represents a coordinate in the transformation matrix. </summary>
        public const int DY = 7;
    
        /** type of image */
        public const int ORIGINAL_NONE = 0;

        /** type of image */
        public const int ORIGINAL_JPEG = 1;

        /** type of image */
        public const int ORIGINAL_PNG = 2;

        /** type of image */
        public const int ORIGINAL_GIF = 3;

        /** type of image */
        public const int ORIGINAL_BMP = 4;

        /** type of image */
        public const int ORIGINAL_TIFF = 5;

        /** type of image */
        public const int ORIGINAL_WMF = 6;

        /** Image color inversion */
        protected bool invert = false;
    
        /// <summary> The imagetype. </summary>
        protected int type;
    
        /// <summary> The URL of the image. </summary>
        protected Uri url;
    
        /// <summary> The raw data of the image. </summary>
        protected byte[] rawData;
    
        /// <summary> The template to be treated as an image. </summary>
        protected PdfTemplate[] template = new PdfTemplate[1];
    
        /// <summary> The alignment of the Image. </summary>
        protected int alignment;
    
        /// <summary> Text that can be shown instead of the image. </summary>
        protected string alt;
    
        /// <summary> This is the absolute X-position of the image. </summary>
        protected float absoluteX = float.NaN;
    
        /// <summary> This is the absolute Y-position of the image. </summary>
        protected float absoluteY = float.NaN;
    
        /// <summary> This is the width of the image without rotation. </summary>
        protected float plainWidth;
    
        /// <summary> This is the width of the image without rotation. </summary>
        protected float plainHeight;
    
        /// <summary> This is the scaled width of the image taking rotation into account. </summary>
        protected float scaledWidth;
    
        /// <summary> This is the original height of the image taking rotation into account. </summary>
        protected float scaledHeight;
    
        /// <summary> This is the rotation of the image. </summary>
        protected new float rotation;
    
        /// <summary> this is the colorspace of a jpeg-image. </summary>
        protected int colorspace = -1;
    
        /// <summary> this is the bits per component of the raw image. It also flags a CCITT image.</summary>
        protected int bpc = 1;
    
        /// <summary> this is the transparency information of the raw image</summary>
        protected int[] transparency;
    
        // for the moment these variables are only used for Images in class Table
        // code contributed by Pelikan Stephan
        /** the indentation to the left. */
        protected float indentationLeft = 0;

        /** the indentation to the right. */
        protected float indentationRight = 0;
        // serial stamping
    
        protected long mySerialId = GetSerialId();
    
        static object serialId = 0L;

        /// <summary> Holds value of property dpiX. </summary>
        protected int dpiX = 0;
    
        /// <summary> Holds value of property dpiY. </summary>
        protected int dpiY = 0;
    
        protected bool mask = false;
    
        protected Image imageMask;
    
        /// <summary> Holds value of property interpolation. </summary>
        protected bool interpolation;
    
        /// <summary> if the annotation is not null the image will be clickable. </summary>
        protected Annotation annotation = null;

        /// <summary> ICC Profile attached </summary>
        protected ICC_Profile profile = null;
    
        /** Holds value of property deflated. */
        protected bool deflated = false;

        private PdfDictionary additional = null;

        /** Holds value of property smask. */
        private bool smask;

        /** Holds value of property XYRatio. */
        private float xyRatio = 0;

        /** Holds value of property originalType. */
        protected int originalType = ORIGINAL_NONE;

        /** Holds value of property originalData. */
        protected byte[] originalData;

        /** The spacing before the image. */
        protected float spacingBefore;

        /** The spacing after the image. */
        protected float spacingAfter;

        /**
        * Holds value of property widthPercentage.
        */
        private float widthPercentage = 100;

        protected IPdfOCG layer;

        private static byte[] skipBuffer = new byte[4096];

        /**
        * Holds value of property initialRotation.
        */
        private float initialRotation;
        
        private static String excUri = " <>#%\"{}[]|\\\u005E\u0060";
        private static String[] excUriEsc = {"%20", "%3C", "%3E", "%23", "%25", "%22", "%7B", "%7D", "%5B", "%5D", "%7C", "%5C", "%5E", "%60"};

        private PdfIndirectReference directReference;

        // constructors
    
        /// <summary>
        /// Constructs an Image-object, using an url.
        /// </summary>
        /// <param name="url">the URL where the image can be found.</param>
        public Image(Uri url) : base(0, 0) {
            this.url = url;
            this.alignment = DEFAULT;
            rotation = 0;
        }

        /// <summary>
        /// Constructs an Image-object, using an url.
        /// </summary>
        /// <param name="image">another Image object.</param>
        protected Image(Image image) : base(image) {
            this.type = image.type;
            this.url = image.url;
            this.alignment = image.alignment;
            this.alt = image.alt;
            this.absoluteX = image.absoluteX;
            this.absoluteY = image.absoluteY;
            this.plainWidth = image.plainWidth;
            this.plainHeight = image.plainHeight;
            this.scaledWidth = image.scaledWidth;
            this.scaledHeight = image.scaledHeight;
            this.rotation = image.rotation;
            this.colorspace = image.colorspace;
            this.rawData = image.rawData;
            this.template = image.template;
            this.bpc = image.bpc;
            this.transparency = image.transparency;
            this.mySerialId = image.mySerialId;
            this.invert = image.invert;
            this.dpiX = image.dpiX;
            this.dpiY = image.dpiY;
            this.mask = image.mask;
            this.imageMask = image.imageMask;
            this.interpolation = image.interpolation;
            this.annotation = image.annotation;
            this.profile = image.profile;
            this.deflated = image.deflated;
            this.additional = image.additional;
            this.smask = image.smask;
            this.XYRatio = image.XYRatio;
            this.originalData = image.originalData;
            this.originalType = image.originalType;
            this.spacingAfter = image.spacingAfter;
            this.spacingBefore = image.spacingBefore;
            this.widthPercentage = image.widthPercentage;
            this.layer = image.layer;
            this.initialRotation = image.initialRotation;
            this.directReference = image.directReference;
        }
    
        /// <summary>
        /// Gets an instance of an Image.
        /// </summary>
        /// <param name="image">an Image</param>
        /// <returns>an object of type Gif, Jpeg or Png</returns>
        public static Image GetInstance(Image image) {
            if (image == null)
                return null;
            return (Image)image.GetType().GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] {typeof(Image)}, null).Invoke(new object[] {image});
        }

        /// <summary>
        /// Gets an instance of an Image.
        /// </summary>
        /// <param name="url">an URL</param>
        /// <returns>an object of type Gif, Jpeg or Png</returns>
        public static Image GetInstance(Uri url) {
            Stream istr = null;
            try {
//NETCF does not know fileWebRequest (file://...)
#if !NETCF
                WebRequest w = WebRequest.Create(url);
                istr = w.GetResponse().GetResponseStream();
#else
                istr = new FileStream(url.LocalPath, FileMode.Open);
#endif
                int c1 = istr.ReadByte();
                int c2 = istr.ReadByte();
                int c3 = istr.ReadByte();
                int c4 = istr.ReadByte();
                istr.Close();

                istr = null;
                if (c1 == 'G' && c2 == 'I' && c3 == 'F') {
                    GifImage gif = new GifImage(url);
                    Image img = gif.GetImage(1);
                    return img;
                }
                if (c1 == 0xFF && c2 == 0xD8) {
                    return new Jpeg(url);
                }
                if (c1 == PngImage.PNGID[0] && c2 == PngImage.PNGID[1]
                        && c3 == PngImage.PNGID[2] && c4 == PngImage.PNGID[3]) {
                    Image img = PngImage.GetImage(url);
                    return img;
                }
                if (c1 == 0xD7 && c2 == 0xCD) {
                    Image img = new ImgWMF(url);
                    return img;
                }
                if (c1 == 'B' && c2 == 'M') {
                    Image img = BmpImage.GetImage(url);
                    return img;
                }
                if ((c1 == 'M' && c2 == 'M' && c3 == 0 && c4 == 42)
                        || (c1 == 'I' && c2 == 'I' && c3 == 42 && c4 == 0)) {
                    RandomAccessFileOrArray ra = null;
                    try
                    {
                        if (url.IsFile)
                        {
                            String file = url.LocalPath;
                            ra = new RandomAccessFileOrArray(file);
                        }
                        else
                            ra = new RandomAccessFileOrArray(url);
                        Image img = TiffImage.GetTiffImage(ra, 1);
                        img.url = url;
                        return img;
                    }
                    catch (InvalidCastException ix)
                    {
                        System.Diagnostics.Debug.WriteLine("InvalidCastException in TIFF adding: " + ix.Message + ":" + ix.StackTrace);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception in TIFF adding:" + ex.Message + ":" + ex.InnerException.Message);
                    }
                    finally {
                        if (ra != null)
                            ra.Close();
                    }

                }
                throw new IOException(url.ToString()
                        + " is not a recognized imageformat.");
            } finally {
                if (istr != null) {
                    istr.Close();
                }
            }
        }

        /// <summary>
        /// Gets an instance of an Image.
        /// </summary>
        /// <param name="url">an URL</param>
        /// <returns>an object of type Gif, Jpeg or Png</returns>
        public static Image GetLocalInstance(string sFileName)
        {
            Stream istr = null;
            try
            {
#if !NETCF
                WebRequest w = WebRequest.Create(url);
                istr = w.GetResponse().GetResponseStream();
#else
                istr = new FileStream(sFileName, FileMode.Open);
#endif
                //istr = w.GetResponse().GetResponseStream();
                int c1 = istr.ReadByte();
                int c2 = istr.ReadByte();
                int c3 = istr.ReadByte();
                int c4 = istr.ReadByte();
                istr.Close();

                istr = null;
                if (c1 == 'G' && c2 == 'I' && c3 == 'F')
                {
                    GifImage gif = new GifImage(sFileName);
                    Image img = gif.GetImage(1);
                    return img;
                }
                if (c1 == 0xFF && c2 == 0xD8)
                {
                    return new Jpeg(sFileName);
                }
                if (c1 == PngImage.PNGID[0] && c2 == PngImage.PNGID[1]
                        && c3 == PngImage.PNGID[2] && c4 == PngImage.PNGID[3])
                {
                    Image img = PngImage.GetImage(sFileName);
                    return img;
                }
                if (c1 == 0xD7 && c2 == 0xCD)
                {
                    Image img = new ImgWMF(sFileName);
                    return img;
                }
                if (c1 == 'B' && c2 == 'M')
                {
                    Image img = BmpImage.GetImage(sFileName);
                    return img;
                }
                if ((c1 == 'M' && c2 == 'M' && c3 == 0 && c4 == 42)
                        || (c1 == 'I' && c2 == 'I' && c3 == 42 && c4 == 0))
                {
                    RandomAccessFileOrArray ra = null;
                    try
                    {
                            ra = new RandomAccessFileOrArray(sFileName);
                        Image img = TiffImage.GetTiffImage(ra, 1);
                        img.url = new Uri(sFileName);
                        return img;
                    }
                    finally
                    {
                        if (ra != null)
                            ra.Close();
                    }

                }
                throw new IOException(sFileName.ToString()
                        + " is not a recognized imageformat.");
            }
            finally
            {
                if (istr != null)
                {
                    istr.Close();
                }
            }
        }

        public static Image GetInstance(Stream s) {
            byte[] a = RandomAccessFileOrArray.InputStreamToArray(s);
            s.Close();
            return GetInstance(a);
        }

        /// <summary>
        /// Gets an instance of an Image.
        /// </summary>
        /// <param name="img">a byte array</param>
        /// <returns>an object of type Gif, Jpeg or Png</returns>
        public static Image GetInstance(byte[] imgb) {
            int c1 = imgb[0];
            int c2 = imgb[1];
            int c3 = imgb[2];
            int c4 = imgb[3];

            if (c1 == 'G' && c2 == 'I' && c3 == 'F') {
                GifImage gif = new GifImage(imgb);
                return gif.GetImage(1);
            }
            if (c1 == 0xFF && c2 == 0xD8) {
                return new Jpeg(imgb);
            }
            if (c1 == PngImage.PNGID[0] && c2 == PngImage.PNGID[1]
                    && c3 == PngImage.PNGID[2] && c4 == PngImage.PNGID[3]) {
                return PngImage.GetImage(imgb);
            }
            if (c1 == 0xD7 && c2 == 0xCD) {
                return new ImgWMF(imgb);
            }
            if (c1 == 'B' && c2 == 'M') {
                return BmpImage.GetImage(imgb);
            }
            if ((c1 == 'M' && c2 == 'M' && c3 == 0 && c4 == 42)
                    || (c1 == 'I' && c2 == 'I' && c3 == 42 && c4 == 0)) {
                RandomAccessFileOrArray ra = null;
                try {
                    ra = new RandomAccessFileOrArray(imgb);
                    Image img = TiffImage.GetTiffImage(ra, 1);
                    if (img.OriginalData == null)
                        img.OriginalData = imgb;

                    return img;
                } finally {
                    if (ra != null)
                        ra.Close();
                }

            }
            throw new IOException("The byte array is not a recognized imageformat.");
        }
        /// <summary>
        /// Converts a .NET image to a Native(PNG, JPG, GIF, WMF) image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public static Image GetInstance(System.Drawing.Image image, System.Drawing.Imaging.ImageFormat format) {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, format);
            return GetInstance(ms.ToArray());
        }
    
        /// <summary>
        /// Gets an instance of an Image from a System.Drwaing.Image.
        /// </summary>
        /// <param name="image">the System.Drawing.Image to convert</param>
        /// <param name="color">
        /// if different from null the transparency
        /// pixels are replaced by this color
        /// </param>
        /// <param name="forceBW">if true the image is treated as black and white</param>
        /// <returns>an object of type ImgRaw</returns>
        public static Image GetInstance(System.Drawing.Image image, Color color, bool forceBW) {
            System.Drawing.Bitmap bm = (System.Drawing.Bitmap)image;
            int w = bm.Width;
            int h = bm.Height;
            int pxv = 0;
            if (forceBW) {
                int byteWidth = (w / 8) + ((w & 7) != 0 ? 1 : 0);
                byte[] pixelsByte = new byte[byteWidth * h];
            
                int index = 0;
                int size = h * w;
                int transColor = 1;
                if (color != null) {
                    transColor = (color.R + color.G + color.B < 384) ? 0 : 1;
                }
                int[] transparency = null;
                int cbyte = 0x80;
                int wMarker = 0;
                int currByte = 0;
                if (color != null) {
                    for (int j = 0; j < h; j++) {
                        for (int i = 0; i < w; i++) {
                            int alpha = bm.GetPixel(i, j).A;
                            if (alpha < 250) {
                                if (transColor == 1)
                                    currByte |= cbyte;
                            }
                            else {
                                if ((bm.GetPixel(i, j).ToArgb() & 0x888) != 0)
                                    currByte |= cbyte;
                            }
                            cbyte >>= 1;
                            if (cbyte == 0 || wMarker + 1 >= w) {
                                pixelsByte[index++] = (byte)currByte;
                                cbyte = 0x80;
                                currByte = 0;
                            }
                            ++wMarker;
                            if (wMarker >= w)
                                wMarker = 0;
                        }
                    }
                }
                else {
                    for (int j = 0; j < h; j++) {
                        for (int i = 0; i < w; i++) {
                            if (transparency == null) {
                                int alpha = bm.GetPixel(i, j).A;
                                if (alpha == 0) {
                                    transparency = new int[2];
                                    transparency[0] = transparency[1] = ((bm.GetPixel(i, j).ToArgb() & 0x888) != 0) ? 1 : 0;
                                }
                            }
                            if ((bm.GetPixel(i, j).ToArgb() & 0x888) != 0)
                                currByte |= cbyte;
                            cbyte >>= 1;
                            if (cbyte == 0 || wMarker + 1 >= w) {
                                pixelsByte[index++] = (byte)currByte;
                                cbyte = 0x80;
                                currByte = 0;
                            }
                            ++wMarker;
                            if (wMarker >= w)
                                wMarker = 0;
                        }
                    }
                }
                return Image.GetInstance(w, h, 1, 1, pixelsByte, transparency);
            }
            else {
                byte[] pixelsByte = new byte[w * h * 3];
                byte[] smask = null;
            
                int index = 0;
                int size = h * w;
                int red = 255;
                int green = 255;
                int blue = 255;
                if (color != null) {
                    red = color.R;
                    green = color.G;
                    blue = color.B;
                }
                int[] transparency = null;
                if (color != null) {
                    for (int j = 0; j < h; j++) {
                        for (int i = 0; i < w; i++) {
                            int alpha = (bm.GetPixel(i, j).ToArgb() >> 24) & 0xff;
                            if (alpha < 250) {
                                pixelsByte[index++] = (byte) red;
                                pixelsByte[index++] = (byte) green;
                                pixelsByte[index++] = (byte) blue;
                            }
                            else {
                                pxv = bm.GetPixel(i, j).ToArgb();
                                pixelsByte[index++] = (byte) ((pxv >> 16) & 0xff);
                                pixelsByte[index++] = (byte) ((pxv >> 8) & 0xff);
                                pixelsByte[index++] = (byte) ((pxv) & 0xff);
                            }
                        }
                    }
                }
                else {
                    int transparentPixel = 0;
                    smask = new byte[w * h];
                    bool shades = false;
                    int smaskPtr = 0;
                    for (int j = 0; j < h; j++) {
                        for (int i = 0; i < w; i++) {
                            pxv = bm.GetPixel(i, j).ToArgb();
                            byte alpha = smask[smaskPtr++] = (byte) ((pxv >> 24) & 0xff);
                            /* bugfix by Chris Nokleberg */
                            if (!shades) {
                                if (alpha != 0 && alpha != 255) {
                                    shades = true;
                                } else if (transparency == null) {
                                    if (alpha == 0) {
                                        transparentPixel = pxv & 0xffffff;
                                        transparency = new int[6];
                                        transparency[0] = transparency[1] = (transparentPixel >> 16) & 0xff;
                                        transparency[2] = transparency[3] = (transparentPixel >> 8) & 0xff;
                                        transparency[4] = transparency[5] = transparentPixel & 0xff;
                                    }
                                } else if ((pxv & 0xffffff) != transparentPixel) {
                                    shades = true;
                                }
                            }
                            pixelsByte[index++] = (byte) ((pxv >> 16) & 0xff);
                            pixelsByte[index++] = (byte) ((pxv >> 8) & 0xff);
                            pixelsByte[index++] = (byte) (pxv & 0xff);
                        }
                    }
                    if (shades)
                        transparency = null;
                    else
                        smask = null;
                }
                Image img = Image.GetInstance(w, h, 3, 8, pixelsByte, transparency);
                if (smask != null) {
                    Image sm = Image.GetInstance(w, h, 1, 8, smask);
                    sm.MakeMask();
                    img.ImageMask = sm;
                }
                return img;
            }
        }
    
        /// <summary>
        /// Gets an instance of an Image from a System.Drawing.Image.
        /// </summary>
        /// <param name="image">the System.Drawing.Image to convert</param>
        /// <param name="color">
        /// if different from null the transparency
        /// pixels are replaced by this color
        /// </param>
        /// <returns>an object of type ImgRaw</returns>
        public static Image GetInstance(System.Drawing.Image image, Color color) {
            return Image.GetInstance(image, color, false);
        }
    
        /// <summary>
        /// Gets an instance of an Image.
        /// </summary>
        /// <param name="filename">a filename</param>
        /// <returns>an object of type Gif, Jpeg or Png</returns>
        public static Image GetInstance(string filename) {
            return GetInstance(ToURL(filename));
        }
    
        /// <summary>
        /// Gets an instance of an Image in raw mode.
        /// </summary>
        /// <param name="width">the width of the image in pixels</param>
        /// <param name="height">the height of the image in pixels</param>
        /// <param name="components">1,3 or 4 for GrayScale, RGB and CMYK</param>
        /// <param name="bpc">bits per component</param>
        /// <param name="data">the image data</param>
        /// <returns>an object of type ImgRaw</returns>
        public static Image GetInstance(int width, int height, int components, int bpc, byte[] data) {
            return Image.GetInstance(width, height, components, bpc, data, null);
        }
    
        /**
        * Reuses an existing image.
        * @param ref the reference to the image dictionary
        * @throws BadElementException on error
        * @return the image
        */    
        public static Image GetInstance(PRIndirectReference iref) {
            PdfDictionary dic = (PdfDictionary)PdfReader.GetPdfObjectRelease(iref);
            int width = ((PdfNumber)PdfReader.GetPdfObjectRelease(dic.Get(PdfName.WIDTH))).IntValue;
            int height = ((PdfNumber)PdfReader.GetPdfObjectRelease(dic.Get(PdfName.HEIGHT))).IntValue;
            Image imask = null;
            PdfObject obj = dic.Get(PdfName.SMASK);
            if (obj != null && obj.IsIndirect()) {
                imask = GetInstance((PRIndirectReference)obj);
            }
            else {
                obj = dic.Get(PdfName.MASK);
                if (obj != null && obj.IsIndirect()) {
                    PdfObject obj2 = PdfReader.GetPdfObjectRelease(obj);
                    if (obj2 is PdfDictionary)
                        imask = GetInstance((PRIndirectReference)obj);
                }
            }
            Image img = new ImgRaw(width, height, 1, 1, null);
            img.imageMask = imask;
            img.directReference = iref;
            return img;
        }
        
        /// <summary>
        /// Gets an instance of an Image in raw mode.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public static Image GetInstance(PdfTemplate template) {
            return new ImgTemplate(template);
        }
    
        /// <summary>
        /// Gets an instance of an Image in raw mode.
        /// </summary>
        /// <param name="width">the width of the image in pixels</param>
        /// <param name="height">the height of the image in pixels</param>
        /// <param name="reverseBits"></param>
        /// <param name="typeCCITT"></param>
        /// <param name="parameters"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Image GetInstance(int width, int height, bool reverseBits, int typeCCITT, int parameters, byte[] data) {
            return Image.GetInstance(width, height, reverseBits, typeCCITT, parameters, data, null);
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="reverseBits"></param>
        /// <param name="typeCCITT"></param>
        /// <param name="parameters"></param>
        /// <param name="data"></param>
        /// <param name="transparency"></param>
        /// <returns></returns>
        public static Image GetInstance(int width, int height, bool reverseBits, int typeCCITT, int parameters, byte[] data, int[] transparency) {
            if (transparency != null && transparency.Length != 2)
                throw new BadElementException("Transparency length must be equal to 2 with CCITT images");
            Image img = new ImgCCITT(width, height, reverseBits, typeCCITT, parameters, data);
            img.transparency = transparency;
            return img;
        }

        /// <summary>
        /// Gets an instance of an Image in raw mode.
        /// </summary>
        /// <param name="width">the width of the image in pixels</param>
        /// <param name="height">the height of the image in pixels</param>
        /// <param name="components">1,3 or 4 for GrayScale, RGB and CMYK</param>
        /// <param name="bpc">bits per component</param>
        /// <param name="data">the image data</param>
        /// <param name="transparency">
        /// transparency information in the Mask format of the
        /// image dictionary
        /// </param>
        /// <returns>an object of type ImgRaw</returns>
        public static Image GetInstance(int width, int height, int components, int bpc, byte[] data, int[] transparency) {
            if (transparency != null && transparency.Length != components * 2)
                throw new BadElementException("Transparency length must be equal to (componentes * 2)");
            if (components == 1 && bpc == 1) {
                byte[] g4 = CCITTG4Encoder.Compress(data, width, height);
                return Image.GetInstance(width, height, false, Element.CCITTG4, Element.CCITT_BLACKIS1, g4, transparency);
            }
            Image img = new ImgRaw(width, height, components, bpc, data);
            img.transparency = transparency;
            return img;
        }
    
        /// <summary>
        /// Returns an Image that has been constructed taking in account
        /// the value of some attributes.
        /// </summary>
        /// <param name="attributes">Some attributes</param>
        /// <returns>an Image</returns>
        public static Image GetInstance(Properties attributes) {
            string value = attributes.Remove(ElementTags.URL);
            if (value == null) throw new Exception("The URL of the image is missing.");
            Image image = Image.GetInstance(value);
            int align = Element.ALIGN_LEFT;
            if ((value = attributes.Remove(ElementTags.ALIGN)) != null) {
                if (ElementTags.ALIGN_LEFT.ToLower().Equals(value)) align |= Image.LEFT_ALIGN;
                else if (ElementTags.ALIGN_RIGHT.ToLower().Equals(value)) align |= Image.RIGHT_ALIGN;
                else if (ElementTags.ALIGN_MIDDLE.ToLower().Equals(value)) align |= Image.MIDDLE_ALIGN;
            }
            if ((value = attributes.Remove(ElementTags.UNDERLYING)) != null) {
                if (bool.Parse(value)) align |= Image.UNDERLYING;
            }
            if ((value = attributes.Remove(ElementTags.TEXTWRAP)) != null) {
                if (bool.Parse(value)) align |= Image.TEXTWRAP;
            }
            image.alignment = align;
            if ((value = attributes.Remove(ElementTags.ALT)) != null) {
                image.Alt = value;
            }
            string x;
            string y;
            if (((x = attributes.Remove(ElementTags.ABSOLUTEX)) != null)
                && ((y = attributes.Remove(ElementTags.ABSOLUTEY)) != null)) {
                image.SetAbsolutePosition(float.Parse(x), float.Parse(y, System.Globalization.NumberFormatInfo.InvariantInfo));
            }
            if ((value = attributes.Remove(ElementTags.PLAINWIDTH)) != null) {
                image.ScaleAbsoluteWidth(float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo));
            }
            if ((value = attributes.Remove(ElementTags.PLAINHEIGHT)) != null) {
                image.ScaleAbsoluteHeight(float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo));
            }
            if ((value = attributes.Remove(ElementTags.ROTATION)) != null) {
                image.Rotation = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            return image;
        }
    
        // methods to set information
    
        /// <summary>
        /// Sets the absolute position of the Image.
        /// </summary>
        /// <param name="absoluteX"></param>
        /// <param name="absoluteY"></param>
        public void SetAbsolutePosition(float absoluteX, float absoluteY) {
            this.absoluteX = absoluteX;
            this.absoluteY = absoluteY;
        }
    
        /// <summary>
        /// Scale the image to an absolute width and an absolute height.
        /// </summary>
        /// <param name="newWidth">the new width</param>
        /// <param name="newHeight">the new height</param>
        public void ScaleAbsolute(float newWidth, float newHeight) {
            plainWidth = newWidth;
            plainHeight = newHeight;
            float[] matrix = this.Matrix;
            scaledWidth = matrix[DX] - matrix[CX];
            scaledHeight = matrix[DY] - matrix[CY];
        }
    
        /// <summary>
        /// Scale the image to an absolute width.
        /// </summary>
        /// <param name="newWidth">the new width</param>
        public void ScaleAbsoluteWidth(float newWidth) {
            plainWidth = newWidth;
            float[] matrix = this.Matrix;
            scaledWidth = matrix[DX] - matrix[CX];
            scaledHeight = matrix[DY] - matrix[CY];
        }
    
        /// <summary>
        /// Scale the image to an absolute height.
        /// </summary>
        /// <param name="newHeight">the new height</param>
        public void ScaleAbsoluteHeight(float newHeight) {
            plainHeight = newHeight;
            float[] matrix = Matrix;
            scaledWidth = matrix[DX] - matrix[CX];
            scaledHeight = matrix[DY] - matrix[CY];
        }
    
        /// <summary>
        /// Scale the image to a certain percentage.
        /// </summary>
        /// <param name="percent">the scaling percentage</param>
        public void ScalePercent(float percent) {
            ScalePercent(percent, percent);
        }
    
        /// <summary>
        /// Scale the width and height of an image to a certain percentage.
        /// </summary>
        /// <param name="percentX">the scaling percentage of the width</param>
        /// <param name="percentY">the scaling percentage of the height</param>
        public void ScalePercent(float percentX, float percentY) {
            plainWidth = (this.Width * percentX) / 100f;
            plainHeight = (this.Height * percentY) / 100f;
            float[] matrix = Matrix;
            scaledWidth = matrix[DX] - matrix[CX];
            scaledHeight = matrix[DY] - matrix[CY];
        }
    
        /// <summary>
        /// Scales the image so that it fits a certain width and height.
        /// </summary>
        /// <param name="fitWidth">the width to fit</param>
        /// <param name="fitHeight">the height to fit</param>
        public void ScaleToFit(float fitWidth, float fitHeight) {
            ScalePercent(100);
            float percentX = (fitWidth * 100) / this.ScaledWidth;
            float percentY = (fitHeight * 100) / this.ScaledHeight;
            ScalePercent(percentX < percentY ? percentX : percentY);
        }
    
        /**
        * Gets the current image rotation in radians.
        * @return the current image rotation in radians
        */
        public float GetImageRotation() {
            float rot = (float) ((rotation - initialRotation) % (2.0 * Math.PI));
            if (rot < 0) {
                rot += (float)(2.0 * Math.PI);
            }
            return rot;
        }

        /// <summary>
        /// Sets the rotation of the image in radians.
        /// </summary>
        /// <param name="r">rotation in radians</param>
        public new float Rotation {
            set {
                double d=Math.PI;                  //__IDS__
                rotation = (float) ((value + initialRotation) % (2.0 * d)); //__IDS__
                if (rotation < 0) {
                    rotation += (float)(2.0 * d);           //__IDS__
                }
                float[] matrix = Matrix;
                scaledWidth = matrix[DX] - matrix[CX];
                scaledHeight = matrix[DY] - matrix[CY];
            }
        }
    
        /// <summary>
        /// Sets the rotation of the image in degrees.
        /// </summary>
        /// <param name="deg">rotation in degrees</param>
        public float RotationDegrees {
            set {
                Rotation = (value / 180 * (float)Math.PI); //__IDS__
            }
        }
    
        /// <summary>
        /// Get/set the annotation.
        /// </summary>
        /// <value>the Annotation</value>
        public Annotation Annotation {
            get {
                return annotation;
            }

            set {
                this.annotation = value;
            }
        }
    
        // methods to retrieve information
    
        /// <summary>
        /// Gets the bpc for the image.
        /// </summary>
        /// <remarks>
        /// this only makes sense for Images of the type RawImage.
        /// </remarks>
        /// <value>a bpc value</value>
        public int Bpc {
            get {
                return bpc;
            }
        }
    
        /// <summary>
        /// Gets the raw data for the image.
        /// </summary>
        /// <remarks>
        /// this only makes sense for Images of the type RawImage.
        /// </remarks>
        /// <value>the raw data</value>
        public byte[] RawData {
            get {
                return rawData;
            }
        }
    
        /// <summary>
        /// Get/set the template to be used as an image.
        /// </summary>
        /// <remarks>
        /// this only makes sense for Images of the type ImgTemplate.
        /// </remarks>
        /// <value>the template</value>
        public PdfTemplate TemplateData {
            get {
                return template[0];
            }

            set {
                this.template[0] = value;
            }
        }
    
        /// <summary>
        /// Checks if the Images has to be added at an absolute position.
        /// </summary>
        /// <returns>a bool</returns>
        public bool HasAbsolutePosition() {
            return !float.IsNaN(absoluteY);
        }
    
        /// <summary>
        /// Checks if the Images has to be added at an absolute X position.
        /// </summary>
        /// <returns>a bool</returns>
        public bool HasAbsoluteX() {
            return !float.IsNaN(absoluteX);
        }
    
        /// <summary>
        /// Returns the absolute X position.
        /// </summary>
        /// <value>a position</value>
        public float AbsoluteX {
            get {
                return absoluteX;
            }
        }
    
        /// <summary>
        /// Returns the absolute Y position.
        /// </summary>
        /// <value>a position</value>
        public float AbsoluteY {
            get {
                return absoluteY;
            }
        }
    
        /// <summary>
        /// Returns the type.
        /// </summary>
        /// <value>a type</value>
        public override int Type {
            get {
                return type;
            }
        }
    
        /// <summary>
        /// Returns true if the image is a Jpeg-object.
        /// </summary>
        /// <returns>a bool</returns>
        public bool IsJpeg() {
            return type == Element.JPEG;
        }
    
        /// <summary>
        /// Returns true if the image is a ImgRaw-object.
        /// </summary>
        /// <returns>a bool</returns>
        public bool IsImgRaw() {
            return type == Element.IMGRAW;
        }

        /// <summary>
        /// Returns true if the image is an ImgTemplate-object.
        /// </summary>
        /// <returns>a bool</returns>
        public bool IsImgTemplate() {
            return type == Element.IMGTEMPLATE;
        }
    
        /// <summary>
        /// Gets the string-representation of the reference to the image.
        /// </summary>
        /// <value>a string</value>
        public Uri Url {
            get {
                return url;
            }
            set {
                url = value;
            }
        }
    
        /// <summary>
        /// Get/set the alignment for the image.
        /// </summary>
        /// <value>a value</value>
        public int Alignment {
            get {
                return alignment;
            }

            set {
                this.alignment = value;
            }
        }
    
        /// <summary>
        /// Get/set the alternative text for the image.
        /// </summary>
        /// <value>a string</value>
        public string Alt {
            get {
                return alt;
            }

            set {
                this.alt = value;
            }
        }
    
        /// <summary>
        /// Gets the scaled width of the image.
        /// </summary>
        /// <value>a value</value>
        public float ScaledWidth {
            get {
                return scaledWidth;
            }
        }
    
        /// <summary>
        /// Gets the scaled height of the image.
        /// </summary>
        /// <value>a value</value>
        public float ScaledHeight {
            get {
                return scaledHeight;
            }
        }
    
        /// <summary>
        /// Gets the colorspace for the image.
        /// </summary>
        /// <remarks>
        /// this only makes sense for Images of the type Jpeg.
        /// </remarks>
        /// <value>a colorspace value</value>
        public int Colorspace {
            get {
                return colorspace;
            }
        }
    
        /// <summary>
        /// Returns the transformation matrix of the image.
        /// </summary>
        /// <value>an array [AX, AY, BX, BY, CX, CY, DX, DY]</value>
        public float[] Matrix {
            get {
                float[] matrix = new float[8];
                float cosX = (float)Math.Cos(rotation);
                float sinX = (float)Math.Sin(rotation);
                matrix[AX] = plainWidth * cosX;
                matrix[AY] = plainWidth * sinX;
                matrix[BX] = (- plainHeight) * sinX;
                matrix[BY] = plainHeight * cosX;
                if (rotation < Math.PI / 2f) {
                    matrix[CX] = matrix[BX];
                    matrix[CY] = 0;
                    matrix[DX] = matrix[AX];
                    matrix[DY] = matrix[AY] + matrix[BY];
                }
                else if (rotation < Math.PI) {
                    matrix[CX] = matrix[AX] + matrix[BX];
                    matrix[CY] = matrix[BY];
                    matrix[DX] = 0;
                    matrix[DY] = matrix[AY];
                }
                else if (rotation < Math.PI * 1.5f) {
                    matrix[CX] = matrix[AX];
                    matrix[CY] = matrix[AY] + matrix[BY];
                    matrix[DX] = matrix[BX];
                    matrix[DY] = 0;
                }
                else {
                    matrix[CX] = 0;
                    matrix[CY] = matrix[AY];
                    matrix[DX] = matrix[AX] + matrix[BX];
                    matrix[DY] = matrix[BY];
                }
                return matrix;
            }
        }
    
        /// <summary>
        /// This method is an alternative for the Stream.Skip()-method
        /// that doesn't seem to work properly for big values of size.
        /// </summary>
        /// <param name="istr">the stream</param>
        /// <param name="size">the number of bytes to skip</param>
        public static void Skip(Stream istr, int size) {
            while (size > 0) {
                int r = istr.Read(skipBuffer, 0, Math.Min(skipBuffer.Length, size));
                if (r <= 0)
                    return;
                size -= r;
            }
        }
    
        /// <summary>
        /// This method makes a valid URL from a given filename.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="filename">a given filename</param>
        /// <returns>a valid URL</returns>
        public static Uri ToURL(string filename) {
            if (filename.StartsWith("file:/") || filename.StartsWith("http://") || filename.StartsWith("https://")) {
                return new Uri(filename);
            }
            FileInfo f = new FileInfo(filename);
            string path = f.FullName;
            if (Path.DirectorySeparatorChar != '/') {
                StringBuilder sr = new StringBuilder(path);
                sr.Replace(Path.DirectorySeparatorChar, '/');
                path = sr.ToString();
            }
            if (!path.StartsWith("/")) {
                path = "/" + path;
            }
            char[] t = path.ToCharArray();
            StringBuilder sb = new StringBuilder();
            foreach (char c in t) {
                int a = excUri.IndexOf(c);
                if (a >= 0)
                    sb.Append(excUriEsc[a]);
                else
                    sb.Append(c);
            }
//#if NETCF
            //No drive letters in NETCF
            //return new Uri("file:/" + sb.ToString());
//#else
            //return new Uri("file:///" + sb.ToString());
            return new Uri("file://" + sb.ToString());
//#endif
        }
    
        /**
        * Unescapes an URL. All the "%xx" are replaced by the 'xx' hex char value.
        * @param src the url to unescape
        * @return the eunescaped value
        */    
        public static String UnEscapeURL(String src) {
            StringBuilder bf = new StringBuilder();
            char[] s = src.ToCharArray();
            for (int k = 0; k < s.Length; ++k) {
                char c = s[k];
                if (c == '%') {
                    if (k + 2 >= s.Length) {
                        bf.Append(c);
                        continue;
                    }
                    int a0 = PRTokeniser.GetHex((int)s[k + 1]);
                    int a1 = PRTokeniser.GetHex((int)s[k + 2]);
                    if (a0 < 0 || a1 < 0) {
                        bf.Append(c);
                        continue;
                    }
                    bf.Append((char)(a0 * 16 + a1));
                    k += 2;
                }
                else
                    bf.Append(c);
            }
            return bf.ToString();
        }
        
        /// <summary>
        /// Returns the transparency.
        /// </summary>
        /// <value>the transparency</value>
        public int[] Transparency {
            get {
                return transparency;
            }
            set {
                transparency = value;
            }
        }
    
        /// <summary>
        /// Checks if a given tag corresponds with this object.
        /// </summary>
        /// <param name="tag">the given tag</param>
        /// <returns>true if the tag corresponds</returns>
        public static bool IsTag(string tag) {
            return ElementTags.IMAGE.Equals(tag);
        }
    
        /// <summary>
        /// Gets the plain width of the image.
        /// </summary>
        /// <value>a value</value>
        public float PlainWidth {
            get {
                return plainWidth;
            }
        }
    
        /// <summary>
        /// Gets the plain height of the image.
        /// </summary>
        /// <value>a value</value>
        public float PlainHeight {
            get {
                return plainHeight;
            }
        }
    
        /// <summary>
        /// generates new serial id
        /// </summary>
        static protected long GetSerialId() {
            lock (serialId) {
                serialId = (long)serialId + 1L;
                return (long)serialId;
            }
        }
    
        /// <summary>
        /// returns serial id for this object
        /// </summary>
        public long MySerialId {
            get {
                return mySerialId;
            }
        }
    
        /// <summary>
        /// Gets the dots-per-inch in the X direction. Returns 0 if not available.
        /// </summary>
        /// <value>the dots-per-inch in the X direction</value>
        public int DpiX {
            get {
                return dpiX;
            }
        }
    
        /// <summary>
        /// Gets the dots-per-inch in the Y direction. Returns 0 if not available.
        /// </summary>
        /// <value>the dots-per-inch in the Y direction</value>
        public int DpiY {
            get {
                return dpiY;
            }
        }
    
        /**
        * Sets the dots per inch value
        * 
        * @param dpiX
        *            dpi for x coordinates
        * @param dpiY
        *            dpi for y coordinates
        */
        public void SetDpi(int dpiX, int dpiY) {
            this.dpiX = dpiX;
            this.dpiY = dpiY;
        }

        /// <summary>
        /// Returns true if this Image has the
        /// requisites to be a mask.
        /// </summary>
        /// <returns>true if this Image can be a mask</returns>
        public bool IsMaskCandidate() {
            if (type == Element.IMGRAW) {
                if (bpc > 0xff)
                    return true;
            }
            return colorspace == 1;
        }
    
        /// <summary>
        /// Make this Image a mask.
        /// </summary>
        public void MakeMask() {
            if (!IsMaskCandidate())
                throw new DocumentException("This image can not be an image mask.");
            mask = true;
        }
    
        /// <summary>
        /// Get/set the explicit masking.
        /// </summary>
        /// <value>the explicit masking</value>
        public Image ImageMask {
            get {
                return imageMask;
            }

            set {
                if (this.mask)
                    throw new DocumentException("An image mask cannot contain another image mask.");
                if (!value.mask)
                    throw new DocumentException("The image mask is not a mask. Did you do MakeMask()?");
                imageMask = value;
                smask = (value.bpc > 1 && value.bpc <= 8);
            }
        }
    
        /// <summary>
        /// Returns true if this Image is a mask.
        /// </summary>
        /// <returns>true if this Image is a mask</returns>
        public bool IsMask() {
            return mask;
        }
    
        /// <summary>
        /// Inverts the meaning of the bits of a mask.
        /// </summary>
        /// <value>true to invert the meaning of the bits of a mask</value>
        public bool Inverted {
            set {
                this.invert = value;
            }
            get {
                return this.invert;
            }
        }
    
        /// <summary>
        /// Sets the image interpolation. Image interpolation attempts to
        /// produce a smooth transition between adjacent sample values.
        /// </summary>
        /// <value>New value of property interpolation.</value>
        public bool Interpolation {
            set {
                this.interpolation = value;
            }
            get {
                return this.interpolation;
            }
        }
    
        /** Tags this image with an ICC profile.
         * @param profile the profile
         */    
        public ICC_Profile TagICC {
            get {
                return profile;
            }
            set {
                this.profile = value;
            }
        }
    
        /** Checks is the image has an ICC profile.
         * @return the ICC profile or null
         */    
        public bool HasICCProfile() {
            return (this.profile != null);
        }

        public bool Deflated {
            get {
                return deflated;
            }
            set {
                deflated = value;
            }
        }

        public PdfDictionary Additional {
            get {
                return additional;
            }
            set {
                additional = value;
            }
        }

        public bool Smask {
            get {
                return smask;
            }
            set {
                smask = value;
            }
        }

        public float XYRatio {
            get {
                return xyRatio;
            }
            set {
                xyRatio = value;
            }
        }

        public float IndentationLeft {
            get {
                return indentationLeft;
            }
            set {
                indentationLeft = value;
            }
        }

        public float IndentationRight {
            get {
                return indentationRight;
            }
            set {
                indentationRight = value;
            }
        }

        public int OriginalType {
            get {
                return originalType;
            }
            set {
                originalType = value;
            }
        }

        public byte[] OriginalData {
            get {
                return originalData;
            }
            set {
                originalData = value;
            }
        }

        public float SpacingBefore {
            get {
                return spacingBefore;
            }
            set {
                spacingBefore = value;
            }
        }

        public float SpacingAfter {
            get {
                return spacingAfter;
            }
            set {
                spacingAfter = value;
            }
        }

        public float WidthPercentage {
            get {
                return widthPercentage;
            }
            set {
                widthPercentage = value;
            }
        }

        public IPdfOCG Layer {
            get {
                return layer;
            }
            set {
                layer = value;
            }
        }

        private PdfObject SimplifyColorspace(PdfObject obj) {
            if (obj == null || !obj.IsArray())
                return obj;
            PdfObject first = (PdfObject)(((PdfArray)obj).ArrayList[0]);
            if (PdfName.CALGRAY.Equals(first))
                return PdfName.DEVICEGRAY;
            else if (PdfName.CALRGB.Equals(first))
                return PdfName.DEVICERGB;
            else
                return obj;
        }

        /**
        * Replaces CalRGB and CalGray colorspaces with DeviceRGB and DeviceGray.
        */    
        public void SimplifyColorspace() {
            if (additional == null)
                return;
            PdfObject value = additional.Get(PdfName.COLORSPACE);
            if (value == null || !value.IsArray())
                return;
            PdfObject cs = SimplifyColorspace(value);
            if (cs.IsName())
                value = cs;
            else {
                PdfObject first = (PdfObject)(((PdfArray)value).ArrayList[0]);
                if (PdfName.INDEXED.Equals(first)) {
                    ArrayList array = ((PdfArray)value).ArrayList;
                    if (array.Count >= 2 && ((PdfObject)array[1]).IsArray()) {
                        array[1] = SimplifyColorspace((PdfObject)array[1]);
                    }
                }
            }
            additional.Put(PdfName.COLORSPACE, value);
        }
        
        /**
        * Some image formats, like TIFF may present the images rotated that have
        * to be compensated.
        */
        public float InitialRotation {
            get {
                return initialRotation;
            }
            set {
                float old_rot = rotation - this.initialRotation;
                this.initialRotation = value;
                Rotation = old_rot;
            }
        }

        public PdfIndirectReference DirectReference {
            set {
                directReference = value;
            }
            get {
                return directReference;
            }
        }
    }
}
