using System;
using System.Runtime.InteropServices;

//
// auto generated message format code
//

using SocketLibrary;
namespace ArduinoInterface
{
    public partial class LoopbackDataMsg_Auto
    {
        [StructLayout (LayoutKind.Sequential, Pack = 1)]
        public class Data
        {
            static public UInt16 MaxCount = 12;

            public short [] dataWords = new short  [Data.MaxCount];
        };

        public MessageHeader header;
        public Data data;
    }
}
