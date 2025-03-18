using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixedPointLib
{
    public class FixedPoint_1_5_10  // _1_5_10 => 1 sign bit, 5 integer bits, 10 fraction bits
    { 
        private static readonly short OneBit = 10;
        private static readonly short One = (short) (1 << OneBit);

        private readonly short intValue;

        public double AsDouble {get {return (double) intValue / One;}}
        public short    AsInt    {get {return intValue;}}

        public FixedPoint_1_5_10 (double a)
        {
            intValue = (short) (a * One);
        }

        // note this is private
        private FixedPoint_1_5_10 (short a)
        {
            intValue = a;
        }

        public override string ToString ()
        {
            return string.Format ("0x{0:x4}, {1}", intValue, AsDouble);
        }

        public static FixedPoint_1_5_10 operator* (FixedPoint_1_5_10 a, FixedPoint_1_5_10 b)
        {
            long aa = a.intValue;
            long bb = b.intValue;
            long cc = (aa * bb) >> OneBit;
            short  c = (short) cc;
            return new FixedPoint_1_5_10 (c);
        }
        
        public static FixedPoint_1_5_10 operator+ (FixedPoint_1_5_10 a, FixedPoint_1_5_10 b)
        {
            long cc = (a.intValue + b.intValue);
            short  c  = (short) cc;
            return new FixedPoint_1_5_10 (c);
        }

        public static FixedPoint_1_5_10 operator- (FixedPoint_1_5_10 a, FixedPoint_1_5_10 b)
        {
            long cc = (a.intValue - b.intValue);
            short  c  = (short) cc;
            return new FixedPoint_1_5_10 (c);
        }
    }
}
