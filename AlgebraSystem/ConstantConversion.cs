using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystemV4 {
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

        public override Term Evaluate(List<Term> argsTermList) {
            List<string> argsStringList = Parser.TermsToNames(argsTermList);
            return Evaluate(argsStringList);
        }

        public override Term Evaluate(List<string> args) {
            string resultString = this.conversion(args);
            if (resultString == "") return null;
            Term resultTerm = Term.MakePrimitiveTree(resultString, this.ns);
            return resultTerm;
        }



    }
}
