using System;
using System.IO;
using iTextSharp.text;
/**
 * $Id: AbstractRtfField.cs,v 1.1 2005/07/04 22:51:35 psoares33 Exp $
 *
 * Copyright 2002 by 
 * <a href="http://www.smb-tec.com">SMB</a> 
 * <a href="mailto:Dirk.Weigenand@smb-tec.com">Dirk.Weigenand@smb-tec.com</a>
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
 * LGPL license (the “GNU LIBRARY GENERAL PUBLIC LICENSE”), in which case the
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
    * This class implements an abstract RtfField.
    *
    * This class is based on the RtfWriter-package from Mark Hall.
    * 
    * ONLY FOR USE WITH THE RtfWriter NOT with the RtfWriter2.
    *
    * @author <a href="mailto:Dirk.Weigenand@smb-tec.com">Dirk Weigenand</a>
    * @version $Id: AbstractRtfField.cs,v 1.1 2005/07/04 22:51:35 psoares33 Exp $
    * @since Mon Aug 19 14:50:39 2002
    */
    public abstract class AbstractRtfField : Chunk, IRtfField {
        private static byte[] fldDirty = DocWriter.GetISOBytes("\\flddirty");
        private static byte[] fldPriv = DocWriter.GetISOBytes("\\fldpriv");
        private static byte[] fldLock = DocWriter.GetISOBytes("\\fldlock");
        private static byte[] fldEdit = DocWriter.GetISOBytes("\\fldedit");
        private static byte[] fldAlt = DocWriter.GetISOBytes("\\fldalt");

        /**
        * public constructor
        * @param content the content of the field
        * @param font the font of the field
        */
        public AbstractRtfField(String content, Font font) : base(content, font) {
        }

        /**
        * Determines whether this RtfField is locked, i.e. it cannot be
        * updated. Defaults to <tt>false</tt>.
        */
        private bool rtfFieldIsLocked = false;

        /**
        * Determines whether a formatting change has been made since the
        * field was last updated. Defaults to <tt>false</tt>.
        */
        private bool rtfFieldIsDirty = false;

        /**
        * Determines whether text has been added, removed from thre field
        * result since the field was last updated. Defaults to
        * <tt>false</tt>.
        */
        private bool rtfFieldWasEdited = false;

        /**
        * Determines whether the field is in suitable form for
        * display. Defaults to <tt>false</tt>.
        */
        private bool rtfFieldIsPrivate = false;

        /**
        * Determines whether this RtfField shall refer to an end note.
        */
        private bool rtfFieldIsAlt = false;

        /**
        * Determines whtether the field is locked, i.e. it cannot be
        * updated.
        * 
        * @return <tt>true</tt> iff the field cannot be updated,
        * <tt>false</tt> otherwise.
        */
        public bool IsLocked() {
            return this.rtfFieldIsLocked;
        }

        /**
        * Set whether the field can be updated.
        *
        * @param rtfFieldIsLocked <tt>true</tt> if the field cannot be
        * updated, <tt>false</tt> otherwise.
        */
        public void SetLocked(bool rtfFieldIsLocked) {
            this.rtfFieldIsLocked = rtfFieldIsLocked;
        }

        /**
        * Set whether a formatting change has been made since the field
        * was last updated
        * @param rtfFieldIsDirty <tt>true</tt> if the field was
        * changed since the field was last updated, <tt>false</tt>
        * otherwise.
        */
        public void SetDirty(bool rtfFieldIsDirty) {
            this.rtfFieldIsDirty = rtfFieldIsDirty;
        }

        /**
        * Determines whether the field was changed since the field was
        * last updated
        * @return <tt>true</tt> if the field was changed since the field
        * was last updated, <tt>false</tt> otherwise.
        */
        public bool IsDirty() {
            return this.rtfFieldIsDirty;
        }

        /**
        * Set whether text has been added, removed from thre field result
        * since the field was last updated.
        * @param rtfFieldWasEdited Determines whether text has been
        * added, removed from the field result since the field was last
        * updated (<tt>true</tt>, <tt>false</tt> otherwise..
        */
        public void SetEdited(bool rtfFieldWasEdited) {
            this.rtfFieldWasEdited = rtfFieldWasEdited;
        }

        /**
        * Determines whether text has been added, removed from the field
        * result since the field was last updated.
        * @return rtfFieldWasEdited <tt>true</tt> if text has been added,
        * removed from the field result since the field was last updated,
        * <tt>false</tt> otherwise.
        */
        public bool WasEdited() {
            return this.rtfFieldWasEdited;
        }

        /**
        * Set whether the field is in suitable form for
        * display. I.e. it's not a field with a picture as field result
        * @param rtfFieldIsPrivate Determines whether the field is in
        * suitable form for display: <tt>true</tt> it can be displayed,
        * <tt>false</tt> it cannot be displayed.
        */
        public void SetPrivate(bool rtfFieldIsPrivate) {
            this.rtfFieldIsPrivate = rtfFieldIsPrivate;
        }

        /**
        * Determines whether the field is in suitable form for display.
        * @return whether the field is in suitable form for display:
        * <tt>true</tt> yes, <tt>false</tt> no it cannot be displayed.
        */
        public bool IsPrivate() {
            return this.rtfFieldIsPrivate;
        }

        /**
        * Abstract method for writing custom stuff to the Field
        * Initialization Stuff part of an RtfField.
        * @param outp
        * @throws IOException
        */
        public abstract void WriteRtfFieldInitializationStuff(Stream outp);

        /**
        * Abstract method for writing custom stuff to the Field Result
        * part of an RtfField.
        * @param outp
        * @throws IOException
        */
        public abstract void WriteRtfFieldResultStuff(Stream outp);

        /**
        * Determines whether this RtfField shall refer to an end note.
        * @param rtfFieldIsAlt <tt>true</tt> if this RtfField shall refer
        * to an end note, <tt>false</tt> otherwise
        */
        public void SetAlt(bool rtfFieldIsAlt) {
            this.rtfFieldIsAlt = rtfFieldIsAlt;
        }

        /**
        * Determines whether this RtfField shall refer to an end
        * note.
        * @return <tt>true</tt> if this RtfField shall refer to an end
        * note, <tt>false</tt> otherwise.
        */
        public bool IsAlt() {
            return this.rtfFieldIsAlt;
        }

        /**
        * empty implementation for Chunk.
        * @return an empty string
        */
        public override String Content {
            get {
                return "";
            }
        }

        /**
        * For Interface RtfField.
        * @param writer
        * @param outp
        * @throws IOException
        */
        public virtual void Write( RtfWriter writer, Stream outp ) {
            WriteRtfFieldBegin(outp);
            WriteRtfFieldModifiers(outp);
            WriteRtfFieldInstBegin(outp);
            writer.WriteInitialFontSignature( outp, this );
            WriteRtfFieldInitializationStuff(outp);
            WriteRtfFieldInstEnd(outp);
            WriteRtfFieldResultBegin(outp);
            writer.WriteInitialFontSignature( outp, this );
            WriteRtfFieldResultStuff(outp);
            WriteRtfFieldResultEnd(outp);
            WriteRtfFieldEnd(outp);
        }

        /**
        * Write the beginning of an RtfField to the Stream.
        * @param outp
        * @throws IOException
        */
        protected void WriteRtfFieldBegin(Stream outp)  {
            outp.WriteByte(RtfWriter.openGroup);
            outp.WriteByte(RtfWriter.escape);
            outp.Write(RtfWriter.field, 0, RtfWriter.field.Length);
        }

        /**
        * Write the modifiers defined for a RtfField to the Stream.
        * @param outp
        * @throws IOException
        */
        protected void WriteRtfFieldModifiers(Stream outp) {
            if (IsDirty()) {
                outp.Write(fldDirty, 0, fldDirty.Length);
            }

            if (WasEdited()) {
                outp.Write(fldEdit, 0, fldEdit.Length);
            }

            if (IsLocked()) {
                outp.Write(fldLock, 0, fldLock.Length);
            }

            if (IsPrivate()) {
                outp.Write(fldPriv, 0, fldPriv.Length);
            }
        }

        /**
        * Write RtfField Initialization Stuff to Stream.
        * @param outp
        * @throws IOException
        */
        protected void WriteRtfFieldInstBegin(Stream outp) {
            outp.WriteByte( RtfWriter.openGroup);        
            outp.WriteByte( RtfWriter.escape);
            outp.Write( RtfWriter.fieldContent , 0,  RtfWriter.fieldContent .Length);
            outp.WriteByte( RtfWriter.delimiter);
        }

        /**
        * Write end of RtfField Initialization Stuff to Stream.
        * @param outp
        * @throws IOException
        */
        protected void WriteRtfFieldInstEnd(Stream outp) {
            if (IsAlt()) {
                outp.Write( fldAlt , 0,  fldAlt .Length);
                outp.WriteByte( RtfWriter.delimiter);
            }

            outp.WriteByte( RtfWriter.closeGroup);
        }

        /**
        * Write beginning of RtfField Result to Stream.
        * @param outp
        * @throws IOException
        */
        protected void WriteRtfFieldResultBegin(Stream outp) {
            outp.WriteByte( RtfWriter.openGroup);        
            outp.WriteByte( RtfWriter.escape);
            outp.Write( RtfWriter.fieldDisplay , 0,  RtfWriter.fieldDisplay .Length);
            outp.WriteByte( RtfWriter.delimiter);
        }

        /**
        * Write end of RtfField Result to Stream.
        * @param outp
        * @throws IOException
        */
        protected void WriteRtfFieldResultEnd(Stream outp) {
            outp.WriteByte( RtfWriter.delimiter);
            outp.WriteByte( RtfWriter.closeGroup);
        }

        /**
        * Close the RtfField.
        * @param outp
        * @throws IOException
        */
        protected void WriteRtfFieldEnd(Stream outp) {
            outp.WriteByte( RtfWriter.closeGroup);
        }
    }
}