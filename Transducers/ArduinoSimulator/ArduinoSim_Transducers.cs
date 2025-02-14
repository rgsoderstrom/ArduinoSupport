
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using ArduinoInterface;
using SocketLibrary;

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0052 // Remove unread private members

namespace ArduinoSimulator
{
    public class ArduinoSim_Transducers : ArduinoSimBase
    {
        readonly System.Timers.Timer SamplingDelayTimer = new System.Timers.Timer ();

        public ArduinoSim_Transducers (string name, 
                                       SocketLibrary.TcpClient sock,
                                       PrintCallback ptl) : base (name, sock, ptl)
        {            
            thisClientSocket.MessageHandler += MessageHandler;

            SamplingDelayTimer.Elapsed += SamplingTimer_Elapsed;
            SamplingDelayTimer.Enabled = false;
        }

		//**********************************************************************************
		//**********************************************************************************
		//**********************************************************************************
		
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
                    case (ushort)ArduinoMessageIDs.StartSamplingMsgId:
                        if (Verbose) PrintToLog ("Start Sampling message received");
                        StartSamplingMessageHandler (msgBytes);
                        break;

                    case (ushort)ArduinoMessageIDs.SendSamplesMsgId:
                        if (Verbose) PrintToLog ("Send Samples message received");
                        SendSamplesMessageHandler (msgBytes);
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

     // const double SampleTime = 50; // milliseconds between samples
        const double SampleTime = 5;  // sped-up

        const short Count = 234;
        short put = 0;
        short get = 0;

        readonly short [] Pressure = new short  [Count];
        readonly short [] Angle    = new short  [Count];
        readonly short [] Time     = new short  [Count];

        private void SamplingTimer_Elapsed (object sender, System.Timers.ElapsedEventArgs e)
        {
            SamplingDelayTimer.Enabled = false;

            DoneSamplingMsg_Auto msg = new DoneSamplingMsg_Auto ();
            thisClientSocket.Send (msg.ToBytes ());
        }

        private void StartSamplingMessageHandler (byte [] msgBytes)
        {
            SamplingDelayTimer.Interval = SampleTime * Count;
            SamplingDelayTimer.Enabled = true;

            get = 0;

            for (put=0; put<Count; put++)
            {
                Pressure [put] = (short) (100 + put); 
                Angle    [put] = (short) (200 + put); 
                Time     [put] = (short) (put * SampleTime); 
            }
        }

        //***************************************************************************************************************

        private void SendSamplesMessageHandler (byte [] msgBytes)
        {
            int remaining = put - get;
            int sendCount = remaining > SensorDataMsg_Auto.Data.MaxCount ? SensorDataMsg_Auto.Data.MaxCount : remaining;

            SensorDataMsg_Auto msg = new SensorDataMsg_Auto ();

            for (int i=0; i<sendCount; i++, get++)
            { 
                msg.data.Pressure [i] = Pressure [get];
                msg.data.Angle    [i] = Angle [get];
                msg.data.Time     [i] = Time [get];
            }

            msg.data.Count = (short) sendCount;
            thisClientSocket.Send (msg.ToBytes ());
        }
    }
}





