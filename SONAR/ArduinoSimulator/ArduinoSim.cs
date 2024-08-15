using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using SocketLibrary;
using ArduinoInterface;
using Common;
using System.ComponentModel.Design;

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

        string machineName = "RandysLaptop";
        //string machineName = "RandysLG";

        public void Run ()
        {
            try
            {
                string str = Environment.CurrentDirectory;
                Console.WriteLine ("cwd " + str);

                PrintToLog ("Connecting to server");
                thisClientSocket = new SocketLibrary.TcpClient (machineName, PrintToConsole); 

                if (thisClientSocket.Connected == false)
                {
                    PrintToLog ("\n\nFailed to connect to server");

                    while (true)
                        Thread.Sleep (1000);
                }

                thisClientSocket.MessageHandler += MessageHandler;
                thisClientSocket.PrintHandler   += PrintToLog;

                ReadyMsg_Auto readyMsg = new ReadyMsg_Auto ();
                thisClientSocket.Send (readyMsg.ToBytes ());

                TextMessage msg2 = new TextMessage ("Arduino sim ready");
                thisClientSocket.Send (msg2.ToBytes ());

                while (Running)
                { 
                    Thread.Sleep (1000);
                }

                PrintToLog (Name + " closing socket");

                thisClientSocket.Close ();

                while (true)
                    Thread.Sleep (1000);
            }

            catch (Exception ex)
            {
                PrintToLog ("Exception in Main.Run: " + ex.Message);
            }
        }

        //************************************************************************************

        private void PrintToConsole (string str)
        {
            Console.WriteLine (str);
        }

        private void PrintToLog (string str)
        {
            EventLog.WriteLine (str);
            Console.WriteLine (str);
        }

        //************************************************************************************

        static int aaa = 5;

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

                //**************************************************************************
                //
                // Acknowledge before handling, like real Arduino
                //
                MessageHeader hdr = new MessageHeader (msgBytes);

                //if (aaa-- == 0)
                //    hdr.SequenceNumber = 77;

                //if (aaa == -5)
                //    hdr.SequenceNumber = 77;

                AcknowledgeMsg_Auto ackMsg = new AcknowledgeMsg_Auto ();
                ackMsg.data.MsgSequenceNumber = hdr.SequenceNumber;
                thisClientSocket.Send (ackMsg.ToBytes ());

                //**************************************************************************

                switch (header.MessageId)
                {
                    case (ushort) ArduinoMessageIDs.ClearMsgId:
                        if (Verbose) PrintToLog ("Clear message received");
                        ClearMessageHandler (msgBytes);
                        break;
                        
                    case (ushort) ArduinoMessageIDs.CollectMsgId:
                        if (Verbose) PrintToLog ("Collect message received");
                        CollectMessageHandler (msgBytes);
                        break;
                        
                    case (ushort) ArduinoMessageIDs.SendMsgId:
                        if (Verbose) PrintToLog ("Send message received");
                        SendMessageHandler (msgBytes);
                        break;
                        
                    case (ushort) ArduinoMessageIDs.KeepAliveMsgId:
                        break;
                        
                    default:
                        PrintToLog ("Received unrecognized message, Id: " + header.MessageId.ToString ());
                        break;
                }
            }

            catch (Exception ex)
            {
                PrintToLog ("Exception: " + Name + ", " + ex.Message);
            }
        } 
        
        //***************************************************************************************************************
        //***************************************************************************************************************
        //***************************************************************************************************************

        private const int BatchSize = 1024;
        private const double SampleRate = 100000;
        private const double Resolution = SampleRate / BatchSize;

        private const double SignalBin = 200; // 200.5;
        private const double Frequency = SignalBin * Resolution;

        private List<double> Samples = new List<double> ();
        private int get = 0;

        private void ClearMessageHandler (byte [] msgBytes)
        {
            Samples.Clear ();
            get = 0;

            for (int i=0; i<BatchSize; i++) // Write a test pattern. Will be overwritten by "Collect"
                Samples.Add (i);

            ReadyMsg_Auto rdyMsg = new ReadyMsg_Auto ();
            thisClientSocket.Send (rdyMsg.ToBytes ());
        }

        //***************************************************************************************************************

        static Random random = new Random ();

        private void CollectMessageHandler (byte [] msgBytes)
        {
            Console.WriteLine ("Frequency = " + Frequency);
            Samples.Clear ();
            get = 0;

            double t = 0;

            for (int i=0; i<BatchSize; i++, t+=1/SampleRate)
            { 
                Samples.Add (2 * random.NextDouble () + 512 + 500 * Math.Sin (2 * Math.PI * Frequency * t));
            }

            ReadyMsg_Auto rdyMsg = new ReadyMsg_Auto ();
            thisClientSocket.Send (rdyMsg.ToBytes ());
        }

        //***************************************************************************************************************

        private void SendMessageHandler (byte [] msgBytes)
        {
            if (Verbose)
                PrintToLog ("Sending, get = " + get.ToString ());

            SampleDataMsg_Auto msg = new SampleDataMsg_Auto ();

            try
            { 
                for (int i=0; i<SampleDataMsg_Auto.Data.MaxCount; i++)
                {
                    msg.data.Sample [i] = (short) Samples [get++];
                }

                thisClientSocket.Send (msg.ToBytes ());

                if (get >= BatchSize)
                {
                    PrintToLog ("All Sent");

                    AllSentMsg_Auto msg2 = new AllSentMsg_Auto ();
                    thisClientSocket.Send (msg2.ToBytes ());
                    get = 0;
                }
            }

            catch (Exception ex)
            {
                PrintToLog ("Exception: " + ex.Message);
            }
        }
    }
}


