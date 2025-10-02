
using System.IO;
using System.Windows;
using System.Windows.Media;

using Plot2D_Embedded;

#pragma warning disable CA1822 
#pragma warning disable IDE0044
#pragma warning disable IDE0055
#pragma warning disable IDE0052 
#pragma warning disable IDE0051
#pragma warning disable IDE0028
#pragma warning disable IDE0090


namespace FpgaTestDataGen
{
    public partial class MainWindow : Window
    {
        private static readonly short OneBit = 10;
        private static readonly short One = (short) (1 << OneBit);

        private readonly List<double> Signal   = new List<double> (); // -1 to +1
        private readonly List<short>  Signal10 = new List<short> ();  // unsigned 10 bit integer, offset binary
        private readonly List<short>  Signal16 = new List<short> ();  // signed integer, 1_5_10

        private readonly List<double> Replica   = new List<double> (); // -1 to +1
        private readonly List<short>  Replica10 = new List<short> ();  // unsigned 10 bit integer, offset binary
        private readonly List<short>  Replica16 = new List<short> ();  // signed integer, 1_5_10

        private readonly List<double> Correlation   = new List<double> (); 
        private readonly List<int>    Correlation32 = new List<int> (); 



        // PeakPick and Log2
        private readonly int PeakPickWindow = 8;

        private readonly List<double> Compressed   = new List<double> (); 
        private readonly List<byte>   Compressed32 = new List<byte> (); 



        private short  SignalLength  = 1024; // Check maximum allowed in Verilog Testbench
        private short  ReplicaLength = 20; 
        private double ReplicaCycles = 1;
        private int    ReturnStarts  = 200;

        private readonly string? FileDir     = @"C:\Users\rgsod\Documents\FPGA\Xilinx\projects\Sonar1Chan\Sonar1Chan.sim\sim_1\behav\xsim";
        private readonly string? ReplicaFile;// = "replica.mem";
        private readonly string? SignalFile  = "signal.mem";
        private readonly string? ResultsFile;// = "results.txt";

        public MainWindow ()
        {
            try
            { 
                InitializeComponent ();
                DoCalculations ();
                WriteFiles ();
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception in ctor: " + ex.Message);
            }
        }

        //*************************************************************
        //*************************************************************
        //*************************************************************

        private void DoCalculations ()
        {
            CalculateReplica ();
            CalculateSignal ();

            for (int i=0; i<Signal.Count - Replica.Count; i++)
            { 
                CorrStepInDouble (i);
                CorrStepInFixed (i);
            }

            CompressDouble (); 
            CompressFixed (); 
        }

        //*************************************************************

        // Replica16 = short, signed integer, 1_5_10
        // Replica10 = short,  offset binary 10 bit integer
        // Replica   = double, -1 to +1

        private void CalculateReplica ()
        {
            // Replica in doubles, -1 to 1

            double RA = 511 / 512.0;

            if (ReplicaCycles == 0)
            {
                for (int i = 0; i<ReplicaLength; i++)
                {
                    Replica.Add (RA);
                }
            }

            else
            { 
                for (int i = 0; i<ReplicaLength; i++)
                {
                    double x = RA * Math.Sin (2 * Math.PI * ReplicaCycles * i / (ReplicaLength - 1));
                    Replica.Add (x);
                }
            }

            //***************************************************************

            // Convert to Replica10
            //    - 10 bit offset binary (OB) used by ADC/DAC hardware
            //    - OB 1024 ==  1 float (can't be stored in 10 bits)
            //    -  OB 512 ==  0 float
            //    -    OB 0 == -1 float

            double m = (1024 - 0) / (1 - -1);
            double b = 512;

            foreach (double x in Replica)
            {
                short y = (short)(m * x + b);
                Replica10.Add (y);
            }

            //**********************************************************

            // Convert to Replica16, as used in FPGA calculations

            foreach (short x in Replica10)
            {
                short s = (short) ((x - 512) << 1);
                Replica16.Add (s);
            }

            //foreach (double x in Replica)
            //{
            //    short s = (short)(x * One);
            //    Replica16.Add (s);
            //}
        }

        //*************************************************************

        private void CalculateSignal ()
        {
            Random random = new Random (12); // provide seed so noise is same every run

            double NA = 0.05; // noise amplitude
            double SA = 0.5; // signal amplitude

            // initialize the signal with just noise ...
            for (int i = 0; i<SignalLength; i++)
            {                
                double noise = NA * 2 * (random.NextDouble () - 0.5);
                Signal.Add (noise);
            }

            // ... then add scaled replica

            for (int i = 0; i<ReplicaLength; i++)
            {
                Signal [ReturnStarts + i] += SA * Replica [i];
            }

            //***************************************************************

            // Convert to Signal10
            //    - 10 bit offset binary (OB) used by ADC/DAC hardware
            //    - OB 1024 ==  1 float (can't be stored in 10 bits)
            //    -  OB 512 ==  0 float
            //    -    OB 0 == -1 float

            double m = (1024 - 0) / (1 - -1);
            double b = 512;

            foreach (double x in Signal)
            {
                short y = (short)(m * x + b);
                Signal10.Add (y);
            }

            //******************************************************

            // -1 to 1 double => 1_5_10 signed short

            short Pos1 = 1 << 10;
            short Neg1 = (short)(Pos1 * -1);

            m = (Pos1 - Neg1) / 2;
            b = 0;

            foreach (double x in Signal)
                Signal16.Add ((short)(m * x + b));
        }

        //*************************************************************

        private void CorrStepInDouble (int signalStart)
        { 
            double Acc = 0;

            for (int i=0; i<ReplicaLength; i++)
                Acc += Replica [i] * Signal [signalStart + i];

            Correlation.Add (Acc);
        }

        //*************************************************************

        private void CorrStepInFixed (int signalStart)
        {
            try
            {
                int Acc32 = 0;

                for (int i = 0; i<ReplicaLength; i++)
                {
                    short a = Replica16 [i];
                    short b = Signal16 [signalStart + i];
                    int full = a * b;

                    Acc32 += full;

                    //Console.Write     (" Replica = " + a.ToString ("x4"));
                    //Console.Write     (" Signal = " + b.ToString ("x4"));
                    //Console.Write     (" full = " + full.ToString ("x8"));
                    //Console.WriteLine (" Acc32 = " + Acc32.ToString ("x8"));
                }

                Correlation32.Add (Acc32);
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception in CorrStepInFixed: " + ex.Message);
            }
        }

        //*************************************************************

        private void CompressDouble ()
        {
            List<double> Peaks = new List<double> ();

            for (int i=0; i<Correlation.Count; i+=PeakPickWindow)
            {
                double maxAbs = 0;

                for (int j=0; j<PeakPickWindow; j++)
                {
                    int get = i+j;
                    if (get >= Correlation.Count) break;

                    maxAbs = Math.Max (maxAbs, Math.Abs (Correlation [get]));
                }

                Peaks.Add (maxAbs);
            }

            foreach (double peak in Peaks)
                Compressed.Add (Math.Log2 (peak));
        }
            
        private void CompressFixed ()
        {
            List<Int32> Peaks = new List<Int32> ();

            for (int i=0; i<Correlation32.Count; i+=PeakPickWindow)
            {
                Int32 maxAbs = 0;

                for (int j=0; j<PeakPickWindow; j++)
                {
                    int get = i+j;
                    if (get >= Correlation32.Count) break;

                    Int32 abs = Correlation32 [get] >= 0 ? Correlation32 [get] : -1 * Correlation32 [get];
                    maxAbs = maxAbs > abs ?  maxAbs : abs;
                }

                Peaks.Add (maxAbs);
            }

            foreach (Int32 peak in Peaks)
                Compressed32.Add (Log2Approx (peak));
        }

        //*******************************************************************************************

        // 5 MSBs are position of first bit. 3 LSBs are next 3 bits of "x" after rounding

        //private void UnitTestLog2Approx ()
        //{
        //    Int32 pattern = 0x7;
        //    Int32 arg = pattern;

        //    while (arg != 0)
        //    {                
        //        try
        //        { 
        //            byte l2 = Log2Approx (arg);
        //            double l2d = (double) l2 / 8;

        //            Console.Write     (l2d);
        //            Console.Write     (" ");
        //            Console.WriteLine (string.Format ("0x{0:x8}", arg));
        //        }

        //        catch (Exception ex)
        //        {
        //            Console.WriteLine ("Exception in Log2 unit test: " + ex.Message);
        //        }

        //        arg <<= 1;
        //    }
        //}
        
        private byte Log2Approx (Int32 x)
        {
            if (x < 0)
                throw new Exception ("Exception in Log2Approx - attempted to take log of negative number");

            if (x == 0)
                throw new Exception ("Exception in Log2Approx - attempted to take log of 0");

            byte approx = 0;

            try
            { 
                Int32 bitCounter = 30;

                Int32 Bit31 = 1 << 31;
                Int32 Bit30 = 1 << 30;
                Int32 Bit29 = 1 << 29;
                Int32 Bit26 = 1 << 26; // added before truncate

                while (bitCounter > 0)
                {
                    bool found = (Bit30 & x) != 0;

                    if (found == true)
                        break;

                    bitCounter--;
                    x <<= 1;
                }

                x += Bit26; // add before truncate

                if ((Bit31 & x) != 0)
                {
                    bitCounter++;
                    x <<= 1; // move next 3 bits to MSBs
                }
                else
                { 
                    x <<= 2;
                }

                approx |= (byte)(bitCounter << 3);

                if ((x & Bit31) != 0) approx |= 4;
                if ((x & Bit30) != 0) approx |= 2;
                if ((x & Bit29) != 0) approx |= 1;
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception in Log2Approx: " + ex.Message);
            }

            return approx;
        }

        //*************************************************************

        private void DoDoublePlots ()
        {
            try
            { 
                LineView rep = new LineView (Replica);
                rep.Color = Brushes.Red;
                DoublePlots.Plot (rep);

                LineView sig = new LineView (Signal);
                sig.Color = Brushes.Green;
                DoublePlots.Plot (sig);
               
                LineView corr = new LineView (Correlation);
                corr.Color = Brushes.Blue;
                DoublePlots.Plot (corr);

                //******************************************************

                List<Point> compPoints = new List<Point> ();

                for (int i=0; i<Compressed.Count; i++)
                {
                    compPoints.Add (new Point ((PeakPickWindow / 2) + PeakPickWindow * i, Compressed [i]));
                }

                LineView comp = new LineView (compPoints);
                comp.Color = Brushes.Black;
                DoublePlots.Plot (comp);

                DoublePlots.RectangularGridOn = true;
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception in DoDoublePlots:" + ex.Message);
            }
        }

        private void DoIntegerPlots ()
        {
            try
            { 
                LineView rep = new LineView (Replica16, OneBit);
                rep.Color = Brushes.Red;
                IntegerPlots.Plot (rep);

                //LineView rep10 = new LineView (Replica10, OneBit);
                //rep10.Color = Brushes.Red;
                //IntegerPlots.Plot (rep10);

                LineView sig = new LineView (Signal16, OneBit);
                sig.Color = Brushes.Green;
                IntegerPlots.Plot (sig);

                LineView corr = new LineView (Correlation32, 2 * OneBit);
                corr.Color = Brushes.Blue;
                IntegerPlots.Plot (corr);

                //******************************************************

                List<Point> compPoints = new List<Point> ();

                for (int i=0; i<Compressed32.Count; i++)
                {
                    compPoints.Add (new Point ((PeakPickWindow / 2) + PeakPickWindow * i, Compressed32 [i] / 8.0));
                }

                LineView comp = new LineView (compPoints);
                comp.Color = Brushes.Black;
                IntegerPlots.Plot (comp);

                IntegerPlots.RectangularGridOn = true;
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception in DoIntegerPlots:" + ex.Message);
            }

        }

        //*************************************************************

        private void WriteFiles ()
        {
            if (FileDir == null) return;

            short count;

            if (ReplicaFile != null)
            { 
                count = ReplicaLength;

                using (StreamWriter outputFile = new StreamWriter (System.IO.Path.Combine (FileDir, ReplicaFile)))
                {
                    outputFile.WriteLine (count.ToString ("X"));

                    for (int i = 0; i<count; i++)
                    {
                        short s = Replica10 [i];
                        outputFile.WriteLine (s.ToString ("X"));
                    }
                }
            }

            if (SignalFile != null)
            { 
                count = SignalLength;

                using (StreamWriter outputFile = new StreamWriter (System.IO.Path.Combine (FileDir, SignalFile)))
                {
                    outputFile.WriteLine (count.ToString ("X"));

                    for (int i = 0; i<count; i++)
                    {
                      //short s = Signal16 [i];
                        short s = Signal10 [i];
                        outputFile.WriteLine (s.ToString ("X"));
                    }
                }
            }

            if (ResultsFile != null)
            { 
                using (StreamWriter outputFile = new StreamWriter (System.IO.Path.Combine (FileDir, ResultsFile)))
                {
                    for (int i = 0; i<Compressed32.Count; i++)
                    {
                        byte s = Compressed32 [i];
                        outputFile.WriteLine (s.ToString ("x2"));
                    }

                    //for (int i = 0; i<Correlation32.Count; i++)
                    //{
                    //    int s = Correlation32 [i];
                    //    outputFile.WriteLine (s.ToString ("x8"));
                    //}
                }
            }
        }

        //*****************************************************************

        private void Double_PlotWindowReady (object sender)
        {
            DoDoublePlots ();
        }

        private void Integer_PlotWindowReady (object sender)
        {
            DoIntegerPlots ();
        }
    }
}