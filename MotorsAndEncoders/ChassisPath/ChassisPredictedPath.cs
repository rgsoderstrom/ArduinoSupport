using System;
using System.Collections.Generic;
using System.Windows;
using System.Text;
using System.Threading.Tasks;

//
// PredictedPath - MobileChassis path prediction
//

namespace ChassisPath
{
    internal class PredictedPath
    {
        public List<State> predictedPath;

        //*******************************************************************************************************
        //
        // Constructor - predict path from requested path segments
        //
        public PredictedPath (State initial, List<Chassis.RequestedCourseLeg> segments)
        {
            predictedPath = new List<State> {initial};

            foreach (Chassis.RequestedCourseLeg leg in segments)
            {
                if (leg.legType == Chassis.PathSegmentType.Straight)
                {
                    State before = predictedPath [predictedPath.Count - 1];
                    predictedPath.Add (PredictStraightMove (before, leg.speed, leg.distance));
                }

                else if (leg.legType == Chassis.PathSegmentType.Curved)
                {
                    State before = predictedPath [predictedPath.Count - 1];
                    predictedPath.AddRange (PredictCircleMove (before, leg.radius, leg.angle));
                }
            }
        }

        //***********************************************************************
        //
        // Return State at start or end of path
        //
        public State InitialState
        {
            get
            {
                if (predictedPath != null)
                    return predictedPath [0];

                throw new Exception ("PredictedPath not initialized");
            }
        }

        public State FinalState
        {
            get
            {
                if (predictedPath != null)
                    return predictedPath [predictedPath.Count - 1];

                throw new Exception ("PredictedPath not initialized");
            }
        }

        //***********************************************************************
        //
        // Return positions along path
        //
        public List<Point> PathPoints
        {
            get
            {
                if (predictedPath == null)
                    throw new Exception ("PredictedPath not initialized");

                List<Point> pts = new List<Point> ();

                foreach (State st in predictedPath)
                    pts.Add (st.position);

                return pts;
            }
        }

        //***********************************************************************
        //
        //  State of the chassis at one time
        //
        public struct State
        {
            public Point position;   // inches
            public double direction; // degrees, 0 is along +X, increasing CCW
        };

        //************************************************************************
        //
        // PredictStraightMove - predict state change from one straight path segment
        //
        private State PredictStraightMove (State prior, double speed, double distance)
        {
            if (speed < 0)
                throw new Exception ("Profile segment speed can't be negative");

            if (speed > 20)
                throw new Exception ("Profile segment max speed is 20 inches per second");

            if (distance <= 0)
                throw new Exception ("Profile segment distance must be greater than 0");

            Point  p1 = prior.position;   // position before this move
            double h1 = prior.direction;  // direction   "    "    "

            Point  p2 = p1 + distance * new Vector (Math.Cos (h1 * Math.PI / 180), Math.Sin (h1 * Math.PI / 180));
            double h2 = h1;

            State after = new State ();
            after.position  = p2;
            after.direction = h2;

            return after;
        }

        //******************************************************************************************
        //
        // PredictCircleMove - predict state change from one curved path segment
        //

        // turnAngle positive is a left turn

        private List<State> PredictCircleMove (State prior, double radius, double turnAngle)
        {
            if (radius < 0)
                throw new Exception ("MoveCircle - radius can't be negative");

            Point  p1 = prior.position;   // position before this move
            double d1 = prior.direction;  // direction   "    "    "

            double ba = d1 + (turnAngle > 0 ? 90 : -90); // bearing angle to circle center
            Point center = p1 + radius * new Vector (Math.Cos (ba * Math.PI / 180), Math.Sin (ba * Math.PI / 180));

            List<Point> circlePoints = CirclePoints (center, radius, ba + 180, turnAngle, 1);
            List<State> path = new List<State> ();

            int i;

            for (i=0; i<circlePoints.Count - 1; i++)
            {
                State st = new State ();
                st.position = circlePoints [i]; 
                path.Add (st);
            }

            State after = new State ();
            after.position = circlePoints [i];
            after.direction = d1 + turnAngle;
            path.Add (after);

            return path;
        }

        //****************************************************************************************************
        //
        //  CirclePoints - utility to calculate points around the center of a turn
        //
        private List<Point> CirclePoints (Point center, double radius, double startAngle, double angleRange, double distanceStep)
        {
            if (distanceStep <= 0)
                throw new Exception ("CirclePoints distanceStep must be positive");

            List<Point> points = new List<Point> ();

            double circumference = 2 * Math.PI * radius;
            double angleStep = circumference == 0 ? angleRange : 360 * distanceStep / circumference;

            if (angleRange < 0)
                angleStep *= -1;

            int N = (int) (0.5 + Math.Abs (angleRange / angleStep));

            for (int i=1; i<=N; i++) 
            {
                double angle = startAngle + i * angleStep;
                double rad = angle * Math.PI / 180;
                Point pt = center + radius * new Vector (Math.Cos (rad), Math.Sin (rad));
                points.Add (pt);
            }

            return points;
        }
    }  
}
