using System;
using System.Runtime.InteropServices;

//
// auto generated message format code
//

using SocketLibrary;
namespace ArduinoInterface
{
    public partial class AcknowledgeMsg_Auto
    {
        [StructLayout (LayoutKind.Sequential, Pack = 1)]
        public class Data
        {
            public UInt16 MsgSequenceNumber;
        };

        public MessageHeader header;
        public Data data;
    }
}
