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

        private readonly string ThisArduinoName;
        private readonly string ServerName;

        private          double SampleRate;
        private readonly int    BatchSize; // number of samples in one collection
        private          double Frequency;

        //****************************************************************************

        public ArduinoSim (string name, 
                           string serverName, 
                           double sampleRate, 
                           int    batchSize,
                           double frequency)
        {            
            ThisArduinoName = name;
            ServerName      = serverName;
            SampleRate      = sampleRate;
            BatchSize       = batchSize;
            Frequency       = frequency;
        }

        //****************************************************************************

        bool Running = true;

        public void Run ()
        {
            try
            {
                string str = Environment.CurrentDirectory;
                Console.WriteLine ("cwd " + str);

                PrintToLog ("Connecting to server");
                thisClientSocket = new SocketLibrary.TcpClient (ServerName, PrintToConsole); 

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

                PrintToLog (ThisArduinoName + " closing socket");

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
                        SendSamplesMessageHandler (msgBytes);
                        break;
                        
                    case (ushort) ArduinoMessageIDs.SampleRateMsgId:
                        if (Verbose) PrintToLog ("Sample rate message received");
                        SampleRateMessageHandler (msgBytes);
                        break;
                        
                    case (ushort) ArduinoMessageIDs.AnalogGainMsgId:
                        if (Verbose) PrintToLog ("Analog gain message received");
                        AnalogGainMessageHandler (msgBytes);
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
                PrintToLog ("Exception: " + ThisArduinoName + ", " + ex.Message);
            }
        } 
        
        //***************************************************************************************************************
        //***************************************************************************************************************
        //***************************************************************************************************************

        private void SampleRateMessageHandler (byte [] msgBytes)
        {
            SampleRateMsg_Auto msg = new SampleRateMsg_Auto (msgBytes);
            SampleRate = 50e6 / msg.data.RateDivisor;

            PrintToConsole ("Sample rate: " + SampleRate);
        }

        private void AnalogGainMessageHandler (byte [] msgBytes)
        {
            AnalogGainMsg_Auto msg = new AnalogGainMsg_Auto (msgBytes);
            PrintToConsole ("DAC word: " + msg.data.DacValue);
        }

        //***************************************************************************************************************

        private List<double> Samples = new List<double> ();
        private int samplesGetIndex = 0;

        private void ClearMessageHandler (byte [] msgBytes)
        {
            Samples.Clear ();
            samplesGetIndex = 0;

            for (int i=0; i<BatchSize; i++) // Write a test pattern. Will be overwritten by "Collect"
                Samples.Add (i);

            ReadyMsg_Auto rdyMsg = new ReadyMsg_Auto ();
            thisClientSocket.Send (rdyMsg.ToBytes ());
        }

        //***************************************************************************************************************

        // Create a batch of simulated samples

        static Random random = new Random ();

        List<double> harmonics = new List<double> () {1};//, 2, 3, 4, 5};

        private void CollectMessageHandler (byte [] msgBytes)
        {
            PrintToConsole ("Frequency = " + Frequency);
            if (harmonics.Count > 1)
                PrintToConsole ("plus " + (harmonics.Count - 1) + " harmonics");

            Samples.Clear ();
            samplesGetIndex = 0;

            double time = 0;

            for (int i=0; i<BatchSize; i++, time+=1/SampleRate)
            { 
                double s = 0;

                for (int k=0; k<harmonics.Count; k++)
                { 
                    double ampl = k == 0 ? 1 : 0.1;
                    s += ampl * 500 * Math.Sin (2 * Math.PI * Frequency * harmonics [k] * time);
                }

                double withNoiseAndDC = random.NextDouble () + 512 + s;
                Samples.Add (Math.Truncate (withNoiseAndDC));
            }

            ReadyMsg_Auto rdyMsg = new ReadyMsg_Auto ();
            thisClientSocket.Send (rdyMsg.ToBytes ());
        }

        //***************************************************************************************************************

        private void SendSamplesMessageHandler (byte [] msgBytes)
        {
            int remaining = Samples.Count - samplesGetIndex;

            if (remaining < 0)
                remaining = 0;

            short thisMsgCount = (short) (remaining < SampleDataMsg_Auto.Data.MaxCount ? remaining : SampleDataMsg_Auto.Data.MaxCount);

            if (Verbose)
                PrintToLog ("Sending " + thisMsgCount + " samples");

            SampleDataMsg_Auto msg = new SampleDataMsg_Auto ();
            msg.data.Count = thisMsgCount;

            try
            { 
                for (int i=0; i<thisMsgCount; i++)
                {
                    msg.data.Sample [i] = (short) Samples [samplesGetIndex++];
                }

                thisClientSocket.Send (msg.ToBytes ());
            }

            catch (Exception ex)
            {
                PrintToLog ("Exception: " + ex.Message);
            }
        }
    }
}


