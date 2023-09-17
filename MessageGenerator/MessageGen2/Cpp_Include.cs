
//
// CppCpp_Include - C++ code in and C++ .h file out
//

using System;
using System.Collections.Generic;
using System.IO;

namespace MessageGenerator
{
    partial class MessageCodeGenerator
    {
        //**********************************************************************

        // ctor

        public static void CppCpp_Include (StreamWriter sw, string msgName, List<string []> consAsTokens, List<string []> varsAsTokens)
        {
            List<string> fileOpening = BeginFile (msgName);

            foreach (string str in fileOpening)
                sw.WriteLine (str);

            //****************************************************************

            List<string> constants = CodeGenerator_IncludeConstants (consAsTokens);

            foreach (string str in constants)
                sw.WriteLine ("        " + str);

            sw.WriteLine ("");

            //****************************************************************

            List<string> variables = CodeGenerator_IncludeVariables (varsAsTokens);

            foreach (string str in variables)
                sw.WriteLine ("        " + str);

            sw.WriteLine ("        };");

            //****************************************************************

            List<string> fileClosing = EndFile (msgName);

            foreach (string str in fileClosing)
                sw.WriteLine ("        " + str);

            sw.WriteLine ("};");
            sw.WriteLine ("#endif");

        }

        //************************************************************************************************

        static List<string> BeginFile (string msgName)
        {
            // store generated code here
            List<string> codeFragments = new List<string> ();

            codeFragments.Add ("//");
            codeFragments.Add ("// " + msgName + ".h");
            codeFragments.Add ("//   - auto generated code");
            codeFragments.Add ("");

            codeFragments.Add ("#ifndef " + msgName.ToUpper () + "_H");
            codeFragments.Add ("#define " + msgName.ToUpper () + "_H");
            codeFragments.Add ("");

            codeFragments.Add ("#include <Arduino.h>");
            codeFragments.Add ("#include \"MessageIDs.h\"");
            codeFragments.Add ("#include \"MessageHeader.h\"");
            codeFragments.Add ("");

            codeFragments.Add ("class " + msgName);
            codeFragments.Add ("{");
            codeFragments.Add ("    public:");
            codeFragments.Add ("        struct MessageData");
            codeFragments.Add ("        {");


            return codeFragments;
        }

        //************************************************************************************************

        static List<string> CodeGenerator_IncludeConstants (List<string []> memberVariableTokens) 
        {
            // store generated code here
            List<string> codeFragments = new List<string> ();

            foreach (string [] tokens in memberVariableTokens)
            {
                if (tokens.Length == 3)
                {
                    string frag = "    static const " + tokens [0] + " " + tokens [1] + " = " + tokens [2] + ";";
                    codeFragments.Add (frag);
                }

                else
                    throw new Exception ("Error generating include file constants");
            }

            return codeFragments;
        }        

        static List<string> CodeGenerator_IncludeVariables (List<string []> memberVariableTokens) 
        {
            // store generated code here
            List<string> codeFragments = new List<string> ();

            foreach (string [] tokens in memberVariableTokens)
            {
                if (tokens.Length == 2)
                {
                    string frag = "    " + tokens [0] + " " + tokens [1] + ";";
                    codeFragments.Add (frag);
                }

                else if (tokens.Length == 3)
                {
                    string frag = "    " + tokens [0] + " " + tokens [1] + " [" + tokens [2] + "]" + ";";
                    codeFragments.Add (frag);
                }

                else if (tokens.Length == 4)
                {
                    string frag = "    " + tokens [0] + " " + tokens [1] + " " + tokens [2] + " " + tokens [3] + ";";
                    codeFragments.Add (frag);
                }

                else
                    throw new Exception ("Error generating include file variables");
            }

            return codeFragments;
        }

        //************************************************************************************************

        static List<string> EndFile (string msgName)
        {
            // store generated code here
            List<string> codeFragments = new List<string> ();

            codeFragments.Add ("");
            codeFragments.Add (msgName + " ();");
            codeFragments.Add (msgName + " (byte *msgBytes);");
            codeFragments.Add ("void ToBytes    (byte *byteArray);");
            codeFragments.Add ("void ToConsole ();");
            codeFragments.Add ("");
            codeFragments.Add ("MessageHeader header;");
            codeFragments.Add ("MessageData   data;");

            return codeFragments;
        }
    }
}
