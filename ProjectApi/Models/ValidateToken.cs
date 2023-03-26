using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProjectApi.Models
{
    public class ValidateToken
    {
        public static ClaimsPrincipal ValidToken(string jwtToken)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes("m6WbPxmgFfbMgmdhCu2rmdrb1Cz264Ou");
            var tokenValidateParam = new TokenValidationParameters
            {
                //tự cấp token
                ValidateIssuer = false,
                ValidateAudience = false,

                //ký vào token
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),

                ClockSkew = TimeSpan.Zero,

                ValidateLifetime = false //ko kiểm tra token hết hạn
            };
            ClaimsPrincipal principal = jwtTokenHandler.ValidateToken(jwtToken, tokenValidateParam, out var validatedToken);

            return principal;
        }

    }
}
