using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RestaurantBackend.API.Filters;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParameters = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IFormFile[]))
            .ToList();

        if (!fileParameters.Any())
            return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>(),
                        Required = new HashSet<string>()
                    }
                }
            }
        };

        var schema = operation.RequestBody.Content["multipart/form-data"].Schema;

        // Add all parameters from the method
        foreach (var parameter in context.MethodInfo.GetParameters())
        {
            if (parameter.ParameterType == typeof(IFormFile))
            {
                schema.Properties[parameter.Name!] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                    Description = "Upload file"
                };
            }
            else if (parameter.ParameterType == typeof(IFormFile[]))
            {
                schema.Properties[parameter.Name!] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    },
                    Description = "Upload files"
                };
            }
            else
            {
                // Add DTO properties
                var dtoProperties = parameter.ParameterType.GetProperties();
                foreach (var prop in dtoProperties)
                {
                    var propType = prop.PropertyType;
                    var underlyingType = Nullable.GetUnderlyingType(propType) ?? propType;

                    schema.Properties[prop.Name] = new OpenApiSchema
                    {
                        Type = GetOpenApiType(underlyingType),
                        Format = GetOpenApiFormat(underlyingType),
                        Nullable = propType != underlyingType
                    };
                }
            }
        }
    }

    private string GetOpenApiType(Type type)
    {
        if (type == typeof(int) || type == typeof(long) || type == typeof(short))
            return "integer";
        if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
            return "number";
        if (type == typeof(bool))
            return "boolean";
        return "string";
    }

    private string? GetOpenApiFormat(Type type)
    {
        if (type == typeof(int))
            return "int32";
        if (type == typeof(long))
            return "int64";
        if (type == typeof(float))
            return "float";
        if (type == typeof(double))
            return "double";
        if (type == typeof(decimal))
            return "double";
        if (type == typeof(DateTime))
            return "date-time";
        return null;
    }
}
