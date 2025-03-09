using System;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

namespace SolidCqrsFramework.Aws
{
    public static class AwsUtils
    {
        private static readonly Lazy<string> _accountId = new Lazy<string>(() =>
        {
            // This client uses the default credentials and region.
            using var stsClient = new AmazonSecurityTokenServiceClient();
            var identity = stsClient.GetCallerIdentityAsync(new GetCallerIdentityRequest()).Result;
            return identity.Account;
        });

        public static string AccountId => _accountId.Value;
    }
}
