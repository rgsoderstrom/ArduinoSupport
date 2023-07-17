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

namespace ChassisPath
{
    public partial class MainWindow
    {
        //*******************************************************************************************************
        //
        // ClearProfileButton_Click - 
        //
        private void ClearProfileButton_Click (object sender, RoutedEventArgs e)
        {
            foreach (var child in RadioButton_Grid.Children)
            {
                Leg1None.IsChecked = true;
                Leg2None.IsChecked = true;
                Leg3None.IsChecked = true;
                Leg4None.IsChecked = true;
                Leg5None.IsChecked = true;

                Leg1None_Click (new object (), new RoutedEventArgs ());
                Leg2None_Click (new object (), new RoutedEventArgs ());
                Leg3None_Click (new object (), new RoutedEventArgs ());
                Leg4None_Click (new object (), new RoutedEventArgs ());
                Leg5None_Click (new object (), new RoutedEventArgs ());
            }

            foreach (var child in TextBox_Grid.Children)
            {
                if (child is TextBox tb) 
                    tb.Text = "";
            }
        }

        //*******************************************************************************
        //
        // PlotProfileButton_Click
        //
        private void PlotProfileButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                // clear any old plotted data
                ExpectedPathPlot.Clear ();
                ExpectedPathPlot.AxesEqual = true;
                ExpectedPathPlot.RectangularGridOn = true;

                ClearEncoderCounts ();

                // read grid of requested course segments
                List<Chassis.RequestedCourseLeg> RequestedCourse = ReadRequestedCourse ();

                // Construct chassis instance to follow that course
                MobileChassis = new Chassis (new Point (0, 0), 0, RequestedCourse, Print);

                // Draw chassis symbol at initial position
                ExpectedPathPlot.Plot (MobileChassis.SymbolAtStart);

                // Draw predicted positions
                LineView lv = new LineView (MobileChassis.PathPoints);
                ExpectedPathPlot.Plot (lv);

                // draw calculated speed profiles
                SpeedProfilePlot.Clear ();
                SpeedProfilePlot.RectangularGridOn = true;

                SpeedProfile speeds = MobileChassis.SpeedProfile;

                if (speeds != null)
                {
                    LineView left = new LineView (speeds.LeftPlotData);
                    left.Color = Brushes.Red;
                    SpeedProfilePlot.Plot (left);

                    LineView right = new LineView (speeds.RightPlotData);
                    right.Color = Brushes.Green;
                    SpeedProfilePlot.Plot (right);
                }
            }

            catch (Exception ex)
            {
                Print ("Exception: " + ex.Message);
            }
        }

        //*******************************************************************************************************

        private void ClearPlotsButton_Click (object sender, RoutedEventArgs e)
        {
            ExpectedPathPlot.Clear ();
            SpeedProfilePlot.Clear ();
        }

        //*******************************************************************************************************

        private void SendProfileButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                EventLog.WriteLine ("SendProfileButton_Click");

                ClearRemoteProfileButton_Click (null, null); 

                List<SpeedProfile.SpeedSegment> speeds = MobileChassis.SpeedProfile.SpeedSegments;

                for (short i=0; i<speeds.Count; i++)
                {
                    SpeedProfileSegmentMsg msg1 = new SpeedProfileSegmentMsg (i, 1, (short) (speeds [i].leftSpeed * 115.0 / 20), (short)(speeds [i].duration * 10));
                    messageQueue.AddMessage (msg1.ToBytes ());
               
                    SpeedProfileSegmentMsg msg2 = new SpeedProfileSegmentMsg (i, 2, (short) (speeds [i].rightSpeed * 115.0 / 20), (short)(speeds [i].duration * 10));
                    messageQueue.AddMessage (msg2.ToBytes ());
                }

                ClearRemoteProfileButton.IsEnabled = true;
                TransferProfileButton.IsEnabled = true;
            }

            catch (Exception ex)
            {
                Print (string.Format ("Exception: {0}", ex.Message));
            }
        }

        private void ClearRemoteProfileButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                EventLog.WriteLine ("ClearRemoteProfileButton_Click");

                ClearSpeedProfileMsg msg = new ClearSpeedProfileMsg ();
                messageQueue.AddMessage (msg.ToBytes ());
                TransferProfileButton.IsEnabled = false;
            }

            catch (Exception ex)
            {
                Print (string.Format ("Exception: {0}", ex.Message));
            }
        }

        private void TransferProfileButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                EventLog.WriteLine ("TransferProfileButton_Click");

                TransferSpeedProfileMsg msg = new TransferSpeedProfileMsg ();
                messageQueue.AddMessage (msg.ToBytes ());

                //RunProfileButton.IsEnabled = true;
            }

            catch (Exception ex)
            {
                Print (string.Format ("Exception: {0}", ex.Message));
            }
        }

        private void RunProfileButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                EventLog.WriteLine ("RunProfileButton_Click");

                RunMotorsMsg msg = new RunMotorsMsg ();
                messageQueue.AddMessage (msg.ToBytes ());

                StartCollButton_Click (sender, e);
            }

            catch (Exception ex)
            {
                Print (string.Format ("Exception: {0}", ex.Message));
            }
        }

        //private void StopProfileButton_Click (object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        StopProfileMsg msg = new StopProfileMsg ();
        //        messageQueue.AddMessage (msg.ToBytes ());
        //    }

        //    catch (Exception ex)
        //    {
        //        Print (string.Format ("Exception: {0}", ex.Message));
        //    }
        //}

        private void SlowStopButton_Click (object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    SlowStopMotorsMsg msg = new SlowStopMotorsMsg ();
            //    messageQueue.AddMessage (msg.ToBytes ());
            //}

            //catch (Exception ex)
            //{
            //    Print (string.Format ("Exception: {0}", ex.Message));
            //}
        }

        private void FastStopButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                EventLog.WriteLine ("FastStopButton_Click");

                FastStopMotorsMsg msg = new FastStopMotorsMsg ();
                messageQueue.AddMessage (msg.ToBytes ());
            }

            catch (Exception ex)
            {
                Print (string.Format ("Exception: {0}", ex.Message));
            }
        }

        //*******************************************************************************************************
                     
        private void StartCollButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                EventLog.WriteLine ("StartCollButton_Click");

                ClearEncoderCounts ();

                StartCollectionMsg msg = new StartCollectionMsg ();
                messageQueue.AddMessage (msg.ToBytes ());
            }

            catch (Exception ex)
            {
                Print (string.Format ("Exception: {0}", ex.Message));
            }
        }


        private void StopCollButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                EventLog.WriteLine ("StopCollButton_Click");

                StopCollectionMsg msg = new StopCollectionMsg ();
                messageQueue.AddMessage (msg.ToBytes ());
            }

            catch (Exception ex)
            {
                Print (string.Format ("Exception: {0}", ex.Message));
            }
        }



        private void SendCountsButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                EventLog.WriteLine ("SendCountsButton_Click");

                SendCountsMsg msg = new SendCountsMsg ();
                messageQueue.AddMessage (msg.ToBytes ());
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
        //private void ClearCollectionButton_Click (object sender, RoutedEventArgs e)
        //{
            //try
            //{
            //    ClearCollectionMsg msg = new ClearCollectionMsg ();
            //     (msg.ToBytes ());
            //}

            //catch (Exception ex)
            //{
            //    Print (string.Format ("Exception: {0}", ex.Message));
            //}
        //}

        private void DisconnectButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                DisconnectMsg msg = new DisconnectMsg ();
                messageQueue.AddMessage (msg.ToBytes ());
            }

            catch (Exception ex)
            {
                Print (string.Format ("Exception: {0}", ex.Message));
            }
        }
    }
}
