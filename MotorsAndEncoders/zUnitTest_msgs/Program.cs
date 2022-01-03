using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SocketLib;
using ArduinoInterface;

namespace zUnitTest_msgs
{
    class Program
    {
        static void Main (string [] args)
        {
            try
            {
                var msg = new MotorSpeedProfileMsg (1234, 2345, 3456, 5555);


                Console.WriteLine ("first, type : " + msg.GetType ().ToString ());
                Console.WriteLine (msg.ToString ());

                byte [] msgBytes = msg.ToBytes ();

                var msg2 = new MotorSpeedProfileMsg (msgBytes);
                Console.WriteLine ("\n\nsecond, type : " + msg2.GetType ().ToString ());
                Console.WriteLine (msg2.ToString ());


            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception: " + ex.Message);
            }
        }

        static void Print (string str)
        {
            Console.WriteLine (str);
        }
    }
}
