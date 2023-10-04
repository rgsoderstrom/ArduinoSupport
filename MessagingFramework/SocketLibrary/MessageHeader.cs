
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices; // for StructLayout

namespace SocketLibrary
{
    // used by all messages
    abstract public class Message 
    {
        public const ushort Sync = 0x1234;
    }

    //*********************************************************************************************
    //
    // Header used by all socket messages
    //

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public partial class MessageHeader
    {
        public ushort Sync;              // offset = 0
        public ushort ByteCount;         //        = 2
        public ushort MessageId;         //        = 4
        public ushort SequenceNumber;    //        = 6
    }

    //*********************************************************************************************
    //
    // Header methods
    //(int) Marshal.OffsetOf<Header> ("Sync")); 
    // 
    public partial class MessageHeader
    {
        static ushort NextSequenceNumber = 1;

        public MessageHeader ()
        {
            Sync           = Message.Sync;
            SequenceNumber = NextSequenceNumber++;
        }
        
        public MessageHeader (byte[] fromBytes)
        {
            Sync           = BitConverter.ToUInt16 (fromBytes, (int) Marshal.OffsetOf<MessageHeader> ("Sync"));
            ByteCount      = BitConverter.ToUInt16 (fromBytes, (int) Marshal.OffsetOf<MessageHeader> ("ByteCount"));
            MessageId      = BitConverter.ToUInt16 (fromBytes, (int) Marshal.OffsetOf<MessageHeader> ("MessageId"));
            SequenceNumber = BitConverter.ToUInt16 (fromBytes, (int) Marshal.OffsetOf<MessageHeader> ("SequenceNumber"));
        }

        public byte[] ToBytes () // convert to byte stream to be sent out socket
        {
            List<byte> msgList = new List<byte> ();

            msgList.InsertRange (msgList.Count, BitConverter.GetBytes (Sync));
            msgList.InsertRange (msgList.Count, BitConverter.GetBytes (ByteCount));
            msgList.InsertRange (msgList.Count, BitConverter.GetBytes (MessageId));
            msgList.InsertRange (msgList.Count, BitConverter.GetBytes (SequenceNumber));

            byte[] msgBytes = new byte [ByteCount];
            msgList.CopyTo (msgBytes);

            return msgBytes;
        }

        public override string ToString ()
        {
            string str = "";
            str += String.Format ("Sync:         {0:X4}\n", Sync);
            str += String.Format ("Byte count:   {0}\n", ByteCount);
            str += String.Format ("Message ID:   {0}\n", MessageId);
            str += String.Format ("Sequence Num: {0}\n", SequenceNumber);

            return str;
        }
    }
}
