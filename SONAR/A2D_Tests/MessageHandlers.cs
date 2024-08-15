using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using ArduinoInterface;
using Plot2D_Embedded;
using SocketLibrary;
using Common;
using System.Windows.Media;

namespace A2D_Tests
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
                    case (ushort)ArduinoMessageIDs.AllSentMsgId:    AllSentMessageHandler    (msgBytes); break;

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
                CollectButton.IsEnabled = true;
                SendButton.IsEnabled = true;

                ReadyEllipse.Fill = Brushes.Green;
                messageQueue.ArduinoReady ();

                SocketLibrary.MessageHeader hdr = new MessageHeader (msgBytes);

                if (Verbosity > 1)      Print ("FPGA Ready message received, seq number " + hdr.SequenceNumber);
                else if (Verbosity > 0) Print ("FPGA Ready message received");
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in ReadyMsg handler: {0}", ex.Message));
            }
        }

        //*******************************************************************************************************

        List<double> Samples = new List<double> ();
        int ExpectedBatchSize = 1024;

        int sendMsgCounter = 0;

        private void SampleDataMessageHandler (byte [] msgBytes)
        {
            try
            { 
                int x = Samples.Count;

                SampleDataMsg_Auto msg = new SampleDataMsg_Auto (msgBytes);

                for (int i=0; i<SampleDataMsg_Auto.Data.MaxCount; i++)
                {
                    Samples.Add (msg.data.Sample [i]);
                }

                if (Verbosity > 2)      Print ("Sample msg received" + Samples.Count.ToString () + " total samples received" + " seq = " + msg.header.SequenceNumber);
                else if (Verbosity > 1) Print ("Sample msg received" + Samples.Count.ToString () + " total samples received");
                else if (Verbosity > 0) Print ("Sample msg received");

                if (Samples.Count < ExpectedBatchSize)
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

        readonly bool WriteSamplesFile = false;
        int fileCounter = 1;

        SignalProcessing signalProcessor;

        private void AllSentMessageHandler (byte [] msgBytes)
        {
            try
            { 
                AllSentMsg_Auto msg = new AllSentMsg_Auto (msgBytes);

                if (Verbosity > 1)      Print ("Received AllSent msg " + sendMsgCounter + " seq number " + msg.header.SequenceNumber);
                else if (Verbosity > 0) Print ("Received AllSent msg");

                signalProcessor = new SignalProcessing (Samples, 100000);

                PlotArea.Clear ();

                if (SelectedDisplay == DisplayOptions.InputSamples)  PlotArea.Plot (new LineView (signalProcessor.InputSamples));
                if (SelectedDisplay == DisplayOptions.InputSpectrum) PlotArea.Plot (new LineView (signalProcessor.InputSpectrum));

                PlotArea.RectangularGridOn = true;

                if (WriteSamplesFile)
                {
                    string fileName = "samples" + fileCounter++ + ".m";
                    StreamWriter samplesFile = new StreamWriter (fileName);
                    samplesFile.WriteLine ("z = [...");
                    
                    for (int i=0; i<Samples.Count-1; i++)
                        samplesFile.WriteLine (Samples [i].ToString () + " ; ...");

                    samplesFile.WriteLine (Samples [Samples.Count-1].ToString () + "];");
                    samplesFile.Close ();
                }
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
