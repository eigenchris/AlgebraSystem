using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystemV4 {
    public class GenericTree<T> {
        public T value;
        public List<GenericTree<T>> children;

        public GenericTree(T val) {
            this.value = val;
            this.children = new List<GenericTree<T>>();
        }

        public override string ToString() {
            string childrenString = "";
            foreach (var child in this.children) {
                childrenString += " " + child;
            }
            return "(" + this.value + childrenString + ")";
        }
   
    }
}
