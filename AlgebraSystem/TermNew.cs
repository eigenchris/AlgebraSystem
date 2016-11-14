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


        // Given a parsed S-Expression, perform type inference and assign a type to every variable in the expression
        public static bool TypeInference(SExpression sexp, 
                                        Namespace ns, 
                                        Dictionary<string, TypeTree> variableTypes = null, 
                                        Dictionary<SExpression, TypeTree> expressionTypes = null, 
                                        List<string> introducedVars = null, 
                                        List<string> introducedTypeVars = null) 
        {
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

            // create a type variable (e.g. "e12") for the expression
            // add this to introducedTypeVars so we don't confuse it with any other type variables called "e12" we might bring in
            string eTypeVar = "e" + expressionTypes.Count;
            TypeTree thisExpressionType = new TypeTree(eTypeVar);
            expressionTypes.Add(sexp, thisExpressionType);
            introducedTypeVars.Add(eTypeVar);

            if (sexp.IsLeaf()) {
                string v = sexp.value;

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
                    TypeTree temp = ns.VariableLookup(sexp.value).typeExpr.typeTree;
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
                    expressionTypes[sexp] = variableTypes[v];
                    //$TODO: do I need to reach down more than one level here? don't seem like it
                }

            } else {
                TypeTree tt = new TypeTree(expressionTypes[sexp.GetRight()], expressionTypes[sexp], TypeConstructor.Function); // (e1->e2)
                TypeTree ss = expressionTypes[sexp.GetLeft()]; // actual type tree

                Dictionary<string, TypeTree> subs = TypeTree.UnifyAndSolve(tt, ss);
                if (subs == null) return false;
                expressionTypes[sexp] = expressionTypes[sexp].Substitute(subs);
                expressionTypes[sexp.GetLeft()] = expressionTypes[sexp.GetLeft()].Substitute(subs);
                expressionTypes[sexp.GetRight()] = expressionTypes[sexp.GetRight()].Substitute(subs);

                // update all variables; left and right aren't enough, a variable to othe far, far right might be affected
                foreach (var key in variableTypes.Keys.ToList()) {
                    variableTypes[key] = variableTypes[key].Substitute(subs);
                }
            }

            return true;
        }



    }
}