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
        public static SExpression MakePrimitiveTree(string s) {
            return new SExpression(s);
        }

        public SExpression GetLeft() { return this.left; }
        public SExpression GetRight() { return this.right; }


        public bool IsLeaf() {
            return (this.left == null);
        }

        // ----- Copying and Equals/Matching ------------------
        public SExpression DeepCopy() {
            if (this.IsLeaf()) {
                return SExpression.MakePrimitiveTree(this.value);
            } else {
                return new SExpression(this.left, this.right);
            }
        }

        // ----- Parsing and conversion To/From other datatypes ---------
        public override string ToString() {
            if (this.value != null) return this.value;
            if (this.right.value==null) {
                return  this.left + " (" + this.right + ")";
            }
            return this.left + " " + this.right;
        }

    }
}
