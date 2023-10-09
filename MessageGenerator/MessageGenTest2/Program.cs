
//
// Unit test for auto generated .cs message code
//

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
                AcknowledgeMessage msg1 = new AcknowledgeMessage (0x4321);

                //for (int i = 0; i<LoopbackDataMsg_Auto.Data.MaxCount; i++)
                  //  msg1.data.dataWords [i] = (byte) (0x76 + i);

                Console.WriteLine ("Initial:");
                Console.WriteLine (msg1.ToString ());

                Console.WriteLine ("-------------------------------------");

                byte [] msgBytes = msg1.ToBytes ();

                Console.WriteLine (msgBytes.Length + " bytes");

                for (int i = 0; i<msgBytes.Length; i++)
                    Console.WriteLine (string.Format ("{0}: {1}, 0x{2:x}", i, msgBytes [i], msgBytes [i]));

                Console.WriteLine ("-------------------------------------");

                Console.WriteLine ("Copied:");
                AcknowledgeMessage msg2 = new AcknowledgeMessage (msgBytes);
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