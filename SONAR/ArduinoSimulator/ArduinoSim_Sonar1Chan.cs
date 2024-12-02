﻿
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
                    case (ushort) ArduinoMessageIDs.KeepAliveMsgId:
                        if (Verbose) PrintToLog ("KeepAlive message received");
                        break;
                        
                    case (ushort) ArduinoMessageIDs.ClearSamplesMsgId:
                        if (Verbose) PrintToLog ("Clear message received");
                        ClearMessageHandler (msgBytes);
                        break;
                        
                    case (ushort) ArduinoMessageIDs.BeginPingCycleMsgId:
                        if (Verbose) PrintToLog ("Collect message received");
                        CollectMessageHandler (msgBytes);
                        break;
                        
                    case (ushort) ArduinoMessageIDs.SendSamplesMsgId:
                        if (Verbose) PrintToLog ("Send message received");
                        SendSamplesMessageHandler (msgBytes);
                        break;
                        
                    case (ushort) ArduinoMessageIDs.SonarParametersMsgId:
                        if (Verbose) PrintToLog ("SONAR parameters message received");
                        SonarParametersMessageHandler (msgBytes);
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

        private void SonarParametersMessageHandler (byte [] msgBytes)
        {
            SonarParametersMsg_Auto msg = new SonarParametersMsg_Auto (msgBytes);

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

            ReadyMsg_Auto rdyMsg = new ReadyMsg_Auto ();
            thisClientSocket.Send (rdyMsg.ToBytes ());
        }

        //***************************************************************************************************************

        // Create a batch of simulated samples

        static Random random = new Random ();

        int LeadingZero = 1000;
        int Ramp        = 100;
        int Level       = 500;

        double ampl = 200;
        int    DC = 512;

        private void CollectMessageHandler (byte [] msgBytes)
        {
            Samples.Clear ();
            samplesGetIndex = 0;

            double time = 0;
            
            for (int i=0; i<LeadingZero; i++, time+=1/SampleRate)
            {
                double withNoiseAndDC = random.NextDouble () + DC;
                Samples.Add (Math.Truncate (withNoiseAndDC));
            }

            for (int i=0; i<Ramp; i++, time+=1/SampleRate)
            {
                double win = (double) i / Ramp;
                double s = win * ampl * Math.Sin (2 * Math.PI * 40000 * time);
                double withNoiseAndDC = random.NextDouble () + DC + s;
                Samples.Add (Math.Truncate (withNoiseAndDC));
            }            
            
            for (int i=0; i<Level; i++, time+=1/SampleRate)
            {
                double win = 1;
                double s = win * ampl * Math.Sin (2 * Math.PI * 40000 * time);
                double withNoiseAndDC = random.NextDouble () + DC + s;
                Samples.Add (Math.Truncate (withNoiseAndDC));
            }            
            
            for (int i=0; i<Ramp; i++, time+=1/SampleRate)
            {
                double win = 1 - (double) i / Ramp;
                double s = win * ampl * Math.Sin (2 * Math.PI * 40000 * time);
                double withNoiseAndDC = random.NextDouble () + DC + s;
                Samples.Add (Math.Truncate (withNoiseAndDC));
            }            

            int rem = BatchSize - Samples.Count;
            
            for (int i=0; i<rem; i++, time+=1/SampleRate)
            {
                double withNoiseAndDC = random.NextDouble () + DC;
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