﻿namespace Unit.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.Implementation;
    using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.Implementation.QuickPulse;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class QuickPulseDataSampleTests
    {
        private IDictionary<string, Tuple<PerformanceCounterData, double>> dummyDictionary;

        [TestInitialize]
        public void TestInitialize()
        {
            this.dummyDictionary = new Dictionary<string, Tuple<PerformanceCounterData, double>>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void QuickPulseDataSampleThrowsWhenAccumulatorIsNull()
        {
            new QuickPulseDataSample(null, this.dummyDictionary);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void QuickPulseDataSampleThrowsWhenPerfDataIsNull()
        {
            new QuickPulseDataSample(new QuickPulseDataAccumulator(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void QuickPulseDataSampleThrowsWhenAccumulatorStartTimestampIsNull()
        {
            new QuickPulseDataSample(new QuickPulseDataAccumulator() { EndTimestamp = DateTimeOffset.UtcNow }, this.dummyDictionary);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void QuickPulseDataSampleThrowsWhenAccumulatorEndTimestampIsNull()
        {
            new QuickPulseDataSample(new QuickPulseDataAccumulator() { StartTimestamp = DateTimeOffset.UtcNow }, this.dummyDictionary);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void QuickPulseDataSampleThrowsWhenTimestampsAreReversedInTime()
        {
            new QuickPulseDataSample(
                new QuickPulseDataAccumulator() { StartTimestamp = DateTimeOffset.UtcNow, EndTimestamp = DateTimeOffset.UtcNow.AddSeconds(-1) },
                this.dummyDictionary);
        }

        [TestMethod]
        public void QuickPulseDataSampleTimestampsItselfCorrectly()
        {
            // ARRANGE
            var timestampStart = DateTimeOffset.UtcNow;
            var timestampEnd = DateTimeOffset.UtcNow.AddSeconds(3);
            var accumulator = new QuickPulseDataAccumulator { StartTimestamp = timestampStart, EndTimestamp = timestampEnd };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyDictionary);

            // ASSERT
            Assert.AreEqual(timestampStart, dataSample.StartTimestamp);
            Assert.AreEqual(timestampEnd, dataSample.EndTimestamp);
        }

        #region AI data calculation checks

        #region Requests

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIRpsCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
                                  {
                                      StartTimestamp = DateTimeOffset.UtcNow,
                                      EndTimestamp = DateTimeOffset.UtcNow.AddSeconds(2),
                                      AIRequestCountAndDurationInTicks = QuickPulseDataAccumulator.EncodeCountAndDuration(10, 0)
                                  };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyDictionary);

            // ASSERT
            Assert.AreEqual(10.0 / 2, dataSample.AIRequestsPerSecond);
        }

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIRequestDurationAveCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
                                  {
                                      StartTimestamp = DateTimeOffset.UtcNow,
                                      EndTimestamp = DateTimeOffset.UtcNow.AddSeconds(2),
                                      AIRequestCountAndDurationInTicks =
                                          QuickPulseDataAccumulator.EncodeCountAndDuration(10, TimeSpan.FromSeconds(5).Ticks)
                                  };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyDictionary);

            // ASSERT
            Assert.AreEqual(TimeSpan.FromSeconds(5).TotalMilliseconds / 10.0, dataSample.AIRequestDurationAveInMs);
        }

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIRequestsFailedPerSecondCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
                                  {
                                      StartTimestamp = DateTimeOffset.UtcNow,
                                      EndTimestamp = DateTimeOffset.UtcNow.AddSeconds(2),
                                      AIRequestFailureCount = 10
                                  };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyDictionary);

            // ASSERT
            Assert.AreEqual(10.0 / 2, dataSample.AIRequestsFailedPerSecond);
        }

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIRequestsSucceededPerSecondCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
                                  {
                                      StartTimestamp = DateTimeOffset.UtcNow,
                                      EndTimestamp = DateTimeOffset.UtcNow.AddSeconds(2),
                                      AIRequestSuccessCount = 10
                                  };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyDictionary);

            // ASSERT
            Assert.AreEqual(10.0 / 2, dataSample.AIRequestsSucceededPerSecond);
        }

        #endregion

        #region Dependency calls

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIDependencyCallsPerSecondCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
                                  {
                                      StartTimestamp = DateTimeOffset.UtcNow,
                                      EndTimestamp = DateTimeOffset.UtcNow.AddSeconds(2),
                                      AIDependencyCallCountAndDurationInTicks =
                                          QuickPulseDataAccumulator.EncodeCountAndDuration(10, 0)
                                  };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyDictionary);

            // ASSERT
            Assert.AreEqual(10.0 / 2, dataSample.AIDependencyCallsPerSecond);
        }

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIDependencyCallDurationAveCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
                                  {
                                      StartTimestamp = DateTimeOffset.UtcNow,
                                      EndTimestamp = DateTimeOffset.UtcNow.AddSeconds(2),
                                      AIDependencyCallCountAndDurationInTicks =
                                          QuickPulseDataAccumulator.EncodeCountAndDuration(10, TimeSpan.FromSeconds(5).Ticks)
                                  };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyDictionary);

            // ASSERT
            Assert.AreEqual(TimeSpan.FromSeconds(5).TotalMilliseconds / 10.0, dataSample.AIDependencyCallDurationAveInMs);
        }

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIDependencyCallsFailedPerSecondCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
                                  {
                                      StartTimestamp = DateTimeOffset.UtcNow,
                                      EndTimestamp = DateTimeOffset.UtcNow.AddSeconds(2),
                                      AIDependencyCallFailureCount = 10
                                  };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyDictionary);

            // ASSERT
            Assert.AreEqual(10.0 / 2, dataSample.AIDependencyCallsFailedPerSecond);
        }

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIDependencyCallsSucceededPerSecondCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
                                  {
                                      StartTimestamp = DateTimeOffset.UtcNow,
                                      EndTimestamp = DateTimeOffset.UtcNow.AddSeconds(2),
                                      AIDependencyCallSuccessCount = 10
                                  };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyDictionary);

            // ASSERT
            Assert.AreEqual(10.0 / 2, dataSample.AIDependencyCallsSucceededPerSecond);
        }

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIRequestDurationAveWhenRequestCountIsZeroCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
                                  {
                                      StartTimestamp = DateTimeOffset.UtcNow,
                                      EndTimestamp = DateTimeOffset.UtcNow.AddSeconds(2),
                                      AIRequestCountAndDurationInTicks =
                                          QuickPulseDataAccumulator.EncodeCountAndDuration(0, TimeSpan.FromSeconds(5).Ticks)
                                  };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyDictionary);

            // ASSERT
            Assert.AreEqual(0.0, dataSample.AIRequestDurationAveInMs);
        }

        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIDependencyCallDurationAveWhenDependencyCallCountIsZeroCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
                                  {
                                      StartTimestamp = DateTimeOffset.UtcNow,
                                      EndTimestamp = DateTimeOffset.UtcNow.AddSeconds(2),
                                      AIDependencyCallCountAndDurationInTicks =
                                          QuickPulseDataAccumulator.EncodeCountAndDuration(0, TimeSpan.FromSeconds(5).Ticks)
                                  };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyDictionary);

            // ASSERT
            Assert.AreEqual(0.0, dataSample.AIDependencyCallDurationAveInMs);
        }

        #endregion

        #region Exceptions
        [TestMethod]
        public void QuickPulseDataSampleCalculatesAIExceptionsPerSecondCorrectly()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
            {
                StartTimestamp = DateTimeOffset.UtcNow,
                EndTimestamp = DateTimeOffset.UtcNow.AddSeconds(2),
                AIExceptionCount = 3
            };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyDictionary);

            // ASSERT
            Assert.AreEqual(3.0 / 2, dataSample.AIExceptionsPerSecond);
        }

        #endregion

        #endregion

        #region Perf data calculation checks
        [TestMethod]
        public void QuickPulseDataSampleHandlesAbsentCounterInPerfData()
        {
            // ARRANGE
            var accumulator = new QuickPulseDataAccumulator
            {
                StartTimestamp = DateTimeOffset.UtcNow,
                EndTimestamp = DateTimeOffset.UtcNow.AddSeconds(2)
            };

            // ACT
            var dataSample = new QuickPulseDataSample(accumulator, this.dummyDictionary);

            // ASSERT
            Assert.IsFalse(dataSample.PerfCountersLookup.Any());
        }
        #endregion
    }
}