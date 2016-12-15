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


            var implementation = new Dictionary<string, string>() {
                {"comboG", "+" },
                {"idG", "0" },
                {"inverseG", "neg" },
            };
            gns.TypeClassLookup(name).MakeInstance(implementation, gns);

            TermNew t = TermNew.TermFromSExpression("comboG (inverseG (comboG 4 5)) idG", gns);
            //TermNew r = t.Eval(gns);

            //Assert.AreEqual(r.value,"-9");
        }

    }
}
