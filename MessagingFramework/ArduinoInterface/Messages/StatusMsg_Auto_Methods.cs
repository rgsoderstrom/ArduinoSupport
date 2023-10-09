//
// auto-generated code for message StatusMsg_Auto
//

using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SocketLibrary;

namespace ArduinoInterface
{
    public partial class StatusMsg_Auto
    {
        //
        // Default ctor
        //
        public StatusMsg_Auto ()
        {
             header = new MessageHeader ();
             data = new Data ();

             header.MessageId = (ushort) ArduinoMessageIDs.StatusMsgId;
             header.ByteCount = (ushort)(Marshal.SizeOf (header)
							  + sizeof (byte) * Data.MaxNameLength
							  + sizeof (byte)
							  + sizeof (byte));
         }

        //********************************************************
        //
        // from-bytes constructor
        //
        public StatusMsg_Auto (byte [] fromBytes)
        {
            header = new MessageHeader ();
            data = new Data ();
            int byteIndex = 0;

            header.Sync           = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
            header.ByteCount      = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
            header.MessageId      = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;
            header.SequenceNumber = BitConverter.ToUInt16 (fromBytes, byteIndex); byteIndex += 2;

            for (int i=0; i<Data.MaxNameLength; i++)
            {
                 data.name [i] = (char) fromBytes [byteIndex++];
            }

            data.DataReceived = fromBytes [byteIndex++];

            data.DataReady = fromBytes [byteIndex++];
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

            byteList.InsertRange (byteList.Count, Encoding.ASCII.GetBytes (data.name));

            byteList.Insert (byteList.Count, data.DataReceived);

            byteList.Insert (byteList.Count, data.DataReady);

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


            for (int i=0; i<Data.MaxNameLength; i++)
            {
                 str += "name [" + i + "] = ";
                 str += data.name [i];
                 str += "\n";
            }
            str += "DataReceived = " + data.DataReceived + "\n";
            str += "DataReady = " + data.DataReady + "\n";

            return str;
        }
    }
}
