using System;
using System.IO;
using Enferno.Public.Azure.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Enferno.Public.Azure.Tests.Storage
{
    [TestClass]
    public class BlobStorageTests
    {
        private const string connectionString = "DefaultEndpointsProtocol=https;AccountName=MyStorageAccount;AccountKey=MyStorageAccessKey";
        private const string containerName = "my-upload-container-name";

        [TestMethod]
        [DeploymentItem("Storage/TestFiles/Test1.txt")]
        public void WriteTextFileTest()
        {
            // Arrange
            var storageWrapper = MockRepository.GenerateMock<IBlobStorageWrapper>();
            var blobStorage = new BlobStorage(storageWrapper);
           
            // Act            
            var sourceFilePath = Path.GetFullPath("Test1.txt");
            var targetFileName = string.Format("{0}.txt", new Guid("4A882D7E-4958-40F4-8648-028F44D9E435"));
            blobStorage.WriteFile(sourceFilePath, targetFileName);

            // Assert
            storageWrapper.AssertWasCalled(x => x.UploadFromStream((Stream)null, targetFileName), o => o.IgnoreArguments());
        }

        [TestMethod]
        [DeploymentItem("Storage/TestFiles/Test1.txt")]
        public void ReadTextFileTest()
        {
            // Arrange
            var storageWrapper = MockRepository.GenerateMock<IBlobStorageWrapper>();
            var blobStorage = new BlobStorage(storageWrapper);

            // Act            
            var sourceFileName = string.Format("{0}.txt", new Guid("4A882D7E-4958-40F4-8648-028F44D9E435"));

            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            {
                blobStorage.ReadFile(stream, sourceFileName);

                // Assert            
                Assert.AreEqual(0, stream.Position);
                storageWrapper.AssertWasCalled(x => x.DownloadToStream((Stream)null, sourceFileName), o => o.IgnoreArguments());
            }
        }
    }
}
