using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public class STree {

        private STree left;
        private STree right;
        private string _value;
        // when we write to _value, kill the child nodes
        public string value
        {
            get { return _value; }
            set { _value = value; left = null; right = null; }
        }

        public STree(string s) {
            STree temp = Parser.ParseSExpression(s);
            if (temp == null) { // parseing failed
                this.value = string.Empty;
            } else if (temp.IsLeaf()) {
                this.value = temp.value;
            } else {
                this.SetChildren(temp.left, temp.right);
            }
        }
        public STree(STree l, STree r) {
            if (l == null) {
                this.SetChildren(null, r.DeepCopy());
            } else {
                this.SetChildren(l.DeepCopy(), r.DeepCopy());
            }            
        }
        public STree() {
            this.value = string.Empty;
        }
        public static STree MakePrimitiveTree(string s) {
            STree temp = new STree();
            temp.value = s;
            return temp;
        }

        // Getting is normal, but setting should set the string to empty
        public void SetChildren(STree l, STree r) {
            if (l == null && r != null) {
                if (r.IsLeaf()) this.value = r.value;
                else this.SetChildren(r.left, r.right);
            } else {
                this.value = string.Empty;
                this.left = l;
                this.right = r;
            }
        }
        public STree GetLeft() { return this.left; }
        public STree GetRight() { return this.right; }


        // ----- Copying and Equals/Matching ------------------
        public STree DeepCopy() {
            if (this.IsLeaf()) {
                return STree.MakePrimitiveTree(this.value);
            } else {
                return new STree(this.left, this.right);
            }
        }


        public bool IsLeaf() {
            return this.left == null;
        }

        public bool Contains(string s) {
            if (this.value == s) return true;
            return this.left.Contains(s) || this.right.Contains(s);
        }


        // ----- Parsing and conversion To/From other datatypes ---------
        public override string ToString() {
            if (this.IsLeaf()) return this.value;

            STree currentTree = this;
            string childrenString = currentTree.right.ToString();
            while(!currentTree.left.IsLeaf()) {
                currentTree = currentTree.left;
                childrenString = currentTree.right + " " + childrenString; // pre-pend;                
            }
            childrenString = currentTree.left + " " + childrenString;
                                  
            return "(" + childrenString + ")";
        }
    }

}
