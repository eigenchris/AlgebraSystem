using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystemV4 {
    public class TypeTree {

        public List<string> boundTypeVars;

        private TypeTree left;
        private TypeTree right;
        private string _value;
        // when we write to _value, kill the child nodes
        public string value {
            get { return _value; }
            set { _value = value; left = null; right = null; }
        }
        
        public TypeTree(string s, string boundVars = "") {
            TypeTree temp = Parser.ParseTypeTree(s);
            if (temp == null) { // parseing failed
                this.value = string.Empty;
            } else if (temp.IsPrimitive()) {
                this.value = temp.value;
            } else {
                this.SetChildren(temp.left,temp.right);
            }
            this.boundTypeVars = Parser.CssToList(boundVars);
        }
        public TypeTree(TypeTree l, TypeTree r) {
            this.SetChildren(l.DeepCopy(), r.DeepCopy());
        }
        public TypeTree() {
            this.value = string.Empty;
        }
        public static TypeTree MakePrimitiveTree(string s) {
            TypeTree temp = new TypeTree();
            temp.value = s;
            return temp;
        }

        // Getting is normal, but setting should set the string to empty
        public void SetChildren(TypeTree l, TypeTree r) {
            this.value = string.Empty;
            this.left = l;
            this.right = r;
        }
        public TypeTree GetLeft() { return this.left; }
        public TypeTree GetRight() { return this.right; }


        // ----- Copying and Equals/Matching ------------------
        public TypeTree DeepCopy() {
            if(this.IsPrimitive()) {
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

        // (Bool -> (Bool -> (Bool -> Bool))) will match (i -> j) with
        //  i = Bool;   j = (Bool -> (Bool -> Bool))
        // which is exactly how things work in Haskell
        public static bool MatchPrototype(TypeTree prototype, TypeTree candidate, Dictionary<string, TypeTree> typeVarLookup) {
            // if prototype node is a branch, confirm that candidate node is also a branch, and check for matches on the left and right sides
            if (!prototype.IsPrimitive()) {
                if (candidate.IsPrimitive()) {
                    return false;
                } else {
                    return MatchPrototype(prototype.left, candidate.left, typeVarLookup)
                        && MatchPrototype(prototype.right, candidate.right, typeVarLookup);
                }
            } else { // otherwise, if we meet a leaf node in the prototype
                // if the value is a type variable...
                //if (ns.typeVariables.Contains(prototype.value)) {
                if (TypeTree.IsTypeVariable(prototype.value)) {
                    // if it already has been logged in the typeVarLookup, confirm it equals the candidate
                    //  .DeepEquals() is fine, since, even if the candidate contains more type variables, they should
                    //  be the same throughout the tree
                    if (typeVarLookup.ContainsKey(prototype.value)) {
                        return typeVarLookup[prototype.value].DeepEquals(candidate);
                    } else { // otherwise, bind the candidate tree to the type variable
                        typeVarLookup.Add(prototype.value, candidate.DeepCopy());
                        return true;
                    }
                } else { // if the value is NOT a type variable, confirm it equals teh candidate value
                    return prototype.value == candidate.value;
                }
            }
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

            if (!t1.IsPrimitive() && !t2.IsPrimitive()) {
                var success1 = Unify(t1.GetLeft(), t2.GetLeft(), subs);
                if (success1 == null) return null;
                var success2 = Unify(t1.GetRight(), t2.GetRight(), subs);
                if (success2 == null) return null;
            } else if (t1.IsPrimitive() && TypeTree.IsTypeVariable(t1.value)) {
                if(!subs.ContainsKey(t1.value)) {
                    subs.Add(t1.value, t2.DeepCopy());
                } else if(t2.IsPrimitive() && t1.value != t2.value) { // don't map a variable to itself
                    var success = Unify(subs[t1.value], t2, subs);   // If a type var matches two different subtrees, unify them
                    if (success == null) return null;
                }
            } else if (t2.IsPrimitive() && TypeTree.IsTypeVariable(t2.value)) {
                if (!subs.ContainsKey(t2.value)) {
                    subs.Add(t2.value, t1.DeepCopy());
                } else if (t1.IsPrimitive() && t2.value != t1.value) { // don't map a variable to itself
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
                if(subs[key].IsPrimitive() && key==subs[key].value) {
                    subs.Remove(key);
                }
            }
        }

        // ----- Basic Tree Operations ----------------------------------
        // does the tree have zero children? (no left child should mean no right child)
        public bool IsPrimitive() {
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
            if (this.IsPrimitive()) {
                return null;
            } else {
                return this.right.DeepCopy();
            }
        }

        public TypeTree PopOutput() {
            TypeTree returnTree = this.DeepCopy();
            if (returnTree.IsPrimitive()) {
                return null;
            } else if (returnTree.right.IsPrimitive()) {
                return returnTree.left;
            } else {
                TypeTree tempTree1 = returnTree;
                TypeTree tempTree2 = returnTree.right;
                while (!tempTree2.right.IsPrimitive()) {
                    tempTree1 = tempTree1.right;
                    tempTree2 = tempTree2.right;
                }
                tempTree1.right = tempTree2.left;
                return returnTree;
            }
        }

        // ----- Substitutions ------------------------------------------
        public TypeTree Substitute(string subVar, TypeTree subTree) {
            if (this.IsPrimitive()) {
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
            if(this.IsPrimitive()) {
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
            if (this.IsPrimitive()) {
                if (this.value == oldName) {
                    this.value = newName;
                }
            } else {
                this.left.ReplaceName(oldName, newName);
                this.right.ReplaceName(oldName, newName);
            }
        }
        public void ReplaceString(Dictionary<string, string> subs) {
            foreach (var key in subs.Keys) {
                this.ReplaceName(key, subs[key]);
            }
        }


        // ----- Namespace stuff ----------------------------------------
        public bool ValidateAgainstNamespace(Namespace ns) {
            if (this.IsPrimitive()) {
                return TypeTree.IsTypeVariable(this.value) || (ns.TypeLookup(this.value) != null);                    
            } else {
                return this.left.ValidateAgainstNamespace(ns) && this.right.ValidateAgainstNamespace(ns);
            }
        }

        public static bool IsTypeVariable(string name) {
            if (name == "") return false;
            return name[0] == char.ToLower(name[0]);
        }

        // Get a list of all type variables in the tree
        public List<string> GetTypeVariables(List<string> vars = null) {
            vars = vars ?? new List<string>();

            if(this.IsPrimitive()) {
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

            if (this.IsPrimitive()) {
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




        // ----- Parsing and conversion To/From other datatypes ---------
        // display a TypeTree as (Bool -> (Bool -> Bool)), for example
        public override string ToString() {
            if (this.IsPrimitive()) {
                return this.value;
            } else {
                return "(" + this.left.ToString() + " -> " + this.right.ToString() + ")";
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
                tree.SetChildren(treeList[i].DeepCopy(), tree.DeepCopy());
            }
            return tree;
        }


    }
}
