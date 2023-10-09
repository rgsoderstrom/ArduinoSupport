using System;
using System.Runtime.InteropServices;

//
// auto generated message format code
//

using SocketLibrary;
namespace ArduinoInterface
{
    public partial class TextMsg_Auto
    {
        [StructLayout (LayoutKind.Sequential, Pack = 1)]
        public class Data
        {
            static public int TextBufferSize = 40;

            public char [] text = new char  [Data.TextBufferSize];
        };

        public MessageHeader header;
        public Data data;
    }
}
