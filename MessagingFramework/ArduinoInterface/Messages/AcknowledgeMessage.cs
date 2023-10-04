using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using SocketLibrary;

namespace ArduinoInterface
{
    //*********************************************************************************************

    [StructLayout (LayoutKind.Sequential, Pack = 1)]
    public partial class AcknowledgeMessage
    {
        [StructLayout(LayoutKind.Sequential, Pack=1)]
        public class Data
        {
            public ushort MsgSequenceNumber;
        }

        public MessageHeader  header;
        public Data    data;

        //***************************************************************
        //***************************************************************
        //***************************************************************

        public AcknowledgeMessage (ushort seqNumber)
        {
            header = new MessageHeader ();
            data = new Data ();

            header.MessageId = (ushort) ArduinoMessageIDs.AcknowledgeMsgId;

            header.ByteCount = (ushort) (Marshal.SizeOf (header) + Marshal.SizeOf (data));

            data.MsgSequenceNumber = seqNumber;
        }

        public override string ToString ()
        {
            string str = string.Format ("Header: 0x{0:x}, {1}, {2}, {3}\n", header.Sync, header.ByteCount, header.MessageId, header.SequenceNumber);
            str += string.Format ("Seq number: {0:x}", data.MsgSequenceNumber);
            return str;
        }

/****
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
        } ****/
    }
}
