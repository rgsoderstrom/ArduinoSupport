
/*
    firFilter - with fixed point arithmetic
*/

//#pragma warning disable IDE0051
//#pragma warning disable IDE0052

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using FixedPt = FixedPointLib.FixedPoint_1_7_24;
using FixedPt = FixedPointLib.FixedPoint_1_5_10;

using MathNet.Filtering.FIR;

namespace FixedPointLib
{
    public class FirFilter
    {
        //*********************************************************************
        //
        // FirFilter filter = new FirFilter (sampleRate, cutoffFreq);
        //
        private readonly List<FixedPt> FixedPointCoefs = new List<FixedPt> ();
        public int Length {get {return FixedPointCoefs.Count;}}

        //*********************************************************************
        //
        // Constructor - from sample rate & cutoff frequency
        //
        public FirFilter (double inputSampleRate, double cutoffFrequency)
        { 
            double [] floatingPtCoefs = FirCoefficients.LowPass (inputSampleRate, cutoffFrequency); 

            foreach (double d in floatingPtCoefs)
                FixedPointCoefs.Add (new FixedPt (d));
        }

        //*********************************************************************
        //
        // Constructor - from a list of coefficients
        //
        public FirFilter (IList<double> floatingPtCoefs)
        { 
            foreach (double d in floatingPtCoefs)
                FixedPointCoefs.Add (new FixedPt (d));
        }

        //*********************************************************************
        //
        // filtered = filter.ProcessSample (input);
        //
        public List<FixedPt> ProcessSample (List<FixedPt> input)
        {
            List<FixedPt> filtered = new List<FixedPt> ();

            for (int i=0; i<input.Count-this.Length; i++)
            {
                FixedPt acc = new FixedPt (0);

                for (int j=0; j<Length; j++)
                    acc += input [i + j] * FixedPointCoefs [j];

                filtered.Add (acc);
            }

            return filtered;
        }
    }
}
