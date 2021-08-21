using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ArduinoInterface;


//
// NEED THREAD LOCKS
//     Troelsen Ch 19
//


namespace ControlConsoleTest
{
    public class SampleManager
    {
        ArduinoSim host = null;

        //*********************************************************************

        public SampleManager (ArduinoSim caller)
        {
            host = caller;
        }

        //*********************************************************************

        TemperatureMessage message = null;

        public void Store (UInt32 time, float temperature)
        {
            if (message == null)
                message = new TemperatureMessage ();

            TemperatureSample sample = new TemperatureSample ();
            sample.time = time;
            sample.temperature = temperature;

            message.Add (sample);

            if (message.Full)
            {
                host.SendToConsole (message);
                message.Clear ();
            }
        }

        //*********************************************************************

        public void SendPartialMessage ()
        {
            if (message.Empty == false)
            {
                host.SendToConsole (message);
                message.Clear ();
            }
        }
    }
}
