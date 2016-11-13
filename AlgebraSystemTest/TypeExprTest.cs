
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
        public void TypeTreeTest_UnifyTestSuccesses() {
            // Arrange
            var ATree = TypeTree.MakePrimitiveTree("A");
            var BTree = TypeTree.MakePrimitiveTree("B");
            var ABATree = SExpression.ParseTypeTree("A->B->A");
            var AATree = SExpression.ParseTypeTree("A->A");
            var AaTree = SExpression.ParseTypeTree("(A,a)");

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
        public void TypeTreeTest_UnifyTestFailures() {
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

    }
}