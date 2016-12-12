using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public class TypeConstructor {
        public string name { get; private set; }

        public KindTree kindTree;
        public TypeTree resultTypeTree;

        private TypeConstructor(string name, KindTree kTree, TypeTree resultTypeTree) {
            this.name = name;
            this.kindTree = kTree;
            this.resultTypeTree = resultTypeTree;
        }

        // string format: "List a = Cons a (List a) / Nil"
        public static TypeConstructor ParseTypeConstructor(string s, Namespace ns) {
            if (string.IsNullOrEmpty(s)) throw new Exception("TypeConstructor string cannot be null or Empty");
            string typeConstructorString = s.Trim();

            // Get the resulting TypeTree            
            TypeTree resultTypeTree = Parser.ParseTypeTree(typeConstructorString);

            // Get Type Constructor name and args
            string[] typeConstructorParts = typeConstructorString.Split(' ');
            string tcName = typeConstructorParts[0];
            List<string> tcArgs = typeConstructorParts.Skip(1).ToList();

            if (tcName[0] != char.ToUpper(tcName[0])) throw new Exception("TypeConstructor must start with an upper case letter.");
            bool allLowerCase = tcArgs.Select(x => x[0] == char.ToLower(x[0])).Aggregate(true, (x, y) => x && y);
            if (!allLowerCase) throw new Exception("TypeConstructor args must start with a lower case letter.");

            // Get KindTree
            KindTree kTree = KindTree.MakeSimpleTreeOfLength(tcArgs.Count);

            return new TypeConstructor(tcName, kTree, resultTypeTree);
        }
    }
}
