
using System.Collections.Generic;

namespace MessageGenerator
{
    internal class CppCpp_FromBytes : MsgGenBase
    {
        //
        // Dictionary of custom methods to handle built-in types
        //
        static Dictionary<string, BuiltInTypeToFromBytes> CToByteRules = new Dictionary<string, BuiltInTypeToFromBytes> ()
        {
            {"char",  CharFromBytes},
            {"byte",  CharFromBytes},
            {"int",   IntFromBytes},
            {"short", IntFromBytes},
            {"unsigned_short", IntFromBytes},
            {"float", FloatFromBytes},
            {"Sample", SampleFromBytes},
        };

        static Dictionary<string, BuiltInTypeArrayToFromBytes> CArrayToByteRules = new Dictionary<string, BuiltInTypeArrayToFromBytes> ()
        {
            {"char",  CharArrayFromBytes},
            {"byte",  CharArrayFromBytes},
            {"int",   IntArrayFromBytes},
            {"short", IntArrayFromBytes},
            {"float", FloatArrayFromBytes},
            {"Sample", SampleArrayFromBytes},
        };
        
        //**********************************************************************

        // ctor

        internal CppCpp_FromBytes (List<string> members) : base (members, CToByteRules, CArrayToByteRules)
        {
        }

        //**********************************************************************

        // char qwerty;       
        // char qwe [8]       

        static internal void CharFromBytes (string name, List<string> results)
        {
            results.Add ("    data." + name + " = msgBytes [get]; get += 1;");
            results.Add ("");
        }

        static internal void CharArrayFromBytes (string name, string max, List<string> results)
        {
            results.Add ("    for (int i=0; i<data." + max + "; i++)");
            results.Add ("    {");
            results.Add ("         data." + name + " [i] = msgBytes [get]; get += 1;");
            results.Add ("    }");
            results.Add ("");
        }

        //**********************************************************************

        // int index      
        // int index [8]

        static internal void IntFromBytes (string name, List<string> results)
        {
            results.Add ("    *(((byte *) &data." + name + ") + 0) = msgBytes [get]; get += 1;");
            results.Add ("    *(((byte *) &data." + name + ") + 1) = msgBytes [get]; get += 1;");
            results.Add ("");
        }

        static internal void IntArrayFromBytes (string name, string max, List<string> results)
        {
            results.Add ("     for (int i=0; i<data." + max + "; i++)");
            results.Add ("     {");
            results.Add ("         *(((byte *) &data." + name + " [i]) + 0) = msgBytes [get]; get += 1;");
            results.Add ("         *(((byte *) &data." + name + " [i]) + 1) = msgBytes [get]; get += 1;");
            results.Add ("     }");           
            results.Add ("");
        }

        //**********************************************************************

        // int index      
        // int index [8]

        static internal void FloatFromBytes (string name, List<string> results)
        {
            results.Add ("    *(((byte *) &data." + name + ") + 0) = msgBytes [get]; get += 1;");
            results.Add ("    *(((byte *) &data." + name + ") + 1) = msgBytes [get]; get += 1;");
            results.Add ("    *(((byte *) &data." + name + ") + 2) = msgBytes [get]; get += 1;");
            results.Add ("    *(((byte *) &data." + name + ") + 3) = msgBytes [get]; get += 1;");            
            results.Add ("");
        }

        static internal void FloatArrayFromBytes (string name, string max, List<string> results)
        {
            results.Add ("    for (int i=0; i<data." + max + "; i++)");
            results.Add ("    {");
            results.Add ("        *(((byte *) &data." + name + " [i]) + 0) = msgBytes [get]; get += 1;");
            results.Add ("        *(((byte *) &data." + name + " [i]) + 1) = msgBytes [get]; get += 1;");
            results.Add ("        *(((byte *) &data." + name + " [i]) + 2) = msgBytes [get]; get += 1;");
            results.Add ("        *(((byte *) &data." + name + " [i]) + 3) = msgBytes [get]; get += 1;");
            results.Add ("    }");
            results.Add ("");
        }

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
        
        static internal void SampleFromBytes (string name, List<string> results)
        {
            results.Add ("    *(((byte *) &data." + name + ".enc1) + 0) = msgBytes [get]; get += 1;");
            results.Add ("    *(((byte *) &data." + name + ".enc1) + 1) = msgBytes [get]; get += 1;");
            results.Add ("    *(((byte *) &data." + name + ".enc1) + 2) = msgBytes [get]; get += 1;");
            results.Add ("    *(((byte *) &data." + name + ".enc1) + 3) = msgBytes [get]; get += 1;");            
            results.Add ("    *(((byte *) &data." + name + ".enc2) + 0) = msgBytes [get]; get += 1;");
            results.Add ("    *(((byte *) &data." + name + ".enc2) + 1) = msgBytes [get]; get += 1;");
            results.Add ("    *(((byte *) &data." + name + ".enc2) + 2) = msgBytes [get]; get += 1;");
            results.Add ("    *(((byte *) &data." + name + ".enc2) + 3) = msgBytes [get]; get += 1;");            
            results.Add ("");
        }

        static internal void SampleArrayFromBytes (string name, string max, List<string> results)
        {
            results.Add ("    for (int i=0; i<data." + max + "; i++)");
            results.Add ("    {");
            results.Add ("        *(((byte *) &data." + name + " [i].enc1) + 0) = msgBytes [get]; get += 1;");
            results.Add ("        *(((byte *) &data." + name + " [i].enc1) + 1) = msgBytes [get]; get += 1;");
            results.Add ("        *(((byte *) &data." + name + " [i].enc1) + 2) = msgBytes [get]; get += 1;");
            results.Add ("        *(((byte *) &data." + name + " [i].enc1) + 3) = msgBytes [get]; get += 1;");
            results.Add ("        *(((byte *) &data." + name + " [i].enc2) + 0) = msgBytes [get]; get += 1;");
            results.Add ("        *(((byte *) &data." + name + " [i].enc2) + 1) = msgBytes [get]; get += 1;");
            results.Add ("        *(((byte *) &data." + name + " [i].enc2) + 2) = msgBytes [get]; get += 1;");
            results.Add ("        *(((byte *) &data." + name + " [i].enc2) + 3) = msgBytes [get]; get += 1;");
            results.Add ("    }");
            results.Add ("");
        }

    }
}
