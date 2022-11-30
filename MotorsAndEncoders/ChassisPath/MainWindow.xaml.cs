
/*
    MainWindow for ChassisPath
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
//using System.Diagnostics;

namespace ChassisPath
{
    public partial class MainWindow : Window
    {
        SocketLib.TcpServer ServerSocket = null;
        MessageQueue messageQueue; // messages to Arduino pass through here

        System.Timers.Timer KeepAliveTimer = new System.Timers.Timer (20000); // milliseconds

        // only the thread that created WPF objects can access them. others must use Invoke () to
        // run a task on that thread. Its ID stored here
        readonly int WpfThread;

        Chassis MobileChassis = null;
        
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

                //KeepAliveTimer.Elapsed += KeepAliveTimer_Elapsed;
                KeepAliveTimer.Enabled = true;
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in MainWindow ctor: {0}", ex.Message));
            }
        }

        private void Window_Loaded (object sender, RoutedEventArgs e)
        {
            try
            {
                Leg1Speed.Text = "11.5";   // speed, ips
                Leg1Length.Text = "72";  // distance, inches
                Leg1Straight.IsChecked = true;
                Leg1Straight_Click (new object (), new RoutedEventArgs ());

                //Leg1None.IsChecked = true;
                //Leg1None_Click (new object (), new RoutedEventArgs ());

                Leg2Radius.Text = "12"; // inches
                Leg2Angle.Text = "180"; // angle, left pos, right neg
                Leg2Curve.IsChecked = true;
                Leg2Curve_Click (new object (), new RoutedEventArgs ());

                //Leg2None.IsChecked = true;
                //Leg2None_Click (new object (), new RoutedEventArgs ());

                Leg3Speed.Text = "13";
                Leg3Length.Text = "36";
                Leg3Straight.IsChecked = true;
                Leg3Straight_Click (new object (), new RoutedEventArgs ());

                //Leg3None.IsChecked = true;
                //Leg3None_Click (new object (), new RoutedEventArgs ());

                Leg4Radius.Text = "12"; // inches
                Leg4Angle.Text = "270"; // angle, left pos, right neg
                Leg4Curve.IsChecked = true;
                Leg4Curve_Click (new object (), new RoutedEventArgs ());

                Leg5Speed.Text = "20";
                Leg5Length.Text = "26";
                Leg5Straight.IsChecked = true;
                Leg5Straight_Click (new object (), new RoutedEventArgs ());
                
                ExpectedPathPlot.DataAreaTitle = "Chassis Path";
                SpeedProfilePlot.DataAreaTitle = "Speed Profiles";
                ExpectedPathPlot.MouseEnabled = false;
                SpeedProfilePlot.MouseEnabled = false;
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception in Main WindowLoaded: {0}", ex.Message));
            }
        }


        static List<Chassis.PathSegmentType> segmentTypes = new List<Chassis.PathSegmentType> {new Chassis.PathSegmentType (),
                                                                                               new Chassis.PathSegmentType (),
                                                                                               new Chassis.PathSegmentType (),
                                                                                               new Chassis.PathSegmentType (),
                                                                                               new Chassis.PathSegmentType (),
        };

        private void Leg1Straight_Click (object sender, RoutedEventArgs e) {ShowText ("10", "11"); HideText ("12", "13"); if (segmentTypes != null) segmentTypes [0] = Chassis.PathSegmentType.Straight;}
        private void Leg1Curve_Click    (object sender, RoutedEventArgs e) {HideText ("10", "11"); ShowText ("12", "13"); if (segmentTypes != null) segmentTypes [0] = Chassis.PathSegmentType.Curved;}        
        private void Leg1None_Click     (object sender, RoutedEventArgs e) {HideText ("10", "11"); HideText ("12", "13"); if (segmentTypes != null) segmentTypes [0] = Chassis.PathSegmentType.Off;}

        private void Leg2Straight_Click (object sender, RoutedEventArgs e) {ShowText ("20", "21"); HideText ("22", "23"); if (segmentTypes != null) segmentTypes [1] = Chassis.PathSegmentType.Straight;}
        private void Leg2Curve_Click    (object sender, RoutedEventArgs e) {HideText ("20", "21"); ShowText ("22", "23"); if (segmentTypes != null) segmentTypes [1] = Chassis.PathSegmentType.Curved;}
        private void Leg2None_Click     (object sender, RoutedEventArgs e) {HideText ("20", "21"); HideText ("22", "23"); if (segmentTypes != null) segmentTypes [1] = Chassis.PathSegmentType.Off;}

        private void Leg3Straight_Click (object sender, RoutedEventArgs e) {ShowText ("30", "31"); HideText ("32", "33"); if (segmentTypes != null) segmentTypes [2] = Chassis.PathSegmentType.Straight;}
        private void Leg3Curve_Click    (object sender, RoutedEventArgs e) {HideText ("30", "31"); ShowText ("32", "33"); if (segmentTypes != null) segmentTypes [2] = Chassis.PathSegmentType.Curved;}
        private void Leg3None_Click     (object sender, RoutedEventArgs e) {HideText ("30", "31"); HideText ("32", "33"); if (segmentTypes != null) segmentTypes [2] = Chassis.PathSegmentType.Off;}

        private void Leg4Straight_Click (object sender, RoutedEventArgs e) {ShowText ("40", "41"); HideText ("42", "43"); if (segmentTypes != null) segmentTypes [3] = Chassis.PathSegmentType.Straight;}
        private void Leg4Curve_Click    (object sender, RoutedEventArgs e) {HideText ("40", "41"); ShowText ("42", "43"); if (segmentTypes != null) segmentTypes [3] = Chassis.PathSegmentType.Curved;}
        private void Leg4None_Click     (object sender, RoutedEventArgs e) {HideText ("40", "41"); HideText ("42", "43"); if (segmentTypes != null) segmentTypes [3] = Chassis.PathSegmentType.Off;}

        private void Leg5Straight_Click (object sender, RoutedEventArgs e) {ShowText ("50", "51"); HideText ("52", "53"); if (segmentTypes != null) segmentTypes [4] = Chassis.PathSegmentType.Straight;}
        private void Leg5Curve_Click    (object sender, RoutedEventArgs e) {HideText ("50", "51"); ShowText ("52", "53"); if (segmentTypes != null) segmentTypes [4] = Chassis.PathSegmentType.Curved;}
        private void Leg5None_Click     (object sender, RoutedEventArgs e) {HideText ("50", "51"); HideText ("52", "53"); if (segmentTypes != null) segmentTypes [4] = Chassis.PathSegmentType.Off;}


        private void ShowText (string tag1, string tag2)
        {
            UIElementCollection children = TextBox_Grid.Children;

            foreach (UIElement ui in children)
            {
                if (ui is TextBox tb)
                {
                    string tag = tb.Tag as string;

                    if (tag == tag1) tb.Foreground = Brushes.Black;
                    if (tag == tag2) tb.Foreground = Brushes.Black;
                }
            }
        }

        private void HideText (string tag1, string tag2)
        {
            UIElementCollection children = TextBox_Grid.Children;

            foreach (UIElement ui in children)
            {
                if (ui is TextBox tb)
                {
                    string tag = tb.Tag as string;

                    if (tag == tag1) tb.Foreground = Brushes.Transparent;
                    if (tag == tag2) tb.Foreground = Brushes.Transparent;
                }
            }
        }

        //*******************************************************************************************************
        //*******************************************************************************************************
        //*******************************************************************************************************
        
        static int localLineNumber = 1;
        object LocalTextBoxLock = new object ();

        internal static List<Chassis.PathSegmentType> SegmentTypes { get => segmentTypes; set => segmentTypes=value; }

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
                Dispatcher.BeginInvoke ((SocketLib.PrintCallback) AddTextToLocalTextBox, str);
            }
        }

        //*******************************************************************************************************

        //static int remoteLineNumber = 1;
        //object RemoteTextBoxLock = new object ();

        //void AddTextToRemoteTextBox (string str)
        //{
        //    EventLog.WriteLine (str);

        //    lock (RemoteTextBoxLock)
        //    {
        //        RemoteTextDisplay.Text += string.Format ("{0}: ", remoteLineNumber++);
        //        RemoteTextDisplay.Text += str;
        //        RemoteTextDisplay.Text += "\n";
        //    }

        //    RemoteTextDisplay.ScrollToEnd ();
        //}

        //public void RemotePrint (string str)
        //{
        //    int callingThread = Thread.CurrentThread.ManagedThreadId;

        //    if (callingThread == WpfThread)
        //    {
        //        AddTextToRemoteTextBox (str);
        //    }
        //    else
        //    {
        //        Dispatcher.BeginInvoke ((SocketLib.PrintCallback) AddTextToRemoteTextBox, str);
        //    }
        //}
    }
}
