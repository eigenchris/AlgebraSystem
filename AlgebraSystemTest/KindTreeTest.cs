
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlgebraSystem;
using System.Collections.Generic;

namespace UnitTestProject1 {

    [TestClass]
    public class KindTreeTest {

        [TestMethod]
        public void KindTreeTest_DeepEqualsTest() {
            // Arrange
            var t1 = Parser.ParseKindTree("*");
            var t2 = Parser.ParseKindTree("* -> *");
            var t3 = Parser.ParseKindTree("* -> * -> *");
            var t4 = Parser.ParseKindTree("(* -> *) -> *");

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