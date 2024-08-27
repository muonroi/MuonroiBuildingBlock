namespace Muonroi.BuildingBlock.External.Controller.ActionFilters
{
    public class DefaultProducesResponseTypeFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor controllerActionDescriptor &&
                typeof(MControllerBase).IsAssignableFrom(controllerActionDescriptor.ControllerTypeInfo))
            {
                context.HttpContext.Response.Headers.Append("X-DefaultProducesResponseType", "true");

                Type returnType = controllerActionDescriptor.MethodInfo.ReturnType;

                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    Type genericArgument = returnType.GetGenericArguments().First();

                    if (genericArgument.IsGenericType && genericArgument.GetGenericTypeDefinition() == typeof(MResponse<>))
                    {
                        Type responseType = genericArgument.GetGenericArguments().First();
                        context.ActionDescriptor.EndpointMetadata.Add(new ProducesResponseTypeAttribute(typeof(MResponse<>).MakeGenericType(responseType), (int)HttpStatusCode.OK));
                    }
                }

                context.ActionDescriptor.EndpointMetadata.Add(new ProducesResponseTypeAttribute(typeof(MVoidMethodResult), (int)HttpStatusCode.BadRequest));
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}