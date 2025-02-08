
//
// TextMessage - derived from auto-generated base class
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using SocketLibrary;

namespace ArduinoInterface
{
    public partial class TextMessage : TextMsg_Auto
    {
        public TextMessage (string txt) : base ()
        {
            if (txt.Length > Data.TextBufferSize)
                txt = txt.Remove (Data.TextBufferSize);

            data.text = new char [Data.TextBufferSize];
            txt.CopyTo (0, data.text, 0, txt.Length);

            for (int i = txt.Length; i<Data.TextBufferSize; i++)
                data.text [i] = '\0';
        }

        public TextMessage (byte [] fromBytes) : base (fromBytes)
        {
        }

        public string Text {get {return new string (data.text);}}

        //
        // ToString () - override auto-generated version to print 
        //               something for null characters
        //
        public override string ToString ()
        {
            string str = "";
            str += "Sync      = " + header.Sync + "\n";
            str += "ByteCount = " + header.ByteCount + "\n";
            str += "ID        = " + header.MessageId + "\n";
            str += "SeqNumb   = " + header.SequenceNumber + "\n";

            for (int i=0; i<Data.TextBufferSize; i++)
            {
                str += "text [" + i + "] = ";
                str += data.text [i] != 0 ? data.text [i] : '0'; // just prints blank space w/o this
                str += "\n";
            }

            return str;
        }

    }
}
