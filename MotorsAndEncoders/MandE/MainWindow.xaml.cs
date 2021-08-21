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

namespace ShaftEncoders
{
    public partial class MainWindow : Window
    {
        SocketLib.TcpServer ServerSocket = null;

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
            PlotArea.MouseEnabled = false;
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
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

                ushort MsgId  = BitConverter.ToUInt16 (msgBytes, (int)Marshal.OffsetOf<SocketLib.Header> ("MessageId"));
                ushort SeqNum = BitConverter.ToUInt16 (msgBytes, (int)Marshal.OffsetOf<SocketLib.Header> ("SequenceNumber"));

                switch (MsgId)
                {
                    case (ushort)ArduinoMessageIDs.TextMsgId:
                        TextMessage tm = new TextMessage (msgBytes);
                        string str = new string (tm.text);
                        RemotePrint (str);
                        break;

                    case (ushort)ArduinoMessageIDs.BufferStatusMsgId:
                    {
                        BufferStatusMessage bs = new BufferStatusMessage (msgBytes);
                        CollectionComplete.IsChecked = (bs.data == 1);
                    }
                    break;

                    case (ushort)ArduinoMessageIDs.CollectedDataMsgId:
                    {
                        CollectionDataMessage msg = new CollectionDataMessage (msgBytes, Print);
                        bool more = EncoderCounts (msg);

                        Print ("*");

                        if (more)
                        {
                            SendNextCollectionMsg msg2 = new SendNextCollectionMsg ();
                            ServerSocket.SendToAllClients (msg2.ToBytes ());
                        }
                    }
                    break;

                    case (ushort)ArduinoMessageIDs.CollSendCompleteMsgId:
                        PlotSpeeds ();
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

        List<EncoderCounts> history = new List<EncoderCounts> ();

        private bool EncoderCounts (CollectionDataMessage msg) // returns true if more expected
        {
            for (int i = 0; i<msg.data.put; i++)
            {
                history.Add (msg.data.counts [i]);
            }

            return msg.data.put == CollectionData.MsgBufferSize;
        }

        List<double> VelTimes = new List<double> ();
        List<double> Vel1 = new List<double> ();
        List<double> Vel2 = new List<double> ();

        private void PlotSpeeds ()
        {
            Print (history.Count.ToString () + " samples");
            PlotArea.Clear ();
            PlotArea.RectangularGridOn = true;

            if (history.Count < 2) return;

            VelTimes.Clear ();
            Vel1.Clear ();
            Vel2.Clear ();

            //for (int i=1; i<history.Count; i++)
            //{
            //    double dt = history [i].time - history [i-1].time;
            //    double de1 = history [i].enc1;
            //    double de2 = history [i].enc2;

            //    VelTimes.Add ((history [i].time - history [0].time) / 1000.0);
            //    Vel1.Add (de1 / dt);
            //    Vel2.Add (de2 / dt);
            //}


            for (int i = 1; i<history.Count-1; i++)
            {
                double dt = history [i+1].time - history [i-1].time;
                double de1 = history [i+1].enc1 + history [i].enc1;
                double de2 = history [i+1].enc2 + history [i].enc2;

                VelTimes.Add ((history [i].time - history [0].time) / 1000.0);
                Vel1.Add (de1 / dt);
                Vel2.Add (de2 / dt);
            }



            List<Point> points1 = new List<Point> ();
            List<Point> points2 = new List<Point> ();
            //uint startTime = history [0].time;

            for (int i = 0; i<VelTimes.Count; i++)
            {
                points1.Add (new Point (VelTimes [i], Vel1 [i]));
                points2.Add (new Point (VelTimes [i], Vel2 [i]));


                //EncoderCounts ec = history [i];
                //double millis = ec.time - startTime;
                //points1.Add (new Point (millis, ec.enc1));
                //points2.Add (new Point (millis, ec.enc2));
            }

            LineView line1 = new LineView (points1);
            LineView line2 = new LineView (points2);
            //line1.Size = 0.1;
            line1.Color = Brushes.Red;
            line2.Color = Brushes.Green;

            PlotArea.Plot (line1);
            PlotArea.Plot (line2);

            //foreach (Point pt in points1)
            // Print (pt.X.ToString () + ", " + pt.Y.ToString ());

            //PlotArea.GetAxes ()
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

            //StartMotors_Button.IsEnabled   = true;
            //StopMotors_Button.IsEnabled    = true;
            //DisableMotors_Button.IsEnabled = true;
            //Disconnect_Button.IsEnabled    = true;
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
        //*******************************************************************************************************

        private void SocketServer_closedConnectionHandler ()
        {
            Dispatcher.BeginInvoke ((SocketLib.Callback) LostClient);
        }

        private void LostClient ()
        {
            Print (string.Format ("Lost Client, {0} remaining", ServerSocket.NumberClients));

            //if (ServerSocket.NumberClients == 0)
            //{
            //    StartMotors_Button.IsEnabled   = false;
            //    StopMotors_Button.IsEnabled    = false;
            //    DisableMotors_Button.IsEnabled = false;
            //    Disconnect_Button.IsEnabled    = false;
            //}
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
        //*******************************************************************************************************

        private void SendSpeedsButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                int speed1 = int.Parse (Speed1_TextBox.Text);
                if (speed1 >  15) {speed1 =  15; Speed1_TextBox.Text = speed1.ToString ();}
                if (speed1 < -15) {speed1 = -15; Speed1_TextBox.Text = speed1.ToString ();}

                int speed2 = int.Parse (Speed2_TextBox.Text);
                if (speed2 >  15) {speed2 =  15; Speed2_TextBox.Text = speed2.ToString ();}
                if (speed2 < -15) {speed2 = -15; Speed2_TextBox.Text = speed2.ToString ();}

                MotorSpeedMsg msg = new MotorSpeedMsg (speed1, speed2);
                ServerSocket.SendToAllClients (msg.ToBytes ());
            }

            catch (Exception ex)
            {
                Print (string.Format ("Exception: {0}", ex.Message));
            }
        }

        //*******************************************************************************************************
        //
        // Start Collection
        //
        private void StartCollectionButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                StartCollectionMsg msg = new StartCollectionMsg ();
                ServerSocket.SendToAllClients (msg.ToBytes ());
            }

            catch (Exception ex)
            {
                Print (string.Format ("Exception: {0}", ex.Message));
            }
        }

        //*******************************************************************************************************
        //
        // Stop Collection
        //
        private void StopCollectionButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                StopCollectionMsg msg = new StopCollectionMsg ();
                ServerSocket.SendToAllClients (msg.ToBytes ());
            }

            catch (Exception ex)
            {
                Print (string.Format ("Exception: {0}", ex.Message));
            }
        }

        //*******************************************************************************************************
        //
        // Send Collection
        //
        private void SendCollectionButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                history.Clear ();
                SendFirstCollectionMsg msg = new SendFirstCollectionMsg ();
                ServerSocket.SendToAllClients (msg.ToBytes ());
            }

            catch (Exception ex)
            {
                Print (string.Format ("Exception: {0}", ex.Message));
            }
        }

        //*******************************************************************************************************
        //
        // Clear Collection
        //
        private void ClearCollectionButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                ClearCollectionMsg msg = new ClearCollectionMsg ();
                ServerSocket.SendToAllClients (msg.ToBytes ());
            }

            catch (Exception ex)
            {
                Print (string.Format ("Exception: {0}", ex.Message));
            }
        }

        private void DisconnectButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                DisconnectMsg msg = new DisconnectMsg ();
                ServerSocket.SendToAllClients (msg.ToBytes ());
            }

            catch (Exception ex)
            {
                Print (string.Format ("Exception: {0}", ex.Message));
            }
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
