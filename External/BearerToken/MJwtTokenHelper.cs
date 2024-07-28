namespace MBuildingBlock.External.BearerToken
{
    public static class MJwtTokenHelper
    {
        public static string GenerateJwtToken(MTokenInfo jwtConfig, MUserModel user, DateTime? expiresTime)
        {
            try
            {
                List<Claim> claims =
                [
                    new Claim("user_id", user.UserId),
                    new Claim("user_guid", user.UserGuid),
                    new Claim("username", user.Username),
                    new Claim(JwtRegisteredClaimNames.Iss, jwtConfig.Issuer),
                    new Claim(JwtRegisteredClaimNames.Aud, jwtConfig.Audience),
                ];
                foreach (string role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                RSA rsa = RSA.Create();
                rsa.ImportFromPem(jwtConfig.PrivateKey.ToCharArray());

                RsaSecurityKey signingKey = new(rsa);
                SigningCredentials credentials = new(signingKey, SecurityAlgorithms.RsaSha256);

                SecurityTokenDescriptor tokenDescriptor = new()
                {
                    Issuer = jwtConfig.Issuer,
                    Audience = jwtConfig.Audience,
                    Claims = new Dictionary<string, object>
                    {
                        { "user_id", user.UserId },
                        { "user_guid", user.UserGuid },
                        { "username", user.Username }
                    },
                    Expires = expiresTime ?? DateTime.UtcNow.AddMinutes(jwtConfig.ExpiryMinutes),
                    SigningCredentials = credentials
                };

                JwtSecurityTokenHandler tokenHandler = new();
                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
                string resultToken = tokenHandler.WriteToken(token);

                return resultToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                throw;
            }
        }
    }
}