using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    class TypeConstructor {
        public string name { get; private set; }

        private List<string> args;
        private List<string> valueConstructors;
        private KindTree kindTree;
        private TypeTree resultTypeTree;

        private TypeConstructor(string name, List<string> args, List<string> vcNames, KindTree kTree, TypeTree resultTypeTree) {
            this.name = name;
            this.args = args;
            this.valueConstructors = vcNames;
            this.kindTree = kTree;
            this.resultTypeTree = resultTypeTree;
        }

        // string format: "List a = Cons a (List a) / Nil"
        public static TypeConstructor ParseTypeConstructor(string s) {
            if (string.IsNullOrEmpty(s)) throw new Exception("TypeConstructor string cannot be null or Empty");
            string[] halves = s.Split('=');
            if (halves.Length != 2) throw new Exception("TypeConstructor string must have a '=' character.");

            // Get the resulting TypeTree
            string typeConstructorString = halves[0].Trim();
            TypeTree resultTypeTree = Parser.ParseTypeTree(typeConstructorString);

            // Get Type Constructor name and args
            string[] typeConstructorParts = typeConstructorString.Split(' ');
            string tcName = typeConstructorParts[0];
            List<string> tcArgs = typeConstructorParts.Skip(1).ToList();

            bool allLowerCase = tcArgs.Select(x => x[0] == char.ToLower(x[0])).Aggregate(true, (x, y) => x && y);
            if (!allLowerCase) throw new Exception("TypeConstructor args must start with a lower case letter.");

            // Get KindTree
            KindTree kTree = KindTree.MakeSimpleTreeOfLength(tcArgs.Count);

            // Get Value constructor name and args
            var vcNames = new List<string>();
            IEnumerable<string> valueConstructorStrings = halves[1].Split('/').Select(x => x.Trim());
            foreach(var vcString in valueConstructorStrings) {
                var vc = ValueConstructor.ParseValueConstructor(vcString, resultTypeTree);
                vcNames.Add(vc.name);
            }

            return new TypeConstructor(tcName, tcArgs, vcNames, kTree, resultTypeTree);
        }
    }
}
