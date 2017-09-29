using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management.Models;
using Microsoft.WindowsAzure.Management.Storage;
using Microsoft.WindowsAzure.Management.Storage.Models;

namespace Enferno.Public.Azure.Management.ManagementHandlers
{
    public class StorageManagementHandler : BaseManagementHandler
    {
        public StorageManagementHandler(SubscriptionCloudCredentials credentials) : base(credentials)
        {
        }

        public async Task<string> CreateStorageAccount(string accountName, string label,
            string location = LocationNames.NorthEurope, bool geoReplicationEnabled = false)
        {
            using (var storageClient = new StorageManagementClient(Credentials))
            {
                var result = await storageClient.StorageAccounts.CreateAsync(
                    new StorageAccountCreateParameters
                    {
                        GeoReplicationEnabled = geoReplicationEnabled,
                        Label = label,
                        Location = location,
                        Name = accountName
                    });
            }

            return accountName;
        }

        public async Task<IEnumerable<StorageAccount>> GetExistingStorageAccounts()
        {
            using (var storageClient = new StorageManagementClient(Credentials))
            {
                var storageAccountListResult = await storageClient.StorageAccounts.ListAsync();
                return storageAccountListResult.StorageAccounts;
            }            
        }
    }
}
