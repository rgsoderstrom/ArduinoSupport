using System;
using System.Collections.Generic;
using System.Windows;

using Common;
using Plot2D_Embedded;

using FixedPointLib;
//using FixedPt = FixedPointLib.FixedPoint_1_5_10;
using FixedPt = FixedPointLib.FixedPoint_1_7_24;

using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics;

#pragma warning disable IDE0051
#pragma warning disable IDE0052

namespace FixedPointTests
{

    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow ()
        {
            InitializeComponent ();
            EventLog.Open (@"..\..\log.txt");
        }

        //*********************************************************************************

        private readonly int fftSize = 4096;
        private readonly double SampleRate = 10000; // samples/second

        private readonly double MixerFreq = 4020;  // unit amplitude

        private readonly double SignalFreq = 4000;
        private readonly double SignalAmpl = 102.3;  // scaled A/D output amplitude

        private readonly List<FixedPt> Signal   = new List<FixedPt> ();
        private readonly List<FixedPt> LocalOsc = new List<FixedPt> ();
        private readonly List<FixedPt> Mixed    = new List<FixedPt> ();

        private FirFilter firFilter;
        private List<FixedPt>  Filtered;

        readonly List<Point> MagSpectrum = new List<Point> ();

        //*********************************************************************************

        private void DoCalculations ()
        {
            try
            {
                firFilter = new FirFilter (SampleRate, 1800); // 30);

                double dt = 1 / SampleRate;
                double t = 0;

                //int PrintCount = 5;

                while (Signal.Count < fftSize + firFilter.Length)
                {
                    double s = SignalAmpl * Math.Cos (2 * Math.PI * SignalFreq * t);
                    Signal.Add (new FixedPt (s));
                    
                    double m = Math.Sin (2 * Math.PI * MixerFreq * t);
                    LocalOsc.Add (new FixedPt (m));

                    //if (PrintCount-- > 0)
                    //{
                    //    FixedPt fp = new FixedPt (m);
                    //    Console.WriteLine (string.Format ("0x{0:x8}", fp.AsInt));
                    //}


                    t+=dt;
                }

                for (int i = 0; i<Signal.Count; i++)
                {
                    FixedPt mix = Signal [i] * LocalOsc [i];
                    Mixed.Add (mix);
                }

                Filtered = firFilter.Run (Mixed);

                RunFFT (Filtered, SampleRate, MagSpectrum, true);


                EventLog.WriteLine ("Initial calculations complete");
            }

            catch (Exception ex)
            {
                EventLog.WriteLine ("Exception in DoCalculations: " + ex.Message);
            }
        }

        //*********************************************************************************

        private void DoPlots ()
        {
            List<double> mixed = new List<double> ();
            List<double> filtered = new List<double> ();

            foreach (FixedPt fp in Mixed)
                mixed.Add (fp.AsDouble);

            foreach (FixedPt fp in Filtered)
                filtered.Add (fp.AsDouble);

            //LineView lv = new LineView (mixed);
            //lv.Color = Brushes.Red;
            //PlotArea.Plot (lv);

            //LineView lv2 = new LineView (filtered);
            //lv2.Color = Brushes.Green;
            //PlotArea.Plot (lv2);

            LineView lv = new LineView (MagSpectrum);
            PlotArea.Plot (lv);

            PlotArea.RectangularGridOn = true;
        }

        //*********************************************************************************

        private void Window_Loaded (object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe) EventLog.WriteLine (fe.Name + " loaded");
            DoCalculations ();
        }

        private void PlotArea_Loaded (object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe) EventLog.WriteLine (fe.Name + " loaded");
            DoPlots ();
        }

        private void TextArea_Loaded (object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe) EventLog.WriteLine (fe.Name + " loaded");
        }

        private void ButtonArea_Loaded (object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe) EventLog.WriteLine (fe.Name + " loaded");
        }

        //**********************************************************************************

       
        void RunFFT (List<FixedPt> timeSignal, double sampleRate, List<Point> magSpectum, bool HammWindow = false)
        {
            try
            { 
                int sampleCount = fftSize;
                int pad = sampleCount.IsEven () ? 2 : 1;
                int workBufferSize = sampleCount + pad;

                double [] workBuffer = new double [workBufferSize]; // before FFT: input samples
                                                                    // after FFT: half of complex spectrum
                int get = timeSignal.Count - fftSize;

                // copy input to WorkBuffer. the "pad" words were initialized to 0 and will remain 0
                int ii;

                for (ii=0; ii<sampleCount; ii++)
                    workBuffer [ii] = timeSignal [get + ii].AsDouble;

                // optional windowing
                if (HammWindow == true)
                {
                    double [] Hamm = MathNet.Numerics.Window.Hamming (sampleCount);

                    for (int i = 0; i<fftSize; i++)
                        workBuffer [i] *= Hamm [i];
                }

                Fourier.ForwardReal (workBuffer, fftSize, FourierOptions.Matlab); // .NoScaling);

                double [] fftReal = new double [fftSize]; // complex spectrum unpacked and reflected into these
                double [] fftImag = new double [fftSize];

                int put = 0;

                for (int k = 0; k<workBuffer.Length; k+=2, put++)
                {
                    fftReal [put] = workBuffer [k];
                    fftImag [put] = workBuffer [k+1];
                }

                put = fftReal.Length - 1;

                for (int k = 2; k<workBuffer.Length; k+=2, put--)
                {
                    fftReal [put] = workBuffer [k];
                    fftImag [put] = workBuffer [k+1] * -1;
                }

                double [] frequencyScale = Fourier.FrequencyScale (fftSize, sampleRate);
                double [] magnitude = new double [fftSize];

                for (int i = 0; i<fftSize; i++)
                    magnitude [i] = 10 * Math.Log10 (Math.Sqrt (fftReal [i] * fftReal [i] + fftImag [i] * fftImag [i]));

                int L2 = 1 + fftSize / 2;

                for (int i = L2; i<fftSize; i++)
                    magSpectum.Add (new Point (frequencyScale [i], magnitude [i]));

                for (int i = 0; i<L2; i++)
                    magSpectum.Add (new Point (frequencyScale [i], magnitude [i]));
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception in RunFFT: " + ex.Message);
            }
        }



    }
}
