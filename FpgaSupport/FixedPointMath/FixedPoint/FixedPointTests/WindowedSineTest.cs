using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

//using FixedPt = FixedPointLib.FixedPoint_1_7_24;
using FixedPt = FixedPointLib.FixedPoint_1_5_10;

using Common;
using Plot2D_Embedded;
using System.Windows.Media;
using System.IO;

#pragma warning disable IDE0051
#pragma warning disable IDE0052
#pragma warning disable CS0162

namespace FixedPointTests
{
    class WindowedSineTest : IFixedPtTest
    {
        private const double SampleRate = 10240;
        private const int NumberSamples = 1024;
        private const double Freq = 300;

        private readonly int WindowStart = 200;
        private readonly int RampSamples = 50;
        private readonly int LevelSamples = 200;

        private readonly List<FixedPt> CordicOut    = new List<FixedPt> (); // sinusoid, -1 to +1
        private readonly List<FixedPt> Window       = new List<FixedPt> (); // 0 to +1
        private readonly List<FixedPt> WindowedSine = new List<FixedPt> (); // 0 to +1

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

        public WindowedSineTest ()
        {
        }

        //*********************************************************************************

        public void DoCalculations ()
        {
            // sine wave
            double t = 0;

            while (CordicOut.Count < NumberSamples)
            {
                // the 2.048 factor gives 12 bits, same as CORDIC.v
                CordicOut.Add (new FixedPt (2.048 * Math.Cos (2 * Math.PI * Freq * t)));
                t += 1 / SampleRate;
            }

            // window
            while (Window.Count < WindowStart)
                Window.Add (new FixedPt (0));
    
            for (int i=0; i<RampSamples; i++)
                Window.Add (new FixedPt (i / (double) RampSamples));

            for (int i=0; i<LevelSamples; i++)
                Window.Add (new FixedPt (1));

            for (int i=0; i<RampSamples; i++)
                Window.Add (new FixedPt (1 - i / (double) RampSamples));

            while (Window.Count < NumberSamples)
                Window.Add (new FixedPt (0));    

            // product
            for (int i=0; i<NumberSamples; i++)
                WindowedSine.Add (CordicOut [i] * Window [i]);

            // write some to text file
            List<FixedPt> printSource = Window; // which collection to print

            using (StreamWriter stream = new StreamWriter (@"..\..\WindowedSine.txt"))
            {
                for (int i=0; i<RampSamples + 1; i++)
                { 
                    string str = string.Format ("0x{0:X4}", printSource [WindowStart + i].AsInt);
                    stream.WriteLine (str);
                }
            }
        }

        //*********************************************************************************

        public void DoPlots ()
        {
            List<Int16> plotValues = new List<short> ();

            //foreach (FixedPt fp in CordicOut)
            //    plotValues.Add (fp.AsInt);

            //LineView l1 = new LineView (plotValues, 10);
            //l1.Color = Brushes.Red;
            //PlotArea.Plot (l1);

            //**********************************************

            //plotValues.Clear ();

            //foreach (FixedPt fp in Window)
            //    plotValues.Add (fp.AsInt);

            //LineView l2 = new LineView (plotValues, 10);
            //l2.Color = Brushes.Green;
            //PlotArea.Plot (l2);

            //**********************************************

            plotValues.Clear ();

            foreach (FixedPt fp in WindowedSine)
                plotValues.Add (fp.AsInt);

            LineView l3 = new LineView (plotValues, 10);
            l3.Color = Brushes.Blue;
            PlotArea.Plot (l3);



            PlotArea.RectangularGridOn = true;
        }
    }
}
