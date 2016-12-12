using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public class TypeTree {

        public TypeTree left { get; private set; }
        public TypeTree right { get; private set; }
        private string _value;
        // when we write to _value, kill the child nodes
        public string value {
            get { return _value; }
            set { _value = value; left = null; right = null; }
        }
        
        public TypeTree(string s) {
            TypeTree temp = Parser.ParseTypeTree(s);
            if (temp == null) { // parseing failed
                this.value = null;
            } else if (temp.IsLeaf()) {
                this.value = temp.value;
            } else {
                this.SetChildren(temp.left,temp.right);
            }
        }
        public TypeTree(TypeTree l, TypeTree r) {
            this.SetChildren(l.DeepCopy(), r.DeepCopy());
        }
        public TypeTree() {
            this.value = null;
        }
        public static TypeTree MakePrimitiveTree(string s) {
            TypeTree temp = new TypeTree();
            temp.value = s;
            return temp;
        }

        // Getting is normal, but setting should set the string to empty
        public void SetChildren(TypeTree l, TypeTree r) {
            this.value = null;
            this.left = l;
            this.right = r;
        }
        public TypeTree GetLeft() { return this.left; }
        public TypeTree GetRight() { return this.right; }

        public TypeTree(TypeTree l, TypeTree r, TypeConstructorEnum c) {
            string cstring;
            if (c == TypeConstructorEnum.Function) cstring = "->";
            else if (c == TypeConstructorEnum.Sum) cstring = "|";
            else cstring = ",";
            TypeTree ctree = TypeTree.MakePrimitiveTree(cstring);

            TypeTree temp = new TypeTree(ctree, l);
            this.SetChildren(temp, r);
        }


        // ----- Copying and Equals/Matching ------------------
        public TypeTree DeepCopy() {
            if(this.IsLeaf()) {
                return TypeTree.MakePrimitiveTree(this.value);
            } else {
                return new TypeTree(this.left, this.right);
            }
        }

        public bool DeepEquals(TypeTree t) {
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

        public bool AlphaEquivalent(TypeTree t, Dictionary<string,string> typeVarEquivalencies = null) {
            typeVarEquivalencies = typeVarEquivalencies ?? new Dictionary<string, string>();

            if (this.left == null ^ t.left == null) return false;
            if (this.right == null ^ t.right == null) return false;
            if (this.left == null && this.right == null && t.left == null && t.right == null) {
                if(TypeTree.IsTypeVariable(this.value) &&  TypeTree.IsTypeVariable(t.value)) {
                    if (typeVarEquivalencies.ContainsKey(this.value)) {
                        return typeVarEquivalencies[this.value] == t.value;
                    } else if (typeVarEquivalencies.ContainsValue(t.value)) {
                        return false;
                    } else {
                        typeVarEquivalencies.Add(this.value, t.value);
                        return true;
                    }
                }
                return this.value == t.value;
            }
            return this.left.AlphaEquivalent(t.left, typeVarEquivalencies) 
                && this.right.AlphaEquivalent(t.right, typeVarEquivalencies);
        }

        // ----- Instance of -------------------------------
        public bool InstanceOf(TypeTree generalTree, List<string> boundVars, Dictionary<string, TypeTree> subs = null) {
            subs = subs ?? new Dictionary<string, TypeTree>();

            if (generalTree.IsLeaf()) {
                var v = generalTree.value;
                if (!boundVars.Contains(v)) return v == this.value;
                if (!subs.ContainsKey(v)) {
                    subs.Add(v, this);
                    return true;
                }
                return this.DeepEquals(subs[v]);
            }
            if (this.IsLeaf()) return false;

            bool leftSuccess = this.left.InstanceOf(generalTree.GetLeft(), boundVars, subs);
            if (!leftSuccess) return false;
            bool rightSuccess = this.right.InstanceOf(generalTree.GetRight(), boundVars, subs);
            return rightSuccess;
        }


        // ----- Unification -------------------------------
        public static Dictionary<string, TypeTree> UnifyAndSolve(TypeTree t1, TypeTree t2, Dictionary<string, TypeTree> subs = null) {
            if (t1 == null || t2 == null) return null;
            subs = subs ?? new Dictionary<string, TypeTree>();

            subs = Unify(t1, t2, subs);
            if (subs == null) return null;

            SolveMappings(subs);
            return subs;
        }

        // returns a dictionary "subs" such that t1.Substitute(subs) == t2.Substitute(subs)
        public static Dictionary<string, TypeTree> Unify(TypeTree t1, TypeTree t2, Dictionary<string, TypeTree> subs = null) {
            subs = subs ?? new Dictionary<string, TypeTree>();

            if (!t1.IsLeaf() && !t2.IsLeaf()) {
                var success1 = Unify(t1.GetLeft(), t2.GetLeft(), subs);
                if (success1 == null) return null;
                var success2 = Unify(t1.GetRight(), t2.GetRight(), subs);
                if (success2 == null) return null;
            } else if (t1.IsLeaf() && TypeTree.IsTypeVariable(t1.value)) {
                if(!subs.ContainsKey(t1.value)) {
                    subs.Add(t1.value, t2.DeepCopy());
                } else { // "don't map a variable to itself" is old reasoning... we must alpha-convert trees before unifying them
                    var success = Unify(subs[t1.value], t2, subs);   // If a type var matches two different subtrees, unify them
                    if (success == null) return null;
                }
            } else if (t2.IsLeaf() && TypeTree.IsTypeVariable(t2.value)) {
                if (!subs.ContainsKey(t2.value)) {
                    subs.Add(t2.value, t1.DeepCopy());
                } else {
                    var success = Unify(subs[t2.value], t1, subs);   // If a type var matches two different subtrees, unify them
                    if (success == null) return null;
                }
            } else if (!t1.DeepEquals(t2)) {
                Console.WriteLine("Error in Unify:");
                Console.WriteLine("Value " + t1 + " does not match " + t2);
                return null;
            }

            return subs;
        }

        // if a: Bool, and b: a, we need to make sure that b: Bool as well
        // substitution should only be applied once to unify expressions
        public static void SolveMappings(Dictionary<string, TypeTree> subs) {
            TypeTree tempTree = new TypeTree();
            foreach (var key in subs.Keys.ToList()) {
                tempTree = subs[key];
                foreach (var key2 in subs.Keys.ToList()) {
                    if (key == key2) continue;
                    subs[key2] = subs[key2].Substitute(key, tempTree);
                }
            }
            // remove loops
            foreach(var key in subs.Keys.ToList()) {
                if(subs[key].IsLeaf() && key==subs[key].value) {
                    subs.Remove(key);
                }
            }
        }

        // ----- Basic Tree Operations ----------------------------------
        // does the tree have zero children? (no left child should mean no right child)
        public bool IsLeaf() {
            return (this.left == null);
        }

        public int GetNumberOfInputs() {
            int num = 0;
            TypeTree tree = this;
            while (tree.right != null) {
                num++;
                tree = tree.right;
            }
            return num;
        }

        public TypeTree PopInput() {
            if (this.IsLeaf()) {
                return null;
            } else {
                return this.right.DeepCopy();
            }
        }

        public TypeTree PopOutput() {
            TypeTree returnTree = this.DeepCopy();
            if (returnTree.IsLeaf()) {
                return null;
            } else if (returnTree.right.IsLeaf()) {
                return returnTree.left;
            } else {
                TypeTree tempTree1 = returnTree;
                TypeTree tempTree2 = returnTree.right;
                while (!tempTree2.right.IsLeaf()) {
                    tempTree1 = tempTree1.right;
                    tempTree2 = tempTree2.right;
                }
                tempTree1.right = tempTree2.left;
                return returnTree;
            }
        }

        // ----- Substitutions ------------------------------------------
        public TypeTree Substitute(string subVar, TypeTree subTree) {
            if (this.IsLeaf()) {
                if (this.value == subVar) {
                    return subTree.DeepCopy();
                } else {
                    return TypeTree.MakePrimitiveTree(this.value);
                }
            } else {
                TypeTree leftSub = this.left.Substitute(subVar, subTree);
                TypeTree rightSub = this.right.Substitute(subVar, subTree);
                return new TypeTree(leftSub, rightSub);
            }
        }
        public TypeTree Substitute(Dictionary<string,TypeTree> subs) {
            if (subs == null) return null;
            if(this.IsLeaf()) {
                if (subs.ContainsKey(this.value)) return subs[this.value].DeepCopy();
                else return this.DeepCopy();
            } else {
                TypeTree left = this.GetLeft().Substitute(subs);
                TypeTree right = this.GetRight().Substitute(subs);
                return new TypeTree(left, right);
            }
        }

        public TypeTree MakeTypeVarsUnique(TypeTree parentTree) {
            TypeTree newChildTree = this.DeepCopy();

            List<string> parentTypeVars = parentTree.GetTypeVariables();
            List<string> childTypeVars = this.GetTypeVariables();
            List<string> allTypeVars = new List<string>(parentTypeVars.Concat(childTypeVars));
            foreach (var childTypeVar in childTypeVars) {
                if (parentTypeVars.Contains(childTypeVar)) {
                    string newVar = TypeTree.AddPrime(childTypeVar, allTypeVars);
                    newChildTree.ReplaceName(childTypeVar, newVar);
                }
            }
            return newChildTree;
        }

        // keep adding the ' character to a type variable until it is unique
        public static string AddPrime(string s, List<string> typeVars) {
            while (typeVars.Contains(s)) {
                s += "'";
            }
            return s;
        }
        public TypeTree AddPrime(List<string> usedTypeVars) {
            TypeTree t2 = this.DeepCopy();
            List<string> treeTypeVars = this.GetTypeVariables();
            foreach(var typeVar in usedTypeVars) {
                if(treeTypeVars.Contains(typeVar)) {
                    string newTypeVar = AddPrime(typeVar, usedTypeVars);
                    t2.ReplaceName(typeVar, newTypeVar);
                }
            }
            return t2;
        }

        public void ReplaceName(string oldName, string newName) {
            if (this.IsLeaf()) {
                if (this.value == oldName) {
                    this.value = newName;
                }
            } else {
                this.left.ReplaceName(oldName, newName);
                this.right.ReplaceName(oldName, newName);
            }
        }
        public void ReplaceNames(Dictionary<string,string> subs) {
            if (this.IsLeaf()) {
                if (subs.ContainsKey(this.value)) {
                    this.value = subs[this.value];
                }
            } else {
                this.left.ReplaceNames(subs);
                this.right.ReplaceNames(subs);
            }
        }


        // ----- Namespace stuff ----------------------------------------
        public static bool IsTypeVariable(string name) {
            if (string.IsNullOrEmpty(name)) return false;
            if (!char.IsLetter(name[0])) return false;
            if(name[0] != char.ToLower(name[0])) return false;
            foreach(var c in name) {
                if (!char.IsLetterOrDigit(c)) return false;
            }
            return true;
        }

        // Get a list of all type variables in the tree
        public List<string> GetTypeVariables(List<string> vars = null) {
            vars = vars ?? new List<string>();

            if(this.IsLeaf()) {
                string val = this.value;
                if (TypeTree.IsTypeVariable(val) && !vars.Contains(val)) {
                    vars.Add(val);
                }
            } else {
                this.left.GetTypeVariables(vars);
                this.right.GetTypeVariables(vars);
            }

            return vars;
        }
        public List<string> GetTypeConstants(List<string> vars = null) {
            vars = vars ?? new List<string>();

            if (this.IsLeaf()) {
                string val = this.value;
                if (!TypeTree.IsTypeVariable(val) && !vars.Contains(val)) {
                    vars.Add(val);
                }
            } else {
                this.left.GetTypeConstants(vars);
                this.right.GetTypeConstants(vars);
            }

            return vars;
        }
        public List<string> GetTypeUnderscoreVariables(List<string> vars = null) {
            vars = vars ?? new List<string>();

            if (this.IsLeaf()) {
                string val = this.value;
                if (val[0] == '_' && !vars.Contains(val)) {
                    vars.Add(val);
                }
            } else {
                this.left.GetTypeUnderscoreVariables(vars);
                this.right.GetTypeUnderscoreVariables(vars);
            }

            return vars;
        }



        // ----- Parsing and conversion To/From other datatypes ---------
        // display a TypeTree as (Bool -> (Bool -> Bool)), for example
        public override string ToString() {
            if (this.IsLeaf()) {
                return this.value;
            } else {
                string symbol = this.left?.left?.value;
                if (symbol == "->" || symbol == "," || symbol == "|") {
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

        public static TypeTree TypeTreeFromTreeList(List<TypeTree> treeList) {
            if (treeList == null || treeList.Count == 0) {
                //Console.WriteLine("Cannot make TypeTree from List: treeList is empty!");
                // this isn't an error: this is normal functionality...
                return null;
            }
            TypeTree tree = treeList[treeList.Count - 1].DeepCopy();
            for (int i = treeList.Count - 2; i >= 0; i--) {
                //tree.SetChildren(treeList[i].DeepCopy(), tree.DeepCopy());
                tree = new TypeTree(treeList[i].DeepCopy(), tree.DeepCopy(), TypeConstructorEnum.Function);
            }
            return tree;
        }


    }
}
