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
using System.Linq;

namespace Sonar1Chan
{
    public partial class ArduinoWindow : System.Windows.Window
    {
        readonly MessageQueue messageQueue; // messages to Arduino pass through here

      //  readonly System.Timers.Timer KeepAliveTimer = new System.Timers.Timer (20000); // milliseconds

        // only the thread that created WPF objects can access them. others must use Invoke () to
        // run a task on that thread. Its ID stored here
        readonly int WpfThread;

        readonly string clientName = "Unknown"; // only used for error reporting

        private int Verbosity = 3;//1;

        private void ArduinoStuckCallback  () {Print              ("Arduino message queue stuck"); 
                                               EventLog.WriteLine ("Arduino message queue stuck");}

        //*******************************************************************************

        public ArduinoWindow (Socket socket)//, double sampleRate)
        {
            try
            {
                InitializeComponent ();
                ConnectedEllipse.Fill = Brushes.Green; // only get here when there is a connection

                // queue to hold and send msgs to Arduino
                messageQueue = new MessageQueue (ArduinoStuckCallback, 
                                                 ArduinoBusyCallback, 
                                                 ArduinoReadyCallback, 
                                                 Print, socket);

                // Create the state object.
                SocketLibrary.StateObject state = new SocketLibrary.StateObject ();
                state.workSocket = socket;

                socket.BeginReceive (state.buffer, 0, SocketLibrary.StateObject.BufferSize, 0, new AsyncCallback (ReceiveCallback), state);

                // only this thread can access WPF objects
                WpfThread = Thread.CurrentThread.ManagedThreadId;

        //        KeepAliveTimer.Elapsed += KeepAliveTimer_Elapsed;
          //      KeepAliveTimer.Enabled = true;    //-------------------------------------------------------

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

        //private void KeepAliveTimer_Elapsed (object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    KeepAliveMsg_Auto msg = new KeepAliveMsg_Auto ();

        //    if (Verbosity > 2)      Print ("Queueing KeepAlive msg, seq numb " + msg.header.SequenceNumber);
        //    else if (Verbosity > 1) Print ("Queueing KeepAlive msg");

        //    messageQueue.AddMessage (msg);
        //}

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


            // set default options

            foreach (object child in Verbosity_ComboBox.Items)
            {
                if (child is ComboBoxItem)
                {
                    ComboBoxItem cbi = child as ComboBoxItem;
                    int thisItemsTag = 0;

                    if (Utils.ConvertTagToInteger (cbi.Tag, ref thisItemsTag))
                    {
                        if (thisItemsTag == Verbosity)
                        {
                            cbi.IsSelected = true;
                            break;
                        }
                    }
                }
            }

            ZoomX_Button.IsChecked = true;
            //InputSamples_Button.IsChecked = true;
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

        //**************************************************************************
        //
        // Button-press handlers
        //
        private void ArduinoReadyCallback () {ReadyEllipse.Fill = Brushes.Green;}
        private void ArduinoBusyCallback  () {ReadyEllipse.Fill = Brushes.White;}

        double SampleRate = 100000; // these are written to Matlab "save" file
        double PingDuration = 1;
        double PingFrequency = 1;

        private void SendParamsMessage ()
        { 
            try
            { 
                // hardware characteristics
                const double CountsPerVolt = 1024 / 2.048;  // Mercury 2 DAC
                const double ClockFreq     = 50e6;          // FPGA Clock
                const double FreqScale     = 1 / 190.0;     // Mercury 2 CORDIC

                // convert user entered data
                       SampleRate    = double.Parse (SampleRateTB.Text);    // samples per second
                double RampStart     = double.Parse (RampStartTB.Text);     // volts
                double RampStop      = double.Parse (RampStopTB.Text);      // "
                double BlankingLevel = double.Parse (BlankingLevelTB.Text); // "
                double RampTime      = double.Parse (RampTimeTB.Text);      // milliseconds
                       PingFrequency = double.Parse (PingFrequencyTB.Text); // Hz
                       PingDuration  = double.Parse (PingDurationTB.Text);  // milliseconds

                // convert to format required by outgoing message. check for overflow or underflow
                double _sampleClockDiv = (uint) (0.5 + ClockFreq / SampleRate);
                ushort  sampleClockDiv = (ushort) _sampleClockDiv;
                SampleRateTB.Foreground = _sampleClockDiv == sampleClockDiv ? Brushes.Black : Brushes.Red;

                double _rampStart = (uint) (0.5 + RampStart * CountsPerVolt);
                ushort rampStart  = (ushort) _rampStart;
                RampStartTB.Foreground = _rampStart == rampStart ? Brushes.Black : Brushes.Red;

                double _rampStop = (uint) (0.5 + RampStop  * CountsPerVolt);
                ushort rampStop  = (ushort) _rampStop;
                RampStopTB.Foreground = _rampStop == rampStop ? Brushes.Black : Brushes.Red;

                double _blankingLevel  = (uint) (0.5 + BlankingLevel * CountsPerVolt);
                ushort  blankingLevel  = (ushort) _blankingLevel;
                BlankingLevelTB.Foreground = _blankingLevel == blankingLevel ? Brushes.Black : Brushes.Red;

                double  rampRate = (rampStop - rampStart) / (RampTime / 1000); // counts per second
                double _rampDivisor = (uint) (0.5 + ClockFreq / rampRate);
                ushort  rampDivisor = (ushort) _rampDivisor;
                RampTimeTB.Foreground = _rampDivisor == rampDivisor ? Brushes.Black : Brushes.Red;

                double _frequency = (uint) (0.5 + PingFrequency * FreqScale);
                ushort  frequency = (ushort) _frequency;
                PingFrequencyTB.Foreground = _frequency == frequency ? Brushes.Black : Brushes.Red;

                double _duration = (uint) (8.5 + (PingDuration / 1000) * ClockFreq);
                ushort  duration = (ushort) _duration;
                PingDurationTB.Foreground = _duration == duration ? Brushes.Black : Brushes.Red;

                SonarParametersMsg_Auto msg = new SonarParametersMsg_Auto ();
                msg.data.SampleClockDivisor   = sampleClockDiv;
                msg.data.RampStartingLevel    = rampStart;
                msg.data.RampStoppingLevel    = rampStop;
                msg.data.BlankingLevel        = blankingLevel;
                msg.data.RampRateClockDivisor = rampDivisor;
                msg.data.PingFrequency        = frequency;
                msg.data.PingDuration         = duration;
                
                //Print (msg.ToString ());

                messageQueue.AddMessage (msg);

                if (Verbosity > 1)
                    Print ("Queueing Params msg " + msg.SequenceNumber);
            }
        
            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in SendParams Button click: {0}", ex.Message));
            }
        }
        
        private void SendClearMessage ()
        { 
            try
            {
                Samples.Clear ();
                SaveButton.IsEnabled = false;

                ClearSamplesMsg_Auto msg = new ClearSamplesMsg_Auto ();
                messageQueue.AddMessage (msg);

                if (Verbosity > 1)
                    Print ("Queueing Clear msg " + msg.SequenceNumber);
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in ClearButton click: {0}", ex.Message));
            }
        }

        private void SendPingMessage ()
        { 
            try
            {
                BeginPingCycleMsg_Auto msg = new BeginPingCycleMsg_Auto (); 
                messageQueue.AddMessage (msg);

                if (Verbosity > 1)
                    Print ("Queueing Ping msg " + msg.SequenceNumber);
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in Send Ping click: {0}", ex.Message));
            }
        }

        /** used for debugging
        private void ClearButton_Click       (object sender, RoutedEventArgs e) {SendClearMessage (); }
        private void ParamsButton_Click      (object sender, RoutedEventArgs e) {SendParamsMessage ();}
        private void PingButton_Click        (object sender, RoutedEventArgs e) {SendPingMessage ();}
        private void SendSamplesButton_Click (object sender, RoutedEventArgs e) {RequestSamples ();}
        **/

        private void PingSequenceButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                if (Verbosity > 0)
                    Print ("Queueing all Ping Cycle messages");

                SendClearMessage ();
                SendParamsMessage ();
                SendPingMessage ();
                RequestSamples ();
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in Ping Sequence Button click: {0}", ex.Message));
            }
        }

        //*****************************************************************************************
        //*****************************************************************************************
        //*****************************************************************************************

        // send the request message. Invoked after button click and after a sample message is received if there are more 
        // samples expected
        private void RequestSamples ()
        {
            try
            { 
                SendSamplesMsg_Auto msg = new SendSamplesMsg_Auto ();
                messageQueue.AddMessage (msg);

                if (Verbosity > 1) 
                    Print ("Queueing Sample Request msg " + msg.SequenceNumber);
            }
        
            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in RequestSamples: {0}", ex.Message));
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
            samplesFile.WriteLine ("PingDuration = " + PingDuration + "; % ping duration, milliseconds");

            samplesFile.WriteLine ("z = [...");
                    
            for (int i=0; i<Samples.Count-1; i++)
                samplesFile.WriteLine (Samples [i].ToString () + " ; ...");

            samplesFile.WriteLine (Samples [Samples.Count-1].ToString () + "];");
            samplesFile.Close ();
        }

        //*****************************************************************************************
        //*****************************************************************************************
        //*****************************************************************************************

        private void ClearPlotButton_Click (object sender, RoutedEventArgs e)
        {
            PlotArea.Clear ();
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
