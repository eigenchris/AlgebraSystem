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
    }
}
