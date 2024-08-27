namespace Muonroi.BuildingBlock.External.Controller
{
    public abstract class MControllerBase(IMediator mediator, ILogger logger) : ControllerBase
    {
        protected IMediator Mediator { get; } = mediator;
        protected ILogger Logger { get; } = logger;
    }
}