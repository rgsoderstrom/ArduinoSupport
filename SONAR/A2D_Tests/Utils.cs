using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;

namespace A2D_Tests
{
    static internal class Utils
    {
        internal static bool ConvertTagToInteger (object tag, ref int results)
        {
            bool success = false;

            try
            {
                string s = tag as string;
                results = Int32.Parse (s);
                success = true;
            }

            catch (Exception ex)
            {
                EventLog.WriteLine ("Error converting tag to integer: " + ex.Message);
            }

            return success;
        }

        internal static double PowerSpectrum (double re, double im, double len)
        {
            return (re * re + im * im) / len;
        }

    }
}
