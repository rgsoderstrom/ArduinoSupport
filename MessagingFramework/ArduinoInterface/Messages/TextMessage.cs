using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using SocketLibrary;

namespace ArduinoInterface
{
    public partial class TextMessage
    {
        public class Data
        {
            public const int TextBufferSize = 40;
            public char [] text = new char [TextBufferSize];
        }

        public Header header;
        public Data data;

        public TextMessage ()
        {
            header = new Header ();
            header.MessageId = (ushort) ArduinoMessageIDs.TextMsgId;
            header.ByteCount = (ushort)(Marshal.SizeOf (header) + Data.TextBufferSize * Marshal.SizeOf<char> ());

            data = new Data ();
        }

        public TextMessage (string txt) : this ()
        {
            if (txt.Length > Data.TextBufferSize)
                txt = txt.Remove (Data.TextBufferSize);

            data.text = txt.ToCharArray ();
        }

        public string Text {get {return new string (data.text);}}

        public override string ToString ()
        {
            string str = header.ToString ();
            str += "Text: " + new string (data.text);
            return str;
        }
    }
}
