
/*
    Transmit wave generator, TxWaveGen
        - used by
            - ArduinoSimulator
            - receiver's matched filter

        Limitation
            - FPGA generates its own tx wave samples. Ramp length is
              fixed at 20. Duration at max amplitude is programmable
            - future growth - send the wave generated here to FPGA
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarCommon
{
    public class TxWaveGen
    { 
        public class CW
        {
            const int Ramp = 20; // number of samples to ramp up or down between 0 and max amplitude. fixed in harware

            private readonly List<double> samples = new List<double> ();
            public           List<double> Samples {get {return samples;}}

            /// <summary>
            /// Generate samples for transmitted wave  
            /// </summary>
            /// <param name="Amplitude"></param>
            /// <param name="SampleRate"></param>
            /// <param name="PingFrequency"></param>
            /// <param name="PingDurationMillis">Duration at max amplitude, in milliseconds</param>
            /// <param name="Normalize">Optional, default is false</param>
            /// 
            public CW (double Amplitude, 
                       double SampleRate, 
                       double PingFrequency, 
                       double PingDurationMillis, // milliseconds
                       bool   Normalize = false)  // adjust amplitudes so sum of absolute values is 1. Overrides Amplitude 
            { 
                double time = 0;
                double duration = PingDurationMillis / 1000;

                 // number of samples at maximum amplitude
                int PingSamples = (int) (duration * SampleRate); // seconds * Samples / second

                // initial ramping up
                for (int i=0; i<Ramp; i++, time+=1/SampleRate)
                {
                    double win = (double) i / Ramp;
                    double s = win * Amplitude * Math.Sin (2 * Math.PI * PingFrequency * time);
                    samples.Add (s);
                }            
            
                // for time at max level
                for (int i=0; i<PingSamples; i++, time+=1/SampleRate)
                {
                    double win = 1;
                    double s = win * Amplitude * Math.Sin (2 * Math.PI * PingFrequency * time);
                    samples.Add (s);
                }            
            
                // final ramping down
                for (int i=0; i<Ramp; i++, time+=1/SampleRate)
                {
                    double win = 1 - (double) i / Ramp;
                    double s = win * Amplitude * Math.Sin (2 * Math.PI * PingFrequency * time);
                    samples.Add (s);
                }            
            
                if (Normalize == true)
                { 
                    // adjust amplitudes so sum of absolute values is 1
                    double sum = 0;

                    foreach (double s in samples)
                        sum += Math.Abs (s);

                    for (int i=0; i<samples.Count; i++)
                        samples [i] /= sum;
                }
            }
        }
    }
}
