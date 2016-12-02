using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public class ConstantExpression : Constant {

        public TermNew expression;
        public Namespace expressionNS;
        public List<string> boundVariables;

        private ConstantExpression(string name, TypeExpr typeExpr, Namespace ns, string printString, TermNew expression, List<string> inputVars) :
            base(name, typeExpr, ns, printString) {
            this.expression = expression;
            this.boundVariables = inputVars;
            this.expectedNumberOfArgs = inputVars.Count;
            this.compType = ComputationType.expression; // parent constructor gets called first, so this is okay
        }
        public ConstantExpression(string name, TypeExpr typeExpr, Namespace ns, TermNew expression, List<string> inputVars) :
            this(name, typeExpr, ns, name, expression, inputVars) { }

        public override ComputationType GetCompType() {
            return ComputationType.expression;
        }

        // include an ExpressionTree here
        public override TermNew Evaluate(List<TermNew> args) {
            if (args.Count != this.expectedNumberOfArgs) {
                return null; // number of args is not correct
            }

            // type variable checking
            Dictionary<string, TypeTree> typeSubs = new Dictionary<string, TypeTree>();
            for (int i = 0; i < boundVariables.Count; i++) {
                Variable expectedArg = this.ns.VariableLookup(this.boundVariables[i]);
                typeSubs = TypeTree.UnifyAndSolve(args[i].typeTree, expectedArg.typeExpr.typeTree, typeSubs);
                if (typeSubs == null) {
                    Console.WriteLine("Could not apply variable " + args[i] + " :: " + args[i].typeTree);
                    Console.WriteLine("to variable slot " + expectedArg + " :: " + expectedArg.typeExpr);
                    return null;
                }
            }

            //copy the tree so we can substitude safely
            var varSubs = new Dictionary<string, TermNew>();
            for (int i = 0; i < boundVariables.Count; i++) {
                varSubs.Add(boundVariables[i], args[i]);
            }
            TermNew expressionCopy = expression.Substitute(varSubs);

            // collapse tree with Eval() and return result
            //expressionCopy.Eval();

            return expressionCopy;
        }


    }
}
