﻿using System;
using System.Windows;
using System.Net.Sockets;
using System.Threading;

using System.Collections.Generic;

using Common;
using ArduinoInterface;
using SocketLibrary;
using Plot2D_Embedded;
using System.Net;
using System.Windows.Controls;
using System.IO;
using System.Linq;

namespace A2D_Tests
{
    public partial class ArduinoWindow : System.Windows.Window
    {
        readonly MessageQueue messageQueue; // messages to Arduino pass through here

        readonly System.Timers.Timer KeepAliveTimer = new System.Timers.Timer (20000); // milliseconds

        // only the thread that created WPF objects can access them. others must use Invoke () to
        // run a task on that thread. Its ID stored here
        readonly int WpfThread;

        readonly string clientName = "Unknown"; // only used for error reporting

        public static double SampleRate = 100000;
        private int Verbosity = 3;//1;

        //*******************************************************************************

        //
        // Message re-send
        //
        void EnableReSendButton () // this runs in the thread that can access WPF objects
        {
            ResendBtn.IsEnabled = true;
        }

        void ResendTimerCallback ()
        {
            Print ("Message to Arduino not acknowledged");
            Dispatcher.BeginInvoke ((Callback) EnableReSendButton);
        }

        //*******************************************************************************

        public ArduinoWindow (Socket socket)
        {
            try
            {
                InitializeComponent ();

                // queue to hold and send msgs to Arduino
                messageQueue = new MessageQueue (ResendTimerCallback, null, null, Print, socket);

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

              //if (Verbosity > 2)
              //    Print (string.Format ("Message received, {0} bytes", bytesRead));

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

            //
            // set default options
            //
            foreach (object child in Verbosity_ComboBox.Items)
            {
                if (child is ComboBoxItem)
                { 
                    ComboBoxItem cbi = child as ComboBoxItem;
                    int tagAsInt = 0;

                    if (Utils.ConvertTagToInteger (cbi.Tag, ref tagAsInt))
                    {
                        if (tagAsInt == Verbosity)
                        {
                            cbi.IsSelected = true;
                            break;
                        }
                    }
                }
            }

            ZoomX_Button.IsChecked = true;
            InputSpect_Button.IsChecked = true;
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
                            PlotArea.ZoomBoth = true;
                            break;

                        case "Zoom_X":
                          //PlotArea.AxesEqual = false;
                            PlotArea.ZoomX = true;
                            break;

                        case "Zoom_Y":
                          //PlotArea.AxesEqual = false;
                            PlotArea.ZoomY = true;
                            break;

                        default:
                            throw new Exception ("Invalid zoom option");
                    }
                }
            }
        }

        //**************************************************************************************

        enum DisplayOptions {InputSamples, InputSpectrum, WindowedSamples, WindowedSpectrum};
        private DisplayOptions SelectedDisplay ;//= DisplayOptions.InputSamples;

        private void DisplayOptionButton_Checked (object sender, RoutedEventArgs args)
        {
            if (sender is RadioButton rb)
            {
                string tag = rb.Tag as string;

                if (WindowIsLoaded)
                {
                    switch (tag)
                    {
                        case "Input_Samples": SelectedDisplay = DisplayOptions.InputSamples;  break;
                        case "Input_Spect":   SelectedDisplay = DisplayOptions.InputSpectrum; break;
                        case "Win_Samples":   SelectedDisplay = DisplayOptions.WindowedSamples; break;
                        case "Win_Spect":     SelectedDisplay = DisplayOptions.WindowedSpectrum; break;
                        default: throw new Exception ("Invalid display option");
                    }
                }
            }

            if (signalProcessor != null)
            {
                PlotArea.Clear ();
                if (SelectedDisplay == DisplayOptions.InputSamples)  PlotArea.Plot (new LineView (signalProcessor.InputSamples));
                if (SelectedDisplay == DisplayOptions.InputSpectrum) PlotArea.Plot (new LineView (signalProcessor.InputSpectrum));
                if (SelectedDisplay == DisplayOptions.WindowedSamples)  PlotArea.Plot (new LineView (signalProcessor.WindowedSamples));
                if (SelectedDisplay == DisplayOptions.WindowedSpectrum) PlotArea.Plot (new LineView (signalProcessor.WindowedSpectrum));
                PlotArea.RectangularGridOn = true;
            }
        }

        //**************************************************************************
        //
        // Button-press handlers
        //

        private void ClearButton_Click (object sender, RoutedEventArgs e)
        { 
            try
            { 
                Samples.Clear ();
                SaveButton.IsEnabled = false;
                PeaksButton.IsEnabled = false;

                ClearSamplesMsg_Auto msg = new ClearSamplesMsg_Auto ();
                messageQueue.AddMessage (msg);

                if (Verbosity > 1)      Print ("Sending Clear msg, seq numb " + msg.header.SequenceNumber);
                else if (Verbosity > 0) Print ("Sending Clear msg");
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
                BeginSamplingMsg_Auto msg = new BeginSamplingMsg_Auto ();
                messageQueue.AddMessage (msg);

                if (Verbosity > 1)      Print ("Sending Collect msg, seq numb " + msg.header.SequenceNumber);
                else if (Verbosity > 0) Print ("Sending Collect msg");
            }
        
            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in CollectButton click: {0}", ex.Message));
            }
        }

        //*****************************************************************************************
        //*****************************************************************************************
        //*****************************************************************************************

        private void SendSampleRateButton_Click (object sender, RoutedEventArgs e)
        {
            try
            { 
                double sampleRate = 0;
                bool success = Double.TryParse (SampleRateBox.Text, out sampleRate);

                if (success == true)
                {
                    SampleRate = sampleRate;

                    ushort divisor = (ushort) (0.5 + 50e6 / sampleRate); 
                    SampleRateMsg_Auto msg = new SampleRateMsg_Auto ();
                    msg.data.RateDivisor = divisor;
                    messageQueue.AddMessage (msg);

                    if (Verbosity > 1)      Print ("Sending Sample Rate msg, seq numb " + msg.header.SequenceNumber);
                    else if (Verbosity > 0) Print ("Sending Sample Rate msg");


                    double ActualRate = 50e6 / divisor;
                    SampleRateBox.Text = string.Format ("{0:0.###}", ActualRate);
                    SampleRate = ActualRate;
                }
                else
                    Print ("Invalid sample rate");
            }

            catch (Exception ex)
            {
                Print ("Exception processing sample rate: " + ex.Message);
            }
        }

        private void SendGainButton_Click (object sender, RoutedEventArgs e)
        {
            if (Verbosity > 0) Print ("Send Gain button clicked");

            double gainPercent = 0;
            bool success = Double.TryParse (GainBox.Text, out gainPercent);

            if (success == false)
            {
                Print ("Invalid gain percentage");
                return;
            }

            if (gainPercent < 0 || gainPercent > 100)
            {
                Print ("Gain percentage out of range");
                return;
            }

            ushort gainWord = (ushort) ((gainPercent / 100) * 1.25 * (1024 / 2.048)); 
            AnalogGainMsg_Auto msg = new AnalogGainMsg_Auto ();
            msg.data.DacValue = gainWord;
            messageQueue.AddMessage (msg);

            if (Verbosity > 1)      Print ("Sending Gain msg, seq numb " + msg.header.SequenceNumber);
            else if (Verbosity > 0) Print ("Sending Gain msg");
        }


        //*****************************************************************************************
        //*****************************************************************************************
        //*****************************************************************************************

        private void SendSamplesButton_Click (object sender, RoutedEventArgs e)
        {
            if (Verbosity > 0) Print ("Send button clicked");

            Samples.Clear ();
            sendMsgCounter = 1;
            RequestSamples ();
        }

        // send the request message. Invoked after button click and after a sample message is received if there are more 
        // samples expected
        private void RequestSamples ()
        {
            try
            { 
                SendSamplesMsg_Auto msg = new SendSamplesMsg_Auto ();
                messageQueue.AddMessage (msg);

                if (Verbosity > 1)      Print ("Sending Send msg " + sendMsgCounter + " seq number " + msg.header.SequenceNumber);
                else if (Verbosity > 0) Print ("Sending Send msg");
            }
        
            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in SendButton click: {0}", ex.Message));
            }
        }

        //*****************************************************************************************
        //*****************************************************************************************
        //*****************************************************************************************

        private void SaveButton_Click (object sender, RoutedEventArgs e)
        {
            int lastNumber = 0;

            string[] existingFiles = Directory.GetFiles(@".", "samples*.m");

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

            string fileName = "samples" + ++lastNumber + ".m";
            Print ("Saving samples to file " + fileName);
            StreamWriter samplesFile = new StreamWriter (fileName);

            samplesFile.WriteLine ("Fs = " + SampleRate + "; % sample rate");

            samplesFile.WriteLine ("z = [...");
                    
            for (int i=0; i<Samples.Count-1; i++)
                samplesFile.WriteLine (Samples [i].ToString () + " ; ...");

            samplesFile.WriteLine (Samples [Samples.Count-1].ToString () + "];");
            samplesFile.Close ();
        }

        //*****************************************************************************************
        //*****************************************************************************************
        //*****************************************************************************************

        private void PeaksButton_Click (object sender, RoutedEventArgs e)
        {
            double thresh;            
            bool success = Double.TryParse (ThreshBox.Text, out thresh);

            if (success == true)
            { 
                List<Point> peaks = signalProcessor.FindPeaks (thresh);
                Print ("Peaks:");

                foreach (Point pk in peaks)
                    if (pk.X > 0)
                        Print (pk.X.ToString ());
            }
            else
                Print ("Error: peak threshold invalid");
        }

        //*****************************************************************************************
        //*****************************************************************************************
        //*****************************************************************************************

        private void Resend_Click (object sender, RoutedEventArgs e)
        {
            Print ("Resending last message");
            messageQueue.ResendLastMsg ();
            ResendBtn.IsEnabled = false;
        }

        private void Verbosity_ComboBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            ComboBoxItem cbi = cb.SelectedItem as ComboBoxItem;
            int number = 0;

            bool success = Utils.ConvertTagToInteger (cbi.Tag, ref number);

            if (success)
                Verbosity = number;
        }
    }
}
