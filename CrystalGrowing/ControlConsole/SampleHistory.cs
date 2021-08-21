using System;
using System.Collections.Generic;
using System.Windows;
using System.Text;
using System.Threading.Tasks;

using ArduinoInterface;


//
// NEED THREAD LOCKS
//

namespace ControlConsole
{
    public class SampleHistoryRecord
    {
        //DateTime startTime;
        List<TemperatureSample> sampleSet = new List<TemperatureSample> ();

        public SampleHistoryRecord ()
        {
            //startTime = DateTime.Now;
        }

        public void Add (TemperatureSample sam)
        {
            sampleSet.Add (sam);
        }

        public List<Point> GetPlotData ()
        {
            List<Point> rdata = new List<Point> ();

            foreach (TemperatureSample ts in sampleSet)
                rdata.Add (new Point (ts.time / 1000, ts.temperature));

            return rdata;
        }

    }

    public class SampleHistory
    {
        SampleHistoryRecord activeRecord; // record being filled

        List<SampleHistoryRecord> allRecords = new List<SampleHistoryRecord> (); // all records, including one being filled

        public void OpenNewRecord ()
        {
            activeRecord = new SampleHistoryRecord ();
            allRecords.Add (activeRecord);
        }

        public void CloseRecord ()
        {
            //activeRecord = null;
        }

        public void Add (TemperatureSample sam)
        {
          //  if (activeRecord == null)
            //    return;


            if (activeRecord == null)
                throw new Exception ("Null record in SampleHistory");

            activeRecord.Add (sam);
        }

        public void Add (TemperatureMessage msg)
        {
            for (int i = 0; i<msg.NumberSamples; i++)
                Add (msg.Samples [i]);
        }

        public List<List<Point>> GetPlotData ()
        {
            List<List<Point>> rdata = new List<List<Point>> ();

            foreach (SampleHistoryRecord shr in allRecords)
                rdata.Add (shr.GetPlotData ());

            return rdata;
        }
    }
}

