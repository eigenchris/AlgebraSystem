using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    class ValueConstructor {
        public string name { get; private set; }
        private TypeTree typeTree;

        private ValueConstructor(string name, TypeTree typeTree) {
            this.name = name;
            this.typeTree = typeTree;
        }

        public static ValueConstructor ParseValueConstructor(string s, TypeTree resultTypeTree) {
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
                    int endIdx = Parser.GetIndexOfEndParen(s, idx);
                    int length = endIdx - idx + 1; //including both parens
                    typeTree = Parser.ParseTypeTree(s.Substring(idx + 1, length - 1));
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

            return new ValueConstructor(vcName, vcType);
        }
    }
}
