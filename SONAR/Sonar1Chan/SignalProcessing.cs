using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using MathNet.Numerics.Statistics;

namespace Sonar1Chan
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
        // Absolute value
        //      x = sample times, y = amplitude
        //
        private readonly List<Point> absoluteValue = new List<Point> ();
        public List<Point> AbsoluteValue {get {return absoluteValue;}}

        //
        // Median filter
        //      x = sample times, y = amplitude
        //
        private readonly List<Point> medianFiltered = new List<Point> ();
        public List<Point> MedianFiltered {get {return medianFiltered;}}




        //*******************************************************************************
        //
        // Constructor
        //
        public SignalProcessing (List<Point> samp)
        {
            //
            // Save a copy of raw samples
            //
            inputSamples = samp;

            //
            // Absolute value
            //
            double dc = 0;

            for (int i=0; i<inputSamples.Count; i++)
                dc += inputSamples [i].Y;

            dc /= inputSamples.Count;

            for (int i=0; i<inputSamples.Count; i++)
                absoluteValue.Add (new Point (inputSamples [i].X, Math.Abs (inputSamples [i].Y - dc)));

            //
            // Median filter
            //
            int WS = 8; // window size

            for (int i=0; i<absoluteValue.Count; i+=WS)
            {
                List<double> filterIn = new List<double> ();

                for (int j=0; j<WS; j++)
                {
                    if (i + j >= absoluteValue.Count)
                        break;

                    filterIn.Add (absoluteValue [i+j].Y);
                }

                medianFiltered.Add (new Point (absoluteValue [i].X, filterIn.Median ()));
            }

        }
    }
}



