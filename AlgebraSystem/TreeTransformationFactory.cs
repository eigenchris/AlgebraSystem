
using System.Collections.Generic;

namespace AlgebraSystem {
    public static class TreeTransformationFactory {

        public static TreeTransformation Make(TermNew tFrom, TermNew tTo, Namespace ns) {
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

    }
}
 