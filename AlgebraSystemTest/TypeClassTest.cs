using AlgebraSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AlgebraSystemTest {

    [TestClass]
    public class TypeClassTest {

        [TestMethod]
        public void TypeClassTest_ParseTest() {
            // Arrange
            var gns = Namespace.CreateGlobalNs();

            string name = "Group";
            var typeArgs = new List<string>() { "g" };
            var methods = new Dictionary<string, string>() {
                {"comboG", "g->g->g" },
                {"idG", "g" },
                {"inverseG", "g->g" },
            };

            // Act
            bool success = gns.AddTypeClass(name, typeArgs, methods);

            // Assert
            Assert.IsTrue(success);
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
