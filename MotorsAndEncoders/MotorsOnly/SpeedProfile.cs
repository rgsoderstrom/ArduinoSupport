using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MotorsOnly
{
    public class SpeedProfile
    {
        //**********************************************************************

        public struct Sample
        {
            public Sample (double sec, int left, int right)
            {
                seconds = sec; leftVel = left; rightVel = right;
            }

            public double seconds;
            public int leftVel;
            public int rightVel;
        }

        //**********************************************************************

        private static double InterpolatedTimeResolution = 0.1f;

        private readonly List<Sample> coarseProfile; // as read from file
        private readonly List<Sample> fineProfile;   // interpolated to TimeResolution
        public int NumberProfileSamples {get {return fineProfile.Count;}}

        public SpeedProfile (string fileFullName)
        {
            coarseProfile = ReadScenarioFile (fileFullName);
            fineProfile = InterpolateScenario (coarseProfile, InterpolatedTimeResolution);


        }

        //**********************************************************************

        public void GetPlotData (ref List<Point> left, ref List<Point> right)
        {
            foreach (Sample sam in coarseProfile)
            {
                left.Add (new Point (sam.seconds, sam.leftVel));
                right.Add (new Point (sam.seconds, sam.rightVel));
            }
        }

        //**********************************************************************

        public int GetMessageData (int startIndex, int numberToReturn, List<double> left, List<double> right)
        {
            int counter = 0;

            for (int i=0; i<numberToReturn; i++)
            {
                if (startIndex + i < NumberProfileSamples)
                {
                    left.Add (fineProfile [startIndex + i].leftVel);
                    right.Add (fineProfile [startIndex + i].rightVel);
                    counter++;
                }

                else
                    break;
            }

            return counter;
        }

        //************************************************************************************************************************

        private List<Sample> ReadScenarioFile (string fileName)
        {
            List<Sample> scenario = new List<Sample> ();

            try
            {
                StreamReader file = new StreamReader (fileName);
                string raw;

                while ((raw = file.ReadLine ()) != null)
                {
                    if (raw.Length > 0 && raw.StartsWith ("//") == false)
                    {
                        string[] numbers = raw.Split (new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);

                        if (numbers.Length == 3)
                        {
                            Sample sample = new Sample (Convert.ToDouble (numbers [0]), Convert.ToInt32 (numbers [1]), Convert.ToInt32 (numbers [2]));
                            scenario.Add (sample);
                        }
                    }
                }

                file.Close ();
                return scenario;
            }

            catch (FileNotFoundException)
            {
                throw new Exception ("File not found");
            }

            catch (Exception ex)
            {
                throw new Exception ("Exception loading scenario file: " + ex.Message);
            }
        }

        //************************************************************************************************************************

        private List<Sample> InterpolateScenario (List<Sample> lowResolution, double timeStep)
        {
            List<Sample> highResolution = new List<Sample> ();
            
            for (int i = 1; i < lowResolution.Count; i++)
            {
                double dt  = lowResolution [i].seconds  - lowResolution [i-1].seconds;
                double dv1 = lowResolution [i].leftVel  - lowResolution [i-1].leftVel;
                double dv2 = lowResolution [i].rightVel - lowResolution [i-1].rightVel;

                double v1 = lowResolution [i-1].leftVel;
                double v2 = lowResolution [i-1].rightVel;

                for (double t = lowResolution [i - 1].seconds; t < lowResolution [i].seconds; t += timeStep)
                {
                    highResolution.Add (new Sample (t, (int) (v1 + 0.5), (int) (v2 + 0.5)));
                    v1 += timeStep * dv1 / dt;
                    v2 += timeStep * dv2 / dt;
                }
            }

            return highResolution;
        }
    }
}





