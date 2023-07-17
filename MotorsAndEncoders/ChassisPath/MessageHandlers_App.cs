using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Windows.Media;

using System.Threading.Tasks;
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

        private void StatusMessageHandler (byte[] msgBytes)
        {
            StatusMessage msg = new StatusMessage (msgBytes);
            StatusMessage.StatusData data = msg.data;

            //Print ("Status from " + msg.data.Name);
            //Print (msg.ToString ());

            //
            // set message queue status
            //
            if (data.readyForMessages == 0) messageQueue.ArduinoNotReady ();
            else                            messageQueue.ArduinoReady ();

            //
            // enable/disable buttons based on status
            //
            if (data.readyForMessages == 0)
            {
                SendProfileButton.IsEnabled = false;
                RunProfileButton.IsEnabled = false;
                //SlowStopButton.IsEnabled = false;

                ClearRemoteProfileButton.IsEnabled = false;
                TransferProfileButton.IsEnabled = false;
                RunProfileButton.IsEnabled = false;
                //StopProfileButton.IsEnabled = false;
                //SlowStopButton.IsEnabled = false;
                FastStopButton.IsEnabled = false;
            }

            else
            {
                ClearRemoteProfileButton.IsEnabled = true;

                SendProfileButton.IsEnabled = true;
                RunProfileButton.IsEnabled = data.readyToRun       != 0 ? true : false;
                //SlowStopButton.IsEnabled   = data.motorsRunning    != 0 ? true : false;
                FastStopButton.IsEnabled = true;
            }




            //
            // update OMI status indicators
            //
            ReadyCommunicateEllipse.Fill = data.readyForMessages != 0 ? Brushes.Green : Brushes.White;
            ReadyRunEllipse.Fill         = data.readyToRun       != 0 ? Brushes.Green : Brushes.White;
            MotorsRunningEllipse.Fill    = data.motorsRunning    != 0 ? Brushes.Green : Brushes.White;
        }

        //*******************************************************************************************************

        //List<EncoderCountsMessage.Batch.Sample> encoderCounts = new List<EncoderCountsMessage.Batch.Sample> ();
        List<Point> points1 = new List<Point> ();
        List<Point> points2 = new List<Point> ();
        LineView line1 = null;
        LineView line2 = null;

        double encoderCountsTime = 0;

        private void ClearEncoderCounts ()
        {
            //encoderCounts.Clear ();
            points1.Clear ();
            points2.Clear ();
            encoderCountsTime = 0;    
            SpeedProfilePlot.RectangularGridOn = true;
        }

        private void EncoderCountsMessageHandler (byte [] msgBytes)
        {
            EncoderCountsMessage msg = new EncoderCountsMessage (msgBytes);

            PlotEncoderCounts (msg);

            if (msg.data.lastBatch == 0)
            {
                SendCountsMsg msg2 = new SendCountsMsg ();
                messageQueue.AddMessage (msg2.ToBytes ());
            }
        }

        //*******************************************************************************************************
        //*******************************************************************************************************

        private void PlotEncoderCounts (EncoderCountsMessage msg)
        {
            List<double> VelTimes = new List<double> ();
            List<double> Vel1 = new List<double> ();
            List<double> Vel2 = new List<double> ();

            for (int i = 0; i<msg.data.put; i++)
            {
                VelTimes.Add (encoderCountsTime + i / 20.0); // 20 samples per second

                Vel1.Add ((sbyte) msg.data.counts [i].enc1);
                Vel2.Add ((sbyte) msg.data.counts [i].enc2);
            }

            encoderCountsTime += msg.data.put / 20.0;

            for (int i = 0; i<VelTimes.Count; i++)
            {
                points1.Add (new Point (VelTimes [i], Vel1 [i]));
                points2.Add (new Point (VelTimes [i], -1 * Vel2 [i]));
            }

            if (line1 != null) SpeedProfilePlot.Remove (line1);
            if (line2 != null) SpeedProfilePlot.Remove (line2);

            line1 = new LineView (points2);
            line2 = new LineView (points1);

            line1.Color = Brushes.Red;
            line2.Color = Brushes.Green;

            SpeedProfilePlot.Plot (line1);
            SpeedProfilePlot.Plot (line2);
        }
    }
}


