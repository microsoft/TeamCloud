using System;
using System.Collections.Generic;
using System.Text;

namespace TeamCloud.Adapters.Authorization
{
    public sealed class AuthorizationEndpoints : IAuthorizationEndpoints
    {
        public string AuthorizationUrl { get; internal set; }

        public string CallbackUrl { get; internal set; }
    }
}
