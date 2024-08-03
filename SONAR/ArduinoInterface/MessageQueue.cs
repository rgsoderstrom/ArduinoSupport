using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SocketLibrary;
using System.Runtime.InteropServices;

//
// MessageQueue used to throttle messages to Arduino.
//

namespace ArduinoInterface
{
    public class MessageQueue
    {
        // list of messages waiting to be sent
        private Queue<IMessage_Auto> pendingMessages = new Queue<IMessage_Auto> (10);
        private bool QueueEmpty {get {return pendingMessages.Count == 0;}}

        // message put here to be sent and left here until acknowledged
        private IMessage_Auto currentMessage = null;
        private bool NoCurrentMsg {get {return currentMessage == null;}}

        // ready to accept messages
        private bool arduinoReady {get; set;} = false;

        // socket to Arduino
        Socket socket;

        //**********************************************************************

        public MessageQueue (Socket _socket)
        {
            socket = _socket;
        }

        public void Close ()
        {
            socket.Close ();
            arduinoReady = false;
        }

        //**********************************************************************

        public void AddMessage (IMessage_Auto msg)
        {
            if (arduinoReady == false || socket.Connected == false)
            {                
                pendingMessages.Enqueue (msg);
            }

            else
            {
                if (QueueEmpty == false)
                { 
                    pendingMessages.Enqueue (msg);
                }

                else
                {
                    if (NoCurrentMsg)
                    {
                        currentMessage = msg;
                        socket.Send (currentMessage.ToBytes ());
                    }

                    else
                        pendingMessages.Enqueue (msg);
                }
            }
        }

        public void ResendLastMsg ()
        {
            if (currentMessage != null)
                socket.Send (currentMessage.ToBytes ());
        }

        //**********************************************************************

        // called when an acknowledge message is received from Arduino

        public bool MessageAcknowledged (ushort seqNumber)
        {
            bool flag = seqNumber == currentMessage.SequenceNumber;

            if (flag)
            {
                currentMessage = null;

                if (QueueEmpty == false)
                { 
                    currentMessage = pendingMessages.Dequeue ();
                    socket.Send (currentMessage.ToBytes ());
                }
            }

            else
                throw new Exception ("Ack not for msg just sent");

            return flag;
        }

        //**********************************************************************

        // stop sending messages

        public void ArduinoNotReady ()
        {
            arduinoReady = false;
        }

        //**********************************************************************

        // called when Arduino ready to accept a message

        public void ArduinoReady ()
        {
            arduinoReady = true;

            // if a message is waiting to go out, then send it
            if (QueueEmpty == false && socket.Connected == true)
            {
                    currentMessage = pendingMessages.Dequeue ();
                    socket.Send (currentMessage.ToBytes ());
            }
        }
    }

    //****************************************************************************************
    //****************************************************************************************
    //****************************************************************************************

    //internal class QueuedBytes
    //{
    //    public QueuedBytes (byte [] MsgBytes)
    //    {
    //        MessageBytes = MsgBytes;

    //        if ((MsgBytes.Length != ByteCount) || (Sync != Message.SyncPattern))
    //        {
    //            throw new Exception ("QueuedMessage ctor not passed a valid message");
    //        }
    //    }

    //    public byte[] ToBytes ()
    //    {
    //        return MessageBytes;
    //    }

    //    public ushort Sync           {get {return BitConverter.ToUInt16 (MessageBytes, (int) Marshal.OffsetOf<MessageHeader> ("Sync"));}}
    //    public ushort ByteCount      {get {return BitConverter.ToUInt16 (MessageBytes, (int) Marshal.OffsetOf<MessageHeader> ("ByteCount"));}}
    //    public ushort MessageId      {get {return BitConverter.ToUInt16 (MessageBytes, (int) Marshal.OffsetOf<MessageHeader> ("MessageId"));}}
    //    public ushort SequenceNumber {get {return BitConverter.ToUInt16 (MessageBytes, (int) Marshal.OffsetOf<MessageHeader> ("SequenceNumber"));}}

    //    byte [] MessageBytes = null;
    //}
}



