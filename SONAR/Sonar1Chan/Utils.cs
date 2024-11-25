
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;

namespace Sonar1Chan
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
    }
}
