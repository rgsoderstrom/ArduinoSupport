using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Net.Sockets;

using System.Threading; // sleep
using System.Windows.Threading;
using System.Runtime.InteropServices; // for Marshal

using PlottingLibVer2;
using ArduinoInterface;

using Common;

namespace ControlConsole
{
    public partial class MainWindow : Window
    {
        SampleHistory sampleHistory = new SampleHistory ();
        PlotManager plotManager;

        SocketLib.TcpServer SocketServer = null;

        delegate void function ();

        System.Timers.Timer KeepAliveTimer = new System.Timers.Timer (20000); 

        // sample period in seconds for next "start sampling" message
        static List<ushort> SamplePeriodChoices = new List<ushort> {1, 5, 10, 30, 60};
        ushort SamplePeriodIndex = (ushort)0; 

        // only the thread that created WPF objects can access them. others must use Invoke () to
        // run a task on that thread. Its ID stored here
        readonly int WpfThread;

        //*******************************************************************************************************

        public MainWindow ()
        {
            try
            {
             //   System.Diagnostics.Process.Start (@"C:\Users\rgsod\Documents\Visual Studio 2019\Projects\Arduino\CrystalGrowing\ControlConsoleTest\bin\Debug\ControlConsoleTest.exe");

                InitializeComponent ();

                plotManager = new PlotManager (sampleHistory, temperaturePlot);

                temperaturePlot.RectangularGridOn = true;
                temperaturePlot.AxesTight = true;

                // only this thread can access WPF objects
                WpfThread = Thread.CurrentThread.ManagedThreadId;

                Left = 100;
                Top = 100;

                EventLog.Open ("..\\..\\Log.txt", true);

                SocketServer = new SocketLib.TcpServer (Print);
                SocketServer.MessageHandler          += SocketMessageHandler;
                SocketServer.NewConnectionHandler    += SocketServer_newConnectionHandler;
                SocketServer.ClosedConnectionHandler += SocketServer_closedConnectionHandler;
                SocketServer.PrintHandler            += Print;

                KeepAliveTimer.Elapsed += KeepAliveTimer_Elapsed;
                KeepAliveTimer.Enabled = true;

                for (int i=0; i<SamplePeriodChoices.Count; i++)
                {
                    ushort sec = SamplePeriodChoices [i];
                    RadioButton rb = new RadioButton ();
                    rb.Content = string.Format (sec > 1 ? "{0} Seconds" : "{0} Second", sec);
                    rb.Tag = i;
                    rb.GroupName = "SamplingPeriod";
                    rb.IsChecked = (i == SamplePeriodIndex);
                    rb.Click += SamplingPeriod_Click;
                    rb.Style = (Style)TheMainWindow.FindResource("GrayOut");
                    SampleRateButtons.Children.Add (rb);
                }
            }

            catch (Exception ex)
            {
                Print (String.Format ("Exception: {0}", ex.Message));
            }
        }

        //***************************************************************************************

        static int lineNumber = 1;
        object TextBoxLock = new object ();

        void AddTextToTextBox (string str)
        {
            EventLog.WriteLine (str);

            lock (TextBoxLock)
            {
                TextDisplay.Text += string.Format ("{0}: ", lineNumber++);
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
                AddTextToTextBox (str);
            }
            else
            {
                Dispatcher.BeginInvoke ((SocketLib.PrintCallback) AddTextToTextBox, str);
            }
        }

        //*******************************************************************************************************

        private void GainedClient ()
        {
            Print ("GainedClient");
            ClientButtons.IsEnabled = true; // must be done from WPF thread
        }

        private void LostClient ()
        {
            Print ("LostClient");

            if (SocketServer.NumberClients == 0)
                ClientButtons.IsEnabled = false;
        }

        private void SocketServer_closedConnectionHandler ()
        {
            Dispatcher.BeginInvoke ((SocketLib.Callback) LostClient);
        }

        private void SocketServer_newConnectionHandler ()
        {
            Dispatcher.BeginInvoke ((SocketLib.Callback) GainedClient);
        }

        private void KeepAliveTimer_Elapsed (object sender, System.Timers.ElapsedEventArgs e)
        {
            if (SocketServer.NumberClients > 0)
            {
               ArduinoInterface.KeepAliveMsg msg = new KeepAliveMsg ();
               SocketServer.SendToAllClients (msg.ToBytes ());
            }
        }

        //******************************************************************************************************

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
        //*******************************************************************************************************
        //*******************************************************************************************************

        // temperature samples
        List<Point> history = new List<Point> ();

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

                ushort MsgId = BitConverter.ToUInt16 (msgBytes, (int)Marshal.OffsetOf<SocketLib.Header> ("MessageId"));

                switch (MsgId)
                {
                    case ((ushort) ArduinoMessageIDs.TemperatureMsgId):

                        TemperatureMessage msg = new TemperatureMessage (msgBytes);
                        sampleHistory.Add (msg);
                        plotManager.Plot ();
                        break;

                    case ((ushort)ArduinoMessageIDs.StatusMsgId):
                        StatusMessage status = new StatusMessage (msgBytes);

                        IsSampling.Fill = (status.sampling == 1 ? Brushes.Green : Brushes.White);
                        StoredSamples.Text = string.Format ("{0}", status.numberRamSamples);
                        break;

                    default:
                        Print ("Unrecognized message ID");
                        break;
                }
            }

            catch (Exception ex)
            {
                Print (String.Format ("MessageProcessing Exception: {0}", ex.Message));
            }
        }

        //******************************************************************************************************

        private void ClearDisplayButton_Click (object sender, RoutedEventArgs e)
        {
            history.Clear ();
            temperaturePlot.Clear ();
        }

        private void CommonSend (byte[] bytes)
        {
            try {SocketServer.SendToAllClients (bytes);}
            catch (Exception ex) {Print (string.Format ("Exception: {0}", ex.Message));}
        }

        private void SendStatusButton_Click (object sender, RoutedEventArgs e)
        {
            EventLog.WriteLine ("SendStatus button");
            ArduinoInterface.SendStatusCmdMsg msg = new ArduinoInterface.SendStatusCmdMsg ();
            CommonSend (msg.ToBytes ());
        }

        private void StartSamplingButton_Click (object sender, RoutedEventArgs e)
        {
            EventLog.WriteLine ("StartSampling button");
            sampleHistory.OpenNewRecord ();
            ArduinoInterface.StartSamplingCmdMsg msg = new ArduinoInterface.StartSamplingCmdMsg ();
            msg.period = SamplePeriodChoices [SamplePeriodIndex];
            CommonSend (msg.ToBytes ());
        }

        private void StopSamplingButton_Click (object sender, RoutedEventArgs e)
        {
            EventLog.WriteLine ("StopSampling button");
            ArduinoInterface.StopSamplingCmdMsg msg = new ArduinoInterface.StopSamplingCmdMsg ();
            CommonSend (msg.ToBytes ());
        }

        private void ClearHistoryButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                EventLog.WriteLine ("ClearHistory button");
                ArduinoInterface.ClearHistoryCmdMsg msg = new ArduinoInterface.ClearHistoryCmdMsg ();
                CommonSend (msg.ToBytes ());
            }

            catch (Exception ex)
            {
                Print (string.Format ("ClearHistoryButton Exception: {0}", ex.Message));
            }
        }

        private void SendHistoryButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                EventLog.WriteLine ("SendHistory button");
                sampleHistory.OpenNewRecord ();
                ArduinoInterface.SendHistoryCmdMsg msg = new ArduinoInterface.SendHistoryCmdMsg ();
                CommonSend (msg.ToBytes ());
            }

            catch (Exception ex)
            {
                Print (string.Format ("SendHistoryButton Exception: {0}", ex.Message));
            }
        }

        private void DisconnectButton_Click (object sender, RoutedEventArgs e)
        {
            EventLog.WriteLine ("Disconnect button");
            ArduinoInterface.DisconnectCmdMsg msg = new ArduinoInterface.DisconnectCmdMsg ();
            CommonSend (msg.ToBytes ());
        }

        private void SamplingPeriod_Click (object sender, RoutedEventArgs e)
        {
            EventLog.WriteLine ("SamplePeriod button");

            if (sender is RadioButton)
            {
                SamplePeriodIndex = Convert.ToUInt16 ((sender as RadioButton).Tag);
                //Print (string.Format ("SamplePeriodIndex = {0}", SamplePeriodIndex));
                //Print (string.Format ("SamplePeriod = {0}", SamplePeriodChoices [SamplePeriodIndex]));
            }
        }

        /*****
        private void StartSendContButton_Click (object sender, RoutedEventArgs e)
        {
            EventLog.WriteLine ("StartContinuousSend button");
            ArduinoInterface.SendContinuouslyCmdMsg msg = new ArduinoInterface.SendContinuouslyCmdMsg ();
            CommonSend (msg.ToBytes ());
        }

        private void StopSendContButton_Click (object sender, RoutedEventArgs e)
        {
            EventLog.WriteLine ("StopContinuousSend button");
            ArduinoInterface.StopSamplingCmdMsg msg = new StopSamplingCmdMsg ();
            CommonSend (msg.ToBytes ());
        }
        *****/
    }
}
