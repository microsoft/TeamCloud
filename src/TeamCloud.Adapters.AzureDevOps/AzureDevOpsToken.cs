/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Serialization;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters.AzureDevOps;

public sealed class AzureDevOpsToken : AuthorizationToken
{
    internal static string FormatOrganizationUrl(string organization)
    {
        if (string.IsNullOrWhiteSpace(organization))
            return null;

        if (Uri.TryCreate(organization, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            return organization;

        return $"https://dev.azure.com/{organization}";
    }

    private static DateTime? GetTokenExpirationDate(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            return new JwtSecurityTokenHandler()
                .ReadJwtToken(token)
                .ValidTo;
        }
        catch
        {
            return null;
        }
    }

    public AzureDevOpsToken() : this(null)
    { }

    public AzureDevOpsToken(DeploymentScope deployementScope) : base(GetEntityId(deployementScope))
    { }

    private string organization;

    [DataMember(Name = "organization")]
    public string Organization
    {
        get => FormatOrganizationUrl(organization);
        set => organization = value;
    }

    public string PersonalAccessToken { get; set; }

    [DataMember(Name = "client_id")]
    public string ClientId { get; set; }

    [DataMember(Name = "client_secret")]
    public string ClientSecret { get; set; }

    [DataMember(Name = "access_token")]
    public string AccessToken { get; set; }

    [IgnoreDataMember]
    public DateTime? AccessTokenExpires
        => string.IsNullOrEmpty(PersonalAccessToken) ? null : GetTokenExpirationDate(AccessToken);

    [IgnoreDataMember]
    public bool AccessTokenExpired
        => !AccessTokenExpires.HasValue || AccessTokenExpires < DateTime.UtcNow;

    [DataMember(Name = "refresh_token")]
    public string RefreshToken { get; set; }

    [IgnoreDataMember]
    public DateTime? RefreshTokenExpires
        => string.IsNullOrEmpty(PersonalAccessToken) ? null : GetTokenExpirationDate(RefreshToken);

    [IgnoreDataMember]
    public bool RefreshTokenExpired
        => !RefreshTokenExpires.HasValue || RefreshTokenExpires < DateTime.UtcNow;

    [IgnoreDataMember]
    public string RefreshCallback { get; set; }
}
