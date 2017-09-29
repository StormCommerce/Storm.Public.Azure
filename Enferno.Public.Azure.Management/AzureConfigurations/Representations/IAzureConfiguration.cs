using System.Collections.Generic;
using Enferno.Public.Azure.Management.Authentication.TokenCredentialConfigurations;

namespace Enferno.Public.Azure.Management.AzureConfigurations.Representations
{
    public interface IAzureConfiguration
    {
        IList<DatabaseConfiguration> DatabaseConfigurations { get; }
        IList<WebsiteConfiguration> WebsiteConfigurations { get; }

        ITokenCredentialConfiguration TokenCredentialConfiguration { get; }
    }
}
