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
            string s2 = "List a = Cons a (List a) / Nil";
            string s3 = "List a = Cons a List / Nil";
            string s4 = "| a b = Left a / Right b";
            string s5 = "-> a b";
            string s6 = ", a b = Pair a b";

            // Act
            TypeConstructor tc1 = TypeConstructor.ParseTypeConstructor(s1, gns);
            TypeConstructor tc2 = TypeConstructor.ParseTypeConstructor(s2, gns);
            TypeConstructor tc3 = TypeConstructor.ParseTypeConstructor(s3, gns);
            TypeConstructor tc4 = TypeConstructor.ParseTypeConstructor(s4, gns);
            TypeConstructor tc5 = TypeConstructor.ParseTypeConstructor(s5, gns);
            TypeConstructor tc6 = TypeConstructor.ParseTypeConstructor(s6, gns);

            // Assert
            Assert.IsNotNull(tc1);
            Assert.IsNotNull(tc2);
            Assert.IsNull(tc3);
            Assert.IsNotNull(tc4);
            Assert.IsNotNull(tc5);
            Assert.IsNotNull(tc6);
        }
    }
}
