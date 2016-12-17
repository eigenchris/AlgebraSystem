
using System.Collections.Generic;
using System.Linq;

namespace AlgebraSystem {
    public class TreeTransformation {
        private TermNew t1;
        private TermNew t2;
        private Dictionary<string, TypeTree> varsToTypes;

        public TreeTransformation(TermNew t1, TermNew t2, Dictionary<string,TypeTree> varsToTypes = null) {
            this.t1 = t1;
            this.t2 = t2;
            this.varsToTypes = varsToTypes;
        }


        private TermNew Transform(TermNew t, TermNew tFrom, TermNew tTo) {
            TermNew tFromCopy = tFrom.MakeSymbolsUnique(t);

            var varsToTypesActual = new Dictionary<string, TypeTree>();
            Dictionary<string, TermNew> subs = TermNew.UnifyOneDirection(t, tFromCopy, this.varsToTypes?.Keys.ToList(), varsToTypesActual);
            if (subs == null) return null;

            // Ensure all the TreeTransformation variables have values to take on
            var varsToTypesCopy = new Dictionary<string, TypeTree>();
            foreach(var key in this.varsToTypes.Keys) {
                if (!subs.ContainsKey(key)) return null;
                varsToTypesCopy.Add(key, this.varsToTypes[key]);
            }

            // Ensure types of these variables match expected
            var typesDictionary = new Dictionary<string, TypeTree>();
            foreach(var key in this.varsToTypes.Keys) {
                typesDictionary = TypeTree.UnifyAndSolve(this.varsToTypes[key], varsToTypesActual[key], typesDictionary);
                if (typesDictionary == null) return null;
            }


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

        public override string ToString() {
            return this.t1 + " <=> " + this.t2;
        }

    }
}
 