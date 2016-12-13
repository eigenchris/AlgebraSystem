using System;
using System.Collections.Generic;

namespace AlgebraSystem {
    public class ValueConstructor : Constant {

        public ValueConstructor(string name, TypeExpr typeExpr, Namespace ns, string printString, int numArgs) :
            base(name, typeExpr, ns, printString) 
        {
            this.expectedNumberOfArgs = numArgs;
            this.compType = ComputationType.valueConstructor; // parent constructor gets called first, so this is okay
        }
        public ValueConstructor(string name, TypeExpr typeExpr, Namespace ns, int numArgs) :
            this(name, typeExpr, ns, name, numArgs) { }
        public ValueConstructor(string name, string type, Namespace ns, int numArgs) : 
            this(name, new TypeExpr(type), ns, name, numArgs) { }
        public ValueConstructor(string name, string type, Namespace ns, string printString, int numArgs) : 
            this (name, new TypeExpr(type), ns, printString, numArgs)  { }

        public override TermNew Evaluate(List<TermNew> args) {
            return null;
        }


        public static ValueConstructor ParseValueConstructor(string s, TypeTree resultTypeTree, Namespace ns, Dictionary<string,KindTree> knownKinds = null) {
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
            knownKinds = knownKinds ?? new Dictionary<string, KindTree>();
            if (Inference.KindChecking(vcType, ns, knownKinds) == null) return null;

            TypeExpr vcTypeExr = new TypeExpr(vcType, vcType.GetTypeVariables());
            return new ValueConstructor(vcName, vcTypeExr, ns, typeTreeList.Count-1);
        }

    }
}
