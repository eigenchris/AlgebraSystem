﻿
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlgebraSystem;
using System.Collections.Generic;

namespace UnitTestProject1 {

    [TestClass]
    public class TypeTreeTest {

        [TestMethod]
        public void TypeTreeTest_DeepEqualsTest() {
            // Arrange
            var t1 = Parser.ParseTypeTree("a -> (b , c) -> d");
            var t2 = Parser.ParseTypeTree("a -> (b | c) -> d");
            var t3 = Parser.ParseTypeTree("a -> (b , c) -> e");
            var t4 = Parser.ParseTypeTree("a -> (b , c)");

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
            var t1 = Parser.ParseTypeTree("a -> (b , c) -> d");
            var t2 = Parser.ParseTypeTree("a -> (b | c) -> d");
            var t3 = Parser.ParseTypeTree("a -> (b , c) -> e");
            var t4 = Parser.ParseTypeTree("a -> (b , c)");

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
        public void TypeTreeTest_SolveMappings() {
            // Arrange
            var b = Parser.ParseTypeTree("b");
            var c = Parser.ParseTypeTree("c");
            var d = Parser.ParseTypeTree("d");
            var x = Parser.ParseTypeTree("x");
            var y = Parser.ParseTypeTree("y");
            var z = Parser.ParseTypeTree("z");
            var subs = new Dictionary<string, TypeTree>();
            subs.Add("a", b);
            subs.Add("b", c);
            subs.Add("c", d);
            subs.Add("x", y);
            subs.Add("y", z);
            subs.Add("z", x);

            // Act
            TypeTree.SolveMappings(subs);

            // Assert
            Assert.AreEqual(subs.Count, 5);
            Assert.AreEqual(subs["a"].value, "d");
            Assert.AreEqual(subs["b"].value, "d");
            Assert.AreEqual(subs["c"].value, "d");
            Assert.AreEqual(subs["x"].value, "z");
            Assert.AreEqual(subs["y"].value, "z");
        }

    }
}