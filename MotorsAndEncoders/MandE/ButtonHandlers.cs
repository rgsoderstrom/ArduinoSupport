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
    public partial class MainWindow
    {
        //*******************************************************************************************************

        private void ClearProfileButton_Click (object sender, RoutedEventArgs e)
        {
            foreach (var child in Motor1_Grid.Children)
            {
                if (child != null)
                {
                    if (child is TextBox)
                        (child as TextBox).Text = "";
                }
            }

            foreach (var child in Motor2_Grid.Children)
            {
                if (child != null)
                {
                    if (child is TextBox)
                        (child as TextBox).Text = "";
                }
            }

            (Motor1_Grid.Children [0] as TextBox).Text = "0";
            (Motor1_Grid.Children [1] as TextBox).Text = "0";
            (Motor2_Grid.Children [0] as TextBox).Text = "0";
            (Motor2_Grid.Children [1] as TextBox).Text = "0";
        }

        //*******************************************************************************************************

        LineView Motor1SpeedProfileView = null;
        LineView Motor2SpeedProfileView = null;

        private void PlotProfileButton_Click (object sender, RoutedEventArgs e)
        {
            List<int>    speed1    = new List<int> (); // commanded speeds and durations read from OMI
            List<double> duration1 = new List<double> ();

            List<int>    speed2    = new List<int> (); 
            List<double> duration2 = new List<double> ();

            ReadProfileGrid (Motor1_Grid.Children, ref speed1, ref duration1);
            ReadProfileGrid (Motor2_Grid.Children, ref speed2, ref duration2);

            List<Point> expectedMotor1Profile = GenerateExpectedProfile (speed1, duration1);
            List<Point> expectedMotor2Profile = GenerateExpectedProfile (speed2, duration2);

            int Motor1Count = IntegrateProfile (expectedMotor1Profile);
            int Motor2Count = IntegrateProfile (expectedMotor2Profile);

            Print (string.Format ("Left total counts =  {0}", Motor1Count));
            Print (string.Format ("Right total counts =  {0}", Motor2Count));

            if (Motor1SpeedProfileView != null)
                PlotAreaLeft.Remove (Motor1SpeedProfileView);

            if (Motor2SpeedProfileView != null)
                PlotAreaRight.Remove (Motor2SpeedProfileView);

            Motor1SpeedProfileView = new LineView (expectedMotor1Profile);
            Motor1SpeedProfileView.Color = Brushes.Red;
            PlotAreaLeft.Plot (Motor1SpeedProfileView);

            Motor2SpeedProfileView = new LineView (expectedMotor2Profile);
            Motor2SpeedProfileView.Color = Brushes.Green;
            PlotAreaRight.Plot (Motor2SpeedProfileView);

            PlotAreaLeft.RectangularGridOn = PlotAreaRight.RectangularGridOn = true;
            PlotAreaLeft.AxesTight = PlotAreaRight.AxesTight = true;
            PlotAreaLeft.GetAxes (out double x1, out double x2, out double y1, out double y2);
            y1 = -127; y2 = 127; 
            PlotAreaLeft.SetAxes (x1, x2, y1, y2);
            PlotAreaRight.SetAxes (x1, x2, y1, y2);
        }

        //*******************************************************************************************************

        private void ClearPlotsButton_Click (object sender, RoutedEventArgs e)
        {
            PlotAreaLeft.Clear ();
            PlotAreaRight.Clear ();
        }

        //*******************************************************************************************************

        private void SendProfileButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                List<int> speed1 = new List<int> (); // speeds and durations read from OMI
                List<double> duration1 = new List<double> ();

                List<int> speed2 = new List<int> (); // speeds and durations read from OMI
                List<double> duration2 = new List<double> ();

                ReadProfileGrid (Motor1_Grid.Children, ref speed1, ref duration1);
                ReadProfileGrid (Motor2_Grid.Children, ref speed2, ref duration2);

                for (short i = 0; i<speed1.Count; i++)
                {
                    SpeedProfileSegmentMsg msg = new SpeedProfileSegmentMsg (i, 1, (short)speed1 [i], (short)(duration1 [i] * 10));
                    messageQueue.AddMessage (msg.ToBytes ());
                }

                for (short i = 0; i<speed2.Count; i++)
                {
                    SpeedProfileSegmentMsg msg = new SpeedProfileSegmentMsg (i, 2, (short)speed2 [i], (short)(duration2 [i] * 10));
                    messageQueue.AddMessage (msg.ToBytes ());
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
            try
            {
                SlowStopMotorsMsg msg = new SlowStopMotorsMsg ();
                messageQueue.AddMessage (msg.ToBytes ());
            }

            catch (Exception ex)
            {
                Print (string.Format ("Exception: {0}", ex.Message));
            }
        }

        private void FastStopButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
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
                encoderCounts.Clear ();

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
