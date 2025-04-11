using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plot2D_Embedded;

namespace FixedPointTests
{
    interface IFixedPtTest
    {
        void DoCalculations ();
        void DoPlots ();

        Bare2DPlot    PlotArea {get; set;}
        PrintCallback Print    {get; set;}
    }
}
