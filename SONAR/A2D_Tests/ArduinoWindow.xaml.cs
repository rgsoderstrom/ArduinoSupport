using System;
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

                messageQueue.ArduinoReady (); //************************************

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
              //PrintHandler?.Invoke ("Message received");

              // Retrieve the state object and the handler socket from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

              // Read data from the socket. 
                int bytesRead = handler.EndReceive (ar);

                //PrintHandler?.Invoke (string.Format ("{0} bytes", bytesRead));

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
            object [] args = new object [2];
            args [0] = sender;
            args [1] = messageBytes;

            // this will run MessageProcessing in the main thread, i.e. the one that can
            // access WPF objects
            Dispatcher.BeginInvoke ((MsgProcessingDelegate) MessageProcessing, args);
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
            messageQueue.AddMessage (msg.ToBytes ());
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
            samplesFile = new StreamWriter ("samples.txt");

            Samples.Clear ();

            ClearMsg_Auto msg = new ClearMsg_Auto ();
            messageQueue.AddMessage (msg.ToBytes ());

            Print ("Sending Clear msg");
        }

        private void CollectButton_Click (object sender, RoutedEventArgs e)
        {
            Samples.Clear ();

            CollectMsg_Auto msg = new CollectMsg_Auto ();
            messageQueue.AddMessage (msg.ToBytes ());

            Print ("Sending Collect msg");
        }

        private void SendButton_Click (object sender, RoutedEventArgs e)
        {
            SendMsg_Auto msg = new SendMsg_Auto ();
            messageQueue.AddMessage (msg.ToBytes ());

            Print ("Sending Send msg");
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
        //*******************************************************************************************************        
        //
        // Received-message handlers for application-specific messages
        //

        List<Point> Samples = new List<Point> ();
        int ExpectedBatchSize = 1024;

        private void SampleDataMessageHandler (byte [] msgBytes)
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

            Print (Samples.Count.ToString () + " total samples received"); // , seq = " + msg.header.SequenceNumber);

            if (Samples.Count < ExpectedBatchSize)
            {
                SendButton_Click (null, null);
            }
            else
            {
                PlotArea.Clear ();
                PlotArea.Plot (new LineView (Samples));
                PlotArea.RectangularGridOn = true;
            }
        }

        //*******************************************************************************************************

        private void AllSentMessageHandler (byte [] msgBytes)
        {
            //PlotArea.Clear ();
            //PlotArea.Plot (new LineView (Samples));
            //PlotArea.RectangularGridOn = true;

            //SocketLibrary.MessageHeader hdr = new MessageHeader (msgBytes);
            Print ("All Sent message received "); // + hdr.SequenceNumber);

            if (samplesFile != null)
            {
                samplesFile.Close ();
                samplesFile = null;
            }
        }

        //*******************************************************************************************************

        private void ReadyMessageHandler (byte [] msgBytes)
        {
            ClearButton.IsEnabled = true;
            CollectButton.IsEnabled = true;
            SendButton.IsEnabled = true;

            ReadyEllipse.Fill = Brushes.Green;
            messageQueue.ArduinoReady ();

            //SocketLibrary.MessageHeader hdr = new MessageHeader (msgBytes);
            Print ("FPGA Ready message received "); // + hdr.SequenceNumber);
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
        //*******************************************************************************************************
        //
        // Messages common to most Arduino apps
        //

        private void TextMessageHandler (byte [] msgBytes)
        {
            TextMessage msg = new TextMessage (msgBytes);
            Print ("Text from Arduino: " + msg.Text.TrimEnd (new char [] {'\0'}));

            //Print ("Text " + msg.header.SequenceNumber);
        }

        private void AcknowledgeMessageHandler (byte [] msgBytes)
        {
            AcknowledgeMsg_Auto msg = new AcknowledgeMsg_Auto (msgBytes);

            bool found = messageQueue.MessageAcknowledged (msg.data.MsgSequenceNumber);

            if (found == false)
                Print ("Ack'd message not found: " + msg.data.MsgSequenceNumber.ToString ());

            //Print ("AckMsg " + msg.header.SequenceNumber);
        }
    }
}
