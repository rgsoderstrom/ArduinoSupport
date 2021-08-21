//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ArduinoSimulator
//{
//    class Encoder
//    {
//        readonly int thisID = -1;

//        static readonly int [] period = new int [] {10, 20}; // seconds
//        static readonly int [] maxSpeed = new int [] {500, -1000};  // counts per second

//        public Encoder (int id) // id == 0 or 1
//        {
//            thisID = id;
//        }

//        double prevDistance = 0;
//        double prevSpeed = 0;
//        double prevTime = 0;  // seconds

//        public void Clear ()
//        {
//            prevDistance = 0;
//            prevSpeed = 0;
//            prevTime = 0;
//        }

//        public void GetCounts (ulong millisIntoRun, out short iSpeed, out int iDist)
//        {
//            int select = thisID == 0 ? 0 : 1;
//            double time = millisIntoRun / 1000.0; // seconds into run
//            double dt = time - prevTime;

//            double speed = maxSpeed [select] * Math.Sin (2 * Math.PI * time / (period [select]));
//            double avgSpeed = (speed + prevSpeed) / 2;
//            double ds = avgSpeed * dt;
//            double distance = prevDistance + ds;

//            iSpeed = (short)speed;
//            iDist = (int)distance;

//            prevDistance = distance;
//            prevSpeed = speed;
//            prevTime = time;
//        }

//    }
//}
