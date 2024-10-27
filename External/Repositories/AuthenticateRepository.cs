


namespace Muonroi.BuildingBlock.External.Repositories
{
    public class AuthenticateRepository<TDbContext, TPermission>(
    MAuthenticateTokenHelper<TPermission> tokenHelper,
    TDbContext dbContext,
    MAuthenticateInfoContext authContext,
    MTokenInfo mTokenInfo) : MRepository<MUser>(dbContext, authContext), IAuthenticateRepository
        where TDbContext : MDbContext
        where TPermission : Enum
    {
        private readonly TDbContext _dbContext = dbContext;

        private readonly MAuthenticateInfoContext _authContext = authContext;

        public async Task<MResponse<LoginResponseModel>> Login(LoginRequestModel request, CancellationToken cancellationToken)
        {
            MResponse<LoginResponseModel> result = new();

            if (IsRequestInvalid(request, out string? errorMessage))
            {
                result.AddApiErrorMessage(errorMessage, [request.Username]);
                return result;
            }

            MUser? existedUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == request.Username, cancellationToken: cancellationToken);

            if (existedUser is null)
            {
                result.AddApiErrorMessage(nameof(SystemEnum.InvalidCredentials), [request.Username]);
                return result;
            }

            result = await _dbContext.ResolveLoginAsync(request,
                result,
                existedUser,
                mTokenInfo,
                tokenHelper,
                cancellationToken);

            return result;
        }



        public async Task<MResponse<RefreshTokenResponseModel>> RefreshToken(RefreshTokenRequestModel request, CancellationToken cancellationToken)
        {
            MResponse<RefreshTokenResponseModel> result = new();

            MUser? existedUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == _authContext.CurrentUserGuid, cancellationToken: cancellationToken);

            if (existedUser is null)
            {
                result.AddApiErrorMessage(nameof(SystemEnum.InvalidCredentials), [_authContext.CurrentUsername]);
                return result;
            }

            result = await _dbContext.ResolveRefreshToken<TDbContext, TPermission>(request, result, existedUser, mTokenInfo, cancellationToken);

            return result;

        }

        private static bool IsRequestInvalid(LoginRequestModel request, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                errorMessage = nameof(SystemEnum.InvalidLoginInfo);
                return true;
            }
            return false;
        }
    }

}