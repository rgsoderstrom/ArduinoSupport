
//
// SignalProcessing2
//      - matched filter
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Common;
using SonarCommon;

using MathNet.Filtering;
using MathNet.Filtering.FIR;

namespace Sonar1Chan
{
    //*********************************************************************

    internal class SignalProcessing2
    {
        //
        // Parameters
        //
        public readonly double CenterFrequency;
        public readonly double InputSampleRate;

        //*************************************************************************************
        //
        // Samples as received, with range included
        //
        private readonly List<Point> inputSamples = new List<Point> (); // x = range, y = amplitude
        public           List<Point> InputSamples {get {return inputSamples;}}



        // RESTORE DC REMOVAL




        //
        // Signed samples, DC removed
        //
        private readonly List<double> signedSamples = new List<double> ();

        //
        // range to each sample
        //
        private readonly List<double> range = new List<double> (); // range for signed sample

        //
        // Replica of transmitted wave form
        //
        private readonly List<double> Replica;

        //
        // Matched filter output
        //
        readonly int decimation = 1;
        private readonly List<Point> matchedFilter = new List<Point> ();

        //
        // Peak-pick output
        //
        private readonly int peakWin = 16;
        private readonly List<Point> peakPicked = new List<Point> ();
        public           List<Point> Magnitude {get {return peakPicked;}} // use the name Magnitude for compatability
                                                                          // with SigProc ver 1

        //*******************************************************************************
        //
        // Constructor
        //
        public SignalProcessing2 (List<double> samples, double PingFrequency, double SampleRate, double PingDuration)
        {
            try
            { 
                CenterFrequency = PingFrequency;
                InputSampleRate = SampleRate;

                const double SoundSpeed = 1125; // feet per second

                double BlankingTime = (PingDuration / 1000) + 0.003; // seconds from ping command to first sample
                double timeTag = BlankingTime;

                //
                // Construct replica
                //
                TxWaveGen.CW waveGen = new TxWaveGen.CW (1, SampleRate, PingFrequency, PingDuration, true);
                Replica = waveGen.Samples;

                //
                // Save a copy of raw samples with range attached
                //
                int N = samples.Count;

                for (int i=0; i<N; i++)
                {                    
                    inputSamples.Add (new Point (timeTag * SoundSpeed / 2, (Int16) samples [i]));
                    timeTag += 1 / InputSampleRate;
                }

                //
                // Remove DC
                //
                //double sum = 0;

                //for (int i = 0; i<N; i++)
                //    sum += inputSamples [i].Y;

                Int16 dc = 511; // (Int16) (sum / N);

                for (int i = 0; i<N; i++)
                {
                    signedSamples.Add (inputSamples [i].Y - dc);
                    range.Add (inputSamples [i].X);
                }

                //
                // filter
                //
                OnlineFirFilter filter = new OnlineFirFilter (Replica);
                int delay = Replica.Count / 2; // shift filter output to line up with
                                               // leading edge of echo

                // run filter
                double [] filtered = filter.ProcessSamples (signedSamples.ToArray ());

                // decimate, abs value
                for (int i = 0; i<N-delay; i+=decimation)
                {
                    double mag = Math.Abs (filtered [i + delay]);
                    matchedFilter.Add (new Point (inputSamples [i].X, mag));
                }

                // peak pick
                for (int i=0; i<matchedFilter.Count; i+=peakWin)
                {
                    Point peak = matchedFilter [i];

                    for (int j=0; j<peakWin; j++)
                    { 
                        if (i + j >= matchedFilter.Count)
                            break;

                        if (peak.Y < matchedFilter [i+j].Y)
                            peak = matchedFilter [i+j];
                    }

                    peakPicked.Add (peak);
                }
            }

            catch (Exception ex)
            {
                Common.EventLog.WriteLine ("Exception constructing SignalProcessor2: " + ex.Message);
            }
        }

        //******************************************************************************
        //
        // ConstructReplica
        //
        //const int Ramp = 20; // number of samples to ramp up or down between 0 and max amplitude. fixed in harware

        //private List<double> ConstructReplica (double SampleRate, 
        //                                       double PingFrequency, 
        //                                       double PingDuration) // milliseconds
        //{ 
        //    List<double> replica = new List<double> ();
        //    double time = 0;

        //     // number of samples at maximum amplitude
        //    int PingSamples = (int) ((PingDuration / 1000) * SampleRate); // seconds * Samples / second

        //    //Common.EventLog.WriteLine (SampleRate + " sample rate");
        //    //Common.EventLog.WriteLine ((PingDuration / 1000) + " ping duration");
        //    //Common.EventLog.WriteLine (PingSamples + " ping samples at max");

        //    // returns from time the xmit waveform is ramping up
        //    for (int i=0; i<Ramp; i++, time+=1/SampleRate)
        //    {
        //        double win = (double) i / Ramp;
        //        double s = win * Math.Sin (2 * Math.PI * PingFrequency * time);
        //        replica.Add (s);
        //    }            
            
        //    // from time at max level
        //    for (int i=0; i<PingSamples; i++, time+=1/SampleRate)
        //    {
        //        double win = 1;
        //        double s = win * Math.Sin (2 * Math.PI * PingFrequency * time);
        //        replica.Add (s);
        //    }            
            
        //    // while ramping down
        //    for (int i=0; i<Ramp; i++, time+=1/SampleRate)
        //    {
        //        double win = 1 - (double) i / Ramp;
        //        double s = win * Math.Sin (2 * Math.PI * PingFrequency * time);
        //        replica.Add (s);
        //    }            
            
        //    // adjust amplitudes so sum of absolute values is 1
        //    double sum = 0;

        //    foreach (double s in replica)
        //        sum += Math.Abs (s);

        //    for (int i=0; i<replica.Count; i++)
        //        replica [i] /= sum;

        //    return replica;
        //}
    }
}


