using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management.WebSites;
using Microsoft.WindowsAzure.Management.WebSites.Models;

namespace Enferno.Public.Azure.Management.ManagementHandlers
{
    public class WebsiteManagementHandler : BaseManagementHandler
    {
        public WebsiteManagementHandler(SubscriptionCloudCredentials credentials) : base(credentials)
        {
        }

        public async Task<WebSite> CreateWebSite(string websiteName, string webSpaceName, string webSpaceGeoRegion,
            WebSiteComputeMode computeMode, WebSiteMode? siteMode = null)
        {
            if (String.IsNullOrEmpty(websiteName))
                throw new ArgumentException("A website name must be provided");

            websiteName = websiteName.Replace('.', '-'); //make sure we don't have any dots in the website name.

            if (String.IsNullOrEmpty(webSpaceName))
                throw new ArgumentException("A web space name must be provided");

            if (String.IsNullOrEmpty(webSpaceGeoRegion))
                throw new ArgumentException("A web space geo region must be provided");

            using (var websiteClient = new WebSiteManagementClient(Credentials))
            {
                var domainIsAvailableResult = await websiteClient.WebSites.IsHostnameAvailableAsync(websiteName);

                if (!domainIsAvailableResult.IsAvailable)
                {
                    throw new ArgumentException("The given domain name is not available");
                }

                if (siteMode == null)
                {
                    siteMode = WebSiteMode.Basic;
                }

                var hostName = String.Format("{0}.azurewebsites.net", websiteName);
                var result = await websiteClient.WebSites.CreateAsync(webSpaceName, new WebSiteCreateParameters()
                {
                    Name = websiteName,
                    HostNames = new List<string> {hostName},
                    SiteMode = siteMode,
                    ComputeMode = computeMode,
                    WebSpaceName = webSpaceName,
                    WebSpace =
                        new WebSiteCreateParameters.WebSpaceDetails()
                        {
                            GeoRegion = webSpaceGeoRegion,
                            Name = webSpaceName,
                            Plan = WebSpacePlanNames.VirtualDedicatedPlan
                        }
                });

                return result.WebSite;
            }
        }

        public async Task<IEnumerable<WebSiteGetPublishProfileResponse.PublishProfile>> GetPublishingProfiles(string webSiteName, string webSpaceName)
        {
            using (var websiteclient = new WebSiteManagementClient(Credentials))
            {
                var publishingProfileResponse = await websiteclient.WebSites.GetPublishProfileAsync(webSpaceName, webSiteName);

                return publishingProfileResponse.PublishProfiles;
            }
        }

        public async Task<bool> DeleteWebSite(string webSiteName, WebSpacesListResponse.WebSpace webSpace)
        {
            using (var websiteClient = new WebSiteManagementClient(Credentials))
            {
                var deleteResult = await websiteClient.WebSites.DeleteAsync(webSpace.Name, webSiteName,
                    new WebSiteDeleteParameters()
                    {
                        DeleteAllSlots = true,
                        DeleteMetrics = true,
                        DeleteEmptyServerFarm = true
                    });

                return deleteResult.StatusCode == HttpStatusCode.OK;
            }            
        }

        public async Task<WebSite> GetExistingWebSite(string websiteName, string webSpaceName)
        {
            using (var websiteClient = new WebSiteManagementClient(Credentials))
            {
                try
                {
                    var webSiteResult = await websiteClient.WebSites.GetAsync(webSpaceName, websiteName, null);
                    return webSiteResult.WebSite;
                }
                catch (CloudException ex)
                {
                    if (ex.ErrorCode == "NotFound")
                        return null;
                    
                    throw;
                }

            }
        }

        public async Task<IEnumerable<WebSpacesListResponse.WebSpace>> GetExistingWebSpaces()
        {
            using (var websiteClient = new WebSiteManagementClient(Credentials))
            {
                var webSpacesListResult = await websiteClient.WebSpaces.ListAsync();
                return webSpacesListResult.WebSpaces;
            }            
        }

        public async Task<IEnumerable<ServerFarmListResponse.ServerFarm>> GetExistingServerFarms(string webSpaceName)
        {
            using (var websiteClient = new WebSiteManagementClient(Credentials))
            {
                var serverFarmListResult = await websiteClient.ServerFarms.ListAsync(webSpaceName);
                return serverFarmListResult.ServerFarms;
            }
        }
    }
}
