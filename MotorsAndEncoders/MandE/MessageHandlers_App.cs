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

namespace ShaftEncoders
{
    public partial class MainWindow
    {
        //*******************************************************************************************************

        private void StatusMessageHandler (byte[] msgBytes)
        {
            StatusMessage msg = new StatusMessage (msgBytes);
            StatusMessage.StatusData data = msg.data;

            Print ("Status from " + msg.data.Name);
            Print (msg.ToString ());

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
                SlowStopButton.IsEnabled = false;

                ClearRemoteProfileButton.IsEnabled = false;
                TransferProfileButton.IsEnabled = false;
                RunProfileButton.IsEnabled = false;
                //StopProfileButton.IsEnabled = false;
                SlowStopButton.IsEnabled = false;
                FastStopButton.IsEnabled = false;
            }

            else
            {
                ClearRemoteProfileButton.IsEnabled = true;

                SendProfileButton.IsEnabled = true;
                RunProfileButton.IsEnabled = data.readyToRun       != 0 ? true : false;
                SlowStopButton.IsEnabled   = data.motorsRunning    != 0 ? true : false;
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

        List<EncoderCountsMessage.Batch.Sample> encoderCounts = new List<EncoderCountsMessage.Batch.Sample> ();

        private void EncoderCountsMessageHandler (byte [] msgBytes)
        {
            EncoderCountsMessage msg = new EncoderCountsMessage (msgBytes);

            //for (int i=0; i<16; i++)
            //{
            //    string cbuf = string.Format ("{0}, {1}", (int) msg.data.counts [i].enc1, (int) msg.data.counts [i].enc2);
            //    Print (cbuf);
            //}

            ExtractEncoderCounts (msg);

            if (msg.data.lastBatch != 0 || encoderCounts.Count > 2000)
            {
                PlotSpeeds ();
                encoderCounts.Clear ();
            }

            else
            {
                SendCountsMsg msg2 = new SendCountsMsg ();
                messageQueue.AddMessage (msg2.ToBytes ());
            }
        }

        //*******************************************************************************************************
        //*******************************************************************************************************

        private void ExtractEncoderCounts (EncoderCountsMessage msg)
        {
            for (int i = 0; i<msg.data.put; i++)
            {
                encoderCounts.Add (msg.data.counts [i]);
            }
        }

        List<double> VelTimes = new List<double> ();
        List<double> Vel1 = new List<double> ();
        List<double> Vel2 = new List<double> ();

        private void PlotSpeeds ()
        {
            int enc1Total = 0, enc2Total = 0;

            for (int i = 0; i<encoderCounts.Count; i++)
            {
                enc1Total += (sbyte) encoderCounts [i].enc1;
                enc2Total += (sbyte) encoderCounts [i].enc2;
            }


            Print (encoderCounts.Count.ToString () + " samples");
            Print (string.Format ("{0}, {1} total counts each", enc1Total, enc2Total));


            //PlotArea.Clear ();
            PlotAreaLeft.RectangularGridOn = true;
            PlotAreaRight.RectangularGridOn = true;

            if (encoderCounts.Count < 2) return;

            VelTimes.Clear ();
            Vel1.Clear ();
            Vel2.Clear ();

            for (int i = 0; i<encoderCounts.Count; i++)
            {
                VelTimes.Add (i / 20.0); // 20 samples per second

                Vel1.Add ((sbyte) encoderCounts [i].enc1);
                Vel2.Add ((sbyte) encoderCounts [i].enc2);
            }

            List<Point> points1 = new List<Point> ();
            List<Point> points2 = new List<Point> ();

            for (int i = 0; i<VelTimes.Count; i++)
            {
                points1.Add (new Point (VelTimes [i], Vel1 [i]));
                points2.Add (new Point (VelTimes [i], -1 * Vel2 [i]));
            }

            LineView line1 = new LineView (points2);
            LineView line2 = new LineView (points1);
            //line1.Size = 0.1;
            line1.Color = Brushes.Green;
            line2.Color = Brushes.Red;

            PlotAreaLeft.Plot (line1);
            PlotAreaRight.Plot (line2);

            //foreach (Point pt in points1)
            // Print (pt.X.ToString () + ", " + pt.Y.ToString ());

            //PlotArea.GetAxes ()
        }

    }
}


