using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public class Term {

        public Namespace ns;
        public TypeTree typeTree;
        public List<Term> children;

        public string value;

        public Term(string name, Namespace ns) {
            this.value = name;
            this.typeTree = ns.VariableLookup(name).typeExpr.typeTree.DeepCopy();
            this.ns = ns;
            this.children = new List<Term>();
        }
        public Term(List<Term> children, Namespace ns) {
            this.ns = ns;
            // to be completed
        }
        public Term(Namespace ns) {
            this.value = string.Empty;
            this.ns = ns;
            this.typeTree = null;
        }
        public static Term MakePrimitiveTree(string s, Namespace ns) {
            Term temp = new Term(ns);
            temp.value = s;
            temp.typeTree = ns.VariableLookup(s).typeExpr.typeTree.DeepCopy();
            temp.children = new List<Term>();
            return temp;
        }

        // this is a hacky solution
        public static void ChangeNS(Term t, Namespace ns) {
            t.ns = ns;
            foreach(var child in t.children) {
                ChangeNS(child, ns);
            }
        }

        // ----- Copying and Equals/Matching ------------------
        public Term DeepCopy() {
            if (this.IsLeaf()) {
                return Term.MakePrimitiveTree(this.value,this.ns);
            } else {
                Term parent = new Term(this.value,this.ns);
                foreach(var child in this.children) {
                    parent.children.Add(child.DeepCopy());
                }
                return parent;
            }
        }
        public bool DeepEquals(Term t) {
            if (this.value != t.value) return false;
            if (this.children.Count != t.children.Count) return false;
            bool success;
            for (int i = 0; i < this.children.Count; i++) {
                success = this.children[i].DeepEquals(t.children[i]);
                if (!success) return false;
            }
            return true;
        }


        public Term GetTopNode() {
            Term term = new Term(this.value, this.ns);
            return term;
        }

        public bool IsLeaf() {
            return this.children.Count == 0;
        }

        public Term Substitute(Dictionary<string, Term> subs = null) {
            //post-order transveral
            List<Term> newChildren = new List<Term>();
            for (int i = 0; i < this.children.Count; i++) {
                newChildren.Add(this.children[i].Substitute(subs));
            }

            // replace variable name if needed
            Term newParent;
            if (subs != null && subs.ContainsKey(this.value)) {
                newParent = subs[this.value].DeepCopy();
            } else {
                newParent = new Term(this.value, this.ns); ;
            }

            // apply the children to the parent
            foreach (var child in newChildren) {
                newParent.Apply(child);
            }
            return newParent;
        }

        // get a list of all variables in the tree, in a pre-traversal
        public List<string> GetVariables(List<string> varsList = null) {
            varsList = varsList ?? new List<string>();
            // lookup will always succeed if node was added properly
            if (this.ns.VariableLookup(this.value).GetCompType() == Variable.ComputationType.variable
                && !varsList.Contains(this.value)) {
                varsList.Add(this.value);
            }
            foreach (var child in this.children) {
                child.GetVariables(varsList); // pass  by reference should keep all relevant vars
            }
            return varsList;
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
                Term t = new Term(arg, this.ns);
                success = success && Apply(t);
                if (!success) { break; }
            }
            return success;
        }

        public bool Apply(Term childTerm) {
            // make sure that an input slot exists
            if (this.typeTree.IsPrimitive()) {
                Console.WriteLine("Cannot add child: no argument slots remaining!");
                return false;
            }

            // replace type variable names in the child tree as needed so that it shares no type vars with the parent
            TypeTree parentTree = this.typeTree.DeepCopy();
            TypeTree oldChildTree = childTerm.typeTree.DeepCopy();
            TypeTree childTree = oldChildTree.MakeTypeVarsUnique(parentTree);
            
            // Unification (type matching)
            Dictionary<string, TypeTree> typeVarDictionary = TypeTree.UnifyAndSolve(parentTree.GetLeft(), childTree);
            if (typeVarDictionary==null) {
                Console.WriteLine("Input type of '" + childTerm + "': " + childTree + " does not match expected type of " + parentTree.GetLeft());
                return false;
            }

            // substitute type trees for type variables as needed
            parentTree = parentTree.Substitute(typeVarDictionary);

            // Add child term to parent term, and update parent term's type
            this.children.Add(childTerm);
            this.typeTree = parentTree.GetRight();

            return true;
        }

        public void Eval() {
            // if function has enough arguments to evaluate, do so
            List<Term> args = new List<Term>();
            foreach (var child in this.children) {
                child.Eval();
                args.Add(child);
            }

            // do evaluation if all arguments are present; otherwise, do not
            Variable functionObj = this.ns.VariableLookup(this.value);
            Term result = functionObj.Evaluate(args);

            // nul means no evaluation takes place
            if (result != null) {
                this.value = result.value;
                this.typeTree = result.typeTree;
                this.ns = result.ns;
                this.children = result.children;
            }
            //}

        }

        // ----- Parsing and conversion To/From other datatypes ---------
        public override string ToString() {
            string currentString = "(" + this.value;

            foreach(var child in this.children) {
                if (child.IsLeaf()) {
                    currentString += " " + child.value;
                } else {
                    currentString += " " + child ;
                }
            }

            return currentString + ")";
        }

        public TermApply ToTermApply() {
            if (this.IsLeaf()) return TermApply.MakePrimitiveTree(this.value, this.ns);

            TermApply currentTermApply = TermApply.MakePrimitiveTree(this.value, this.ns);

            foreach(var child in this.children) {
                currentTermApply = new TermApply(currentTermApply,child.ToTermApply());
            }

            return currentTermApply;
        }

        public static Term TermFromSExpression(string s, Namespace containerNS = null) {
            TermApply t = TermApply.TermApplyFromSExpression(s, containerNS);
            if (t == null) return null;
            return t.ToTerm();
        }

        // ----- Unification --------------------------------------------
        public static Dictionary<string, Term> Unify(Term t1, Term t2, Dictionary<string, Term> subs = null) {
            subs = subs ?? new Dictionary<string, Term>();

            if (t2.ns.VariableLookup(t2.value).compType == Variable.ComputationType.variable) {
                if (subs.ContainsKey(t2.value)) {
                    if (!subs[t2.value].DeepEquals(t1.GetTopNode()) && !t2.DeepEquals(t1)) {
                        Console.WriteLine("Error in Unify:");
                        Console.WriteLine("Value " + t2.value + " maps to " + subs[t2.value] + " and does not match " + t1);
                        return null;
                    }
                } else {
                    //subs.Add(t2.value, t1.DeepCopy());
                    subs.Add(t2.value, t1.GetTopNode());
                }
            }

            if (!t1.IsLeaf() && !t2.IsLeaf()) {
                if (t1.children.Count != t2.children.Count) return null;
                for (int i = 0; i < t2.children.Count; i++) {
                    var success = Unify(t1.children[i], t2.children[i], subs);
                    if (success == null) return null;
                }
            }

            return subs;
        }
    }
}
