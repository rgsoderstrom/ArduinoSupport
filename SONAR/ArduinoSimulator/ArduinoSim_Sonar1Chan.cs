
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ArduinoInterface;
using SocketLibrary;
using SonarCommon;

namespace ArduinoSimulator
{
    public class ArduinoSim_Sonar1Chan : ArduinoSim
    {
        private readonly int BatchSize = 4096;
        private readonly int PeakPickWindow = 16;

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

                //if (Verbose)
                //{
                //    PrintToLog ("");

                //    PrintToLog (string.Format ("Header: {0}, {1}, {2}, {3}", header.Sync, header.ByteCount, header.MessageId, header.SequenceNumber));

                //    for (int i = Marshal.SizeOf (header); i<header.ByteCount; i++)
                //        PrintToLog (string.Format (" {0}", msgBytes [i]));
                //}

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
                        if (Verbose) PrintToLog ("Begin Ping Cycle message received");
                        PingMessageHandler (msgBytes);
                        break;
                        
                    case (ushort) ArduinoMessageIDs.SendRawSamplesMsgId:
                        if (Verbose) PrintToLog ("Send raw samples message received");
                        SendRawSamplesMessageHandler (msgBytes);
                        SendReadyMessage = false;
                        break;
                        
                    case (ushort) ArduinoMessageIDs.SendMfSamplesMsgId:
                        if (Verbose) PrintToLog ("Send MF samples message received");
                        SendMfDataMessageHandler (msgBytes);
                        SendReadyMessage = false;
                        break;
                        
                    case (ushort) ArduinoMessageIDs.SonarParametersMsgId:
                        if (Verbose) PrintToLog ("Parameters message received");
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
        double PingDurationSecs = 0.0005;
        double PingFrequency = 40200;

        private void SonarParametersMessageHandler (byte [] msgBytes)
        {
            SonarParametersMsg_Auto msg = new SonarParametersMsg_Auto (msgBytes);

            SampleRate       = 50e6 / msg.data.SampleClockDivisor;
            PingDurationSecs = msg.data.PingDuration / 50e6; // seconds
            PingFrequency    = msg.data.PingFrequency * 190;


            //PrintToLog ("Parameters: " + msg.ToString ());
        }

        //***************************************************************************************************************

        private List<double> RawSamples = new List<double> ();
        private int rawSamplesGetIndex = 0;

        private List<byte> MatchedFilterSamples = new List<byte> ();
        private int mfGetIndex = 0;

        private void ClearMessageHandler (byte [] msgBytes)
        {
            RawSamples.Clear ();
            rawSamplesGetIndex = 0;

            MatchedFilterSamples.Clear ();
            mfGetIndex = 0;

            for (int i=0; i<BatchSize; i++) // Write a test pattern. Will be overwritten by "Collect"
                RawSamples.Add (i);

            for (int i=0; i<BatchSize/PeakPickWindow; i++)
                MatchedFilterSamples.Add (0);//(byte) i);
        }

        //***************************************************************************************************************
        //
        // Ping - Create a batch of simulated samples
        //

        private void PingMessageHandler (byte [] msgBytes)
        {
            RawSamples.Clear ();
            rawSamplesGetIndex = 0;

            MatchedFilterSamples.Clear ();
            mfGetIndex = 0;

            GenerateRawSamples    (BatchSize, RawSamples);
            GenerateMatchedFilter (PeakPickWindow, RawSamples, MatchedFilterSamples);
        }

        //
        // just fill buffers with a counting pattern
        //
        private void GenerateMatchedFilter (int peakPickWindow, List<double> rawSamples, List<byte> mfSamples) 
        {
            int count = rawSamples.Count / peakPickWindow;

            for (int i=0; i<count; i++)
            { 
                mfSamples.Add ((byte) rawSamples [i * peakPickWindow]);
            }

            Common.EventLog.WriteLine (mfSamples [0] + " MF ramp start");
        }

        static double FirstRawSample = 1000;
            
        private void GenerateRawSamples (int count, List<double> samples)
        { 
            for (int i=0; i<count; i++)
                samples.Add (FirstRawSample + i);

            FirstRawSample -= 100;
        }

        //static Random random = new Random ();

        //readonly double Range = 10; // feet

        //readonly double ampl  = 200;
        //readonly double noise = 10;
        //readonly int    DC    = 0;
        //
        //private void GeneratePingSamples (List<double> samples)
        //{ 
        //    int PingSamples = (int) (PingDurationSecs * SampleRate); // seconds * Samples / second

        //    Common.EventLog.WriteLine (SampleRate + " sample rate");
        //    Common.EventLog.WriteLine (PingDurationSecs + " ping duration, seconds");
        //    Common.EventLog.WriteLine (PingSamples + " ping samples at max amplitude");

        //    double BlankingTime = PingDurationSecs + 0.003; // seconds from ping command to first sample        
        //    double travelTime   = 2 * Range / 1125;
        //    int LeadingZero     = (int) ((travelTime - BlankingTime) * SampleRate);

        //    // samples for transmitted waveform
        //    SonarCommon.TxWaveGen.CW transmitWave = new TxWaveGen.CW (ampl, SampleRate, PingFrequency, PingDurationSecs * 1000);
        //    double time = 0;

        //    // before the target return begins
        //    for (int i=0; i<LeadingZero; i++, time+=1/SampleRate)
        //    {
        //        double withNoiseAndDC = noise * (random.NextDouble () - 0.5) + DC;
        //        samples.Add (Math.Truncate (withNoiseAndDC));
        //    }

        //    // copy target return
        //    for (int i = 0; i<transmitWave.Samples.Count; i++, time+=1/SampleRate)
        //    {
        //        samples.Add (transmitWave.Samples [i] + (random.NextDouble () - 0.5) + DC);
        //    }

        //    // after target return ends
        //    int rem = BatchSize - samples.Count;
            
        //    for (int i=0; i<rem; i++, time+=1/SampleRate)
        //    {
        //        double withNoiseAndDC = noise * (random.NextDouble () - 0.5) + DC;
        //        samples.Add (Math.Truncate (withNoiseAndDC));
        //    }
        //}
        //

        //***************************************************************************************************************
        //
        // SendSamplesMessageHandler
        //
        private void SendRawSamplesMessageHandler (byte [] msgBytes)
        {
            int remaining = RawSamples.Count - rawSamplesGetIndex;

            if (remaining < 0)
                remaining = 0;

            short thisMsgCount = (short) (remaining < PingReturnRawDataMsg_Auto.Data.MaxCount ? remaining : PingReturnRawDataMsg_Auto.Data.MaxCount);

            if (Verbose)
                PrintToLog ("Sending " + thisMsgCount + " samples");

            PingReturnRawDataMsg_Auto msg = new PingReturnRawDataMsg_Auto ();
            msg.data.Count = thisMsgCount;

            try
            { 
                for (int i=0; i<thisMsgCount; i++)
                {
                    msg.data.Sample [i] = (short) RawSamples [rawSamplesGetIndex++];
                }

                thisClientSocket.Send (msg.ToBytes ());
            }

            catch (Exception ex)
            {
                PrintToLog ("Exception: " + ex.Message);
            }
        }

        //
        // Send Matched Filter Data Message Handler
        //
        private void SendMfDataMessageHandler (byte [] msgBytes)
        {
            int remaining = MatchedFilterSamples.Count - mfGetIndex;

            if (remaining < 0)
                remaining = 0;

            short thisMsgCount = (short) (remaining < PingReturnMfDataMsg_Auto.Data.MaxCount ? remaining 
                                                    : PingReturnMfDataMsg_Auto.Data.MaxCount);

            if (Verbose)
                PrintToLog ("Sending " + thisMsgCount + " matched filter data");

            PingReturnMfDataMsg_Auto msg = new PingReturnMfDataMsg_Auto ();
            msg.data.Count = thisMsgCount;

            try
            { 
                for (int i=0; i<thisMsgCount; i++)
                {
                    msg.data.Sample [i] = MatchedFilterSamples [mfGetIndex++];
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
