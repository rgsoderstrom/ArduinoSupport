﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SocketLib;

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
        SocketLib.TcpServer Socket;

        List<ushort> sentSeqNumbers = new List<ushort> ();

        //**********************************************************************

        public MessageQueue (SocketLib.TcpServer _socket)
        {
            Socket = _socket;
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
                Header header = new Header (msgBytes);
                sentSeqNumbers.Add (header.SequenceNumber);

                Socket.SendToAllClients (msgBytes);
                arduinoReady = false;
            }
        }

        //**********************************************************************

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

        public void ArduinoNotReady ()
        {
            arduinoReady = false;
        }

        //**********************************************************************

        public void ArduinoReady ()
        {
            if (pendingMessages.Count > 0)
            {
                Byte [] nextMessage = pendingMessages.Dequeue ();
                Header header = new Header (nextMessage);
                sentSeqNumbers.Add (header.SequenceNumber);

                Socket.SendToAllClients (nextMessage);
                arduinoReady = false;
            }

            else
            {
                arduinoReady = true;
            }
        }
    }
}



