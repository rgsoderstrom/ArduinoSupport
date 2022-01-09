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

            SendProfileButton.IsEnabled = true;

            if (data.readyForMessages == 0) messageQueue.ArduinoNotReady ();
            else                            messageQueue.ArduinoReady ();

            //
            // enable/disable buttons based on status
            //
            RunProfileButton.IsEnabled = data.readyToRun       != 0 ? true : false;
            SlowStopButton.IsEnabled   = data.motorsRunning    != 0 ? true : false;
            FastStopButton.IsEnabled   = data.motorsRunning    != 0 ? true : false;

            //
            // update OMI status indicators
            //
            ReadyCommunicateEllipse.Fill = data.readyForMessages != 0 ? Brushes.Green : Brushes.White;
            ReadyRunEllipse.Fill         = data.readyToRun       != 0 ? Brushes.Green : Brushes.White;
            MotorsRunningEllipse.Fill    = data.motorsRunning    != 0 ? Brushes.Green : Brushes.White;
        }

        //*******************************************************************************************************

        private void EncoderCountsMessageHandler (byte [] msgBytes)
        {
            EncoderCountsMessage msg = new EncoderCountsMessage (msgBytes);

            ExtractEncoderCounts (msg);

            if (msg.More)
            {
                SendNextCollectionMsg msg2 = new SendNextCollectionMsg ();
                messageQueue.AddMessage (msg2.ToBytes ());
            }

            else
            {
                PlotSpeeds ();
            }
        }

        //*******************************************************************************************************
        //*******************************************************************************************************

        List<EncoderCountsMessage.Batch.Sample> history = new List<EncoderCountsMessage.Batch.Sample> ();

        private void ExtractEncoderCounts (EncoderCountsMessage msg)
        {
            for (int i = 0; i<msg.data.put; i++)
            {
                history.Add (msg.data.counts [i]);
            }
        }

        List<double> VelTimes = new List<double> ();
        List<double> Vel1 = new List<double> ();
        List<double> Vel2 = new List<double> ();

        private void PlotSpeeds ()
        {
            //Print (history.Count.ToString () + " samples");

            //PlotArea.Clear ();
            PlotArea.RectangularGridOn = true;

            if (history.Count < 2) return;

            VelTimes.Clear ();
            Vel1.Clear ();
            Vel2.Clear ();

            for (int i = 0; i<history.Count; i++)
            {
                VelTimes.Add (i * 0.1);

               

                Vel1.Add ((sbyte) history [i].enc1 / 6);
                Vel2.Add ((sbyte) history [i].enc2 / 6);
            }




            List<Point> points1 = new List<Point> ();
            List<Point> points2 = new List<Point> ();

            for (int i = 0; i<VelTimes.Count; i++)
            {
                points1.Add (new Point (VelTimes [i], Vel1 [i]));
                points2.Add (new Point (VelTimes [i], Vel2 [i]));
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

    }
}


