using System;
using System.Windows;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using ArduinoInterface;
using SocketLibrary;
using Common;
using System.Windows.Media;
using System.Windows.Documents;
using Plot2D_Embedded;

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0052 // Remove unread private members

namespace PioneerSensors
{
    public partial class ArduinoWindow
    {
        //*******************************************************************************************************
        //
        // Come here to process received messages
        //

        void MessageProcessing (object arg1, object arg2)
        {
            try
            {
                Socket sender    = arg1 as Socket;
                byte [] msgBytes = arg2 as byte [];

                ushort MsgId = BitConverter.ToUInt16 (msgBytes, (int)Marshal.OffsetOf<MessageHeader> ("MessageId"));

                switch (MsgId)
                {
                    case (ushort)ArduinoMessageIDs.ReadyMsgId:       ReadyMessageHandler       (msgBytes); break;
                    case (ushort)ArduinoMessageIDs.AcknowledgeMsgId: AcknowledgeMessageHandler (msgBytes); break;
                    case (ushort)ArduinoMessageIDs.TextMsgId:        TextMessageHandler        (msgBytes); break;

                    case (ushort)ArduinoMessageIDs.DoneSamplingMsgId: DoneSamplingMessageHandler (msgBytes); break;
                    case (ushort)ArduinoMessageIDs.SensorDataMsgId:   SensorDataMessageHandler   (msgBytes); break;

                    default: Print ("Unrecognized message ID: " + MsgId.ToString ());  break;
                }
            }

            catch (Exception ex)
            {
                Print (String.Format ("MessageProcessing Exception: {0}", ex.Message));
                Print (String.Format ("MessageProcessing Exception: {0}", ex.StackTrace));
            }
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
        //*******************************************************************************************************
        //
        // Handlers for application-specific messages
        //
        readonly List<double> ReceivedTime     = new List<double> ();
        readonly List<double> ReceivedPressure = new List<double> ();
        readonly List<double> ReceivedAngle    = new List<double> ();

        private void DoneSamplingMessageHandler (byte [] msgBytes)
        {
            SendButton.IsEnabled = true;
            DataAvailableEllipse.Fill = Brushes.Green;
            Print ("Sensor sampling complete");
        }

        private void SensorDataMessageHandler (byte [] msgBytes)
        {
            Print ("Sensor data message received");
            SensorDataMsg_Auto msg = new SensorDataMsg_Auto (msgBytes);

            //for (int i=0; i<msg.data.Count && i<3; i++)
            //{ 
            //    string str1 = String.Format ("T = {0:x} P = {1:x} A = {2:x} ", msg.data.Time [i], msg.data.Pressure [i], msg.data.Angle [i]);
            //    Print (str1);
            //}

            for (int i=0; i<msg.data.Count; i++)
            { 
                ReceivedTime    .Add (msg.data.Time [i]);
                ReceivedPressure.Add (msg.data.Pressure [i]);
                ReceivedAngle   .Add (msg.data.Angle [i]);
            }

            if (msg.data.Count == SensorDataMsg_Auto.Data.MaxCount)
            { 
                SendSamplesMsg_Auto msg2 = new SendSamplesMsg_Auto ();
                messageQueue.AddMessage (msg2);
            }
            else
            {
                List<Point> pts = new List<Point> ();

                for (int i=0; i<ReceivedAngle.Count; i++)
                {
                    Point pt = new Point (ReceivedAngle [i], ReceivedPressure [i]);
                    pts.Add (pt);
                }

                PlotArea.Clear ();
                PlotArea.RectangularGridOn = true;

                if (pts.Count > 1)
                { 
                    LineView lv = new LineView (pts);
                    PlotArea.Plot (lv);
                }

                List<Point> timeTics = new List<Point> ();
                double t0 = ReceivedTime [0];

                for (int i=0; i<ReceivedTime.Count; i++)
                {
                    if (ReceivedTime [i] > t0 + 1000)
                    {
                        timeTics.Add (new Point (ReceivedAngle [i], ReceivedPressure [i]));
                        t0 = ReceivedTime [i];
                    }
                }

                if (timeTics.Count > 1)
                { 
                    PointView pv = new PointView (timeTics);
                    pv.Size = 3;
                    PlotArea.Plot (pv);
                }
            }
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
        //*******************************************************************************************************
        //
        // Messages common to most Arduino apps
        //
        private void ReadyMessageHandler (byte [] msgBytes)
        {
            try
            { 
                BeginButton.IsEnabled = true;
                ReadyEllipse.Fill = Brushes.Green;
                messageQueue.ArduinoReady ();
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in ReadyMsg handler: {0}", ex.Message));
            }
        }

        private void TextMessageHandler (byte [] msgBytes)
        {
            try
            { 
                TextMessage msg = new TextMessage (msgBytes);
                Print ("Text received: " + msg.Text.TrimEnd (new char [] {'\0'}));
            }
        
            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in TextMsg handler: {0}", ex.Message));
            }
        }

        private void AcknowledgeMessageHandler (byte [] msgBytes)
        {
            try
            { 
                AcknowledgeMsg_Auto msg = new AcknowledgeMsg_Auto (msgBytes);

                bool found = messageQueue.MessageAcknowledged (msg.data.MsgSequenceNumber);

                if (found == false)
                    Print ("Ack'd message not found: " + msg.data.MsgSequenceNumber.ToString ());

                if (Verbosity > 1)
                    Print ("Arduino Acknowledged " + msg.data.MsgSequenceNumber);
            }
        
            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in AckMsg handler: {0}", ex.Message));
            }
        }
    }
}
