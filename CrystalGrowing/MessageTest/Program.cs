using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using SocketLib;
using ArduinoInterface;

namespace UnitTest
{
    class Program
    {
        static DateTime startTime = DateTime.Now;

        static UInt32 millis ()
        {
            DateTime now = DateTime.Now;
            TimeSpan messageTimeTag = now - startTime;
            return (UInt32) messageTimeTag.TotalMilliseconds;
        }





        static void Main ()
        {
            try
            {
                UInt32 baseTime = millis ();

                Thread.Sleep (100);
                
                TemperatureMessage msg = new TemperatureMessage (/*millis () - baseTime*/);

                TemperatureSample sam = new TemperatureSample ();
                sam.time = 123123;
                sam.temperature = 123;

                msg.Add (sam);

                sam.time = 111111;
                sam.temperature = 99;

                msg.Add (sam);

                Console.WriteLine (msg.ToString ());

                Thread.Sleep (100);
                msg.Clear (/*millis () - baseTime*/);
                Console.WriteLine (msg.ToString ());

                sam.time = 222222;
                sam.temperature = 456;
                msg.Add (sam);
                sam.time = 333333;
                sam.temperature = 789;
                msg.Add (sam);
                sam.time = 444444;
                sam.temperature = 1011;
                msg.Add (sam);

                Console.WriteLine (msg.ToString ());

                Console.WriteLine ("**************************************");

                /***/
                byte [] msgBytes = msg.ToBytes ();


                TemperatureMessage msg2 = new TemperatureMessage (msgBytes);

                Console.WriteLine (msg2.ToString ());
                /***/

            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception: {0}", ex.Message);
                Console.WriteLine ("Exception: {0}", ex.StackTrace);
            }
        }
    }
}
