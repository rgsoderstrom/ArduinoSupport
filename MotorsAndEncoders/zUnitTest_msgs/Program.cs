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
                StatusMessage.StatusData _data = new StatusMessage.StatusData ();

                _data.Name = "pQ3456789012";
                _data.readyForMessages = 1234;
                _data.readyToRun = 2468;
                _data.motorsRunning = 3456;
                _data.readyToSend = 9876;

                var msg = new StatusMessage ();
                msg = new StatusMessage (_data);




                Console.WriteLine ("first, type : " + msg.GetType ().ToString ());
                Console.WriteLine (msg.ToString ());

                byte [] msgBytes = msg.ToBytes ();

                Console.WriteLine ("");

                for (int i = 0; i<msgBytes.Length; i++)
                    Console.WriteLine (msgBytes [i]);


                var msg2 = new StatusMessage (msgBytes);
                Console.WriteLine ("\n\nsecond, type : " + msg2.GetType ().ToString ());
                Console.WriteLine (msg2.ToString ());

                Console.WriteLine ("\n\n\nNNNN = " + msg2.data.Name);
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
