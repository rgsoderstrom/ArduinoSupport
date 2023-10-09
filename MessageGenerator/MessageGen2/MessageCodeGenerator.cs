﻿
//
// MessageCodeGenerator.cs - 
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace MessageGenerator
{
    internal struct MessageDescription
    {
        public string Name;
        public List<string> Constants;
        public List<string> Variables;

        public override string ToString ()
        {
            string str = "";
            str += "Name: " + Name + "\n";
            str += "Constants:" + "\n";
            foreach (string s in Constants) str += "<" + s + ">" + "\n"; // the brackets are there to show leading & trailing spaces
            str += "Variables:" + "\n";
            foreach (string s in Variables) str += "<" + s + ">" + "\n";
            return str;
        }
    }


    public partial class MessageCodeGenerator
    {
        static void Main (string [] args)
        {
            // debug flag to write details to console
            int verbose = 0; // 0 => quiet, 1 or 2 is more verbose

            // input message description file
            string inputFileDir = @"C:\Users\rgsod\Documents\Visual Studio 2022\Projects\ArduinoSupport\MessageGenerator\MessageGen2\";
            string inputFileName = @"ExampleMessages.xml";

            // only used for C# files
            string messageNameSpace = "ArduinoInterface"; // default, may be changed

            List<MessageDescription> AllMessageDescriptions = new List<MessageDescription> ();

            // put generated files here
            string cppFileDir = @"C:\Users\rgsod\Documents\Visual Studio 2022\Projects\ArduinoSupport\MessageGenerator\AutoGeneratedFiles\";
            string csFileDir  = @"C:\Users\rgsod\Documents\Visual Studio 2022\Projects\ArduinoSupport\MessageGenerator\AutoGeneratedFiles\";

            //**********************************************************************************************
            //**********************************************************************************************
            //**********************************************************************************************

            //
            // load .xml help file and look for some errors
            //

            try
            {
                XmlDocument xd = new XmlDocument ();

                xd.Load (inputFileDir + inputFileName);

                string topNodeName = "Messages";

                XmlNodeList xmlNodelist = xd.SelectNodes (topNodeName);

                XmlNode top = xmlNodelist [0];

                // no supported attribute
                if (top.Attributes.Count != 0)
                    throw new Exception ("No top-level attributes allowed");
                   
                foreach (XmlNode msgNode in top.ChildNodes)
                {
                    if (msgNode.NodeType == XmlNodeType.Comment)
                        continue;

                    if (msgNode.Name == "Option")
                    {
                        switch (msgNode.Attributes [0].Name)
                        {
                            case "Namespace": messageNameSpace = msgNode.Attributes [0].Value; break;
                            case "CppDir":    cppFileDir       = msgNode.Attributes [0].Value; break;
                            case "CsDir":     csFileDir        = msgNode.Attributes [0].Value; break;
                            default: throw new Exception ("Unrecognized Option attribue: " + msgNode.Attributes [0].Name);
                        }

                        // make sure directories end in back slash
                        if (cppFileDir.EndsWith ("\\") == false) cppFileDir += "\\";
                        if (csFileDir.EndsWith ("\\")  == false) csFileDir += "\\";
                    }

                    else if (msgNode.Name == "Message")
                    {
                        MessageDescription descr = new MessageDescription ();

                        // extract message name
                        descr.Name = msgNode.Attributes [0].InnerText.Trim ();

                        foreach (XmlNode child in msgNode.ChildNodes)
                        {
                            switch (child.Name)
                            {
                                case "Constants":
                                    descr.Constants = child.InnerText.Split (new char [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList ();

                                    for (int i = 0; i<descr.Constants.Count; i++)
                                        descr.Constants [i] = descr.Constants [i].Trim ();
                                    break;

                                case "Variables":
                                    descr.Variables = child.InnerText.Split (new char [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList ();

                                    for (int i = 0; i<descr.Variables.Count; i++)
                                        descr.Variables [i] = descr.Variables [i].Trim ();
                                    break;

                                default:
                                    throw new Exception ("Unsupported message section: " + child.Name);
                            }
                        }

                        AllMessageDescriptions.Add (descr);
                    }

                    else
                        throw new Exception ("Unsupported node type: " + msgNode.Name);
                }

                if (verbose > 1)
                    foreach (MessageDescription md in AllMessageDescriptions)
                        Console.WriteLine (md.ToString ());
            }

            catch (Exception ex)
            {
                Console.WriteLine ("Exception loading input xml file: " + ex.Message);
                Console.WriteLine ("Stack trace: " + ex.StackTrace);
            }

            //**********************************************************************************************
            //**********************************************************************************************
            //**********************************************************************************************

            foreach (MessageDescription descr in AllMessageDescriptions)
            {
                // these are just shorter aliases
                string messageName = descr.Name;
                List<string> Constants = descr.Constants;
                List<string> Variables = descr.Variables;

                // parse 
                List<string []> variablesAsTokens = new List<string []> ();
                List<string []> constantsAsTokens = new List<string []> ();

                try
                {
                    if (Variables != null) variablesAsTokens = ParseVariables (Variables);
                    if (Constants != null) constantsAsTokens = ParseConstants (Constants);
                }

                catch (Exception ex)
                {
                    Console.WriteLine ("Exception parsing " + messageName + ": " + ex.Message);
                    //Console.WriteLine ("Stack trace: " + ex.StackTrace);
                }

                if (verbose > 0)
                {
                    Console.WriteLine (messageName);

                    // constants
                    Console.WriteLine ("Constants:");
                    foreach (string [] strArray in constantsAsTokens)
                    {
                        Console.Write ("\t");
                        foreach (string str in strArray)
                            Console.Write (str + ", ");
                        Console.WriteLine ();
                    }

                    // variables
                    Console.WriteLine ("\nVariables:");
                    foreach (string [] strArray2 in variablesAsTokens)
                    {
                        Console.Write ("\t");
                        foreach (string str in strArray2)
                            Console.Write (str + ", ");
                        Console.WriteLine ();
                    }

                    Console.WriteLine ();
                }

                //******************************************************************

                //
                // Cpp files
                //
                string cppFile = cppFileDir + messageName + ".cpp";
                string hFile = cppFileDir + messageName + ".h";

                try
                {
                    using (StreamWriter sw = new StreamWriter (hFile))
                    {
                        Cpp_Include include = new Cpp_Include (sw, messageName, constantsAsTokens, variablesAsTokens);
                    }

                    using (StreamWriter sw = new StreamWriter (cppFile))
                    {
                        Cpp_Code code = new Cpp_Code (sw, messageName, variablesAsTokens);
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine ("Exception generating C++ file: " + ex.Message);
                }

                //
                // C# files
                //
                string formatFileName = csFileDir + messageName + "_Format.cs";
                string methodsFileName = csFileDir + messageName + "_Methods.cs";

                try
                {
                    using (StreamWriter sw = new StreamWriter (formatFileName))
                    {
                        Cs_FormatFile f1 = new Cs_FormatFile (sw, messageNameSpace, messageName, constantsAsTokens, variablesAsTokens);
                    }

                    using (StreamWriter sw = new StreamWriter (methodsFileName))
                    {
                        Cs_MethodsFile f1 = new Cs_MethodsFile (sw, messageNameSpace, messageName, variablesAsTokens);
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine ("Exception generating C# code: " + ex.Message);
                }
            }
        }
    }
}



