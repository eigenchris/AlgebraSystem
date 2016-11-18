using System;
using System.Collections.Generic;
using System.Linq;

namespace AlgebraSystem {
    public class TermNew {
        public TypeTree typeTree;

        private TermNew left;
        private TermNew right;
        private string _value;
        // when we write to _value, kill the child nodes
        public string value
        {
            get { return _value; }
            set { _value = value; left = null; right = null; }
        }

        public TermNew(TermNew l, TermNew r, TypeTree typeExpr) {
            this.SetChildren(l.DeepCopy(), r.DeepCopy());
            this.typeTree = typeExpr;
        }
        public TermNew() {
            this.value = null;
        }
        public static TermNew MakePrimitiveTree(string s, TypeTree typeTree) {
            TermNew temp = new TermNew();
            temp.value = s;
            temp.typeTree = typeTree;
            return temp;
        }
        // Getting is normal, but setting should set the string to empty
        public void SetChildren(TermNew l, TermNew r) {
            this.value = null;
            this.left = l;
            this.right = r;
        }

        /*
        public static TermNew TermNewFromSExpression(SExpression sexp) {
            if (sexp.IsLeaf()) {
                return TermNew.MakePrimitiveTree(sexp.value);
            } else {
                TermNew left = TermNewFromSExpression(sexp.left);
                TermNew right = TermNewFromSExpression(sexp.right);
                left.Apply(right);
                return left;
            }
        }
        */

        // ----- Copying and Equals/Matching ------------------
        public TermNew DeepCopy() {
            // It might be better to use Apply() so we get the type inference for free...
            if (this.IsLeaf()) {
                TermNew ta = TermNew.MakePrimitiveTree(this.value, this.typeTree.DeepCopy());
                return ta;
            } else {
                return new TermNew(this.left, this.right, this.typeTree.DeepCopy());
            }
        }
        public bool DeepEquals(TermNew t) {
            if (this.IsLeaf() ^ t.IsLeaf()) return false;
            if (this.IsLeaf()) return this.value == t.value;
            return this.left.DeepEquals(t.left) && this.right.DeepEquals(t.right);
        }

        public bool IsLeaf() {
            return this.left == null;
        }



        // ----- Parsing and conversion To/From other datatypes ---------
        public override string ToString() {
            if (this.IsLeaf()) return this.value;

            TermNew currentTree = this;
            string childrenString = currentTree.right.ToString();
            while (!currentTree.left.IsLeaf()) {
                currentTree = currentTree.left;
                childrenString = currentTree.right + " " + childrenString; // pre-pend;                
            }
            childrenString = currentTree.left + " " + childrenString;

            return "(" + childrenString + ")";
        }

        public static TermNew TermFromSExpression(string s, Namespace parentNS) {
            SExpression sexp = Parser.ParseSExpression(s);
            return TermFromSExpression(sexp, parentNS);
        }
        public static TermNew TermFromSExpression(SExpression sexp, Namespace parentNS) {
            var tuple = TypeInference(sexp, parentNS);
            Dictionary<string,TypeTree> namesToTypes = tuple.Item1;
            TermNew termNew = ConstructFromSExpression(sexp, namesToTypes);
            return termNew;
        }
        public static TermNew ConstructFromSExpression(SExpression sexp, Dictionary<string,TypeTree> namesToType) {
            if(sexp.IsLeaf()) {
                TypeTree typeTree = namesToType[sexp.value];
                return TermNew.MakePrimitiveTree(sexp.value, typeTree);
            } else {
                TermNew leftTerm = ConstructFromSExpression(sexp.left, namesToType);
                TermNew rightTerm = ConstructFromSExpression(sexp.right, namesToType);
                TypeTree typeTree = leftTerm.typeTree.GetRight();
                return new TermNew(leftTerm, rightTerm, typeTree);
            }
        }

        // ----- New Type Inference Attempt -----------------------------
        public static Dictionary<string, TypeTree> GetVarTypes(SExpression sexp, Namespace ns) {
            var variableTypes = new Dictionary<string, TypeTree>();
            var introducedTypeVars = new List<string>();
            GetVarTypesRecur(sexp, ns, variableTypes, introducedTypeVars);
            return variableTypes;
        }

        public static void GetVarTypesRecur(SExpression sexp, Namespace ns, Dictionary<string, TypeTree> variableTypes, List<string> introducedTypeVars) {
            if (!sexp.IsLeaf()) {
                GetVarTypesRecur(sexp.right, ns, variableTypes, introducedTypeVars);
                GetVarTypesRecur(sexp.left, ns, variableTypes, introducedTypeVars);
            } else {
                string v = sexp.value;
                TypeTree t; 
                if (variableTypes.ContainsKey(v)) {
                } else if (ns.ContainsVariable(v)) {
                    t = ns.VariableLookup(sexp.value).typeExpr.typeTree.DeepCopy();
                    variableTypes.Add(v, t);
                } else {
                    string eTypeVar = "v" + introducedTypeVars.Count;
                    t = new TypeTree(eTypeVar);
                    variableTypes.Add(v, t);
                    introducedTypeVars.Add(eTypeVar);
                }
            }
        }



        public static TypeTree GetTypeEquations(SExpression sexp, Dictionary<string, TypeTree> variableTypes, List<Tuple<TypeTree, TypeTree>> equations) {
            var introducedTypeVars = new List<string>();
            var t = GetTypeEquationsRecur(sexp, variableTypes, equations, introducedTypeVars);
            return t;
        }

        public static TypeTree GetTypeEquationsRecur(SExpression sexp, Dictionary<string, TypeTree> variableTypes, List<Tuple<TypeTree, TypeTree>> equations, List<string> introducedTypeVars) {
            if (sexp.IsLeaf()) {
                return variableTypes[sexp.value];
            }
            // unifty(t0, t1->t)
            // t0 is the type of the left subtree (function which is acting)
            // t1 is the type of the right subtree (variable being acted on)
            // t is the type of this node (the result of the function acting on the variable)
            var t0 = GetTypeEquationsRecur(sexp.left, variableTypes, equations, introducedTypeVars);
            if (t0 == null) return null;
            var t1 = GetTypeEquationsRecur(sexp.right, variableTypes, equations, introducedTypeVars);
            if (t1 == null) return null;

            // Generate a tpye variable for this node in the S-Expression
            string eTypeVar = "e" + introducedTypeVars.Count;
            introducedTypeVars.Add(eTypeVar);
            var t = TypeTree.MakePrimitiveTree(eTypeVar);

            // Unify (t0, t1->t) and get the resulting type t
            var t1_to_t = new TypeTree(t1, t, TypeConstructor.Function);
            equations.Add(new Tuple<TypeTree, TypeTree>(t0, t1_to_t));

            return t;
        }

        // converts a list of type equations into a dictionary mapping type variables to their corresponding expressions
        public static Dictionary<string,TypeTree> SolveTypeEquations(List<Tuple<TypeTree,TypeTree>> typeEquations) {
            var masterSubs = new Dictionary<string, TypeTree>(); 
            for(int i=0; i<typeEquations.Count; i++) {
                var subs = TypeTree.UnifyAndSolve(typeEquations[i].Item1, typeEquations[i].Item2);
                foreach(var k in subs.Keys) {
                    if (!masterSubs.ContainsKey(k)) masterSubs.Add(k, subs[k]);
                    else if (subs[k].DeepEquals(masterSubs[k]) || CheckIfEquationExists(typeEquations, subs[k], masterSubs[k])) continue;
                    else typeEquations.Add(new Tuple<TypeTree, TypeTree>(subs[k], masterSubs[k]));
                }
            }

            TypeTree.SolveMappings(masterSubs);
            return masterSubs;
        }

        public static bool CheckIfEquationExists(List<Tuple<TypeTree,TypeTree>> equationList, TypeTree tree1, TypeTree tree2) {
            foreach(var eqn in equationList) {
                if (!eqn.Item1.DeepEquals(tree1)) continue;
                if (!eqn.Item2.DeepEquals(tree2)) continue;
                return true;
            }
            return false;
        }

        public static Tuple<Dictionary<string, TypeTree>, TypeTree> TypeInference(SExpression sexp, Namespace ns) {
            // assign types to a ll variables and make all type variables unique across the system
            var namesToTypes = GetVarTypes(sexp, ns);
            MakeTypeVarsUniqueAndNice(namesToTypes);

            // get and solve type equations
            var typeEquations = new List<Tuple<TypeTree, TypeTree>>();
            var expressionType = GetTypeEquations(sexp, namesToTypes, typeEquations);
            var typeSolutions = SolveTypeEquations(typeEquations);

            // substitute solutions to type equations
            var keys = namesToTypes.Keys.ToList();
            for (int i = 0; i < keys.Count; i++) {
                string k = keys[i];
                namesToTypes[k] = namesToTypes[k].Substitute(typeSolutions);
            }
            expressionType = expressionType.Substitute(typeSolutions);

            return new Tuple<Dictionary<string, TypeTree>, TypeTree>(namesToTypes, expressionType);
        }



        public static void MakeTypeVarsUniqueAndNice(Dictionary<string, TypeTree> namesToTypes) {
            int charInt = 97;
            foreach (var k in namesToTypes.Keys) {
                List<string> typeVarList = namesToTypes[k].GetTypeVariables();
                var subs = new Dictionary<string, string>();
                foreach (var v in typeVarList) {
                    subs.Add(v, ((char)charInt).ToString());
                    charInt++;
                }
                namesToTypes[k].ReplaceNames(subs);
            }
        }



        }
}