﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystemV4 {
    class Parser {

        static string specials = @"!@#$%^&*-+=<>/\:~|?";

        // ----- Helper Functions -----------------------------
        // comma-separated string "a,b,c" to List<string>
        public static List<string> CssToList(string s) {
            if (s == "") return new List<string>();
            return s.Split(new char[] { ',' }).ToList();
        }

        // take a list of primitive terms and return their variable names
        public static List<string> TermsToNames(List<Term> t) {
            List<string> s = new List<string>();
            foreach(var term in t) {
                if (!term.IsLeaf()) return null;
                s.Add(term.value);
            }
            return s;
        }
        public static List<string> TermsToNames(List<TermApply> t) {
            List<string> s = new List<string>();
            foreach (var term in t) {
                if (!term.IsLeaf()) return null;
                s.Add(term.value);
            }
            return s;
        }
        public static List<Term> NamesToTerms(List<string> s, Namespace ns) {
            List<Term> t = new List<Term>();
            foreach (var name in s) {
                t.Add(Term.MakePrimitiveTree(name, ns));
            }
            return t;
        }

        public static Dictionary<Tkey,Tvalue> MergeDictionaries<Tkey,Tvalue>(Dictionary<Tkey,Tvalue> d1, Dictionary<Tkey,Tvalue> d2) {
            Dictionary<Tkey,Tvalue> d = new Dictionary<Tkey,Tvalue>();
            foreach (var key1 in d1.Keys) {
                d1.Add(key1, d1[key1]);
            }
            foreach (var key2 in d2.Keys) {
                if (d1.ContainsKey(key2)) return null;
                d1.Add(key2, d2[key2]); 
            }
            return null;
        }

        public static int Any(string input, int startIdx=0) {
            int length;
            length = Identifier(input, startIdx);
            if (length > 0) return length;
            length = Number(input, startIdx);
            if (length > 0) return length;
            length = Special(input, startIdx);
            if (length > 0) return length;
            return 0;
        }

        // gets the number of characters in an alphanumeric identifier, starting from a given string index 
        public static int Identifier(string input, int startIdx=0) {
            int length = 0;
            if (char.IsLetter(input[startIdx+length])) {
                length = 1;
                while (length < (input.Length-startIdx) && char.IsLetterOrDigit(input[startIdx+length])) {
                    length++;
                }
            }

            return length;
        }

        // gets the number of characters in an alphanumeric identifier, starting from a given string index 
        public static int Number(string input, int startIdx = 0) {
            int length = 0;
            if (input[startIdx] == '-') length += 1;

            while (length < (input.Length - startIdx) && char.IsDigit(input[startIdx + length])) {
                length++;
            }

            return length;
        }

        // gets the number of characters in an alphanumeric identifier, starting from a given string index 
        public static int Special(string input, int startIdx = 0) {
            int length = 0;            
            while (length < (input.Length - startIdx) && specials.Contains(input[startIdx + length])) {
                length++;
            }

            return length;
        }

        // ----- Parsing TypeTrees ----------------------------
        // parse (Bool -> (Bool -> Bool)) and create a TypeTree
        // this method removes spaces and then consructs the tree
        public static TypeTree ParseTypeTree(string s, int idx = 0) {
            return ParseTreeNoSpaces(s.Replace(" ", ""), ref idx);
        }

        public static TypeTree ParseTreeNoSpaces(string s, ref int idx) {
            TypeTree returnTree = new TypeTree();

            int identifierLength = Parser.Any(s,idx);
            if (identifierLength > 0) {
                string identifier = s.Substring(idx, identifierLength); //(start,length)
                returnTree.value = identifier;
                idx += identifierLength;
            } else if (idx<s.Length || s[idx]=='(') {
                // here we must parse (A->B), where A and B are also TypeTrees

                // Parse the (
                idx += 1;

                // Parse A
                TypeTree leftTree = ParseTreeNoSpaces(s, ref idx);
                if (leftTree == null) return null;

                // Parse ->
                //confirm this is true
                if (idx>=s.Length || !(s[idx]=='-' && s[idx+1]=='>')) {
                    // error!!
                    TypeTreeParseError(s, idx);
                    return null;
                }
                idx += 2;

                // Parse B
                TypeTree rightTree = ParseTreeNoSpaces(s, ref idx);
                if (rightTree == null) return null;

                // Parse )
                if (idx>=s.Length || !(s[idx]==')')) {
                    // error!!
                    TypeTreeParseError(s, idx);
                    return null;
                }
                idx += 1;

                returnTree.SetChildren(leftTree, rightTree);
                
            } else {
                TypeTreeParseError(s, idx);
                return null;
            }

            return returnTree;
        }



        // methods in here return the number of parsed characters


        public static int Spaces(string input, int idx = 0) {
            int length = 0;
            while (length < input.Length && char.IsWhiteSpace(input[length + idx])) {
                length++;
            }
            return length;
        }

        public static int Arrow(string input) {
            return (input.Substring(0, 2) == "->") ? 2 : 0;
        }
        public static int Open(string input) {
            return (input[0] == '(') ? 1 : 0;
        }
        public static int Close(string input) {
            return (input[0] == ')') ? 1 : 0;
        }

        // ----- Parsing S-Expressions ------------------------
        // methods in here return the number of parsed characters
        // yes, this is a pretty terrible/inefficient parser with a lot of string copying
        // I don't care, I just want it to work
        public static STree ParseSExpression(string input, int idx = 0) {

            STree leftTree = null;

            int depth = 0;
            int subExpStart = 0;
            int subExpLength = 0;
            char c;

            // ----- left tree / identifier
            while (idx < input.Length) {
                c = input[idx];
                if (char.IsLetterOrDigit(c) || specials.Contains(c)) {
                    if (depth == 0) {
                        int length = Parser.Any(input, idx);
                        if (length == 0) {
                            Parser.SExprParseError(input, idx);
                            return null;
                        }
                        string word = input.Substring(idx, length);
                        leftTree = new STree(leftTree, STree.MakePrimitiveTree(word));
                        if (leftTree == null) return null;
                        idx += length;
                    } else {
                        idx += 1;
                    }                   
                } else if(char.IsWhiteSpace(c)) {
                    idx += 1;
                } else if(c == '(') {
                    idx += 1;
                    if (depth == 0) subExpStart = idx;
                    depth += 1;
                } else if(c == ')') {
                    depth -= 1;
                    if (depth < 0) {
                        SExprParseError(input, idx);
                        return null;
                    } else if (depth == 0) {
                        subExpLength = idx - subExpStart;
                        if (subExpLength == 0) {
                            Parser.SExprParseError(input, idx);
                            return null;
                        }
                        STree rightTree = ParseSExpression(input.Substring(subExpStart, subExpLength));
                        leftTree = new STree(leftTree, rightTree);
                    }
                    idx += 1;
                } else {
                    Parser.SExprParseError(input, idx);
                    return null;
                }


            }

            return leftTree;
        }





        private static void SExprParseError(string input, int idx) {
            Console.WriteLine("ERROR parsing S-Expression -- could not parse symbol: ");
            GeneralParseError(input, idx);
        }

        private static void TypeTreeParseError(string input, int idx) {
            Console.WriteLine("ERROR parsing TypeTree -- could not parse symbol: ");
            GeneralParseError(input, idx);
        }

        private static void GeneralParseError(string input, int idx) {
            Console.WriteLine(input);
            string spaces = "";
            for (int i = 0; i < idx; i++) spaces += " ";
            Console.WriteLine(spaces + "^");
        }






    }
}

/*
public static STree ParseSExpression(string input, int idx = 0) {

    // ----- identifier
    int start = idx;
    int length = Parser.Identifier(input,idx);
    if (length == 0) {
        Parser.SExprParseError(input, idx);
        return null;
    }
    string variable = input.Substring(idx, length);
    STree tree = STree.MakePrimitiveTree(variable);
    idx += length;

    // ----- parse args
    int depth = 0;
    int subExpStart = 0;
    int subExpLength = 0;
    char c;
    // break only if string runs out, or if depth is 0 and we see a )
    while (idx < input.Length) { // || (depth!=0 && input[idx]!=')') ) { 
        c = input[idx];
        if (c == '(') {
            idx += 1;
            if (depth == 0) subExpStart = idx;
            depth += 1;
        } else if (c == ')') {
            depth -= 1;
            if (depth == 0) {
                subExpLength = idx - subExpStart;
                if (subExpLength == 0) {
                    Parser.SExprParseError(input, idx);
                    return null;
                }
                STree childTree = Parser.ParseSExpression(input.Substring(subExpStart, subExpLength));
                if (childTree == null) return null;
                tree.children.Add(childTree);
            }
            if (depth < 0) return null;
            idx += 1;
        } else if (c == ' ') {
            idx += 1;
        } else if (Char.IsLetterOrDigit(c)) {
            if (depth == 0) {
                length = Parser.Identifier(input,idx);
                if (length == 0) {
                    Parser.SExprParseError(input, idx);
                    return null;
                }
                variable = input.Substring(idx, length);
                tree.children.Add(STree.MakePrimitiveTree(variable));
                idx += length;
            } else {
                idx += 1;
            }
        } else {
            Parser.SExprParseError(input, idx);
            return null;
        }
    }

    return tree;
}
*/
