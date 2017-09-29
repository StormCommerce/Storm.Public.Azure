using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management.Models;
using Microsoft.WindowsAzure.Management.Sql;
using Microsoft.WindowsAzure.Management.Sql.Models;

namespace Enferno.Public.Azure.Management.ManagementHandlers
{
    public class SqlDatabaseManagementHandler : BaseManagementHandler
    {
        public SqlDatabaseManagementHandler(SubscriptionCloudCredentials credentials) : base(credentials)
        {
        }

        public async Task<Database> CreateDatabase(string serverName, string databaseName, string edition = DatabaseEditions.Web, int maximumDatabaseSizeInGB = 1)
        {
            using (var sqlClient = new SqlManagementClient(Credentials))
            {
                var createDatabaseRequest =
                    await sqlClient.Databases.CreateAsync(serverName, new DatabaseCreateParameters()
                    {
                        Edition = edition,
                        Name = databaseName,
                        MaximumDatabaseSizeInGB = maximumDatabaseSizeInGB,
                        CollationName = "SQL_Latin1_General_CP1_CI_AS"
                    });

                return createDatabaseRequest.Database;
            }
        }

        public async Task<IEnumerable<Database>> GetExistingDatabases(string serverName)
        {
            using (var sqlClient = new SqlManagementClient(Credentials))
            {
                var listDatabasesRequest =
                    await sqlClient.Databases.ListAsync(serverName);

                return listDatabasesRequest.Databases;
            }            
        } 

        public async Task<string> CreateServer(string adminUserName,
            string adminPassword, string locationName = LocationNames.NorthEurope)
        {
            using (var managementClient = new SqlManagementClient(Credentials))
            {
                var createServerResult = await managementClient.Servers.CreateAsync(new ServerCreateParameters()
                {
                    AdministratorPassword = adminPassword,
                    AdministratorUserName = adminUserName,
                    Location = locationName
                });

                return createServerResult.ServerName;
            }
        }

        public async Task<IEnumerable<Server>> GetExistingServers()
        {
            using (var managementClient = new SqlManagementClient(Credentials))
            {
                var listServerNamesResult = await managementClient.Servers.ListAsync();

                return listServerNamesResult.Servers;
            } 
        }

        public async Task<Server> GetExistingServer(string serverName)
        {
            using (var managementClient = new SqlManagementClient(Credentials))
            {
                var listServerNamesResult = await managementClient.Servers.ListAsync();

                return listServerNamesResult.Servers.SingleOrDefault(server => server.Name == serverName);
            }             
        }

    }
}
