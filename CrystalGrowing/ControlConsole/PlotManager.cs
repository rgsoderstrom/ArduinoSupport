using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;

using PlottingLibVer2;

namespace ControlConsole
{
    public class PlotManager
    {
        //*****************************************************************

        SampleHistory sampleHistory = null;
        Bare2DPlot temperaturePlot = null;

        public PlotManager (SampleHistory sh, Bare2DPlot tp)
        {
            sampleHistory = sh;
            temperaturePlot = tp;

            temperaturePlot.Hold = false;
            temperaturePlot.RectangularGridOn = true;
            temperaturePlot.AxesTight = true;
        }

     //*****************************************************************

        public void Plot ()
        {
            List<List<Point>> allHistory = sampleHistory.GetPlotData ();

            temperaturePlot.Clear ();
            temperaturePlot.RectangularGridOn = true;
            temperaturePlot.Hold = true;

            EventLog.WriteLine (string.Format ("{0} records", allHistory.Count));


            foreach (List<Point> rec in allHistory)
                temperaturePlot.Plot (rec);
        }
    }
}
