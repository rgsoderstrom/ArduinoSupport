
//
// ArduinoSimulator - emulate the socket traffic from an Arduino. 
//

// Start with:
// Empty Project (.NET Framework)
// this file manually added with "Add New Item -> class"

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Common;

namespace ArduinoSimulator
{
    internal class MainClass
    {
        static string ServerName;
        static string SimulatorName;

        private static void PrintToLog (string str)
        {
            Common.EventLog.WriteLine (str);
            Console.WriteLine (str);
        }

        public static int Main (string [] args)
        {
            //
            // Parse command line arguments
            //
            int InstanceCount = 1;

            try
            { 
                for (int i=0; i<args.Length; i+=2)
                {
                    if (i + 1 == args.Length) // error if odd number of args passed in. Must be (name, value) pairs
                        break;

                    switch (args [i])
                    {
                        case "SimName":
                            SimulatorName = args [i+1];
                            break;

                        case "ServerName":
                            ServerName = args [i+1];
                            break;

                        case "Instance":
                            InstanceCount = Convert.ToInt32 (args [i+1]);
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

            //
            // Open log file
            //
            try
            { 
                string fname = @"..\..\LogSimulator" + InstanceCount.ToString () + ".txt";
                EventLog.Open (fname, true);
                PrintToLog ("Arduino simulator started");
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception opening log file: " + ex.Message);
            }

            //
            // Connect to server and create specified simulator
            //
            try
            {
                PrintToLog (SimulatorName + " connecting to server " + ServerName);

                SocketLibrary.TcpClient thisClientSocket = new SocketLibrary.TcpClient (ServerName, PrintToLog); 

                if (thisClientSocket.Connected == false)
                {
                    PrintToLog ("\n\nFailed to connect to server");

                    while (true)
                        Thread.Sleep (1000);
                }

                thisClientSocket.PrintHandler += PrintToLog;

                //**********************************************************************************************

                ArduinoSimBase arduino1;

                arduino1 = new ArduinoSim_Transducers  ("ard1", thisClientSocket, PrintToLog);

                //if      (SimulatorName == "A2D_Tests")  arduino1 = new ArduinoSim_A2D_Tests  ("ard1", thisClientSocket, PrintToLog);
                //else if (SimulatorName == "Sonar1Chan") arduino1 = new ArduinoSim_Sonar1Chan ("ard1", thisClientSocket, PrintToLog);
                //else throw new Exception ("Unrecognized simulator type requested");

                Task [] allTasks =
                {
                    new Task (arduino1.Run),
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
            return 0;
        }
    }
}
