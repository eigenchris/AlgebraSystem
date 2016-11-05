using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    class SubsList : List<Tuple<string,TypeTree>> {
        
        public bool ContainsKey(string key) {
            foreach(var pair in this) {
                if (pair.Item1 == key) return true;
            }
            return false;
        }

        public void AddPair(string s, TypeTree t) {
            this.Add(new Tuple<string, TypeTree>(s, t));
        }
    }
}
