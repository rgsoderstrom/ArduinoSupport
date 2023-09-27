using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageGenerator
{
    public partial class MessageCodeGenerator
    {
        //
        // ParseDataMembers () - passed a list of "Data" members in C++ syntax
        //                     - returns a list of string arrays
        //                          - one array per input line, with input line split into tokens
        //                              - two tokens for scalar
        //                              - three for arrays
        //


        static List<string []> ParseVariables (List<string> memberVariables)
        {
            char [] separators = new char [] {' ', '\t', '[', ']', ';'};
            return ParseCommon (memberVariables, separators);
        }

        static List<string []> ParseConstants (List<string> memberConstants)
        {
            char [] separators = new char [] {' ', '\t', '[', ']', '=', ';'};
            return ParseCommon (memberConstants, separators);
        }

        //*********************************************************************************************

        static List<string []> ParseCommon (List<string> members, char [] separators)
        {
            List<string []> linesAsTokens = new List<string []> ();

            foreach (string str in members)
            {
                string [] tokens = str.Split (separators, StringSplitOptions.RemoveEmptyEntries);

                //
                // if first token is "unsigned", combine it with second
                //
                if (tokens [0] == "unsigned")
                {
                    string [] tokens2 = new string [tokens.Length - 1];
                    tokens2 [0] = tokens [0] + " " + tokens [1];

                    for (int i = 2; i<tokens.Length; i++)
                        tokens2 [i-1] = tokens [i];

                    tokens = tokens2;
                }

                linesAsTokens.Add (tokens);
            }

            return linesAsTokens;
        }



        //************************************************************************************************

        static public List<string> CodeGenerator_Variables (List<string []> memberVariableTokens, 
                                                            Dictionary<string, VariableTypeToCode> typeHandlers, 
                                                            Dictionary<string, VariableTypeArrayToCode> typeArrayHandlers)
        {
            // store generated code here
            List<string> codeFragments = new List<string> ();

            foreach (string [] tokens in memberVariableTokens)
            {
                if (tokens.Length == 2)
                {
                    if (typeHandlers.ContainsKey (tokens [0]))
                    {
                        VariableTypeToCode typeHandler = typeHandlers [tokens [0]];
                        typeHandler (tokens [1], codeFragments);
                    }

                    else
                        throw new Exception ("Type " + tokens [0] + " not found");
                }

                else if (tokens.Length == 3)
                {
                    if (typeArrayHandlers.ContainsKey (tokens [0]))
                    {
                        VariableTypeArrayToCode typeHandler = typeArrayHandlers [tokens [0]];
                        typeHandler (tokens [1], tokens [2], codeFragments);
                    }

                    else
                        throw new Exception ("Array type " + tokens [0] + " not found");
                }

                else
                {
                    string str = "";

                    foreach (string s in tokens)
                        str += s + " ";

                    throw new Exception ("Unsupported: " + str);
                }
            }

            return codeFragments;
        }        
    }
}
