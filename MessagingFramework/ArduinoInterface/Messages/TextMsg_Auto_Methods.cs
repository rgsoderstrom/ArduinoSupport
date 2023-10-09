//
// auto-generated code for message TextMsg_Auto
//

using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SocketLibrary;

namespace ArduinoInterface
{
    public partial class TextMsg_Auto
    {
        //
        // Default ctor
        //
        public TextMsg_Auto ()
        {
             header = new MessageHeader ();
             data = new Data ();

             header.MessageId = (ushort) ArduinoMessageIDs.TextMsgId;
             header.ByteCount = (ushort)(Marshal.SizeOf (header)
							  + sizeof (byte) * Data.TextBufferSize);
         }

        //********************************************************
        //
        // from-bytes constructor
        //
        public TextMsg_Auto (byte [] fromBytes)
        {
            header = new MessageHeader ();
            data = new Data ();
            int byteIndex = 0;

            header.Sync           = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
            header.ByteCount      = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
            header.MessageId      = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
            header.SequenceNumber = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;

            for (int i=0; i<Data.TextBufferSize; i++)
            {
                 data.text [i] = (char) fromBytes [byteIndex++];
            }
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

            byteList.InsertRange (byteList.Count, Encoding.ASCII.GetBytes (data.text));

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


            for (int i=0; i<Data.TextBufferSize; i++)
            {
                 str += "text [" + i + "] = ";
                 str += data.text [i];
                 str += "\n";
            }

            return str;
        }
    }
}
