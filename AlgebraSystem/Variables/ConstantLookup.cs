using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public class ConstantLookup : Constant {

        public Dictionary<string, string> lookup;

        private ConstantLookup(string name, TypeExpr typeExpr, Namespace ns, string printString, Dictionary<string, string> lookup) :
            base(name, typeExpr, ns, printString) {
            this.lookup = lookup;
            this.expectedNumberOfArgs = Parser.CssToList(lookup.Keys.First()).Count;
            this.compType = ComputationType.lookup; // parent constructor gets called first, so this is okay
        }

        public ConstantLookup(string name, TypeExpr typeExpr, Namespace ns, Dictionary<string, string> lookup) :
            this(name, typeExpr, ns, name, lookup) { }
        public ConstantLookup(string name, Namespace ns, Dictionary<string, string> lookup) :
            this(name, ConstantLookup.ValidateDictionary(lookup, ns), ns, name, lookup) { }
        public ConstantLookup(string name, Namespace ns, string printString, Dictionary<string, string> lookup) :
            this(name, ConstantLookup.ValidateDictionary(lookup, ns), ns, printString, lookup) { }

        public override ComputationType GetCompType() {
            return ComputationType.lookup;
        }

        public override TermNew Evaluate(List<TermNew> argsTermList) {
            if (argsTermList.Count != this.expectedNumberOfArgs) {
                return null; // number of args is not correct
            }

            // this will allow non-primative Terms to be added to the dictionary
            var argsStringList = argsTermList.Select(t => t.ToString()) as List<string>;

            string argsString = string.Join(",", argsStringList.ToArray());
            if (!this.lookup.ContainsKey(argsString)) {
                // if it's not in the lookup, it's not a failure; we just don't evaluate and leave the expression as-is
                //Console.WriteLine("Evaluation failed!\nInput set '"+argsString+"' isn't in the lookup.");
                return null;
            } else {
                string evalResult = this.lookup[argsString];
                TypeExpr typeExpr = ns.VariableLookup(evalResult).typeExpr.DeepCopy();
                return TermNew.MakePrimitiveTree(evalResult, typeExpr.typeTree);
            }
        }

        public override TermNew Evaluate(List<string> args) {
            string argsString = string.Join(",", args.ToArray());
            if (!this.lookup.ContainsKey(argsString)) {
                // if it's not in the lookup, it's not a failure; we just don't evaluate and leave the expression as-is
                //Console.WriteLine("Evaluation failed!\nInput set '"+argsString+"' isn't in the lookup.");
                return null;
            } else {
                string evalResult = this.lookup[argsString];
                TypeExpr typeExpr = ns.VariableLookup(evalResult).typeExpr.DeepCopy();
                return TermNew.MakePrimitiveTree(evalResult, typeExpr.typeTree);
            }
        }

        // confirm everything in the dictionary matches the same type signature
        public static TypeExpr ValidateDictionary(Dictionary<string, string> d, Namespace ns) {
            string keyString = d.Keys.First();
            List<string> keyList = new List<string>();

            // validate inputs/keys 
            List<TypeTree> typeTrees = new List<TypeTree>();
            foreach (var key in d.Keys) {
                keyList = Parser.CssToList(key);
                for (int i = 0; i < keyList.Count; i++) {
                    string variable = keyList[i];
                    if (!ns.variableLookup.ContainsKey(variable)) {
                        Console.WriteLine("Invalid dictionary! Variable " + variable + " does not exist in the namespace!");
                        return null;
                    }
                    if (typeTrees.Count != keyList.Count) {
                        typeTrees.Add(ns.VariableLookup(variable).typeExpr.typeTree);
                    } else {
                        if (!ns.VariableLookup(variable).typeExpr.typeTree.DeepEquals(typeTrees[i])) {
                            Console.WriteLine("Invalid dictionary! Type mismatch!");
                            return null;
                        }
                    }
                }

            }

            // validate outputs; return the TypeTree of the function's type signature
            TypeTree outputTree = null;
            foreach (var output in d.Values) {
                if (!ns.variableLookup.ContainsKey(output)) {
                    Console.WriteLine("Invalid dictionary! Variable " + output + " does not exist in the namespace!");
                    return null;
                }
                if (outputTree == null) {
                    outputTree = ns.VariableLookup(output).typeExpr.typeTree;
                } else {
                    if (!ns.VariableLookup(output).typeExpr.typeTree.DeepEquals(outputTree)) {
                        Console.WriteLine("Invalid dictionary! Type mismatch!");
                        return null;
                    }
                }
            }

            typeTrees.Add(outputTree);
            TypeTree typeTree = TypeTree.TypeTreeFromTreeList(typeTrees);
            return new TypeExpr(typeTree);
        }

    }
}
