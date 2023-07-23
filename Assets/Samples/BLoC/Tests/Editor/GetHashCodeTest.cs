using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.TestTools.Constraints;
using Is = NUnit.Framework.Is;

namespace Samples.BLoC.Tests.Editor
{
    public class GetHashCodeTest
    {
        [Test]
        public void BoxingTest()
        {
            var boxing = new BoxingHashCode<int>(10).GetHashCode();
            var nonBoxing = new NonBoxingHashCode<int>(10).GetHashCode();
            Assert.IsTrue(boxing == nonBoxing);
        }

        [Test]
        public void BoxingAllocTest()
        {
            var boxing = new BoxingHashCode<int>(10);
            Assert.That(() =>
            {
                var boxingCode = boxing.GetHashCode();
            }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void NonBoxingAllocTest()
        {
            var boxing = new NonBoxingHashCode<int>(10);
            Assert.That(() =>
            {
                var boxingCode = boxing.GetHashCode();
            }, Is.Not.AllocatingGCMemory());
        }

        [Test, Performance]
        public void EqualityComparerPerformanceTest()
        {
            var boxing = new BoxingHashCode<int>(10);
            var nonBoxing = new NonBoxingHashCode<int>(10);
            Measure.Method(() =>
                {
                    using (Measure.Scope("boxing"))
                    {
                        var boxingCode = boxing.GetHashCode();
                    }

                    using (Measure.Scope("non boxing"))
                    {
                        var nonBoxingCode = nonBoxing.GetHashCode();
                    }
                })
                .MeasurementCount(100)
                .Run();
        }
    }

    public class BoxingHashCode<T>
    {
        private readonly T _value;

        public BoxingHashCode(T value)
        {
            _value = value;
        }

        public override int GetHashCode() => _value.GetHashCode();
    }

    public class NonBoxingHashCode<T>
    {
        private readonly T _value;

        public NonBoxingHashCode(T value)
        {
            _value = value;
        }

        public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(_value);
    }
}