//
// ControlConsoleTest - emulate the socket traffic from an Arduino. 
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

                ArduinoSim arduino1 = new ArduinoSim ();
                //ArduinoSim arduino2 = new ArduinoSim ();

                Task [] allTasks = {new Task (arduino1.Run),
                                  //new Task (arduino2.Run)
                };

                foreach (Task t in allTasks)
                {
                    t.Start ();
                }

                Task.WaitAll (allTasks);
            }

            catch (Exception ex)
            {
                EventLog.WriteLine (string.Format ("Exception: {0}", ex.Message));
            }

            //var c = Console.ReadKey ();                
            return 0;
        }

    }
}