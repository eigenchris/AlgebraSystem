using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AlgebraSystem {
    public class TypeSet {
        public string name { get; }

        public List<string> constantMembers;
        public List<string> variableMembers;
        public List<Regex> constantRegex;

        // TypeSet constructor informs the namespace that it has been created
        public TypeSet(string name, Namespace ns) {
            if (ns.ContainsTypeLocal(name)) {
                ns.NameError(name);
            }
            this.name = name;
            this.constantMembers = new List<string>();
            this.variableMembers = new List<string>();
            this.constantRegex = new List<Regex>();
        }

        // from a TypeSet, create a list of values, and automatically inform the namespace

        // $TODO: this...
        public bool IsMember(string value) {
            return IsMemberRegex(value) || IsMemberLookup(value);
        }
        public bool IsMemberLookup(string value) {
            return constantMembers.Contains(value) || variableMembers.Contains(value);
        }
        public bool IsMemberRegex(string value) {
            bool success;
            foreach (var regex in this.constantRegex) {
                success = regex.IsMatch(value);
                if (success) return true;
            }
            return false;
        }

        // ----- Printing Methods ---------------------------------- //   
        public void PrintMembers() {
            Console.WriteLine("TypeSet:\t" + this.name);
            Console.Write("Constants:\t");
            foreach (var x in this.constantMembers) {
                Console.Write(x + ", ");
            }
            Console.Write("\nVariables:\t");
            foreach (var x in this.variableMembers) {
                Console.Write(x + ", ");
            }
            Console.WriteLine();
        }

    }
}
