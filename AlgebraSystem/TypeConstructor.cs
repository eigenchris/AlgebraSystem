using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public class TypeConstructor {
        public string name { get; private set; }

        private List<string> args;
        private List<string> valueConstructors;
        public KindTree kindTree;
        private TypeTree resultTypeTree;

        private TypeConstructor(string name, List<string> args, List<string> vcNames, KindTree kTree, TypeTree resultTypeTree) {
            this.name = name;
            this.args = args;
            this.valueConstructors = vcNames;
            this.kindTree = kTree;
            this.resultTypeTree = resultTypeTree;
        }

        // string format: "List a = Cons a (List a) / Nil"
        public static TypeConstructor ParseTypeConstructor(string s, Namespace ns) {
            if (string.IsNullOrEmpty(s)) throw new Exception("TypeConstructor string cannot be null or Empty");
            string[] halves = s.Split('=');
            string typeConstructorString = halves[0].Trim();
            string valueConstructorString = null;
            if (halves.Length == 2) {
                valueConstructorString = halves[1];
            }
            if (halves.Length > 2) throw new Exception("TypeConstructor string must have at most one '=' character.");


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

            // Get Value constructor name and args
            var vcList = new List<ValueConstructor>();
            var vcNames = new List<string>();
            if (valueConstructorString != null) {
                List<string> valueConstructorStrings = valueConstructorString.Split('/').Select(x => x.Trim()).ToList();
                foreach (var vcString in valueConstructorStrings) {
                    var vc = ValueConstructor.ParseValueConstructor(vcString, resultTypeTree, ns);
                    if (vc == null) return null;
                    vcList.Add(vc);
                }
                vcNames = vcList.Select(x => x.name).ToList();
            }

            return new TypeConstructor(tcName, tcArgs, vcNames, kTree, resultTypeTree);
        }
    }
}
