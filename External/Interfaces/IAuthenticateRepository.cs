﻿using Muonroi.BuildingBlock.External.Common.Models.Requests.Login;
using Muonroi.BuildingBlock.External.Common.Models.Responses.Login;

namespace Muonroi.BuildingBlock.External.Interfaces
{
    public interface IAuthenticateRepository : IMRepository<MUser>
    {
        Task<MResponse<LoginResponseModel>> Login(LoginRequestModel model, CancellationToken cancellationToken);
        Task<MResponse<RefreshTokenResponseModel>> RefreshToken(RefreshTokenRequestModel request, CancellationToken cancellationToken);
    }
}