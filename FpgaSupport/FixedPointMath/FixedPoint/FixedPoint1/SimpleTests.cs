
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
            //int N = 15;

            //for (int i = 0; i<N; i++)
            //{
            //    double m = Math.Cos (2 * Math.PI * i / N);
            //    FixedPoint fp = new FixedPoint (m);
            //    Console.WriteLine (fp.ToString ());

            //    //Console.WriteLine (string.Format ("0x{0:x8}", fp.AsInt));
            //    //Console.WriteLine (string.Format ("{0}", fp.AsDouble));
            //}

            //return;

            //*************************************
            //
            // arithmetic operations
            //
            FixedPoint i1 = new FixedPoint (1); // (1.125);
            FixedPoint i2 = new FixedPoint (-1); // (2.250);

            FixedPoint i3 = i1 * i2;
            // FixedPoint i4 = i1 / i2;
            FixedPoint i5 = i1 + i2;
            FixedPoint i6 = i1 - i2;

            Console.WriteLine (i1.ToString ());
            Console.WriteLine (i2.ToString ());
            Console.WriteLine (i3.ToString ());
            Console.WriteLine (i5.ToString ());
            Console.WriteLine (i6.ToString ());

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
