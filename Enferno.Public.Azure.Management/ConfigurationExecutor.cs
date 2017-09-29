using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enferno.Public.Azure.Management.Authentication;
using Enferno.Public.Azure.Management.AzureConfigurations.Representations;
using Enferno.Public.Azure.Management.ManagementHandlers;
using log4net;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Common.Tracing.Log4Net;
using Microsoft.WindowsAzure.Management.Sql.Models;

namespace Enferno.Public.Azure.Management
{
    public class ConfigurationExecutor
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ConfigurationExecutor));

        private readonly IAzureConfiguration _configuration;
        private TokenCloudCredentials _credentials;

        public ConfigurationExecutor(IAzureConfiguration configuration, Log4NetTracingInterceptor tracingInterceptor = null)
        {
            if (tracingInterceptor != null)
            {
                CloudContext.Configuration.Tracing.AddTracingInterceptor(tracingInterceptor);
                CloudContext.Configuration.Tracing.IsEnabled = true;
            }

            _configuration = configuration;
        }

        private TokenCloudCredentials Credentials
        {
            get
            {
                if (_credentials == null)
                {
                    var tokenCredentialHelper = new TokenCredentialHelper(_configuration.TokenCredentialConfiguration);
                    _credentials = tokenCredentialHelper.GetCredentials().Result;
                }

                return _credentials;
            }
        }

        public async Task Execute()
        {
            Logger.Info("Got credentials");

            try
            {
                await EnsureWebsitesExist(_configuration.WebsiteConfigurations, Credentials);
            }
            catch (Exception e)
            {
                Logger.Error("Error ensuring websites", e);
                throw;
            }


            try
            {
                await EnsureDatabasesExists(_configuration.DatabaseConfigurations, Credentials);
            }
            catch (Exception e)
            {
                Logger.Error("Error ensuring databases", e);
                throw;
            }
        }

        private static async Task EnsureWebsitesExist(IEnumerable<WebsiteConfiguration> websiteDefinitions, SubscriptionCloudCredentials credentials)
        {
            var websiteManagementHandler = new WebsiteManagementHandler(credentials);

            foreach (var websiteDef in websiteDefinitions)
            {
                var existingWebsite = await websiteManagementHandler.GetExistingWebSite(websiteDef.Name,
                    websiteDef.WebSpaceName);

                if (existingWebsite == null)
                {
                    var website =
                        await
                            websiteManagementHandler.CreateWebSite(websiteDef.Name, websiteDef.WebSpaceName,
                                websiteDef.WebSpaceGeoRegion, websiteDef.ComputeMode, websiteDef.SiteMode);
                    Logger.Info(String.Format("Website with name {0} created. Can be found at url: {1}",
                        websiteDef.Name, website.Uri));
                }
                else
                {
                    Logger.Info(String.Format("Website with name {0} already existed. Can be found at url: {1}",
                        websiteDef.Name, existingWebsite.Uri));                    
                }
            }          
        }

        private static async Task EnsureDatabasesExists(IEnumerable<DatabaseConfiguration> databaseConfigurations,
            SubscriptionCloudCredentials credentials)
        {


            var sqlDatabaseManagementHandler = new SqlDatabaseManagementHandler(credentials);

            foreach (var databaseConfiguration in databaseConfigurations)
            {
                if (String.IsNullOrEmpty(databaseConfiguration.Name))
                    throw new ArgumentException("A database name must be provided");

                var serverName = String.Empty;

                if (String.IsNullOrEmpty(databaseConfiguration.DatabaseServerAdminUser))
                    throw new ArgumentException("A admin user name must be provided");
                if (String.IsNullOrEmpty(databaseConfiguration.DatabaseServerAdminPassword))
                    throw new ArgumentException("A admin password must be provided");

                var existingServersEnumerable =
                    await sqlDatabaseManagementHandler.GetExistingServers();

                var existingServers = existingServersEnumerable.ToList();

                if (!String.IsNullOrWhiteSpace(databaseConfiguration.ServerName))
                {
                    var matchingServer =
                        existingServers.SingleOrDefault(server => server.Name == databaseConfiguration.ServerName);
                    if (matchingServer == null)
                    {
                        throw new ArgumentException(String.Format("Server with name {0} does not exist.", databaseConfiguration.ServerName));
                    }

                    serverName = matchingServer.Name;
                }
                else //no server name was given in the configuration.
                {
                    if (!existingServers.Any())
                    {
                        serverName =
                            await
                                sqlDatabaseManagementHandler.CreateServer(databaseConfiguration.DatabaseServerAdminUser,
                                    databaseConfiguration.DatabaseServerAdminPassword);

                        Logger.Info(String.Format("No database server existed. Created server with name {0}", serverName));
                    }
                    else
                    {
                        serverName = existingServers.First().Name;
                        Logger.Info(String.Format("Found server with name {0}. Will use that one.", serverName));
                    }
                }

                var existingDatabasesForServer =
                    await sqlDatabaseManagementHandler.GetExistingDatabases(serverName);

                var databaseWithNameExistsOnServer =
                    existingDatabasesForServer.Any(database => database.Name == databaseConfiguration.Name);

                if (!databaseWithNameExistsOnServer)
                {
                    var database =
                        await
                            sqlDatabaseManagementHandler.CreateDatabase(serverName, databaseConfiguration.Name,
                            databaseConfiguration.Edition ?? DatabaseEditions.Web, 
                            databaseConfiguration.MaximumDatabaseSizeInGB == 0 ? 1 : databaseConfiguration.MaximumDatabaseSizeInGB);

                    Logger.Info(String.Format("Created database with name {0} on server {1}", database.Name, serverName));
                }
                else
                {
                    Logger.Info(String.Format("Database with name {0} already existed on server {1}", databaseConfiguration.Name, serverName));
                }


            }
        }

        private static async Task HandlePublishProfiles(WebsiteConfiguration websiteConfiguration, SubscriptionCloudCredentials credentials)
        {
            var websiteManagementHandler = new WebsiteManagementHandler(credentials);

            var profiles = await websiteManagementHandler.GetPublishingProfiles(websiteConfiguration.Name, websiteConfiguration.WebSpaceName);
        }
    }
}
