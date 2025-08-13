
using System.IO;
using System.Windows;
using System.Windows.Media;

using Plot2D_Embedded;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0044 // readonly
#pragma warning disable IDE0055 // formatting
#pragma warning disable IDE0052 
#pragma warning disable IDE0051


namespace FpgaTestDataGen
{
    public partial class MainWindow : Window
    {
        private static readonly short OneBit = 10;
        private static readonly short One = (short) (1 << OneBit);

        private readonly List<double> Signal   = new List<double> (); // -1 to +1
        private readonly List<short>  Signal16 = new List<short> ();  // signed integer, 1_5_10

        private readonly List<double> Replica   = new List<double> (); // -1 to +1
        private readonly List<short>  Replica10 = new List<short> ();  // unsigned 10 bit integer, offset binary
        private readonly List<short>  Replica16 = new List<short> ();  // signed integer, 1_5_10

        private readonly List<double> Correlation = new List<double> (); 
        private readonly List<short>  Correlation16 = new List<short> (); 


        private short  SignalLength = 128; // 256 is maximum allowd in Verilog Testbench
        private short  ReplicaLength = 25; //  64 is max
        private double ReplicaCycles = 1;//3;
        private int    ReturnStarts = 50;//120;

        private readonly string FileDir     = @"C:\Users\rgsod\Documents\FPGA\Xilinx\projects\Sonar1Chan\Sonar1Chan.sim\sim_1\behav\xsim";
        private readonly string ReplicaFile = "replica.mem";
        private readonly string SignalFile  = "signal.mem";

        public MainWindow ()
        {
            InitializeComponent ();
            DoCalculations ();
            WriteFiles ();
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
        }

        //*************************************************************

        // Replica16 = short, signed integer, 1_5_10
        // Replica10 = short,  offset binary 10 bit integer
        // Replica   = double, -1 to +1

        private void CalculateReplica ()
        {
            // Replica in doubles, -1 to 1

            if (ReplicaCycles == 0)
            {
                for (int i = 0; i<ReplicaLength; i++)
                {
                    Replica.Add (1);
                }
            }

            else
            { 
                for (int i = 0; i<ReplicaLength; i++)
                {
                    double x = Math.Sin (2 * Math.PI * ReplicaCycles * i / (ReplicaLength - 1));
                    Replica.Add (x);
                }
            }

            //***************************************************************

            // Convert to Replica10
            //    - 10 bit offset binary (OB) used by ADC/DAC hardware
            //    - OB 1024 ==  1 float (can't be stored in 10 bits)
            //    -  OB 512 ==  0 float
            //    -    OB 0 == -1 float

            double Ampl = 511 / 512.0; // scale sine to amplitude that will fit
            double m = (1024 - 0) / (1 - -1);
            double b = 512;

            foreach (double x in Replica)
            {
                double sx = Ampl * x;
                short y = (short)(m * sx + b);
                Replica10.Add (y);
            }

            //**********************************************************

            // Convert to Replica16, as used in FPGA calculations
            foreach (double x in Replica)
            {
                short s = (short)(x * One);
                Replica16.Add (s);
            }
        }

        //*************************************************************

        private void CalculateSignal ()
        {
            Random random = new Random (12); // provide seed so noise is same every run

            double NA = 0.1; // noise amplitude
            double SA = 0.9; // signal amplitude

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

            //******************************************************

            // -1 to 1 double => 1_5_10 signed short

            short Pos1 = 1 << 10;
            short Neg1 = (short)(Pos1 * -1);

            double m = (Pos1 - Neg1) / 2;
            double b = 0;

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
                }

                short Acc16 = (short) (Acc32 >> OneBit);
                Correlation16.Add (Acc16);

                //Console.WriteLine ("Acc32 = " + Acc32.ToString ("X8"));
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception in CorrStepInFixed: " + ex.Message);
            }
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

                LineView corr = new LineView (Correlation16, OneBit);
                corr.Color = Brushes.Blue;
                IntegerPlots.Plot (corr);

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
            short count = ReplicaLength;

            using (StreamWriter outputFile = new StreamWriter (System.IO.Path.Combine (FileDir, ReplicaFile)))
            {
                outputFile.WriteLine (count.ToString ("X"));

                for (int i = 0; i<count; i++)
                {
                    short s = Replica10 [i];
                    outputFile.WriteLine (s.ToString ("X"));

                    //short s = Replica16 [i];
                    //double f = Replica [i];

                    //string str = string.Format (@"{0:X4}  // {1:F}", s, f);
                    //outputFile.WriteLine (str);
                }
            }

            count = SignalLength;

            using (StreamWriter outputFile = new StreamWriter (System.IO.Path.Combine (FileDir, SignalFile)))
            {
                outputFile.WriteLine (count.ToString ("X"));

                for (int i = 0; i<count; i++)
                {
                    short s = Signal16 [i];
                    outputFile.WriteLine (s.ToString ("X"));
                }
            }
        }

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