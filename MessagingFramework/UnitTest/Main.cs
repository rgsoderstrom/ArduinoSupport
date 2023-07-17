using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestNS
{

    public class PairOfThings
    { 
        public int Val;
        public string Name;

        public PairOfThings (int a, string b)
        {
            Val = a; Name = b;
        }

        public override string ToString ()
        {
            return Name + ", " + Val.ToString ();
        }
    }

    internal class UnitTest
    {
        static List<PairOfThings> listOfThings = new List<PairOfThings> ();

        public static int Main (string [] args)
        {
            listOfThings.Add (new PairOfThings (3, "ccc"));
            listOfThings.Add (new PairOfThings (1, "aaa"));
            listOfThings.Add (new PairOfThings (5, "eee"));

            PairOfThings p1 = listOfThings.Find (x => x.Val == 1);
         // PairOfThings p1 = listOfThings.Find (x => x.Name == "eee");

            if (p1 == null)
                Console.WriteLine ("not found");
            else
                Console.WriteLine (p1);

            return 0;
        }
    }
}
