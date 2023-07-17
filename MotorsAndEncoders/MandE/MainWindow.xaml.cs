
/*
    MainWindow for MandE project
        - specify seed vs. time profile for two motors
        - run those profiles, request and plot the measured speeds
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Windows.Media;

using System.Threading; // sleep
using System.Windows.Threading;
using System.Runtime.InteropServices; // for Marshal

using Common;
using ArduinoInterface;
using Plot2D_Embedded;

namespace ShaftEncoders
{
    public partial class MainWindow : Window
    {
        SocketLib.TcpServer ServerSocket = null;
        MessageQueue messageQueue; // messages to Arduino pass through here

        System.Timers.Timer KeepAliveTimer = new System.Timers.Timer (20000); // milliseconds

        // only the thread that created WPF objects can access them. others must use Invoke () to
        // run a task on that thread. Its ID stored here
        readonly int WpfThread;

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

                messageQueue = new MessageQueue (ServerSocket);

                KeepAliveTimer.Elapsed += KeepAliveTimer_Elapsed;
                KeepAliveTimer.Enabled = true;
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in MainWindow ctor: {0}", ex.Message));
            }
        }

        private void Window_Loaded (object sender, RoutedEventArgs e)
        {
            (Motor1_Grid.Children [0] as TextBox).Text = "110";
            (Motor1_Grid.Children [1] as TextBox).Text = "10";

            //(Motor1_Grid.Children [2] as TextBox).Text = "19";
            //(Motor1_Grid.Children [3] as TextBox).Text = "3.28";     // outside dia = 48, t = 4.8 sec

            //(Motor1_Grid.Children [4] as TextBox).Text = "66";
            //(Motor1_Grid.Children [5] as TextBox).Text = "1.18";

            //(Motor1_Grid.Children [6] as TextBox).Text = "86";
            //(Motor1_Grid.Children [7] as TextBox).Text = "2.46";

            //(Motor1_Grid.Children [8] as TextBox).Text = "0";
            //(Motor1_Grid.Children [9] as TextBox).Text = "1";

            (Motor2_Grid.Children [0] as TextBox).Text = "110";
            (Motor2_Grid.Children [1] as TextBox).Text = "10";

            //(Motor2_Grid.Children [2] as TextBox).Text = "112";
            //(Motor2_Grid.Children [3] as TextBox).Text = "3.28";

            //(Motor2_Grid.Children [4] as TextBox).Text = "66";
            //(Motor2_Grid.Children [5] as TextBox).Text = "1.18";

            //(Motor2_Grid.Children [6] as TextBox).Text = "86";
            //(Motor2_Grid.Children [7] as TextBox).Text = "2.46";

            //(Motor2_Grid.Children [8] as TextBox).Text = "0";
            //(Motor2_Grid.Children [9] as TextBox).Text = "1";

            PlotAreaLeft.DataAreaTitle = "Left";
            PlotAreaRight.DataAreaTitle = "Right";
            PlotAreaLeft.MouseEnabled = false;
            PlotAreaRight.MouseEnabled = false;
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
        //*******************************************************************************************************

        static int localLineNumber = 1;
        object LocalTextBoxLock = new object ();

        void AddTextToLocalTextBox (string str)
        {
            EventLog.WriteLine (str);

            lock (LocalTextBoxLock)
            {
                LocalTextDisplay.Text += string.Format ("{0}: ", localLineNumber++);
                LocalTextDisplay.Text += str;
                LocalTextDisplay.Text += "\n";
            }

            LocalTextDisplay.ScrollToEnd ();
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

        //*******************************************************************************************************

        static int remoteLineNumber = 1;
        object RemoteTextBoxLock = new object ();

        void AddTextToRemoteTextBox (string str)
        {
            EventLog.WriteLine (str);

            lock (RemoteTextBoxLock)
            {
                RemoteTextDisplay.Text += string.Format ("{0}: ", remoteLineNumber++);
                RemoteTextDisplay.Text += str;
                RemoteTextDisplay.Text += "\n";
            }

            RemoteTextDisplay.ScrollToEnd ();
        }

        public void RemotePrint (string str)
        {
            int callingThread = Thread.CurrentThread.ManagedThreadId;

            if (callingThread == WpfThread)
            {
                AddTextToRemoteTextBox (str);
            }
            else
            {
                Dispatcher.BeginInvoke ((SocketLib.PrintCallback) AddTextToRemoteTextBox, str);
            }
        }
    }
}
