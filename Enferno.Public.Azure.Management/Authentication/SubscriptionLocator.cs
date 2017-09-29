using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Subscriptions;
using Microsoft.WindowsAzure.Subscriptions.Models;

namespace Enferno.Public.Azure.Management.Authentication
{
    /// <summary>
    /// The subscription portion of the demo.
    /// </summary>
    public class SubscriptionLocator
    {
        public async static Task<SubscriptionListOperationResponse.Subscription>
            SelectSubscription(SubscriptionCloudCredentials credentials,
            string subscriptionId)
        {
            IEnumerable<SubscriptionListOperationResponse.Subscription> ret = null;

            using (var subscriptionClient = new SubscriptionClient(credentials))
            {
                try
                {
                    var listSubscriptionResults =
                        await subscriptionClient.Subscriptions.ListAsync();


                    var subscriptions = listSubscriptionResults.Subscriptions;

                    ret = subscriptions;
                }
                catch (Exception)
                {
                }
            }

            return ret.First(x => x.SubscriptionId == subscriptionId);
        }
    }
}
