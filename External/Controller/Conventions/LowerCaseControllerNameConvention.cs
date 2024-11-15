namespace Muonroi.BuildingBlock.External.Controller.Conventions
{
    public class LowerCaseControllerNameConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            string controllerName = controller.ControllerName;
            string lowerControllerName = char.ToLowerInvariant(controllerName[0]) + controllerName[1..];

            foreach (SelectorModel selector in controller.Selectors)
            {
                if (selector.AttributeRouteModel != null)
                {
                    selector.AttributeRouteModel.Template =
                        selector.AttributeRouteModel.Template?.Replace("[controller]", lowerControllerName);
                }
            }
        }
    }

}
