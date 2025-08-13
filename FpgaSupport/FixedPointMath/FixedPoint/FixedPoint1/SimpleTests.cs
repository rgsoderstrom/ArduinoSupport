
/*
    FixedPoint1 - fixed-point number with add and multiply operators

*/

#pragma warning disable  IDE0051

using System.Numerics;
using System.Transactions;

//using FixedPoint = FixedPointLib.FixedPoint_1_7_24;
using FixedPoint = FixedPointLib.FixedPoint_1_5_10;

namespace FixedPoint1
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //*************************************
            //
            // sinusoid
            //
            //int N = 16;
            //Random rand = new Random (3);

            //for (int i = 0; i<N; i++)
            //{
            //    //double m = Math.Sin (2 * Math.PI * i / N);
            //    double m = (rand.NextDouble () - 0.5) * 1;
            //    FixedPoint fp = new FixedPoint (m);

            //    //Int16 offsetBinary = (Int16) (512 + m * 500);
            //    //Console.WriteLine (offsetBinary);

            //    Console.WriteLine (fp.ToString ());

            //    //Console.WriteLine (string.Format ("0x{0:x8}", fp.AsInt));
            //    //Console.WriteLine (string.Format ("{0}", fp.AsDouble));
            //}

            //return;

            //*************************************
            //
            // arithmetic operations
            //
            FixedPoint i1 = new FixedPoint (1.2);
            FixedPoint i2 = new FixedPoint (2.3);

            FixedPoint i3 = i1 * i2;


            Console.WriteLine (i1.ToString ());
            Console.WriteLine (i2.ToString ());
            Console.WriteLine (i3.ToString ());

            double a = i1.AsDouble;
            double b = i2.AsDouble;
            double c = i3.AsDouble;

            Console.WriteLine (a.ToString ());
            Console.WriteLine (b.ToString ());
            Console.WriteLine (c.ToString ());


            FixedPoint oneHalf = new FixedPoint (0.5);
            FixedPoint one     = new FixedPoint (1);

            int aa = oneHalf.AsInt * one.AsInt;
            Console.WriteLine (aa.ToString ("X"));
            Console.WriteLine ((aa >> 10).ToString ("X"));


            return;



            //Random rand = new Random (); // 123);

            //for (int i=0; i<1; i++)
            //{
            //    double a = (rand.NextDouble () - 0.5) * 5;
            //    double b = (rand.NextDouble () - 0.5) * 2;


            //    FixedPoint aa = new FixedPoint (a);
            //    FixedPoint bb = new FixedPoint (b);
            //    FixedPoint c = aa * bb;
            //    FixedPoint d = aa + bb;
            //    FixedPoint e = aa - bb;


            //    Console.Write     (string.Format ("a = " + a.ToString ()));
            //    Console.WriteLine (string.Format (" aa = " + aa.AsDouble.ToString ()));

            //    Console.Write     (string.Format ("b = " + b.ToString ()));
            //    Console.WriteLine (string.Format (" bb = " + bb.AsDouble.ToString ()));

            //    Console.WriteLine (string.Format ("c = " + c.AsDouble.ToString ()));
            //    Console.WriteLine (string.Format ("d = " + d.AsDouble.ToString ()));
            //    Console.WriteLine (string.Format ("e = " + e.AsDouble.ToString ()));

            //}
        }
    }
}
