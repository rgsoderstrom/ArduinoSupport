
//
// Cpp_Include - C++ code in and C++ .h file out
//

using System;
using System.Collections.Generic;
using System.IO;

namespace MessageGenerator
{
    public class Cpp_Include
    {
        // build-up file contents here
        public List<string> FileText { get; protected set; } = new List<string> ();

        //**********************************************************************

        // ctor

        protected Cpp_Include (string msgName, List<string []> consAsTokens, List<string []> varsAsTokens)
        {
            bool headerOnly = (consAsTokens.Count == 0) && (varsAsTokens.Count == 0);
            BeginFile (msgName, headerOnly, FileText);

            if (headerOnly == false)
            {
                CodeGenerator_IncludeConstants (consAsTokens, FileText);
                //FileText.Add ("");
                CodeGenerator_IncludeVariables (varsAsTokens, FileText);
            }

            EndFile (msgName, headerOnly, FileText);
        }

        public Cpp_Include (StreamWriter sw, string msgName, List<string []> consAsTokens, List<string []> varsAsTokens) : this (msgName, consAsTokens, varsAsTokens)
        {
            foreach (string str in FileText)
                sw.WriteLine (str);
        }

        //************************************************************************************************

        void BeginFile (string msgName, bool headerOnly, List<string> fileText)
        {
            fileText.Add ("//");
            fileText.Add ("// " + msgName + ".h");
            fileText.Add ("//   - auto generated code");
            fileText.Add ("");

            fileText.Add ("#ifndef " + msgName.ToUpper () + "_H");
            fileText.Add ("#define " + msgName.ToUpper () + "_H");
            fileText.Add ("");

            fileText.Add ("#include <Arduino.h>");
            fileText.Add ("#include \"MessageIDs.h\"");
            fileText.Add ("#include \"MessageHeader.h\"");
            fileText.Add ("");

            fileText.Add ("class " + msgName);
            fileText.Add ("{");
            fileText.Add ("    public:");

            if (headerOnly == false)
            {
                fileText.Add ("        struct Data");
                fileText.Add ("        {");
            }
        }

        //************************************************************************************************

        void CodeGenerator_IncludeConstants (List<string []> memberVariableTokens, List<string> fileText) 
        {
            foreach (string [] tokens in memberVariableTokens)
            {
                if (tokens.Length == 3)
                {
                    string frag = "            static const " + tokens [0] + " " + tokens [1] + " = " + tokens [2] + ";";
                    fileText.Add (frag);
                }

                else
                    throw new Exception ("Error generating include file constants");
            }
        }        

        //************************************************************************************************

        void CodeGenerator_IncludeVariables (List<string []> memberVariableTokens, List<string> fileText) 
        {
            foreach (string [] tokens in memberVariableTokens)
            {
                if (tokens.Length == 2)
                {
                    string frag = "            " + tokens [0] + " " + tokens [1] + ";";
                    fileText.Add (frag);
                }

                else if (tokens.Length == 3)
                {
                    string token_2 = tokens [2].Replace ("Data.", "Data::");

                    string frag = "            " + tokens [0] + " " + tokens [1] + " [" + token_2 + "]" + ";";
                    fileText.Add (frag);
                }

                else if (tokens.Length == 4)
                {
                    string frag = "            " + tokens [0] + " " + tokens [1] + " " + tokens [2] + " " + tokens [3] + ";";
                    fileText.Add (frag);
                }

                else
                    throw new Exception ("Error generating include file variables");
            }

            FileText.Add ("        };\n");
        }

        //************************************************************************************************

        void EndFile (string msgName, bool headerOnly, List<string> fileText)
        {
           // fileText.Add ("");
            fileText.Add ("\t\t" + msgName + " ();");
            fileText.Add ("\t\t" + msgName + " (byte *msgBytes);");
            fileText.Add ("\t\tvoid ToBytes   (byte *byteArray);");
            fileText.Add ("\t\tvoid ToConsole ();");
            fileText.Add ("");
            fileText.Add ("\t\tMessageHeader header;");

            if (headerOnly == false)
                fileText.Add ("\t\tData          data;");

            FileText.Add ("};");
            FileText.Add ("#endif");
        }
    }
}
