
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using ArduinoInterface;

using SocketLibrary;

namespace ArduinoSimulator
{
    public class ArduinoSim_Sonar1Chan : ArduinoSim
    {
        private double SampleRate = 100000;
        private readonly int BatchSize = 4096;
        private double Frequency = 40000;

        public ArduinoSim_Sonar1Chan (string name, SocketLibrary.TcpClient sock, PrintCallback ptl) : base (name, sock, ptl)
        {    
            thisClientSocket.MessageHandler += MessageHandler;
        }

        //************************************************************************************

        private void MessageHandler (Socket src, byte [] msgBytes)
        {
            try
            {
                MessageHeader header = new MessageHeader (msgBytes);

                if (Verbose)
                {
                    PrintToLog ("");

                    PrintToLog (string.Format ("Header: {0}, {1}, {2}, {3}", header.Sync, header.ByteCount, header.MessageId, header.SequenceNumber));

                    for (int i = Marshal.SizeOf (header); i<header.ByteCount; i++)
                        PrintToLog (string.Format (" {0}", msgBytes [i]));
                }

                //**************************************************************************
                //
                // Acknowledge before handling
                //

                AcknowledgeMsg_Auto ackMsg = new AcknowledgeMsg_Auto ();
                ackMsg.data.MsgSequenceNumber = header.SequenceNumber;
                thisClientSocket.Send (ackMsg.ToBytes ());

                //**************************************************************************

                switch (header.MessageId)
                {
                    case (ushort) ArduinoMessageIDs.ClearSamplesMsgId:
                        if (Verbose) PrintToLog ("Clear message received");
                        ClearMessageHandler (msgBytes);
                        break;
                        
                    case (ushort) ArduinoMessageIDs.BeginSamplingMsgId:
                        if (Verbose) PrintToLog ("Collect message received");
                        CollectMessageHandler (msgBytes);
                        break;
                        
                    case (ushort) ArduinoMessageIDs.BeginPingCycleMsgId:
                        if (Verbose) PrintToLog ("Collect message received");
                        CollectMessageHandler (msgBytes);
                        break;
                        
                    case (ushort) ArduinoMessageIDs.SendSamplesMsgId:
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
                        if (Verbose) PrintToLog ("Keep-Alive message received");
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

            PrintToLog ("Sample rate: " + SampleRate);
        }

        private void AnalogGainMessageHandler (byte [] msgBytes)
        {
            AnalogGainMsg_Auto msg = new AnalogGainMsg_Auto (msgBytes);
            PrintToLog ("DAC word: " + msg.data.DacValue);
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
            PrintToLog ("Frequency = " + Frequency);

            if (harmonics.Count > 1)
                PrintToLog ("plus " + (harmonics.Count - 1) + " harmonics");

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
