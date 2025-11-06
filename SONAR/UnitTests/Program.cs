
/*
    Console app for simple unit tests
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Sonar1Chan;

namespace SonarUT
{
    class Program
    {
        static SampleHistory history = new SampleHistory (2);

        static void Main (string [] _)
        {
            try
            {
                int N1 = 100;
                byte [] samples1 = new byte [N1];
                for (int i=0; i<N1; i++) samples1 [i] = (byte) (i + 128);

                int N2 = 90;
                byte [] samples2 = new byte [N2];
                for (int i=0; i<N2; i++) samples2 [i] = (byte) (i + 129);

                int N3 = 80;
                byte [] samples3 = new byte [N3];
                for (int i=0; i<N3; i++) samples3 [i] = (byte) (i + 130);

                history.Add (samples1);
                history.Add (samples2);
                history.Add (samples3);

                Console.WriteLine (history.ToString ());

                List<double>? ld = history.GetNewest ();

                while (ld != null)
                {
                    Console.WriteLine (ld.Count);
                    ld = history.GetNext ();
                }

            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception: " + ex.Message);
                Console.WriteLine ("Exception: " + ex.StackTrace);
            }
        }
    }
}
