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
        bool Verbose = false;

        SocketLibrary.TcpClient thisClientSocket = null;

        string Name;

        //****************************************************************************

        int seconds = 100;

        public ArduinoSim (string name, int sec)
        {
            Name = name;
            seconds = sec;
        }

        //****************************************************************************

        bool Running = true;

        public void Run ()
        {
            try
            {
                Console.WriteLine ("Connecting to server");
                thisClientSocket = new SocketLibrary.TcpClient (PrintToConsole); 

                if (thisClientSocket.Connected == false)
                {
                    Console.WriteLine ("\n\nFailed to connect to server");

                    while (true)
                        Thread.Sleep (1000);
                }

                thisClientSocket.MessageHandler += MessageHandler;
              //thisClientSocket.PrintHandler   += PrintToLog;

                ReadyMsg_Auto readyMsg = new ReadyMsg_Auto ();
                thisClientSocket.Send (readyMsg.ToBytes ());

                while (Running)
                { 
                    Thread.Sleep (1000);
                }

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
                    case (ushort) ArduinoMessageIDs.ClearMsgId:
                        Console.WriteLine ("Clear message received");
                        ClearMessageHandler (msgBytes);
                        break;
                        
                    case (ushort) ArduinoMessageIDs.CollectMsgId:
                        Console.WriteLine ("Collect message received");
                        CollectMessageHandler (msgBytes);
                        break;
                        
                    case (ushort) ArduinoMessageIDs.SendMsgId:
                        Console.WriteLine ("Send message received");
                        SendMessageHandler (msgBytes);
                        break;
                        
                    case (ushort) ArduinoMessageIDs.KeepAliveMsgId:
                        break;
                        
                    default:
                        Console.WriteLine ("Received unrecognized message, Id: " + header.MessageId.ToString ());
                        break;
                }

                MessageHeader hdr = new MessageHeader (msgBytes);

                AcknowledgeMsg_Auto ackMsg = new AcknowledgeMsg_Auto ();
                ackMsg.data.MsgSequenceNumber = hdr.SequenceNumber;
                thisClientSocket.Send (ackMsg.ToBytes ());
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception: " + Name + ", " + ex.Message);
            }
        } 
        
        //***************************************************************************************************************
        //***************************************************************************************************************
        //***************************************************************************************************************

        private int Count = 1024;
        private List<double> Samples = new List<double> ();
        private int get = 0;

        private void ClearMessageHandler (byte [] msgBytes)
        {
            Samples.Clear ();
            get = 0;

            for (int i=0; i<Count; i++)
                Samples.Add (i);

            ReadyMsg_Auto rdyMsg = new ReadyMsg_Auto ();
            thisClientSocket.Send (rdyMsg.ToBytes ());
        }

        //***************************************************************************************************************

        double f = 5;

        private void CollectMessageHandler (byte [] msgBytes)
        {
            Samples.Clear ();
            get = 0;

            for (int i=0; i<Count; i++)
            { 
                Samples.Add (1000 * Math.Sin (2 * Math.PI * f * i / Count));
            }

            ReadyMsg_Auto rdyMsg = new ReadyMsg_Auto ();
            thisClientSocket.Send (rdyMsg.ToBytes ());

            f += 1;
        }

        //***************************************************************************************************************

        private void SendMessageHandler (byte [] msgBytes)
        {
            Console.WriteLine ("Sending, get = " + get.ToString ());

            SampleDataMsg_Auto msg = new SampleDataMsg_Auto ();

            try
            { 
                for (int i=0; i<SampleDataMsg_Auto.Data.MaxCount; i++)
                {
                    msg.data.Sample [i] = (short) Samples [get++];
                }

                thisClientSocket.Send (msg.ToBytes ());

                if (get >= Count)
                {
                    Console.WriteLine ("All Sent");

                    AllSentMsg_Auto msg2 = new AllSentMsg_Auto ();
                    thisClientSocket.Send (msg2.ToBytes ());
                    get = 0;
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception: " + ex.Message);
            }
        }
    }
}


