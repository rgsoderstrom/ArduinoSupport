using System;
using System.Windows;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Media;

using Common;
using ArduinoInterface;
using SocketLibrary;
using System.Net;
using System.Windows.Controls;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Policy;

// This created with Add -> new item -> Window (WPF)

namespace PioneerSensors
{
    public partial class ArduinoWindow : System.Windows.Window
    {
        readonly MessageQueue messageQueue; // messages to Arduino pass through here

        readonly System.Timers.Timer KeepAliveTimer = new System.Timers.Timer (20000); // milliseconds

        // only the thread that created WPF objects can access them. others must use Invoke () to
        // run a task on that thread. Its ID stored here
        readonly int WpfThread;

        readonly string clientName = "Unknown"; // only used for error reporting

        private int Verbosity = 3;//1;

        //*******************************************************************************

        void ReadyCallback () {ReadyEllipse.Fill = Brushes.Green;}
        void BusyCallback  () {ReadyEllipse.Fill = Brushes.White;}
        void StuckCallback () {Print ("Queue stuck");}

        public ArduinoWindow (Socket socket)
        {
            try
            {
                InitializeComponent ();
                ConnectedEllipse.Fill = Brushes.Green; // don't get here until there is a connection

                // queue to hold and send msgs to Arduino
                messageQueue = new MessageQueue (StuckCallback, BusyCallback, ReadyCallback, Print, socket);

                // Create the state object.
                SocketLibrary.StateObject state = new SocketLibrary.StateObject ();
                state.workSocket = socket;

                socket.BeginReceive (state.buffer, 0, SocketLibrary.StateObject.BufferSize, 0, new AsyncCallback (ReceiveCallback), state);

                // only this thread can access WPF objects
                WpfThread = Thread.CurrentThread.ManagedThreadId;

                KeepAliveTimer.Elapsed += KeepAliveTimer_Elapsed;
                KeepAliveTimer.Enabled = true;    //-------------------------------------------------------

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
              // Retrieve the state object and the handler socket from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

              // Read data from the socket. 
                int bytesRead = handler.EndReceive (ar);

                if (bytesRead == 0)
                {
                   // allClients.Remove (state.workSocket);
                    state.workSocket.Close ();
                    Print ("bytesRead == 0"); // this is an error, so always print
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
            ConnectedEllipse.Fill = Brushes.White;
            ReadyEllipse.Fill     = Brushes.White;

            messageQueue.ArduinoReady = false;
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
        //*******************************************************************************************************
        //*******************************************************************************************************

        private void KeepAliveTimer_Elapsed (object sender, System.Timers.ElapsedEventArgs e)
        {
            KeepAliveMsg_Auto msg = new KeepAliveMsg_Auto ();

            if (Verbosity > 2)      Print ("Sending KeepAlive msg, seq numb " + msg.header.SequenceNumber);
            else if (Verbosity > 1) Print ("Sending KeepAlive msg");

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

        bool WindowIsLoaded = false;

        private void ArduinoWindow_Loaded (object sender, RoutedEventArgs e)
        {
            EventLog.WriteLine ("Arduino Window Loaded");
            WindowIsLoaded = true;

            ZoomX_Button.IsChecked = true;
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
        //*******************************************************************************************************

        private void ZoomOptionButton_Checked (object sender, RoutedEventArgs args)
        {
            if (sender is RadioButton rb)
            {
                string tag = rb.Tag as string;

                if (WindowIsLoaded)
                {
                    switch (tag)
                    {
                        case "Zoom_Both":
                            PlotArea.ZoomX = true;
                            PlotArea.ZoomY = true;
                            break;

                        case "Zoom_X":
                            PlotArea.AxesEqual = false;
                            PlotArea.ZoomX = true;
                            PlotArea.ZoomY = false;
                            break;

                        case "Zoom_Y":
                            PlotArea.AxesEqual = false;
                            PlotArea.ZoomX = false;
                            PlotArea.ZoomY = true;
                            break;

                        default:
                            throw new Exception ("Invalid zoom option");
                    }
                }
            }
        }

        //**************************************************************************************
        //
        // Button-press handlers
        //

        private void ClearButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                PlotArea.Clear ();
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in ClearButton click: {0}", ex.Message));
            }
        }

        private void BeginButton_Click (object sender, RoutedEventArgs e)
        {
            DataAvailableEllipse.Fill = Brushes.White;
            BeginButton.IsEnabled = false;                
            PlotArea.Clear ();

            StartSamplingMsg_Auto msg = new StartSamplingMsg_Auto ();
            messageQueue.AddMessage (msg);

            Print ("Queueing StartSampling msg " + msg.SequenceNumber);
        }

        //******************************************************************************
        //******************************************************************************
        //******************************************************************************

        const string defaultDescr = "Optional data description here";
        private string previousComment = defaultDescr;

        const string SavePath = @"..\..";
        const string SaveBaseName = "samples";
        const string SaveSearchName = SaveBaseName + "*.m";

        private void SaveButton_Click (object sender, RoutedEventArgs e)
        {
            int lastNumber = 0;
            string[] existingFiles = Directory.GetFiles(SavePath, SaveSearchName);

            foreach (string fname in existingFiles)
            {
                string numStr = new string (fname.SkipWhile (c=>!char.IsDigit (c))
                                                 .TakeWhile (c=>char.IsDigit(c))
                                                 .ToArray ());
                
                if (numStr != null && numStr.Length > 0)
                { 
                    int number = Convert.ToInt32 (numStr);
                    if (lastNumber < number) lastNumber = number;
                }
            }

            string fileName = SavePath + "\\" + SaveBaseName + ++lastNumber + ".m";

            SaveDialogBox dlg = new SaveDialogBox ();

            dlg.FileName = fileName;
            dlg.FileComments = previousComment;

            bool? accept = dlg.ShowDialog ();

            if (accept == true)
            { 
                previousComment = dlg.FileComments;
                SaveSampleData (dlg.FileName, dlg.FileComments, lastNumber);
            }
        }

        private void SaveSampleData (string fileName, string comment, int number)
        {
            StreamWriter samplesFile = new StreamWriter (fileName);
            int Count = ReceivedTime.Count;

            //
            // if a comment present, write it to file
            //
            if (comment != defaultDescr)
            {
                // split into words
                string [] tokens = comment.Split (new char [] {' '});

                samplesFile.WriteLine (" ");
                string commentLine = "%";

                for (int i=0; i<tokens.Length; i++)
                {
                    commentLine += " " + tokens [i];
                    
                    bool lastToken = i == tokens.Length - 1;

                    if (commentLine.Length > 80 || lastToken == true)
                    {
                        samplesFile.WriteLine (commentLine);
                        commentLine = "%";
                    }
                }

                samplesFile.WriteLine (" ");
            }

            //
            // Write samples to file
            //
            samplesFile.WriteLine ("z" + number + " = [...");

            int j;
            
            for (j=0; j<Count-1; j++)
            { 
                samplesFile.Write     (ReceivedTime     [j].ToString () + "  ");
                samplesFile.Write     (ReceivedPressure [j].ToString () + " ");
                samplesFile.WriteLine (ReceivedAngle    [j].ToString () + " ; ...");
            }

            samplesFile.Write     (ReceivedTime     [j].ToString () + "  ");
            samplesFile.Write     (ReceivedPressure [j].ToString () + " ");
            samplesFile.WriteLine (ReceivedAngle    [j].ToString () + "];");

            samplesFile.Close ();
        }
    }
}
