

namespace Muonroi.BuildingBlock.External.BearerToken;

public class MAuthenticateTokenHelper<TPermission> where TPermission : Enum
{
    private readonly MTokenInfo _tokenConfig;
    private readonly RSA _rsa;

    public MAuthenticateTokenHelper(MTokenInfo tokenConfig)
    {
        _tokenConfig = tokenConfig ?? throw new ArgumentNullException(nameof(tokenConfig));

        _rsa = RSA.Create();

        _rsa.ImportFromPem(_tokenConfig.PrivateKey.ToCharArray());
    }

    public string GenerateAuthenticateToken(MUserModel user, List<TPermission> permissions, DateTime? expiresTime = null, List<Claim>? claims = null)
    {
        try
        {
            List<Claim> privateClaims = claims ??
            [
                new Claim(ClaimConstants.Username, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserGuid),
            ];

            long permissionsBitmask = MPermissionExtension<TPermission>.CalculatePermissionsBitmask(permissions);

            privateClaims.Add(new Claim(ClaimConstants.Permission, permissionsBitmask.ToString()));


            RsaSecurityKey signingKey = new(_rsa);
            SigningCredentials credentials = new(signingKey, SecurityAlgorithms.RsaSha256);

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Issuer = _tokenConfig.Issuer,
                Audience = _tokenConfig.Audience,
                Subject = new ClaimsIdentity(claims),
                Expires = expiresTime ?? DateTime.UtcNow.AddMinutes(_tokenConfig.ExpiryMinutes),
                SigningCredentials = credentials
            };

            JwtSecurityTokenHandler tokenHandler = new();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.Message}");
            throw;
        }
    }

    public void Dispose()
    {
        _rsa.Dispose();
    }
}