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
            DataAvailableEllipse.Fill = Brushes.Green;
            Print ("Sensor sampling complete");
            //SendButton.IsEnabled = true;
            //CollectButton.IsEnabled = true; 
            SendButton_Click (null, new RoutedEventArgs ());
        }


        private void LeftClick (object sender, Point pt)
        {
           // Print (string.Format ("Pointer at {0:0.00}", pt));
            Print (string.Format ("Angle {0:0.0}, Pressure {1:0.0}", pt.X, pt.Y));
        }

        private void SensorDataMessageHandler (byte [] msgBytes)
        {
            //
            // Extract received data
            //
            SensorDataMsg_Auto msg = new SensorDataMsg_Auto (msgBytes);

            Print ("Sensor data message received, " + msg.data.Count + " samples");

            if (msg.data.Count < 2 && ReceivedTime.Count == 0)
                return;

            for (int i=0; i<msg.data.Count; i++)
            { 
                ReceivedTime    .Add (msg.data.Time [i]);
                ReceivedPressure.Add (msg.data.Pressure [i]);
                ReceivedAngle   .Add (msg.data.Angle [i]);
            }

            //
            // if (this message was full) request more
            //
            if (msg.data.Count == SensorDataMsg_Auto.Data.MaxCount)
            { 
                SendSamplesMsg_Auto msg2 = new SendSamplesMsg_Auto ();
                messageQueue.AddMessage (msg2);

                Print ("Queueing another SendSamples msg " + msg2.SequenceNumber);
            }
            else // plot all received data
            {
                CollectButton.IsEnabled = true;                
                SaveButton.IsEnabled = true;
                ClearButton.IsEnabled = true;

                DataAvailableEllipse.Fill = Brushes.White;

                List<Point> pts = new List<Point> ();

                for (int i=0; i<ReceivedAngle.Count; i++)
                {
                    Point pt = new Point (ReceivedAngle [i], ReceivedPressure [i]); // pressure as function of angle
                    //Point pt = new Point (ReceivedTime [i], ReceivedAngle [i]); // angle as function of time
                    pts.Add (pt);
                }

                PlotArea.Clear ();
                PlotArea.RectangularGridOn = true;

                //
                // plot pressure vs angle
                //
                if (pts.Count > 1)
                {
                    LineView lv = new LineView (pts);
                    PlotArea.Plot (lv);
                    PlotArea.XAxisLabel = "Angle, degrees";
                    PlotArea.YAxisLabel = "Pressure";

                    lv.RegisterForMouseLeftClick (LeftClick);
                }

                //
                // Mark the plot with 1-second time marks
                //
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

                if (timeTics.Count > 1) // should always be true
                { 
                    PointView pv = new PointView (timeTics);
                    pv.Size = 2;
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
                Print ("Ready message received");
             //   CollectButton.IsEnabled = true;                
                messageQueue.ArduinoReady = true;
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in ReadyMessageHandler: {0}", ex.Message));
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
                EventLog.WriteLine (string.Format ("Exception in TextMessageHandler: {0}", ex.Message));
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
                EventLog.WriteLine (string.Format ("Exception in AcknowledgeMessageHandler: {0}", ex.Message));
            }
        }
    }
}
