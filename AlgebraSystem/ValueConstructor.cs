using System;
using System.Collections.Generic;

namespace AlgebraSystem {
    public class ValueConstructor {
        public string name { get; private set; }
        private TypeTree typeTree;

        private ValueConstructor(string name, TypeTree typeTree) {
            this.name = name;
            this.typeTree = typeTree;
        }

        public static ValueConstructor ParseValueConstructor(string s, TypeTree resultTypeTree, Namespace ns) {
            if (string.IsNullOrEmpty(s)) throw new Exception("ValueConstructor string cannot be null or Empty");
            if (s[0] != char.ToUpper(s[0])) throw new Exception("ValueConstructor must start with a capital letter.");
            s = s.Trim();
            int nameLength = Parser.Identifier(s, 0);
            string vcName = s.Substring(0, nameLength);

            int idx = nameLength + 1;
            var typeTreeList = new List<TypeTree>();
            while (idx < s.Length) {
                TypeTree typeTree = null;
                idx += Parser.Spaces(s, idx);
                if (s[idx] == '(') {
                    int endIdx = Parser.GetIndexOfEndParen(s, idx+1);
                    int length = endIdx - (idx + 1); //including both parens
                    string treeString = s.Substring(idx + 1, length);
                    typeTree = Parser.ParseTypeTree(treeString);
                    idx = endIdx + 1;
                } else {
                    int length = Parser.Identifier(s, idx);
                    string identifier = s.Substring(idx, length);
                    typeTree = TypeTree.MakePrimitiveTree(identifier);
                    idx += length;
                }
                typeTreeList.Add(typeTree);
            }
            typeTreeList.Add(resultTypeTree);

            TypeTree vcType = TypeTree.TypeTreeFromTreeList(typeTreeList);
            // Kind Checking...
            if (KindChecking(vcType, ns) == null) return null;

            return new ValueConstructor(vcName, vcType);
        }

        // this all works by assuming that all type trees for filled TypeConstructors have kind *
        public static Dictionary<string, KindTree> KindChecking(TypeTree typeTree, Namespace ns, KindTree assumedKind = null, Dictionary<string,KindTree> typeKinds = null) {
            assumedKind = assumedKind ?? KindTree.MakePrimitiveTree("*");
            typeKinds = typeKinds ?? new Dictionary<string, KindTree>();

            // assume all inputs (right) have kind "*" and all operators have kind "* => current"
            if(!typeTree.IsLeaf()) {
                var succ1 = KindChecking(typeTree.left, ns, assumedKind.ExtendTree(), typeKinds);
                if (succ1 == null) return null;
                var succ2 = KindChecking(typeTree.right, ns, KindTree.MakePrimitiveTree("*"), typeKinds);
                if (succ2 == null) return null;
            } else {
                string typeName = typeTree.value;

                KindTree k = ns.TypeConstructorLookup(typeName)?.kindTree;
                if(k!=null) {
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
