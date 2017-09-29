
using System;
using System.IO;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.AzureStorage;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Enferno.Public.Azure.Storage
{
    public interface IBlobStorageWrapper
    {
        void UploadFromStream(Stream stream, string targetFileName);
        void DownloadToStream(Stream target, string sourceFileName);
    }

    public class BlobStorageWrapper : IBlobStorageWrapper
    {
        private readonly string connectionString;
        private readonly string containerName;

        public BlobStorageWrapper(string connectionString, string containerName)
        {
            this.connectionString = connectionString;
            this.containerName = containerName;
        }

        public void UploadFromStream(Stream stream, string targetFileName)
        {
            var blobContainer = GetBlobContainer();
            var blockBlob = blobContainer.GetBlockBlobReference(targetFileName);
            blockBlob.UploadFromStream(stream);
        }

        public void DownloadToStream(Stream target, string sourceFileName)
        {
            var blobContainer = GetBlobContainer();
            var blockBlob = blobContainer.GetBlockBlobReference(sourceFileName);
            blockBlob.DownloadToStream(target);
        }

        private CloudBlobContainer GetBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            return blobClient.GetContainerReference(containerName);
        }
    }

    public class BlobStorage
    {
        private readonly IBlobStorageWrapper wrapper; 

        /// <summary>
        /// The BlobStorage class enables reads and write to an Azure BlobStorage account.
        /// </summary>
        /// <param name="connectionString">Format: DefaultEndpointsProtocol=https;AccountName=YourAccountName;AccountKey=YourAccessKey. The values can be found on your Azure Storage</param>
        /// <param name="containerName">The Container name.</param>
        public BlobStorage(string connectionString, string containerName)
        {
            this.wrapper = new BlobStorageWrapper(connectionString, containerName);
        }

        public BlobStorage(IBlobStorageWrapper wrapper)
        {
            this.wrapper = wrapper;
        }

        public void WriteFile(string sourceFilePath, string targetFileName)
        {
            var retryStrategy = new FixedInterval(3, TimeSpan.FromSeconds(1));
            var retryPolicy = new RetryPolicy<StorageTransientErrorDetectionStrategy>(retryStrategy);

            retryPolicy.ExecuteAction(() =>
            {
                using (var stream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
                {
                    wrapper.UploadFromStream(stream, targetFileName);
                }
            });
        }

        public void ReadFile(Stream target, string sourceFileName)
        {
            var retryStrategy = new FixedInterval(3, TimeSpan.FromSeconds(1));
            var retryPolicy = new RetryPolicy<StorageTransientErrorDetectionStrategy>(retryStrategy);

            retryPolicy.ExecuteAction(() => wrapper.DownloadToStream(target, sourceFileName));
            target.Position = 0;
        }     
    }
}
