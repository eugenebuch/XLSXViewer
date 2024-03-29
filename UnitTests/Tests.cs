using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Net;
using XLSXViewer.Tools;

namespace UnitTests
{
    [TestFixture]
    public class Tests
    {
        private const string Url = @"https://bdu.fstec.ru/files/documents/thrlist.xlsx";
        private readonly string savePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Test.xlsx";

        [SetUp]
        public void Setup()
        {
            var client = new WebClient();
            client.DownloadFile(new Uri(Url), savePath);
        }

        [Test]
        public void ExcelReader_ParseToThreat_ThreatOnReturn()
        {
            //arrange

            //act
            var result = ExcelReader.Read(savePath).ToList()[0].Id != null;

            //assert
            Assert.AreEqual(true, result);
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(savePath);
        }
    }
}