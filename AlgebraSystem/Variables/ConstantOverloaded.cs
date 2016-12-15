using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public class ConstantOverloaded : Constant {

        // This dictionary maps "typeKeys" to the name of the actual "dispatched" function we are going to Evaluate()
        //  For example. "Vect s v where +:v->v->v *:s->v->v" has two types, s and v.
        //  If we implement this with s=Float and v=(Float,Float), then the typeKey is "Float;(Float,Float)"
        //  (we just separate the types with semi-colons)
        public Dictionary<string, string> typeKeyToName;

        public ConstantOverloaded(string name, TypeExpr typeExpr, Namespace ns, string printString) :
            base(name, typeExpr, ns, printString) 
        {
            this.expectedNumberOfArgs = this.typeExpr.typeTree.GetNumberOfInputs();
            this.compType = ComputationType.overloaded; // parent constructor gets called first, so this is okay
            this.typeKeyToName = new Dictionary<string, string>();
        }
        public ConstantOverloaded(string name, TypeExpr typeExpr, Namespace ns) :
            this(name, typeExpr, ns, name) { }
        public ConstantOverloaded(string name, string type, Namespace ns) : 
            this(name, new TypeExpr(type), ns, name) { }
        public ConstantOverloaded(string name, string type, Namespace ns, string printString) : 
            this (name, new TypeExpr(type), ns, printString)  { }

        public override TermNew Evaluate(List<TermNew> args) {
            string[] types = args.Select(a => a.typeTree.ToString()).ToArray();
            string typeKey = string.Join(";", types);
            if (!this.typeKeyToName.ContainsKey(typeKey)) return null;

            string dispatchName = this.typeKeyToName[typeKey];
            Variable f = this.ns.VariableLookup(dispatchName);
            return f?.Evaluate(args);                
        }
    }
}
