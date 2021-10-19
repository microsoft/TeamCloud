using System;

namespace TeamCloud.API.Swagger
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class SwaggerIgnoreAttribute : Attribute
    {
    }
}
