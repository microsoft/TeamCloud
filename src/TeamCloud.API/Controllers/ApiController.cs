using System;
using Microsoft.AspNetCore.Mvc;

namespace TeamCloud.API.Controllers
{
    public abstract class ApiController : ControllerBase
    {
        public string ProjectId
            => RouteData.Values.GetValueOrDefault(nameof(ProjectId), StringComparison.OrdinalIgnoreCase)?.ToString();

        public string UserId
            => User?.GetObjectId();
    }
}
