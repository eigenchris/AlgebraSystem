using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public class ConstantPrimitive : Constant {

        public ConstantPrimitive(string name, TypeExpr typeExpr, Namespace ns, string printString) :
            base(name, typeExpr, ns, printString) 
        {
            this.expectedNumberOfArgs = 0;
            this.compType = ComputationType.primative; // parent constructor gets called first, so this is okay
        }
        public ConstantPrimitive(string name, TypeExpr typeExpr, Namespace ns) :
            this(name, typeExpr, ns, name) { }
        public ConstantPrimitive(string name, string type, Namespace ns) : 
            this(name, new TypeExpr(type), ns, name) { }
        public ConstantPrimitive(string name, string type, Namespace ns, string printString) : 
            this (name, new TypeExpr(type), ns, printString)  { }

        public override ComputationType GetCompType() {
            return ComputationType.primative;
        }

        // Primative takes no arguments; it evaluates to itself
        public override TermNew Evaluate(List<TermNew> args) {
            return TermNew.MakePrimitiveTree(this.name, this.typeExpr.typeTree);
        }



    }
}
