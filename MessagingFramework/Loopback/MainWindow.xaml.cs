using System;
using System.Windows;

using Common;
using ArduinoInterface;
using System.Threading;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Interop;
using SocketLibrary;

namespace Loopback
{
    public partial class MainWindow : Window
    {
        SocketLibrary.TcpServer ServerSocket = null;

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

                ServerSocket = new SocketLibrary.TcpServer (Print);
                ServerSocket.NewConnectionHandler += SocketServer_newConnectionHandler;
                ServerSocket.PrintHandler         += Print;
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in MainWindow ctor: {0}", ex.Message));
            }
        }

        private void SocketServer_newConnectionHandler (Socket sock)
        {
            Dispatcher.BeginInvoke ((SocketLibrary.Callback2) GainedClient, sock);
        }

        private void GainedClient (Socket sock)
        {
            ArduinoWindow ard = new ArduinoWindow (sock);
            ard.Owner = this;
            ard.Show ();
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
                Dispatcher.BeginInvoke ((SocketLibrary.PrintCallback) AddTextToLocalTextBox, str);
            }
        }

        private void MainWindow_Loaded (object sender, RoutedEventArgs e)
        {
            EventLog.WriteLine ("MainWindow loaded");
        }

        private void MainWindow_Closed (object sender, EventArgs e)
        {
            EventLog.Close ();
        }
    }
}
