using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Globalization;
using System.Text;
using System.util;
using iTextSharp.text;
using iTextSharp.text.pdf.codec.wmf;
/*
 * $Id: RtfWriter.cs,v 1.4 2007/02/09 15:12:51 psoares33 Exp $
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
    * If you are creating a new project using the rtf part of iText, please
    * consider using the new RtfWriter2. The RtfWriter is in bug-fix-only mode,
    * will be deprecated end of 2005 and removed end of 2007.
    * 
    * A <CODE>DocWriter</CODE> class for Rich Text Files (RTF).
    * <P>
    * A <CODE>RtfWriter</CODE> can be added as a <CODE>DocListener</CODE>
    * to a certain <CODE>Document</CODE> by getting an instance.
    * Every <CODE>Element</CODE> added to the original <CODE>Document</CODE>
    * will be written to the <CODE>Stream</CODE> of this <CODE>RtfWriter</CODE>.
    * <P>
    * Example:
    * <BLOCKQUOTE><PRE>
    * // creation of the document with a certain size and certain margins
    * Document document = new Document(PageSize.A4, 50, 50, 50, 50);
    * try {
    *    // this will write RTF to the Standard Stream
    *    <STRONG>RtfWriter.GetInstance(document, System.outp);</STRONG>
    *    // this will write Rtf to a file called text.rtf
    *    <STRONG>RtfWriter.GetInstance(document, new FileOutputStream("text.rtf"));</STRONG>
    *    // this will write Rtf to for instance the Stream of a HttpServletResponse-object
    *    <STRONG>RtfWriter.GetInstance(document, response.GetOutputStream());</STRONG>
    * }
    * catch (DocumentException de) {
    *    System.err.Println(de.GetMessage());
    * }
    * // this will close the document and all the OutputStreams listening to it
    * <STRONG>document.Close();</CODE>
    * </PRE></BLOCKQUOTE>
    * <P>
    * <STRONG>LIMITATIONS</STRONG><BR>
    * There are currently still a few limitations on what the RTF Writer can do:
    * <ul>
    *    <li>Watermarks</li>
    *    <li>Viewer preferences</li>
    *    <li>Encryption</li>
    *    <li>Embedded fonts</li>
    *    <li>Phrases with a leading</li>
    *    <li>Lists with non-bullet symbols</li>
    *    <li>Nested tables</li>
    *    <li>Images other than JPEG and PNG</li>
    *    <li>Rotated images</li>
    * </ul>
    * <br />
    *
    * @author <a href="mailto:mhall@myrealbox.com">Mark.Hall@myrealbox.com</a>
    * @author Steffen Stundzig
    * @author <a href="ericmattes@yahoo.com">Eric Mattes</a>
    * @author <a href="raul.wegmann@uam.es">Raul Wegmann</a>
    */
    public class RtfWriter : DocWriter, IDocListener {
        /**
        * Static Constants
        */

        /**
        * General
        */

        /** This is the escape character which introduces RTF tags. */
        public const byte escape = (byte) '\\';

        /** This is another escape character which introduces RTF tags. */
        private static byte[] extendedEscape = DocWriter.GetISOBytes("\\*\\");

        /** This is the delimiter between RTF tags and normal text. */
        protected internal const byte delimiter = (byte) ' ';

        /** This is another delimiter between RTF tags and normal text. */
        private const byte commaDelimiter = (byte) ';';

        /** This is the character for beginning a new group. */
        public const byte openGroup = (byte) '{';

        /** This is the character for closing a group. */
        public const byte closeGroup = (byte) '}';

        /**
        * RTF Information
        */

        /** RTF begin and version. */
        private static byte[] docBegin = DocWriter.GetISOBytes("rtf1");

        /** RTF encoding. */
        private static byte[] ansi = DocWriter.GetISOBytes("ansi");

        /** RTF encoding codepage. */
        private static byte[] ansiCodepage = DocWriter.GetISOBytes("ansicpg");

        /**
        *Font Data
        */

        /** Begin the font table tag. */
        private static byte[] fontTable = DocWriter.GetISOBytes("fonttbl");

        /** Font number tag. */
        protected internal const byte fontNumber = (byte) 'f';

        /** Font size tag. */
        protected internal static byte[] fontSize = DocWriter.GetISOBytes("fs");

        /** Font color tag. */
        protected internal static byte[] fontColor = DocWriter.GetISOBytes("cf");

        /** Modern font tag. */
        private static byte[] fontModern = DocWriter.GetISOBytes("fmodern");

        /** Swiss font tag. */
        private static byte[] fontSwiss = DocWriter.GetISOBytes("fswiss");

        /** Roman font tag. */
        private static byte[] fontRoman = DocWriter.GetISOBytes("froman");

        /** Tech font tag. */
        private static byte[] fontTech = DocWriter.GetISOBytes("ftech");

        /** Font charset tag. */
        private static byte[] fontCharset = DocWriter.GetISOBytes("fcharset");

        /** Font Courier tag. */
        private static byte[] fontCourier = DocWriter.GetISOBytes("Courier");

        /** Font Arial tag. */
        private static byte[] fontArial = DocWriter.GetISOBytes("Arial");

        /** Font Symbol tag. */
        private static byte[] fontSymbol = DocWriter.GetISOBytes("Symbol");

        /** Font Times New Roman tag. */
        private static byte[] fontTimesNewRoman = DocWriter.GetISOBytes("Times New Roman");

        /** Font Windings tag. */
        private static byte[] fontWindings = DocWriter.GetISOBytes("Windings");

        /** Default Font. */
        private static byte[] defaultFont = DocWriter.GetISOBytes("deff");

        /** First indent tag. */
        private static byte[] firstIndent = DocWriter.GetISOBytes("fi");

        /** Left indent tag. */
        private static byte[] listIndent = DocWriter.GetISOBytes("li");

        /** Right indent tag. */
        private static byte[] rightIndent = DocWriter.GetISOBytes("ri");

        /**
        * Sections / Paragraphs
        */

        /** Reset section defaults tag. */
        private static byte[] sectionDefaults = DocWriter.GetISOBytes("sectd");

        /** Begin new section tag. */
        private static byte[] section = DocWriter.GetISOBytes("sect");

        /** Reset paragraph defaults tag. */
        public static byte[] paragraphDefaults = DocWriter.GetISOBytes("pard");

        /** Begin new paragraph tag. */
        public static byte[] paragraph = DocWriter.GetISOBytes("par");

        /** Page width of a section. */
        public static byte[] sectionPageWidth = DocWriter.GetISOBytes("pgwsxn");

        /** Page height of a section. */
        public static byte[] sectionPageHeight = DocWriter.GetISOBytes("pghsxn");

        /**
        * Lists
        */

        /** Begin the List Table */
        private static byte[] listtableGroup = DocWriter.GetISOBytes("listtable");

        /** Begin the List Override Table */
        private static byte[] listoverridetableGroup = DocWriter.GetISOBytes("listoverridetable");

        /** Begin a List definition */
        private static byte[] listDefinition = DocWriter.GetISOBytes("list");

        /** List Template ID */
        private static byte[] listTemplateID = DocWriter.GetISOBytes("listtemplateid");

        /** RTF Writer outputs hybrid lists */
        private static byte[] hybridList = DocWriter.GetISOBytes("hybrid");

        /** Current List level */
        private static byte[] listLevelDefinition = DocWriter.GetISOBytes("listlevel");

        /** Level numbering (old) */
        private static byte[] listLevelTypeOld = DocWriter.GetISOBytes("levelnfc");

        /** Level numbering (new) */
        private static byte[] listLevelTypeNew = DocWriter.GetISOBytes("levelnfcn");

        /** Level alignment (old) */
        private static byte[] listLevelAlignOld = DocWriter.GetISOBytes("leveljc");

        /** Level alignment (new) */
        private static byte[] listLevelAlignNew = DocWriter.GetISOBytes("leveljcn");

        /** Level starting number */
        private static byte[] listLevelStartAt = DocWriter.GetISOBytes("levelstartat");

        /** Level text group */
        private static byte[] listLevelTextDefinition = DocWriter.GetISOBytes("leveltext");

        /** Filler for Level Text Length */
        private static byte[] listLevelTextLength = DocWriter.GetISOBytes("\'0");

        /** Level Text Numbering Style */
        private static byte[] listLevelTextStyleNumbers = DocWriter.GetISOBytes("\'00.");

        /** Level Text Bullet Style */
        private static byte[] listLevelTextStyleBullet = DocWriter.GetISOBytes("u-3913 ?");

        /** Level Numbers Definition */
        private static byte[] listLevelNumbersDefinition = DocWriter.GetISOBytes("levelnumbers");

        /** Filler for Level Numbers */
        private static byte[] listLevelNumbers = DocWriter.GetISOBytes("\\'0");

        /** Tab Stop */
        private static byte[] tabStop = DocWriter.GetISOBytes("tx");

        /** Actual list begin */
        private static byte[] listBegin = DocWriter.GetISOBytes("ls");

        /** Current list level */
        private static byte[] listCurrentLevel = DocWriter.GetISOBytes("ilvl");

        /** List text group for older browsers */
        private static byte[] listTextOld = DocWriter.GetISOBytes("listtext");

        /** Tab */
        private static byte[] tab = DocWriter.GetISOBytes("tab");

        /** Old Bullet Style */
        private static byte[] listBulletOld = DocWriter.GetISOBytes("\'b7");

        /** Current List ID */
        private static byte[] listID = DocWriter.GetISOBytes("listid");

        /** List override */
        private static byte[] listOverride = DocWriter.GetISOBytes("listoverride");

        /** Number of overrides */
        private static byte[] listOverrideCount = DocWriter.GetISOBytes("listoverridecount");

        /**
        * Text Style
        */

        /** Bold tag. */
        protected internal const byte bold = (byte) 'b';

        /** Italic tag. */
        protected internal const byte italic = (byte) 'i';

        /** Underline tag. */
        protected internal static byte[] underline = DocWriter.GetISOBytes("ul");

        /** Strikethrough tag. */
        protected internal static byte[] strikethrough = DocWriter.GetISOBytes("strike");

        /** Text alignment left tag. */
        public static byte[] alignLeft = DocWriter.GetISOBytes("ql");

        /** Text alignment center tag. */
        public static byte[] alignCenter = DocWriter.GetISOBytes("qc");

        /** Text alignment right tag. */
        public static byte[] alignRight = DocWriter.GetISOBytes("qr");

        /** Text alignment justify tag. */
        public static byte[] alignJustify = DocWriter.GetISOBytes("qj");

        /**
        * Colors
        */

        /** Begin colour table tag. */
        private static byte[] colorTable = DocWriter.GetISOBytes("colortbl");

        /** Red value tag. */
        private static byte[] colorRed = DocWriter.GetISOBytes("red");

        /** Green value tag. */
        private static byte[] colorGreen = DocWriter.GetISOBytes("green");

        /** Blue value tag. */
        private static byte[] colorBlue = DocWriter.GetISOBytes("blue");

        /**
        * Information Group
        */

        /** Begin the info group tag.*/
        private static byte[] infoBegin = DocWriter.GetISOBytes("info");

        /** Author tag. */
        private static byte[] metaAuthor = DocWriter.GetISOBytes("author");

        /** Subject tag. */
        private static byte[] metaSubject = DocWriter.GetISOBytes("subject");

        /** Keywords tag. */
        private static byte[] metaKeywords = DocWriter.GetISOBytes("keywords");

        /** Title tag. */
        private static byte[] metaTitle = DocWriter.GetISOBytes("title");

        /** Producer tag. */
        private static byte[] metaProducer = DocWriter.GetISOBytes("operator");

        /** Creation Date tag. */
        private static byte[] metaCreationDate = DocWriter.GetISOBytes("creationdate");

        /** Year tag. */
        private static byte[] year = DocWriter.GetISOBytes("yr");

        /** Month tag. */
        private static byte[] month = DocWriter.GetISOBytes("mo");

        /** Day tag. */
        private static byte[] day = DocWriter.GetISOBytes("dy");

        /** Hour tag. */
        private static byte[] hour = DocWriter.GetISOBytes("hr");

        /** Minute tag. */
        private static byte[] minute = DocWriter.GetISOBytes("min");

        /** Second tag. */
        private static byte[] second = DocWriter.GetISOBytes("sec");

        /** Start superscript. */
        private static byte[] startSuper = DocWriter.GetISOBytes("super");

        /** Start subscript. */
        private static byte[] startSub = DocWriter.GetISOBytes("sub");

        /** End super/sub script. */
        private static byte[] endSuperSub = DocWriter.GetISOBytes("nosupersub");

        /**
        * Header / Footer
        */

        /** Title Page tag */
        private static byte[] titlePage = DocWriter.GetISOBytes("titlepg");

        /** Facing pages tag */
        private static byte[] facingPages = DocWriter.GetISOBytes("facingp");

        /** Begin header group tag. */
        private static byte[] headerBegin = DocWriter.GetISOBytes("header");

        /** Begin footer group tag. */
        private static byte[] footerBegin = DocWriter.GetISOBytes("footer");

        // header footer 'left', 'right', 'first'
        private static byte[] headerlBegin = DocWriter.GetISOBytes("headerl");

        private static byte[] footerlBegin = DocWriter.GetISOBytes("footerl");

        private static byte[] headerrBegin = DocWriter.GetISOBytes("headerr");

        private static byte[] footerrBegin = DocWriter.GetISOBytes("footerr");

        private static byte[] headerfBegin = DocWriter.GetISOBytes("headerf");

        private static byte[] footerfBegin = DocWriter.GetISOBytes("footerf");

        /**
        * Paper Properties
        */

        /** Paper width tag. */
        private static byte[] rtfPaperWidth = DocWriter.GetISOBytes("paperw");

        /** Paper height tag. */
        private static byte[] rtfPaperHeight = DocWriter.GetISOBytes("paperh");

        /** Margin left tag. */
        private static byte[] rtfMarginLeft = DocWriter.GetISOBytes("margl");

        /** Margin right tag. */
        private static byte[] rtfMarginRight = DocWriter.GetISOBytes("margr");

        /** Margin top tag. */
        private static byte[] rtfMarginTop = DocWriter.GetISOBytes("margt");

        /** Margin bottom tag. */
        private static byte[] rtfMarginBottom = DocWriter.GetISOBytes("margb");

        /** New Page tag. */
        private static byte[] newPage = DocWriter.GetISOBytes("page");

        /** Document Landscape tag 1. */
        private static byte[] landscapeTag1 = DocWriter.GetISOBytes("landscape");

        /** Document Landscape tag 2. */
        private static byte[] landscapeTag2 = DocWriter.GetISOBytes("lndscpsxn");

        /**
        * Annotations
        */

        /** Annotation ID tag. */
        private static byte[] annotationID = DocWriter.GetISOBytes("atnid");

        /** Annotation Author tag. */
        private static byte[] annotationAuthor = DocWriter.GetISOBytes("atnauthor");

        /** Annotation text tag. */
        private static byte[] annotation = DocWriter.GetISOBytes("annotation");

        /**
        * Images
        */

        /** Begin the main Picture group tag */
        private static byte[] pictureGroup = DocWriter.GetISOBytes("shppict");

        /** Begin the picture tag */
        private static byte[] picture = DocWriter.GetISOBytes("pict");

        /** PNG Image */
        private static byte[] picturePNG = DocWriter.GetISOBytes("pngblip");

        /** JPEG Image */
        private static byte[] pictureJPEG = DocWriter.GetISOBytes("jpegblip");

        /** BMP Image */
        private static byte[] pictureBMP = DocWriter.GetISOBytes("dibitmap0");

        /** WMF Image */
        private static byte[] pictureWMF = DocWriter.GetISOBytes("wmetafile8");

        /** Picture width */
        private static byte[] pictureWidth = DocWriter.GetISOBytes("picw");

        /** Picture height */
        private static byte[] pictureHeight = DocWriter.GetISOBytes("pich");

        /** Picture scale horizontal percent */
        private static byte[] pictureScaleX = DocWriter.GetISOBytes("picscalex");

        /** Picture scale vertical percent */
        private static byte[] pictureScaleY = DocWriter.GetISOBytes("picscaley");

        /**
        * Fields (for page numbering)
        */

        /** Begin field tag */
        protected internal static byte[] field = DocWriter.GetISOBytes("field");

        /** Content fo the field */
        protected internal static byte[] fieldContent = DocWriter.GetISOBytes("fldinst");

        /** PAGE numbers */
        protected internal static byte[] fieldPage = DocWriter.GetISOBytes("PAGE");

        /** HYPERLINK field */
        protected internal static byte[] fieldHyperlink = DocWriter.GetISOBytes("HYPERLINK");

        /** Last page number (not used) */
        protected internal static byte[] fieldDisplay = DocWriter.GetISOBytes("fldrslt");


        /** Class variables */

        /**
        * Because of the way RTF works and the way itext works, the text has to be
        * stored and is only written to the actual Stream at the end.
        */

        /** This <code>ArrayList</code> contains all fonts used in the document. */
        private ArrayList fontList = new ArrayList();

        /** This <code>ArrayList</code> contains all colours used in the document. */
        private ArrayList colorList = new ArrayList();

        /** This <code>MemoryStream</code> contains the main body of the document. */
        private MemoryStream content = null;

        /** This <code>MemoryStream</code> contains the information group. */
        private MemoryStream info = null;

        /** This <code>MemoryStream</code> contains the list table. */
        private MemoryStream listtable = null;

        /** This <code>MemoryStream</code> contains the list override table. */
        private MemoryStream listoverride = null;

        /** Document header. */
        private HeaderFooter header = null;

        /** Document footer. */
        private HeaderFooter footer = null;

        /** Left margin. */
        private int marginLeft = 1800;

        /** Right margin. */
        private int marginRight = 1800;

        /** Top margin. */
        private int marginTop = 1440;

        /** Bottom margin. */
        private int marginBottom = 1440;

        /** Page width. */
        private int pageWidth = 11906;

        /** Page height. */
        private int pageHeight = 16838;

        /** Factor to use when converting. */
        public const double TWIPSFACTOR = 20;//20.57140;

        /** Current list ID. */
        private int currentListID = 1;

        /** List of current Lists. */
        private ArrayList listIds = null;

        /** Current List Level. */
        private int listLevel = 0;

        /** Current maximum List Level. */
        private int maxListLevel = 0;

        /** Write a TOC */
        private bool writeTOC = false;

        /** Special title page */
        private bool hasTitlePage = false;

        /** Currently writing either Header or Footer */
        private bool inHeaderFooter = false;

        /** Currently writing a Table */
        private bool inTable = false;

        /** Landscape or Portrait Document */
        private bool landscape = false;

        private static Random random = new Random();

        /** Protected Constructor */

        /**
        * Constructs a <CODE>RtfWriter</CODE>.
        *
        * @param doc         The <CODE>Document</CODE> that is to be written as RTF
        * @param os          The <CODE>Stream</CODE> the writer has to write to.
        */

        protected internal RtfWriter(Document doc, Stream os) : base(doc, os) {
            document.AddDocListener(this);
            InitDefaults();
        }

        /** Public functions special to the RtfWriter */

        /**
        * This method controls whether TOC entries are automatically generated
        *
        * @param writeTOC    bool value indicating whether a TOC is to be generated
        */
        public void SetGenerateTOCEntries(bool writeTOC) {
            this.writeTOC = writeTOC;
        }

        /**
        * Gets the current setting of writeTOC
        *
        * @return    bool value indicating whether a TOC is being generated
        */
        public bool GetGeneratingTOCEntries() {
            return writeTOC;
        }

        /**
        * This method controls whether the first page is a title page
        *
        * @param hasTitlePage    bool value indicating whether the first page is a title page
        */
        public void SetHasTitlePage(bool hasTitlePage) {
            this.hasTitlePage = hasTitlePage;
        }

        /**
        * Gets the current setting of hasTitlePage
        *
        * @return    bool value indicating whether the first page is a title page
        */
        public bool GetHasTitlePage() {
            return hasTitlePage;
        }

        /**
        * Explicitly sets the page format to use.
        * Otherwise the RtfWriter will try to guess the format by comparing pagewidth and pageheight
        *
        * @param landscape bool value indicating whether we are using landscape format or not
        */
        public void SetLandscape(bool landscape) {
            this.landscape = landscape;
        }

        /**
        * Returns the current landscape setting
        *
        * @return bool value indicating the current page format
        */
        public bool GetLandscape() {
            return landscape;
        }

        /** Public functions from the DocWriter Interface */

        /**
        * Gets an instance of the <CODE>RtfWriter</CODE>.
        *
        * @param document    The <CODE>Document</CODE> that has to be written
        * @param os  The <CODE>Stream</CODE> the writer has to write to.
        * @return    a new <CODE>RtfWriter</CODE>
        */
        public static RtfWriter GetInstance(Document document, Stream os) {
            return (new RtfWriter(document, os));
        }

        /**
        * Signals that the <CODE>Document</CODE> has been opened and that
        * <CODE>Elements</CODE> can be added.
        */
        public override void Open() {
            base.Open();
        }

        /**
        * Signals that the <CODE>Document</CODE> was closed and that no other
        * <CODE>Elements</CODE> will be added.
        * <p>
        * The content of the font table, color table, information group, content, header, footer are merged into the final
        * <code>Stream</code>
        */
        public override void Close() {
            if (open) {
                WriteDocument();
                base.Close();
            }
        }

        /**
        * Adds the footer to the bottom of the <CODE>Document</CODE>.
        * @param footer
        */
        public override HeaderFooter Footer {
            set {
                this.footer = value;
                ProcessHeaderFooter(this.footer);
            }
        }

        /**
        * Adds the header to the top of the <CODE>Document</CODE>.
        * @param header
        */
        public override HeaderFooter Header {
            set {
                this.header = value;
                ProcessHeaderFooter(this.header);
            }
        }

        /**
        * Resets the footer.
        */
        public override void ResetFooter() {
            Footer = null;
        }

        /**
        * Resets the header.
        */
        public override void ResetHeader() {
            Header = null;
        }

        /**
        * Tells the <code>RtfWriter</code> that a new page is to be begun.
        *
        * @return <code>true</code> if a new Page was begun.
        * @throws DocumentException if the Document was not open or had been closed.
        */
        public override bool NewPage() {
            try {
                content.WriteByte(escape);
                content.Write(newPage, 0, newPage.Length);
                content.WriteByte(escape);
                content.Write(paragraph, 0, paragraph.Length);
            } catch (IOException) {
                return false;
            }
            return true;
        }

        /**
        * Sets the page margins
        *
        * @param marginLeft The left margin
        * @param marginRight The right margin
        * @param marginTop The top margin
        * @param marginBottom The bottom margin
        *
        * @return <code>true</code> if the page margins were set.
        */
        public override bool SetMargins(float marginLeft, float marginRight, float marginTop, float marginBottom) {
            this.marginLeft = (int) (marginLeft * TWIPSFACTOR);
            this.marginRight = (int) (marginRight * TWIPSFACTOR);
            this.marginTop = (int) (marginTop * TWIPSFACTOR);
            this.marginBottom = (int) (marginBottom * TWIPSFACTOR);
            return true;
        }

        /**
        * Sets the page size
        *
        * @param pageSize A <code>Rectangle</code> specifying the page size
        *
        * @return <code>true</code> if the page size was set
        */
        public override bool SetPageSize(Rectangle pageSize) {
            if (!ParseFormat(pageSize, false)) {
                pageWidth = (int) (pageSize.Width * TWIPSFACTOR);
                pageHeight = (int) (pageSize.Height * TWIPSFACTOR);
                landscape = pageWidth > pageHeight;
            }
            return true;
        }

        /**
        * Write the table of contents.
        *
        * @param tocTitle The title that will be displayed above the TOC
        * @param titleFont The <code>Font</code> that will be used for the tocTitle
        * @param showTOCasEntry Set this to true if you want the TOC to appear as an entry in the TOC
        * @param showTOCEntryFont Use this <code>Font</code> to specify what Font to use when showTOCasEntry is true
        *
        * @return <code>true</code> if the TOC was added.
        */
        public bool WriteTOC(String tocTitle, Font titleFont, bool showTOCasEntry, Font showTOCEntryFont) {
            try {
                RtfTOC toc = new RtfTOC(tocTitle, titleFont);
                if (showTOCasEntry) {
                    toc.AddTOCAsTOCEntry(tocTitle, showTOCEntryFont);
                }
                Add(new Paragraph(toc));
            } catch (DocumentException) {
                return false;
            }
            return true;
        }

        /**
        * Signals that an <CODE>Element</CODE> was added to the <CODE>Document</CODE>.
        * 
        * @param element A high level object to add
        * @return    <CODE>true</CODE> if the element was added, <CODE>false</CODE> if not.
        * @throws    DocumentException   if a document isn't open yet, or has been closed
        */
        public override bool Add(IElement element) {
            if (pause) {
                return false;
            }
            return AddElement(element, content);
        }


        /** Private functions */

        /**
        * Adds an <CODE>Element</CODE> to the <CODE>Document</CODE>.
        * @param element the high level element to add
        * @param outp the outputstream to which the RTF data is sent
        * @return    <CODE>true</CODE> if the element was added, <CODE>false</CODE> if not.
        * @throws    DocumentException   if a document isn't open yet, or has been closed
        */
        protected internal bool AddElement(IElement element, MemoryStream outp) {
            try {
                switch (element.Type) {
                    case Element.CHUNK:
                        WriteChunk((Chunk) element, outp);
                        break;
                    case Element.PARAGRAPH:
                        WriteParagraph((Paragraph) element, outp);
                        break;
                    case Element.ANCHOR:
                        WriteAnchor((Anchor) element, outp);
                        break;
                    case Element.PHRASE:
                        WritePhrase((Phrase) element, outp);
                        break;
                    case Element.CHAPTER:
                    case Element.SECTION:
                        WriteSection((Section) element, outp);
                        break;
                    case Element.LIST:
                        WriteList((List) element, outp);
                        break;
                    case Element.TABLE:
                        try {
                            WriteTable((Table) element, outp);
                        }
                        catch (InvalidCastException) {
                            WriteTable(((SimpleTable)element).CreateTable(), outp);
                        }
                        break;
                    case Element.ANNOTATION:
                        WriteAnnotation((Annotation) element, outp);
                        break;
                    case Element.IMGRAW:
                    case Element.IMGTEMPLATE:
                    case Element.JPEG:
                        Image img = (Image)element;
                        WriteImage(img, outp);
                        break;

                    case Element.AUTHOR:
                        WriteMeta(metaAuthor, (Meta) element);
                        break;
                    case Element.SUBJECT:
                        WriteMeta(metaSubject, (Meta) element);
                        break;
                    case Element.KEYWORDS:
                        WriteMeta(metaKeywords, (Meta) element);
                        break;
                    case Element.TITLE:
                        WriteMeta(metaTitle, (Meta) element);
                        break;
                    case Element.PRODUCER:
                        WriteMeta(metaProducer, (Meta) element);
                        break;
                    case Element.CREATIONDATE:
                        WriteMeta(metaCreationDate, (Meta) element);
                        break;
                }
            } catch (IOException) {
                return false;
            }
            return true;
        }

        /**
        * Write the beginning of a new <code>Section</code>
        *
        * @param sectionElement The <code>Section</code> be written
        * @param outp The <code>MemoryStream</code> to write to
        *
        * @throws IOException
        * @throws DocumentException
        */
        private void WriteSection(Section sectionElement, MemoryStream outp) {
            if (sectionElement.Type == Element.CHAPTER) {
                outp.WriteByte(escape);
                outp.Write(sectionDefaults, 0, sectionDefaults.Length);
                WriteSectionDefaults(outp);
            }
            if (sectionElement.Title != null) {
                if (writeTOC) {
                    StringBuilder title = new StringBuilder();
                    foreach (Chunk ck in sectionElement.Title.Chunks) {
                        title.Append(ck.Content);
                    }
                    Add(new RtfTOCEntry(title.ToString(), sectionElement.Title.Font));
                } else {
                    Add(sectionElement.Title);
                }
                outp.WriteByte(escape);
                outp.Write(paragraph, 0, paragraph.Length);
            }
            sectionElement.Process(this);
            if (sectionElement.Type == Element.CHAPTER) {
                outp.WriteByte(escape);
                outp.Write(section, 0, section.Length);
            }
            if (sectionElement.Type == Element.SECTION) {
                outp.WriteByte(escape);
                outp.Write(paragraph, 0, paragraph.Length);
            }
        }

        /**
        * Write the beginning of a new <code>Paragraph</code>
        *
        * @param paragraphElement The <code>Paragraph</code> to be written
        * @param outp The <code>MemoryStream</code> to write to
        *
        * @throws IOException
        */
        private void WriteParagraph(Paragraph paragraphElement, MemoryStream outp) {
            outp.WriteByte(escape);
            outp.Write(paragraphDefaults, 0, paragraphDefaults.Length);
            if (inTable) {
                outp.WriteByte(escape);
                outp.Write(RtfCell.cellInTable, 0, RtfCell.cellInTable.Length);
            }
            switch (paragraphElement.Alignment) {
                case Element.ALIGN_LEFT:
                    outp.WriteByte(escape);
                    outp.Write(alignLeft, 0, alignLeft.Length);
                    break;
                case Element.ALIGN_RIGHT:
                    outp.WriteByte(escape);
                    outp.Write(alignRight, 0, alignRight.Length);
                    break;
                case Element.ALIGN_CENTER:
                    outp.WriteByte(escape);
                    outp.Write(alignCenter, 0, alignCenter.Length);
                    break;
                case Element.ALIGN_JUSTIFIED:
                case Element.ALIGN_JUSTIFIED_ALL:
                    outp.WriteByte(escape);
                    outp.Write(alignJustify, 0, alignJustify.Length);
                    break;
            }
            outp.WriteByte(escape);
            outp.Write(listIndent, 0, listIndent.Length);
            WriteInt(outp, (int) (paragraphElement.IndentationLeft * TWIPSFACTOR));
            outp.WriteByte(escape);
            outp.Write(rightIndent, 0, rightIndent.Length);
            WriteInt(outp, (int) (paragraphElement.IndentationRight * TWIPSFACTOR));
            foreach (Chunk ch in paragraphElement.Chunks) {
                ch.Font = paragraphElement.Font.Difference(ch.Font);
            }
            MemoryStream save = content;
            content = outp;
            paragraphElement.Process(this);
            content = save;
            if (!inTable) {
                outp.WriteByte(escape);
                outp.Write(paragraph, 0, paragraph.Length);
            }
        }

        /**
        * Write a <code>Phrase</code>.
        *
        * @param phrase  The <code>Phrase</code> item to be written
        * @param outp     The <code>MemoryStream</code> to write to
        *
        * @throws IOException
        */
        private void WritePhrase(Phrase phrase, MemoryStream outp) {
            outp.WriteByte(escape);
            outp.Write(paragraphDefaults, 0, paragraphDefaults.Length);
            if (inTable) {
                outp.WriteByte(escape);
                outp.Write(RtfCell.cellInTable, 0, RtfCell.cellInTable.Length);
            }
            foreach (Chunk ch in phrase.Chunks) {
                ch.Font = phrase.Font.Difference(ch.Font);
            }
            MemoryStream save = content;
            content = outp;
            phrase.Process(this);
            content = save;
        }

        /**
        * Write an <code>Anchor</code>. Anchors are treated like Phrases.
        *
        * @param anchor  The <code>Chunk</code> item to be written
        * @param outp     The <code>MemoryStream</code> to write to
        *
        * @throws IOException
        */
        private void WriteAnchor(Anchor anchor, MemoryStream outp) {
            if (anchor.Url != null) {
                outp.WriteByte(openGroup);
                outp.WriteByte(escape);
                outp.Write(field, 0, field.Length);
                outp.WriteByte(openGroup);
                outp.Write(extendedEscape, 0, extendedEscape.Length);
                outp.Write(fieldContent, 0, fieldContent.Length);
                outp.WriteByte(openGroup);
                outp.Write(fieldHyperlink, 0, fieldHyperlink.Length);
                outp.WriteByte(delimiter);
                byte[] t = DocWriter.GetISOBytes(anchor.Url.ToString());
                outp.Write(t, 0, t.Length);
                outp.WriteByte(closeGroup);
                outp.WriteByte(closeGroup);
                outp.WriteByte(openGroup);
                outp.WriteByte(escape);
                outp.Write(fieldDisplay, 0, fieldDisplay.Length);
                outp.WriteByte(delimiter);
                WritePhrase(anchor, outp);
                outp.WriteByte(closeGroup);
                outp.WriteByte(closeGroup);
            } else {
                WritePhrase(anchor, outp);
            }
        }

        /**
        * Write a <code>Chunk</code> and all its font properties.
        *
        * @param chunk The <code>Chunk</code> item to be written
        * @param outp The <code>MemoryStream</code> to write to
        *
        * @throws IOException
        * @throws DocumentException
        */
        private void WriteChunk(Chunk chunk, MemoryStream outp) {
            if (chunk is IRtfField) {
                ((IRtfField) chunk).Write(this, outp);
            } else {
                if (chunk.GetImage() != null) {
                    WriteImage(chunk.GetImage(), outp);
                } else {
                    WriteInitialFontSignature(outp, chunk);
                    byte[] t = DocWriter.GetISOBytes(FilterSpecialChar(chunk.Content, false));
                    outp.Write(t, 0, t.Length);
                    WriteFinishingFontSignature(outp, chunk);
                }
            }
        }


        protected internal void WriteInitialFontSignature(Stream outp, Chunk chunk) {
            Font font = chunk.Font;

            outp.WriteByte(escape);
            outp.WriteByte(fontNumber);
            if (!Util.EqualsIgnoreCase(font.Familyname, "unknown")) {
                WriteInt(outp, AddFont(font));
            } else {
                WriteInt(outp, 0);
            }
            outp.WriteByte(escape);
            outp.Write(fontSize, 0, fontSize.Length);
            if (font.Size > 0) {
                WriteInt(outp, (int) (font.Size * 2));
            } else {
                WriteInt(outp, 20);
            }
            outp.WriteByte(escape);
            outp.Write(fontColor, 0, fontColor.Length);
            WriteInt(outp, AddColor(font.Color));
            if (font.IsBold()) {
                outp.WriteByte(escape);
                outp.WriteByte(bold);
            }
            if (font.IsItalic()) {
                outp.WriteByte(escape);
                outp.WriteByte(italic);
            }
            if (font.IsUnderlined()) {
                outp.WriteByte(escape);
                outp.Write(underline, 0, underline.Length);
            }
            if (font.IsStrikethru()) {
                outp.WriteByte(escape);
                outp.Write(strikethrough, 0, strikethrough.Length);
            }

            /*
            * Superscript / Subscript added by Scott Dietrich (sdietrich@emlab.com)
            */
            if (chunk.Attributes != null) {
                object f = chunk.Attributes[Chunk.SUBSUPSCRIPT];
                if (f != null)
                    if ((float)f > 0) {
                        outp.WriteByte(escape);
                        outp.Write(startSuper, 0, startSuper.Length);
                    } else if ((float)f < 0) {
                        outp.WriteByte(escape);
                        outp.Write(startSub, 0, startSub.Length);
                    }
            }

            outp.WriteByte(delimiter);
        }


        protected internal void WriteFinishingFontSignature(Stream outp, Chunk chunk) {
            Font font = chunk.Font;

            if (font.IsBold()) {
                outp.WriteByte(escape);
                outp.WriteByte(bold);
                WriteInt(outp, 0);
            }
            if (font.IsItalic()) {
                outp.WriteByte(escape);
                outp.WriteByte(italic);
                WriteInt(outp, 0);
            }
            if (font.IsUnderlined()) {
                outp.WriteByte(escape);
                outp.Write(underline, 0, underline.Length);
                WriteInt(outp, 0);
            }
            if (font.IsStrikethru()) {
                outp.WriteByte(escape);
                outp.Write(strikethrough, 0, strikethrough.Length);
                WriteInt(outp, 0);
            }

            /*
            * Superscript / Subscript added by Scott Dietrich (sdietrich@emlab.com)
            */
            if (chunk.Attributes != null) {
                object f = chunk.Attributes[Chunk.SUBSUPSCRIPT];
                if (f != null)
                    if ((float)f != 0) {
                        outp.WriteByte(escape);
                        outp.Write(endSuperSub, 0, endSuperSub.Length);
                    }
            }
        }

        /**
        * Write a <code>ListItem</code>
        *
        * @param listItem The <code>ListItem</code> to be written
        * @param outp The <code>MemoryStream</code> to write to
        *
        * @throws IOException
        * @throws DocumentException
        */
        private void WriteListElement(ListItem listItem, MemoryStream outp) {
            foreach (Chunk ch in listItem.Chunks) {
                AddElement(ch, outp);
            }
            outp.WriteByte(escape);
            outp.Write(paragraph, 0, paragraph.Length);
        }

        /**
        * Write a <code>List</code>
        *
        * @param list The <code>List</code> to be written
        * @param outp The <code>MemoryStream</code> to write to
        *
        * @throws    IOException
        * @throws    DocumentException
        */
        private void WriteList(List list, MemoryStream outp) {
            int type = 0;
            int align = 0;
            int fontNr = AddFont(new Font(Font.SYMBOL, 10, Font.NORMAL, new Color(0, 0, 0)));
            if (!list.IsNumbered()) type = 23;
            if (listLevel == 0) {
                maxListLevel = 0;
                listtable.WriteByte(openGroup);
                listtable.WriteByte(escape);
                listtable.Write(listDefinition, 0, listDefinition.Length);
                int i = GetRandomInt();
                listtable.WriteByte(escape);
                listtable.Write(listTemplateID, 0, listTemplateID.Length);
                WriteInt(listtable, i);
                listtable.WriteByte(escape);
                listtable.Write(hybridList, 0, hybridList.Length);
                listtable.WriteByte((byte) '\n');
            }
            if (listLevel >= maxListLevel) {
                maxListLevel++;
                listtable.WriteByte(openGroup);
                listtable.WriteByte(escape);
                listtable.Write(listLevelDefinition, 0, listLevelDefinition.Length);
                listtable.WriteByte(escape);
                listtable.Write(listLevelTypeOld, 0, listLevelTypeOld.Length);
                WriteInt(listtable, type);
                listtable.WriteByte(escape);
                listtable.Write(listLevelTypeNew, 0, listLevelTypeNew.Length);
                WriteInt(listtable, type);
                listtable.WriteByte(escape);
                listtable.Write(listLevelAlignOld, 0, listLevelAlignOld.Length);
                WriteInt(listtable, align);
                listtable.WriteByte(escape);
                listtable.Write(listLevelAlignNew, 0, listLevelAlignNew.Length);
                WriteInt(listtable, align);
                listtable.WriteByte(escape);
                listtable.Write(listLevelStartAt, 0, listLevelStartAt.Length);
                WriteInt(listtable, 1);
                listtable.WriteByte(openGroup);
                listtable.WriteByte(escape);
                listtable.Write(listLevelTextDefinition, 0, listLevelTextDefinition.Length);
                listtable.WriteByte(escape);
                listtable.Write(listLevelTextLength, 0, listLevelTextLength.Length);
                if (list.IsNumbered()) {
                    WriteInt(listtable, 2);
                } else {
                    WriteInt(listtable, 1);
                }
                listtable.WriteByte(escape);
                if (list.IsNumbered()) {
                    listtable.Write(listLevelTextStyleNumbers, 0, listLevelTextStyleNumbers.Length);
                } else {
                    listtable.Write(listLevelTextStyleBullet, 0, listLevelTextStyleBullet.Length);
                }
                listtable.WriteByte(commaDelimiter);
                listtable.WriteByte(closeGroup);
                listtable.WriteByte(openGroup);
                listtable.WriteByte(escape);
                listtable.Write(listLevelNumbersDefinition, 0, listLevelNumbersDefinition.Length);
                if (list.IsNumbered()) {
                    listtable.WriteByte(delimiter);
                    listtable.Write(listLevelNumbers, 0, listLevelNumbers.Length);
                    WriteInt(listtable, listLevel + 1);
                }
                listtable.WriteByte(commaDelimiter);
                listtable.WriteByte(closeGroup);
                if (!list.IsNumbered()) {
                    listtable.WriteByte(escape);
                    listtable.WriteByte(fontNumber);
                    WriteInt(listtable, fontNr);
                }
                listtable.WriteByte(escape);
                listtable.Write(firstIndent, 0, firstIndent.Length);
                WriteInt(listtable, (int) (list.IndentationLeft * TWIPSFACTOR * -1));
                listtable.WriteByte(escape);
                listtable.Write(listIndent, 0, listIndent.Length);
                WriteInt(listtable, (int) ((list.IndentationLeft + list.SymbolIndent) * TWIPSFACTOR));
                listtable.WriteByte(escape);
                listtable.Write(rightIndent, 0, rightIndent.Length);
                WriteInt(listtable, (int) (list.IndentationRight * TWIPSFACTOR));
                listtable.WriteByte(escape);
                listtable.Write(tabStop, 0, tabStop.Length);
                WriteInt(listtable, (int) (list.SymbolIndent * TWIPSFACTOR));
                listtable.WriteByte(closeGroup);
                listtable.WriteByte((byte) '\n');
            }
            // Actual List Begin in Content
            outp.WriteByte(escape);
            outp.Write(paragraphDefaults, 0, paragraphDefaults.Length);
            outp.WriteByte(escape);
            outp.Write(alignLeft, 0, alignLeft.Length);
            outp.WriteByte(escape);
            outp.Write(firstIndent, 0, firstIndent.Length);
            WriteInt(outp, (int) (list.IndentationLeft * TWIPSFACTOR * -1));
            outp.WriteByte(escape);
            outp.Write(listIndent, 0, listIndent.Length);
            WriteInt(outp, (int) ((list.IndentationLeft + list.SymbolIndent) * TWIPSFACTOR));
            outp.WriteByte(escape);
            outp.Write(rightIndent, 0, rightIndent.Length);
            WriteInt(outp, (int) (list.IndentationRight * TWIPSFACTOR));
            outp.WriteByte(escape);
            outp.Write(fontSize, 0, fontSize.Length);
            WriteInt(outp, 20);
            outp.WriteByte(escape);
            outp.Write(listBegin, 0, listBegin.Length);
            WriteInt(outp, currentListID);
            if (listLevel > 0) {
                outp.WriteByte(escape);
                outp.Write(listCurrentLevel, 0, listCurrentLevel.Length);
                WriteInt(outp, listLevel);
            }
            outp.WriteByte(openGroup);
            ListIterator listItems = new ListIterator(list.Items);
            IElement listElem;
            int count = 1;
            while (listItems.HasNext()) {
                listElem = (IElement) listItems.Next();
                if (listElem.Type == Element.CHUNK) {
                    listElem = new ListItem((Chunk) listElem);
                }
                if (listElem.Type == Element.LISTITEM) {
                    outp.WriteByte(openGroup);
                    outp.WriteByte(escape);
                    outp.Write(listTextOld, 0, listTextOld.Length);
                    outp.WriteByte(escape);
                    outp.Write(paragraphDefaults, 0, paragraphDefaults.Length);
                    outp.WriteByte(escape);
                    outp.WriteByte(fontNumber);
                    if (list.IsNumbered()) {
                        WriteInt(outp, AddFont(new Font(Font.TIMES_ROMAN, Font.NORMAL, 10, new Color(0, 0, 0))));
                    } else {
                        WriteInt(outp, fontNr);
                    }
                    outp.WriteByte(escape);
                    outp.Write(firstIndent, 0, firstIndent.Length);
                    WriteInt(outp, (int) (list.IndentationLeft * TWIPSFACTOR * -1));
                    outp.WriteByte(escape);
                    outp.Write(listIndent, 0, listIndent.Length);
                    WriteInt(outp, (int) ((list.IndentationLeft + list.SymbolIndent) * TWIPSFACTOR));
                    outp.WriteByte(escape);
                    outp.Write(rightIndent, 0, rightIndent.Length);
                    WriteInt(outp, (int) (list.IndentationRight * TWIPSFACTOR));
                    outp.WriteByte(delimiter);
                    if (list.IsNumbered()) {
                        WriteInt(outp, count);
                        outp.WriteByte((byte)'.');
                    } else {
                        outp.WriteByte(escape);
                        outp.Write(listBulletOld, 0, listBulletOld.Length);
                    }
                    outp.WriteByte(escape);
                    outp.Write(tab, 0, tab.Length);
                    outp.WriteByte(closeGroup);
                    WriteListElement((ListItem) listElem, outp);
                    count++;
                } else if (listElem.Type == Element.LIST) {
                    listLevel++;
                    WriteList((List) listElem, outp);
                    listLevel--;
                    outp.WriteByte(escape);
                    outp.Write(paragraphDefaults, 0, paragraphDefaults.Length);
                    outp.WriteByte(escape);
                    outp.Write(alignLeft, 0, alignLeft.Length);
                    outp.WriteByte(escape);
                    outp.Write(firstIndent, 0, firstIndent.Length);
                    WriteInt(outp, (int) (list.IndentationLeft * TWIPSFACTOR * -1));
                    outp.WriteByte(escape);
                    outp.Write(listIndent, 0, listIndent.Length);
                    WriteInt(outp, (int) ((list.IndentationLeft + list.SymbolIndent) * TWIPSFACTOR));
                    outp.WriteByte(escape);
                    outp.Write(rightIndent, 0, rightIndent.Length);
                    WriteInt(outp, (int) (list.IndentationRight * TWIPSFACTOR));
                    outp.WriteByte(escape);
                    outp.Write(fontSize, 0, fontSize.Length);
                    WriteInt(outp, 20);
                    outp.WriteByte(escape);
                    outp.Write(listBegin, 0, listBegin.Length);
                    WriteInt(outp, currentListID);
                    if (listLevel > 0) {
                        outp.WriteByte(escape);
                        outp.Write(listCurrentLevel, 0, listCurrentLevel.Length);
                        WriteInt(outp, listLevel);
                    }
                }
                outp.WriteByte((byte) '\n');
            }
            outp.WriteByte(closeGroup);
            if (listLevel == 0) {
                int i = GetRandomInt();
                listtable.WriteByte(escape);
                listtable.Write(listID, 0, listID.Length);
                WriteInt(listtable, i);
                listtable.WriteByte(closeGroup);
                listtable.WriteByte((byte) '\n');
                listoverride.WriteByte(openGroup);
                listoverride.WriteByte(escape);
                listoverride.Write(listOverride, 0, listOverride.Length);
                listoverride.WriteByte(escape);
                listoverride.Write(listID, 0, listID.Length);
                WriteInt(listoverride, i);
                listoverride.WriteByte(escape);
                listoverride.Write(listOverrideCount, 0, listOverrideCount.Length);
                WriteInt(listoverride, 0);
                listoverride.WriteByte(escape);
                listoverride.Write(listBegin, 0, listBegin.Length);
                WriteInt(listoverride, currentListID);
                currentListID++;
                listoverride.WriteByte(closeGroup);
                listoverride.WriteByte((byte) '\n');
            }
            outp.WriteByte(escape);
            outp.Write(paragraphDefaults, 0, paragraphDefaults.Length);
        }

        /**
        * Write a <code>Table</code>.
        *
        * @param table The <code>table</code> to be written
        * @param outp The <code>MemoryStream</code> to write to
        *
        * Currently no nesting of tables is supported. If a cell contains anything but a Cell Object it is ignored.
        *
        * @throws IOException
        * @throws DocumentException
        */
        private void WriteTable(Table table, MemoryStream outp) {
            inTable = true;
            table.Complete();
            RtfTable rtfTable = new RtfTable(this);
            rtfTable.ImportTable(table, pageWidth - marginLeft - marginRight);
            rtfTable.WriteTable(outp);
            inTable = false;
        }


        /**
        * Write an <code>Image</code>.
        *
        * @param image The <code>image</code> to be written
        * @param outp The <code>MemoryStream</code> to write to
        *
        * At the moment only PNG and JPEG Images are supported.
        *
        * @throws IOException
        * @throws DocumentException
        */
        private void WriteImage(Image image, MemoryStream outp) {
            int type = image.OriginalType;
            if (!(type == Image.ORIGINAL_JPEG || type == Image.ORIGINAL_BMP
                || type == Image.ORIGINAL_PNG || type == Image.ORIGINAL_WMF))
                throw new DocumentException("Only BMP, PNG, WMF and JPEG images are supported by the RTF Writer");
            switch (image.Alignment) {
                case Element.ALIGN_LEFT:
                    outp.WriteByte(escape);
                    outp.Write(alignLeft, 0, alignLeft.Length);
                    break;
                case Element.ALIGN_RIGHT:
                    outp.WriteByte(escape);
                    outp.Write(alignRight, 0, alignRight.Length);
                    break;
                case Element.ALIGN_CENTER:
                    outp.WriteByte(escape);
                    outp.Write(alignCenter, 0, alignCenter.Length);
                    break;
                case Element.ALIGN_JUSTIFIED:
                    outp.WriteByte(escape);
                    outp.Write(alignJustify, 0, alignJustify.Length);
                    break;
            }
            outp.WriteByte(openGroup);
            outp.Write(extendedEscape, 0, extendedEscape.Length);
            outp.Write(pictureGroup, 0, pictureGroup.Length);
            outp.WriteByte(openGroup);
            outp.WriteByte(escape);
            outp.Write(picture, 0, picture.Length);
            outp.WriteByte(escape);
            switch (type) {
                case Image.ORIGINAL_JPEG:
                    outp.Write(pictureJPEG, 0, pictureJPEG.Length);
                    break;
                case Image.ORIGINAL_PNG:
                    outp.Write(picturePNG, 0, picturePNG.Length);
                    break;
                case Image.ORIGINAL_WMF:
                case Image.ORIGINAL_BMP:
                    outp.Write(pictureWMF, 0, pictureWMF.Length);
                    break;
            }
            outp.WriteByte(escape);
            outp.Write(pictureWidth, 0, pictureWidth.Length);
            WriteInt(outp, (int) (image.PlainWidth * TWIPSFACTOR));
            outp.WriteByte(escape);
            outp.Write(pictureHeight, 0, pictureHeight.Length);
            WriteInt(outp, (int) (image.PlainHeight * TWIPSFACTOR));


    // For some reason this messes up the intended image size. It makes it too big. Weird
    //
    //        outp.WriteByte(escape);
    //        outp.Write(pictureIntendedWidth);
    //        WriteInt(outp, (int) (image.PlainWidth() * twipsFactor));
    //        outp.WriteByte(escape);
    //        outp.Write(pictureIntendedHeight);
    //        WriteInt(outp, (int) (image.PlainHeight() * twipsFactor));


            if (image.Width > 0) {
                outp.WriteByte(escape);
                outp.Write(pictureScaleX, 0, pictureScaleX.Length);
                WriteInt(outp, (int) (100 / image.Width * image.PlainWidth));
            }
            if (image.Height > 0) {
                outp.WriteByte(escape);
                outp.Write(pictureScaleY, 0, pictureScaleY.Length);
                WriteInt(outp, (int) (100 / image.Height * image.PlainHeight));
            }
            outp.WriteByte(delimiter);
            Stream imgIn;
            if (type == Image.ORIGINAL_BMP) {
                imgIn = new MemoryStream(MetaDo.WrapBMP(image));
            }
            else {
                if (image.OriginalData == null) {
#if !NETCF
                    imgIn = WebRequest.Create(image.Url).GetResponse().GetResponseStream();
#else
                    imgIn=new FileStream(image.Url.LocalPath, FileMode.Open);
#endif
                } else {
                    imgIn = new MemoryStream(image.OriginalData);
                }
                if (type == Image.ORIGINAL_WMF) { //remove the placeable header
                        Image.Skip(imgIn, 22);
                }
            }
            int buffer = -1;
            int count = 0;
            outp.WriteByte((byte) '\n');
            while ((buffer = imgIn.ReadByte()) != -1) {
                String helperStr = buffer.ToString("X2");
                byte[] t = DocWriter.GetISOBytes(helperStr);
                outp.Write(t, 0, t.Length);
                count++;
                if (count == 64) {
                    outp.WriteByte((byte) '\n');
                    count = 0;
                }
            }
            imgIn.Close();
            outp.WriteByte(closeGroup);
            outp.WriteByte(closeGroup);
            outp.WriteByte((byte) '\n');
        }

        /**
        * Write an <code>Annotation</code>
        *
        * @param annotationElement The <code>Annotation</code> to be written
        * @param outp The <code>MemoryStream</code> to write to
        *
        * @throws IOException
        */
        private void WriteAnnotation(Annotation annotationElement, MemoryStream outp) {
            int id = GetRandomInt();
            outp.WriteByte(openGroup);
            outp.Write(extendedEscape, 0, extendedEscape.Length);
            outp.Write(annotationID, 0, annotationID.Length);
            outp.WriteByte(delimiter);
            WriteInt(outp, id);
            outp.WriteByte(closeGroup);
            outp.WriteByte(openGroup);
            outp.Write(extendedEscape, 0, extendedEscape.Length);
            outp.Write(annotationAuthor, 0, annotationAuthor.Length);
            outp.WriteByte(delimiter);
            byte[] t = DocWriter.GetISOBytes(annotationElement.Title);
            outp.Write(t, 0, t.Length);
            outp.WriteByte(closeGroup);
            outp.WriteByte(openGroup);
            outp.Write(extendedEscape, 0, extendedEscape.Length);
            outp.Write(annotation, 0, annotation.Length);
            outp.WriteByte(escape);
            outp.Write(paragraphDefaults, 0, paragraphDefaults.Length);
            outp.WriteByte(delimiter);
            t = DocWriter.GetISOBytes(annotationElement.Content);
            outp.Write(t, 0, t.Length);
            outp.WriteByte(closeGroup);
        }

        /**
        * Add a <code>Meta</code> element. It is written to the Inforamtion Group
        * and merged with the main <code>MemoryStream</code> when the
        * Document is closed.
        *
        * @param metaName The type of <code>Meta</code> element to be added
        * @param meta The <code>Meta</code> element to be added
        *
        * Currently only the Meta Elements Author, Subject, Keywords, Title, Producer and CreationDate are supported.
        *
        * @throws IOException
        */
        private void WriteMeta(byte[] metaName, Meta meta) {
            info.WriteByte(openGroup);
            try {
                info.WriteByte(escape);
                info.Write(metaName, 0, metaName.Length);
                info.WriteByte(delimiter);
                if (meta.Type == Element.CREATIONDATE) {
                    WriteFormatedDateTime(meta.Content);
                } else {
                    byte[] t = DocWriter.GetISOBytes(meta.Content);
                    info.Write(t, 0, t.Length);
                }
            } finally {
                info.WriteByte(closeGroup);
            }
        }

        /**
        * Writes a date. The date is formated <strong>Year, Month, Day, Hour, Minute, Second</strong>
        *
        * @param date The date to be written
        *
        * @throws IOException
        */
        private void WriteFormatedDateTime(String date) {
            DateTime d;
            try {
                d = DateTime.Parse(date);
            }
            catch {
                d = DateTime.Now;
            }   
            info.WriteByte(escape);
            info.Write(year, 0, year.Length);
            WriteInt(info, d.Year);
            info.WriteByte(escape);
            info.Write(month, 0, month.Length);
            WriteInt(info, d.Month);
            info.WriteByte(escape);
            info.Write(day, 0, day.Length);
            WriteInt(info, d.Day);
            info.WriteByte(escape);
            info.Write(hour, 0, hour.Length);
            WriteInt(info, d.Hour);
            info.WriteByte(escape);
            info.Write(minute, 0, minute.Length);
            WriteInt(info, d.Minute);
            info.WriteByte(escape);
            info.Write(second, 0, second.Length);
            WriteInt(info, d.Second);
        }

        /**
        * Add a new <code>Font</code> to the list of fonts. If the <code>Font</code>
        * already exists in the list of fonts, then it is not added again.
        *
        * @param newFont The <code>Font</code> to be added
        *
        * @return The index of the <code>Font</code> in the font list
        */
        protected internal int AddFont(Font newFont) {
            int fn = -1;

            for (int i = 0; i < fontList.Count; i++) {
                if (newFont.Familyname.Equals(((Font) fontList[i]).Familyname)) {
                    fn = i;
                }
            }
            if (fn == -1) {
                fontList.Add(newFont);
                return fontList.Count - 1;
            }
            return fn;
        }

        /**
        * Add a new <code>Color</code> to the list of colours. If the <code>Color</code>
        * already exists in the list of colours, then it is not added again.
        *
        * @param newColor The <code>Color</code> to be added
        *
        * @return The index of the <code>color</code> in the colour list
        */
        protected internal int AddColor(Color newColor) {
            int cn = 0;
            if (newColor == null) {
                return cn;
            }
            cn = colorList.IndexOf(newColor);
            if (cn == -1) {
                colorList.Add(newColor);
                return colorList.Count - 1;
            }
            return cn;
        }

        /**
        * Merge all the different <code>ArrayList</code>s and <code>MemoryStream</code>s
        * to the final <code>MemoryStream</code>
        *
        * @return <code>true</code> if all information was sucessfully written to the <code>MemoryStream</code>
        */
        private bool WriteDocument() {
            try {
                WriteDocumentIntro();
                WriteFontList();
                os.WriteByte((byte) '\n');
                WriteColorList();
                os.WriteByte((byte) '\n');
                WriteList();
                os.WriteByte((byte) '\n');
                WriteInfoGroup();
                os.WriteByte((byte) '\n');
                WriteDocumentFormat();
                os.WriteByte((byte) '\n');
                MemoryStream hf = new MemoryStream();
                WriteSectionDefaults(hf);
                hf.WriteTo(os);
                content.WriteTo(os);
                os.WriteByte(closeGroup);
                return true;
            } catch (IOException) {
                //System.err.Println(e.GetMessage());
                return false;
            }

        }

        /** Write the Rich Text file settings
        * @throws IOException
        */
        private void WriteDocumentIntro() {
            os.WriteByte(openGroup);
            os.WriteByte(escape);
            os.Write(docBegin, 0, docBegin.Length);
            os.WriteByte(escape);
            os.Write(ansi, 0, ansi.Length);
            os.WriteByte(escape);
            os.Write(ansiCodepage, 0, ansiCodepage.Length);
            WriteInt(os, 1252);
            os.WriteByte((byte)'\n');
            os.WriteByte(escape);
            os.Write(defaultFont, 0, defaultFont.Length);
            WriteInt(os, 0);
        }

        /**
        * Write the font list to the final <code>MemoryStream</code>
        * @throws IOException
        */
        private void WriteFontList() {
            Font fnt;

            os.WriteByte(openGroup);
            os.WriteByte(escape);
            os.Write(fontTable, 0, fontTable.Length);
            for (int i = 0; i < fontList.Count; i++) {
                fnt = (Font) fontList[i];
                os.WriteByte(openGroup);
                os.WriteByte(escape);
                os.WriteByte(fontNumber);
                WriteInt(os, i);
                os.WriteByte(escape);
                switch (Font.GetFamilyIndex(fnt.Familyname)) {
                    case Font.COURIER:
                        os.Write(fontModern, 0, fontModern.Length);
                        os.WriteByte(escape);
                        os.Write(fontCharset, 0, fontCharset.Length);
                        WriteInt(os, 0);
                        os.WriteByte(delimiter);
                        os.Write(fontCourier, 0, fontCourier.Length);
                        break;
                    case Font.HELVETICA:
                        os.Write(fontSwiss, 0, fontSwiss.Length);
                        os.WriteByte(escape);
                        os.Write(fontCharset, 0, fontCharset.Length);
                        WriteInt(os, 0);
                        os.WriteByte(delimiter);
                        os.Write(fontArial, 0, fontArial.Length);
                        break;
                    case Font.SYMBOL:
                        os.Write(fontRoman, 0, fontRoman.Length);
                        os.WriteByte(escape);
                        os.Write(fontCharset, 0, fontCharset.Length);
                        WriteInt(os, 2);
                        os.WriteByte(delimiter);
                        os.Write(fontSymbol, 0, fontSymbol.Length);
                        break;
                    case Font.TIMES_ROMAN:
                        os.Write(fontRoman, 0, fontRoman.Length);
                        os.WriteByte(escape);
                        os.Write(fontCharset, 0, fontCharset.Length);
                        WriteInt(os, 0);
                        os.WriteByte(delimiter);
                        os.Write(fontTimesNewRoman, 0, fontTimesNewRoman.Length);
                        break;
                    case Font.ZAPFDINGBATS:
                        os.Write(fontTech, 0, fontTech.Length);
                        os.WriteByte(escape);
                        os.Write(fontCharset, 0, fontCharset.Length);
                        WriteInt(os, 0);
                        os.WriteByte(delimiter);
                        os.Write(fontWindings, 0, fontWindings.Length);
                        break;
                    default:
                        os.Write(fontRoman, 0, fontRoman.Length);
                        os.WriteByte(escape);
                        os.Write(fontCharset, 0, fontCharset.Length);
                        WriteInt(os, 0);
                        os.WriteByte(delimiter);
                        byte[] t = DocWriter.GetISOBytes(FilterSpecialChar(fnt.Familyname, true));
                        os.Write(t, 0, t.Length);
                        break;
                }
                os.WriteByte(commaDelimiter);
                os.WriteByte(closeGroup);
            }
            os.WriteByte(closeGroup);
        }

        /**
        * Write the colour list to the final <code>MemoryStream</code>
        * @throws IOException
        */
        private void WriteColorList() {
            Color color = null;

            os.WriteByte(openGroup);
            os.WriteByte(escape);
            os.Write(colorTable, 0, colorTable.Length);
            for (int i = 0; i < colorList.Count; i++) {
                color = (Color) colorList[i];
                os.WriteByte(escape);
                os.Write(colorRed, 0, colorRed.Length);
                WriteInt(os, color.R);
                os.WriteByte(escape);
                os.Write(colorGreen, 0, colorGreen.Length);
                WriteInt(os, color.G);
                os.WriteByte(escape);
                os.Write(colorBlue, 0, colorBlue.Length);
                WriteInt(os, color.B);
                os.WriteByte(commaDelimiter);
            }
            os.WriteByte(closeGroup);
        }

        /**
        * Write the Information Group to the final <code>MemoryStream</code>
        * @throws IOException
        */
        private void WriteInfoGroup() {
            os.WriteByte(openGroup);
            os.WriteByte(escape);
            os.Write(infoBegin, 0, infoBegin.Length);
            info.WriteTo(os);
            os.WriteByte(closeGroup);
        }

        /**
        * Write the listtable and listoverridetable to the final <code>MemoryStream</code>
        * @throws IOException
        */
        private void WriteList() {
            listtable.WriteByte(closeGroup);
            listoverride.WriteByte(closeGroup);
            listtable.WriteTo(os);
            os.WriteByte((byte) '\n');
            listoverride.WriteTo(os);
        }

        /**
        * Write an integer
        *
        * @param outp The <code>OuputStream</code> to which the <code>int</code> value is to be written
        * @param i The <code>int</code> value to be written
        * @throws IOException
        */
        public static void WriteInt(Stream outp, int i) {
            byte[] t = DocWriter.GetISOBytes(i.ToString());
            outp.Write(t, 0, t.Length);
        }

        /**
        * Get a random integer.
        * This returns a <b>unique</b> random integer to be used with listids.
        *
        * @return Random <code>int</code> value.
        */
        private int GetRandomInt() {
            int newInt;
            while (true) {
                lock (random) {
                    newInt = random.Next(int.MaxValue - 2);
                }
                if (!listIds.Contains(newInt))
                    break;
            }
            listIds.Add(newInt);
            return newInt;
        }

        /**
        * Write the current header and footer to a <code>MemoryStream</code>
        *
        * @param os        The <code>MemoryStream</code> to which the header and footer will be written.
        * @throws IOException
        */
        public void WriteHeadersFooters(MemoryStream os) {
            if (this.footer is RtfHeaderFooters) {
                RtfHeaderFooters rtfHf = (RtfHeaderFooters) this.footer;
                HeaderFooter hf = rtfHf.Get(RtfHeaderFooters.ALL_PAGES);
                if (hf != null) {
                    WriteHeaderFooter(hf, footerBegin, os);
                }
                hf = rtfHf.Get(RtfHeaderFooters.LEFT_PAGES);
                if (hf != null) {
                    WriteHeaderFooter(hf, footerlBegin, os);
                }
                hf = rtfHf.Get(RtfHeaderFooters.RIGHT_PAGES);
                if (hf != null) {
                    WriteHeaderFooter(hf, footerrBegin, os);
                }
                hf = rtfHf.Get(RtfHeaderFooters.FIRST_PAGE);
                if (hf != null) {
                    WriteHeaderFooter(hf, footerfBegin, os);
                }
            } else {
                WriteHeaderFooter(this.footer, footerBegin, os);
            }
            if (this.header is RtfHeaderFooters) {
                RtfHeaderFooters rtfHf = (RtfHeaderFooters) this.header;
                HeaderFooter hf = rtfHf.Get(RtfHeaderFooters.ALL_PAGES);
                if (hf != null) {
                    WriteHeaderFooter(hf, headerBegin, os);
                }
                hf = rtfHf.Get(RtfHeaderFooters.LEFT_PAGES);
                if (hf != null) {
                    WriteHeaderFooter(hf, headerlBegin, os);
                }
                hf = rtfHf.Get(RtfHeaderFooters.RIGHT_PAGES);
                if (hf != null) {
                    WriteHeaderFooter(hf, headerrBegin, os);
                }
                hf = rtfHf.Get(RtfHeaderFooters.FIRST_PAGE);
                if (hf != null) {
                    WriteHeaderFooter(hf, headerfBegin, os);
                }
            } else {
                WriteHeaderFooter(this.header, headerBegin, os);
            }
        }

        /**
        * Write a <code>HeaderFooter</code> to a <code>MemoryStream</code>
        *
        * @param headerFooter  The <code>HeaderFooter</code> object to be written.
        * @param hfType        The type of header or footer to be added.
        * @param target        The <code>MemoryStream</code> to which the <code>HeaderFooter</code> will be written.
        * @throws IOException
        */
        private void WriteHeaderFooter(HeaderFooter headerFooter, byte[] hfType, MemoryStream target) {
            inHeaderFooter = true;
            try {
                target.WriteByte(openGroup);
                target.WriteByte(escape);
                target.Write(hfType, 0, hfType.Length);
                target.WriteByte(delimiter);
                if (headerFooter != null) {
                    if (headerFooter is RtfHeaderFooter && ((RtfHeaderFooter) headerFooter).Content() != null) {
                        this.AddElement(((RtfHeaderFooter) headerFooter).Content(), target);
                    } else {
                        Paragraph par = new Paragraph();
                        par.Alignment = headerFooter.Alignment;
                        if (headerFooter.Before != null) {
                            par.Add(headerFooter.Before);
                        }
                        if (headerFooter.IsNumbered()) {
                            par.Add(new RtfPageNumber("", headerFooter.Before.Font));
                        }
                        if (headerFooter.After != null) {
                            par.Add(headerFooter.After);
                        }
                        this.AddElement(par, target);
                    }
                }
                target.WriteByte(closeGroup);
            } catch (DocumentException e) {
                throw new IOException("DocumentException - " + e.ToString());
            }
            inHeaderFooter = false;
        }

        /**
        *  Write the <code>Document</code>'s Paper and Margin Size
        *  to the final <code>MemoryStream</code>
        * @throws IOException
        */
        private void WriteDocumentFormat() {
    //        os.WriteByte(openGroup);
            os.WriteByte(escape);
            os.Write(rtfPaperWidth, 0, rtfPaperWidth.Length);
            WriteInt(os, pageWidth);
            os.WriteByte(escape);
            os.Write(rtfPaperHeight, 0, rtfPaperHeight.Length);
            WriteInt(os, pageHeight);
            os.WriteByte(escape);
            os.Write(rtfMarginLeft, 0, rtfMarginLeft.Length);
            WriteInt(os, marginLeft);
            os.WriteByte(escape);
            os.Write(rtfMarginRight, 0, rtfMarginRight.Length);
            WriteInt(os, marginRight);
            os.WriteByte(escape);
            os.Write(rtfMarginTop, 0, rtfMarginTop.Length);
            WriteInt(os, marginTop);
            os.WriteByte(escape);
            os.Write(rtfMarginBottom, 0, rtfMarginBottom.Length);
            WriteInt(os, marginBottom);
    //        os.WriteByte(closeGroup);
        }

        /**
        * Initialise all helper classes.
        * Clears alls lists, creates new <code>MemoryStream</code>'s
        */
        private void InitDefaults() {
            fontList.Clear();
            colorList.Clear();
            info = new MemoryStream();
            content = new MemoryStream();
            listtable = new MemoryStream();
            listoverride = new MemoryStream();
            document.AddProducer();
            document.AddCreationDate();
            AddFont(new Font(Font.TIMES_ROMAN, 10, Font.NORMAL));
            AddColor(new Color(0, 0, 0));
            AddColor(new Color(255, 255, 255));
            listIds = new ArrayList();
            try {
                listtable.WriteByte(openGroup);
                listtable.Write(extendedEscape, 0, extendedEscape.Length);
                listtable.Write(listtableGroup, 0, listtableGroup.Length);
                listtable.WriteByte((byte) '\n');
                listoverride.WriteByte(openGroup);
                listoverride.Write(extendedEscape, 0, extendedEscape.Length);
                listoverride.Write(listoverridetableGroup, 0, listoverridetableGroup.Length);
                listoverride.WriteByte((byte) '\n');
            } catch (IOException) {
                //System.err.Println("InitDefaultsError" + e);
            }
        }

        /**
        * Writes the default values for the current Section
        *
        * @param outp The <code>MemoryStream</code> to be written to
        * @throws IOException
        */
        private void WriteSectionDefaults(MemoryStream outp) {
            if (header is RtfHeaderFooters || footer is RtfHeaderFooters) {
                RtfHeaderFooters rtfHeader = (RtfHeaderFooters) header;
                RtfHeaderFooters rtfFooter = (RtfHeaderFooters) footer;
                if ((rtfHeader != null && (rtfHeader.Get(RtfHeaderFooters.LEFT_PAGES) != null || rtfHeader.Get(RtfHeaderFooters.RIGHT_PAGES) != null)) || (rtfFooter != null && (rtfFooter.Get(RtfHeaderFooters.LEFT_PAGES) != null || rtfFooter.Get(RtfHeaderFooters.RIGHT_PAGES) != null))) {
                    outp.WriteByte(escape);
                    outp.Write(facingPages, 0, facingPages.Length);
                }
            }
            if (hasTitlePage) {
                outp.WriteByte(escape);
                outp.Write(titlePage, 0, titlePage.Length);
            }
            WriteHeadersFooters(outp);
            if (landscape) {
                //outp.WriteByte(escape);
                //outp.Write(landscapeTag1, 0, landscapeTag1.Length);
                outp.WriteByte(escape);
                outp.Write(landscapeTag2, 0, landscapeTag2.Length);
                outp.WriteByte(escape);
                outp.Write(sectionPageWidth, 0, sectionPageWidth.Length);
                WriteInt(outp, pageWidth);
                outp.WriteByte(escape);
                outp.Write(sectionPageHeight, 0, sectionPageHeight.Length);
                WriteInt(outp, pageHeight);
            } else {
                outp.WriteByte(escape);
                outp.Write(sectionPageWidth, 0, sectionPageWidth.Length);
                WriteInt(outp, pageWidth);
                outp.WriteByte(escape);
                outp.Write(sectionPageHeight, 0, sectionPageHeight.Length);
                WriteInt(outp, pageHeight);
            }
        }

        /**
        * This method tries to fit the <code>Rectangle pageSize</code> to one of the predefined PageSize rectangles.
        * If a match is found the pageWidth and pageHeight will be set according to values determined from files
        * generated by MS Word2000 and OpenOffice 641. If no match is found the method will try to match the rotated
        * Rectangle by calling itself with the parameter rotate set to true.
        * @param pageSize a rectangle defining the size of the page
        * @param rotate portrait or lanscape?
        * @return true if the format parsing succeeded
        */
        private bool ParseFormat(Rectangle pageSize, bool rotate) {
            if (rotate) {
                pageSize = pageSize.Rotate();
            }
            if (RectEquals(pageSize, PageSize.A3)) {
                pageWidth = 16837;
                pageHeight = 23811;
                landscape = rotate;
                return true;
            }
            if (RectEquals(pageSize, PageSize.A4)) {
                pageWidth = 11907;
                pageHeight = 16840;
                landscape = rotate;
                return true;
            }
            if (RectEquals(pageSize, PageSize.A5)) {
                pageWidth = 8391;
                pageHeight = 11907;
                landscape = rotate;
                return true;
            }
            if (RectEquals(pageSize, PageSize.A6)) {
                pageWidth = 5959;
                pageHeight = 8420;
                landscape = rotate;
                return true;
            }
            if (RectEquals(pageSize, PageSize.B4)) {
                pageWidth = 14570;
                pageHeight = 20636;
                landscape = rotate;
                return true;
            }
            if (RectEquals(pageSize, PageSize.B5)) {
                pageWidth = 10319;
                pageHeight = 14572;
                landscape = rotate;
                return true;
            }
            if (RectEquals(pageSize, PageSize.HALFLETTER)) {
                pageWidth = 7927;
                pageHeight = 12247;
                landscape = rotate;
                return true;
            }
            if (RectEquals(pageSize, PageSize.LETTER)) {
                pageWidth = 12242;
                pageHeight = 15842;
                landscape = rotate;
                return true;
            }
            if (RectEquals(pageSize, PageSize.LEGAL)) {
                pageWidth = 12252;
                pageHeight = 20163;
                landscape = rotate;
                return true;
            }
            if (!rotate && ParseFormat(pageSize, true)) {
                int x = pageWidth;
                pageWidth = pageHeight;
                pageHeight = x;
                return true;
            }
            return false;
        }

        /**
        * This method compares to Rectangles. They are considered equal if width and height are the same
        * @param rect1
        * @param rect2
        * @return true if rect1 and rect2 represent the same rectangle
        */
        private static bool RectEquals(Rectangle rect1, Rectangle rect2) {
            return (rect1.Width == rect2.Width) && (rect1.Height == rect2.Height);
        }

        /**
        * Returns whether we are currently writing a header or footer
        *
        * @return the value of inHeaderFooter
        */
        public bool WritingHeaderFooter() {
            return inHeaderFooter;
        }

        /**
        * Replaces special characters with their unicode values
        *
        * @param str The original <code>String</code>
        * @param useHex
        * @return The converted String
        */
        public static String FilterSpecialChar(String str, bool useHex) {
            int length = str.Length;
            int z = (int) 'z';
            StringBuilder ret = new StringBuilder(length);
            for (int i = 0; i < length; i++) {
                char ch = str[i];

                if (ch == '\\') {
                    ret.Append("\\\\");
                } else if (ch == '\n') {
                    ret.Append("\\par ");
                } else if (((int) ch) > z) {
                    if (useHex) {
                        ret.Append("\\\'").Append(((long)ch).ToString("X"));
                    } else {
                    ret.Append("\\u").Append((long) ch).Append('?');
                    }
                } else {
                    ret.Append(ch);
                }
            }
            String s = ret.ToString();
            if (s.IndexOf("$newpage$") >= 0) {
                String before = s.Substring(0, s.IndexOf("$newpage$"));
                String after = s.Substring(s.IndexOf("$newpage$") + 9);
                ret = new StringBuilder(before);
                ret.Append("\\page\\par ");
                ret.Append(after);
                return ret.ToString();
            }
            return s;
        }

        private void AddHeaderFooterFontColor(HeaderFooter hf) {
            if (hf is RtfHeaderFooter) {
                RtfHeaderFooter rhf = (RtfHeaderFooter) hf;
                if (rhf.Content() is Chunk) {
                    AddFont(((Chunk) rhf.Content()).Font);
                    AddColor(((Chunk) rhf.Content()).Font.Color);
                } else if (rhf.Content() is Phrase) {
                    AddFont(((Phrase) rhf.Content()).Font);
                    AddColor(((Phrase) rhf.Content()).Font.Color);
                }
            }
            if (hf.Before != null) {
                AddFont(hf.Before.Font);
                AddColor(hf.Before.Font.Color);
            }
            if (hf.After != null) {
                AddFont(hf.After.Font);
                AddColor(hf.After.Font.Color);
            }
        }

        private void ProcessHeaderFooter(HeaderFooter hf) {
            if (hf != null) {
                if (hf is RtfHeaderFooters) {
                    RtfHeaderFooters rhf = (RtfHeaderFooters) hf;
                    if (rhf.Get(RtfHeaderFooters.ALL_PAGES) != null) {
                        AddHeaderFooterFontColor(rhf.Get(RtfHeaderFooters.ALL_PAGES));
                    }
                    if (rhf.Get(RtfHeaderFooters.LEFT_PAGES) != null) {
                        AddHeaderFooterFontColor(rhf.Get(RtfHeaderFooters.LEFT_PAGES));
                    }
                    if (rhf.Get(RtfHeaderFooters.RIGHT_PAGES) != null) {
                        AddHeaderFooterFontColor(rhf.Get(RtfHeaderFooters.RIGHT_PAGES));
                    }
                    if (rhf.Get(RtfHeaderFooters.FIRST_PAGE) != null) {
                        AddHeaderFooterFontColor(rhf.Get(RtfHeaderFooters.FIRST_PAGE));
                    }
                } else {
                    AddHeaderFooterFontColor(hf);
                }
            }
        }
        
        /**
        * @see com.lowagie.text.DocListener#setMarginMirroring(bool)
        */
        public override bool SetMarginMirroring(bool MarginMirroring) {
            return false;
        }
        
    }
}