
//
// MessCodeGen2.cs
//      - because MessageCodeGenrator.c was getting too big
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MessageGenerator
{
    internal partial class MessageCodeGenerator
    {
        static void MakeCppIncludeFile (string includeFile, string messageName, List<string []> consAsTokens, List<string []> varsAsTokens)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter (includeFile))
                {
                    CppCpp_Include (sw, messageName, consAsTokens, varsAsTokens);
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception generating C++ include file: " + ex.Message);
            }
        }

        //**********************************************************************************************

        static void MakeCppCodeFile (string cppFile, string messageName, List<string []> dataMemberTokens)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter (cppFile))
                {
                    // file header comment and standard includes
                    CppBeginCodeFile (sw, messageName);

                    // default constructor 
                    CppDefaultConstructor (sw, messageName);

                    // from-bytes ctor
                    CppCpp_FromBytes (sw, messageName, dataMemberTokens);

                    // ToBytes ()
                    CppCpp_ToBytes (sw, messageName, dataMemberTokens);

                    // ToConsole ()
                    CppCpp_ToConsole (sw, messageName, dataMemberTokens);
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception generating C++ code: " + ex.Message);
            }
        }

        //************************************************************************

        static void MakeCsCodeFile (string csFile, string messageNameSpace, string messageName, List<string []> dataMemberTokens)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter (csFile))
                {
                    CsOpenNamespace (sw, messageNameSpace, messageName);
                    CsOpenClass (sw, messageName);
                    CsOpenConstructor (sw, messageName);

                    CppCs_FromBytes (sw, messageName, dataMemberTokens);
                    //foreach (string item in cppCsFromBytes.results)
                    //    sw.WriteLine (item);
                    CsCloseConstructor (sw);

                    //CsOpenToBytesMethod (sw, messageName);
                    //CppCs_ToBytes cppCsToBytes = new CppCs_ToBytes (dataMembers);
                    //foreach (string item in cppCsToBytes.results)
                    //    sw.WriteLine (item);
                    CsCloseToBytesMethod (sw);
                    CsCloseClass (sw);
                    CsCloseNamespace (sw);
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception generating C# code: " + ex.Message);
            }
        }
    }
}
