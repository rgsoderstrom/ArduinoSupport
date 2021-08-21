using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace zUnitTest_files
{
    partial class Program
    {
        static void Main (string [] args)
        {
            TestRun inst = new TestRun ();
            inst.Run ();
        }
    }

    partial class TestRun
    { 
        EncoderCounts [] data = new EncoderCounts [100];

        public void Run ()
        {
            uint t0 = 1000;
            int j = 0;

            for (int i=0; i<100; i++)
            {
                data [i].time =(uint) (t0 + i);
                data [i].enc1 = (short) (10 + i % 10 + j);
                data [i].enc2 = (short) (30 + i % 10 + j);
                data [i].s1 = (byte) (50 + i % 10 + j);
                data [i].s2 = (byte) (70 + i % 10 + j);

                if (i % 10 == 9)
                    j++;
            }

            string fileName = @"..\..\testData.bin";

            using (BinaryWriter writer = new BinaryWriter(File.Open (fileName, FileMode.Create)))
            {
                for (int i = 0; i<100; i++)
                {
                    writer.Write (data [i].time);
                    writer.Write (data [i].enc1);
                    writer.Write (data [i].enc2);
                    writer.Write (data [i].s1);
                    writer.Write (data [i].s2);
                }
            }

            EncoderCounts [] readback = new EncoderCounts [100];

            if (File.Exists (fileName))
            {
                using (BinaryReader reader = new BinaryReader (File.Open (fileName, FileMode.Open)))
                {
                    for (int i = 0; i<100; i++)
                    {
                        readback [i].time = reader.ReadUInt32 ();
                        readback [i].enc1 = reader.ReadInt16 ();
                        readback [i].enc2 = reader.ReadInt16 ();
                        readback [i].s1 = reader.ReadByte ();
                        readback [i].s2 = reader.ReadByte ();
                    }
                }

            }

        }

        //*********************************************************************
    }
}
