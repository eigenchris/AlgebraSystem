﻿
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlgebraSystem;
using System.Collections.Generic;

namespace UnitTestProject1 {

    [TestClass]
    public class TypeExprTest {

        [TestMethod]
        public void TypeExprTest_ParseCorrectly() {
            // Arrange
            var t1 = Parser.ParseTypeExpr("a,b.a->b->a");
            var t2 = Parser.ParseTypeExpr("a.a->b->a");
            var t3 = Parser.ParseTypeExpr("a->b->a");
            var t4 = Parser.ParseTypeExpr(".a");

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
        public void TypeExprTest_UnifyTestSuccesses() {
            // Arrange
            var ATree = TypeTree.MakePrimitiveTree("A");
            var BTree = TypeTree.MakePrimitiveTree("B");
            var ABATree = Parser.ParseTypeTree("A->B->A");
            var AATree = Parser.ParseTypeTree("A->A");
            var AaTree = Parser.ParseTypeTree("(A,a)");

            var t1 = Parser.ParseTypeExpr("A->B->A");
            var subs1 = new Dictionary<string, TypeTree>();

            var t2 = Parser.ParseTypeExpr("x.A->B->x");
            var subs2 = new Dictionary<string,TypeTree>();
            subs2.Add("x", ATree);

            var t3 = Parser.ParseTypeExpr("x.x");
            var subs3 = new Dictionary<string, TypeTree>();
            subs3.Add("x", ABATree);

            var t4a = Parser.ParseTypeExpr("x,y.x->y->x");
            var t4b = Parser.ParseTypeExpr("a.(A->A)->a->A->A");
            var subs4 = new Dictionary<string, TypeTree>();
            subs4.Add("x", AATree);
            subs4.Add("y", TypeTree.MakePrimitiveTree("z"));

            var t5a = Parser.ParseTypeExpr("x.x,x");
            var t5b = Parser.ParseTypeExpr("a,b.a,b");
            var subs5 = new Dictionary<string, TypeTree>();
        
            var t6a = Parser.ParseTypeExpr("x.x->x");
            var t6b = Parser.ParseTypeExpr("a,b.(A,a)->(A,b)");            
            var subs6 = new Dictionary<string, TypeTree>();

            var t7a = Parser.ParseTypeExpr("a,b.(A,a) | a | (A,b)");
            var t7b = Parser.ParseTypeExpr("x,y.x|y|x");
            var subs7 = new Dictionary<string, TypeTree>();

            // Act & Assert
            var result1 = TypeExpr.Unify(t1, t1);
            Assert.AreEqual(result1.Count, 0);

            var result2 = TypeExpr.Unify(t1, t2);
            Assert.AreEqual(result2.Count, 1);
            Assert.IsTrue(result2["x"].DeepEquals(ATree));

            var result4 = TypeExpr.Unify(t4a, t4b);
            Assert.AreEqual(result4.Count, 2);
            Assert.IsTrue(result4["x"].DeepEquals(AATree));
            Assert.IsTrue(result4["y"].DeepEquals(TypeTree.MakePrimitiveTree("a")));

            var result5 = TypeExpr.Unify(t5a, t5b);
            Assert.AreEqual(result5.Count, 2);
            Assert.IsTrue(result5["x"].DeepEquals(TypeTree.MakePrimitiveTree("a")));
            Assert.IsTrue(result5["a"].DeepEquals(TypeTree.MakePrimitiveTree("b")));

            var result6 = TypeExpr.Unify(t6a, t6b);
            Assert.AreEqual(result6.Count, 2);
            Assert.IsTrue(result6["x"].DeepEquals(AaTree));
            Assert.IsTrue(result6["a"].DeepEquals(TypeTree.MakePrimitiveTree("b")));

            var result7 = TypeExpr.Unify(t7a, t7b);
            Assert.AreEqual(result7.Count, 3);
            Assert.IsTrue(result7["x"].DeepEquals(AaTree));
            Assert.IsTrue(result7["a"].DeepEquals(TypeTree.MakePrimitiveTree("y")));
            Assert.IsTrue(result7["b"].DeepEquals(TypeTree.MakePrimitiveTree("y")));
        }

        [TestMethod]
        public void TypeExprTest_UnifySameVars() {
            // Arrange
            var t1a = Parser.ParseTypeExpr("x.A->x");
            var t1b = Parser.ParseTypeExpr("x.x->B");

            // Act & Assert
            var result1 = TypeExpr.Unify(t1a, t1b);
            Assert.AreEqual(result1.Count,2);
            Assert.IsTrue(result1["x"].DeepEquals(TypeTree.MakePrimitiveTree("B")));
            Assert.IsTrue(result1["x'"].DeepEquals(TypeTree.MakePrimitiveTree("A")));
        }



        [TestMethod]
        public void TypeExprTest_UnifyTestFailures() {
            // Arrange
            var t1 = Parser.ParseTypeExpr("A->B->A");
            var t1c = Parser.ParseTypeExpr("A->B->C");
            var t2 = Parser.ParseTypeExpr("x.A->x->x");
            var t3a = Parser.ParseTypeExpr("x.A->x->x");
            var t3b = Parser.ParseTypeExpr("y.y->B->y");


            // Act & Assert
            var result1 = TypeExpr.Unify(t1, t1c);
            Assert.IsNull(result1);

            var result2 = TypeExpr.Unify(t1, t2);
            Assert.IsNull(result2);

            var result3 = TypeExpr.Unify(t3a, t3b);
            Assert.IsNull(result3);
        }


        [TestMethod]
        public void TypeExprTest_UnifyAndSolve() {
            // Arrange
            var t1 = Parser.ParseTypeExpr("A->B->A");
            var t2 = Parser.ParseTypeExpr("x.A->B->x");
            var t3 = Parser.ParseTypeExpr("x.x");
            var t4a = Parser.ParseTypeExpr("x,y.x->y->x");
            var t4b = Parser.ParseTypeExpr("a.(A->A)->a->A->A");
            var t5a = Parser.ParseTypeExpr("x.x,x");
            var t5b = Parser.ParseTypeExpr("a,b.a,b");
            var t6a = Parser.ParseTypeExpr("x.x->x");
            var t6b = Parser.ParseTypeExpr("a,b.(A,a)->(A,b)");
            var t7a = Parser.ParseTypeExpr("a,b.(A,a) | a | (A,b)");
            var t7b = Parser.ParseTypeExpr("x,y.x|y|x");

            // Act
            var subs2 = TypeExpr.UnifyAndSolve(t1, t2);
            var subs3 = TypeExpr.UnifyAndSolve(t1, t3);
            var subs4 = TypeExpr.UnifyAndSolve(t4a, t4b);
            var subs5 = TypeExpr.UnifyAndSolve(t5a, t5b);
            var subs6 = TypeExpr.UnifyAndSolve(t6a, t6b);
            var subs7 = TypeExpr.UnifyAndSolve(t7a, t7b);

            var T2 = t2.Substitute(subs2);
            var T3 = t3.Substitute(subs3);
            var T4A = t4a.Substitute(subs4);
            var T4B = t4b.Substitute(subs4);
            var T5A = t5a.Substitute(subs5);
            var T5B = t5b.Substitute(subs5);
            var T6A = t6a.Substitute(subs6);
            var T6B = t6b.Substitute(subs6);
            var T7A = t7a.Substitute(subs7);
            var T7B = t7b.Substitute(subs7);

            // Assert
            Assert.IsTrue(t1.DeepEquals(T2));
            Assert.IsTrue(t1.DeepEquals(T3));
            Assert.IsTrue(T4A.DeepEquals(T4B));
            Assert.IsTrue(T5A.DeepEquals(T5B));
            Assert.IsTrue(T6A.DeepEquals(T6B));
            Assert.IsTrue(T7A.DeepEquals(T7B));
        }        
    }
}