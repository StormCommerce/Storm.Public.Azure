using Microsoft.WindowsAzure.Management.WebSites.Models;

namespace Enferno.Public.Azure.Management.AzureConfigurations.Representations
{
    public struct WebsiteConfiguration
    {
        public string Name;
        public WebSiteComputeMode ComputeMode;
        public string WebSpaceName;
        public string WebSpaceGeoRegion;
        public string ServerFarmName;
        public WebSiteMode SiteMode;
    }
}