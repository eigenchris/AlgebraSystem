
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlgebraSystem;

namespace UnitTestProject1 {

    [TestClass]
    public class TypeTreeTest {

        [TestMethod]
        public void TypeTreeTest_DeepEqualsTest() {
            // Arrange
            var t1 = SExpression.ParseTypeTree("a -> (b , c) -> d");
            var t2 = SExpression.ParseTypeTree("a -> (b | c) -> d");
            var t3 = SExpression.ParseTypeTree("a -> (b , c) -> e");
            var t4 = SExpression.ParseTypeTree("a -> (b , c)");

            // Act & Assert
            Assert.IsTrue(t1.DeepEquals(t1));
            Assert.IsTrue(t2.DeepEquals(t2));
            Assert.IsTrue(t3.DeepEquals(t3));
            Assert.IsTrue(t4.DeepEquals(t4));

            Assert.IsFalse(t1.DeepEquals(t2));
            Assert.IsFalse(t1.DeepEquals(t3));
            Assert.IsFalse(t1.DeepEquals(t4));
            Assert.IsFalse(t2.DeepEquals(t3));
            Assert.IsFalse(t2.DeepEquals(t4));
            Assert.IsFalse(t3.DeepEquals(t4));
        }

        [TestMethod]
        public void TypeTreeTest_UnifyTest() {
            // Arrange
            var t1 = SExpression.ParseTypeTree("a -> (b , c) -> d");
            var t2 = SExpression.ParseTypeTree("a -> (b | c) -> d");
            var t3 = SExpression.ParseTypeTree("a -> (b , c) -> e");
            var t4 = SExpression.ParseTypeTree("a -> (b , c)");

            // Act & Assert
            Assert.IsTrue(t1.DeepEquals(t1));
            Assert.IsTrue(t2.DeepEquals(t2));
            Assert.IsTrue(t3.DeepEquals(t3));
            Assert.IsTrue(t4.DeepEquals(t4));

            Assert.IsFalse(t1.DeepEquals(t2));
            Assert.IsFalse(t1.DeepEquals(t3));
            Assert.IsFalse(t1.DeepEquals(t4));
            Assert.IsFalse(t2.DeepEquals(t3));
            Assert.IsFalse(t2.DeepEquals(t4));
            Assert.IsFalse(t3.DeepEquals(t4));
        }

    }
}