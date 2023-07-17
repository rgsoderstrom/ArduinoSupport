//
// ArduinoSimulator - emulate the socket traffic from an Arduino. 
//
using System;
using System.Threading;
using System.Threading.Tasks;

using Common;

namespace ArduinoSimulator
{
    public class ConsoleTest
    {
        public static int Main (string [] args)
        {
            EventLog.Open (@"..\..\Log.txt", true);
            EventLog.WriteLine ("Arduino Simulator");

            try
            {
                ArduinoSim arduino1 = new ArduinoSim ("ard1", 9999);
                ArduinoSim arduino2 = new ArduinoSim ("ard2", 9999);

                Task [] allTasks =
                {
                        new Task (arduino1.Run),
                        new Task (arduino2.Run)
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