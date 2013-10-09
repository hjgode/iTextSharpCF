using System;
using System.IO;
using System.Collections;
using iTextSharp.text;
/*
 * $Id: PdfSmartCopy.cs,v 1.1 2007/01/24 17:42:19 psoares33 Exp $
 * $Name:  $
 *
 * Copyright 2007 Michael Neuweiler and Bruno Lowagie
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
 * This class was written by Michael Neuweiler based on hints given by Bruno Lowagie
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
namespace iTextSharp.text.pdf {

    /**
    * PdfSmartCopy has the same functionality as PdfCopy,
    * but when resources (such as fonts, images,...) are
    * encountered, a reference to these resources is saved
    * in a cache, so that they can be reused.
    * This requires more memory, but reduces the file size
    * of the resulting PDF document.
    */

    public class PdfSmartCopy : PdfCopy {

        /** the cache with the streams and references. */
        private Hashtable streamMap = null;

        /** Creates a PdfSmartCopy instance. */
        public PdfSmartCopy(Document document, Stream os) : base(document, os) {
            this.streamMap = new Hashtable();
        }
        /**
        * Translate a PRIndirectReference to a PdfIndirectReference
        * In addition, translates the object numbers, and copies the
        * referenced object to the output file if it wasn't available
        * in the cache yet. If it's in the cache, the reference to
        * the already used stream is returned.
        * 
        * NB: PRIndirectReferences (and PRIndirectObjects) really need to know what
        * file they came from, because each file has its own namespace. The translation
        * we do from their namespace to ours is *at best* heuristic, and guaranteed to
        * fail under some circumstances.
        */
        protected override PdfIndirectReference CopyIndirect(PRIndirectReference inp) {
            PdfObject srcObj = PdfReader.GetPdfObjectRelease(inp);
            ByteStore streamKey = null;
            if (srcObj.Type == PdfObject.STREAM) {
                byte[] streamContent = PdfReader.GetStreamBytesRaw((PRStream) srcObj);
                // Only the content is compared, probably the keys should also be compared
                streamKey = new ByteStore(streamContent);
                PdfIndirectReference streamRef = (PdfIndirectReference) streamMap[streamKey];
                if (streamRef != null) {
                    return streamRef;
                }
            }

            PdfIndirectReference theRef;
            RefKey key = new RefKey(inp);
            IndirectReferences iRef = (IndirectReferences) indirects[key];
            if (iRef != null) {
                theRef = iRef.Ref;
                if (iRef.Copied) {
                    return theRef;
                }
            } else {
                theRef = body.PdfIndirectReference;
                iRef = new IndirectReferences(theRef);
                indirects[key] = iRef;
            }
            iRef.SetCopied();

            if (srcObj.Type == PdfObject.STREAM) {
                streamMap[streamKey] = theRef;
            }

            PdfObject obj = CopyObject(srcObj);
            AddToBody(obj, theRef);
            return theRef;
        }

        internal class ByteStore {
            private byte[] b;
            private int hash;
            
            internal ByteStore(byte[] b) {
                this.b = b;
            }

            public override bool Equals(Object obj) {
                if (obj == null || !(obj is ByteStore))
                    return false;
                byte[] b2 = ((ByteStore)obj).b;
                if (b2.Length != b.Length)
                    return false;
                int len = b.Length;
                for (int k = 0; k < len; ++k) {
                    if (b[k] != b2[k])
                        return false;
                }
                return true;
            }

            public override int GetHashCode() {
                if (hash == 0) {
                    int len = b.Length;
                    for (int k = 0; k < len; ++k) {
                        hash = hash * 31 + b[k];
                    }
                }
                return hash;
            }
        }
    }
}