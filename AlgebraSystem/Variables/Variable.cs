using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public class Variable {
        public enum ComputationType { variable, primative, conversion, lookup, expression, overloaded, valueConstructor };

        public string name { get; private set; }
        public TypeExpr typeExpr { get; }
        public Namespace ns { get; }
        //public TypeTree typeTree {
            //get { return this.typeExpr.typeTree; }
            //set { this.typeExpr.typeTree = value; }
        //}

        public string printString { get; }
        public int numberOfInputs;

        public ComputationType compType;

        public Variable(string name, TypeExpr typeExpr, Namespace ns, string printString) {
            if (ns.ContainsVariableLocal(name)) {
                this.ns.NameError(name);
            }
            this.name = name;
            this.ns = ns;
            this.printString = printString;
            this.typeExpr = typeExpr;

            this.compType = ComputationType.variable;
            this.numberOfInputs = this.typeExpr.GetNumberOfInputs();
        }

        public virtual bool IsVariabe() {
            return true;
        }

        public virtual ComputationType GetCompType() {
            return ComputationType.variable;
        }

        // A variable does *NOT* evaluate to itself (a variable function keeps its children!!)
        // Do not evaluate  at all, i.e., return the empty string
        public virtual TermNew Evaluate(List<TermNew> args) {
            return null;
        }
        public virtual TermNew Evaluate(List<string> args) {
            //List<Term> t = Parser.NamesTSoTerms(args, this.ns);
            List<TermNew> t = new List<TermNew>();
            foreach(var s in args) { // parse each string as an S-expression
                t.Add(TermNew.TermFromSExpression(s, this.ns));
            }
            return Evaluate(t);
        }
        // there aren't any arguments anyway.. so zero arguments is allowed
        public TermNew Evaluate() {
            // this will call Evaluate() in child classes too, if we call from a Constant instead
            return this.Evaluate(new List<string>());
        }
        public TermNew Evaluate(string args) {
            List<string> argsList = Parser.ScsvToList(args);
            return this.Evaluate(argsList);
        }
    }
}
