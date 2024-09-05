using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics;

namespace A2D_Tests
{
    internal class SignalProcessing
    {
        //
        // Samples as received
        //      x = sample times, y = amplitude
        //
        private readonly List<Point> inputSamples = new List<Point> ();
        public List<Point> InputSamples {get {return inputSamples;}}

        //
        // Spectrum of samples as received
        //      x = frequency, y = 10*log10 (power)
        //
        private readonly List<Point> inputSpectrum = new List<Point> ();
        public List<Point> InputSpectrum {get {return inputSpectrum;}}

        //
        // Samples windowed and DC removed
        //      x = sample times, y = amplitude
        //
        private readonly List<Point> windowedSamples = new List<Point> ();
        public List<Point> WindowedSamples {get {return windowedSamples;}}

        //
        // Spectrum of windowed samples
        //      x = frequency, y = 10*log10 (power)
        //
        private readonly List<Point> windowedSpectrum = new List<Point> ();
        public List<Point> WindowedSpectrum {get {return windowedSpectrum;}}



        private readonly double inputSampleRate;

        //*******************************************************************************
        //
        // Constructor
        //
        public SignalProcessing (List<double> samp, double sampleRate)
        {
            if (sampleRate <= 0) throw new Exception ("Sample rate must be greater than zero");

            inputSampleRate = sampleRate;

            double t = 0; // running time assigned to each sample
            double dt = 1 / sampleRate;

            for (int i=0; i<samp.Count; i++, t+=dt)
                inputSamples.Add (new Point (t, samp [i]));

            inputSpectrum = DoFFT (InputSamples, inputSampleRate);


            // raised cosine window
            double [] window = new double [InputSamples.Count];
            for (int i = 0; i<samp.Count; i++, t+=dt)
                window [i] = 0.5 * (1 - Math.Cos (2 * Math.PI * i / samp.Count));

            // DC component of input samples
            double sum = 0;

            foreach (Point pt in inputSamples)
                sum += pt.Y;

            double DC = sum / inputSamples.Count;

            t = 0;

            for (int i = 0; i<samp.Count; i++, t+=dt)
                windowedSamples.Add (new Point (t, window [i] * (samp [i] - DC)));

            windowedSpectrum = DoFFT (WindowedSamples, inputSampleRate);
        }

        //**************************************************************************************
        //
        // Processing and display formatting functions
        //

        //
        // FFT
        //
        private List<Point> DoFFT (List<Point> samples, double sampleRate)
        {
            int sampleCount = samples.Count;
            int pad = sampleCount.IsEven () ? 2 : 1;

            double [] fftReal    = new double [sampleCount];
            double [] fftImag    = new double [sampleCount];
            double [] workBuffer = new double [sampleCount + pad]; // before FFT: input signal
                                                                   // after FFT: half of complex spectrum

           for (int i=0; i<sampleCount; i++)
                workBuffer [i] = samples [i].Y;

            Fourier.ForwardReal (workBuffer, sampleCount, FourierOptions.NoScaling);

            int put = 0;
                
            for (int k=0; k<workBuffer.Length; k+=2, put++)
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

            List<Point> results = FormatSpectrum (fftReal, fftImag, sampleRate);
            return results;
        }

        //
        // FormatSpectrum
        //
        private static List<Point> FormatSpectrum (double [] real, double [] imag, double sampleRate)
        {
            int length = real.Length;
            List<Point> results = new List<Point> ();

            //
            // Magnitude spectrum, in dB. Normalized to strongest bin
            //
            double [] magnitudeSpectrum = new double [length];
            double peakMagnitude = 0; // in dB

            for (int i = 0; i<length; i++)
            {   
                double pwr = Utils.PowerSpectrum (real [i], imag [i], length);
                double dB = 10 * Math.Log10 (pwr > 0 ? pwr : 1e-99);
                
                if (i != 0) // ignore DC when looking for strongest bin
                    if (peakMagnitude < dB)
                        peakMagnitude = dB;

                magnitudeSpectrum [i] = dB;
            }

            for (int i = 0; i<length; i++)
                magnitudeSpectrum [i] -= peakMagnitude;

            //
            // Get frequency scale
            //
            double [] frequencyScale = Fourier.FrequencyScale (length, sampleRate);

            //
            // Put magnitude & frequency together in the format expected by plot function
            //
            int L2 = 1 + length / 2;
            int put = 0;

            for (int i = L2; i<length; i++, put++)
            {
                Point pt = new Point (frequencyScale [i], magnitudeSpectrum [i]);
                results.Add (pt);
            }

            for (int i = 0; i<L2; i++, put++)
            {
                Point pt = new Point (frequencyScale [i], magnitudeSpectrum [i]);
                results.Add (pt);
            }

            return results;
        }

        //*****************************************************************************************
        //
        // FindPeaks - find peak in spectrum
        //

        internal List<Point> FindPeaks (double threshold)
        {
            List<Point> peaks = new List<Point> ();
            List<Point> spect = WindowedSpectrum;

            for (int i=1; i<spect.Count - 1; i++)
            {
                if (spect [i].Y > threshold)
                {
                    if (spect [i].Y > spect [i-1].Y && spect [i].Y > spect [i+1].Y)
                    {
                        peaks.Add (spect [i]);
                    }
                }
            }

            return peaks;
        }
    }
}
