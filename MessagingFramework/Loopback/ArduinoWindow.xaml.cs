using System;
using System.Windows;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Media;

using Common;
using ArduinoInterface;
using System.Windows.Controls;
using SocketLibrary;
using System.Windows.Interop;

namespace Loopback
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
            ReadyCommunicateEllipse.Fill = Brushes.Red;
            InputRcvdEllipse.Fill        = Brushes.White;
            ResultsReadyEllipse.Fill     = Brushes.White;

            SendButton.IsEnabled = false;
            RunButton.IsEnabled = false;
            GetButton.IsEnabled = false;

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

                ushort MsgId = BitConverter.ToUInt16 (msgBytes, (int)Marshal.OffsetOf<Header> ("MessageId"));

                switch (MsgId)
                {
                    case (ushort)ArduinoMessageIDs.TextMsgId: TextMessageHandler (msgBytes); break;
                    case (ushort)ArduinoMessageIDs.StatusMsgId: StatusMessageHandler (msgBytes); break;
                    case (ushort)ArduinoMessageIDs.AcknowledgeMsgId: AcknowledgeMessageHandler (msgBytes); break;
                    case (ushort)ArduinoMessageIDs.LoopbackDataMsgId: LoopbackMessageHandler (msgBytes); break;

                    default: Print ("Unrecognized message ID: " + MsgId.ToString ()); break;
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
            KeepAliveMsg msg = new KeepAliveMsg ();
            messageQueue.AddMessage (msg.ToBytes ());
        }

        //*******************************************************************************************************

        private void TextMessageHandler (byte [] msgBytes)
        {
            TextMessage msg = new TextMessage (msgBytes);
            Print ("Text received: " + msg.Text);
        }

        //*******************************************************************************************************

        private void StatusMessageHandler (byte [] msgBytes)
        {
            StatusMessage msg = new StatusMessage (msgBytes);
            ArduinoNameTextBox.Text = msg.Name;
            clientName = msg.Name;

            SendButton.IsEnabled = true;
            RunButton.IsEnabled = msg.DataReceived;
            GetButton.IsEnabled = msg.DataReady;

            ReadyCommunicateEllipse.Fill = Brushes.Green;
            InputRcvdEllipse.Fill        = msg.DataReceived ? Brushes.Green : Brushes.White;
            ResultsReadyEllipse.Fill     = msg.DataReady ? Brushes.Green : Brushes.White;

            messageQueue.ArduinoReady ();
        }

        //*******************************************************************************************************

        private void AcknowledgeMessageHandler (byte [] msgBytes)
        {
            AcknowledgeMessage msg = new AcknowledgeMessage (msgBytes);

            bool found = messageQueue.MessageAcknowledged (msg.data.MsgSequenceNumber);

            if (found == false)
                Print ("Ack'd message not found: " + msg.data.MsgSequenceNumber.ToString ());
        }

        //*******************************************************************************************************

        private void LoopbackMessageHandler (byte [] msgBytes)
        {
            LoopbackDataMessage msg = new LoopbackDataMessage (msgBytes);
            Print ("Loopback data: " + msg.ToString ());
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

            if (callingThread == WpfThread)
            {
                AddTextToLocalTextBox (str);
            }
            else
            {
                Dispatcher.BeginInvoke ((PrintCallback)AddTextToLocalTextBox, str);
            }
        }

        //***************************************************************************************

        private void SendButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                EventLog.WriteLine ("send button");

                LoopbackDataMessage msg = new LoopbackDataMessage ();

                for (int i = 0; i<32; i++)
                    msg.Put ((byte)(i & 0xf));

                msg.Source = 100;

                messageQueue.AddMessage (msg.ToBytes ());
            }

            catch (Exception ex)
            {
                Print ("Ex: " + ex.Message);
            }
        }

        private void RunButton_Click (object sender, RoutedEventArgs e)
        {
            try
            { 
                EventLog.WriteLine ("run button");
                RunLoopbackTestMsg msg = new RunLoopbackTestMsg ();
                messageQueue.AddMessage (msg.ToBytes ());
            }

            catch (Exception ex)
            {
                Print ("Ex: " + ex.Message);
            }
        }

        private void GetButton_Click (object sender, RoutedEventArgs e)
        {
            try
            { 
                EventLog.WriteLine ("get button");
                SendLoopbackTestResultsMsg msg = new SendLoopbackTestResultsMsg ();
                messageQueue.AddMessage (msg.ToBytes ());
            }

            catch (Exception ex)
            {
                Print ("Ex: " + ex.Message);
            }
        }
    }
}
