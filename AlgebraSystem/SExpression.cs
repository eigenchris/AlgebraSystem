using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public class SExpression {
        public string value;
        public SExpression left;
        public SExpression right;

        public SExpression(string s) {
            this.value = s;
            this.left = null;
            this.right = null;
        }

        public SExpression(SExpression left, SExpression right) {
            this.value = null;
            this.left = left;
            this.right = right;
        }

        public override string ToString() {
            if (this.value != null) return this.value;
            if (this.left.value==null) {
                return "(" + this.left + ") " + this.right;
            }
            return this.left + " " + this.right;
        }

    }
}
