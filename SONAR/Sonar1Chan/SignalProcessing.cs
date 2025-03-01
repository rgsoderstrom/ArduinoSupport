using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using MathNet.Filtering;
using MathNet.Filtering.FIR;

using Common;

namespace Sonar1Chan
{
    internal class SignalProcessing
    {
        //
        // Parameters
        //
        public readonly double CenterFrequency;
        public readonly double InputSampleRate;
        public readonly double BasebandSampleRate;

        //
        // Samples as received
        //
        private readonly List<Point> inputSamples = new List<Point> (); // x = range, y = amplitude
        public List<Point> InputSamples {get {return inputSamples;}}

        //
        // Signed samples, DC removed
        //
        private readonly List<Point> signedSamples = new List<Point> ();
        public List<Point> SignedSamples {get {return signedSamples;}}

        //
        // Baseband, magnitude
        //
        private readonly List<Point> magnitude = new List<Point> ();
        public List<Point> Magnitude {get {return magnitude;}}

        //*******************************************************************************
        //
        // Constructor
        //
        public SignalProcessing (List<double> samples, double _CenterFrequency, double _SampleRate, double PingDuration)
        {
            try
            { 
                const double SoundSpeed = 1125; // feet per second

                double BlankingTime = (PingDuration / 1000) + 0.003; // seconds from ping command to first sample
                double timeTag = BlankingTime;

                CenterFrequency = _CenterFrequency;
                InputSampleRate = _SampleRate;

                //
                // Save a copy of raw samples
                //
                int N = samples.Count;

                for (int i=0; i<N; i++)
                {                    
                    inputSamples.Add (new Point (timeTag * SoundSpeed / 2, samples [i]));
                    timeTag += 1 / InputSampleRate;
                }

                //
                // Baseband
                //
                List<double> LocalOscI = new List<double> (N);
                List<double> LocalOscQ = new List<double> (N);
                List<double> MixerOutI = new List<double> (N);
                List<double> MixerOutQ = new List<double> (N);
                List<double> FilteredI = new List<double> (N);
                List<double> FilteredQ = new List<double> (N);

                double dc = 0;

                for (int i=0; i<N; i++)
                    dc += inputSamples [i].Y;

                dc /= inputSamples.Count;

                for (int i=0; i<N; i++)
                    signedSamples.Add (new Point (inputSamples [i].X, inputSamples [i].Y - dc));

                // Generate local oscillators ------------------------- MAKE THESE STATIC?
                for (int i=0; i<N; i++)
                {
                    LocalOscI.Add (Math.Cos (2 * Math.PI * CenterFrequency * i / InputSampleRate));
                    LocalOscQ.Add (Math.Sin (2 * Math.PI * CenterFrequency * i / InputSampleRate));
                }

                // Mixer
                for (int i=0; i<N; i++)
                {
                    MixerOutI.Add (signedSamples [i].Y * LocalOscI [i]);
                    MixerOutQ.Add (signedSamples [i].Y * LocalOscQ [i]);
                }

                // Low-pass filter
                int dec = 16;
                double cutoff = InputSampleRate / (1 * dec); // s/b 2?
                BasebandSampleRate = InputSampleRate / dec;

                double [] filterCoefs = MathNet.Filtering.FIR.FirCoefficients.LowPass (InputSampleRate, cutoff);
                OnlineFirFilter filter = new OnlineFirFilter (filterCoefs);
                
                Common.EventLog.WriteLine ("Filter Length = " + filterCoefs.Length.ToString ());

                /****
                for (int i=0; i<filterCoefs.Length; i++)
                    Common.EventLog.WriteLine (filterCoefs [i].ToString ());
                ****/

                int delay = filterCoefs.Length / 2; // filter delay

                // run filter
                double [] filteredI = filter.ProcessSamples (MixerOutI.ToArray ());
                double [] filteredQ = filter.ProcessSamples (MixerOutQ.ToArray ());

                // decimate, calc magnitude
                for (int i = 0; i<N-delay; i+=dec)
                {
                    double mag2 = Math.Pow (filteredI [i + delay], 2)  // "+ delay" for time alignment
                                + Math.Pow (filteredQ [i + delay], 2);

                    magnitude.Add (new Point (InputSamples [i].X, Math.Sqrt (mag2)));
                }
            }

            catch (Exception ex)
            {
                Common.EventLog.WriteLine ("Exception constructing SignalProcessor: " + ex.Message);
            }
        }
    }
}



