using AlgebraSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AlgebraSystemTest {

    [TestClass]
    public class EvaluationTests {

        [TestMethod]
        public void ConstantLookup_BooleanEvaluate() {
            var gns = Namespace.CreateGlobalNs();

            TermNew tANDt = gns.VariableLookup("AND").Evaluate("true;true");
            TermNew tANDf = gns.VariableLookup("AND").Evaluate("true;false");
            TermNew tXORf = gns.VariableLookup("XOR").Evaluate("true;false");
            TermNew NOTt = gns.VariableLookup("NOT").Evaluate("true");

            Assert.AreEqual("true", tANDt.value);
            Assert.AreEqual("false", tANDf.value);
            Assert.AreEqual("true", tXORf.value);
            Assert.AreEqual("false", NOTt.value);
        }


        [TestMethod]
        public void ConstantLookup_BooleanTermEvals() {
            var gns = Namespace.CreateGlobalNs();

            string s0 = "true";
            string s1 = "NOT false";
            string s2 = "AND true true";
            string s3 = "OR true false";
            string s4 = "XOR true false";
            string s5 = "NOT (AND true (XOR (NOT true) false))";

            TermNew t0 = TermNew.TermFromSExpression(s0, gns);
            TermNew t1 = TermNew.TermFromSExpression(s1, gns);
            TermNew t2 = TermNew.TermFromSExpression(s2, gns);
            TermNew t3 = TermNew.TermFromSExpression(s3, gns);
            TermNew t4 = TermNew.TermFromSExpression(s4, gns);
            TermNew t5 = TermNew.TermFromSExpression(s5, gns);

            TermNew r0 = t0.Eval(gns);
            TermNew r1 = t1.Eval(gns);
            TermNew r2 = t2.Eval(gns);
            TermNew r3 = t3.Eval(gns);
            TermNew r4 = t4.Eval(gns);
            TermNew r5 = t5.Eval(gns);

            Assert.AreEqual("true", r0.value);
            Assert.AreEqual("true", r1.value);
            Assert.AreEqual("true", r2.value);
            Assert.AreEqual("true", r3.value);
            Assert.AreEqual("true", r4.value);
            Assert.AreEqual("true", r5.value);
        }

        [TestMethod]
        public void ConstantConversion_IntegerTermEvals() {
            var gns = Namespace.CreateGlobalNs();

            string s0 = "1";
            string s1 = "-1";
            string s2 = "neg 1";
            string s3 = "+ 4 -2";
            string s4 = "* 6 7";
            string s5 = "/ 12 4";

            TermNew t0 = TermNew.TermFromSExpression(s0, gns);
            TermNew t1 = TermNew.TermFromSExpression(s1, gns);
            TermNew t2 = TermNew.TermFromSExpression(s2, gns);
            TermNew t3 = TermNew.TermFromSExpression(s3, gns);
            TermNew t4 = TermNew.TermFromSExpression(s4, gns);
            TermNew t5 = TermNew.TermFromSExpression(s5, gns);

            TermNew r0 = t0.Eval(gns);
            TermNew r1 = t1.Eval(gns);
            TermNew r2 = t2.Eval(gns);
            TermNew r3 = t3.Eval(gns);
            TermNew r4 = t4.Eval(gns);
            TermNew r5 = t5.Eval(gns);

            Assert.AreEqual("1", r0.value);
            Assert.AreEqual("-1", r1.value);
            Assert.AreEqual("-1", r2.value);
            Assert.AreEqual("2", r3.value);
            Assert.AreEqual("42", r4.value);
            Assert.AreEqual("3", r5.value);
        }
    }
}
