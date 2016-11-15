
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlgebraSystem;

namespace UnitTestProject1 {

    [TestClass]
    public class TermNewTest {

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

            var n0 = TermNew.TypeInference(sexp0, gns);
            var n1 = TermNew.TypeInference(sexp1, gns);
            var n2 = TermNew.TypeInference(sexp2, gns);
            var n3 = TermNew.TypeInference(sexp3, gns);

            Assert.Fail("example 3 doesn't work.");


            // Act & Assert

        }

    }
}
