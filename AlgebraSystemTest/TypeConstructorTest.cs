using AlgebraSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AlgebraSystemTest {

    [TestClass]
    public class TypeConstructorTest {

        [TestMethod]
        public void TypeConstructorTest_ParseAndInferenceTest() {
            // Arrange
            var gns = Namespace.CreateGlobalNs();

            string s1 = "RGB = Red / Green / Blue";
            string s2 = "List2 a = Cons a List2 / Nil";

            // Act
            bool succ1 = gns.AddTypeDefinition(s1);
            bool succ2 = gns.AddTypeDefinition(s2);

            // Assert
            Assert.IsTrue(succ1);
            Assert.IsFalse(succ2);
        }

        [TestMethod]
        public void TypeConstructorTest_EvaluateTest() {
            // Arrange
            var gns = Namespace.CreateGlobalNs();

            SExpression s1 = Parser.ParseSExpression("Cons 4 (Cons 2 Nil)");
            SExpression s2 = Parser.ParseSExpression("id (Cons 4 (Cons 2 Nil))");

            // Act
            TermNew t1 = TermNew.TermFromSExpression(s1, gns);
            TermNew t1e = t1.Eval(gns);
            TermNew t2 = TermNew.TermFromSExpression(s2, gns);
            TermNew t2e = t2.Eval(gns);

            // Assert
            Assert.IsTrue(t1.DeepEquals(t1e));
            Assert.IsTrue(t1.DeepEquals(t2e));
        }


    }
}
