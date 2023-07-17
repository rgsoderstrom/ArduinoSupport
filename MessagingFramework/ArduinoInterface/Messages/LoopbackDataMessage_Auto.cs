//
// auto-generated code for message LoopbackDataMessage
//

using System;
using System.Text;
using System.Collections.Generic;

using SocketLibrary;

namespace ArduinoInterface
{
    public partial class LoopbackDataMessage
    {
        public LoopbackDataMessage (byte [] fromBytes)
        {
             header = new Header ();
             data = new Data ();
             int byteIndex = 0;

             header.Sync           = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
             header.ByteCount      = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
             header.MessageId      = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
             header.SequenceNumber = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;

            data.source = fromBytes [byteIndex++];

            data.dataByteCount = fromBytes [byteIndex++];

            for (int i=0; i<Data.MaxCount; i++)
            {
                 data.dataBytes [i] = fromBytes [byteIndex++];
            }

        }

        public byte[] ToBytes ()
        {
             List<byte> byteList = new List<byte> ();

             byteList.InsertRange (byteList.Count, BitConverter.GetBytes (header.Sync));
             byteList.InsertRange (byteList.Count, BitConverter.GetBytes (header.ByteCount));
             byteList.InsertRange (byteList.Count, BitConverter.GetBytes (header.MessageId));
             byteList.InsertRange (byteList.Count, BitConverter.GetBytes (header.SequenceNumber));

            byteList.Insert (byteList.Count, data.source);

            byteList.Insert (byteList.Count, data.dataByteCount);

            byteList.InsertRange (byteList.Count, data.dataBytes);

            // append data bytes to header bytes
            byte[] msgBytes = new byte [byteList.Count];
            byteList.CopyTo (msgBytes, 0);
            return msgBytes;
        }
    }
}
