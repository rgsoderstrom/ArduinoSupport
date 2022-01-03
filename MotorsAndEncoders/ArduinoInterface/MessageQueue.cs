using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private bool arduinoReady {get; set;} = false;

        // socket to Arduino
        SocketLib.TcpServer Socket;

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
                Socket.SendToAllClients (msgBytes);
                arduinoReady = false;
            }
        }

        //**********************************************************************

        public void ArduinoReady ()
        {
            if (pendingMessages.Count > 0)
            {
                Socket.SendToAllClients (pendingMessages.Dequeue ());
                arduinoReady = false;
            }

            else
            {
                arduinoReady = true;
             }
        }
    }
}



