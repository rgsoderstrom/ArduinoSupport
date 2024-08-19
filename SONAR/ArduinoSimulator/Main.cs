//
// ArduinoSimulator - emulate the socket traffic from an Arduino. 
//

// started as "Empty Project (.Net Framework)"

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Common;

namespace ArduinoSimulator
{
    public class ConsoleTest
    {
        // Parameters initially set to default values
        static string ServerName = "RandysLaptop";
        //static string ServerName = "RandysLG";

        static double SampleRate = 4096;
        static int    BatchSize  = 1024;
        static double Frequency  = 128;

        //************************************************************************

        public static int Main (string [] args)
        {
            EventLog.Open (@"..\..\SimulatorLog.txt", true);
            EventLog.WriteLine ("Arduino Simulator");

            try
            { 
                for (int i=0; i<args.Length; i+=2)
                {
                    //Console.WriteLine (i.ToString () + ": " + args [i] + ", " + args [i+1]);

                    if (i + 1 == args.Length) // not enough args passed in
                        break;

                    switch (args [i])
                    {
                        case "ServerName":
                            ServerName = args [i+1];
                            break;

                        case "SampleRate":
                            SampleRate = Convert.ToDouble (args [i+1]);
                            break;

                        case "Frequency":
                            Frequency = Convert.ToDouble (args [i+1]);
                            break;

                        case "BatchSize":
                            BatchSize = Convert.ToInt32 (args [i+1]);
                            break;

                        default:
                            Console.WriteLine ("Unrecognized arg: " + args [i+1]);
                            break;
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception parsing input arguments: " + ex.Message);
            }

            try
            {
                ArduinoSim arduino1 = new ArduinoSim ("ard1",
                                                      ServerName, 
                                                      SampleRate,
                                                      BatchSize,
                                                      Frequency);

                //ArduinoSim arduino2 = new ArduinoSim ("ard2");

                Task [] allTasks =
                {
                        new Task (arduino1.Run),
                        //new Task (arduino2.Run)
                };

                foreach (Task t in allTasks)
                {
                    t.Start ();     
                    Thread.Sleep (1000);
                }

                Task.WaitAll (allTasks);

                Console.WriteLine ("All tasks complete");
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception: {0}", ex.Message));
            }

            EventLog.Close ();
            //Console.WriteLine ("Hit a key to exit");
           // var c = Console.ReadKey ();                
            return 0;
        }
    }
}