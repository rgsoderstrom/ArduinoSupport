﻿using System;
using System.Windows;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Media;

using System.Collections.Generic;
using System.Windows.Interop;

using Common;
using ArduinoInterface;
using SocketLibrary;
using Plot2D_Embedded;
using System.Net;
using System.Windows.Controls;
using System.IO;
using System.Reflection.Emit;

namespace A2D_Tests
{
    public partial class ArduinoWindow : Window
    {
        MessageQueue messageQueue; // messages to Arduino pass through here

        System.Timers.Timer KeepAliveTimer = new System.Timers.Timer (20000); // milliseconds

        // only the thread that created WPF objects can access them. others must use Invoke () to
        // run a task on that thread. Its ID stored here
        readonly int WpfThread;

        string clientName = "???"; // for error reporting

        int Verbosity = 1;

        public ArduinoWindow (Socket socket)
        {
            try
            {
                InitializeComponent ();

                messageQueue = new MessageQueue (socket);

                // Create the state object.
                SocketLibrary.StateObject state = new SocketLibrary.StateObject ();
                state.workSocket = socket;

                socket.BeginReceive (state.buffer, 0, SocketLibrary.StateObject.BufferSize, 0, new AsyncCallback (ReceiveCallback), state);

                // only this thread can access WPF objects
                WpfThread = Thread.CurrentThread.ManagedThreadId;

                KeepAliveTimer.Elapsed += KeepAliveTimer_Elapsed;
                KeepAliveTimer.Enabled = true;

                try
                {                
                    var hostEntry = Dns.GetHostEntry (((IPEndPoint) socket.RemoteEndPoint).Address);
                    clientName = hostEntry.HostName;
                }
                catch (Exception )
                {
                    Print ("Failed to find client's name");
                }            
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in ArduinoWindow ctor: {0}", ex.Message));
            }
        }

        //*********************************************************************************************************
        //
        // ReceiveCallback - 
        //    

        static readonly object messageBytesLock = new object ();

        void ReceiveCallback (IAsyncResult ar)
        {
            try
            {
                if (Verbosity > 1)
                    Print ("Message received");

              // Retrieve the state object and the handler socket from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

              // Read data from the socket. 
                int bytesRead = handler.EndReceive (ar);

                if (Verbosity > 2)
                    Print (string.Format ("{0} bytes", bytesRead));

                if (bytesRead == 0)
                {
                   // allClients.Remove (state.workSocket);
                    state.workSocket.Close ();
                    Print ("bytesRead == 0");
                }

                else if (bytesRead > 0)
                {
                    lock (messageBytesLock)
                    {
                        TcpUtils.ExtractMessage (state, bytesRead, SocketMessageHandler);
                    }

                  // get ready for next receive
                    handler.BeginReceive (state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback (ReceiveCallback), state);
                }
            }

            catch (SocketException ex)
            {
                EventLog.WriteLine ("SocketError from " + clientName + ": " + ex.Message);
                Dispatcher.BeginInvoke ((Callback) ShowSocketError);
            }

            catch (Exception ex)
            {
                Print (string.Format ("ReadCallback exception: {0}", ex.Message));
            }
        }

        //*******************************************************************************************************

        void ShowSocketError ()
        {
            //ReadyCommunicateEllipse.Fill = Brushes.Red;
            //InputRcvdEllipse.Fill        = Brushes.White;
            //ResultsReadyEllipse.Fill     = Brushes.White;

            //SendButton.IsEnabled = false;
            //RunButton.IsEnabled = false;
            //GetButton.IsEnabled = false;

            messageQueue.ArduinoNotReady ();
            Print ("Socket error");
        }

        //*******************************************************************************************************

        delegate void MsgProcessingDelegate (object arg1, object arg2);

        void SocketMessageHandler (Socket sender, byte [] messageBytes)
        {
            try
            { 
                object [] args = new object [2];
                args [0] = sender;
                args [1] = messageBytes;

                // this will run MessageProcessing in the main thread, i.e. the one that can
                // access WPF objects
                Dispatcher.BeginInvoke ((MsgProcessingDelegate) MessageProcessing, args);
            }

            catch (Exception ex)
            {
                EventLog.WriteLine ("Exception in SocketMessageHandler: " + ex.Message);
            }
        }

        //*******************************************************************************************************

        void MessageProcessing (object arg1, object arg2)
        {
            try
            {
                Socket sender    = arg1 as Socket;
                byte [] msgBytes = arg2 as byte [];

                if (msgBytes == null)
                {
                    Print ("msgBytes == null");
                    return;
                }

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

        private void KeepAliveTimer_Elapsed (object sender, System.Timers.ElapsedEventArgs e)
        {
            KeepAliveMsg_Auto msg = new KeepAliveMsg_Auto ();

            if (Verbosity > 3)
                Print ("Sending KeepAlive msg, seq numb " + msg.header.SequenceNumber);

            else if (Verbosity > 2)
                Print ("Sending KeepAlive msg");

            messageQueue.AddMessage (msg);
        }

        //*******************************************************************************************************

        static int localLineNumber = 1;
        object LocalTextBoxLock = new object ();

        void AddTextToLocalTextBox (string str)
        {
            EventLog.WriteLine (str);

            lock (LocalTextBoxLock)
            {
                TextDisplay.Text += string.Format ("{0}: ", localLineNumber++);
                TextDisplay.Text += str;
                TextDisplay.Text += "\n";
            }

            TextDisplay.ScrollToEnd ();
        }

        public void Print (string str)
        {
            int callingThread = Thread.CurrentThread.ManagedThreadId;

            //EventLog.WriteLine ("Print: " + str + ", calling thread " + callingThread.ToString () + ", WPF Thread " + WpfThread);

            if (callingThread == WpfThread)
            {
                AddTextToLocalTextBox (str);
            }
            else
            {
                Dispatcher.BeginInvoke ((PrintCallback)AddTextToLocalTextBox, str);
            }
        }

        private void ArduinoWindow_Loaded (object sender, RoutedEventArgs e)
        {
            EventLog.WriteLine ("Arduino Window Loaded");
            int number = 0;

            bool success = ConvertTagToInteger ((Verbosity_ComboBox.SelectedItem as ComboBoxItem).Tag, ref number);

            if (success)
                Verbosity = number;
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
        //*******************************************************************************************************

        //
        // Button-press handlers, cause message to be sent
        //

        StreamWriter samplesFile = null;

        private void ClearButton_Click (object sender, RoutedEventArgs e)
        { 
            try
            { 
                //samplesFile = new StreamWriter ("samples.txt");

                Samples.Clear ();

                ClearMsg_Auto msg = new ClearMsg_Auto ();

                if (Verbosity > 1)
                    Print ("Sending Clear msg, seq numb " + msg.header.SequenceNumber);

                else if (Verbosity > 0)
                    Print ("Sending Clear msg");

                messageQueue.AddMessage (msg);
            }
        
            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in ClearButton click: {0}", ex.Message));
            }
        }

        private void CollectButton_Click (object sender, RoutedEventArgs e)
        {
            try
            { 
                CollectMsg_Auto msg = new CollectMsg_Auto ();

                if (Verbosity > 1)
                    Print ("Sending Collect msg, seq numb " + msg.header.SequenceNumber);

                else if (Verbosity > 0)
                    Print ("Sending Collect msg");

                Samples.Clear ();

                messageQueue.AddMessage (msg);
            }
        
            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in CollectButton click: {0}", ex.Message));
            }
        }

        //*****************************************************************************************

        private void SendButton_Click (object sender, RoutedEventArgs e)
        {
            if (Verbosity > 0)
                Print ("Send button clicked");

            sendMsgCounter = 1;
            RequestSamples ();
        }

        private void RequestSamples ()
        {
            try
            { 
                SendMsg_Auto msg = new SendMsg_Auto ();

                if (Verbosity > 1)
                    Print ("Sending Send msg " + sendMsgCounter + " seq number " + msg.header.SequenceNumber);

                else if (Verbosity > 0)
                    Print ("Sending Send msg");

                messageQueue.AddMessage (msg);
            }
        
            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in SendButton click: {0}", ex.Message));
            }
        }

        //*****************************************************************************************

        private void Resend_Click (object sender, RoutedEventArgs e)
        {
            messageQueue.ResendLastMsg ();
        }

        private bool ConvertTagToInteger (object tag, ref int results)
        {
            bool success = false;

            try
            {
                string s = tag as string;
                results = Int32.Parse (s);
                success = true;
            }

            catch (Exception ex)
            {
                EventLog.WriteLine ("Error converting tag to integer: " + ex.Message);
            }

            return success;
        }

        private void Verbosity_ComboBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            ComboBoxItem cbi = cb.SelectedItem as ComboBoxItem;
            int number = 0;

            bool success = ConvertTagToInteger (cbi.Tag, ref number);

            if (success)
                Verbosity = number;
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
        //*******************************************************************************************************        
        //
        // Received-message handlers for application-specific messages
        //

        List<Point> Samples = new List<Point> ();
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
                    Samples.Add (new Point (x + i, msg.data.Sample [i]));

                    if (samplesFile != null)
                    {
                        samplesFile.WriteLine ((x + i).ToString () + ", " + msg.data.Sample [i].ToString () + " ; ...");
                    }
                }

                if (Verbosity > 2)
                    Print ("Sample msg received" + Samples.Count.ToString () + " total samples received" + " seq = " + msg.header.SequenceNumber);

                else if (Verbosity > 1)
                    Print ("Sample msg received" + Samples.Count.ToString () + " total samples received");

                else if (Verbosity > 0)
                    Print ("Sample msg received");

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

        private void AllSentMessageHandler (byte [] msgBytes)
        {
            try
            { 
                AllSentMsg_Auto msg = new AllSentMsg_Auto (msgBytes);

                if (Verbosity > 1)
                    Print ("Received AllSent msg " + sendMsgCounter + " seq number " + msg.header.SequenceNumber);

                else if (Verbosity > 0)
                    Print ("Received AllSent msg");

                PlotArea.Clear ();
                PlotArea.Plot (new LineView (Samples));
                PlotArea.RectangularGridOn = true;

                if (samplesFile != null)
                {
                    samplesFile.Close ();
                    samplesFile = null;
                }
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in SampleDataMsg handler: {0}", ex.Message));
            }
        }

        //*******************************************************************************************************

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

                if (Verbosity > 1)
                    Print ("FPGA Ready message received, seq number " + hdr.SequenceNumber);

                else if (Verbosity > 0)
                    Print ("FPGA Ready message received");
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in ReadyMsg handler: {0}", ex.Message));
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
