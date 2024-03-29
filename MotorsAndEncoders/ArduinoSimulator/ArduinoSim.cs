﻿using System;
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

        //DriveWheelEncoders encoders = new DriveWheelEncoders ();

        StatusMessage.StatusData statusData;

        //****************************************************************************

        public ArduinoSim ()
        {
        }

        //****************************************************************************

        public void Run ()
        {
            try
            {
                statusData = new StatusMessage.StatusData ();
                statusData.Name = "ArdSim";

                Console.WriteLine ("Connecting to server");
                thisClientSocket = new SocketLib.TcpClient (PrintToLog); // (PrintToConsole);
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception in Main: " + ex.Message);
            }

            if (thisClientSocket.Connected == false)
            {
                Console.WriteLine ("\n\nFailed to connect to server");

                while (true)
                    Thread.Sleep (1000);
            }

            thisClientSocket.MessageHandler += MessageHandler;
            thisClientSocket.PrintHandler   += PrintToLog; // PrintToConsole;


            statusData.readyForMessages = 1;
            StatusMessage msg = new StatusMessage (statusData);
            thisClientSocket.Send (msg.ToBytes ());

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
                case (ushort)CommandMessageIDs.SendCounts:
                    SendEncoderCounts ();
                    break;

                case (ushort) CommandMessageIDs.SpeedProfileSegment:
                    {
                        SpeedProfileSegmentMsg rcvd = new SpeedProfileSegmentMsg (msgBytes);
                        //encoders.AddProfileSegment (rcvd.data);
                        Console.Write ("ID = " + rcvd.data.motorID + ", ");
                        Console.Write ("index = " + rcvd.data.index + ", ");
                        Console.Write ("speed = " + rcvd.data.speed + ", ");
                        Console.Write ("dur = " + rcvd.data.duration + "\n");
                        statusData.readyToRun = 1;
                        thisClientSocket.Send (new StatusMessage (statusData).ToBytes ());
                    }
                    break;

                case (ushort)CommandMessageIDs.ClearSpeedProfile:
                    Console.WriteLine ("Clear Profile");
                    //encoders.ClearSpeedProfile ();
                    statusData.readyToRun = 0;
                    thisClientSocket.Send (new StatusMessage (statusData).ToBytes ());
                    break;

                case (ushort)CommandMessageIDs.TransferSpeedProfile:
                    Console.WriteLine ("Transfer profile");
                    statusData.readyToRun = 1;
                    thisClientSocket.Send (new StatusMessage (statusData).ToBytes ());
                    break;

                case (ushort) CommandMessageIDs.RunMotors:
                    Console.WriteLine ("Run Motors");
                    statusData.motorsRunning = 1;
                    thisClientSocket.Send (new StatusMessage (statusData).ToBytes ());
                    break;

                case (ushort) CommandMessageIDs.SlowStopMotors:
                    Console.WriteLine ("Slow Stop Motors");
                    statusData.motorsRunning = 0;
                    thisClientSocket.Send (new StatusMessage (statusData).ToBytes ());
                    break;

                case (ushort) CommandMessageIDs.FastStopMotors:
                    Console.WriteLine ("Fast Stop Motors");
                    statusData.motorsRunning = 0;
                    thisClientSocket.Send (new StatusMessage (statusData).ToBytes ());
                    break;

                case (ushort) CommandMessageIDs.KeepAlive:
                    if (Verbose) Console.WriteLine ("Received KeepAlive msg");
                    break;

                //case (ushort) CommandMessageIDs.SendFirstCollection:
                //{
                //    EncoderCountsMessage.Batch batch = encoders.GetFirstSampleBatch ();
                //    thisClientSocket.Send (new EncoderCountsMessage (batch).ToBytes ());
                //}
                //break;

                //case (ushort) CommandMessageIDs.SendNextCollection:
                //{
                //    EncoderCountsMessage.Batch batch = encoders.GetNextSampleBatch ();
                //    thisClientSocket.Send (new EncoderCountsMessage (batch).ToBytes ());
                //}
                //break;

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

            Header hdr = new Header (msgBytes);
            AcknowledgeMessage msg = new AcknowledgeMessage (hdr.SequenceNumber);
            thisClientSocket.Send (msg.ToBytes ());
        }

        //****************************************************************************************

        List<byte> encoder1 = new List<byte> ();
        List<byte> encoder2 = new List<byte> ();
        bool firstTime = true;
        int get = 0;

        private void SendEncoderCounts ()
        { 
            if (firstTime)
            {
                firstTime = false;

                for (int i=0; i<100; i++)
                {
                    byte e1 = (byte) (120 * Math.Sin (2 * Math.PI * i * 7 / 100.0));
                    byte e2 = (byte) (100 * Math.Sin (2 * Math.PI * i * 5 / 100.0));
                    encoder1.Add (e1);
                    encoder2.Add (e2);
                }
            }

            EncoderCountsMessage msg = new EncoderCountsMessage ();

            int count = encoder1.Count - get < EncoderCountsMessage.Batch.MaxNumberSamples
                      ? encoder1.Count - get : EncoderCountsMessage.Batch.MaxNumberSamples;

            for (int i = 0; i<count; i++)
            {
                msg.Add (encoder1 [get], encoder2 [get]);
                get++;
            }

            if (get == encoder1.Count)
                msg.IsLastBatch = true;

            thisClientSocket.Send (msg.ToBytes ());
        }
    }
}


