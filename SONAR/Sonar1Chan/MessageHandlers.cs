using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using ArduinoInterface;
using Plot2D_Embedded;
using SocketLibrary;
using Common;
using System.Windows.Media;

namespace Sonar1Chan
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
                    case (ushort)ArduinoMessageIDs.ReadyMsgId:             ReadyMessageHandler        (msgBytes); break;
                    case (ushort)ArduinoMessageIDs.PingReturnRawDataMsgId: SampleDataMessageHandler   (msgBytes); break;
                    case (ushort)ArduinoMessageIDs.PingReturnMfDataMsgId:  MfSampleDataMessageHandler (msgBytes); break;

                    case (ushort)ArduinoMessageIDs.AcknowledgeMsgId: AcknowledgeMessageHandler (msgBytes); break;
                    case (ushort)ArduinoMessageIDs.TextMsgId:        TextMessageHandler        (msgBytes); break;

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

        private List<double> Samples = new List<double> ();
        private readonly int PeakPickWindow = 16;

        private List<double>  MatchedFilterPing = new List<double> ();
        private SampleHistory MatchedFilterHistory = new SampleHistory (6);


        private void MfSampleDataMessageHandler (byte [] msgBytes)
        {
            try
            { 
                PingReturnMfDataMsg_Auto msg = new PingReturnMfDataMsg_Auto (msgBytes);

                int samplesThisMsg = msg.data.Count;
                bool lastSamples   = msg.data.Count < PingReturnMfDataMsg_Auto.Data.MaxCount;

                //  MatchedFilterHistory.Add (msg.data.Sample, msg.data.Count);

                for (int i = 0; i<samplesThisMsg; i++)
                {
                    //double range = TimeTag * SoundSpeed / 2;
                    //Samples.Add (new Point (range, msg.data.Sample [i]));
                    //TimeTag += SampleTime;
                    MatchedFilterPing.Add (msg.data.Sample [i]);
                }

                if (Verbosity > 2)      Print ("MF msg received, " + msg.data.Count.ToString () + " samples this message, seq = " + msg.header.SequenceNumber);
                else if (Verbosity > 1) Print ("MF msg received, " + msg.data.Count.ToString () + " samples this message");
                else if (Verbosity > 0) Print ("MF msg received");

                if (lastSamples)
                {
                    MatchedFilterHistory.Add (MatchedFilterPing);
                    MatchedFilterPing = new List<double> ();
                    DisplayMatchedFilter ();
                 //   PingButton.IsEnabled = true;
                }
                else
                {
                    RequestMfSamples ();
                }

                messageQueue.ArduinoReady = true; // tells message queue to send next message
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in SampleDataMsg handler: {0}", ex.Message));
            }
        }

        private void SampleDataMessageHandler (byte [] msgBytes)
        {
            double SampleTime = 1 / SampleRate;

            try
            { 
                PingReturnRawDataMsg_Auto msg = new PingReturnRawDataMsg_Auto (msgBytes);

                int samplesThisMsg = msg.data.Count;
                bool lastSamples   = msg.data.Count < SampleDataMsg_Auto.Data.MaxCount;

                for (int i=0; i<samplesThisMsg; i++)
                {
                    //double range = TimeTag * SoundSpeed / 2;
                    //Samples.Add (new Point (range, msg.data.Sample [i]));
                    //TimeTag += SampleTime;
                    Samples.Add (msg.data.Sample [i]);
                }

                if (Verbosity > 2)      Print ("Sample msg received, " + msg.data.Count.ToString () + " samples this message, seq = " + msg.header.SequenceNumber);
                else if (Verbosity > 1) Print ("Sample msg received, " + msg.data.Count.ToString () + " samples this message");
                else if (Verbosity > 0) Print ("Sample msg received");

                if (lastSamples)
                {
                    DisplaySamples ();
                 //   PingButton.IsEnabled = true;
                }
                else
                {
                    RequestRawSamples ();
                }

                messageQueue.ArduinoReady = true; // tells message queue to send next message
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in SampleDataMsg handler: {0}", ex.Message));
            }
        }

        //*******************************************************************************************************

       SignalProcessing2 signalProcessor;
       //SignalProcessing  signalProcessor;

        private void DisplaySamples ()
        {
            try
            {
                signalProcessor = new SignalProcessing2 (Samples, PingFrequency, SampleRate, PingDuration);
              //signalProcessor = new SignalProcessing  (Samples, PingFrequency, SampleRate, PingDuration);

                SaveButton.IsEnabled = true;

                PlotArea.Plot (new LineView (signalProcessor.InputSamples));

                //PointView pv = new PointView (signalProcessor.Magnitude);//, PointView.DrawingStyle.Star);
                //pv.Size = 0.1;
                //pv.Color = Brushes.Red;



               // LineView lv = new LineView (signalProcessor.Magnitude);
               // lv.Color = Brushes.Red;

               // PlotArea.Plot (lv);
                PlotArea.RectangularGridOn = true;
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in DisplaySamples handler: {0}", ex.Message));
            }
        }

        private void DisplayMatchedFilter ()
        {
            const double SoundSpeed = 1125; // feet per second

            double BlankingTime = 0; // (PingDuration / 1000) + 0.003; // seconds from ping command to first sample

            double SampleTime = 1 / SampleRate;
            double MfTimeStep = SampleTime * PeakPickWindow;

            double timeTag;// = BlankingTime + MfTimeStep / 2;

            try
            {
                List<Point> mfData = new List<Point> ();
                List<double> OB = MatchedFilterHistory.GetNewest ();

                Brush LineColor = Brushes.Red;

                while (OB != null)
                {
                    mfData.Clear ();
                    timeTag = BlankingTime + MfTimeStep / 2;

                    foreach (double d in OB)
                    {
                        double dd = Math.Pow (2, 12 * d / 200.0); 
                        mfData.Add (new Point (timeTag * SoundSpeed / 2, dd));
                      //mfData.Add (new Point (timeTag * SoundSpeed / 2, d));
                        timeTag += MfTimeStep;
                    }

                    if (mfData.Count > 1)
                    { 
                        LineView lv = new LineView (mfData);
                        lv.Color = LineColor;
                        PlotArea.Plot (lv);
                    }

                    LineColor = Brushes.Pink;
                    OB = MatchedFilterHistory.GetNext ();
                }

                PlotArea.RectangularGridOn = true;
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in DisplaySamples handler: {0}", ex.Message));
            }
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
        //*******************************************************************************************************
        //
        // Messages common to most Arduino apps
        //

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

        private void ReadyMessageHandler (byte [] msgBytes)
        {
            try
            {
                //PingButton.IsEnabled = true;
                messageQueue.ArduinoReady = true;

                if (Verbosity > 1)
                    Print ("Arduino reports ready");
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in ReadyMsg handler: {0}", ex.Message));
            }
        }

    }
}
