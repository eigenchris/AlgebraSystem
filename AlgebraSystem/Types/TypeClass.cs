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
        public Dictionary<string, string> methodDispatchLookup;

        public TypeClass (string name, Dictionary<string, KindTree> typeArgKinds, Dictionary<string, TypeTree> methodTypes) {
            this.name = name;
            this.typeArgKinds = typeArgKinds;
            this.methodTypes = methodTypes;
            this.methodDispatchLookup = new Dictionary<string, string>();
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


    }
}
