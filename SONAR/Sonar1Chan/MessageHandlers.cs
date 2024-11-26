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
                    case (ushort)ArduinoMessageIDs.ReadyMsgId:      ReadyMessageHandler      (msgBytes); break;
                    case (ushort)ArduinoMessageIDs.SampleDataMsgId: SampleDataMessageHandler (msgBytes); break;

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

        private void ReadyMessageHandler (byte [] msgBytes)
        {
            try
            {
                ClearButton.IsEnabled = true;
                PingButton.IsEnabled = true;
                SendButton.IsEnabled = true;

                ReadyEllipse.Fill = Brushes.Green;
                messageQueue.ArduinoReady ();

                SocketLibrary.MessageHeader hdr = new MessageHeader (msgBytes);

                if (SelectedVerbosity > 1) Print ("FPGA Ready message received, seq number " + hdr.SequenceNumber);
                else if (SelectedVerbosity > 0) Print ("FPGA Ready message received");
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in ReadyMsg handler: {0}", ex.Message));
            }
        }

        //*******************************************************************************************************

        const double SoundSpeed = 1125; // feet per second
        double BlankingTime = 0.005; // seconds from ping command to first sample
        double SampleTime = 1 / 100_000.0;

        List<Point> Samples = new List<Point> ();
        double TimeTag = 0; // set to BlankingTime when user requests sample

        int sendMsgCounter = 0; // number of sample request messages sent, just for status display

        private void SampleDataMessageHandler (byte [] msgBytes)
        {
            try
            { 
                SampleDataMsg_Auto msg = new SampleDataMsg_Auto (msgBytes);

                int samplesThisMsg = msg.data.Count;
                bool lastSamples   = msg.data.Count < SampleDataMsg_Auto.Data.MaxCount;

                for (int i=0; i<samplesThisMsg; i++)
                {
                    double range = TimeTag * SoundSpeed / 2;
                    Samples.Add (new Point (range, msg.data.Sample [i]));
                    TimeTag += SampleTime;
                }

                if (SelectedVerbosity > 2)      Print ("Sample msg received, " + msg.data.Count.ToString () + " samples this message, seq = " + msg.header.SequenceNumber);
                else if (SelectedVerbosity > 1) Print ("Sample msg received, " + msg.data.Count.ToString () + " samples this message");
                else if (SelectedVerbosity > 0) Print ("Sample msg received");

                if (lastSamples)
                {
                    DisplaySamples ();
                }
                else
                {
                    RequestSamples ();
                }
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in SampleDataMsg handler: {0}", ex.Message));
            }
        }

        //*******************************************************************************************************

      //SignalProcessing signalProcessor;

        private void DisplaySamples ()
        {
            try
            { 
                PlotArea.Plot (new LineView (Samples));

                //if (Verbosity > 1)      Print ("Received AllSent msg " + sendMsgCounter + " seq number " + msg.header.SequenceNumber);
                //else if (Verbosity > 0) Print ("Received AllSent msg");

                //signalProcessor = new SignalProcessing (Samples, SampleRate);
                //SaveButton.IsEnabled = true;
                //PeaksButton.IsEnabled = true;

                //PlotArea.Clear ();

                //if (SelectedDisplay == DisplayOptions.InputSamples)  PlotArea.Plot (new LineView (signalProcessor.InputSamples));
                //if (SelectedDisplay == DisplayOptions.InputSpectrum) PlotArea.Plot (new LineView (signalProcessor.InputSpectrum));
                //if (SelectedDisplay == DisplayOptions.WindowedSamples)  PlotArea.Plot (new LineView (signalProcessor.WindowedSamples));
                //if (SelectedDisplay == DisplayOptions.WindowedSpectrum) PlotArea.Plot (new LineView (signalProcessor.WindowedSpectrum));

                PlotArea.RectangularGridOn = true;
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in AllSentMsg handler: {0}", ex.Message));
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

                //Print ("Text " + msg.header.SequenceNumber);
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

                if (SelectedVerbosity > 1)
                    Print ("Arduino Acknowledged " + msg.data.MsgSequenceNumber);
            }
        
            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in AckMsg handler: {0}", ex.Message));
            }
        }

    }
}
