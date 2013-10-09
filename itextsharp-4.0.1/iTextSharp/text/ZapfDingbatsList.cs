using System;

namespace iTextSharp.text
{
    /**
    * 
    * A special-version of <CODE>LIST</CODE> whitch use zapfdingbats-letters.
    * 
    * @see com.lowagie.text.List
    * @version 2003-06-22
    * @author Michael Niedermair
    */
    public class ZapfDingbatsList : List {
        /**
        * char-number in zapfdingbats
        */
        protected int zn;

        /**
        * Creates a ZapfDingbatsList
        * 
        * @param zn a char-number
        * @param symbolIndent    indent
        */
        public ZapfDingbatsList(int zn, int symbolIndent) : base(true, symbolIndent) {
            this.zn = zn;
            float fontsize = symbol.Font.Size;
            symbol.Font = FontFactory.GetFont(FontFactory.ZAPFDINGBATS, fontsize, Font.NORMAL);
        }

        /**
        * set the char-number 
        * @param zn a char-number
        */
        public int CharNumber {
            set {
                this.zn = value;
            }
            get {
                return this.zn;
            }
        }

        /**
        * Adds an <CODE>Object</CODE> to the <CODE>List</CODE>.
        *
        * @param    o    the object to add.
        * @return true if adding the object succeeded
        */
        public override bool Add(Object o) {
            if (o is ListItem) {
                ListItem item = (ListItem) o;
                Chunk chunk = new Chunk((char)zn, symbol.Font);
                item.ListSymbol = chunk;
                item.SetIndentationLeft(symbolIndent, autoindent);
                item.IndentationRight = 0;
                list.Add(item);
                return true;
            } else if (o is List) {
                List nested = (List) o;
                nested.IndentationLeft = nested.IndentationLeft + symbolIndent;
                first--;
                list.Add(nested);
                return true;
            } else if (o is String) {
                return this.Add(new ListItem((string) o));
            }
            return false;
        }
    }
}
