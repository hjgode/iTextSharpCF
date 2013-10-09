using System;
using System.Collections;
using System.IO;
using iTextSharp.text;
/*
 * $Id: PdfCopy.cs,v 1.14 2007/02/09 15:34:38 psoares33 Exp $
 * $Name:  $
 *
 * Copyright 1999, 2000, 2001, 2002 Bruno Lowagie
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
 * This module by Mark Thompson. Copyright (C) 2002 Mark Thompson
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
    * Make copies of PDF documents. Documents can be edited after reading and
    * before writing them out.
    * @author Mark Thompson
    */

    public class PdfCopy : PdfWriter {
        /**
        * This class holds information about indirect references, since they are
        * renumbered by iText.
        */
        internal class IndirectReferences {
            PdfIndirectReference theRef;
            bool hasCopied;
            internal IndirectReferences(PdfIndirectReference refi) {
                theRef = refi;
                hasCopied = false;
            }
            internal void SetCopied() { hasCopied = true; }
            internal bool Copied {
                get {
                    return hasCopied; 
                }
            }
            internal PdfIndirectReference Ref {
                get {
                    return theRef; 
                }
            }
        };
        protected Hashtable indirects;
        protected Hashtable indirectMap;
        protected int currentObjectNum = 1;
        protected PdfReader reader;
        protected PdfIndirectReference acroForm;
        protected PdfIndirectReference topPageParent;
        protected ArrayList pageNumbersToRefs = new ArrayList();
        protected ArrayList newBookmarks;
        
        /**
        * A key to allow us to hash indirect references
        */
        protected class RefKey {
            internal int num;
            internal int gen;
            internal RefKey(int num, int gen) {
                this.num = num;
                this.gen = gen;
            }
            internal RefKey(PdfIndirectReference refi) {
                num = refi.Number;
                gen = refi.Generation;
            }
            internal RefKey(PRIndirectReference refi) {
                num = refi.Number;
                gen = refi.Generation;
            }
            public override int GetHashCode() {
                return (gen<<16)+num;
            }
            public override bool Equals(Object o) {
                if (!(o is RefKey)) return false;
                RefKey other = (RefKey)o;
                return this.gen == other.gen && this.num == other.num;
            }
            public override String ToString() {
                return "" + num + " " + gen;
            }
        }
        
        /**
        * Constructor
        * @param document
        * @param os outputstream
        */
        public PdfCopy(Document document, Stream os) : base(new PdfDocument(), os) {
            document.AddDocListener(pdf);
            pdf.AddWriter(this);
            indirectMap = new Hashtable();
        }
        public override void Open() {
            base.Open();
            topPageParent = PdfIndirectReference;
            root.SetLinearMode(topPageParent);
        }

        /**
        * Grabs a page from the input document
        * @param reader the reader of the document
        * @param pageNumber which page to get
        * @return the page
        */
        public override PdfImportedPage GetImportedPage(PdfReader reader, int pageNumber) {
            if (currentPdfReaderInstance != null) {
                if (currentPdfReaderInstance.Reader != reader) {
                    try {
                        currentPdfReaderInstance.Reader.Close();
                        currentPdfReaderInstance.ReaderFile.Close();
                    }
                    catch (IOException) {
                        // empty on purpose
                    }
                    currentPdfReaderInstance = reader.GetPdfReaderInstance(this);
                }
            }
            else {
                currentPdfReaderInstance = reader.GetPdfReaderInstance(this);
            }
            return currentPdfReaderInstance.GetImportedPage(pageNumber);            
        }
        
        
        /**
        * Translate a PRIndirectReference to a PdfIndirectReference
        * In addition, translates the object numbers, and copies the
        * referenced object to the output file.
        * NB: PRIndirectReferences (and PRIndirectObjects) really need to know what
        * file they came from, because each file has its own namespace. The translation
        * we do from their namespace to ours is *at best* heuristic, and guaranteed to
        * fail under some circumstances.
        */
        protected virtual PdfIndirectReference CopyIndirect(PRIndirectReference inp) {
            PdfIndirectReference theRef;
            RefKey key = new RefKey(inp);
            IndirectReferences iRef = (IndirectReferences)indirects[key] ;
            if (iRef != null) {
                theRef = iRef.Ref;
                if (iRef.Copied) {
                    return theRef;
                }
            }
            else {
                theRef = body.PdfIndirectReference;
                iRef = new IndirectReferences(theRef);
                indirects[key] =  iRef;
            }
            iRef.SetCopied();
            PdfObject obj = CopyObject(PdfReader.GetPdfObjectRelease(inp));
            AddToBody(obj, theRef);
            return theRef;
        }
        
        /**
        * Translate a PRDictionary to a PdfDictionary. Also translate all of the
        * objects contained in it.
        */
        protected PdfDictionary CopyDictionary(PdfDictionary inp) {
            PdfDictionary outp = new PdfDictionary();
            PdfName type = (PdfName)inp.Get(PdfName.TYPE);
            
            foreach (PdfName key in inp.Keys) {
                PdfObject value = inp.Get(key);
                if (type != null && PdfName.PAGE.Equals(type)) {
                    if (key.Equals(PdfName.PARENT))
                        outp.Put(PdfName.PARENT, topPageParent);
                    else if (!key.Equals(PdfName.B))
                        outp.Put(key, CopyObject(value));
                }
                else
                    outp.Put(key, CopyObject(value));
            }
            return outp;
        }
        
        /**
        * Translate a PRStream to a PdfStream. The data part copies itself.
        */
        protected PdfStream CopyStream(PRStream inp) {
            PRStream outp = new PRStream(inp, null);
            
            foreach (PdfName key in inp.Keys) {
                PdfObject value = inp.Get(key);
                outp.Put(key, CopyObject(value));
            }
            
            return outp;
        }
        
        
        /**
        * Translate a PRArray to a PdfArray. Also translate all of the objects contained
        * in it
        */
        protected PdfArray CopyArray(PdfArray inp) {
            PdfArray outp = new PdfArray();
            
            foreach (PdfObject value in inp.ArrayList) {
                outp.Add(CopyObject(value));
            }
            return outp;
        }
        
        /**
        * Translate a PR-object to a Pdf-object
        */
        protected PdfObject CopyObject(PdfObject inp) {
            if (inp == null)
                return PdfNull.PDFNULL;
            switch (inp.Type) {
                case PdfObject.DICTIONARY:
                    return CopyDictionary((PdfDictionary)inp);
                case PdfObject.INDIRECT:
                    return CopyIndirect((PRIndirectReference)inp);
                case PdfObject.ARRAY:
                    return CopyArray((PdfArray)inp);
                case PdfObject.NUMBER:
                case PdfObject.NAME:
                case PdfObject.STRING:
                case PdfObject.NULL:
                case PdfObject.BOOLEAN:
                    return inp;
                case PdfObject.STREAM:
                    return CopyStream((PRStream)inp);
                    //                return in;
                default:
                    if (inp.Type < 0) {
                        String lit = ((PdfLiteral)inp).ToString();
                        if (lit.Equals("true") || lit.Equals("false")) {
                            return new PdfBoolean(lit);
                        }
                        return new PdfLiteral(lit);
                    }
                    return null;
            }
        }
        
        /**
        * convenience method. Given an importedpage, set our "globals"
        */
        protected int SetFromIPage(PdfImportedPage iPage) {
            int pageNum = iPage.PageNumber;
            PdfReaderInstance inst = currentPdfReaderInstance = iPage.PdfReaderInstance;
            reader = inst.Reader;
            SetFromReader(reader);
            return pageNum;
        }
        
        /**
        * convenience method. Given a reader, set our "globals"
        */
        protected void SetFromReader(PdfReader reader) {
            this.reader = reader;
            indirects = (Hashtable)indirectMap[reader] ;
            if (indirects == null) {
                indirects = new Hashtable();
                indirectMap[reader] = indirects;
                PdfDictionary catalog = reader.Catalog;
                PRIndirectReference refi = (PRIndirectReference)catalog.Get(PdfName.PAGES);
                indirects[new RefKey(refi)] =  new IndirectReferences(topPageParent);
                refi = null;
                PdfObject o = catalog.Get(PdfName.ACROFORM);
                if (o == null || o.Type != PdfObject.INDIRECT)
                    return;
                refi = (PRIndirectReference)o;
                if (acroForm == null) acroForm = body.PdfIndirectReference;
                indirects[new RefKey(refi)] =  new IndirectReferences(acroForm);
            }
        }
        /**
        * Add an imported page to our output
        * @param iPage an imported page
        * @throws IOException, BadPdfFormatException
        */
        public void AddPage(PdfImportedPage iPage) {
            int pageNum = SetFromIPage(iPage);
            
            PdfDictionary thePage = reader.GetPageN(pageNum);
            PRIndirectReference origRef = reader.GetPageOrigRef(pageNum);
            reader.ReleasePage(pageNum);
            RefKey key = new RefKey(origRef);
            PdfIndirectReference pageRef;
            IndirectReferences iRef = (IndirectReferences)indirects[key] ;
            iRef = null; // temporary hack to have multiple pages, may break is some cases
            // if we already have an iref for the page (we got here by another link)
            if (iRef != null) {
                pageRef = iRef.Ref;
            }
            else {
                pageRef = body.PdfIndirectReference;
                iRef = new IndirectReferences(pageRef);
                indirects[key] =  iRef;
            }
            pageReferences.Add(pageRef);
            ++currentPageNumber;
            if (! iRef.Copied) {
                iRef.SetCopied();
                PdfDictionary newPage = CopyDictionary(thePage);
                newPage.Put(PdfName.PARENT, topPageParent);
                AddToBody(newPage, pageRef);
            }
            root.AddPage(pageRef);
            pageNumbersToRefs.Add(pageRef);
        }
        
        public override PdfIndirectReference GetPageReference(int page) {
            if (page < 0 || page > pageNumbersToRefs.Count)
                throw new ArgumentException("Invalid page number " + page);
            return (PdfIndirectReference)pageNumbersToRefs[page - 1] ;
        }

        /**
        * Copy the acroform for an input document. Note that you can only have one,
        * we make no effort to merge them.
        * @param reader The reader of the input file that is being copied
        * @throws IOException, BadPdfFormatException
        */
        public void CopyAcroForm(PdfReader reader) {
            SetFromReader(reader);
            
            PdfDictionary catalog = reader.Catalog;
            PRIndirectReference hisRef = null;
            PdfObject o = catalog.Get(PdfName.ACROFORM);
            if (o != null && o.Type == PdfObject.INDIRECT)
                hisRef = (PRIndirectReference)o;
            if (hisRef == null) return; // bugfix by John Engla
            RefKey key = new RefKey(hisRef);
            PdfIndirectReference myRef;
            IndirectReferences iRef = (IndirectReferences)indirects[key] ;
            if (iRef != null) {
                acroForm = myRef = iRef.Ref;
            }
            else {
                acroForm = myRef = body.PdfIndirectReference;
                iRef = new IndirectReferences(myRef);
                indirects[key] =  iRef;
            }
            if (! iRef.Copied) {
                iRef.SetCopied();
                PdfDictionary theForm = CopyDictionary((PdfDictionary)PdfReader.GetPdfObject(hisRef));
                AddToBody(theForm, myRef);
            }
        }
        
        /*
        * the getCatalog method is part of PdfWriter.
        * we wrap this so that we can extend it
        */
        protected override PdfDictionary GetCatalog(PdfIndirectReference rootObj) {
            PdfDictionary theCat = pdf.GetCatalog(rootObj);
            if (acroForm != null) theCat.Put(PdfName.ACROFORM, acroForm);
            if (newBookmarks == null || newBookmarks.Count == 0)
                return theCat;
            PdfDictionary top = new PdfDictionary();
            PdfIndirectReference topRef = PdfIndirectReference;
            Object[] kids = SimpleBookmark.IterateOutlines(this, topRef, newBookmarks, false);
            top.Put(PdfName.FIRST, (PdfIndirectReference)kids[0]);
            top.Put(PdfName.LAST, (PdfIndirectReference)kids[1]);
            top.Put(PdfName.COUNT, new PdfNumber((int)kids[2]));
            AddToBody(top, topRef);
            theCat.Put(PdfName.OUTLINES, topRef);
            return theCat;
        }
        
        /**
        * Sets the bookmarks. The list structure is defined in
        * <CODE>SimpleBookmark#</CODE>.
        * @param outlines the bookmarks or <CODE>null</CODE> to remove any
        */    
        public void SetOutlines(ArrayList outlines) {
            newBookmarks = outlines;
        }

        /**
        * Signals that the <CODE>Document</CODE> was closed and that no other
        * <CODE>Elements</CODE> will be added.
        * <P>
        * The pages-tree is built and written to the outputstream.
        * A Catalog is constructed, as well as an Info-object,
        * the referencetable is composed and everything is written
        * to the outputstream embedded in a Trailer.
        */
        
        public override void Close() {
            if (open) {
                PdfReaderInstance ri = currentPdfReaderInstance;
                pdf.Close();
                base.Close();
                if (ri != null) {
                    try {
                        ri.Reader.Close();
                        ri.ReaderFile.Close();
                    }
                    catch (IOException) {
                        // empty on purpose
                    }
                }
            }
        }
        internal override PdfIndirectReference Add(PdfImage pdfImage, PdfIndirectReference fixedRef) { return null; }
        public override void AddAnnotation(PdfAnnotation annot) {  }
        internal override PdfIndirectReference Add(PdfPage page, PdfContents contents) { return null; }

        public override void FreeReader(PdfReader reader) {
            indirectMap.Remove(reader);
            if (currentPdfReaderInstance != null) {
                if (currentPdfReaderInstance.Reader == reader) {
                    try {
                        currentPdfReaderInstance.Reader.Close();
                        currentPdfReaderInstance.ReaderFile.Close();
                    }
                    catch (IOException) {
                        // empty on purpose
                    }
                    currentPdfReaderInstance = null;
                }
            }
        }
    }
}
