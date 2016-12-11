
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlgebraSystem;
using System.Collections.Generic;

namespace UnitTestProject1 {

    [TestClass]
    public class TreeTransformationTest {

        [TestMethod]
        public void TreeTransformationFactory_Make_DeMorgansLawTest() {
            // Arrange
            Namespace gns = Namespace.CreateGlobalNs();

            TermNew t1 = TermNew.TermFromSExpression("AND (f x) (f y)", gns);
            TermNew t2 = TermNew.TermFromSExpression("f (OR x y)", gns);

            TermNew expectedLeft = TermNew.TermFromSExpression("AND (NOT true) (NOT false)", gns);
            TermNew expectedRight = TermNew.TermFromSExpression("NOT (OR true false)", gns);

            // Act
            TreeTransformation tt = TreeTransformationFactory.Make(t1, t2, gns);
            TermNew actualRight = tt.TransformLeft(expectedLeft);
            TermNew actualLeft = tt.TransformRight(actualRight);

            // Assert
            Assert.IsTrue(expectedLeft.DeepEquals(actualLeft));
            Assert.IsTrue(expectedRight.DeepEquals(actualRight));
        }


        [TestMethod]
        public void TreeTransformationFactory_MakeHomomorphism_ExponentLawTest() {
            // Arrange
            Namespace gns = Namespace.CreateGlobalNs();

            TermNew t1 = TermNew.TermFromSExpression("* (^ b x) (^ b y)", gns);
            TermNew t2 = TermNew.TermFromSExpression("^ b (+ x y)", gns);

            TermNew expectedLeft = TermNew.TermFromSExpression("* (^ 2 6) (^ 2 4)", gns);
            TermNew expectedRight = TermNew.TermFromSExpression("^ 2 (+ 6 4)", gns);

            // Act
            TreeTransformation tt = TreeTransformationFactory.MakeHomomorphism("^ b", "*", "+", gns);
            TermNew actualRight = tt.TransformLeft(expectedLeft);
            TermNew actualLeft = tt.TransformRight(actualRight);

            // Assert
            Assert.IsTrue(expectedLeft.DeepEquals(actualLeft));
            Assert.IsTrue(expectedRight.DeepEquals(actualRight));
        }

        [TestMethod]
        public void TreeTransformationFactory_LookUp_FactoringTest() {
            // Arrange
            Namespace gns = Namespace.CreateGlobalNs();


            TermNew expectedLeft = TermNew.TermFromSExpression("+ (* 2 6) (* 2 4)", gns);
            TermNew expectedRight = TermNew.TermFromSExpression("* 2 (+ 6 4)", gns);

            // Act
            TreeTransformation tt = gns.TransformationLookup("Factoring");
            TermNew actualRight = tt.TransformLeft(expectedLeft);
            TermNew actualLeft = tt.TransformRight(actualRight);

            // Assert
            Assert.IsTrue(expectedLeft.DeepEquals(actualLeft));
            Assert.IsTrue(expectedRight.DeepEquals(actualRight));
        }

    }
}