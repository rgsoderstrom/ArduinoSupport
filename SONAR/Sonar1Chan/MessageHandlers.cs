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
                    case (ushort)ArduinoMessageIDs.PingReturnDataMsgId: SampleDataMessageHandler (msgBytes); break;

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
                //ClearButton.IsEnabled = true;
                PingButton.IsEnabled = true;
                SendButton.IsEnabled = true;

                ReadyEllipse.Fill = Brushes.Green;
                messageQueue.ArduinoReady ();

                SocketLibrary.MessageHeader hdr = new MessageHeader (msgBytes);

                if (Verbosity > 1) Print ("FPGA Ready message received, seq number " + hdr.SequenceNumber);
                else if (Verbosity > 0) Print ("FPGA Ready message received");
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in ReadyMsg handler: {0}", ex.Message));
            }
        }

        //*******************************************************************************************************

        List<double> Samples = new List<double> ();

        double TimeTag = 0; // initialized to BlankingTime when user requests sample

        int sendMsgCounter = 0; // number of sample request messages sent, just for status display

        private void SampleDataMessageHandler (byte [] msgBytes)
        {
            double SampleTime = 1 / SampleRate;

            try
            { 
                SampleDataMsg_Auto msg = new SampleDataMsg_Auto (msgBytes);

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

      SignalProcessing signalProcessor;

        private void DisplaySamples ()
        {
            try
            {
                signalProcessor = new SignalProcessing (Samples, PingFrequency, SampleRate, PingDuration);


                SaveButton.IsEnabled = true;

                //PlotArea.Clear ();

                PlotArea.Plot (new LineView (signalProcessor.InputSamples));

                LineView lv = new LineView (signalProcessor.Magnitude);
                lv.Color = Brushes.Red;
                PlotArea.Plot (lv);

                //PlotArea.Plot (new LineView (signalProcessor.MedianFiltered));

                //if (SelectedDisplay == DisplayOptions.InputSamples)    PlotArea.Plot (new LineView (signalProcessor.InputSamples));
                //if (SelectedDisplay == DisplayOptions.AbsInputSamples) PlotArea.Plot (new LineView (signalProcessor.AbsoluteValue));
                //if (SelectedDisplay == DisplayOptions.MedianFiltered)  PlotArea.Plot (new LineView (signalProcessor.MedianFiltered));

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
