//
// auto-generated code for message HeaderOnlyMsg_Auto
//

using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SocketLibrary;

namespace ArduinoInterface
{
    public partial class HeaderOnlyMsg_Auto
    {
        //
        // Default ctor
        //
        public HeaderOnlyMsg_Auto ()
        {
             header = new MessageHeader ();
             data = new Data ();

             header.MessageId = (ushort) ArduinoMessageIDs.HeaderOnlyMsgId;
             header.ByteCount = (ushort)(Marshal.SizeOf (header));
         }

        //********************************************************
        //
        // from-bytes constructor
        //
        public HeaderOnlyMsg_Auto (byte [] fromBytes)
        {
            header = new MessageHeader ();
            data = new Data ();
            int byteIndex = 0;

            header.Sync           = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
            header.ByteCount      = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
            header.MessageId      = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
            header.SequenceNumber = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
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


            return str;
        }
    }
}
