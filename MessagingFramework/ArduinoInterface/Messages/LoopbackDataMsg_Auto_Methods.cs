//
// auto-generated code for message LoopbackDataMsg_Auto
//

using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SocketLibrary;

namespace ArduinoInterface
{
    public partial class LoopbackDataMsg_Auto
    {
        //
        // Default ctor
        //
        public LoopbackDataMsg_Auto ()
        {
             header = new MessageHeader ();
             data = new Data ();

             header.MessageId = (ushort) ArduinoMessageIDs.LoopbackDataMsgId;
             header.ByteCount = (ushort)(Marshal.SizeOf (header)
							  + sizeof (Int16) * Data.MaxCount);
         }

        //********************************************************
        //
        // from-bytes constructor
        //
        public LoopbackDataMsg_Auto (byte [] fromBytes)
        {
            header = new MessageHeader ();
            data = new Data ();
            int byteIndex = 0;

            header.Sync           = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
            header.ByteCount      = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
            header.MessageId      = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
            header.SequenceNumber = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;

            for (int i=0; i<Data.MaxCount; i++)
            {
                 data.dataWords [i] = BitConverter.ToInt16 (fromBytes, byteIndex); byteIndex += 2;
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

            for (int i=0; i<Data.MaxCount; i++)
            {
                byteList.InsertRange (byteList.Count, BitConverter.GetBytes (data.dataWords [i]));
            }

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


            for (int i=0; i<Data.MaxCount; i++)
            {
                 str += "dataWords [" + i + "] = ";
                 str += data.dataWords [i];
                 str += "\n";
            }

            return str;
        }
    }
}
