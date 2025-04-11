using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

using MathNet.Filtering.FIR;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics;

//using FixedPt = FixedPointLib.FixedPoint_1_7_24;
using FixedPt = FixedPointLib.FixedPoint_1_5_10;

using Common;
using Plot2D_Embedded;

#pragma warning disable CS0162

namespace FixedPointTests
{
    class FirFilterTest : IFixedPtTest
    {

        private readonly int fftSize = 1024;
        private readonly double InputSampleRate = 10240; // samples/second
        private readonly double cutoff = 3000; // low pass cutoff, Hz

        // frequencies in to FIR filter
        private readonly List<double> frequencies = new List<double> {100 };//, 3000, 4000};//
        private readonly List<double> amplitudes  = new List<double> {1,    1,    1};

        //*********************************************************************************

        private readonly List<double>  InputSignal_dbl   = new List<double> ();
        private readonly List<FixedPt> InputSignal_fixed = new List<FixedPt> ();

        private double []  FilteredSignal_dbl;
        private List<FixedPt> FilteredSignal_fixed;

        private double [] filterCoefficients = null; // common to fixed & double
        private MathNet.Filtering.FIR.OnlineFirFilter firFilter_dbl;
        private FixedPointLib.FirFilter               firFilter_fixed;

        private readonly List<Point> MagSpectrum_dbl   = new List<Point> ();
        private readonly List<Point> MagSpectrum_fixed = new List<Point> ();

        //*********************************************************************************

        private Bare2DPlot plotArea = null;
        public  Bare2DPlot PlotArea
        {
            set {plotArea = value;}
            get {return plotArea;}
        }

        private PrintCallback print = null;
        public  PrintCallback Print
        {
            set {print = value;}
            get {return print;}
        }

        //*********************************************************************************

        public FirFilterTest ()
        {
        }

        //*********************************************************************************

        public void DoCalculations ()
        {
            DoCommonCalculations ();
            DoFloatCalculations ();
            DoFixedCalculations ();
        }

        private void DoCommonCalculations ()
        {
            try
            {
                // filter
                filterCoefficients = MathNet.Filtering.FIR.FirCoefficients.LowPass (InputSampleRate, cutoff);
                Print ("Filter Length = " + filterCoefficients.Length.ToString ());

                // input signal
                double dt = 1 / InputSampleRate;
                double t = 0;

                while (InputSignal_dbl.Count < fftSize + filterCoefficients.Length)
                {
                    double s = 0;

                    for (int i=0; i<frequencies.Count; i++)
                    {
                        double freq = frequencies [i];
                        double ampl = amplitudes [i];
                        s += ampl * Math.Cos (2 * Math.PI * freq * t);
                    }

                    InputSignal_dbl.Add (s);
                    t+=dt;
                }

                Print (InputSignal_dbl.Count + " input samples");
            }

            catch (Exception ex)
            {
                EventLog.WriteLine ("Exception in DoCommonCalculations: " + ex.Message);
            }
        }

        private void DoFloatCalculations ()
        {
            try
            {
                firFilter_dbl = new OnlineFirFilter (filterCoefficients);   
                
                double [] filt = firFilter_dbl.ProcessSamples (InputSignal_dbl.ToArray ());

                FilteredSignal_dbl = new double [fftSize];

                int get = filt.Length - fftSize;

                for (int i=0; i<fftSize; i++)
                    FilteredSignal_dbl [i] = filt [get + i];

                
                
                RunFFT (FilteredSignal_dbl, InputSampleRate, MagSpectrum_dbl, true);

                EventLog.WriteLine ("Float calculations complete");
            }

            catch (Exception ex)
            {
                EventLog.WriteLine ("Exception in DoFloatCalculations: " + ex.Message);
            }
        }

        private void DoFixedCalculations ()
        {
            try
            {
                foreach (double d in InputSignal_dbl)
                    InputSignal_fixed.Add (new FixedPt (d));

                firFilter_fixed = new FixedPointLib.FirFilter (filterCoefficients);
                FilteredSignal_fixed = firFilter_fixed.ProcessSample (InputSignal_fixed);
                
                RunFFT (FilteredSignal_fixed, InputSampleRate, MagSpectrum_dbl, true);

                EventLog.WriteLine ("Fixed calculations complete");
            }

            catch (Exception ex)
            {
                EventLog.WriteLine ("Exception in DoFixedCalculations: " + ex.Message);
            }
        }

        //*********************************************************************************

        public void DoPlots ()
        {
            try
            { 
                LineView lv1;
                LineView lv2;

                // frequency domain plots
                if (false)
                { 
                    lv1 = new LineView (MagSpectrum_dbl);
                    lv2 = new LineView (MagSpectrum_fixed);
                }

                // time domain plots
                else
                {
                    lv1 = new LineView (FilteredSignal_dbl);

                    List<double> filt = new List<double> ();
                    foreach (FixedPt fp in FilteredSignal_fixed) filt.Add (fp.AsDouble);
                    lv2 = new LineView (filt);
                }

                // common
                lv1.Color = Brushes.Red;
                lv2.Color = Brushes.Green;

                PlotArea.Plot (lv1);
                PlotArea.Plot (lv2);
                PlotArea.RectangularGridOn = true;
            }

            catch (Exception ex)
            {
                EventLog.WriteLine ("Exception in DoPlots: " + ex.Message);
                EventLog.WriteLine ("Exception in DoPlots: " + ex.StackTrace);
            }
        }

        //**********************************************************************************
       
        private void RunFFT (List<FixedPt> timeSignal, double sampleRate, List<Point> magSpectum, bool HammWindow = false)
        {
            double [] timeSignal_dbl = new double [timeSignal.Count];

            for (int i=0; i<timeSignal.Count; i++)
                timeSignal_dbl [i] = timeSignal [i].AsDouble;

            RunFFT (timeSignal_dbl, sampleRate, MagSpectrum_fixed, HammWindow);
        }
        
        private void RunFFT (double [] timeSignal, double sampleRate, List<Point> magSpectum, bool HammWindow = false)
        {
            try
            { 
                int sampleCount = fftSize;
                int pad = sampleCount.IsEven () ? 2 : 1;
                int workBufferSize = sampleCount + pad;

                double [] workBuffer = new double [workBufferSize]; // before FFT: input samples
                                                                    // after FFT: half of complex spectrum
                int get = timeSignal.Length - fftSize;

                // copy input to WorkBuffer. the "pad" words were initialized to 0 and will remain 0
                int ii;

                for (ii=0; ii<sampleCount; ii++)
                    workBuffer [ii] = timeSignal [get + ii];

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
                {
                    double mag = Math.Sqrt (fftReal [i] * fftReal [i] + fftImag [i] * fftImag [i]);
                    if (mag < 1e-7) mag = 1e-7;
                    magnitude [i] = 10 * Math.Log10 (mag);
                }

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
