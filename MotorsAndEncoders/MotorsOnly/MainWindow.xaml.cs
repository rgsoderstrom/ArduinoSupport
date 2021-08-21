using System;
using System.Windows;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Windows.Media;

using System.Threading; // sleep
using System.Windows.Threading;
using System.Runtime.InteropServices; // for Marshal

using Common;
using ArduinoInterface;
using Plot2D_Embedded;

namespace MotorsOnly
{
    public partial class MainWindow : Window
    {
        SocketLib.TcpServer ServerSocket = null;

        System.Timers.Timer KeepAliveTimer = new System.Timers.Timer (20000); // milliseconds

        // only the thread that created WPF objects can access them. others must use Invoke () to
        // run a task on that thread. Its ID stored here
        readonly int WpfThread;

        string ScenarioFilePath = @"..\..\";

        public MainWindow ()
        {
            EventLog.Open (@"..\..\Log.txt", true);

            try
            {
                InitializeComponent ();

                // only this thread can access WPF objects
                WpfThread = Thread.CurrentThread.ManagedThreadId;

                ServerSocket = new SocketLib.TcpServer (Print);
                ServerSocket.MessageHandler          += SocketMessageHandler;
                ServerSocket.NewConnectionHandler    += SocketServer_newConnectionHandler;
                ServerSocket.ClosedConnectionHandler += SocketServer_closedConnectionHandler;
                ServerSocket.PrintHandler            += Print;

                KeepAliveTimer.Elapsed += KeepAliveTimer_Elapsed;
                KeepAliveTimer.Enabled = true;

                ScenarioFileName.Text = "Scenario1.txt";
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in MainWindow ctor: {0}", ex.Message));
            }
        }

        private void Window_Loaded (object sender, RoutedEventArgs e)
        {
            PlotArea.MouseEnabled = false;
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
        //*******************************************************************************************************

        delegate void MsgProcessingDelegate (object arg1, object arg2);

        private void SocketMessageHandler (Socket sender, byte [] messageBytes)
        {
            object [] args = new object [2];
            args [0] = sender;
            args [1] = messageBytes;

            // this will run MessageProcessing in the main thread, i.e. the one that can
            // access WPF objects
            Dispatcher.BeginInvoke ((MsgProcessingDelegate) MessageProcessing, args);
        }

        //*******************************************************************************************************

        private void MessageProcessing (object arg1, object arg2)
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

                ushort MsgId  = BitConverter.ToUInt16 (msgBytes, (int)Marshal.OffsetOf<SocketLib.Header> ("MessageId"));
                ushort SeqNum = BitConverter.ToUInt16 (msgBytes, (int)Marshal.OffsetOf<SocketLib.Header> ("SequenceNumber"));

                MessageProcessing (MsgId, SeqNum, msgBytes);
            }

            catch (Exception ex)
            {
                Print (String.Format ("MessageProcessing Exception: {0}", ex.Message));
            }
        }

        //*******************************************************************************************************

        private void KeepAliveTimer_Elapsed (object sender, System.Timers.ElapsedEventArgs e)
        {
            if (ServerSocket.NumberClients > 0)
            {
               ArduinoInterface.KeepAliveMsg msg = new KeepAliveMsg ();
               ServerSocket.SendToAllClients (msg.ToBytes ());
            }
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
        //*******************************************************************************************************

        private void SocketServer_newConnectionHandler ()
        {
            Dispatcher.BeginInvoke ((SocketLib.Callback) GainedClient);
        }

        private void GainedClient ()
        {
            Print (string.Format ("Gained Client, {0} total", ServerSocket.NumberClients));
        }

        private void SocketServer_closedConnectionHandler ()
        {
            Dispatcher.BeginInvoke ((SocketLib.Callback) LostClient);
        }

        private void LostClient ()
        {
            Print (string.Format ("Lost Client, {0} remaining", ServerSocket.NumberClients));
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
        //*******************************************************************************************************

        static int localLineNumber = 1;
        object LocalTextBoxLock = new object ();

        private void AddTextToLocalTextBox (string str)
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
                Dispatcher.BeginInvoke ((SocketLib.PrintCallback) AddTextToLocalTextBox, str);
            }
        }
    }
}
