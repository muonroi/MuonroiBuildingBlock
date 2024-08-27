namespace Muonroi.BuildingBlock.External.Default
{
    public class SwaggerDefaultValues : IOperationFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription apiDescription = context.ApiDescription;

            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1752#issue-663991077
            foreach (Microsoft.AspNetCore.Mvc.ApiExplorer.ApiResponseType responseType in context.ApiDescription.SupportedResponseTypes)
            {
                // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/b7cf75e7905050305b115dd96640ddd6e74c7ac9/src/Swashbuckle.AspNetCore.SwaggerGen/SwaggerGenerator/SwaggerGenerator.cs#L383-L387
                string responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
                OpenApiResponse response = operation.Responses[responseKey];

                foreach (string? contentType in response.Content.Keys)
                {
                    if (!responseType.ApiResponseFormats.Any(x => x.MediaType == contentType))
                    {
                        _ = response.Content.Remove(contentType);
                    }
                }
            }

            if (operation.Parameters == null)
            {
                return;
            }

            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/413
            foreach (OpenApiParameter? parameter in operation.Parameters)
            {
                Microsoft.AspNetCore.Mvc.ApiExplorer.ApiParameterDescription description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

                parameter.Description ??= description.ModelMetadata?.Description;

                if (parameter.Schema.Default == null &&
                     description.DefaultValue != null &&
                     description.DefaultValue is not DBNull &&
                     description.ModelMetadata is ModelMetadata modelMetadata)
                {
                    // REF: https://github.com/Microsoft/aspnet-api-versioning/issues/429#issuecomment-605402330
                    string json = JsonSerializer.Serialize(description.DefaultValue, modelMetadata.ModelType);
                    parameter.Schema.Default = OpenApiAnyFactory.CreateFromJson(json);
                }

                parameter.Required |= description.IsRequired;
            }
        }
    }
}