
using System;

using ArduinoInterface;

namespace MessageGenTest1
{
    internal class Program
    {
        static void Main (string [] args)
        {
            try
            {
                StatusMessage msg1 = new StatusMessage ();
                msg1.SetName ("abc");

                Console.WriteLine (msg1.ToString ());

                byte [] msgBytes = msg1.ToBytes ();

                for (int i = 0; i<msgBytes.Length; i++)
                    Console.WriteLine (string.Format ("{0}: {1}, 0x{2:x}", i, msgBytes [i], msgBytes [i]));

                Console.WriteLine ("-------------------------------------");

                StatusMessage msg2 = new StatusMessage (msgBytes);
                Console.WriteLine (msg2.ToString ());
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception: " + ex.Message);
                Console.WriteLine ("Exception: " + ex.StackTrace);
            }
        }
    }
}