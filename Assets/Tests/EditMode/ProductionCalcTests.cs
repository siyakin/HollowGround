using HollowGround.Domain.Production;
using NUnit.Framework;

namespace HollowGround.Tests
{
    [TestFixture]
    public class ProductionCalcTests
    {
        [Test]
        public void NoWorkersRequired_Returns1()
        {
            float result = ProductionCalc.WorkerModifier(0, 0, 1f);
            Assert.AreEqual(1f, result);
        }

        [Test]
        public void FullDependency_NoWorkers_Returns0()
        {
            float result = ProductionCalc.WorkerModifier(0, 4, 1f);
            Assert.AreEqual(0f, result);
        }

        [Test]
        public void FullDependency_FullWorkers_Returns1()
        {
            float result = ProductionCalc.WorkerModifier(4, 4, 1f);
            Assert.AreEqual(1f, result);
        }

        [Test]
        public void HalfDependency_HalfWorkers_Returns0Point5()
        {
            float result = ProductionCalc.WorkerModifier(2, 4, 0.5f);
            Assert.AreEqual(0.75f, result, 0.001f);
        }

        [Test]
        public void NoDependency_IgnoresWorkers()
        {
            float result = ProductionCalc.WorkerModifier(0, 10, 0f);
            Assert.AreEqual(1f, result);
        }

        [Test]
        public void ExcessWorkers_CappedAt1()
        {
            float result = ProductionCalc.WorkerModifier(20, 4, 1f);
            Assert.AreEqual(1f, result);
        }

        [Test]
        public void ModifiedInterval_AllBonuses()
        {
            float result = ProductionCalc.ModifiedInterval(10f, 0.5f, 0.2f, 2f);
            Assert.AreEqual(2f, result, 0.001f);
        }

        [Test]
        public void ModifiedInterval_NoWorkerModifier()
        {
            float result = ProductionCalc.ModifiedInterval(10f, 1f, 0f, 0f);
            Assert.AreEqual(10f, result, 0.001f);
        }

        [Test]
        public void ProductionAmount_Level1()
        {
            Assert.AreEqual(5, ProductionCalc.ProductionAmount(5, 1));
        }

        [Test]
        public void ProductionAmount_Level5()
        {
            Assert.AreEqual(9, ProductionCalc.ProductionAmount(5, 5));
        }
    }
}
