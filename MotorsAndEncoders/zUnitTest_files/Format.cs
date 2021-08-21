using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices; // for StructLayout

namespace zUnitTest_files
{
    partial class TestRun
    {
        public class Header
        {
            public ushort Sync;
            public ushort ByteCount;
            public ushort MessageId;
            public ushort SequenceNumber;
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        public struct EncoderCounts
        {
            public uint  time;   
            public short enc1; 
            public short enc2;
            public byte  s1;  
            public byte  s2;
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        public partial class EncCntsData
        {
            static public readonly int HistorySize = 10;

            public short put;
            public EncoderCounts [] counts = new EncoderCounts [HistorySize];
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        public partial class EncoderCountsMessage
        {
            public Header        header;
            public EncCntsData   data;      
        }

    }
}
