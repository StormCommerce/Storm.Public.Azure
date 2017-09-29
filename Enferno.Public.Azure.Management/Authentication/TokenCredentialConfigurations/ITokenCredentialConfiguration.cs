
namespace Enferno.Public.Azure.Management.Authentication.TokenCredentialConfigurations
{
    /// <summary>
    /// Provides the basic data points needed to support AAD 
    /// authentication in an app making use of the 
    /// management libraries.
    /// </summary>
    public interface ITokenCredentialConfiguration
    {
        string TenantId { get; }
        string ClientId { get; }
        string RedirectUrl { get; }
        string SubscriptionId { get; }
    }
}
