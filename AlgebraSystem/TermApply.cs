using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public class TermApply {

        public Namespace ns;
        public TypeTree typeTree;

        private TermApply left;
        private TermApply right;
        private string _value;
        // when we write to _value, kill the child nodes
        public string value
        {
            get { return _value; }
            set { _value = value; left = null; right = null; }
        }

        public TermApply(string name, Namespace ns) {
            this.value = name;
            this.typeTree = ns.VariableLookup(name).typeExpr.typeTree.DeepCopy();
            this.ns = ns;
        }
        public TermApply(TermApply l, TermApply r) {
            if(l.ns != r.ns) {
                Console.WriteLine("Namespaces from l and r TermApply trees do not agree");
            }
            this.ns = l.ns;
            if (l == null) {
                this.SetChildren(null, r.DeepCopy());
            } else {
                this.SetChildren(l.DeepCopy(), r.DeepCopy());
            }
        }
        public TermApply() {
            this.value = string.Empty;
        }
        public static TermApply MakePrimitiveTree(string s, Namespace ns) {
            TermApply temp = new TermApply();
            temp.value = s;
            temp.ns = ns;
            temp.typeTree = ns.VariableLookup(s).typeExpr.typeTree.DeepCopy();
            return temp;
        }

        // Getting is normal, but setting should set the string to empty
        public bool SetChildren(TermApply l, TermApply r) {
            if (l == null && r != null) {
                if (r.IsLeaf()) {
                    this.value = r.value;
                    return true;
                } else {
                    return this.SetChildren(r.left, r.right);
                }
            } else {
                TypeTree parentTree = l.typeTree.DeepCopy();
                TypeTree oldChildTree = r.typeTree.DeepCopy();
                TypeTree childTree = oldChildTree.MakeTypeVarsUnique(parentTree);

                // Unification (type matching)
                Dictionary<string, TypeTree> typeVarDictionary = TypeTree.UnifyAndSolve(parentTree.GetLeft(), childTree);
                if (typeVarDictionary == null) {
                    Console.WriteLine("Input type of '" + l + "': " + childTree + " does not match expected type of " + parentTree.GetLeft());
                    this.value = string.Empty;
                    return false;
                }
                parentTree = parentTree.Substitute(typeVarDictionary);

                 //sometimes we might pass "this" as "l", don't want to clear it!
                this.left = l.DeepCopy();
                this.right = r.DeepCopy();
                this._value = string.Empty;
                this.typeTree = parentTree.PopInput();

                return true;
            }
        }
        public TermApply GetLeft() { return this.left; }
        public TermApply GetRight() { return this.right; }

        // ----- Copying and Equals/Matching ------------------
        public TermApply DeepCopy() {
            // It might be better to use Apply() so we get the type inference for free...
            if (this.IsLeaf()) {
                TermApply ta = TermApply.MakePrimitiveTree(this.value,this.ns);
                return ta;
            } else {
                return new TermApply(this.left, this.right);
            }
        }
        public bool DeepEquals(TermApply t) {
            if (this.IsLeaf() ^ t.IsLeaf()) return false;
            if (this.IsLeaf()) return this.value == t.value;
            return this.left.DeepEquals(t.left) && this.right.DeepEquals(t.right);
        }

        public bool IsLeaf() {
            return this.left == null;
        }

        public TermApply Substitute(Dictionary<string, TermApply> subs = null, Namespace outputNS = null) {
            outputNS = outputNS ?? this.ns.DeepCopy(); // temporary solution to the substitution namespace problem

            //post-order transveral
            if(this.IsLeaf()) {
                if (subs != null && subs.ContainsKey(this.value)) {
                    TermApply ta = subs[this.value].DeepCopy();
                    ta.ns = outputNS; // $TODO this is a hack!!!
                    return ta;
                } else {
                    TermApply ta = TermApply.MakePrimitiveTree(this.value, outputNS);
                    return  ta;
                }
            } else {
                TermApply left = this.left.Substitute(subs, outputNS);
                TermApply right = this.right.Substitute(subs, outputNS);
                return new TermApply(left, right);
            }
        }


        // ----- Apply and Eval -------------------------------
        public bool Apply(string args) {
            List<string> argsList = Parser.CssToList(args);
            bool success = true;
            foreach (var arg in argsList) {
                if (!ns.ContainsVariable(arg)) {
                    Console.WriteLine("Cannot add child: lookup of '" + arg + "' failed!");
                    return false;
                }
                TermApply t = new TermApply(arg, this.ns);
                success = success && Apply(t);
                if (!success) { break; }
            }
            return success;
        }

        public bool Apply(TermApply childTerm) {
            // the type-checking and mechanical stuff is mostly implemented in SetChildren(), so this function is short

            // make sure that an input slot exists
            if (this.typeTree.IsLeaf()) {
                Console.WriteLine("Cannot add child: no argument slots remaining!");
                return false;
            }

            bool success = this.SetChildren(this, childTerm);
            return success;
        }


        // ----- Parsing and conversion To/From other datatypes ---------
        public override string ToString() {
            if (this.IsLeaf()) return this.value;

            TermApply currentTree = this;
            string childrenString = currentTree.right.ToString();
            while (!currentTree.left.IsLeaf()) {
                currentTree = currentTree.left;
                childrenString = currentTree.right + " " + childrenString; // pre-pend;                
            }
            childrenString = currentTree.left + " " + childrenString;

            return "(" + childrenString + ")";
        }

        public Term ToTerm() {
            if (this.IsLeaf()) return Term.MakePrimitiveTree(this.value, this.ns);

            Term term = this.left.ToTerm();
            term.children.Add(this.right.ToTerm());
            term.typeTree = this.typeTree;

            return term;
        }


        public static TermApply TermApplyFromSExpression(string sExpression, Namespace containerNS = null) {
            // Parse S-Expression
            STree sTree = new STree(sExpression);
            if (sTree == null) return null;

            // Perform type inference
            Dictionary<string, TypeTree> variableTypes = new Dictionary<string, TypeTree>();
            Dictionary<STree, TypeTree> expressionTypes = new Dictionary<STree, TypeTree>();
            List<string> introducedVars = new List<string>(); // Dictionary<string,bool> would be faster, I guess
            List<string> introducedTypeVars = new List<string>(); // Dictionary<string,bool> would be faster, I guess
            bool success = TypeInference(sTree, containerNS, variableTypes, expressionTypes, introducedVars, introducedTypeVars);
            if (!success) return null;


            // make type variables more pretty

            int charIdx = 97;
            Dictionary<string, string> prettify = new Dictionary<string, string>();
            foreach (var key in introducedTypeVars) {
                string newName = ((char)charIdx).ToString();
                if (!introducedTypeVars.Contains(newName)) {
                    prettify.Add(key, newName);
                }
                charIdx++;
            }
            foreach (var key in variableTypes.Keys) {
                variableTypes[key].ReplaceString(prettify);
            }


            // Create a namespace with all the inferred types
            // ns is where we can lookup variables from; expressionNS is the local expression NS where variables live
            Namespace expressionNS = new Namespace(containerNS);
            foreach (var var in introducedVars) {
                // Can't just do expressionNS.AddVariable here; that erases the particular type of evaluation constant to use
                // I'm re-adding certain variables here (like AND, true); not idea but it works... $TODO
                expressionNS.AddVariable(var, new TypeExpr(variableTypes[var]));
            }

            // Use STree and namespace to create expression node
            TermApply termApply = TermApplyFromSTree(sTree, expressionNS);

            return termApply;
        }

        // given an S-Tree and a namespace containing the types of all the variables in the S-Tree, make an expression
        public static TermApply TermApplyFromSTree(STree sTree, Namespace expressionNS) {
            if(sTree.IsLeaf()) {
                return TermApply.MakePrimitiveTree(sTree.value, expressionNS);
            } else {
                TermApply left = TermApplyFromSTree(sTree.GetLeft(), expressionNS);
                TermApply right = TermApplyFromSTree(sTree.GetRight(), expressionNS);
                left.Apply(right);
                return left;
            }
        }


        // Given a parsed S-Expression, perform type inference and assign a type to every variable in the expression
        public static bool TypeInference(STree sTree, Namespace ns, Dictionary<string, TypeTree> variableTypes = null, Dictionary<STree, TypeTree> expressionTypes = null, List<string> introducedVars = null, List<string> introducedTypeVars = null) {
            variableTypes = variableTypes ?? new Dictionary<string, TypeTree>();
            expressionTypes = expressionTypes ?? new Dictionary<STree, TypeTree>();
            introducedVars = introducedVars ?? new List<string>();
            introducedTypeVars = introducedTypeVars ?? new List<string>();

            // Do a post-order transversal (children first)
            if (!sTree.IsLeaf()) {
                bool success;
                success = TypeInference(sTree.GetLeft(), ns, variableTypes, expressionTypes, introducedVars, introducedTypeVars);
                if (!success) return false;
                success = TypeInference(sTree.GetRight(), ns, variableTypes, expressionTypes, introducedVars, introducedTypeVars);
                if (!success) return false;
            }

            // create a type variable (e.g. "e12") for the expression
            // add this to introducedTypeVars so we don't confuse it with any other type variables called "e12" we might bring in
            string eTypeVar = "e" + expressionTypes.Count;
            TypeTree thisExpressionType = new TypeTree(eTypeVar);
            expressionTypes.Add(sTree, thisExpressionType);
            introducedTypeVars.Add(eTypeVar);

            if(sTree.IsLeaf()) {
                string v = sTree.value;

                // if it's a previously unseen variable symbol, and it doesn't exist in the namespace,
                // all we know about it is the stuff we can infered from the child expression types
                if (!variableTypes.ContainsKey(v) && !ns.ContainsVariable(v)) {
                    variableTypes.Add(v, thisExpressionType);
                    introducedVars.Add(v);
                    return true; // nothing else meaningful we can do, so just return
                                 // t1 == t2 here, so unify(t1,t2) is meaningless
                }

                // if it's a previously unseen variable symbol, but it exists in the namespace, look it up
                if (!variableTypes.ContainsKey(v) && ns.ContainsVariable(v)) {
                    TypeTree temp = ns.VariableLookup(sTree.value).typeExpr.typeTree;
                    temp = temp.AddPrime(introducedTypeVars);
                    introducedTypeVars.AddRange(temp.GetTypeVariables());
                    variableTypes.Add(v, temp);
                } // don't "else"! we WANT the next "if" to happen afterward! Stuff in the namespace may have type variables!!

                // if it's a previously seen variable, get its type, and unify against t1 (child expression type info)
                // update variable type and child expression types, using substitutions from unify()
                // unify is necessary in cases like "NOT (NOT true)"; with "NOT :: a->a". 
                // the first application gives us (Bool->a) and the second gives us (Bool->Bool)
                if (variableTypes.ContainsKey(v)) {
                    Dictionary<string, TypeTree> subs = TypeTree.UnifyAndSolve(thisExpressionType, variableTypes[v]);
                    if (subs == null) return false;
                    variableTypes[v] = variableTypes[v].Substitute(subs);
                    expressionTypes[sTree] = variableTypes[v];
                    //$TODO: do I need to reach down more than one level here? don't seem like it
                }

            } else {
                TypeTree tt = new TypeTree(expressionTypes[sTree.GetRight()], expressionTypes[sTree]); // (e1->e2)
                TypeTree ss = expressionTypes[sTree.GetLeft()]; // actual type tree

                Dictionary<string, TypeTree> subs = TypeTree.UnifyAndSolve(tt, ss);
                if (subs == null) return false;                
                expressionTypes[sTree] = expressionTypes[sTree].Substitute(subs);
                expressionTypes[sTree.GetLeft()] = expressionTypes[sTree.GetLeft()].Substitute(subs);
                expressionTypes[sTree.GetRight()] = expressionTypes[sTree.GetRight()].Substitute(subs);

                // update all variables; left and right aren't enough, a variable to othe far, far right might be affected
                foreach(var key in variableTypes.Keys.ToList()) {
                    variableTypes[key] = variableTypes[key].Substitute(subs);
                }                
            }

            return true;
        }


        // ----- Unification --------------------------------------------
        public static Dictionary<string, TermApply> Unify(TermApply t1, TermApply t2, Dictionary<string, TermApply> subs = null) {
            subs = subs ?? new Dictionary<string, TermApply>();

            if (!t1.IsLeaf() && !t2.IsLeaf()) {
                var success1 = Unify(t1.GetLeft(), t2.GetLeft(), subs);
                if (success1 == null) return null;
                var success2 = Unify(t1.GetRight(), t2.GetRight(), subs);
                if (success2 == null) return null;
            } else if (t1.IsLeaf() && t1.ns.VariableLookup(t1.value).IsVariabe()) {
                if (!subs.ContainsKey(t1.value)) {
                    subs.Add(t1.value, t2.DeepCopy());
                } else if (t2.IsLeaf() && t1.value != t2.value) { // don't map a variable to itself
                    var success = Unify(subs[t1.value], t2, subs);   // If a type var matches two different subtrees, unify them
                    if (success == null) return null;
                }
            } else if (t2.IsLeaf() && t2.ns.VariableLookup(t2.value).IsVariabe()) {
                if (!subs.ContainsKey(t2.value)) {
                    subs.Add(t2.value, t1.DeepCopy());
                } else if (t1.IsLeaf() && t2.value != t1.value) { // don't map a variable to itself
                    var success = Unify(subs[t2.value], t1, subs);   // If a type var matches two different subtrees, unify them
                    if (success == null) return null;
                }
            } else if (!t1.DeepEquals(t2)) {
                Console.WriteLine("Error in Unify:");
                Console.WriteLine("Value " + t2.value + " maps to " + subs[t2.value] + " and does not match " + t1);
                //Console.WriteLine("Value " + t1 + " does not match " + t2);
                return null;
            }

            return subs;
        }

    }
}
