/*
    FixedPoint32 - 32 bit fixed point
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixedPointLib
{
    public class FixedPoint_1_7_24  // _1_7_24 => 1 sign bit, 7 integer bits, 24 fraction bits
    { 
        private static readonly int OneBit = 24;
        private static readonly int One = (1 << OneBit);

        private readonly int intValue;

        public double AsDouble {get {return (double) intValue / One;}}
        public int    AsInt    {get {return intValue;}}

        public FixedPoint_1_7_24 (double a)
        {
            intValue = (int)(a * One);
        }

        // note this is private
        private FixedPoint_1_7_24 (int a)
        {
            intValue = a;
        }

        public override string ToString ()
        {
        //  return string.Format ("0x{0:x8}", intValue);
            return string.Format ("0x{0:x8}, {1}", intValue, AsDouble);
        }

        public static FixedPoint_1_7_24 operator* (FixedPoint_1_7_24 a, FixedPoint_1_7_24 b)
        {
            long aa = a.intValue;
            long bb = b.intValue;
            long cc = (aa * bb) >> OneBit;
            int  c = (int) cc;
            return new FixedPoint_1_7_24 (c);
        }
        
        public static FixedPoint_1_7_24 operator+ (FixedPoint_1_7_24 a, FixedPoint_1_7_24 b)
        {
            long cc = (a.intValue + b.intValue);
            int  c  = (int) cc;
            return new FixedPoint_1_7_24 (c);
        }

        public static FixedPoint_1_7_24 operator- (FixedPoint_1_7_24 a, FixedPoint_1_7_24 b)
        {
            long cc = (a.intValue - b.intValue);
            int  c  = (int) cc;
            return new FixedPoint_1_7_24 (c);
        }
    }
}
