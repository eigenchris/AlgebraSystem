using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AlgebraSystem {
    public class TreeTransformation {
        private Term t1;
        private Term t2;
        public Namespace ns;

        public TreeTransformation(Term t1, Term t2) {
            // ensure variables match
            List<string> vars1 = t1.GetVariables();
            List<string> vars2 = t2.GetVariables();
            if (vars1.Count != vars2.Count) Console.WriteLine("Bad TreeTransformation construction!"); //FAIL
            foreach (var v in vars1) {
                if (!vars2.Contains(v)) Console.WriteLine("Bad TreeTransformation construction!"); //FAIL
            }

            this.t1 = t1;
            this.t2 = t2;
            this.ns = t1.ns;
            Term.ChangeNS(this.t2, this.ns); // this is extremely hacky
        }
        /*
        public Term Transform(Term t, Term tFrom, Term tTo) {
            Dictionary<string, Term> subs = Term.Unify(t, tFrom);
            if (subs == null) return null;

            // ensure types match;
            Dictionary<string, TypeTree> typeSubs = new Dictionary<string, TypeTree>();
            foreach (var key in subs.Keys) {
                typeSubs = TypeTree.Unify(this.ns.VariableLookup(key).typeTree, t.ns.VariableLookup(subs[key].value).typeTree, typeSubs);
                if (typeSubs == null) return null;
            }

            return tTo.Substitute(subs);
        }
        */
        public Term Transform(Term t, Term tFrom, Term tTo) {
            TermApply ta = Transform(t.ToTermApply(), tFrom.ToTermApply(), tTo.ToTermApply());                
            return ta.ToTerm();
        }
        public TermApply Transform(TermApply t, TermApply tFrom, TermApply tTo) {
            Dictionary<string, TermApply> subs = TermApply.Unify(t, tFrom);
            if (subs == null) return null;

            // ensure types match;
            Dictionary<string, TypeTree> typeSubs = new Dictionary<string, TypeTree>();
            foreach (var key in subs.Keys) {
                typeSubs = TypeTree.Unify(this.ns.VariableLookup(key).typeExpr.typeTree, t.ns.VariableLookup(subs[key].value).typeExpr.typeTree, typeSubs);
                if (typeSubs == null) return null;
            }

            // terms in "subs" have the namespace of the input term "t"
            // the "tToSubbed" term will have the same namespace as the input term "t"
            // the template terms "tTo" and "tFrom" should have a common namespace different from "t"
            TermApply tToSubbed = tTo.Substitute(subs);
            return tToSubbed;
        }


        public Term TransformLeft(Term t) {
            return Transform(t, t1, t2);
        }
        public Term TransformRight(Term t) {
            return Transform(t, t2, t1);
        }



         


    }
}