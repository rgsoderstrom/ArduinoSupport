using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SocketLibrary;

namespace ArduinoSimulator
{
    internal class ArduinoSim_Transducers : ArduinoSim
    {
        internal ArduinoSim_Transducers (string name, 
                                         SocketLibrary.TcpClient sock,
                                         PrintCallback ptl) : base (name, sock, ptl)
        {            
        }
    }
}
