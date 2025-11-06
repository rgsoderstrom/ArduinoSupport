using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar1Chan
{
    internal class SampleHistory
    {
        //********************************************************

        private List<List<double>> Batches = new List<List<double>> ();
         
        private readonly int MaxNumberBatches;

        //********************************************************

        internal SampleHistory (int maxNumberBatches)
        {
            MaxNumberBatches = maxNumberBatches;
        }

        internal void Clear ()
        {
            Batches.Clear ();
        }

        //********************************************************

        // Add data to history

        internal void Add (byte [] src)
        {
            List<double> srcAsDouble = new List<double> (src.Length);

            for (int i = 0; i<src.Length; i++)
            {
                byte b = src [i];
                srcAsDouble.Add (b);
            }

            Add (srcAsDouble);
        }

        internal void Add (List<double> src)
        {
            Batches.Add (src);

            if (Batches.Count > MaxNumberBatches)
                Batches.RemoveAt (0);
        }

        //********************************************************

        // Retrieve data from history

        private int get = -1;

        internal List<double>? GetNewest ()
        {
            if (Batches.Count == 0)
                return null;

            get = Batches.Count - 1;
            return Batches [get--];
        }

        internal List<double>? GetNext ()
        {
            if (get < 0)
                return null;

            return Batches [get--];
        }

        //
        // return string with number of batches and size of each
        //
        public override string ToString ()
        {
            string str = Batches.Count + " batches\n";

            foreach (List<double> b in Batches)
            { 
                str += "  " + b.Count + " samples\n";
                str += "    " + b [0] + ", " + b [b.Count - 1] + "\n";
            }

            return str;
        }
    }
}

