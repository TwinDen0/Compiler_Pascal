using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tester
{
    [TestClass]
    public class CompilerTester
    {
        public const string TestsDirectory = @"..\..\..\Tests";

        public static IEnumerable<object[]> TestMethodInput
        {
            get
            {
                var res = new List<object[]>();
                var directories = new DirectoryInfo(TestsDirectory).GetDirectories();
                foreach (var directory in directories)
                {
                    var key = "";
                    try
                    {
                        key = new StreamReader(directory.FullName + "\\.key").ReadToEnd();
                    }
                    catch (FileNotFoundException e)
                    {
                        Console.WriteLine(e);
                        continue;
                    }

                    var directoryInfo = new DirectoryInfo(directory.FullName + "\\Files");
                    var files = directoryInfo.GetFiles("*.in");
                    res.AddRange(files.Select(file => new object[]
                        {file.Directory + "\\" + Path.GetFileNameWithoutExtension(file.Name), key}));
                }

                return res;
            }
        }

        [TestMethod]
        [DynamicData(nameof(TestMethodInput))]
        public void Test(string fileName, string compileKeys)
        {
            var outputs = Utils.RunAndGetOutputs(fileName, compileKeys);
            Assert.AreEqual(outputs.Item2, outputs.Item1);
        }
    }
}