using System;
using System.Threading;
using System.Threading.Tasks;
using Enferno.Public.Azure.Management.Authentication.TokenCredentialConfigurations;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure;

namespace Enferno.Public.Azure.Management.Authentication
{
    /// <summary>
    /// Provides shortcuts for creating instances of the 
    /// Token Credential using ADAL.
    /// </summary>
    public class TokenCredentialHelper
    {
        private readonly ITokenCredentialConfiguration _tokenConfiguration;

        public TokenCredentialHelper(ITokenCredentialConfiguration tokenConfiguration)
        {
            _tokenConfiguration = tokenConfiguration;
        }

        /// <summary>
        /// Hands back the credential. 
        /// 
        /// Credentials don't need to belong to a specific subscription
        /// a subscription needs to be accessed. In that case, the 
        /// AAD tenant & app need to be "blessed," or the app needs 
        /// to be accessing assets in the same subscription. 
        /// 
        /// Calling code can create a general-purpose credential, 
        /// mainly to be used to get a list of subscriptions.
        /// 
        /// Once the desired subscription is found, the token can be 
        /// re-used in conjunction with a subscription ID, to provide
        /// direct management access via the Azure API to manage
        /// assets that are under that same subscription.
        /// </summary>
        /// <returns></returns>
        public async Task<TokenCloudCredentials> GetCredentials()
        {
            var token = GetAuthorizationHeader();
            var credential = new TokenCloudCredentials(token);
            var subscription = await SubscriptionLocator.SelectSubscription(credential, _tokenConfiguration.SubscriptionId);
            return new TokenCloudCredentials(subscription.SubscriptionId, credential.Token);
        }

        private string GetAuthorizationHeader()
        {
            AuthenticationResult result = null;

            var tenantId = _tokenConfiguration.TenantId;

            var context = new AuthenticationContext(
                string.Format("https://login.windows.net/{0}",
                    tenantId));

            var thread = new Thread(() =>
            {
                result = context.AcquireToken(
                    clientId: _tokenConfiguration.ClientId,
                    redirectUri: new Uri(_tokenConfiguration.RedirectUrl),
                    resource: "https://management.core.windows.net/",
                    promptBehavior: PromptBehavior.Auto);
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = "AquireTokenThread";
            thread.Start();
            thread.Join();
            return result.CreateAuthorizationHeader().Substring("Bearer ".Length);
        }


    }
}
