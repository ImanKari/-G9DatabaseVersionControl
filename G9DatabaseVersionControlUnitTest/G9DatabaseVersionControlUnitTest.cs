using System;
using System.Threading.Tasks;
using G9DatabaseVersionControlCore.Class.SmallLogger;
using NUnit.Framework;

namespace G9DatabaseVersionControlUnitTest
{
    public class G9DatabaseVersionControlUnitTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Order(1)]
        public void TestSmallLoggerWithoutInitialize()
        {
            Parallel.For(1, 1000, counter =>
            {
                if (counter % 4 == 0)
                    new Exception($"Test Exception: {counter}",
                            new Exception($"Test Inner Exception: {counter}"))
                        .G9SmallLogException($"Test Exception Additional Message: {counter}");
                if (counter % 3 == 0)
                    $"Test Error: {counter}".G9SmallLogError();
                if (counter % 2 == 0)
                    $"Test Warning: {counter}".G9SmallLogWarning();
                else
                    $"Test Information: {counter}".G9SmallLogInformation();
            });
            Assert.Pass();
        }

        [Test]
        [Order(2)]
        public void TestSmallLoggerWitheInitialize()
        {
            G9CSmallLogger.Initialize(Environment.CurrentDirectory, "CustomLogPath",
                $"LogFile-{DateTime.Now:HH-mm-ss.fff}");
            Parallel.For(1, 1000, counter =>
            {
                if (counter % 4 == 0)
                    new Exception($"Test Exception: {counter}",
                            new Exception($"Test Inner Exception: {counter}"))
                        .G9SmallLogException($"Test Exception Additional Message: {counter}");
                if (counter % 3 == 0)
                    $"Test Error: {counter}".G9SmallLogError();
                if (counter % 2 == 0)
                    $"Test Warning: {counter}".G9SmallLogWarning();
                else
                    $"Test Information: {counter}".G9SmallLogInformation();
            });
            Assert.Pass();
        }
    }
}