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

        public TermNew(TermNew l, TermNew r) {
            this.SetChildren(l.DeepCopy(), r.DeepCopy());
        }
        public TermNew() {
            this.value = null;
        }
        public static TermNew MakePrimitiveTree(string s) {
            TermNew temp = new TermNew();
            temp.value = s;
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
                TermNew ta = TermNew.MakePrimitiveTree(this.value);
                return ta;
            } else {
                return new TermNew(this.left, this.right);
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


        public static TypeTree GetTypeEquations(SExpression sexp, Dictionary<string,TypeTree> variableTypes, Dictionary<string,TypeTree> subs) {
            var introducedTypeVars = new List<string>();
            var t = GetTypeEquationsRecur(sexp, variableTypes, subs, introducedTypeVars);
            return t;
        }

        public static TypeTree GetTypeEquationsRecur(SExpression sexp, Dictionary<string, TypeTree> variableTypes, Dictionary<string, TypeTree> subs, List<string> introducedTypeVars) {
            if(sexp.IsLeaf()) {
                return variableTypes[sexp.value];
            }
            // unifty(t0, t1->t)
            // t0 is the type of the left subtree (function which is acting)
            // t1 is the type of the right subtree (variable being acted on)
            // t is the type of this node (the result of the function acting on the variable)
            var t0 = GetTypeEquationsRecur(sexp.left, variableTypes, subs, introducedTypeVars);
            if (t0 == null) return null;
            var t1 = GetTypeEquationsRecur(sexp.right, variableTypes, subs, introducedTypeVars);
            if (t1 == null) return null;

            // Generate a tpye variable for this node in the S-Expression
            string eTypeVar = "e" + introducedTypeVars.Count;
            introducedTypeVars.Add(eTypeVar);
            var t = TypeTree.MakePrimitiveTree(eTypeVar);

            // Unify (t0, t1->t) and get the resulting type t
            var t1_to_t = new TypeTree(t1, t, TypeConstructor.Function);
            var newSubs = TypeTree.UnifyAndSolve(t0, t1_to_t);
            if (newSubs == null) return null;
            var thisNodeTypeTree = t.Substitute(newSubs);

            //  Update the current subs dictionary with any new ones added in newSubs
            foreach(var k in newSubs.Keys) {
                if(!subs.ContainsKey(k)) subs.Add(k, newSubs[k]);
                else { // for keys that already exist, unify and get the best possible solution
                    var fixSubs = TypeTree.UnifyAndSolve(subs[k], newSubs[k]);
                    if (fixSubs.Count > 0) {
                        var newTree = subs[k].Substitute(fixSubs);
                        subs[k] = newTree;
                    }
                }
            }

            return thisNodeTypeTree;
        }


        public static TypeTree GetTypeEquations2(SExpression sexp, Dictionary<string, TypeTree> variableTypes, List<Tuple<TypeTree, TypeTree>> equations) {
            var introducedTypeVars = new List<string>();
            var t = GetTypeEquationsRecur2(sexp, variableTypes, equations, introducedTypeVars);
            return t;
        }

        public static TypeTree GetTypeEquationsRecur2(SExpression sexp, Dictionary<string, TypeTree> variableTypes, List<Tuple<TypeTree, TypeTree>> equations, List<string> introducedTypeVars) {
            if (sexp.IsLeaf()) {
                return variableTypes[sexp.value];
            }
            // unifty(t0, t1->t)
            // t0 is the type of the left subtree (function which is acting)
            // t1 is the type of the right subtree (variable being acted on)
            // t is the type of this node (the result of the function acting on the variable)
            var t0 = GetTypeEquationsRecur2(sexp.left, variableTypes, equations, introducedTypeVars);
            if (t0 == null) return null;
            var t1 = GetTypeEquationsRecur2(sexp.right, variableTypes, equations, introducedTypeVars);
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

        public static Dictionary<string,TypeTree> SolveTypeEquations(List<Tuple<TypeTree,TypeTree>> typeEquations) {
            var masterSubs = new Dictionary<string, TypeTree>();
            for(int i=0; i<typeEquations.Count; i++) {
                var subs = TypeTree.UnifyAndSolve(typeEquations[i].Item1, typeEquations[i].Item2);
                foreach(var k in subs.Keys) {
                    if (!masterSubs.ContainsKey(k)) masterSubs.Add(k, subs[k]);
                    else if (subs[k].DeepEquals(masterSubs[k]) || CheckIfEquationExists(typeEquations, subs[k], masterSubs[k])) continue;
                    else typeEquations.Add(new Tuple<TypeTree, TypeTree>(subs[k], masterSubs[k]));
                        /*
                        var fixSubs = TypeTree.UnifyAndSolve(subs[k], masterSubs[k]);
                        if (fixSubs.Count > 0) {
                            var newTree = masterSubs[k].Substitute(fixSubs);
                            masterSubs[k] = newTree;
                        }
                        foreach(var key in fixSubs.Keys)
                        typeEquations.Add(new Tuple<TypeTree,TypeTree>(TypeTree.MakePrimitiveTree(key), fixSubs[key]));
                        */
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

        public static Tuple<Dictionary<string, TypeTree>, TypeTree> TypeInference2(SExpression sexp, Namespace ns) {
            // assign types to a ll variables and make all type variables unique across the system
            var namesToTypes = GetVarTypes(sexp, ns);
            MakeTypeVarsUniqueAndNice(namesToTypes);

            // get and solve type equations
            var typeEquations = new List<Tuple<TypeTree, TypeTree>>();
            var expressionType = GetTypeEquations2(sexp, namesToTypes, typeEquations);
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



        public static Tuple<Dictionary<string,TypeTree>,TypeTree> TypeInference(SExpression sexp, Namespace ns) {
            // assign types to a ll variables and make all type variables unique across the system
            var namesToTypes = GetVarTypes(sexp, ns);
            MakeTypeVarsUniqueAndNice(namesToTypes);

            // get and solve type equations
            var typeEquations = new Dictionary<string, TypeTree>();
            var expressionType = GetTypeEquations(sexp, namesToTypes, typeEquations);
            TypeTree.SolveMappings(typeEquations);

            // substitute solutions to type equations
            var keys = namesToTypes.Keys.ToList();
            for (int i=0; i<keys.Count; i++) {
                string k = keys[i];
                namesToTypes[k] = namesToTypes[k].Substitute(typeEquations);
            }
            expressionType.Substitute(typeEquations);

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


            /*
            public static bool TypeInference(SExpression sexp,
                                    Namespace ns,
                                    Dictionary<string, TypeTree> variableTypes = null,
                                    Dictionary<SExpression, TypeTree> expressionTypes = null,
                                    List<string> introducedVars = null,
                                    List<string> introducedTypeVars = null) {
                variableTypes = variableTypes ?? new Dictionary<string, TypeTree>();
                expressionTypes = expressionTypes ?? new Dictionary<SExpression, TypeTree>();
                introducedVars = introducedVars ?? new List<string>();
                introducedTypeVars = introducedTypeVars ?? new List<string>();

                // Do a post-order transversal (children first)
                if (!sexp.IsLeaf()) {
                    bool success;
                    success = TypeInference(sexp.GetLeft(), ns, variableTypes, expressionTypes, introducedVars, introducedTypeVars);
                    if (!success) return false;
                    success = TypeInference(sexp.GetRight(), ns, variableTypes, expressionTypes, introducedVars, introducedTypeVars);
                    if (!success) return false;
                }
                */

        }
}