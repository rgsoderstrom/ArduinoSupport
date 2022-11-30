using System;
using System.Collections.Generic;
using System.Windows;

//
// SpeedProfile - Calculate motor speed profiles to run requested course
//

namespace ChassisPath
{
    internal class SpeedProfile
    {        
        //*********************************************************************************************
        //
        // SpeedProfileSegment
        //
        internal struct SpeedSegment
        {
            internal double leftSpeed;  // in "hardware" units
            internal double rightSpeed; 
            internal double avgSpeed {get {return (leftSpeed + rightSpeed) / 2;}}

            internal double duration;   // seconds

            internal SpeedSegment (double sl, double sr, double d) {leftSpeed = sl; rightSpeed = sr; duration = d;}

            public override string ToString ()
            {
                return String.Format ("{0:0.00}, {1:0.00}, {2:0.00}", leftSpeed * 115.0 / 20, rightSpeed * 115.0 / 20, duration);
            }
        }

        // SpeedSegment for each course leg plus a segment with speed = 0 to terminate path.
        public List<SpeedSegment> SpeedSegments  {get; protected set;} = new List<SpeedSegment> ();

        // SpeedProfile plot data
        // point.X = time, point.Y = speed at that time
        public List<Point> LeftPlotData  {get; protected set;} = new List<Point> ();
        public List<Point> RightPlotData {get; protected set;} = new List<Point> ();

        //*********************************************************************************************
        //
        // Constructor
        //

        readonly PrintFunction Print;

        public SpeedProfile (List<Chassis.RequestedCourseLeg> course, PrintFunction pf)
        {
            Print = pf;

            // make sure the chassis is stopped at the end of the course
            if (course [course.Count - 1].speed != 0)
            {
                Chassis.RequestedCourseLeg stop = new Chassis.RequestedCourseLeg ();
                stop.legType = Chassis.PathSegmentType.Straight;
                stop.speed = 0;
                stop.distance = 0.1;
                course.Add (stop);
            }

            SpeedSegments.Clear ();
            LeftPlotData.Clear ();
            RightPlotData.Clear ();

            LeftPlotData.Add (new Point (0, 0));
            RightPlotData.Add (new Point (0, 0));

            double cumTime = 0;

            for (int i=0; i<course.Count; i++)
            {
                // speed at end of previous segment
                double prevSpeed = i == 0 ? 0 : SpeedSegments [i - 1].avgSpeed;

                if (course [i].legType == Chassis.PathSegmentType.Straight)
                {
                    SpeedSegment seg = CalculateStraightSegment (ref cumTime, prevSpeed, course [i]);
                    SpeedSegments.Add (seg);
                }

                else if (course [i].legType == Chassis.PathSegmentType.Curved)
                {
                    List<SpeedSegment> seg = CalculateCurvedSegment (ref cumTime, prevSpeed, course [i]);
                    SpeedSegments.AddRange (seg);
                }

                else
                    throw new Exception ("CalculateSpeedProfileSegments: unrecognized path segment type");
            }
        }

        //*****************************************************************************************

        private SpeedSegment CalculateStraightSegment (ref double cumTime, double prevSpeed, Chassis.RequestedCourseLeg seg)
        {
            // time to transition from prior speed to speed of this leg
            double s1 = prevSpeed;
            double s2 = seg.speed;
            double tt = Math.Abs (s2 - s1) / Chassis.Acceleration;

            // distance traveled in that time
            double avgSpeed = (s1 + s2) / 2;
            double d1 = avgSpeed * tt;

            // distance remaining
            double dr = seg.distance - d1;

            // time to traverse remaining distance
            double tr = 0;

            if (dr > 0 && s2 > 0)
                tr = dr / s2;

            double duration = tt + tr;

            if (duration > 25.5)
                throw new Exception ("CalculateStraightSegment: course leg too long");

            LeftPlotData.Add (new Point (cumTime + tt,       Chassis.InchesPerSecToPWM (seg.speed)));
            LeftPlotData.Add (new Point (cumTime + duration, Chassis.InchesPerSecToPWM (seg.speed)));

            RightPlotData.Add (new Point (cumTime + tt,       Chassis.InchesPerSecToPWM (seg.speed)));
            RightPlotData.Add (new Point (cumTime + duration, Chassis.InchesPerSecToPWM (seg.speed)));

            cumTime += duration;

            SpeedSegment ss = new SpeedSegment (s2, s2, duration);
            return ss;
        }

        //*****************************************************************************************

        //
        // See Visio documentation
        //

        //  Turns
        //  - turns have 3 phases: entry, constant, exit
        //    - entry phase is transitioning from straight path to the turn
        //    - constant speed phase, also constant angular rate
        //    - exit phase transitioning back to straight path 

        private List<SpeedSegment> CalculateCurvedSegment (ref double cumTime, double prevSpeed, Chassis.RequestedCourseLeg seg)
        {
            List<SpeedSegment> ss = new List<SpeedSegment> ();

            try
            {
                // desired wheel path radii during constant speed phase
                double ro = seg.radius + Chassis.DriveWheelWidth / 2; // outer wheel turn radius
                double ri = seg.radius - Chassis.DriveWheelWidth / 2; // inner   "     "    "                

                // wheel speeds during constant speed phase. They are in same ratio as radii and their average is prevSpeed
                double so; // target outer wheel speed
                double si; //   "    inner   "     "

                if (Math.Abs (ri) > 0.01) // not pivoting about inner wheel
                {
                    double k = ro / ri;
                    si = 2 * prevSpeed / (k + 1);
                    so = 2 * prevSpeed - si;

                }
                else // pivoting about inner wheel
                {
                    throw new Exception ("not supported");
                }

                //************************************************************************

                // entry phase

                // estimate angle turned during transition to those speeds
                double t1  = (so - prevSpeed) / Chassis.Acceleration; // time to make speed change. same for both
                double l1o = t1 * 0.5 * (prevSpeed + so); // length outer wheel covers in phase 1
                double l1i = t1 * 0.5 * (prevSpeed + si); //   "    inner   "     "     "   "   "

                // average radius of transition
                double r1 = ((l1o + l1i) / (l1o - l1i)) * (Chassis.DriveWheelWidth / 2);

                // angle turned during transition
                double a1 = l1o / (r1 + Chassis.DriveWheelWidth / 2) * 180 / Math.PI;

                if (seg.angle < 0) a1 *= -1;

                //************************************************************************

                // exit phase, back to straight line travel - same as entry

                double t3 = t1;
                double l3o = l1o;
                double l3i = l1i;
                double r3 = r1;
                double a3 = a1;

                //************************************************************************

                // Constant speed phase

                // angle remaining to be turned in constant speed portion
                double a2 = seg.angle - (a1 + a3);

                // distance to travel while in constant speed portion
                double d2 = Math.Abs (a2 * Math.PI / 180) * seg.radius;
                double t2 = d2 / prevSpeed;

                //************************************************************************

                // assign inner & outer to left & right
                double leftSpeed, rightSpeed;

                if (seg.angle > 0)
                {
                    leftSpeed = si;
                    rightSpeed = so;
                }
                else
                {
                    leftSpeed = so;
                    rightSpeed = si;
                }

                SpeedSegment ss1 = new SpeedSegment (leftSpeed, rightSpeed, t1 + t2);
                SpeedSegment ss2 = new SpeedSegment (prevSpeed, prevSpeed,  t3);

                ss.Add (ss1);
                ss.Add (ss2);

                LeftPlotData.Add  (new Point (cumTime + t1, Chassis.InchesPerSecToPWM (leftSpeed)));
                RightPlotData.Add (new Point (cumTime + t1, Chassis.InchesPerSecToPWM (rightSpeed)));

                LeftPlotData.Add  (new Point (cumTime + t1 + t2, Chassis.InchesPerSecToPWM (leftSpeed)));
                RightPlotData.Add (new Point (cumTime + t1 + t2, Chassis.InchesPerSecToPWM (rightSpeed)));

                LeftPlotData.Add  (new Point (cumTime + t1 + t2 + t3, Chassis.InchesPerSecToPWM (prevSpeed)));
                RightPlotData.Add (new Point (cumTime + t1 + t2 + t3, Chassis.InchesPerSecToPWM (prevSpeed)));

                cumTime += t1 + t2 + t3;
            }

            catch (Exception ex)
            {
                Print ("Exception calculating turn: " + ex.Message);
            }

            return ss;
        }

        //***********************************************************************************

        public override string ToString ()
        {
            int count = 1;
            string str = "========================" + "\n";

            foreach (SpeedSegment ss in SpeedSegments)
                str += count++ + ": " + ss.ToString () + "\n";

            return str;
        }
    }
}
