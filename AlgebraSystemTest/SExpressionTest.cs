
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlgebraSystem;

namespace UnitTestProject1 {

    [TestClass]
    public class SExpressionTest {

        [TestMethod]
        public void SExpressionTest_CheckBalancedParenTest() {
            // Arrange
            string success1 = "";
            string success2 = "((((()))))";
            string success3 = "()()()()()";
            string success4 = "(())(()(()))";

            string fail1 = ")";
            string fail2 = "()()(((";
            string fail3 = "()(()))";

            // Act & Assert
            Assert.IsTrue(SExpression.CheckBalancedParens(success1));
            Assert.IsTrue(SExpression.CheckBalancedParens(success2));
            Assert.IsTrue(SExpression.CheckBalancedParens(success3));
            Assert.IsTrue(SExpression.CheckBalancedParens(success4));

            Assert.IsFalse(SExpression.CheckBalancedParens(fail1));
            Assert.IsFalse(SExpression.CheckBalancedParens(fail2));
            Assert.IsFalse(SExpression.CheckBalancedParens(fail3));            
        }

        [TestMethod]
        public void SExpressionTest_GetIndexOfEndParenTest() {
            // Arrange
            string success1 = "((w+rds)))";
            int start1 = 2; int end1 = 7;
            string success2 = "w*rds(w%rds)words)  ";
            int start2 = 0; int end2 = 17;
            string success3 = "()(()))";
            int start3 = 0; int end3 = 6;

            string fail1 = "";
            string fail2 = "(";
            string fail3 = "()(()";

            // Act & Assert
            Assert.AreEqual(end1,SExpression.GetIndexOfEndParen(success1,start1));
            Assert.AreEqual(end2,SExpression.GetIndexOfEndParen(success2,start2));
            Assert.AreEqual(end3,SExpression.GetIndexOfEndParen(success3,start3));

            Assert.AreEqual(-1, SExpression.GetIndexOfEndParen(fail1));
            Assert.AreEqual(-1, SExpression.GetIndexOfEndParen(fail2));
            Assert.AreEqual(-1, SExpression.GetIndexOfEndParen(fail3));
        }

        [TestMethod]
        public void SExpressionTest_ParseSExpressionSuccessTest() {
            string[] testStrings = new string[] {
                "x",
                "  x  ",
                "  xyz  ",
                "f a b c",
                "((f) ((a) ((b) (c))))",
                " ( f  x )    ( g   y  )  z  "
            };

            foreach(var s in testStrings) {
                var sExp = SExpression.ParseSExpression(s);
            }
        }

        [TestMethod]
        public void SExpressionTest_TypeTreeSuccessTest() {
            string[] basicTestStrings = new string[] {
                "x",
                "  x  ",
                "  xyz  ",
                "f a b c",
                "((f) ((a) ((b) (c))))",
                " ( f  x )    ( g   y  )  z  ",
                "a123xyz456uvw",
            };

            string[] basicResultStrings = new string[] {
                "x",
                "x",
                "xyz",
                "f a b c",
                "f (a (b c))",
                "f x (g y) z",
                "a123xyz456uvw",
            };

            for (int i = 0; i < basicTestStrings.Length; i++) {
                var tTree = SExpression.ParseTypeTree(basicTestStrings[i]);
                Assert.AreEqual(basicResultStrings[i], tTree.ToString());
            }


            string[] treeTestStrings = new string[] {
                "a1 -> b1",
                "a1 | b1",
                "a1, b1",
                "(a1 -> b1) -> c1",
                "a1 -> b1 -> c1 -> d1 -> e1",
                "-> a1 b1",
                "a -> ((b,c) -> d | e)",
                "f a b c -> g x y z"
            };

            string[] treeResultStrings = new string[] {
                "a1 -> b1",
                "a1 | b1",
                "a1 , b1",
                "(a1 -> b1) -> c1",
                "a1 -> b1 -> c1 -> d1 -> e1",
                "a1 -> b1",
                "a -> (b , c) -> d | e",
                "f a b (c -> g x y z)",
            };

            for (int i=0; i<treeTestStrings.Length; i++) {
                var tTree = SExpression.ParseTypeTree(treeTestStrings[i]);
                Assert.AreEqual(treeResultStrings[i],tTree.ToString());
            }
        }

    }
}
