using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public class KindTree {

        private KindTree left;
        private KindTree right;
        private string _value;
        // when we write to _value, kill the child nodes
        public string value {
            get { return _value; }
            set { _value = value; left = null; right = null; }
        }
        
        public KindTree(string s) {
            KindTree temp = Parser.ParseKindTree(s);
            if (temp == null) { // parseing failed
                this.value = null;
            } else if (temp.IsLeaf()) {
                this.value = temp.value;
            } else {
                this.SetChildren(temp.left,temp.right);
            }
        }
        public KindTree(KindTree l, KindTree r) {
            this.SetChildren(l.DeepCopy(), r.DeepCopy());
        }
        public KindTree() {
            this.value = null;
        }
        public static KindTree MakePrimitiveTree(string s) {
            KindTree temp = new KindTree();
            temp.value = s;
            return temp;
        }

        // Getting is normal, but setting should set the string to empty
        public void SetChildren(KindTree l, KindTree r) {
            this.value = null;
            this.left = l;
            this.right = r;
        }
        public KindTree GetLeft() { return this.left; }
        public KindTree GetRight() { return this.right; }

        public KindTree(KindTree l, KindTree r, bool flagToUseArrow) {
            KindTree ctree = KindTree.MakePrimitiveTree("=>");
            KindTree temp = new KindTree(ctree, l);
            this.SetChildren(temp, r);
        }


        // ----- Copying and Equals/Matching ------------------
        public KindTree DeepCopy() {
            if(this.IsLeaf()) {
                return KindTree.MakePrimitiveTree(this.value);
            } else {
                return new KindTree(this.left, this.right);
            }
        }

        public bool DeepEquals(KindTree t) {
            if (this.value != t.value) {
                return false;   // data does not agree
            }
            if (this.left == null ^ t.left == null) {
                return false;   // exactly one of the left branches is null
            }
            if (this.right == null ^ t.right == null) {
                return false;   // exactly one of the right branches is null
            }
            // at this point, we have confirmed the tree shape is the same
            //  the left branches are either both null, or both exist
            // the right branches are either both null, or both exist
            if (this.left == null && this.right == null && t.left == null && t.right == null) {
                return true;    // left and right branches on both trees are null
            }
            // at this point, we have confirmed that the left and right branches on both trees
            //      exist, so we compare the left and right subtrees for equality
            return this.left.DeepEquals(t.left) && this.right.DeepEquals(t.right);
        }

        // ----- Basic Tree Operations ----------------------------------
        // does the tree have zero children? (no left child should mean no right child)
        public bool IsLeaf() {
            return (this.left == null);
        }

        public int GetNumberOfInputs() {
            int num = 0;
            KindTree tree = this;
            while (tree.right != null) {
                num++;
                tree = tree.right;
            }
            return num;
        }

        public KindTree PopInput() {
            if (this.IsLeaf()) {
                return null;
            } else {
                return this.right.DeepCopy();
            }
        }

        public KindTree PopOutput() {
            KindTree returnTree = this.DeepCopy();
            if (returnTree.IsLeaf()) {
                return null;
            } else if (returnTree.right.IsLeaf()) {
                return returnTree.left;
            } else {
                KindTree tempTree1 = returnTree;
                KindTree tempTree2 = returnTree.right;
                while (!tempTree2.right.IsLeaf()) {
                    tempTree1 = tempTree1.right;
                    tempTree2 = tempTree2.right;
                }
                tempTree1.right = tempTree2.left;
                return returnTree;
            }
        }

        // ----- Parsing and conversion To/From other datatypes ---------
        public override string ToString() {
            if (this.IsLeaf()) {
                return this.value;
            } else {
                string symbol = this.left?.left?.value;
                if (symbol == "=>") {
                    if(this.left.right.value==null) {
                        return $"({this.left.right}) {symbol} {this.right}";
                    }
                    return $"{this.left.right} {symbol} {this.right}";
                }
                else if (this.right.value == null) {
                    return $"{this.left} ({this.right})";
                }
                else {
                    return $"{this.left} {this.right}";
                }                
            }
        }

    }
}
