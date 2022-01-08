using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;

namespace ShaftEncoders
{
    public partial class MainWindow
    {
        //*********************************************************************************************

        // helper function to generate expected profile from values read from OMI grid

        private List<Point> GenerateExpectedProfile (List<int> speed, List<double> duration)
        {
            List<Point> profile = new List<Point> () {new Point (0, 0)}; // (time, speed)

            try
            {
                const double secondsPerSpeedStep = 0.05; // used to estimate time to transition between speeds

                double prevEndTime = 0;
                int prevSpeed = 0;

                for (int i = 0; i<speed.Count; i++)
                {
                    double startTime = prevEndTime + Math.Abs (speed [i] - prevSpeed) * secondsPerSpeedStep;
                    profile.Add (new Point (startTime, speed [i]));

                    if (duration [i] != 0)
                    {
                        double endTime = startTime + duration [i];
                        profile.Add (new Point (endTime, speed [i]));
                        prevEndTime = endTime;
                    }

                    prevSpeed = speed [i];
                }
            }

            catch (Exception ex)
            {
                EventLog.WriteLine ("GenerateExpectedProfile Exception: " + ex.Message);                
            }

            return profile;
        }

        //*********************************************************************************************

        // helper function to read values from OMI grid

        private void ReadProfileGrid (UIElementCollection children, ref List<int> speed, ref List<double> duration)
        {
            try
            {
                foreach (var child in children)
                {
                    TextBox tb = child as TextBox;

                    if (tb != null)
                    {
                        switch (tb.Tag)
                        {
                            case "00":
                            case "10":
                            case "20":
                            case "30":
                            case "40":
                            {
                                int sp;
                                bool success = int.TryParse (tb.Text, out sp);

                                if (success)
                                {
                                    if (sp < -15) { sp = -15; tb.Text = sp.ToString (); }
                                    if (sp >  15) { sp =  15; tb.Text = sp.ToString (); }
                                    speed.Add (sp);
                                }
                            }

                            break;

                            case "01":
                            case "11":
                            case "21":
                            case "31":
                            case "41":
                            {
                                double dur;
                                bool success = double.TryParse (tb.Text, out dur);

                                if (success)
                                {
                                    if (dur < 0) { dur = 0; tb.Text = dur.ToString (); }
                                    if (dur > 25.5) { dur =  25.5; tb.Text = dur.ToString (); }
                                    duration.Add (dur);
                                }
                            }

                            break;
                        }
                    }
                }

                speed.Add (0);
                duration.Add (0);
            }

            catch (Exception ex)
            {
                EventLog.WriteLine ("ReadProfileGrid Exception: " + ex.Message);                
            }
        }
    }
}
