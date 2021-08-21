using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ArduinoInterface;

namespace zUnitTest_msgs
{
    class Program
    {
        static void Main (string [] args)
        {
            try
            {
                ProfileSectionRcvdMessage ppp = new ProfileSectionRcvdMessage ();


                List<double> profile1 = new List<double> () {1, 2, 3, 4, 5, -1, -2, -3, -4, -5};
                List<double> profile2 = new List<double> () {31, 32, 33, 34, 35, -31, -32, -33, -34, -35};
                ProfileSectionMsg psm = new ProfileSectionMsg (123, profile1, profile2);

                byte[] pb = psm.ToBytes ();

                ProfileSectionMsg psm2 = new ProfileSectionMsg (pb);

                //ClearProfileMsg cpm = new ClearProfileMsg ();

                //BufferStatusMessage bm = new BufferStatusMessage (0);
                //byte [] tb = bm.ToBytes ();
                //BufferStatusMessage bm2 = new BufferStatusMessage (tb);

                //Console.WriteLine (bm2.data);


            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception: " + ex.Message);
            }
        }

        static void Print (string str)
        {
            Console.WriteLine (str);
        }
    }
}
