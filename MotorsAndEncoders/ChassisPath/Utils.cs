using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;

namespace ChassisPath
{
    public partial class MainWindow
    {
        //*********************************************************************************************

        // helper function to generate expected profile from values read from OMI grid

        //private List<Point> GenerateExpectedProfile (List<int> speed, List<double> duration)
        //{
        //    List<Point> profile = new List<Point> () {new Point (0, 0)}; // (time, speed)

        //    try
        //    {
        //        const double secondsPerSpeedStep = 1.0 / (2 * 20); // used to estimate time to transition between speeds

        //        double prevEndTime = 0; // always start at (0, 0)
        //        int    prevSpeed = 0;

        //        for (int i = 0; i<speed.Count; i++)
        //        {
        //            double rampTime = Math.Abs (speed [i] - prevSpeed) * secondsPerSpeedStep;
        //            double levelTime = duration [i] - rampTime;

        //            if (levelTime < 0) levelTime = 0;

        //            profile.Add (new Point (prevEndTime + rampTime, speed [i]));
        //            profile.Add (new Point (prevEndTime + rampTime + levelTime, speed [i]));

        //            prevEndTime += duration [i];
        //            prevSpeed   = speed [i];
        //        }
        //    }

        //    catch (Exception ex)
        //    {
        //        EventLog.WriteLine ("GenerateExpectedProfile Exception: " + ex.Message);                
        //    }

        //    return profile;
        //}

        //*********************************************************************************************

        // integrate profile to get the expected total number of steps
        // profile [i].X = time in seconds
        // profile [i].Y = speed at that time

        //private int IntegrateProfile (List<Point> profile)
        //{
        //    int encoderCounts = 0;

        //    for (int i=0; i<profile.Count - 1; i++)
        //    {
        //        double avg = 0.5 * (profile [i].Y + profile [i+1].Y);
        //        double dur = (profile [i+1].X - profile [i].X) * 20;  // 
        //        encoderCounts += (int) (avg * dur);
        //    }

        //    return encoderCounts;
        //}

        //*********************************************************************************************

        // helper function to read values from OMI grid

        //private void ReadProfileGrid (UIElementCollection children, ref List<int> speed, ref List<double> duration)
        //{
        //    try
        //    {
        //        foreach (var child in children)
        //        {
        //            TextBox tb = child as TextBox;

        //            if (tb != null)
        //            {
        //                switch (tb.Tag)
        //                {
        //                    case "00": // row, col coords of text box in grid
        //                    case "10":
        //                    case "20":
        //                    case "30":
        //                    case "40":
        //                    {
        //                        int sp;
        //                        bool success = int.TryParse (tb.Text, out sp);

        //                        if (success)
        //                        {
        //                            if (sp < -127) {sp = -127; tb.Text = sp.ToString ();}
        //                            if (sp >  127) {sp =  127; tb.Text = sp.ToString ();}
        //                            speed.Add (sp);
        //                        }
        //                    }

        //                    break;

        //                    case "01":
        //                    case "11":
        //                    case "21":
        //                    case "31":
        //                    case "41":
        //                    {
        //                        double dur;
        //                        bool success = double.TryParse (tb.Text, out dur);

        //                        if (success)
        //                        {
        //                            if (dur < 0)    {dur = 0; tb.Text = dur.ToString ();}
        //                            if (dur > 25.5) {dur =  25.5; tb.Text = dur.ToString ();}
        //                            duration.Add (dur);
        //                        }
        //                    }

        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    catch (Exception ex)
        //    {
        //        EventLog.WriteLine ("ReadProfileGrid Exception: " + ex.Message);                
        //    }
        //}
    }
}
