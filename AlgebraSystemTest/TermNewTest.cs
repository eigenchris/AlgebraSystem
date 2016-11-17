
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlgebraSystem;

namespace UnitTestProject1 {

    [TestClass]
    public class TermNewTest {

        [TestMethod]
        public void TermNewTest_GetVarTypes() {
            // Arrange
            var gns = Namespace.CreateGlobalNs();

            string s0 = "f (g x) y";
            string s1 = "f (g x) (AND x x)";
            string s2 = "g (g (NOT q))";

            var sexp0 = Parser.ParseSExpression(s0);
            var sexp1 = Parser.ParseSExpression(s1);
            var sexp2 = Parser.ParseSExpression(s2);

            // Act
            var names0 = TermNew.GetVarTypes(sexp0, gns);
            var names1 = TermNew.GetVarTypes(sexp1, gns);
            var names2 = TermNew.GetVarTypes(sexp2, gns);

            // Assert
            Assert.AreEqual(4, names0.Count);
            Assert.AreEqual(4, names1.Count);
            Assert.AreEqual(3, names2.Count);

            Assert.IsTrue(names1["AND"].DeepEquals(gns.variableLookup["AND"].typeExpr.typeTree));
            Assert.IsTrue(names2["NOT"].DeepEquals(gns.variableLookup["NOT"].typeExpr.typeTree));
        }

        [TestMethod]
        public void TermNewTest_GetTypeEquations() {
            // Arrange
            var gns = Namespace.CreateGlobalNs();

            string s0 = "f (g x) y";
            string s1 = "f (g x) (AND x x)";
            string s2 = "g (g (NOT q))";

            var sexp0 = Parser.ParseSExpression(s0);
            var sexp1 = Parser.ParseSExpression(s1);
            var sexp2 = Parser.ParseSExpression(s2);

            // Act
            var types0 = TermNew.TypeInference(sexp0, gns);
            var types1 = TermNew.TypeInference(sexp1, gns);
            var types2 = TermNew.TypeInference(sexp2, gns);

            // Assert

        }


        [TestMethod]
        public void TermNewTest_TypeInference() {
            // Arrange
            var gns = Namespace.CreateGlobalNs();

            string s0 = "f (g x) y";
            string s1 = "f (AND x) (AND x y)";
            string s2 = "op (f x) (NOT y)";
            //string s3 = "op (AND x (NOT y)) (g (g (NOT q)))";
            string s3 = "g (g (NOT q))";

            var sexp0 = Parser.ParseSExpression(s0);
            var sexp1 = Parser.ParseSExpression(s1);
            var sexp2 = Parser.ParseSExpression(s2);
            var sexp3 = Parser.ParseSExpression(s3);

            // Act
            var n0 = TermNew.TypeInference(sexp0, gns);
            var n1 = TermNew.TypeInference(sexp1, gns);
            var n2 = TermNew.TypeInference(sexp2, gns);
            var n3 = TermNew.TypeInference(sexp3, gns);

            // Assert
            var BoolBool = Parser.ParseTypeTree("Bool -> Bool");
            Assert.IsTrue(n3.Item1["g"].DeepEquals(BoolBool));
        }

    }
}
