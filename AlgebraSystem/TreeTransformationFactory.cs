
using System.Collections.Generic;

namespace AlgebraSystem {
    public static class TreeTransformationFactory {

        public static TreeTransformation Make(TermNew tFrom, TermNew tTo, Namespace ns) {
            if (tFrom == null || tTo == null || ns == null) return null;
            Dictionary<string,TypeTree> tFromN2T = tFrom.GetNamesToTypesDictionary();
            Dictionary<string,TypeTree> tToN2T = tFrom.GetNamesToTypesDictionary();

            // we only care about variables, not constants
            var varsN2T = new Dictionary<string, TypeTree>();
            foreach(var k in tFromN2T.Keys) {
                if(ns.VariableLookup(k) == null) {
                    varsN2T.Add(k, tFromN2T[k]);
                }
            }
            
            // ensure the variable's types are alpha-equivalent (neither side can be stricter since it is bi-directional)
            foreach(var k in tToN2T.Keys) {
                if (ns.VariableLookup(k) == null) {
                    if (!varsN2T.ContainsKey(k)) return null;
                    if (!varsN2T[k].AlphaEquivalent(tToN2T[k])) return null;
                }
            }
            
            return new TreeTransformation(tFrom, tTo, varsN2T);
        }

        public static TreeTransformation MakeHomomorphism(string f, string op1, string op2, Namespace ns) {
            string sFrom = $"{op1} ({f} x) ({f} y)";
            string sTo = $"{f} ({op2} x y)";

            TermNew tFrom = TermNew.TermFromSExpression(sFrom, ns);
            TermNew tTo = TermNew.TermFromSExpression(sTo, ns);

            return TreeTransformationFactory.Make(tFrom, tTo, ns);
        }

    }
}
 