using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    class ConstantConversion : Constant {

        public ConversionFuncs.ConvertMethod conversion;

        private ConstantConversion(string name, TypeExpr typeExpr, Namespace ns, string printString, ConversionFuncs.ConvertMethod conversion) :
            base(name, typeExpr, ns, printString) {
            this.conversion = conversion;
            this.expectedNumberOfArgs = 2;
            this.compType = ComputationType.conversion; // parent constructor gets called first, so this is okay
        }

        public ConstantConversion(string name, TypeExpr typeExpr, Namespace ns, ConversionFuncs.ConvertMethod conversion) :
            this(name, typeExpr, ns, name, conversion) { }

        public override ComputationType GetCompType() {
            return ComputationType.conversion;
        }

        public override TermNew Evaluate(List<TermNew> argsTermList) {
            // only allow leaf trees for ConstantConversion evals
            List<string> argsStringList = Parser.TermsToNames(argsTermList);
            return Evaluate(argsStringList);
        }

        public override TermNew Evaluate(List<string> args) {
            string resultString = this.conversion(args);
            if (resultString == "") return null;
            TypeExpr typeExpr = this.ns.VariableLookup(resultString).typeExpr;
            TermNew resultTerm = TermNew.MakePrimitiveTree(resultString, typeExpr.typeTree);
            return resultTerm;
        }



    }
}
