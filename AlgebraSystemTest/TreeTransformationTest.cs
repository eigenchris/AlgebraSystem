﻿
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlgebraSystem;
using System.Collections.Generic;

namespace UnitTestProject1 {

    [TestClass]
    public class TreeTransformationTest {

        [TestMethod]
        public void TreeTransformation_DeMorgansLawTest() {
            // Arrange
            Namespace gns = Namespace.CreateGlobalNs();

            TermNew t1 = TermNew.TermFromSExpression("AND (f x) (f y)", gns);
            TermNew t2 = TermNew.TermFromSExpression("f (OR x y)", gns);

            TermNew expectedLeft = TermNew.TermFromSExpression("AND (NOT true) (NOT false)", gns);
            TermNew expectedRight = TermNew.TermFromSExpression("NOT (OR true false)", gns);

            // Act
            TreeTransformation tt = new TreeTransformation(t1, t2);
            TermNew actualRight = tt.TransformLeft(expectedLeft);
            TermNew actualLeft = tt.TransformRight(actualRight);

            // Assert
            Assert.IsTrue(expectedLeft.DeepEquals(actualLeft));
            Assert.IsTrue(expectedRight.DeepEquals(actualRight));
        }
     
    }
}