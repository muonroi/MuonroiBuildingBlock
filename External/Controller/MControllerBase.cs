namespace Muonroi.BuildingBlock.External.Controller
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public abstract class MControllerBase(IMediator mediator, ILogger logger, IMapper mapper) : ControllerBase
    {
        protected IMediator Mediator { get; } = mediator;

        protected ILogger Logger { get; } = logger;

        protected IMapper Mapper { get; } = mapper;

    }
}