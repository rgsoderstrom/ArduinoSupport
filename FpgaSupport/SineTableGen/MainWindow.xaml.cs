
/*
    SineTableGen - generate fixed-point sine table for FPGA sine/cosine lookup
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;

using Plot2D_Embedded;
using FP = FixedPointLib.FixedPoint_1_7_24;

#pragma warning disable IDE0051  // unreferenced
#pragma warning disable IDE0052  // assigned to but never use

namespace SineTableGen
{
    public partial class MainWindow : Window
    {
        const int tableSize = 32;

        readonly List<FP> tableEntries = new List<FP> ();
        readonly List<FP> stepToNext   = new List<FP> ();

        static readonly double phaseStep = 2 * Math.PI / tableSize;

        public MainWindow ()
        {
            InitializeComponent ();
        }

        //*************************************************************************



        private void Figure_Loaded (object sender, RoutedEventArgs e)
        {
            try
            {
                tableEntries.Clear ();

                figure.DataAreaTitle = "Sine Wave";

                List<Point> wave = new List<Point> ();
                int index = 0;

             // table entries for a given phase
                for (double phase = 0; phase<2*Math.PI; phase+=phaseStep)
                {
                    double s = Math.Sin (phase);
                    tableEntries.Add (new FP (s));
                    wave.Add (new Point (index++, s));
                }

             // step to next entry
                for (index = 0; index<tableSize-1; index++)
                {
                    stepToNext.Add (tableEntries [index+1] - tableEntries [index]);
;               }

                stepToNext.Add (tableEntries [0] - tableEntries [index]);

                PointView pv = new PointView (wave);
                pv.Color = Brushes.Red;
                pv.Size = 0.2;

                LineView lv = new LineView (wave);

                figure.Plot (pv);
                figure.Plot (lv);
                figure.XAxisLabel = "Index";
                figure.RectangularGridOn = true;





                List<Point> HiResSine = new List<Point> ();

                for (double phase = 0; phase<2*Math.PI; phase+=2 * Math.PI / 1024)
                    HiResSine.Add (new Point (tableSize * phase / (2 * Math.PI), Math.Sin (phase)));

                LineView lv2 = new LineView (HiResSine);
                lv2.Color = Brushes.LightGray;
                figure.Plot (lv2);





                PrintButton.IsEnabled = true;
                WriteFileButton.IsEnabled = true;
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception: " + ex.Message);
                //Console.WriteLine (ex.StackTrace);
            }
        }

        //*************************************************************************

        private void PrintButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine ("Table:");

                foreach (FP s in tableEntries)
                {
                    Console.WriteLine (Convert.ToString (s).PadLeft (4, '0'));
                }

                Console.WriteLine ("Step:");

                foreach (FP s in stepToNext)
                {
                    Console.WriteLine (Convert.ToString (s).PadLeft (4, '0'));
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception: " + ex.Message);
            }
        }

        //*************************************************************************

        // for Verilog

        private void WriteFileButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                StreamWriter sw = new StreamWriter (@"..\..\sine.txt");
                WriteTable ("SineTable", sw);
                sw.WriteLine ("");
                WriteSteps ("StepTable", sw);
                sw.Close ();
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception: " + ex.Message);
                //Console.WriteLine (ex.StackTrace);
            }
        }

        //*************************************************************************

        const int NumbersPerRow = 4;

        private void WriteList (string Name, List<FP> table, StreamWriter sw)
        {
            int Counter = 0;
            
            string fileString = "        ";

            for (int i = 0; i<table.Count; i++)
            {
                fileString += Name + " [" + i + "] = 32'h" + Convert.ToString (table [i]).PadLeft (4, '0') + ";  ";

                if (++Counter == NumbersPerRow)
                {
                    sw.WriteLine (fileString);
                    fileString = "        ";
                    Counter = 0;
                }
            }
        }

        private void WriteTable (string Name, StreamWriter sw)
        {
            WriteList (Name, tableEntries, sw);
        }

        private void WriteSteps (string Name, StreamWriter sw)
        {
            WriteList (Name, stepToNext, sw);
        }
    }
}



