//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using Common;
//using ArduinoInterface;

//namespace ArduinoSimulator
//{
//    class DriveWheelEncoders
//    {
//        static public readonly double samplePeriod = 0.1; // seconds

//        List<MotorSpeedProfileMsg.Segment> motor1Segments = new List<MotorSpeedProfileMsg.Segment> ();
//        List<MotorSpeedProfileMsg.Segment> motor2Segments = new List<MotorSpeedProfileMsg.Segment> ();

//        public DriveWheelEncoders ()
//        {

//        }

//        //**************************************************************************************

//        public void ClearSpeedProfile ()
//        {
//            motor1Segments.Clear ();
//            motor2Segments.Clear ();
//        }

//        //**************************************************************************************

//        public void AddProfileSegment (MotorSpeedProfileMsg.Segment segment)
//        {
//            if (segment.motorID == 1)
//            {
//                motor1Segments.Add (segment);
//            }

//            else if (segment.motorID == 2)
//            {
//                motor2Segments.Add (segment);
//            }

//            else
//                throw new Exception ("Invalid motor ID");
//        }

//        //**************************************************************************************

//        int sampleGetIndex;

//        public EncoderCountsMessage.Batch GetFirstSampleBatch ()
//        {           
//            ExpandProfileIntoSamples ();
//            sampleGetIndex = 0;
//            return GetNextSampleBatch ();
//        }

//        //**************************************************************************************

//        public EncoderCountsMessage.Batch GetNextSampleBatch ()
//        {
//            EncoderCountsMessage.Batch batch = new EncoderCountsMessage.Batch ();

//            int remaining = samples1.Count - sampleGetIndex; // assumes samples1 and samples2 same length
//            int copyCount = Math.Min (remaining, EncoderCountsMessage.Batch.MaxNumberSamples);

//            for (int i=0; i<copyCount; i++)
//            {
//                batch.counts [i].enc1 = (byte)samples1 [sampleGetIndex];
//                batch.counts [i].enc2 = (byte)samples2 [sampleGetIndex];
//                sampleGetIndex++;
//            }

//            batch.put = (short) copyCount;
//            batch.lastBatch = (short) (remaining > copyCount ? 1 : 0);

//            return batch;
//        }

//        //**************************************************************************************

//        List<double> samples1 = new List<double> ();
//        List<double> samples2 = new List<double> ();

//        private void ExpandProfileIntoSamples ()
//        {
//            double duration1 = 0, duration2 = 0;

//            foreach (MotorSpeedProfileMsg.Segment seg in motor1Segments)
//                duration1 += seg.duration / 10.0;

//            foreach (MotorSpeedProfileMsg.Segment seg in motor2Segments)
//                duration2 += seg.duration / 10.0;

//            for (double t=0; t<=duration1; t += samplePeriod)
//            {
//                double encoderCount = 100 * Math.Sin (2 * 3.14 * t / 5);
//                samples1.Add (encoderCount);
//            }

//            for (double t=0; t<=duration2; t += samplePeriod)
//            {
//                double encoderCount = 75 * Math.Sin (2 * 3.14 * t / 3);
//                samples2.Add (encoderCount);
//            }

//            // pad the shorter of the two so they are same length
//            int diff = Math.Abs (samples1.Count - samples2.Count);
            
//            if (samples1.Count < samples2.Count)
//            {
//                for (int i = 0; i<diff; i++)
//                    samples1.Add (0);
//            }

//            else
//            {
//                for (int i = 0; i<diff; i++)
//                    samples2.Add (0);
//            }
//        }
//    }
//}
