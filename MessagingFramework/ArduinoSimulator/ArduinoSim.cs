using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using SocketLibrary;
using ArduinoInterface;

namespace ArduinoSimulator
{
    public class ArduinoSim
    {
        bool Verbose = true;

        Timer Timer1 = null;

        SocketLibrary.TcpClient thisClientSocket = null;

        DateTime startTime = DateTime.Now;

        StatusMessage statusMsg;
        string Name;

        //****************************************************************************

        int seconds = 100;

        public ArduinoSim (string name, int sec)
        {
            Name = name;
            seconds = sec;
        }

        //****************************************************************************

        public void Run ()
        {
            try
            {
                statusMsg = new StatusMessage ();
                statusMsg.Name = Name;

                Console.WriteLine ("Connecting to server");
                thisClientSocket = new SocketLibrary.TcpClient ("RandysLaptop", PrintToConsole); 

                if (thisClientSocket.Connected == false)
                {
                    Console.WriteLine ("\n\nFailed to connect to server");

                    while (true)
                        Thread.Sleep (1000);
                }

                thisClientSocket.MessageHandler += MessageHandler;
                thisClientSocket.PrintHandler   += PrintToLog; // PrintToConsole;


                Console.WriteLine (statusMsg.ToString ());

                byte [] aaa = statusMsg.ToBytes ();

                for (int i=0; i<aaa.Length; i++)
                    Console.WriteLine (i + ": " + (int) aaa [i]);




                thisClientSocket.Send (statusMsg.ToBytes ());

                ////Timer1 = new Timer (Timer1Interrupt, this, 5000, 1000);

                for (int i=0; i<seconds; i++)
                    Thread.Sleep (1000);

                PrintToConsole (Name + " closing socket");

                thisClientSocket.Close ();

                while (true)
                    Thread.Sleep (1000);               
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception in Main.Run: " + ex.Message);
            }
        }

        //************************************************************************************

        private void PrintToConsole (string str)
        {
            Console.WriteLine (str);
        }

        private void PrintToLog (string str)
        {
            Console.WriteLine (str);
        }

        //************************************************************************************

        LoopbackDataMsg testInput = null;
        LoopbackDataMsg testOutput = null;

        private void MessageHandler (Socket src, byte [] msgBytes)
        {
            try
            {
                MessageHeader header = new MessageHeader (msgBytes);

                if (Verbose)
                {
                    Console.Write ("Header: {0}, {1}, {2}, {3}", header.Sync, header.ByteCount, header.MessageId, header.SequenceNumber);

                    for (int i = Marshal.SizeOf (header); i<header.ByteCount; i++)
                        Console.Write (" {0}", msgBytes [i]);

                    Console.WriteLine ();
                }

                switch (header.MessageId)
                {
                    case (ushort)PCMessageIDs.LoopbackDataMsgId:
                        testInput = new LoopbackDataMsg (msgBytes);
                        statusMsg.DataReceived = true;
                        Console.WriteLine ("Loopback data");
                        break;

                    case (ushort)PCMessageIDs.RunLoopbackTestMsgId:
                        statusMsg.DataReady = true;
                        Console.WriteLine ("Run Test command");

                        testOutput = new LoopbackDataMsg ();
                        //testOutput.Source = 111;
                        for (int i = 0; i<testInput.Count; i++)
                            testOutput.data.dataWords [i] = ((byte)(127 - testInput.Get (i)));
                          //testOutput.Put ((byte)(127 - testInput.Get (i)));

                        break;

                    case (ushort)PCMessageIDs.SendLoopbackDataMsgId:
                        statusMsg.DataReceived = false;
                        statusMsg.DataReady = false;
                        thisClientSocket.Send (testOutput.ToBytes ());
                        Console.WriteLine ("Send Results command");
                        break;


                    case (ushort)PCMessageIDs.Disconnect:
                    {
                        Console.WriteLine ("Received Disconnect cmnd");
                        thisClientSocket.client.Disconnect (false);
                        Timer1.Change (Timeout.Infinite, Timeout.Infinite);
                    }
                    break;

                    case (ushort)PCMessageIDs.KeepAlive:
                        //Console.WriteLine ("KeepAlive message");
                        break;

                    default:
                        Console.WriteLine ("Received unrecognized message");
                        break;
                }

                MessageHeader hdr = new MessageHeader (msgBytes);
                AcknowledgeMessage msg = new AcknowledgeMessage (hdr.SequenceNumber);
                thisClientSocket.Send (msg.ToBytes ());
                thisClientSocket.Send (statusMsg.ToBytes ());
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception: " + Name + ", " + ex.Message);
            }
        }    
    }
}


