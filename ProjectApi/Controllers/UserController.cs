using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ProjectApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ProjectApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ProjectApiContext _dbContext;
        private readonly AppSettings _appSettings;
        public UserController(ProjectApiContext dbContext, IOptionsMonitor<AppSettings> optionsMonitor)
        {
            _dbContext = dbContext;
            _appSettings = optionsMonitor.CurrentValue;
        }

        [Authorize]
        [HttpGet("CheckLogin")]  
        public async Task<IActionResult> CheckLogin()
        {
            return Ok(new ApiResponse
            {
                Success = true, 
                Message = "Da dang nhap"
            });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Validate(LoginModel model)
        {
            var user = _dbContext.Accounts.SingleOrDefault(p => p.Username == model.UserName && model.Password == p.Password);

            if (user == null) //not found
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid username/password"
                });
            }

            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.CreatedAt = DateTime.UtcNow;
            user.ExpiredAt = DateTime.UtcNow.AddHours(1);

            await _dbContext.SaveChangesAsync();

            var accessToken = GenerateToken(user);
            //grant token

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Authenticate success",
                Data = new TokenModel
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                }
            });
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Registrer(Account account)
        {
            var ac = _dbContext.Accounts.SingleOrDefault(p => p.Username == account.Username);
            if (ac != null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Register successfully"
                });
            }
            if (account.Username.Length < 5 || account.Password.Length < 5 || account.DisplayName.Length == 0)
            {
                Console.WriteLine("da chay vao day");
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Username, PassWord, DisplayName do dai ko nho hon 5"
                });
            }
             
            var refreshToken = GenerateRefreshToken();
            account.RefreshToken = refreshToken;
            account.CreatedAt = DateTime.UtcNow;
            account.ExpiredAt = DateTime.UtcNow.AddHours(1);
            account.Role = 2;

            var accessToken = GenerateToken(account);

            _dbContext.Accounts.AddAsync(account);
            await _dbContext.SaveChangesAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Authenticate success",
                Data = new TokenModel
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                }
            });
        }
        private string GenerateToken(Account ac)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var secretKeyBytes = Encoding.UTF8.GetBytes(_appSettings.SecretKey);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, ac.DisplayName),
                    new Claim("UserName", ac.Username),
                    new Claim(ClaimTypes.Role, ac.Role.ToString()),
                    new Claim("Id", ac.Id.ToString()),
                    new Claim("TokenId", Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescription); 
            var accessToken = jwtTokenHandler.WriteToken(token);

            return accessToken;
        }

        private string GenerateRefreshToken()
        {
            var random = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);

                return Convert.ToBase64String(random);
            }
        }

        [HttpPost("RenewToken")]
        public async Task<IActionResult> RenewToken(TokenModel model)
        {

            var username = "";
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(_appSettings.SecretKey);
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
            try
            {
                //check 1: AccessToken valid format
                var tokenInVerification = jwtTokenHandler.ValidateToken(model.AccessToken, tokenValidateParam, out var validatedToken);
                
                //check 2: Check alg
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase);
                    //var tk = new JwtSecurityToken(jwtEncodedString: model.AccessToken);
                    //username = tk.Claims.First(c => c.Type == "UserName").Value;
        
                    if (!result)//false
                    {
                        return Ok(new ApiResponse
                        {
                            Success = false,
                            Message = "Invalid token",
                            Data = 3
                        });
                    }
                }

                username = tokenInVerification.Claims.FirstOrDefault(x => x.Type == "UserName").Value;

                //check 3: Check accessToken expire?
                var utcExpireDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expireDate = ConvertUnixTimeToDateTime(utcExpireDate);
                if (expireDate > DateTime.UtcNow)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Access token has not yet expired",
                        Data = 2
                    });
                }

                //check 4: Check refreshtoken exist in DB and expire
                var user = _dbContext.Accounts.FirstOrDefault(x => x.RefreshToken == model.RefreshToken && x.Username== username);
                if (user == null)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Refresh token does not exist",
                        Data = 3
                    });
                }
                if (user.ExpiredAt < DateTime.UtcNow)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Refresh token expired",
                        Data = 3
                    });
                }

                //create new token
                var token =  GenerateToken(user);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Renew token success",
                    Data = token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Something went wrong"
                });
            }
        }

        private DateTime ConvertUnixTimeToDateTime(long utcExpireDate)
        {
            var dateTimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeInterval.AddSeconds(utcExpireDate).ToUniversalTime();

            return dateTimeInterval;
        }
    }
}
