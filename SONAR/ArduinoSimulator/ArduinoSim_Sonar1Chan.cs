
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
        private readonly int BatchSize = 4096;

        public ArduinoSim_Sonar1Chan (string name, SocketLibrary.TcpClient sock, PrintCallback ptl) : base (name, sock, ptl)
        {    
            MessageHeader.NextSequenceNumber = 1;
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

                bool SendReadyMessage = true; // a "ready" message ends most exchanges but for some a
                                              // different reply message serves its purpose

                //**************************************************************************

                switch (header.MessageId)
                {
                    case (ushort) ArduinoMessageIDs.KeepAliveMsgId:
                        if (Verbose) PrintToLog ("KeepAlive message received");
                        break;
                        
                    case (ushort) ArduinoMessageIDs.ClearSamplesMsgId:
                        if (Verbose) PrintToLog ("Clear message received");
                        ClearMessageHandler (msgBytes);
                        break;
                        
                    case (ushort) ArduinoMessageIDs.BeginPingCycleMsgId:
                        if (Verbose) PrintToLog ("Collect message received");
                        PingMessageHandler (msgBytes);
                        break;
                        
                    case (ushort) ArduinoMessageIDs.SendSamplesMsgId:
                        if (Verbose) PrintToLog ("Send message received");
                        SendSamplesMessageHandler (msgBytes);
                        SendReadyMessage = false;
                        break;
                        
                    case (ushort) ArduinoMessageIDs.SonarParametersMsgId:
                        if (Verbose) PrintToLog ("SONAR parameters message received");
                        SonarParametersMessageHandler (msgBytes);
                        break;
                        
                    default:
                        PrintToLog ("Received unrecognized message, Id: " + header.MessageId.ToString ());
                        break;
                }

                //**************************************************************************
                //
                // Report ready for next message
                //
                if (SendReadyMessage == true)
                { 
                    ReadyMsg_Auto msg = new ReadyMsg_Auto ();
                    thisClientSocket.Send (msg.ToBytes ());
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

        // Processing parameters

        double SampleRate = 100000; // these are written to Matlab "save" file
        double PingDuration = 0.0005;
        double PingFrequency = 40200;

        private void SonarParametersMessageHandler (byte [] msgBytes)
        {
            SonarParametersMsg_Auto msg = new SonarParametersMsg_Auto (msgBytes);

            SampleRate    = 50e6 / msg.data.SampleClockDivisor;
            PingDuration  = msg.data.PingDuration / 50e6; // seconds
            PingFrequency = msg.data.PingFrequency * 190;

            PrintToLog ("Parameters: " + msg.ToString ());
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
        }

        //***************************************************************************************************************
        //
        // Ping - Create a batch of simulated samples
        //
        static Random random = new Random ();

        readonly double Range = 5; // feet

        readonly int    Ramp  = 20;
        readonly double ampl  = 200;
        readonly double noise = 10;
        readonly int    DC    = 512;

        private void PingMessageHandler (byte [] msgBytes)
        {
            Samples.Clear ();
            samplesGetIndex = 0;

            int PingSamples = (int) (PingDuration * 100000); // seconds * Samples / second

            double BlankingTime = PingDuration + 0.003; // seconds from ping command to first sample        
            double travelTime   = 2 * Range / 1125;
            int LeadingZero     = (int) ((travelTime - BlankingTime) * SampleRate);

            double time = 0;

            // before the target return begins
            for (int i=0; i<LeadingZero; i++, time+=1/SampleRate)
            {
                double withNoiseAndDC = noise * random.NextDouble () + DC;
                Samples.Add (Math.Truncate (withNoiseAndDC));
            }

            // returns from time the xmit waveform is ramping up
            for (int i=0; i<Ramp; i++, time+=1/SampleRate)
            {
                double win = (double) i / Ramp;
                double s = win * ampl * Math.Sin (2 * Math.PI * PingFrequency * time);
                double withNoiseAndDC = noise * random.NextDouble () + DC + s;
                Samples.Add (Math.Truncate (withNoiseAndDC));
            }            
            
            // from time at max level
            for (int i=0; i<PingSamples; i++, time+=1/SampleRate)
            {
                double win = 1;
                double s = win * ampl * Math.Sin (2 * Math.PI * PingFrequency * time);
                double withNoiseAndDC = noise * random.NextDouble () + DC + s;
                Samples.Add (Math.Truncate (withNoiseAndDC));
            }            
            
            // while ramping down
            for (int i=0; i<Ramp; i++, time+=1/SampleRate)
            {
                double win = 1 - (double) i / Ramp;
                double s = win * ampl * Math.Sin (2 * Math.PI * PingFrequency * time);
                double withNoiseAndDC = noise * random.NextDouble () + DC + s;
                Samples.Add (Math.Truncate (withNoiseAndDC));
            }            

            // after target return ends
            int rem = BatchSize - Samples.Count;
            
            for (int i=0; i<rem; i++, time+=1/SampleRate)
            {
                double withNoiseAndDC = noise * random.NextDouble () + DC;
                Samples.Add (Math.Truncate (withNoiseAndDC));
            }
        }

        //***************************************************************************************************************
        //
        // SendSamplesMessageHandler
        //
        private void SendSamplesMessageHandler (byte [] msgBytes)
        {
            int remaining = Samples.Count - samplesGetIndex;

            if (remaining < 0)
                remaining = 0;

            short thisMsgCount = (short) (remaining < PingReturnDataMsg_Auto.Data.MaxCount ? remaining : PingReturnDataMsg_Auto.Data.MaxCount);

            if (Verbose)
                PrintToLog ("Sending " + thisMsgCount + " samples");

            PingReturnDataMsg_Auto msg = new PingReturnDataMsg_Auto ();
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
