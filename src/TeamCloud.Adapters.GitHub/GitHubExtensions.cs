/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Jose;
using Newtonsoft.Json.Linq;
using Octokit;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using TeamCloud.Azure.Directory;
using TeamCloud.Serialization;

namespace TeamCloud.Adapters.GitHub
{
    internal static class GitHubExtensions
    {
        private static readonly long TicksSince1970101 = new DateTime(1970, 1, 1).Ticks;

        internal static string GetPemToken(this GitHubToken token)
        {
            var utcNow = DateTime.UtcNow;

            using var rsa = new RSACryptoServiceProvider();

            rsa.ImportParameters(GetRSAParameters());

            return JWT.Encode(new Dictionary<string, object>
            {
                {"iat", ToUtcSeconds(utcNow)},
                {"exp", ToUtcSeconds(utcNow.AddMinutes(1))},
                {"iss", token.ApplicationId}

            }, rsa, JwsAlgorithm.RS256);

            static long ToUtcSeconds(DateTime dt)
                => (dt.ToUniversalTime().Ticks - TicksSince1970101) / TimeSpan.TicksPerSecond;

            RSAParameters GetRSAParameters()
            {
                using var privateKeyReader = new StringReader(token.Pem);

                var pemReader = new PemReader(privateKeyReader);
                var pemKeyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
                var privateKey = (RsaPrivateCrtKeyParameters)pemKeyPair.Private;

                var rp = new RSAParameters
                {
                    Modulus = privateKey.Modulus.ToByteArrayUnsigned(),
                    Exponent = privateKey.PublicExponent.ToByteArrayUnsigned(),
                    P = privateKey.P.ToByteArrayUnsigned(),
                    Q = privateKey.Q.ToByteArrayUnsigned()
                };

                rp.D = ConvertRSAParametersField(privateKey.Exponent, rp.Modulus.Length);
                rp.DP = ConvertRSAParametersField(privateKey.DP, rp.P.Length);
                rp.DQ = ConvertRSAParametersField(privateKey.DQ, rp.Q.Length);
                rp.InverseQ = ConvertRSAParametersField(privateKey.QInv, rp.Q.Length);

                return rp;
            }

            static byte[] ConvertRSAParametersField(BigInteger n, int size)
            {
                byte[] bs = n.ToByteArrayUnsigned();

                if (bs.Length == size)
                    return bs;

                if (bs.Length > size)
                    throw new ArgumentException("Specified size too small", nameof(size));

                byte[] padded = new byte[size];

                Array.Copy(bs, 0, padded, size - bs.Length, bs.Length);

                return padded;
            }
        }

        internal static IFlurlRequest WithGitHubCredentials(this string url, Credentials credentials)
            => new FlurlRequest(url).WithGitHubCredentials(credentials);

        internal static IFlurlRequest WithGitHubCredentials(this Url url, Credentials credentials)
            => new FlurlRequest(url).WithGitHubCredentials(credentials);

        internal static T WithGitHubCredentials<T>(this T clientOrRequest, Credentials credentials)
            where T : IHttpSettingsContainer
        {
            if (clientOrRequest is null)
                throw new ArgumentNullException(nameof(clientOrRequest));

            if (credentials is null)
                throw new ArgumentNullException(nameof(credentials));

            return credentials.AuthenticationType switch
            {
                AuthenticationType.Basic => clientOrRequest.WithBasicAuth(credentials.Login, credentials.Password),
                AuthenticationType.Bearer => clientOrRequest.WithOAuthBearerToken(credentials.Password),
                AuthenticationType.Oauth => clientOrRequest.WithOAuthBearerToken(credentials.Password),
                _ => clientOrRequest
            };
        }



        internal static IFlurlRequest WithGitHubHeaders(this string url, string acceptHeader = null)
            => new FlurlRequest(url).WithGitHubHeaders(acceptHeader);

        internal static IFlurlRequest WithGitHubHeaders(this Url url, string acceptHeader = null)
            => new FlurlRequest(url).WithGitHubHeaders(acceptHeader);

        internal static T WithGitHubHeaders<T>(this T clientOrRequest, string acceptHeader = null)
            where T : IHttpSettingsContainer
        {
            const string ACCEPT_HEADER_PREFIX = "application/vnd.github.";

            if (clientOrRequest is null)
                throw new ArgumentNullException(nameof(clientOrRequest));

            // do some accept header sanitization
            acceptHeader = acceptHeader?.Trim()?.ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(acceptHeader))
            {
                acceptHeader = AcceptHeaders.StableVersionJson;
            }
            else if (!acceptHeader.StartsWith(ACCEPT_HEADER_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                acceptHeader = ACCEPT_HEADER_PREFIX + acceptHeader;

                if (acceptHeader.EndsWith("-preview", StringComparison.OrdinalIgnoreCase))
                    acceptHeader += "+json";
            }

            return clientOrRequest
                .WithHeader("Accept", acceptHeader)
                .WithHeader("User-Agent", GitHubConstants.ProductHeader);
        }

        internal static async Task<bool> IsEmpty(this IRepositoriesClient client, long repositoryId)
        {
            if (client is null)
                throw new ArgumentNullException(nameof(client));

            try
            {
                // to avoid large response payloads we restrict the amount
                // of data returned to  a maximum of one page with one record

                var options = new ApiOptions
                {
                    PageCount = 1,
                    PageSize = 1,
                };

                var commits = await client.Commit
                    .GetAll(repositoryId, options)
                    .ConfigureAwait(false);

                return !commits.Any();
            }
            catch (ApiException exc) when (exc.HttpResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return true; // for whatever reason, github returns a 'conflict' when there is no commit available
            }
        }

        internal static string ToJson(this AzureServicePrincipal servicePrincipal, Guid subscriptionId = default)
        {
            var data = new Dictionary<string, string>()
            {
                { "clientId" , servicePrincipal.ApplicationId.ToString() },
                { "clientSecret" , servicePrincipal.Password },
                { "tenantId", servicePrincipal.TenantId.ToString() }
            };

            if (subscriptionId != default)
            {
                data.Add("subscriptionId", subscriptionId.ToString());
            }

            return TeamCloudSerialize.SerializeObject(data);
        }
    }
}
