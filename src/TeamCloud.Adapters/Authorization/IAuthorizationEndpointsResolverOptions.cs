using System;
using System.Collections.Generic;
using System.Text;

namespace TeamCloud.Adapters.Authorization
{
    public interface IAuthorizationEndpointsResolverOptions
    {
        public string BaseUrl { get; }
    }
}
