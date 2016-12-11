using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public class Parser {

        static string specials = @"!@#$%^&*-+=<>/\:~|?,";

        // ----- Helper Functions -----------------------------
        // semi-colon-separated string "a;b;c" to List<string>
        // TODO: this is a pretty terrible idea to have to rely on this... should try to eliminate the need for it
        // It makes dependences really hard to predict...
        public static List<string> ScsvToList(string s) {
            if (string.IsNullOrEmpty(s)) return new List<string>();
            return s.Split(new char[] { ';' }).ToList();
        }
        public static List<string> CsvToList(string s) {
            if (string.IsNullOrEmpty(s)) return new List<string>();
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
        public static List<string> TermsToNames(List<TermNew> t) {
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
        public static STree ParseSTree(string input, int idx = 0) {

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
                        STree rightTree = ParseSTree(input.Substring(subExpStart, subExpLength));
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



        public static TypeExpr ParseTypeExpr(string input) {
            if (string.IsNullOrEmpty(input)) throw new Exception("Cannot parse: null or empty string.");
            string[] segments = input.Split('.');
            if (segments.Length > 2) throw new Exception("Type expression may only have one '.' character.");

            string treeString;
            List<string> boundTypeVars;
            if (segments.Length == 2) {
                boundTypeVars = CsvToList(segments[0]);
                foreach(var v in boundTypeVars) {
                    if (!TypeTree.IsTypeVariable(v)) throw new Exception("The following bound type varaible must start with lower case: " + v);
                }
                treeString = segments[1];
            } else {
                boundTypeVars = new List<string>();
                treeString = segments[0];
            }
            TypeTree tree = ParseTypeTree(treeString);

            return new TypeExpr(tree, boundTypeVars);
        }



        // methods in here return the number of parsed characters
        public static int IdentifierOrOp(string input, int start = 0) {
            int length = 0;
            while (start + length < input.Length && !"() \n\t".Contains(input[start + length])) {
                length++;
            }
            return length;
        }

        // gets the number of characters in an alphanumeric identifier, starting from a given string index 
        public static int TypeIdentifier(string input, int startIdx = 0) {
            string symbol = ParseSymbol(input, startIdx, "->") ??
                ParseSymbol(input, startIdx, ",") ??
                ParseSymbol(input, startIdx, "|");
            if (symbol != null) return symbol.Length;

            int length = 0;
            if (char.IsLetter(input[startIdx + length])) {
                length = 1;
                while (length < (input.Length - startIdx) && char.IsLetterOrDigit(input[startIdx + length])) {
                    length++;
                }
            }
            return length;
        }


        public static int Spaces(string input, int start = 0) {
            if (start >= input.Length) return 0;
            int length = 0;
            while (start + length < input.Length && char.IsWhiteSpace(input[start + length])) {
                length++;
            }
            return length;
        }

        public static bool CheckBalancedParens(string s) {
            int depth = 0;
            foreach (var c in s) {
                if (c == '(') depth++;
                if (c == ')') depth--;
                if (depth < 0) return false;
            }
            return depth == 0;
        }

        public static int GetIndexOfEndParen(string s, int start = 0) {
            int depth = 1;
            int idx = start;
            while (true) {
                if (idx >= s.Length) return -1;
                char c = s[idx];
                if (c == '(') depth++;
                if (c == ')') depth--;
                if (depth == 0) break;
                idx++;
            }
            return idx;
        }

        public static SExpression ParseSExpression(string s) {
            bool balanced = CheckBalancedParens(s);
            if (!balanced) throw new Exception("Parentheses not balanced!");
            return ParseSExpressionRecur(s, 0, s.Length - 1);
        }

        public static SExpression ParseSExpressionRecur(string s, int left, int right) {
            // trim spaces on either end of string and check for crossover between left and right bounds
            if (left > right) throw new Exception("Parentheses not balanced!");
            while (s[left] == ' ') left++;
            while (s[right] == ' ') right--;
            if (left > right) throw new Exception("Parentheses not balanced!");

            SExpression sexpLeft = null;
            SExpression sexpRight;

            // if we've reached the end of the subexpression, just return what we have
            while (left <= right) {

                int afterEnd; // index after the leftmost sub-expression
                if (s[left] == '(') { // case of a subexpression (...)
                    int end = GetIndexOfEndParen(s, left + 1);
                    sexpRight = ParseSExpressionRecur(s, left + 1, end - 1);
                    afterEnd = end + 1;
                } else { // case of parsing a single identifier
                    int length = IdentifierOrOp(s, left);
                    string identifier = s.Substring(left, length);
                    sexpRight = new SExpression(identifier);
                    afterEnd = left + length;
                }

                if (sexpLeft == null) {
                    sexpLeft = sexpRight;
                } else {
                    sexpLeft = new SExpression(sexpLeft, sexpRight);
                }

                left = afterEnd + Spaces(s, afterEnd);
            }

            return sexpLeft;
        }


        public static string ParseSymbol(string s, int start, string symbol) {
            if (start + symbol.Length > s.Length) return null;
            for (int i = 0; i < symbol.Length; i++) {
                if (s[start + i] != symbol[i]) return null;
            }
            return symbol;
        }

        public static TypeTree ParseTypeTree(string s) {
            bool balanced = CheckBalancedParens(s);
            if (!balanced) throw new Exception("Parentheses not balanced!");
            return ParseTypeTreeRecur(s, 0, s.Length - 1);
        }

        public static TypeTree ParseTypeTreeRecur(string s, int left, int right, TypeTree leftGraftTree = null) {
            // trim spaces on either end of string and check for crossover between left and right bounds
            if (left > right) throw new Exception("Parentheses not balanced!");
            while (s[left] == ' ') left++;
            while (s[right] == ' ') right--;
            if (left > right) throw new Exception("Parentheses not balanced!");

            TypeTree tTree;
            int afterEnd; // index after the leftmost sub-expression
            if (s[left] == '(') { // case of a subexpression (...)
                int end = GetIndexOfEndParen(s, left + 1);
                tTree = ParseTypeTreeRecur(s, left + 1, end - 1);
                afterEnd = end + 1;
            } else { // case of parsing a single identifier
                int length = TypeIdentifier(s, left);
                if (length == 0) throw new Exception("Not a valid Identifier!");
                string identifier = s.Substring(left, length);
                tTree = TypeTree.MakePrimitiveTree(identifier);
                afterEnd = left + length;
            }

            // if we've reached the end of the subexpression, just return what we have
            if (afterEnd > right) {
                if (leftGraftTree == null) {
                    return tTree;
                }
                return new TypeTree(leftGraftTree, tTree);
            }

            // clean up spaces
            afterEnd += Spaces(s, afterEnd);

            // check for the special cases of the type operators -> , |
            string symbol = ParseSymbol(s, afterEnd, "->") ??
                            ParseSymbol(s, afterEnd, ",") ??
                            ParseSymbol(s, afterEnd, "|");
            if (symbol != null) {
                var symbolTree = TypeTree.MakePrimitiveTree(symbol);
                var firstArgTree = new TypeTree(symbolTree, tTree);
                afterEnd += symbol.Length;
                afterEnd += Spaces(s, afterEnd);
                var secondArgTree = ParseTypeTreeRecur(s, afterEnd, right);
                var rightTree = new TypeTree(firstArgTree, secondArgTree);

                if (leftGraftTree == null) return rightTree;
                return new TypeTree(leftGraftTree, rightTree);
            }

            // otherwise, take what we have and make it the left child of a node
            // and out the stuff we'll see in the future in the right child of that node
            TypeTree rightSubTree = tTree;
            if (leftGraftTree == null) {
                leftGraftTree = rightSubTree;
            } else {
                leftGraftTree = new TypeTree(leftGraftTree, rightSubTree);
            }
            return ParseTypeTreeRecur(s, afterEnd, right, leftGraftTree);
        }

        public static KindTree ParseKindTree(string s) {
            bool balanced = CheckBalancedParens(s);
            if (!balanced) throw new Exception("Parentheses not balanced!");
            return ParseKindTreeRecur(s, 0, s.Length - 1);
        }

        public static KindTree ParseKindTreeRecur(string s, int left, int right, KindTree leftGraftTree = null) {
            // trim spaces on either end of string and check for crossover between left and right bounds
            if (left > right) throw new Exception("Parentheses not balanced!");
            while (s[left] == ' ') left++;
            while (s[right] == ' ') right--;
            if (left > right) throw new Exception("Parentheses not balanced!");

            KindTree kTree;
            int afterEnd; // index after the leftmost sub-expression
            if (s[left] == '(') { // case of a subexpression (...)
                int end = GetIndexOfEndParen(s, left + 1);
                kTree = ParseKindTreeRecur(s, left + 1, end - 1);
                afterEnd = end + 1;
            } else { // case of parsing a single identifier
                if (ParseSymbol(s, left, "*")==null) throw new Exception("Not a valid Identifier!");
                kTree = KindTree.MakePrimitiveTree("*");
                afterEnd = left + 1;
            }

            // if we've reached the end of the subexpression, just return what we have
            if (afterEnd > right) {
                if (leftGraftTree == null) {
                    return kTree;
                }
                return new KindTree(leftGraftTree, kTree);
            }

            // clean up spaces
            afterEnd += Spaces(s, afterEnd);

            // check for the special cases of the operator ->
            string symbol = ParseSymbol(s, afterEnd, "=>");
            if (symbol != null) {
                var symbolTree = KindTree.MakePrimitiveTree(symbol);
                var firstArgTree = new KindTree(symbolTree, kTree);
                afterEnd += symbol.Length;
                afterEnd += Spaces(s, afterEnd);
                var secondArgTree = ParseKindTreeRecur(s, afterEnd, right);
                var rightTree = new KindTree(firstArgTree, secondArgTree);

                if (leftGraftTree == null) return rightTree;
                return new KindTree(leftGraftTree, rightTree);
            }

            // otherwise, take what we have and make it the left child of a node
            // and out the stuff we'll see in the future in the right child of that node
            KindTree rightSubTree = kTree;
            if (leftGraftTree == null) {
                leftGraftTree = rightSubTree;
            } else {
                leftGraftTree = new KindTree(leftGraftTree, rightSubTree);
            }
            return ParseKindTreeRecur(s, afterEnd, right, leftGraftTree);
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
