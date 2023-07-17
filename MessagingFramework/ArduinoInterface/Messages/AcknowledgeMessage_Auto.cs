//
// auto-generated code for message AcknowledgeMessage
//

using System;
using System.Text;
using System.Collections.Generic;

using SocketLibrary;

namespace ArduinoInterface
{
    public partial class AcknowledgeMessage
    {
        public AcknowledgeMessage (byte [] fromBytes)
        {
             header = new Header ();
             data = new Data ();
             int byteIndex = 0;

             header.Sync           = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
             header.ByteCount      = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
             header.MessageId      = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
             header.SequenceNumber = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;

            data.MsgSequenceNumber = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;

        }

        public byte[] ToBytes ()
        {
             List<byte> byteList = new List<byte> ();

             byteList.InsertRange (byteList.Count, BitConverter.GetBytes (header.Sync));
             byteList.InsertRange (byteList.Count, BitConverter.GetBytes (header.ByteCount));
             byteList.InsertRange (byteList.Count, BitConverter.GetBytes (header.MessageId));
             byteList.InsertRange (byteList.Count, BitConverter.GetBytes (header.SequenceNumber));

            byteList.InsertRange (byteList.Count, BitConverter.GetBytes (data.MsgSequenceNumber));

            // append data bytes to header bytes
            byte[] msgBytes = new byte [byteList.Count];
            byteList.CopyTo (msgBytes, 0);
            return msgBytes;
        }
    }
}
