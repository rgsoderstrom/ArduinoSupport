using System.IO;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Plot2D_Embedded;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0044 // readonly
#pragma warning disable IDE0055 // formatting

namespace FpgaTestDataGen
{
    public partial class MainWindow : Window
    {
        private static readonly short OneBit = 10;
        private static readonly short One = (short) (1 << OneBit);

        private readonly List<double> Signal   = new List<double> (); // -1 to +1
        private readonly List<short>  Signal16 = new List<short> ();  // signed integer, 1_5_10

        private readonly List<double> Replica   = new List<double> (); // -1 to +1
        private readonly List<short>  Replica10 = new List<short> ();  // unsigned 10 bit integer
        private readonly List<short>  Replica16 = new List<short> ();  // signed integer, 1_5_10

        private int SignalLength = 128; // number of samples
        private int SignalStart  = 0;
        private int ReplicaLength {get {return Replica.Count;}} // specified as PingDuration

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
            ScaleSignal ();

            CorrStepInDouble ();
            CorrStepInFixed ();
        }

        //*************************************************************

        private readonly double SampleRate = 1 * 100000;
        private readonly double Frequency  = 40200;
        private readonly double PingDuration = 0.2 * 1e-3; 
        private readonly List<double> ReplicaTime = new List<double> ();
        private readonly List<double> SignalTime = new List<double> ();

        private void CalculateReplica ()
        {
            double RampUpDuration = 0.15 * PingDuration;
            double RampDnDuration = RampUpDuration;
            double LevelDuration  = PingDuration - RampUpDuration - RampDnDuration;

            double A = 1; // amplitude. 1 == full scale
            double t = 0;

            while (t < RampUpDuration)
            {
                double r = (t / RampUpDuration) * A * Math.Sin (2 * Math.PI * Frequency * t);
                Replica.Add (r);
                ReplicaTime.Add (t);
                t += 1 / SampleRate;
            }

            while (t < RampUpDuration + LevelDuration)
            {
                double r = A * Math.Sin (2 * Math.PI * Frequency * t);
                Replica.Add (r);
                ReplicaTime.Add (t);
                t += 1 / SampleRate;
            }

            while (t < PingDuration)
            {   
                double rt = t - RampUpDuration - LevelDuration;
                double r = (1 - rt / RampDnDuration) * A * Math.Sin (2 * Math.PI * Frequency * t);
                Replica.Add (r);
                ReplicaTime.Add (t);
                t += 1 / SampleRate;
            }

            //**********************************************************

            // convert to 10 bit unsigned. What ADC will read.
            // -1 to 1 double => 0 to 1024 integer. NOTE: 1024 can't be stored
            double m = 1024 / 2;
            double b = 512;

            foreach (double x in Replica)
            {
                short a = (short) (m * x + b);
                a &= 1023;
                Replica10.Add (a);
            }

            //**********************************************************

            // Replica16 from Replica10. As used in FPGA calculations
            foreach (short x in Replica10)
            {
                double dr = 2 * (x - 512);
                int fullProduct = (int)(dr * One);
                Replica16.Add ((short)(fullProduct >> OneBit));
            }
        }

        //*************************************************************

        private void CalculateSignal ()
        {
            double t = 0;

            Random random = new Random ();

            // initialize the signal with just noise ...
            double NA = 0.1; // noise amplitude

            for (int i = 0; i<SignalLength; i++)
            {                
                double noise = NA * 2 * (random.NextDouble () - 0.5);
                Signal.Add (noise);
                SignalTime.Add (t);
                t += 1 / SampleRate;
            }

            // ... then add scaled replica

            //for (int i = 0; i<ReplicaLength; i++)
            //{
            //    Signal [SignalStart + i] += 0.2 * Replica [i];
            //}
        }

        //*************************************************************

        private void CorrStepInDouble ()
        { 
            double Acc = 0;

            for (int i=0; i<ReplicaLength; i++)
                Acc += Replica [i] * Signal [SignalStart + i];

            Console.WriteLine ("DP CorrStep = " + Acc.ToString ());

            short Pos1 = 1 << 10;
            short Neg1 = (short) (Pos1 * -1);

            double m = (Pos1 - Neg1) / 2;
            double b = 0;

            short Acc16 = (short) (m * Acc + b);
            Console.WriteLine ("DP CorrStep converted to 1_5_10: " + Acc16.ToString ("X"));
        }

        //*************************************************************

        private void CorrStepInFixed ()
        { 
            int Acc32 = 0;
            short Acc16 = 0;

            for (int i=0; i<ReplicaLength; i++)
            {
                short a = Replica16 [i];
                short b = Signal16 [SignalStart + i];
                int full = a * b; 


                full += 1 << (OneBit - 1);
                

                Acc32 += full >> OneBit;
                Acc16 += (short) (full >> OneBit);
            }

            Console.WriteLine ("Acc32 = " + Acc32.ToString ("X"));
            Console.WriteLine ("Acc16 = " + Acc16.ToString ("X"));
            Console.WriteLine ((double) Acc32 / One);
        }

        //*************************************************************

        private void DoPlots ()
        {

            LineView sig1 = new LineView (SignalTime, Signal);
            PlotArea.Plot (sig1);

            LineView rep1 = new LineView (ReplicaTime, Replica);
            rep1.Color = Brushes.Red;
            PlotArea.Plot (rep1);

            PlotArea.RectangularGridOn = true;
        }

        //*************************************************************



        // -1 to 1 double => 1_5_10 signed short

        private void ScaleSignal ()
        {
            short Pos1 = 1 << 10;
            short Neg1 = (short) (Pos1 * -1);

            double m = (Pos1 - Neg1) / 2;
            double b = 0;

            foreach (double x in Signal)
                Signal16.Add ((short) (m * x + b));
        }

        //*************************************************************

        private void WriteFiles ()
        {
            int count = ReplicaLength;

            using (StreamWriter outputFile = new StreamWriter (System.IO.Path.Combine (FileDir, ReplicaFile)))
            {
                for (int i=0; i<count; i++)
                {
                    short s = Replica10 [i];
                    outputFile.WriteLine (s.ToString ("X"));
                }
            }
            
            using (StreamWriter outputFile = new StreamWriter (System.IO.Path.Combine (FileDir, SignalFile)))
            {
                for (int i=0; i<count; i++)
                {
                    short s = Signal16 [i];
                    outputFile.WriteLine (s.ToString ("X"));
                }
            }
            
        }

        private void PlotArea_PlotWindowReady (object sender)
        {
            DoPlots ();
        }
    }
}