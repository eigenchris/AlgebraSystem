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
            while (start+length < input.Length && !"() ".Contains(input[start+length])) {
                length++;
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
            if (left > right) throw new Exception("Parentheses not balanced!");
            while (s[left] == ' ') left++;
            while (s[right] == ' ') right--;
            if (left > right) throw new Exception("Parentheses not balanced!");

            SExpression sexp;
            int afterEnd;
            if (s[left] == '(') {
                int end = GetIndexOfEndParen(s, left + 1);
                sexp = ParseSExpressionRecur(s, left + 1, end - 1);
                afterEnd = end + 1;
            } else {
                int length = Identifier(s, left);
                string identifier = s.Substring(left, length);
                sexp = new SExpression(identifier);
                afterEnd = left + length;
            }

            if (afterEnd > right) {
                return sexp;
            }

            int spaces = Spaces(s, afterEnd);
            SExpression leftSubExp = sexp;
            SExpression rightSubExp = ParseSExpressionRecur(s, afterEnd + spaces, right);
            return new SExpression(leftSubExp, rightSubExp);
        }

    }
}
