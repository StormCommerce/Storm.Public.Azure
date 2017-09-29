using Microsoft.WindowsAzure;

namespace Enferno.Public.Azure.Management.ManagementHandlers
{
    public abstract class BaseManagementHandler
    {
        protected readonly SubscriptionCloudCredentials Credentials;

        protected BaseManagementHandler(SubscriptionCloudCredentials credentials)
        {
            Credentials = credentials;
        }
    }
}
