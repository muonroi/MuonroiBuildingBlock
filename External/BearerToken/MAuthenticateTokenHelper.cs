namespace Muonroi.BuildingBlock.External.BearerToken
{
    public static class MAuthenticateTokenHelper
    {
        public static string GenerateAuthenticateToken(MTokenInfo tokenConfig, MUserModel user, DateTime? expiresTime = null)
        {
            try
            {
                List<Claim> claims =
                [
                    new Claim("user_id", user.UserId),
                    new Claim("user_guid", user.UserGuid),
                    new Claim("username", user.Username),
                    new Claim(JwtRegisteredClaimNames.Iss, tokenConfig.Issuer),
                    new Claim(JwtRegisteredClaimNames.Aud, tokenConfig.Audience),
                ];

                // Add roles to claims
                foreach (string role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                using RSA rsa = RSA.Create();
                rsa.ImportFromPem(tokenConfig.PrivateKey.ToCharArray());

                RsaSecurityKey signingKey = new(rsa);
                SigningCredentials credentials = new(signingKey, SecurityAlgorithms.RsaSha256);

                SecurityTokenDescriptor tokenDescriptor = new()
                {
                    Issuer = tokenConfig.Issuer,
                    Audience = tokenConfig.Audience,
                    Subject = new ClaimsIdentity(claims),
                    Expires = expiresTime ?? DateTime.UtcNow.AddMinutes(tokenConfig.ExpiryMinutes),
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
    }
}