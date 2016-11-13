using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public class ConstantExpression : Constant {

        public Term expression;
        public List<string> boundVariables;

        private ConstantExpression(string name, TypeExpr typeExpr, Namespace ns, string printString, Term expression, List<string> inputVars) :
            base(name, typeExpr, ns, printString) {
            this.expression = expression;
            this.boundVariables = inputVars;
            this.expectedNumberOfArgs = inputVars.Count;
            this.compType = ComputationType.expression; // parent constructor gets called first, so this is okay
        }
        public ConstantExpression(string name, TypeExpr typeExpr, Namespace ns, Term expression, List<string> inputVars) :
            this(name, typeExpr, ns, name, expression, inputVars) { }

        public override ComputationType GetCompType() {
            return ComputationType.expression;
        }

        // include an ExpressionTree here
        public override Term Evaluate(List<Term> args) {
            if (args.Count != this.expectedNumberOfArgs) {
                return null; // number of args is not correct
            }

            // type variable checking
            Dictionary<string, TypeTree> typeSubs = new Dictionary<string, TypeTree>();
            for (int i = 0; i < boundVariables.Count; i++) {
                Variable expectedArg = this.expression.ns.VariableLookup(this.boundVariables[i]);
                typeSubs = TypeTree.UnifyAndSolve(args[i].typeTree, expectedArg.typeExpr.typeTree, typeSubs);
                if (typeSubs == null) {
                    Console.WriteLine("Could not apply variable " + args[i] + " :: " + args[i].typeTree);
                    Console.WriteLine("to variable slot " + expectedArg + " :: " + expectedArg.typeExpr);
                    return null;
                }
            }

            //copy the tree so we can substitude safely
            Dictionary<string, Term> varSubs = new Dictionary<string, Term>();
            for (int i = 0; i < boundVariables.Count; i++) {
                varSubs.Add(boundVariables[i], args[i]);
            }
            Term expressionCopy = expression.Substitute(varSubs);

            // collapse tree with Eval() and return result
            expressionCopy.Eval();

            return expressionCopy;
        }


    }
}
