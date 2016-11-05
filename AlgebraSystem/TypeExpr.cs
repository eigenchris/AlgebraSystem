using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystemV4 {
    public class TypeExpr {
        public List<string> boundTypeVars;
        public TypeTree typeTree;

        public TypeExpr(TypeTree t, string boundTypeVars = "") {
            this.typeTree = t;
            this.boundTypeVars = Parser.CssToList(boundTypeVars);
        }
        public TypeExpr(string s, string boundTypeVars = "") : 
            this(new TypeTree(s), boundTypeVars) { }
        

        public TypeExpr() {
            this.boundTypeVars = new List<string>();
            this.typeTree = new TypeTree();
        }
        public static TypeExpr MakePrimitiveTree(string s) {
            TypeExpr te = new TypeExpr();
            te.typeTree = TypeTree.MakePrimitiveTree(s);
            return te;
        }

        public bool DeepEquals(TypeExpr te) {
            if (this.boundTypeVars.Count != te.boundTypeVars.Count) return false;
            for (int i = 0; i < this.boundTypeVars.Count; i++) {
                if (this.boundTypeVars[i] != te.boundTypeVars[i]) return false;
            }
            return this.typeTree.DeepEquals(te.typeTree);
        }
        public TypeExpr DeepCopy() {
            TypeExpr te = new TypeExpr();
            te.typeTree = this.typeTree.DeepCopy();
            te.boundTypeVars = new List<string>(this.boundTypeVars);
            return te;
        }

        public int GetNumberOfInputs() {
            return this.typeTree.GetNumberOfInputs();
        }



    }
}
