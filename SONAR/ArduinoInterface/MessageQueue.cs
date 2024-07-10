using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SocketLibrary;

//
// MessageQueue used to throttle messages to Arduino.
//

namespace ArduinoInterface
{
    public class MessageQueue
    {
        // list of messages waiting to be sent
        private Queue<byte []> pendingMessages = new Queue<byte []> (10);

        // if this is true, passed-in messages are immediately sent
        private bool arduinoReady { get; set; } = false;

        // socket to Arduino
        //TcpServer socket;
        Socket socket;

        List<ushort> sentSeqNumbers = new List<ushort> ();

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

        public void AddMessage (byte [] msgBytes)
        {
            if (arduinoReady == false)
            {
                pendingMessages.Enqueue (msgBytes);
            }

            else
            {
                MessageHeader header = new MessageHeader (msgBytes);
                sentSeqNumbers.Add (header.SequenceNumber);

                if (socket.Connected) socket.Send (msgBytes);
                arduinoReady = false;
            }
        }

        //**********************************************************************

        // called when an acknowledge message is received from Arduino

        public bool MessageAcknowledged (ushort seqNumber)
        {
            bool flag = sentSeqNumbers.Contains (seqNumber);

            if (flag)
            {
                sentSeqNumbers.Remove (seqNumber);
                ArduinoReady ();
            }

            return flag;
        }

        //**********************************************************************

        // set status to "not ready"

        public void ArduinoNotReady ()
        {
            arduinoReady = false;
        }

        //**********************************************************************

        // called when Arduino ready to accept a message

        public void ArduinoReady ()
        {
            // if a message is waiting to go out, then send it
            if (pendingMessages.Count > 0)
            {
                Byte [] nextMessage = pendingMessages.Dequeue ();
                MessageHeader header = new MessageHeader (nextMessage);
                sentSeqNumbers.Add (header.SequenceNumber);

                if (socket.Connected) socket.Send (nextMessage);
                arduinoReady = false;
            }

            else
            {
                arduinoReady = true;
            }
        }
    }
}



