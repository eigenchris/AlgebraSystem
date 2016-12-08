
using System.Collections.Generic;

namespace AlgebraSystem {
    public class TreeTransformation {
        private TermNew t1;
        private TermNew t2;

        public TreeTransformation(TermNew t1, TermNew t2) {
            this.t1 = t1;
            this.t2 = t2;
        }

        private TermNew Transform(TermNew t, TermNew tFrom, TermNew tTo) {
            TermNew tFromCopy = tFrom.MakeSymbolsUnique(t);

            Dictionary<string, TermNew> subs = TermNew.UnifyOneDirection(t, tFromCopy);
            if (subs == null) return null;

            // no type-checking is done here... this should be done by the transformation factory

            // terms in "subs" have the namespace of the input term "t"
            // the "tToSubbed" term will have the same namespace as the input term "t"
            // the template terms "tTo" and "tFrom" should have a common namespace different from "t"
            TermNew tToSubbed = tTo.Substitute(subs);
            return tToSubbed;
        }


        public TermNew TransformLeft(TermNew t) {
            return Transform(t, t1, t2);
        }
        public TermNew TransformRight(TermNew t) {
            return Transform(t, t2, t1);
        }

        /*
        
        Dictionary<string, TypeTree> typeSubs = new Dictionary<string, TypeTree>();
        foreach (var key in subs.Keys) {
            typeSubs = TypeTree.Unify(this.ns.VariableLookup(key).typeExpr.typeTree, t.ns.VariableLookup(subs[key].value).typeExpr.typeTree, typeSubs);
            if (typeSubs == null) return null;
        }
        */

         


    }
}
 