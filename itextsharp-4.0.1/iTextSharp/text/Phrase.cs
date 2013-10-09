using System;
using System.Text;
using System.Collections;
using System.util;
using System.Runtime.CompilerServices;

using iTextSharp.text.html;

namespace iTextSharp.text {
    /// <summary>
    /// A Phrase is a series of Chunks.
    /// </summary>
    /// <remarks>
    /// A Phrase has a main Font, but some chunks
    /// within the phrase can have a Font that differs from the
    /// main Font. All the Chunks in a Phrase
    /// have the same leading.
    /// </remarks>
    /// <example>
    /// <code>
    /// // When no parameters are passed, the default leading = 16
    /// <strong>Phrase phrase0 = new Phrase();
    /// Phrase phrase1 = new Phrase("this is a phrase");</strong>
    /// // In this example the leading is passed as a parameter
    /// <strong>Phrase phrase2 = new Phrase(16, "this is a phrase with leading 16");</strong>
    /// // When a Font is passed (explicitely or embedded in a chunk), the default leading = 1.5 * size of the font
    /// <strong>Phrase phrase3 = new Phrase("this is a phrase with a red, normal font Courier, size 12", FontFactory.GetFont(FontFactory.COURIER, 12, Font.NORMAL, new Color(255, 0, 0)));
    /// Phrase phrase4 = new Phrase(new Chunk("this is a phrase"));
    /// Phrase phrase5 = new Phrase(18, new Chunk("this is a phrase", FontFactory.GetFont(FontFactory.HELVETICA, 16, Font.BOLD, new Color(255, 0, 0)));</strong>
    /// </code>
    /// </example>
    public class Phrase : ArrayList, ITextElementArray {
    
        // membervariables
    
        /// <summary>This is the leading of this phrase.</summary>
        protected Single leading = Single.NaN;
    
        ///<summary> This is the font of this phrase. </summary>
        protected Font font = new Font();

        // constructors
    
    /**
    * Constructs a Phrase that can be used in the static GetInstance() method.
    * @param    dummy    a dummy parameter
    */
        private Phrase(bool dummy) {
        }
        
        /// <summary>
        /// Constructs a Phrase without specifying a leading.
        /// </summary>
        /// <overloads>
        /// Has nine overloads.
        /// </overloads>
        public Phrase() : this(16) {}
    
        /// <summary>
        /// Constructs a Phrase with a certain leading.
        /// </summary>
        /// <param name="leading">the leading</param>
        public Phrase(float leading) {
            this.leading = leading;
        }
    
        /// <summary>
        /// Constructs a Phrase with a certain Chunk.
        /// </summary>
        /// <param name="chunk">a Chunk</param>
        public Phrase(Chunk chunk) {
            base.Add(chunk);
            font = chunk.Font;
        }
    
        /// <summary>
        /// Constructs a Phrase with a certain Chunk and a certain leading.
        /// </summary>
        /// <param name="leading">the leading</param>
        /// <param name="chunk">a Chunk</param>
        public Phrase(float leading, Chunk chunk) : this(leading) {
            base.Add(chunk);
            font = chunk.Font;
        }
    
        /// <summary>
        /// Constructs a Phrase with a certain string.
        /// </summary>
        /// <param name="str">a string</param>
        public Phrase(string str) : this(float.NaN, str, new Font()) {}
    
        /// <summary>
        /// Constructs a Phrase with a certain string and a certain Font.
        /// </summary>
        /// <param name="str">a string</param>
        /// <param name="font">a Font</param>
        public Phrase(string str, Font font) : this(float.NaN, str, font) {
            this.font = font;
        }
    
        /// <summary>
        /// Constructs a Phrase with a certain leading and a certain string.
        /// </summary>
        /// <param name="leading">the leading</param>
        /// <param name="str">a string</param>
        public Phrase(float leading, string str) : this(leading, str, new Font()) {}
    
        public Phrase(float leading, string str, Font font) : this(leading) {
            this.font = font;
            /* bugfix by August Detlefsen */
            if (str != null && str.Length != 0) {
                base.Add(new Chunk(str, font));
            }
        }
        
        /**
        * Gets a special kind of Phrase that changes some characters into corresponding symbols.
        * @param string
        * @return a newly constructed Phrase
        */
        public static Phrase GetInstance(string str) {
            return GetInstance(16, str, new Font());
        }
        
        /**
        * Gets a special kind of Phrase that changes some characters into corresponding symbols.
        * @param leading
        * @param string
        * @return a newly constructed Phrase
        */
        public static Phrase GetInstance(int leading, String str) {
            return GetInstance(leading, str, new Font());
        }
        
        /**
        * Gets a special kind of Phrase that changes some characters into corresponding symbols.
        * @param leading
        * @param string
        * @param font
        * @return a newly constructed Phrase
        */
        public static Phrase GetInstance(int leading, String str, Font font) {
            Phrase p = new Phrase(true);
            p.Leading = leading;
            p.font = font;
            if (font.Family != Font.SYMBOL && font.Family != Font.ZAPFDINGBATS && font.BaseFont == null) {
                int index;
                while ((index = SpecialSymbol.Index(str)) > -1) {
                    if (index > 0) {
                        string firstPart = str.Substring(0, index);
                        /* bugfix [ #461272 ] CODE CHANGE REQUIRED IN Phrase.java
                                           by Arekh Nambiar */
                        ((ArrayList)p).Add(new Chunk(firstPart, font));
                        str = str.Substring(index);
                    }
                    Font symbol = new Font(Font.SYMBOL, font.Size, font.Style, font.Color);
                    StringBuilder buf = new StringBuilder();
                    buf.Append(SpecialSymbol.GetCorrespondingSymbol(str[0]));
                    str = str.Substring(1);
                    while (SpecialSymbol.Index(str) == 0) {
                        buf.Append(SpecialSymbol.GetCorrespondingSymbol(str[0]));
                        str = str.Substring(1);
                    }
                    ((ArrayList)p).Add(new Chunk(buf.ToString(), symbol));
                }
            }
            /* bugfix by August Detlefsen */
            if (str != null && str.Length != 0) {
                ((ArrayList)p).Add(new Chunk(str, font));
            }
            return p;
        }

        /// <summary>
        /// Returns a Phrase that has been constructed taking in account
        /// the value of some attributes.
        /// </summary>
        /// <param name="attributes">some attributes</param>
        public Phrase(Properties attributes) : this("", FontFactory.GetFont(attributes)) {
            this.Clear();
            string value;
            if ((value = attributes.Remove(ElementTags.LEADING)) != null) {
                Leading= float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            else if ((value = attributes.Remove(Markup.CSS_KEY_LINEHEIGHT)) != null) {
                Leading = Markup.ParseLength(value);
            }
            if ((value = attributes.Remove(ElementTags.ITEXT)) != null) {
                Chunk chunk = new Chunk(value);
                if ((value = attributes.Remove(ElementTags.GENERICTAG)) != null) {
                    chunk.SetGenericTag(value);
                }
                Add(chunk);
            }
        }
    
        // implementation of the Element-methods
    
        /// <summary>
        /// Processes the element by adding it (or the different parts) to an
        /// <see cref="iTextSharp.text.IElementListener"/>.
        /// </summary>
        /// <param name="listener">an IElementListener</param>
        /// <returns>true if the element was processed successfully</returns>
        public virtual bool Process(IElementListener listener) {
            try {
                foreach (IElement ele in this) {
                    listener.Add(ele);
                }
                return true;
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
        public virtual int Type {
            get {
                return Element.PHRASE;
            }
        }
    
        /// <summary>
        /// Gets all the chunks in this element.
        /// </summary>
        /// <value>an ArrayList</value>
        public virtual ArrayList Chunks {
            get {
                ArrayList tmp = new ArrayList();
                foreach (IElement ele in this) {
                    tmp.AddRange(ele.Chunks);
                }
                return tmp;
            }
        }
    
        // overriding some of the ArrayList-methods
    
        /// <summary>
        /// Adds a Chunk, an Anchor or another Phrase
        /// to this Phrase.
        /// </summary>
        /// <param name="index">index at which the specified element is to be inserted</param>
        /// <param name="o">an object of type Chunk, Anchor, or Phrase</param>
        public virtual void Add(int index, Object o) {
            if (o == null) return;
            try {
                IElement element = (IElement) o;
                if (element.Type == Element.CHUNK) {
                    Chunk chunk = (Chunk)element;
                    if (!font.IsStandardFont()) {
                        chunk.Font = font.Difference(chunk.Font);
                    }
                    base.Insert(index, chunk);
                }
                else if (element.Type == Element.PHRASE ||
                    element.Type == Element.ANCHOR ||
                    element.Type == Element.ANNOTATION ||
                    element.Type == Element.TABLE || // line added by David Freels
                    element.Type == Element.MARKED) {
                    base.Insert(index, element);
                }
                else {
                    throw new Exception(element.Type.ToString());
                }
            }
            catch (Exception cce) {
                throw new Exception("Insertion of illegal Element: " + cce.Message);
            }
        }
    
        /// <summary>
        /// Adds a Chunk, Anchor or another Phrase
        /// to this Phrase.
        /// </summary>
        /// <param name="o">an object of type Chunk, Anchor or Phrase</param>
        /// <returns>a bool</returns>
        public virtual new bool Add(Object o) {
            if (o == null) return false;
            if (o is string) {
                base.Add(new Chunk((string) o, font));
                return true;
            }
            try {
                IElement element = (IElement) o;
                switch (element.Type) {
                    case Element.CHUNK:
                        return AddChunk((Chunk) o);
                    case Element.PHRASE:
                    case Element.PARAGRAPH:
                        Phrase phrase = (Phrase) o;
                        bool success = true;
                        foreach (IElement e in phrase) {
                            if (e is Chunk) {
                                success &= AddChunk((Chunk)e);
                            }
                            else {
                                success &= this.Add(e);
                            }
                        }
                        return success;
                    case Element.MARKED:
                    case Element.ANCHOR:
                    case Element.ANNOTATION:
                    case Element.TABLE: // case added by David Freels
                    case Element.PTABLE: // case added by Karen Vardanyan
                        // This will only work for PDF!!! Not for RTF/HTML
                    case Element.LIST:
                        base.Add(o);
                        return true;
                    default:
                        throw new Exception(element.Type.ToString());
                }
            }
            catch (Exception cce) {
                throw new Exception("Insertion of illegal Element: " + cce.Message);
            }
        }
    
        /// <summary>
        /// Adds a Chunk.
        /// </summary>
        /// <remarks>
        /// This method is a hack to solve a problem I had with phrases that were split between chunks
        /// in the wrong place.
        /// </remarks>
        /// <param name="chunk">a Chunk</param>
        /// <returns>a bool</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool AddChunk(Chunk chunk) {
            if (!font.IsStandardFont()) {
                chunk.Font = font.Difference(chunk.Font);
            }
            if (Count > 0 && !chunk.HasAttributes()) {
                try {
                    Chunk previous = (Chunk) this[Count - 1];
                    if (!previous.HasAttributes() && previous.Font.CompareTo(chunk.Font) == 0 && !"".Equals(previous.Content.Trim()) && !"".Equals(chunk.Content.Trim())) {
                        previous.Append(chunk.Content);
                        return true;
                    }
                }
                catch {
                }
            }
            base.Add(chunk);
            return true;
        }
    
        /// <summary>
        /// Adds a collection of Chunks
        /// to this Phrase.
        /// </summary>
        /// <param name="collection">a collection of Chunks, Anchors and Phrases.</param>
        /// <returns>true if the action succeeded, false if not.</returns>
        public bool AddAll(ICollection collection) {
            foreach (object itm in collection) {
                this.Add(itm);
            }
            return true;
        }
    
        /// <summary>
        /// Adds a Object to the Paragraph.
        /// </summary>
        /// <param name="obj">the object to add.</param>
        public void AddSpecial(Object obj) {
            base.Add(obj);
        }
    
        // methods
    
        // methods to retrieve information
    
        /// <summary>
        /// Checks is this Phrase contains no or 1 empty Chunk.
        /// </summary>
        /// <returns>
        /// false if the Phrase
        /// contains more than one or more non-emptyChunks.
        /// </returns>
        public bool IsEmpty() {
            switch (Count) {
                case 0:
                    return true;
                case 1:
                    IElement element = (IElement) this[0];
                    if (element.Type == Element.CHUNK && ((Chunk) element).IsEmpty()) {
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }
    
        /// <summary>
        /// Checks you if the leading of this phrase is defined.
        /// </summary>
        /// <value>true if the leading is defined</value>
        public bool LeadingDefined {
            get {
                if (float.IsNaN(leading)) {
                    return false;
                }
                return true;
            }
        }
    
        /// <summary>
        /// Gets/sets the leading of this phrase.
        /// </summary>
        /// <value>the linespacing</value>
        public virtual float Leading {
            get {
                if (float.IsNaN(leading)) {
                    return font.Leading(1.5f);
                }
                return leading;
            }

            set {
                this.leading = value;
            }
        }
    
        /// <summary>
        /// Gets the font of the first Chunk that appears in this Phrase.
        /// </summary>
        /// <value>a Font</value>
        public Font Font {
            get {
                return font;
            }
        }
    
    /**
    * Returns the content as a String object.
    * This method differs from toString because toString will return an ArrayList with the toString value of the Chunks in this Phrase.
    */
        public String Content {
            get {
    	        StringBuilder buf = new StringBuilder();
                foreach (object obj in Chunks)
    		        buf.Append(obj.ToString());
    	        return buf.ToString();
            }
        }
        /// <summary>
        /// Checks if a given tag corresponds with this object.
        /// </summary>
        /// <param name="tag">the given tag</param>
        /// <returns>true if the tag corresponds</returns>
        public static bool IsTag(string tag) {
            return ElementTags.PHRASE.Equals(tag);
        }
    
        public override string ToString() {
            return base.ToString();
        }
    }
}
