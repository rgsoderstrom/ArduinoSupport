using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using SocketLib;
using ArduinoInterface;

namespace ArduinoSimulator
{
    public class ArduinoSim
    {
        bool Verbose = true;
        Timer Timer1 = null;

        SocketLib.TcpClient thisClientSocket = null;
        DateTime startTime = DateTime.Now;

        DriveWheelEncoders encoders = new DriveWheelEncoders ();

        //****************************************************************************

        public ArduinoSim ()
        {
        }

        //****************************************************************************

        public void Run ()
        {
            try
            {
                Console.WriteLine ("Connect to server");
                thisClientSocket = new SocketLib.TcpClient (PrintToLog); // (PrintToConsole);
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception in Main: " + ex.Message);
            }

            if (thisClientSocket.Connected == false)
            {
                Console.WriteLine ("\n\nFailed to connect to server");

                while (true)
                    Thread.Sleep (1000);
            }

            thisClientSocket.MessageHandler += MessageHandler;
            thisClientSocket.PrintHandler   += PrintToLog; // PrintToConsole;

            StatusMessage msg = new StatusMessage ();
            msg.data.readyForMessages = 1;
            thisClientSocket.Send (msg.ToBytes ());

            //Timer1 = new Timer (Timer1Interrupt, this, 5000, 1000);

            while (true)
                Thread.Sleep (1000);
        }

        //************************************************************************************

        private void PrintToConsole (string str)
        {
            Console.WriteLine (str);
        }

        private void PrintToLog (string str)
        {
            //Console.WriteLine (str);
        }

        //************************************************************************************

        private void MessageHandler (Socket src, byte [] msgBytes)
        {
            Header header = new Header (msgBytes);

            if (Verbose)
            {
                Console.Write ("Header: {0}, {1}, {2}, {3}", header.Sync, header.ByteCount, header.MessageId, header.SequenceNumber);

                for (int i = Marshal.SizeOf (header); i<header.ByteCount; i++)
                    Console.Write (" {0}", msgBytes [i]);

                Console.WriteLine ();
            }

            switch (header.MessageId)
            {
                case (ushort) CommandMessageIDs.MotorProfileSegment:
                {
                    MotorSpeedProfileMsg rcvd = new MotorSpeedProfileMsg (msgBytes);
                    encoders.AddProfileSegment (rcvd.data);
                    //Console.WriteLine (string.Format ("received MotorSpeed message: Motor {0}, Index {1}, Speed {2}, Duration {3}", rcvd.data.motorID, rcvd.data.index, rcvd.data.speed, rcvd.data.duration));
                }
                break;

                case (ushort)CommandMessageIDs.ClearMotorProfile:
                    encoders.ClearSpeedProfile ();
                    break;

                case (ushort) CommandMessageIDs.RunMotors:
                    //Console.WriteLine ("received RunMotors command");

                    //TextMessage tm = new TextMessage ("run");
                    //thisClientSocket.Send (tm.ToBytes ());

                    break;

                case (ushort) CommandMessageIDs.SlowStopMotors:
                    //Console.WriteLine ("received SlowStopMotors command");
                    break;

                case (ushort) CommandMessageIDs.FastStopMotors:
                    //Console.WriteLine ("received FastStopMotors command");
                    break;

                case (ushort) CommandMessageIDs.KeepAlive:
                    if (Verbose) Console.WriteLine ("Received KeepAlive msg");
                    break;



                case (ushort) CommandMessageIDs.SendFirstCollection:
                {
                    //Common.EventLog.WriteLine ("SendFirst");
                    EncoderCountsMessage.Batch batch = encoders.GetFirstSampleBatch ();
                    EncoderCountsMessage ecm = new EncoderCountsMessage (batch);
                    thisClientSocket.Send (ecm.ToBytes ());
                }
                break;


                case (ushort) CommandMessageIDs.SendNextCollection:
                {
                    //Common.EventLog.WriteLine ("SendNext");
                    EncoderCountsMessage.Batch batch = encoders.GetNextSampleBatch ();
                    EncoderCountsMessage ecm = new EncoderCountsMessage (batch);

                    //Common.EventLog.WriteLine (ecm.ToString ());

                    thisClientSocket.Send (ecm.ToBytes ());
                }
                break;






                case (ushort) CommandMessageIDs.Disconnect:
                {
                    Console.WriteLine ("Received Disconnect cmnd");
                    thisClientSocket.client.Disconnect (false);
                    Timer1.Change (Timeout.Infinite, Timeout.Infinite);
                }
                break;

                default:
                    Console.WriteLine ("Received unrecognized message");
                    break;
            }

            Header hdr = new Header (msgBytes);
            AcknowledgeMessage msg = new AcknowledgeMessage (hdr.SequenceNumber);
            thisClientSocket.Send (msg.ToBytes ());
        }
    }
}
