namespace Muonroi.BuildingBlock.Internal.Startup.Configurations
{
    internal class APIVersionErrorResponseProvider : IErrorResponseProvider
    {
        public IActionResult CreateResponse(ErrorResponseContext context)
        {
            CustomBadRequest error = new(context);
            return new BadRequestObjectResult(error);
        }
    }
}