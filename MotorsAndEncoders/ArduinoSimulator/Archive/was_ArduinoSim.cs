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
        bool Verbose = false;

        Timer Timer1 = null;

        Encoder encoder1 = new Encoder (0);
        Encoder encoder2 = new Encoder (1);

        SocketLib.TcpClient thisClientSocket = null;

        List<EncoderCounts> DataCollection = new List<EncoderCounts> ();
        int DataCollectionGet = 0;

        //public uint  time;   // 32 bit millis, from time "run" command received
        //public short enc1; 
        //public short enc2;
        //public byte  s1;  // was char
        //public byte  s2;


        //****************************************************************************

        public ArduinoSim ()
        {
            for (int i=0; i<123; i++)
            {
                EncoderCounts cnts = new EncoderCounts ();
                cnts.time = (uint) (12345 + i * 50);
                cnts.enc1 = (short) (300 + 200 * Math.Sin (2 * Math.PI * i / 100));
                cnts.enc2 = (short) (300 + 100 * Math.Sin (2 * Math.PI * 2 * i / 100));

                cnts.s1 = (byte) (10 + 3 * Math.Sin (2 * Math.PI * 4 * i / 100));
                cnts.s2 = (byte) (9 + 2 * Math.Sin (2 * Math.PI * 6 * i / 100));
                DataCollection.Add (cnts);
            }
        }

        //****************************************************************************

        DateTime startTime = DateTime.Now;

        //UInt32 millis ()
        //{
        //    DateTime now = DateTime.Now;
        //    TimeSpan messageTimeTag = now - startTime;
        //    return (UInt32) messageTimeTag.TotalMilliseconds;
        //}

        ////****************************************************************************

        //UInt32 previousMillis = 0;

        //static void Timer1Interrupt (object state)
        //{
        //    ArduinoSim arduino = (ArduinoSim)state;
        //    //arduino.Sample ();
        //}

        //UInt32 runStartTime = 0; // milliseconds

        //void Sample ()
        //{
        //}

        //****************************************************************************

        public void Run ()
        {
            try
            {
                Console.WriteLine ("Connect to server");
                thisClientSocket = new SocketLib.TcpClient (PrintToConsole);
            }

            catch (Exception)
            {
                Console.WriteLine ("Exception in Main");
            }

            if (thisClientSocket.Connected == false)
            {
                Console.WriteLine ("\n\nFailed to connect to server");

                while (true)
                    Thread.Sleep (1000);
            }

            thisClientSocket.MessageHandler += MessageHandler;
            thisClientSocket.PrintHandler   += PrintToConsole;

            //Timer1 = new Timer (Timer1Interrupt, this, 5000, 1000);

            while (true)
                Thread.Sleep (1000);
        }

        //************************************************************************************

        private void PrintToConsole (string str)
        {
            Console.WriteLine (str);
        }

        //************************************************************************************

        bool SendNextDataMessage ()
        {
            CollectionDataMessage msg = new CollectionDataMessage ();

            while (DataCollectionGet < DataCollection.Count)
            {
                if (msg.Add (DataCollection [DataCollectionGet++]) == false)
                {
                    thisClientSocket.Send (msg.ToBytes ());
                    return true;
                }
            }

            if (msg.Empty == false)
                thisClientSocket.Send (msg.ToBytes ());

            return false;
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
                case (ushort)CommandMessageIDs.ProfileSection:
                {
                    ProfileSectionRcvdMessage msg = new ProfileSectionRcvdMessage ();
                    thisClientSocket.Send (msg.ToBytes ());
                }
                break;

                case (ushort)CommandMessageIDs.RunProfile:
                    Console.WriteLine ("received RunProfile command");
                    break;

                case (ushort) CommandMessageIDs.KeepAlive:
                {
                    if (Verbose) Console.WriteLine ("Received KeepAlive msg");
                }
                break;

                //case (ushort) CommandMessageIDs.MotorSpeed:
                //{
                //    MotorSpeedMsg msg = new MotorSpeedMsg (msgBytes);
                //    Console.WriteLine ("Received MotorSpeed cmnd: {0}", msg.ToString ());

                //    TextMessage tm = new TextMessage ("Received Speeds");
                //    thisClientSocket.Send (tm.ToBytes ());

                //    BufferStatusMessage bs = new BufferStatusMessage (1);
                //    thisClientSocket.Send (bs.ToBytes ());
                //}
                //break;

                //case (ushort)CommandMessageIDs.StartCollection:
                //{
                //    Console.WriteLine ("Received StartCollection cmnd");

                //    TextMessage tm = new TextMessage ("StartCollection");
                //    thisClientSocket.Send (tm.ToBytes ());

                //    BufferStatusMessage bs = new BufferStatusMessage (0);
                //    thisClientSocket.Send (bs.ToBytes ());
                //}
                //break;

                //case (ushort)CommandMessageIDs.StopCollection:
                //{
                //    Console.WriteLine ("Received StopCollection cmnd");
                //}
                //break;

                //case (ushort)CommandMessageIDs.SendFirstCollection:
                //{
                //    Console.WriteLine ("Received SendCollection cmnd");
                //    BufferStatusMessage bs = new BufferStatusMessage (1);
                //    thisClientSocket.Send (bs.ToBytes ());
                //    DataCollectionGet = 0;

                //    if (SendNextDataMessage () == false)
                //    {
                //        CollSendCompleteMessage msg2 = new CollSendCompleteMessage ();
                //        thisClientSocket.Send (msg2.ToBytes ());
                //    }
                //}
                //break;

                //case (ushort)CommandMessageIDs.SendNextCollection:
                //{
                //    if (SendNextDataMessage () == false)
                //    {
                //        CollSendCompleteMessage msg2 = new CollSendCompleteMessage ();
                //        thisClientSocket.Send (msg2.ToBytes ());
                //    }
                //}
                //break;

                //case (ushort)CommandMessageIDs.ClearCollection:
                //{
                //    Console.WriteLine ("Received ClearCollection cmnd");
                //}
                //break;

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
        }
    }
}
