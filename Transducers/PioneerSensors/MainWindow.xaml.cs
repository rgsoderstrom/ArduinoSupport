﻿using System;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace PioneerSensors
{
    public partial class MainWindow : Window
    {
        SocketLibrary.TcpServer ServerSocket = null;

        // only the thread that created WPF objects can access them. others must use Invoke () to
        // run a task on that thread. Its ID stored here
        readonly int WpfThread;

        //*****************************************************************

        public MainWindow ()
        {
            Common.EventLog.Open (@"..\..\Log.txt", true);

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
                Common.EventLog.WriteLine (string.Format ("Exception in MainWindow ctor: {0}", ex.Message));
            }
        }

        private void SocketServer_newConnectionHandler (Socket sock)
        {
            Dispatcher.BeginInvoke ((SocketLibrary.Callback2) GainedClient, sock);
        }

        ArduinoWindow ard = null;

        private void GainedClient (Socket sock)
        {
            try
            { 
                //if (ard == null)
                //{ 
                    ard = new ArduinoWindow (sock);
                    ard.Owner = this;
                    ard.Show ();
                    ard.Activate ();
                    Print ("Gained Client");
                //}
                //else
                //{
                //    ard.NewSocket (sock);
                //    Print ("Re-connect");
                //}
            }

            catch (Exception ex)
            {
                Common.EventLog.WriteLine (string.Format ("Exception in GainedClient: {0}", ex.Message));
            }
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
        //*******************************************************************************************************

        static int localLineNumber = 1;
        object LocalTextBoxLock = new object ();

        void AddTextToLocalTextBox (string str)
        {
            Common.EventLog.WriteLine (str);

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
            Common.EventLog.WriteLine ("MainWindow loaded");
        }

        private void MainWindow_Closed (object sender, EventArgs e)
        {
            Common.EventLog.Close ();
        }

        //*****************************************************************************************************
        //*****************************************************************************************************
        //*****************************************************************************************************

        private void LaunchSimButton_Click (object sender, RoutedEventArgs e)
        {
            try
            { 
                string str = AppDomain.CurrentDomain.BaseDirectory;
                Common.EventLog.WriteLine ("Current directory is " + str);

                var p = new System.Diagnostics.Process();
                p.StartInfo.FileName  = @"..\..\..\ArduinoSimulator\bin\Debug\ArduinoSimulator.exe";

                string [] AllArgs = new string [] {"ServerName", System.Net.Dns.GetHostName (),
                                                   "SimName",    "Transducer"}; // optionally tells which simulator to run
                string args = "";

                for (int i=0; i<AllArgs.Length; i++)
                    args += AllArgs [i] + " ";

                p.StartInfo.Arguments = args;                        
                p.Start();
            }

            catch (Exception ex)
            {
                Common.EventLog.WriteLine ("Exception launching simulator: " + ex.Message);
            }
        }
    }
}
