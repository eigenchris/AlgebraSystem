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

        public TermNew Substitute(string subVar, TermNew subTree) {
            if (this.IsLeaf()) {
                if (this.value == subVar) {
                    return subTree.DeepCopy();
                } else {
                    return TermNew.MakePrimitiveTree(this.value, this.typeTree);
                }
            } else {
                TermNew leftSub = this.left.Substitute(subVar, subTree);
                TermNew rightSub = this.right.Substitute(subVar, subTree);
                return new TermNew(leftSub, rightSub, leftSub.typeTree.GetRight());
            }
        }
        public TermNew Substitute(Dictionary<string, TermNew> subs) {
            if (subs == null || !subs.Any()) return this.DeepCopy();

            //post-order transveral
            if (this.IsLeaf()) {
                if (subs != null && subs.ContainsKey(this.value)) {
                    TermNew tn = subs[this.value].DeepCopy();
                    return tn;
                } else {
                    TermNew tn = TermNew.MakePrimitiveTree(this.value, this.typeTree.DeepCopy());
                    return tn;
                }
            } else {
                TermNew left = this.left.Substitute(subs);
                TermNew right = this.right.Substitute(subs);
                return new TermNew(left, right, left.typeTree.GetRight());
            }
        }


        // ----- Helper Methods -------------------------------
        public List<string> GetSymbols(List<string> vars = null) {
            vars = vars ?? new List<string>();

            if (this.IsLeaf()) {
                string val = this.value;
                if (TypeTree.IsTypeVariable(val) && !vars.Contains(val)) {
                    vars.Add(val);
                }
            } else {
                this.left.GetSymbols(vars);
                this.right.GetSymbols(vars);
            }

            return vars;
        }

        public TermNew MakeSymbolsUnique(TermNew usedSymbolsTree) {
            TermNew newTerm = this.DeepCopy();

            List<string> usedSymbols = usedSymbolsTree.GetSymbols();
            List<string> existingSymbols = this.GetSymbols();
            List<string> allSymbols = new List<string>(usedSymbols.Concat(existingSymbols));
            foreach (var sym in existingSymbols) {
                if (usedSymbols.Contains(sym)) {
                    string newVar = TypeTree.AddPrime(sym, allSymbols);
                    newTerm.ReplaceName(sym, newVar);
                }
            }
            return newTerm;
        }


        // keep adding the ' character to a type variable until it is unique
        public static string AddPrime(string s, List<string> typeVars) {
            while (typeVars.Contains(s)) {
                s += "'";
            }
            return s;
        }
        public TermNew AddPrime(List<string> usedTypeVars) {
            TermNew t2 = this.DeepCopy();
            List<string> treeTypeVars = this.GetSymbols();
            foreach (var typeVar in usedTypeVars) {
                if (treeTypeVars.Contains(typeVar)) {
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
        public void ReplaceNames(Dictionary<string, string> subs) {
            if (this.IsLeaf()) {
                if (subs.ContainsKey(this.value)) {
                    this.value = subs[this.value];
                }
            } else {
                this.left.ReplaceNames(subs);
                this.right.ReplaceNames(subs);
            }
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
            var expressionNS = new Namespace(parentNS);
            Dictionary<string, TypeTree> namesToTypes = TypeInference(sexp, expressionNS);
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
                    t = ns.VariableLookup(v).typeExpr.typeTree.DeepCopy();
                    variableTypes.Add(v, t);
                } else {
                    string eTypeVar = "v" + introducedTypeVars.Count;
                    t = new TypeTree(eTypeVar);
                    variableTypes.Add(v, t);
                    introducedTypeVars.Add(eTypeVar);
                }
            }
        }


        public static TypeTree GetTypeEquations(SExpression sexp, Dictionary<string, TypeTree> variableTypes, List<Tuple<TypeTree, TypeTree>> equations, List<string> introducedTypeVars = null) {
            introducedTypeVars = introducedTypeVars ?? new List<string>();
            if (sexp.IsLeaf()) {
                return variableTypes[sexp.value];
            }
            // unifty(t0, t1->t)
            // t0 is the type of the left subtree (function which is acting)
            // t1 is the type of the right subtree (variable being acted on)
            // t is the type of this node (the result of the function acting on the variable)
            var t0 = GetTypeEquations(sexp.left, variableTypes, equations, introducedTypeVars);
            if (t0 == null) return null;
            var t1 = GetTypeEquations(sexp.right, variableTypes, equations, introducedTypeVars);
            if (t1 == null) return null;

            // Generate a tpye variable for this node in the S-Expression
            string eTypeVar = "e" + introducedTypeVars.Count;
            introducedTypeVars.Add(eTypeVar);
            var t = TypeTree.MakePrimitiveTree(eTypeVar);

            // Unify (t0, t1->t) and get the resulting type t
            var t1_to_t = new TypeTree(t1, t, TypeConstructorEnum.Function);
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

        public static Dictionary<string, TypeTree> TypeInference(SExpression sexp, Namespace expressionNS) {
            // assign types to a ll variables and make all type variables unique across the system
            var namesToTypes = GetVarTypes(sexp, expressionNS);
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

            return namesToTypes;
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


        // ----- Apply and Eval -------------------------------
        public TermNew Eval(Namespace ns) {
            // loop over the right-hand branches from top-right to bottom-left
            TermNew currentTerm = this;
            var argList = new List<TermNew>();
            while(!currentTerm.IsLeaf()) {
                TermNew arg = currentTerm.right.Eval(ns);
                if (arg == null) throw new Exception("Something Eval()'d to null... this shouldn't happen!"); 
                else argList.Add(arg);
                currentTerm = currentTerm.left;
            }
            argList.Reverse(); // put left-most argument first in the list

            string functionSymbol = currentTerm.value;
            Variable functionObject = ns.VariableLookup(functionSymbol);
            TermNew result = functionObject?.Evaluate(argList);

            // null = not enough args to eval/collapse; or a ValueConstrutor
            //  just leave the TermNew as-is...
            if (result == null) result = this.DeepCopy();

            return result;
        }

        // ----- Get Variable Types ---------------------------
        public Dictionary<string, TypeTree> GetNamesToTypesDictionary(Dictionary<string, TypeTree> namesToTypes= null) {
            namesToTypes = namesToTypes ?? new Dictionary<string, TypeTree>();
            if (this.IsLeaf()) {
                if (!namesToTypes.ContainsKey(this.value)) namesToTypes.Add(this.value, this.typeTree);
            } else {
                this.left.GetNamesToTypesDictionary(namesToTypes);
                this.right.GetNamesToTypesDictionary(namesToTypes);
            }
            return namesToTypes;
        }


        // ----- Unification -------------------------------
        public static Dictionary<string, TermNew> UnifyAndSolve(TermNew t1, TermNew t2, Dictionary<string, TermNew> subs = null) {
            if (t1 == null || t2 == null) return null;
            subs = subs ?? new Dictionary<string, TermNew>();
            var varsToTypes = new Dictionary<string, TypeTree>();

            subs = UnifyOneDirection(t1, t2, new List<string>(), varsToTypes, subs);
            if (subs == null) return null;

            SolveMappings(subs);
            return subs;
        }

        // returns a dictionary "subs" such that t1.Substitute(subs) == t2.Substitute(subs)
        public static Dictionary<string, TermNew> UnifyOneDirection(TermNew t1, TermNew t2withVars, List<string> vars, Dictionary<string, TypeTree> varTypeLookup, Dictionary<string, TermNew> subs = null) {
            vars = vars ?? new List<string>();
            subs = subs ?? new Dictionary<string, TermNew>();

            if (!t1.IsLeaf() && !t2withVars.IsLeaf()) {
                var success1 = UnifyOneDirection(t1.left, t2withVars.left, vars, varTypeLookup, subs);
                if (success1 == null) return null;
                var success2 = UnifyOneDirection(t1.right, t2withVars.right, vars, varTypeLookup, subs);
                if (success2 == null) return null;
            } else if (t2withVars.IsLeaf() && vars.Contains(t2withVars.value)) {
                if (!subs.ContainsKey(t2withVars.value)) {
                    subs.Add(t2withVars.value, t1.DeepCopy());
                    varTypeLookup.Add(t2withVars.value, t1.typeTree);
                } else {
                    var success = UnifyOneDirection(subs[t2withVars.value], t1, vars, varTypeLookup, subs);   // If a type var matches two different subtrees, unify them
                    if (success == null) return null;
                }
            } else if (!t1.DeepEquals(t2withVars) && vars.Contains(t2withVars.value)) {
                Console.WriteLine("Error in Unify:");
                Console.WriteLine("Value " + t1 + " does not match " + t2withVars);
                return null;
            }

            return subs;
        }

        // if a: Bool, and b: a, we need to make sure that b: Bool as well
        // substitution should only be applied once to unify expressions
        public static void SolveMappings(Dictionary<string, TermNew> subs) {
            TermNew tempTree = new TermNew();
            foreach (var key in subs.Keys.ToList()) {
                tempTree = subs[key];
                foreach (var key2 in subs.Keys.ToList()) {
                    if (key == key2) continue;
                    subs[key2] = subs[key2].Substitute(key, tempTree);
                }
            }
            // remove loops
            foreach (var key in subs.Keys.ToList()) {
                if (subs[key].IsLeaf() && key == subs[key].value) {
                    subs.Remove(key);
                }
            }
        }

    }
}