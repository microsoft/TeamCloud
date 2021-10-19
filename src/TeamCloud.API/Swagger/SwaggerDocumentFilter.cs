using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TeamCloud.API.Swagger
{
    internal sealed class SwaggerDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var apiDescription in context.ApiDescriptions)
            {
                if (apiDescription.TryGetMethodInfo(out var methodInfo) && methodInfo.HasCustomAttribute<SwaggerIgnoreAttribute>(false))
                {
                    swaggerDoc.Paths.Remove($"/{apiDescription.RelativePath}");
                }
            }
        }
    }
}
