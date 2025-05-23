﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;

using SocketLibrary;

//
// MessageQueue used to throttle messages to Arduino.
//

//
// This version:
//   - a sent messaged being acknowledged just cancels a possible re-send
//   - receiving a "ready" message causes next queued message to be sent
//

namespace ArduinoInterface
{
    public class MessageQueue
    {
        // list of messages waiting to be sent
        private readonly Queue<IMessage_Auto> pendingMessages = new Queue<IMessage_Auto> (10);
        private bool QueueEmpty {get {return pendingMessages.Count == 0;}}
        readonly object LocalMsgQueueLock = new object ();

        //
        // message reference put here to be sent and left here until acknowledged
        //
        private IMessage_Auto currentMessage = null;
        private bool NoCurrentMsg     {get {return currentMessage == null;}}
        public  bool IsUnackedMessage {get {return currentMessage != null;}} 

        // socket to Arduino
        private Socket socket;

        //
        // heartbeat timer. 
        //
        readonly System.Timers.Timer HeartbeatTimer = new System.Timers.Timer (5000); // milliseconds

        //
        // if a message is not acknowledged it will be resent
        //
        readonly System.Timers.Timer AcknowledgeWaitTimer = new System.Timers.Timer (500); // milliseconds. if msg unacknowledged for this 
                                                                                           // long a Resend attempted

        // send status back to host object
        readonly Callback QueueStuckCB   = null;
        readonly Callback ArduinoBusyCB  = null;
        readonly Callback ArduinoReadyCB = null;
        readonly PrintCallback PrintCB   = null;

        public int Verbosity {get; set;} = 1;

        //**********************************************************************
        //
        // ctor
        //
        public MessageQueue (Callback      queueStuckCallback, // callbacks can be null
                             Callback      ardBusyCallback,
                             Callback      ardReadyCallback,
                             PrintCallback printCallback, 
                             Socket        _socket)
        {
            socket = _socket;

            QueueStuckCB   = queueStuckCallback;
            ArduinoBusyCB  = ardBusyCallback;
            ArduinoReadyCB = ardReadyCallback;
            PrintCB        = printCallback;

            AcknowledgeWaitTimer.AutoReset = false; 
            AcknowledgeWaitTimer.Elapsed += QueueStuckTimerElapsed;

            HeartbeatTimer.Elapsed += HeartbeatTimerElapsed;
            HeartbeatTimer.Enabled = true;  
            HeartbeatTimer.AutoReset = true;  
        }

        public void NewSocket (Socket _socket)
        {
            socket = _socket;
        }

        //**********************************************************************
        //
        // Arduino is ready to accept the next message
        //
        private bool arduinoReady = false;

        public bool ArduinoReady 
        {
            get {return arduinoReady;}
            
            set 
            {
                arduinoReady = value;

                if (arduinoReady == true)
                {
                    ArduinoReadyCB?.Invoke ();

                    // if a message is waiting to go out, then send it
                    if (QueueEmpty == false && socket.Connected == true)
                    {
                        lock (LocalMsgQueueLock)
                        { 
                            currentMessage = pendingMessages.Dequeue ();
                        }

                        AcknowledgeWaitTimer.Enabled = true;

                        if (Verbosity > 2) PrintCB ("Sending de-queued msg ID " + currentMessage.MessageId + ", Seq = " + currentMessage.SequenceNumber);
                        socket.Send (currentMessage.ToBytes ());
                        HeartbeatTimer.Enabled = false; 
                        HeartbeatTimer.Enabled = true;                     }
                }
                else
                {
                    ArduinoBusyCB?.Invoke ();
                }
            }
        }



        private void HeartbeatTimerElapsed (object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Verbosity > 2) PrintCB ("Heartbeat");
            KeepAliveMsg_Auto msg = new KeepAliveMsg_Auto ();
            AddMessage (msg);
        }


        private void QueueStuckTimerElapsed (object sender, System.Timers.ElapsedEventArgs e)
        {
            AcknowledgeWaitTimer.Enabled = true;
            QueueStuckCB?.Invoke ();

            if (currentMessage != null)
            {
                if (Verbosity > 1) PrintCB ("Resending message ID " + currentMessage.MessageId + ", Seq = " + currentMessage.SequenceNumber);
                socket.Send (currentMessage.ToBytes ());
                HeartbeatTimer.Enabled = false; 
                HeartbeatTimer.Enabled = true;  
            }
        }

        public void Close ()
        {
            socket.Close ();
            ArduinoReady = false;
        }

        //**********************************************************************

        public void AddMessage (IMessage_Auto msg)
        {
            lock (LocalMsgQueueLock)
            {
                if (ArduinoReady == false || socket.Connected == false)
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
                        if (NoCurrentMsg && ArduinoReady)
                        {
                            currentMessage = msg;
                            AcknowledgeWaitTimer.Enabled = true;

                            if (Verbosity > 2) PrintCB ("Sending msg ID " + currentMessage.MessageId + ", Seq = " + currentMessage.SequenceNumber);
                            socket.Send (currentMessage.ToBytes ());
                            HeartbeatTimer.Enabled = false;
                            HeartbeatTimer.Enabled = true;

                            ArduinoReady = false;
                        }
                        else
                            pendingMessages.Enqueue (msg);
                    }
                }
            }
        }

        //**********************************************************************
        //
        // called when an acknowledge message is received from Arduino
        //
        public bool MessageAcknowledged (ushort seqNumber)
        {
            AcknowledgeWaitTimer.Enabled = false;
            bool flag = seqNumber == currentMessage.SequenceNumber;

            if (flag)
            {
                currentMessage = null;
                //AcknowledgeWaitTimer.Enabled = false;
                ArduinoReady = false; // Arduino will send "ready" message when done processing
            }                         // whatever was just ack'd

            else
                throw new Exception ("Ack not for msg just sent. Expected " + currentMessage.SequenceNumber + ", got " + seqNumber);

            return flag;
        }
    }
}



