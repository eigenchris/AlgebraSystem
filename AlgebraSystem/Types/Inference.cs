
using System.Collections.Generic;

namespace AlgebraSystem {
    public static class Inference {

        // this all works by assuming that all type trees for filled TypeConstructors have kind *
        //      this is a bad assumption... see the section on kinds here:
        //      http://learnyouahaskell.com/making-our-own-types-and-typeclasses
        public static Dictionary<string, KindTree> KindChecking(TypeTree typeTree, Namespace ns, Dictionary<string, KindTree> typeKinds = null, KindTree assumedKind = null) {
            assumedKind = assumedKind ?? KindTree.MakePrimitiveTree("*");
            typeKinds = typeKinds ?? new Dictionary<string, KindTree>();

            // assume all inputs (right) have kind "*" and all operators have kind "* => current"
            if (!typeTree.IsLeaf()) {
                var succ1 = KindChecking(typeTree.left, ns, typeKinds, assumedKind.ExtendTree());
                if (succ1 == null) return null;
                var succ2 = KindChecking(typeTree.right, ns, typeKinds, KindTree.MakePrimitiveTree("*"));
                if (succ2 == null) return null;
            } else {
                string typeName = typeTree.value;

                KindTree k = ns.TypeConstructorLookup(typeName)?.kindTree;
                if (k != null) {
                    if (!k.DeepEquals(assumedKind)) return null;
                } else if (typeKinds.ContainsKey(typeName)) {
                    if (!typeKinds[typeName].DeepEquals(assumedKind)) return null;
                } else {
                    typeKinds.Add(typeName, assumedKind);
                }
            }
            return typeKinds;
        }



    }
}
