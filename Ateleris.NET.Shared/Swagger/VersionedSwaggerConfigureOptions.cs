using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ateleris.NET.Shared.Swagger;

public class VersionedSwaggerConfigureOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var desc in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                desc.GroupName,
                new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "API Doc",
                    Version = desc.ApiVersion.ToString(),
                }
            );
        }
    }
}
