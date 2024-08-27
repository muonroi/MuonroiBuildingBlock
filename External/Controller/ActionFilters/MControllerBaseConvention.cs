namespace Muonroi.BuildingBlock.External.Controller.ActionFilters
{
    public class MControllerBaseConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (ControllerModel controller in application.Controllers)
            {
                if (typeof(MControllerBase).IsAssignableFrom(controller.ControllerType))
                {
                    foreach (ActionModel action in controller.Actions)
                    {
                        Type returnType = action.ActionMethod.ReturnType;
                        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                        {
                            Type genericArgument = returnType.GetGenericArguments().First();

                            if (genericArgument.IsGenericType && genericArgument.GetGenericTypeDefinition() == typeof(MResponse<>))
                            {
                                Type responseType = genericArgument.GetGenericArguments().First();
                                action.Filters.Add(new ProducesResponseTypeAttribute(typeof(MResponse<>).MakeGenericType(responseType), (int)HttpStatusCode.OK));
                            }
                        }

                        action.Filters.Add(new ProducesResponseTypeAttribute(typeof(MVoidMethodResult), (int)HttpStatusCode.BadRequest));
                    }
                }
            }
        }
    }
}