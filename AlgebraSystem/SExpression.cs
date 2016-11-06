using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public class SExpression {
        public string value;
        public SExpression left;
        public SExpression right;

        public SExpression(string s) {
            this.value = s;
            this.left = null;
            this.right = null;
        }

        public SExpression(SExpression left, SExpression right) {
            this.value = null;
            this.left = left;
            this.right = right;
        }

        public override string ToString() {
            if (this.value != null) return this.value;
            if (this.left.value==null) {
                return "(" + this.left + ") " + this.right;
            }
            return this.left + " " + this.right;
        }

        // methods in here return the number of parsed characters
        public static int Identifier(string input, int start = 0) {
            int length = 0;
            while (start+length < input.Length && !"() \n\t".Contains(input[start+length])) {
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
            int length = 0;
            while (length < input.Length && char.IsWhiteSpace(input[start+length])) {
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
            return ParseSExpressionRecur(s, 0, s.Length-1);
        }

        public static SExpression ParseSExpressionRecur(string s, int left, int right) {
            // trim spaces on either end of string and check for crossover between left and right bounds
            if (left > right) throw new Exception("Parentheses not balanced!");
            while (s[left] == ' ') left++;
            while (s[right] == ' ') right--;
            if (left > right) throw new Exception("Parentheses not balanced!");

            SExpression sexp;
            int afterEnd; // index after the leftmost sub-expression
            if (s[left] == '(') { // case of a subexpression (...)
                int end = GetIndexOfEndParen(s, left + 1);
                sexp = ParseSExpressionRecur(s, left + 1, end - 1);
                afterEnd = end + 1;
            } else { // case of parsing a single identifier
                int length = Identifier(s, left);
                string identifier = s.Substring(left, length);
                sexp = new SExpression(identifier);
                afterEnd = left + length;
            }

            // if we've reached the end of the subexpression, just return what we have
            if (afterEnd > right) {
                return sexp;
            }

            // otherwise, take what we have and make it the left child of a node
            // and out the stuff we'll see in the future in the right child of that node
            afterEnd += Spaces(s, afterEnd);
            SExpression leftSubExp = sexp;
            SExpression rightSubExp = ParseSExpressionRecur(s, afterEnd, right);
            return new SExpression(leftSubExp, rightSubExp);
        }


        public static TypeTree ParseTypeTree(string s) {
            bool balanced = CheckBalancedParens(s);
            if (!balanced) throw new Exception("Parentheses not balanced!");
            return ParseTypeTreeRecur(s, 0, s.Length - 1);
        }

        public static string ParseSymbol(string s, int start, string symbol) {
            if (start + symbol.Length > s.Length - 1) return null;
            for(int i=0; i<symbol.Length; i++) {
                if (s[start + i] != symbol[i]) return null;
            }
            return symbol;
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
                if(length==0) throw new Exception("Not a valid Identifier!");
                string identifier = s.Substring(left, length);
                tTree = TypeTree.MakePrimitiveTree(identifier);
                afterEnd = left + length;
            }

            // if we've reached the end of the subexpression, just return what we have
            if (afterEnd > right) {
                if(leftGraftTree==null) {
                    return tTree;
                }
                return new TypeTree(leftGraftTree,tTree);
            }

            // clean up spaces
            afterEnd += Spaces(s, afterEnd);

            // check for the special cases of the type operators -> , |
            string symbol = ParseSymbol(s, afterEnd, "->") ??
                            ParseSymbol(s, afterEnd, ",") ??
                            ParseSymbol(s, afterEnd, "|");
            if(symbol!=null) {
                var symbolTree = TypeTree.MakePrimitiveTree(symbol);
                var firstArgTree = new TypeTree(symbolTree, tTree);
                afterEnd += symbol.Length;
                afterEnd += Spaces(s, afterEnd);
                var secondArgTree = ParseTypeTreeRecur(s, afterEnd, right);
                var rightTree = new TypeTree(firstArgTree, secondArgTree);

                if(leftGraftTree==null) return rightTree;
                return new TypeTree(leftGraftTree, rightTree);
            }

            // otherwise, take what we have and make it the left child of a node
            // and out the stuff we'll see in the future in the right child of that node
            TypeTree rightSubTree = tTree;
            if (leftGraftTree==null) {
                leftGraftTree = rightSubTree;
            } else {
                leftGraftTree = new TypeTree(leftGraftTree, rightSubTree);
            }            
            return ParseTypeTreeRecur(s, afterEnd, right, leftGraftTree);
        }

    }
}
