using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public abstract class Constant : Variable {
        public string datum { get; } // the actual value of the variable
        public int expectedNumberOfArgs;

        public Constant(string name, TypeExpr typeExpr, Namespace ns, string printString) :
            base(name, typeExpr, ns, printString) { }
        public Constant(string name, TypeExpr typeExpr, Namespace ns) :
            this(name, typeExpr, ns, name) { }
        public Constant(string name, string type, Namespace ns) :
            this(name, new TypeExpr(type), ns, name) { }
        public Constant(string name, string type, Namespace ns, string printString) :
            this(name, new TypeExpr(type), ns, printString) { }

        public abstract override TermNew Evaluate(List<TermNew> args);

        public override bool IsVariabe() {
            return false;
        }

    }
}
