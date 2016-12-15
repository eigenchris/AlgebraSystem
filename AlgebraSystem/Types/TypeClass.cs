using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {    

    public class TypeClass {
        public string name;
        public Dictionary<string, KindTree> typeArgKinds;
        public Dictionary<string, TypeTree> methodTypes;
        public Dictionary<string, List<string>> implementationsLookup;

        public TypeClass (string name, Dictionary<string, KindTree> typeArgKinds, Dictionary<string, TypeTree> methodTypes) {
            this.name = name;
            this.typeArgKinds = typeArgKinds;
            this.methodTypes = methodTypes;
            this.implementationsLookup = new Dictionary<string, List<string>>();
            foreach(var v in methodTypes.Keys) {
                this.implementationsLookup.Add(v, new List<string>());
            }
        }

        public static TypeClass ParseTypeClass(string name, List<string> typeArgs, Dictionary<string,string> methodTypeStrings, Namespace ns) {
            var methodTypes = new Dictionary<string, TypeTree>();
            var typeKinds = new Dictionary<string, KindTree>();

            // Parse TypeTrees and do kind-checking
            foreach (var methodName in methodTypeStrings.Keys) {
                TypeTree tTree = Parser.ParseTypeTree(methodTypeStrings[methodName]);
                if (tTree == null) return null;
                if(Inference.KindChecking(tTree, ns, typeKinds) == null) return null;
                methodTypes.Add(methodName, tTree);
            }

            // Assign the kinds to the type variables
            var typeArgKinds = new Dictionary<string, KindTree>();
            foreach (var typeVar in typeArgs) {
                if (!typeKinds.ContainsKey(typeVar)) return null;
                typeArgKinds.Add(typeVar, typeKinds[typeVar]);
            } 
            
            return new TypeClass(name, typeArgKinds, methodTypes);
        }

        public bool MakeInstance(Dictionary<string, string> implementationMethods, Namespace ns) {
            var methodTypes = new Dictionary<string, TypeTree>();
            var typeKinds = new Dictionary<string, KindTree>();

            // Parse TypeTrees and do kind-checking
            var implementationTypeArgs = new Dictionary<string, TypeTree>();
            foreach (var methodName in this.methodTypes.Keys) {
                if (!implementationMethods.ContainsKey(methodName)) return false;
                var result = TypeTree.UnifyAndSolve(
                    ns.VariableLookup(implementationMethods[methodName]).typeExpr.typeTree, 
                    this.methodTypes[methodName],
                    implementationTypeArgs);
                if (result == null) return false;              
            }

            // Ensure we have an implementation for each type, and assembly the typeKey
            string typeKey = "";
            foreach (var typeArg in this.typeArgKinds.Keys) {
                if (!implementationTypeArgs.ContainsKey(typeArg)) return false;
                typeKey += implementationTypeArgs[typeArg].ToString() + ";";
            }

            foreach(var key in implementationMethods.Keys) {
                var c = ns.VariableLookup(key) as ConstantOverloaded;
                if (c == null) return false;
                if (c.typeKeyToName.ContainsKey(typeKey)) return false;
                c.typeKeyToName.Add(typeKey, implementationMethods[key]);
            }            
            return true;
        }
    }
}
