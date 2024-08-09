using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using SocketLibrary;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Common;

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

        //
        // message put here to be sent and left here until acknowledged
        //
        private IMessage_Auto currentMessage = null;
        private bool NoCurrentMsg     {get {return currentMessage == null;}}
        public  bool IsUnackedMessage {get {return currentMessage != null;}} 

        // ready to accept messages
        private bool arduinoReady {get; set;} = false;

        // socket to Arduino
        Socket socket;

        //
        // if a message is not acknowledged let the user re-send it
        //
        readonly System.Timers.Timer AcknowledgeWaitTimer = new System.Timers.Timer (100); // milliseconds. must be unacknowledged for this 
                                                                                           // long before the Resend button is enabled
        readonly Callback QueueStuck = null;

        //**********************************************************************
        //
        // ctor
        //
        public MessageQueue (Callback queueStuckCallback, Socket _socket)
        {
            socket = _socket;
            QueueStuck = queueStuckCallback;

            AcknowledgeWaitTimer.AutoReset = false; 
            AcknowledgeWaitTimer.Elapsed += QueueStuckTimerElapsed;
        }

        private void QueueStuckTimerElapsed (object sender, System.Timers.ElapsedEventArgs e)
        {
            AcknowledgeWaitTimer.Enabled = false;
            QueueStuck?.Invoke ();
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
                        AcknowledgeWaitTimer.Enabled = true;
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
                AcknowledgeWaitTimer.Enabled = false;

                if (QueueEmpty == false)
                { 
                    currentMessage = pendingMessages.Dequeue ();
                    AcknowledgeWaitTimer.Enabled = true;
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
                AcknowledgeWaitTimer.Enabled = true;
                socket.Send (currentMessage.ToBytes ());
            }
        }
    }

    //****************************************************************************************
    //****************************************************************************************
    //****************************************************************************************
}



