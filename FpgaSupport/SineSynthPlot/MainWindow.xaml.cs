using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace SineSynthPlot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow ()
        {
            InitializeComponent ();
        }

        //***********************************************************************

        static List<Point> PlotData1 = new List<Point> ();
        static List<Point> PlotData2 = new List<Point> ();
        static List<Point> PlotData3 = new List<Point> ();
        static List<Point> PlotData4 = new List<Point> ();
        static List<Point> PlotData5 = new List<Point> ();

      //string fileName = @"C:\Users\rgsod\Documents\FPGA\Xilinx\projects\SineFromTable\SineFromTable.sim\sim_1\behav\xsim\simulate.log";
        string fileName = @"C:\Users\rgsod\Documents\FPGA\Xilinx\projects\CORDIC1\CORDIC1.sim\sim_1\behav\xsim\simulate.log";

        // DATA:  1024,    314, 

        private void LoadButton_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                PlotData1.Clear ();
                PlotData2.Clear ();
                PlotData3.Clear ();
                PlotData4.Clear ();
                PlotData5.Clear ();

                using (StreamReader sr = new StreamReader (fileName))
                {
                    string line;

                    while ((line = sr.ReadLine ()) != null)
                    {
                        if (line.Contains ("x"))
                            continue;

                        string[] tokens = line.Split (new char [] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);

                        // U1.cordicOut, U1.windowOut, U1.multiplierOut, U1.subtracterOut, U1.dac_input

                        if (tokens.Length > 1)
                        {
                            if (tokens [1].Trim () == "DATA")
                            {
                                double t  = Double.Parse (tokens [0].Trim ());
                                double d1 = Double.Parse (tokens [2].Trim ()) / 4096;
                                double d2 = Double.Parse (tokens [3].Trim ()) / 4096;
                                double d3 = Double.Parse (tokens [4].Trim ()) / 4096;
                                double d4 = Double.Parse (tokens [5].Trim ()) / 4096;
                                double d5 = Double.Parse (tokens [6].Trim ()) / 1024;

                                if (PlotData1.Count == 0 || PlotData1 [PlotData1.Count - 1].Y != d1) PlotData1.Add (new Point (t, d1));
                                if (PlotData2.Count == 0 || PlotData2 [PlotData2.Count - 1].Y != d2) PlotData2.Add (new Point (t, d2));
                                if (PlotData3.Count == 0 || PlotData3 [PlotData3.Count - 1].Y != d3) PlotData3.Add (new Point (t, d3));
                                if (PlotData4.Count == 0 || PlotData4 [PlotData4.Count - 1].Y != d4) PlotData4.Add (new Point (t, d4));
                                if (PlotData5.Count == 0 || PlotData5 [PlotData5.Count - 1].Y != d5) PlotData5.Add (new Point (t, d5));

                                //Console.WriteLine (line);
                            }
                        }
                    }
                }

                PlotButton.IsEnabled = true;
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception: " + ex.Message);
            }
        }

        //***********************************************************************

        private void PlotButton_Click (object sender, RoutedEventArgs e)
        {
            figure.Clear ();
            PlotOneTrace (PlotData1);
            //PlotOneTrace (PlotData2);
            //PlotOneTrace (PlotData3);
            //PlotOneTrace (PlotData4);
            PlotOneTrace (PlotData5);
        }

        //***********************************************************************

        private void PlotOneTrace (List<Point> plotData)
        {
            try
            {
                Vector s = plotData [2] - plotData [1];
                double dotSize = Math.Min (s.X, s.Y) / 5;

                PointView pv = new PointView (plotData);//, PointView.DrawingStyle.Circle);
                LineView lv = new LineView (plotData);
                pv.Size = dotSize;

                figure.Plot (pv);
                figure.Plot (lv);
                figure.RectangularGridOn = true;
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception: " + ex.Message);
            }
        }
    }
}
