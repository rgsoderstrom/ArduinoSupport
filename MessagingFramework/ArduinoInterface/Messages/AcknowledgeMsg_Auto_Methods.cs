//
// auto-generated code for message AcknowledgeMsg_Auto
//

using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SocketLibrary;

namespace ArduinoInterface
{
    public partial class AcknowledgeMsg_Auto
    {
        //
        // Default ctor
        //
        public AcknowledgeMsg_Auto ()
        {
             header = new MessageHeader ();
             data = new Data ();

             header.MessageId = (ushort) ArduinoMessageIDs.AcknowledgeMsgId;
             header.ByteCount = (ushort)(Marshal.SizeOf (header)
							  + sizeof (UInt16));
         }

        //********************************************************
        //
        // from-bytes constructor
        //
        public AcknowledgeMsg_Auto (byte [] fromBytes)
        {
            header = new MessageHeader ();
            data = new Data ();
            int byteIndex = 0;

            header.Sync           = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
            header.ByteCount      = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
            header.MessageId      = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
            header.SequenceNumber = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;

            data.MsgSequenceNumber = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
        }
        //********************************************************
        //
        // member function ToBytes ()
        //
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

        //********************************************************
        //
        // member function ToString ()
        //
        public override string ToString ()
        {
            string str = "";
            str += "Sync      = " + header.Sync + "\n";
            str += "ByteCount = " + header.ByteCount + "\n";
            str += "ID        = " + header.MessageId + "\n";
            str += "SeqNumb   = " + header.SequenceNumber + "\n";

            str += "MsgSequenceNumber = " + data.MsgSequenceNumber + "\n";

            return str;
        }
    }
}
