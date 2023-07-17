using System;
using System.Runtime.InteropServices;

using SocketLibrary;

namespace ArduinoInterface
{
    public partial class LoopbackDataMessage
    {
        [StructLayout (LayoutKind.Sequential, Pack = 1)]
        class Data
        {
            public const int MaxCount = 32;
            
            public byte    source;
            public byte    dataByteCount;
            public byte [] dataBytes = new byte [MaxCount];
        };

        Header header;
        Data data;

        public LoopbackDataMessage ()
        {
            header = new Header ();
            data = new Data ();

            header.MessageId = (ushort) ArduinoMessageIDs.LoopbackDataMsgId;
            header.ByteCount = (ushort) (Marshal.SizeOf (header) +  Marshal.SizeOf (data.source) + Marshal.SizeOf (data.dataByteCount) + Data.MaxCount * Marshal.SizeOf (data.dataBytes [0]));

            data.source = 0;
            data.dataByteCount = 0;
        }

        public byte Source {set {data.source = value;}
                            get {return data.source;}}

        public void Put (byte d) {if (data.dataByteCount < Data.MaxCount)
                                      data.dataBytes [data.dataByteCount++] = d;}

        public byte Get (int i) {if (i < data.dataByteCount) return data.dataBytes [i]; 
                                 else throw new Exception ("Attempt to read past end of data"); }

        public int Count {get {return data.dataByteCount;}}

        public override string ToString ()
        {
            string str = string.Format ("Header: 0x{0:x}, {1}, {2}, {3}\n", header.Sync, header.ByteCount, header.MessageId, header.SequenceNumber);

            str += string.Format ("source {0}\n", data.source);
            str += string.Format ("count {0}\n", data.dataByteCount);

            for (int i = 0; i<Data.MaxCount; i++)
            {
                str += string.Format ("{0}, ", data.dataBytes [i]);
                if ((i & 7) == 7) str += "\n";
            }

            return str;
        }
    }
}
