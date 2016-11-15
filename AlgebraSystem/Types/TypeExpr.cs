using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public class TypeExpr {
        public List<string> boundTypeVars;
        public TypeTree typeTree;

        public TypeExpr(TypeTree t, string boundTypeVars = "") {
            this.typeTree = t;
            this.boundTypeVars = Parser.CssToList(boundTypeVars);
        }
        public TypeExpr(string s, string boundTypeVars = "") : 
            this(new TypeTree(s), boundTypeVars) { }
        public TypeExpr(TypeTree typeTree, List<string> boundTypeVars) {
            this.typeTree = typeTree;
            this.boundTypeVars = boundTypeVars;
        }
        public TypeExpr(string s) {
            var temp = Parser.ParseTypeExpr(s);
            this.typeTree = temp.typeTree;
            this.boundTypeVars = temp.boundTypeVars;
        }

        public TypeExpr() {
            this.boundTypeVars = new List<string>();
            this.typeTree = new TypeTree();
        }
        public static TypeExpr MakePrimitiveTree(string s) {
            TypeExpr te = new TypeExpr();
            te.typeTree = TypeTree.MakePrimitiveTree(s);
            return te;
        }

        public bool DeepEquals(TypeExpr te) {
            if (this.boundTypeVars.Count != te.boundTypeVars.Count) return false;
            for (int i = 0; i < this.boundTypeVars.Count; i++) {
                if (this.boundTypeVars[i] != te.boundTypeVars[i]) return false;
            }
            return this.typeTree.DeepEquals(te.typeTree);
        }
        public TypeExpr DeepCopy() {
            TypeExpr te = new TypeExpr();
            te.typeTree = this.typeTree.DeepCopy();
            te.boundTypeVars = new List<string>(this.boundTypeVars);
            return te;
        }

        public int GetNumberOfInputs() {
            return this.typeTree.GetNumberOfInputs();
        }

        // ----- Instance of -------------------------------
        public bool InstanceOf(TypeExpr generalTree) {
            return this.typeTree.InstanceOf(generalTree.typeTree, generalTree.boundTypeVars);
        }

        // ----- Unification -------------------------------
        public static Dictionary<string, TypeTree> UnifyAndSolve(TypeExpr t1, TypeExpr t2, Dictionary<string, TypeTree> subs = null) {
            if (t1 == null || t2 == null) return null;
            subs = subs ?? new Dictionary<string, TypeTree>();

            subs = Unify(t1, t2, subs);
            if (subs == null) return null;

            TypeTree.SolveMappings(subs);
            return subs;
        }

        // this assumes type vars in t1 and t2 don't overlap; they are unique
        public static Dictionary<string, TypeTree> Unify(TypeExpr t1, TypeExpr t2, Dictionary<string, TypeTree> subs = null) {
            t2.AlphaConvertUnique(t1);
            return Unify(t1.typeTree, t2.typeTree, t1.boundTypeVars, t2.boundTypeVars, subs);
        }

        // returns a dictionary "subs" such that t1.Substitute(subs) == t2.Substitute(subs)
        private static Dictionary<string, TypeTree> Unify(TypeTree t1, TypeTree t2, List<string> vars1, List<string> vars2, Dictionary<string, TypeTree> subs = null) {
            subs = subs ?? new Dictionary<string, TypeTree>();

            if (!t1.IsLeaf() && !t2.IsLeaf()) {
                var success1 = Unify(t1.GetLeft(), t2.GetLeft(), vars1, vars2, subs);
                if (success1 == null) return null;
                var success2 = Unify(t1.GetRight(), t2.GetRight(), vars1, vars2, subs);
                if (success2 == null) return null;
            } else if (t1.IsLeaf() && vars1.Contains(t1.value)) {
                if (!subs.ContainsKey(t1.value)) {
                    subs.Add(t1.value, t2.DeepCopy());
                } else { // "don't map a variable to itself" is old reasoning... we must alpha-convert trees before unifying them
                    var success = Unify(subs[t1.value], t2, vars2, vars2, subs);   // If a type var matches two different subtrees, unify them
                    if (success == null) return null;
                }
            } else if (t2.IsLeaf() && vars2.Contains(t2.value)) {
                if (!subs.ContainsKey(t2.value)) {
                    subs.Add(t2.value, t1.DeepCopy());
                } else { 
                    var success = Unify(subs[t2.value], t1, vars1, vars1, subs);   // If a type var matches two different subtrees, unify them
                    if (success == null) return null;
                }
            } else if (!t1.DeepEquals(t2)) {
                Console.WriteLine("Error in Unify:");
                Console.WriteLine("Value " + t1 + " does not match " + t2);
                return null;
            }
            return subs;
        }



        // ----- Substitutions ------------------------------------------
        public TypeExpr Substitute(string subVar, TypeTree subTree) {
            if(this.boundTypeVars.Contains(subVar)) {
                var vars = new List<string>(this.boundTypeVars);
                vars.Remove(subVar);
                foreach (var v in subTree.GetTypeVariables()) {
                    if(!vars.Contains(v)) vars.Add(v);
                }
                var tree = this.typeTree.Substitute(subVar, subTree);
                return new TypeExpr(tree, vars);
            } else {
                return this.DeepCopy();
            }
        }

        public TypeExpr Substitute(Dictionary<string, TypeTree> subs) {
            if (subs == null) return null;
            var tempTypeExpr = this.DeepCopy();
            foreach(var v in subs.Keys) {
                tempTypeExpr = tempTypeExpr.Substitute(v,subs[v]);
            }
            return tempTypeExpr;
        }

        // Given "this" and another tree, rename type vars in "this" tree so they don't conflict with those in the input parameter
        public void AlphaConvertUnique(TypeExpr tree) {
            foreach (var v in tree.boundTypeVars) {
                int idx = this.boundTypeVars.IndexOf(v);
                if (idx!=-1) {
                    string newVar = v + "'";
                    while(this.boundTypeVars.Contains(newVar) || tree.boundTypeVars.Contains(newVar)) {
                        newVar += "'";
                    }
                    this.boundTypeVars[idx] = newVar;
                    this.typeTree.ReplaceName(v, newVar);
                }
            }
        }


        // ----- Parsing and conversion To/From other datatypes ---------
        // display a TypeTree as (Bool -> (Bool -> Bool)), for example
        public override string ToString() {
            string prefix = "";
            int N = this.boundTypeVars.Count;
            if (N>0) {
                prefix += "Forall";
                for(int i=0; i<N; i++) {
                    prefix += " " + this.boundTypeVars[i];
                    if (i < N - 1) prefix += ",";
                }
                prefix += " . ";
            }
            return prefix + this.typeTree;
        }



    }
}
