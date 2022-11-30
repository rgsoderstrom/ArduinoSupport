using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ChassisPath
{
    public partial class MainWindow
    {
        //*******************************************************************************************************
        //
        // ReadRequestedCourse - 
        //
        private TextBox FindTextBoxFromTag (string tag) {foreach (var child in TextBox_Grid.Children) {if (child is TextBox tb) {if ((string) tb.Tag == tag) return tb;}} return null;}

        private List<Chassis.RequestedCourseLeg> ReadRequestedCourse ()
        {
            List<Chassis.RequestedCourseLeg> requestedCourse = new List<Chassis.RequestedCourseLeg> ();

            if (MainWindow.segmentTypes != null)
            {
                try
                {
                    for (int i = 0; i<MainWindow.segmentTypes.Count; i++)
                    {
                        if (MainWindow.segmentTypes [i] == Chassis.PathSegmentType.Straight)
                        {
                            string tag1 = (i+1).ToString () + 0;
                            string tag2 = (i+1).ToString () + 1;

                            TextBox tb1 = FindTextBoxFromTag (tag1);
                            TextBox tb2 = FindTextBoxFromTag (tag2);

                            if (tb1 == null || tb2 == null)
                                throw new Exception ("Error reading profile segment");

                            Chassis.RequestedCourseLeg leg = new Chassis.RequestedCourseLeg ();
                            leg.legType  = Chassis.PathSegmentType.Straight;
                            leg.speed    = Convert.ToDouble (tb1.Text);
                            leg.distance = Convert.ToDouble (tb2.Text);

                            requestedCourse.Add (leg);
                        }

                        if (MainWindow.segmentTypes [i] == Chassis.PathSegmentType.Curved)
                        {
                            string tag1 = (i+1).ToString () + 2;
                            string tag2 = (i+1).ToString () + 3;

                            TextBox tb1 = FindTextBoxFromTag (tag1);
                            TextBox tb2 = FindTextBoxFromTag (tag2);

                            if (tb1 == null || tb2 == null)
                                throw new Exception ("Error reading profile segment");

                            Chassis.RequestedCourseLeg leg = new Chassis.RequestedCourseLeg ();
                            leg.legType = Chassis.PathSegmentType.Curved;
                            leg.radius  = Convert.ToDouble (tb1.Text);
                            leg.angle   = Convert.ToDouble (tb2.Text);

                            requestedCourse.Add (leg);
                        }
                    }
                }

                catch (Exception ex)
                {
                    Print ("Error reading profile: " + ex.Message);
                }
            }

            return requestedCourse;
        }
    }
}
