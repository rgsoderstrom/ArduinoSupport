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

        SocketLib.TcpClient thisClientSocket = null;
        DateTime startTime = DateTime.Now;

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
            thisClientSocket.PrintHandler   += PrintToLog; // PrintToConsole;

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
                case (ushort)CommandMessageIDs.ProfileSection:
                {
                    ProfileSectionMsg rcvd = new ProfileSectionMsg (msgBytes);
                    Console.WriteLine (string.Format ("received ProfileSection message, index: {0}, count: {1}", rcvd.data.index, rcvd.data.numberValues));

                    for (int i=0; i<rcvd.data.numberValues; i++)
                    {
                        Console.WriteLine (string.Format ("  {0}, {1}", rcvd.data.LeftSpeed [i], rcvd.data.RightSpeed [i]));
                    }

                    ProfileSectionRcvdMessage msg = new ProfileSectionRcvdMessage ();
                    thisClientSocket.Send (msg.ToBytes ());
                }
                break;

                case (ushort)CommandMessageIDs.RunProfile:
                    Console.WriteLine ("received RunProfile command");
                    break;

                case (ushort)CommandMessageIDs.ClearProfile:
                    Console.WriteLine ("received ClearProfile command");
                    break;

                case (ushort) CommandMessageIDs.KeepAlive:
                {
                    if (Verbose) Console.WriteLine ("Received KeepAlive msg");
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
        }
    }
}
