namespace MBuildingBlock.External.Controller
{
    public class MControllerBase(IMediator mediator) : ControllerBase
    {
        protected IMediator Mediator => mediator;
    }
}