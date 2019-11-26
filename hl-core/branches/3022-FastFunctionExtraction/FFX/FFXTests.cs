using HeuristicLab.Algorithms.DataAnalysis.FastFunctionExtraction;
using NUnit.Framework;
using System.Runtime;

namespace FFXTests {
    public class FFXTests {
        [Test]
        public void TestLogSpace1() {
            Assert.AreEqual(2, Utils.logspace(10, 100, 2).Length);
            Assert.AreEqual(20, Utils.logspace(10, 100, 20).Length);
        }

        [Test]
        public void TestLogSpace2() {
            Assert.AreEqual(7, Utils.logspace(7, 71, 2)[0]);
            Assert.AreEqual(7, Utils.logspace(71, 71, 2)[1]);

        }
    }
}
