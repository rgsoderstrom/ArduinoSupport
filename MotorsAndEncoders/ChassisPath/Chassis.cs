
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Windows.Media;

using System.Threading; // sleep
using System.Windows.Threading;
using System.Runtime.InteropServices; // for Marshal

using Common;
using ArduinoInterface;
using Plot2D_Embedded;

//using static ChassisPath.PredictedPath;

namespace ChassisPath
{
    public delegate void PrintFunction (string str);

    internal partial class Chassis
    {
        public const double DriveWheelWidth = 16.75;      // inches
        private const double MaxPWMSpeed = 115;
        private const double MaxSpeed = 20; // inches / sec
        public const double Acceleration = 6.8; // inches / (sec ^ 2)

        public PredictedPath PredictedPath {get; protected set;} = null;
        public SpeedProfile  SpeedProfile  {get; protected set;} = null;

        public static double PwmToInchesPerSec (double pwm) {return pwm * MaxSpeed / MaxPWMSpeed;}
        public static double InchesPerSecToPWM (double ips) {return ips * MaxPWMSpeed / MaxSpeed;}

        //************************************************************************
        //  
        // Support for user specified path
        //
        public enum PathSegmentType {Straight, Curved, Off};

        // only 2 of these 4 are valid for any one leg, depending on type
        public struct RequestedCourseLeg
        {
            public PathSegmentType legType;
            public double speed;
            public double distance;
            public double radius;
            public double angle;
        };

        //***********************************************************************
        //
        // Return PlotSymbol
        //
        public ChassisPlotSymbol SymbolAtStart
        {
            get
            {
                if (PredictedPath == null)
                    throw new Exception ("PredictedPath == null");

                PredictedPath.State st = PredictedPath.InitialState;
                return new ChassisPlotSymbol (st.position, st.direction);
            }
        }

        public ChassisPlotSymbol SymbolAtFinish
        {
            get
            {
                if (PredictedPath == null)
                    throw new Exception ("PredictedPath == null");

                PredictedPath.State st = PredictedPath.FinalState;
                return new ChassisPlotSymbol (st.position, st.direction);
            }
        }

        //***********************************************************************
        //
        // Get path points
        //
        public List<Point> PathPoints
        {
            get
            {
                if (PredictedPath == null)
                    throw new Exception ("PredictedPath == null");

                return PredictedPath.PathPoints;
            }
        }

        //***********************************************************************
        //
        // Constructor
        //
        public Chassis (Point InitialPosition, double InitialDirection, List<RequestedCourseLeg> path, PrintFunction print)
        {
            PredictedPath.State initial = new PredictedPath.State ();
            initial.position  = InitialPosition;
            initial.direction = InitialDirection;

            PredictedPath = new PredictedPath (initial, path);
            SpeedProfile  = new SpeedProfile (path, print);

            print (SpeedProfile.ToString ());
        }

        //*******************************************************************************************************
    }
}







