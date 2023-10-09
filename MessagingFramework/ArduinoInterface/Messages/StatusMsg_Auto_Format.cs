using System;
using System.Runtime.InteropServices;

//
// auto generated message format code
//

using SocketLibrary;
namespace ArduinoInterface
{
    public partial class StatusMsg_Auto
    {
        [StructLayout (LayoutKind.Sequential, Pack = 1)]
        public class Data
        {
            static public int MaxNameLength = 18;

            public char [] name = new char  [Data.MaxNameLength];
            public byte DataReceived;
            public byte DataReady;
        };

        public MessageHeader header;
        public Data data;
    }
}
