
//
// CppCpp_ToBytes - C++ code in and C++ code out
//

using System.Collections.Generic;

namespace MessageGenerator
{
    internal class CppCpp_ToBytes : MsgGenBase
    {
        //
        // Dictionary of custom methods to handle built-in types
        //
        static Dictionary<string, BuiltInTypeToFromBytes> CToByteRules = new Dictionary<string, BuiltInTypeToFromBytes> ()
        {
            {"char",  CharToBytes},
            {"byte",  CharToBytes},
            {"int",   IntToBytes},
            {"short", IntToBytes},
            {"unsigned_short", IntToBytes},
            {"float", FloatToBytes},
            {"Sample", SampleToBytes},
        };

        static Dictionary<string, BuiltInTypeArrayToFromBytes> CArrayToByteRules = new Dictionary<string, BuiltInTypeArrayToFromBytes> ()
        {
            {"char",  CharArrayToBytes},
            {"byte",  CharArrayToBytes},
            {"int",   IntArrayToBytes},
            {"short", IntArrayToBytes},
            {"float", FloatArrayToBytes},
            {"Sample", SampleArrayToBytes},
        };

        //**********************************************************************

        // ctor

        internal CppCpp_ToBytes (List<string> members) : base (members, CToByteRules, CArrayToByteRules)
        {
        }

        //**********************************************************************

        // char qwerty;       
        // char qwe [8]       

        static internal void CharToBytes (string name, List<string> results)
        {
            results.Add ("    byteArray [put++] = data." + name + ";");
            results.Add ("");
        }

        static internal void CharArrayToBytes (string name, string max, List<string> results)
        {
            results.Add ("    for (int i=0; i<data." + max + "; i++)");
            results.Add ("    {");
            results.Add ("        byteArray [put++] = data." + name + " [i];");
            results.Add ("    }");
            results.Add ("");
        }

        //**********************************************************************

        // int index      
        // int index [8]

        static internal void IntToBytes (string name, List<string> results)
        {
            results.Add ("    byteArray [put++] = data." + name + ";");
            results.Add ("    byteArray [put++] = data." + name + " >> 8;");
            results.Add ("");
        }

        static internal void IntArrayToBytes (string name, string max, List<string> results)
        {
            results.Add ("    for (int i=0; i<data." + max + "; i++)");
            results.Add ("    {");
            results.Add ("        byteArray [put++] = data." + name + " [i];");
            results.Add ("        byteArray [put++] = data." + name + " [i] >> 8;");
            results.Add ("    }");           
            results.Add ("");
        }

        //**********************************************************************

        static internal void FloatToBytes (string memberVariable, List<string> results)
        {
            results.Add ("    byteArray [put++] = (*((unsigned long *) &" + "data" + "." + memberVariable + ") >>  0)  & 0xff;");
            results.Add ("    byteArray [put++] = (*((unsigned long *) &" + "data" + "." + memberVariable + ") >>  8)  & 0xff;");
            results.Add ("    byteArray [put++] = (*((unsigned long *) &" + "data" + "." + memberVariable + ") >> 16)  & 0xff;");
            results.Add ("    byteArray [put++] = (*((unsigned long *) &" + "data" + "." + memberVariable + ") >> 24)  & 0xff;");
            results.Add ("");
        }

        static internal void FloatArrayToBytes (string memberVariable, string Count, List<string> results)
        {
            results.Add ("    for (int i=0; i<data." + Count + "; i++)");
            results.Add ("    {");
            results.Add ("        byteArray [put++] = (*((unsigned long *) &" + "data" + "." + memberVariable + " [i]) >>  0)  & 0xff;");
            results.Add ("        byteArray [put++] = (*((unsigned long *) &" + "data" + "." + memberVariable + " [i]) >>  8)  & 0xff;");
            results.Add ("        byteArray [put++] = (*((unsigned long *) &" + "data" + "." + memberVariable + " [i]) >> 16)  & 0xff;");
            results.Add ("        byteArray [put++] = (*((unsigned long *) &" + "data" + "." + memberVariable + " [i]) >> 24)  & 0xff;");
            results.Add ("    }");
            results.Add ("");
        }

        //static internal void FloatToBytes (string structureVariable, string memberVariable, List<string> results)
        //{
        //    results.Add ("    byteArray [put++] = (*((unsigned long *) &" + structureVariable + "." + memberVariable + ") >>  0)  & 0xff;");
        //    results.Add ("    byteArray [put++] = (*((unsigned long *) &" + structureVariable + "." + memberVariable + ") >>  8)  & 0xff;");
        //    results.Add ("    byteArray [put++] = (*((unsigned long *) &" + structureVariable + "." + memberVariable + ") >> 16)  & 0xff;");
        //    results.Add ("    byteArray [put++] = (*((unsigned long *) &" + structureVariable + "." + memberVariable + ") >> 24)  & 0xff;");
        //    results.Add ("");
        //}

        //static internal void FloatArrayToBytes (string structureVariable, string memberVariable, string Count, List<string> results)
        //{
        //    results.Add ("    for (int i=0; i<data." + Count + "; i++)");
        //    results.Add ("    {");
        //    results.Add ("        byteArray [put++] = (*((unsigned long *) &" + structureVariable + "." + memberVariable + " [i]) >>  0)  & 0xff;");
        //    results.Add ("        byteArray [put++] = (*((unsigned long *) &" + structureVariable + "." + memberVariable + " [i]) >>  8)  & 0xff;");
        //    results.Add ("        byteArray [put++] = (*((unsigned long *) &" + structureVariable + "." + memberVariable + " [i]) >> 16)  & 0xff;");
        //    results.Add ("        byteArray [put++] = (*((unsigned long *) &" + structureVariable + "." + memberVariable + " [i]) >> 24)  & 0xff;");
        //    results.Add ("    }");
        //    results.Add ("");
        //}

        //**********************************************************************

        // struct Sample
        // {
        //      float enc1;
        //      float enc2;
        //  }
        //
        //  TypeData data
        //  {
        //      Sample sample;
        //  }
        //
        static internal void SampleToBytes (string name, List<string> results)
        {
            results.Add ("    byteArray [put++] = (*((unsigned long *) &data." + name + ".enc1) >>  0)  & 0xff;");
            results.Add ("    byteArray [put++] = (*((unsigned long *) &data." + name + ".enc1) >>  8)  & 0xff;");
            results.Add ("    byteArray [put++] = (*((unsigned long *) &data." + name + ".enc1) >> 16)  & 0xff;");
            results.Add ("    byteArray [put++] = (*((unsigned long *) &data." + name + ".enc1) >> 24)  & 0xff;");

            results.Add ("    byteArray [put++] = (*((unsigned long *) &data." + name + ".enc2) >>  0)  & 0xff;");
            results.Add ("    byteArray [put++] = (*((unsigned long *) &data." + name + ".enc2) >>  8)  & 0xff;");
            results.Add ("    byteArray [put++] = (*((unsigned long *) &data." + name + ".enc2) >> 16)  & 0xff;");
            results.Add ("    byteArray [put++] = (*((unsigned long *) &data." + name + ".enc2) >> 24)  & 0xff;");
            results.Add ("");
        }

        static internal void SampleArrayToBytes (string name, string Count, List<string> results)
        {
            results.Add ("    for (int i=0; i<data." + Count + "; i++)");
            results.Add ("    {");

            results.Add ("        byteArray [put++] = (*((unsigned long *) &data." + name + " [i].enc1) >>  0)  & 0xff;");
            results.Add ("        byteArray [put++] = (*((unsigned long *) &data." + name + " [i].enc1) >>  8)  & 0xff;");
            results.Add ("        byteArray [put++] = (*((unsigned long *) &data." + name + " [i].enc1) >> 16)  & 0xff;");
            results.Add ("        byteArray [put++] = (*((unsigned long *) &data." + name + " [i].enc1) >> 24)  & 0xff;");

            results.Add ("        byteArray [put++] = (*((unsigned long *) &data." + name + " [i].enc2) >>  0)  & 0xff;");
            results.Add ("        byteArray [put++] = (*((unsigned long *) &data." + name + " [i].enc2) >>  8)  & 0xff;");
            results.Add ("        byteArray [put++] = (*((unsigned long *) &data." + name + " [i].enc2) >> 16)  & 0xff;");
            results.Add ("        byteArray [put++] = (*((unsigned long *) &data." + name + " [i].enc2) >> 24)  & 0xff;");

            results.Add ("    }");
            results.Add ("");
        }
    }
}
