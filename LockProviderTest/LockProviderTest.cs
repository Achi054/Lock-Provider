namespace LockProvider
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LockProviderTest
    {
        [TestMethod]
        public void LockProviderOnConcurrencyReturnWithoutError()
        {
            // Arrange
            var task1 = Task.Run(() => MethodWithLockUnderTest("FirstTask"));
            var task2 = Task.Run(() => MethodWithLockUnderTest("SecondTask"));
            var task3 = Task.Run(() => MethodWithLockUnderTest("ThirdTask"));
            var task4 = Task.Run(() => MethodWithLockUnderTest("FirstTask"));
            var task5 = Task.Run(() => MethodWithLockUnderTest("FirstTask"));
            var task6 = Task.Run(() => MethodWithLockUnderTest("SecondTask"));
            var task7 = Task.Run(() => MethodWithLockUnderTest("ThirdTask"));
            var task8 = Task.Run(() => MethodWithLockUnderTest("ThirdTask"));

            // Act and Assert
            try
            {
                Task.WaitAll(task1, task2, task3, task4, task5, task6, task7);
            }
            catch (Exception ex)
            {
                Assert.Fail("Expected no exception, but got: " + ex.Message);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void LockProviderOnConcurrencyReturnWithError()
        {
            // Arrange
            var task1 = Task.Run(() => MethodWithoutLockUnderTest("FirstTask"));
            var task2 = Task.Run(() => MethodWithoutLockUnderTest("SecondTask"));
            var task3 = Task.Run(() => MethodWithoutLockUnderTest("ThirdTask"));
            var task4 = Task.Run(() => MethodWithoutLockUnderTest("FirstTask"));
            var task5 = Task.Run(() => MethodWithoutLockUnderTest("FirstTask"));
            var task6 = Task.Run(() => MethodWithoutLockUnderTest("SecondTask"));
            var task7 = Task.Run(() => MethodWithoutLockUnderTest("ThirdTask"));
            var task8 = Task.Run(() => MethodWithoutLockUnderTest("ThirdTask"));

            // Act
            Task.WaitAll(task1, task2, task3, task4, task5, task6, task7);
        }

        private static void MethodWithoutLockUnderTest(string key)
        {
            CreateFile(key);
            DeleteFile(key);
            Debug.WriteLine($"Resource under use for {key}!!");
        }

        private static void MethodWithLockUnderTest(string key)
        {
            using (var locker = new LockProvider<string>(key))
            {
                CreateFile(key);
                DeleteFile(key);
                Debug.WriteLine($"Resource under use for {key}!!");
            }
        }

        private static void CreateFile(string fileName)
        {
            using (FileStream fs = File.Create($"./{fileName}.txt"))
            {
                // Add some text to file
                var contentOne = new UTF8Encoding(true).GetBytes("Test content One.");
                fs.Write(contentOne, 0, contentOne.Length);
                var contentTwo = new UTF8Encoding(true).GetBytes("Test content Two.");
                fs.Write(contentTwo, 0, contentTwo.Length);
            }
        }

        private static void DeleteFile(string fileName)
        {
            File.Delete($"./{fileName}.txt");
        }
    }
}
