using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public static class Test {

        public static Namespace gns;


        public static void NeedsWork() {
            gns.AddConstantExpression("qq", "cmp AND NOT");
            Console.WriteLine(gns.VariableLookup("qq").typeExpr);
            Term t0 = null; // Node.NodeFromSExpression("apply (qq true) true", gns);
            //Node n0 = Node.NodeFromSExpression("qq true true", gns);
            Console.WriteLine(t0);
            t0.Eval();
            Console.WriteLine(t0);
        }

        public static void BooleanEvaluate() {
            Term tANDt = gns.VariableLookup("AND").Evaluate("true,true");
            Term tANDf = gns.VariableLookup("AND").Evaluate("true,false");
            Term tXORf = gns.VariableLookup("XOR").Evaluate("true,false");
            Term NOTt = gns.VariableLookup("NOT").Evaluate("true");

            Console.WriteLine("t AND t :\t" + tANDt);
            Console.WriteLine("t AND f :\t" + tANDf);
            Console.WriteLine("t XOR f :\t" + tXORf);
            Console.WriteLine("NOT t   :\t" + NOTt);
            Console.WriteLine();
        }

        public static void ExpressionTree() {
            string s1 = "f (AND x) (AND x y)";
            string s2 = "op (f x) (NOT y)";
            string s3 = "op (AND x (NOT y)) (g (g (NOT q)))";

            Term n1 = Term.TermFromSExpression(s1);
            Term n2 = Term.TermFromSExpression(s2);
            Term n3 = Term.TermFromSExpression(s3);

            gns.AddConstantExpression("homo2", s2, "op,f");
            Term t = Term.TermFromSExpression("homo2 OR NOT");
            Console.WriteLine(t);
            t.Eval();
            Console.WriteLine(t);

            Console.WriteLine();
        }

        public static void ExpressionTree2() {
            

            string sexpr = "NOT (AND x (f y (NOT x)))";
            gns.AddConstantExpression("bork", sexpr, "f,x,y");
            Term result = gns.variableLookup["bork"].Evaluate("AND,true,false");

            string sexpr3 = "cmp id id x";
            gns.AddConstantExpression("bork3", sexpr3, "x");
            Term result3 = gns.variableLookup["bork3"].Evaluate("true");


            string sexpr2 = "cmp cmp cmp";
            TermApply ta = TermApply.TermApplyFromSExpression(sexpr2);

            Console.WriteLine(ta.typeTree);
        }

        public static void PartialApplication() {
            gns.AddConstantExpression("notnot", "o NOT NOT");
            Term tt = null; // Node.NodeFromSExpression("notnot true", gns);
            Console.WriteLine(tt);
            tt.Eval();
            Console.WriteLine(tt);
            Console.WriteLine();

            gns.AddConstantExpression("homoAND", "homo AND");
            gns.AddConstantExpression("homoANDNOT", "homoAND NOT");
            Term t0 = null; // Node.NodeFromSExpression("homoANDNOT false false");
            Console.WriteLine(t0);
            t0.Eval();
            Console.WriteLine(t0);
            Console.WriteLine();
        }

        public static void SExpressionParser2() {
            string s1 = "((f x)) (((y)) (g z)) w";
            STree s = Parser.ParseSExpression(s1);
            Console.WriteLine(s);
            Console.WriteLine();
        }

        public static void TermTest() {
            Namespace ns = new Namespace(gns);
            ns.AddVariable("a,b", "Bool");
            ns.AddVariable("d", "(Bool->Bool)");
            ns.AddVariable("c", "(Bool->(a->Bool))");

            TermApply ta = new TermApply("a", ns);
            TermApply tb = new TermApply("b", ns);
            TermApply tc = new TermApply("c", ns);
            TermApply td = new TermApply("d", ns);

            TermApply tca = new TermApply(tc, ta);
            TermApply tdb = new TermApply(td, tb);
            TermApply termApply = new TermApply(tca, tdb);
            Console.WriteLine(termApply);

            Term term = termApply.ToTerm();
            Console.WriteLine(term);

            TermApply termApply2 = term.ToTermApply();
            Console.WriteLine(termApply2);


            ns.AddVariable("x,y", "Bool");
            ns.AddVariable("f", "(a->a)");
            ns.AddVariable("h", "(b->(a->b))");

            Term t = Term.MakePrimitiveTree("f", ns);
            t.Apply("x");

            TermApply tt = TermApply.MakePrimitiveTree("f", ns);
            tt.Apply("x");

            Term ss = tt.ToTerm();
            Console.WriteLine(ss);

            TermApply rr = ss.ToTermApply();
            Console.WriteLine(rr);

        }

        
        public static void TreeTransformations() {

            Term t1 = Term.TermFromSExpression("AND (f x) (f y)", gns);
            Term t2 = Term.TermFromSExpression("f (OR x y)", gns);

            TreeTransformation tt = new TreeTransformation(t1, t2);
            Term nBefore = Term.TermFromSExpression("AND (NOT true) (NOT false)", gns);
            Term nAfter = tt.TransformLeft(nBefore); // lots of unequal namespaces here...
            Term nAfterAfter = tt.TransformRight(nAfter);

            Console.WriteLine(nBefore);
            Console.WriteLine(nAfter);
            Console.WriteLine(nAfterAfter);
            Console.WriteLine();


            Term t11 = Term.TermFromSExpression("op1 z (op2 x y) ", gns);
            Term t22 = Term.TermFromSExpression("op2 (op1 z x) (op1 z y)", gns);
            TreeTransformation ss = new TreeTransformation(t11, t22);

            nBefore = Term.TermFromSExpression("AND true (OR false true)", gns);
            nAfter = ss.TransformLeft(nBefore);
            nAfterAfter = ss.TransformRight(nAfter);

            Console.WriteLine(nBefore);
            Console.WriteLine(nAfter);
            Console.WriteLine(nAfterAfter);
            Console.WriteLine();
        }

        public static void Substitution() {
            TypeTree t = new TypeTree("((x->y)  -   > x)");
            TypeTree s = new TypeTree("(a->b)");
            string x = "x";
            Console.WriteLine("Input tree:\t" + t);
            Console.WriteLine("Sub var:\t" + x);
            Console.WriteLine("Sub Tree:\t" + s);
            Console.WriteLine("-----");

            TypeTree r = t.Substitute("x", s);
            Console.WriteLine("Substitution:\t" + r);
            Console.WriteLine();

            t.value = "wiped1";
            s.value = "wiped2";
            Console.WriteLine("Input tree:\t" + t);
            Console.WriteLine("Sub Tree:\t" + s);
            Console.WriteLine("Subs after wiping: " + r);
            Console.WriteLine();
        }


    }
}
