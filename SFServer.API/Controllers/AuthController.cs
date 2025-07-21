using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SFServer.API.Data;
using SFServer.Shared.Client.Auth;
using SFServer.Shared.Server.Auth;
using SFServer.Shared.Server.Google;
using SFServer.Shared.Server.UserProfile;
using SFServer.Shared.Server.Admin;
using shortid;
using shortid.Configuration;

namespace SFServer.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly DatabseContext _db;

        public AuthController(IConfiguration config, DatabseContext db)
        {
            _config = config;
            _db = db;
        }

        [HttpPost("login-dashboard")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDashboardRequest request)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(request.Credential))
                return BadRequest(ModelState);

            Administrator admin = null;

            if (Guid.TryParse(request.Credential, out Guid id))
            {
                admin = await _db.Administrators.FirstOrDefaultAsync(a => a.Id == id);
            }

            if (admin == null && request.Credential.Contains("@"))
            {
                admin = await _db.Administrators.FirstOrDefaultAsync(a => a.Email.ToLower() == request.Credential.ToLower());
            }

            if (admin == null)
            {
                admin = await _db.Administrators.FirstOrDefaultAsync(a => a.Username.ToLower() == request.Credential.ToLower());
            }

            if (admin == null)
                return Unauthorized("User not found");

            var hasher = new PasswordHasher<Administrator>();
            var result = hasher.VerifyHashedPassword(admin, admin.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid password");

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, admin.Username),
                new(ClaimTypes.Role, UserRole.Admin.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT_SECRET"]));
            var expirationDate = GetExpirationDate();
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expirationDate,
                signingCredentials: creds
            );
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            await _db.SaveChangesAsync();

            var response = new DashboardLoginResponse
            {
                UserId = admin.Id,
                Username = admin.Username,
                Email = admin.Email,
                Role = UserRole.Admin,
                ExpirationDate = expirationDate,
                JwtToken = jwtToken
            };

            return Ok(response);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
        
            if (string.IsNullOrEmpty(request.Credential) && string.IsNullOrEmpty(request.DeviceId))
            {
                return await AnonymousLogin();
            }

            UserProfile user = null;

            // Try to interpret the credential as an ID (integer)
            if (Guid.TryParse(request.Credential, out var id))
            {
                user = await _db.UserProfiles.FirstOrDefaultAsync(u => u.Id == id);
            }

            // If not found and if the credential contains '@', try email lookup.
            if (user == null && request.Credential.Contains('@'))
            {
                user = await _db.UserProfiles.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Credential.ToLower());
            }

            // If still not found, try username lookup.
            if (user == null)
            {
                user = await _db.UserProfiles.FirstOrDefaultAsync(u => u.Username.ToLower() == request.Credential.ToLower());
            }

            if (user == null && string.IsNullOrEmpty(request.DeviceId) == false)
            {
                user = await _db.UserProfiles.FirstOrDefaultAsync(u => u.DeviceIds != null && u.DeviceIds.Contains(request.DeviceId));
            }
        
            if (user == null)
            {
                return await AnonymousLogin(request);
            }


            // Create JWT token.
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT_SECRET"]));
            var expirationDate = GetExpirationDate();
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expirationDate,
                signingCredentials: creds
            );
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            user.LastLoginAt = DateTime.UtcNow;

            user.DeviceIds ??= new List<string> { };

            if (string.IsNullOrWhiteSpace(request.DeviceId) == false && user.DeviceIds.Contains(request.DeviceId) == false)
            {
                user.DeviceIds.Add(request.DeviceId);
            }
        
            var device = await _db.UserDevices.FirstOrDefaultAsync(x => x.DeviceId == request.DeviceId);
            
            if (device == null)
            {
                var userDevice = new UserDevice
                {
                    Id = Guid.CreateVersion7(),
                    DeviceId = request.DeviceId,
                    UserId = user.Id
                };
                userDevice.SetInfo(request.DeviceInfo);
                await _db.UserDevices.AddAsync(userDevice);
            }
        
            await _db.SaveChangesAsync();

            var response = new LoginResponse
            {
                Index = user.Index,
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                ExpirationDate = expirationDate,
                JwtToken = jwtToken
            };

            return Ok(response);
        }

        private DateTime GetExpirationDate()
        {
            return DateTime.UtcNow.AddHours(int.TryParse(_config["JWT_TIMEOUT_HOURS"], out var hours) ? hours : 1);
        }

        private string GenerateUsername()
        {
            return $"Guest{ShortId.Generate(new GenerationOptions(length: 8, useSpecialCharacters: false, useNumbers: true)).ToUpper()}";
        }

        private async Task<IActionResult> AnonymousLogin(LoginRequestBase loginRequestBase = null)
        {
            var user = new UserProfile
            {
                Id = Guid.CreateVersion7(),
                Username = GenerateUsername(),
                Email = string.Empty, // Email not required
                Role = UserRole.Guest, // Regular user role
                CreatedAt = DateTime.UtcNow,
                LastEditAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            if (loginRequestBase != null)
            {
                var deviceId = loginRequestBase.DeviceId;
            
                if (!string.IsNullOrWhiteSpace(deviceId))
                {
                    user.DeviceIds.Add(deviceId);
                }
        
                var device = await _db.UserDevices.FirstOrDefaultAsync(x => x.DeviceId == deviceId);
            
                if (device == null)
                {
                    var userDevice = new UserDevice();
                    userDevice.DeviceId = deviceId;
                    userDevice.UserId = user.Id;
                    userDevice.SetInfo(loginRequestBase.DeviceInfo);
                    await _db.UserDevices.AddAsync(userDevice);
                }
            }
        
  
        
            _db.UserProfiles.Add(user);
            await _db.SaveChangesAsync();

            // Generate JWT token
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT_SECRET"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expirationDate = GetExpirationDate();
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expirationDate,
                signingCredentials: creds
            );
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            var response = new LoginResponse
            {
                Index = user.Index,
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                ExpirationDate = expirationDate,
                JwtToken = jwtToken
            };

            return Ok(response);
        }

        private async Task<TokenResponse> ExchangeCodeForToken(string authorizationCode)
        {
            using var client = new HttpClient();
            var response = await client.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", authorizationCode),
                new KeyValuePair<string, string>("client_id", _config["GOOGLE_CLIENT_ID"]),
                new KeyValuePair<string, string>("client_secret", _config["GOOGLE_CLIENT_SECRET"]),
                new KeyValuePair<string, string>("redirect_uri", ""),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
            }));

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to exchange code: {json}");

            return JsonConvert.DeserializeObject<TokenResponse>(json);
        }
    
    
        // Метод для получения информации об игроке с использованием access token
        private async Task<string> GetPlayerInfoAsync(string accessToken, string playerId)
        {
            using var client = new HttpClient();

            // Устанавливаем заголовок авторизации с Bearer-токеном
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Пример URL для запроса информации об игроке
            // Если вы используете Google Play Games API, endpoint может выглядеть так:
            var url = $"https://www.googleapis.com/games/v1/players/{playerId}";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception("Failed to retrieve player info: " + error);
            }

            // Возвращаем сырые данные (JSON)
            return await response.Content.ReadAsStringAsync();
        }

        [HttpPost("GooglePlayLogin")]
        [AllowAnonymous]
        public async Task<IActionResult> GooglePlayLogin([FromBody] GooglePlayLoginRequest request)
        {
        
            Console.WriteLine("Request: " + JsonConvert.SerializeObject(request));
        
            if (string.IsNullOrWhiteSpace(request.GoogleClientId) || string.IsNullOrWhiteSpace(request.AuthCode))
                return BadRequest("Missing PlayerId or AuthCode");

            // Проверка действительности кода авторизации и обмен на токен
            TokenResponse token;
            try
            {
                token = await ExchangeCodeForToken(request.AuthCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid Google Play AuthCode: " + ex.Message);
                return BadRequest("Invalid Google Play AuthCode: " + ex.Message);
            }

            // (Опционально) Получаем информацию об игроке через Google Play Games API
            GooglePlayerInfo playerInfo = null;
            try
            {
                var playerInfoJson = await GetPlayerInfoAsync(token.AccessToken, request.GoogleClientId);
            
                playerInfo = JsonConvert.DeserializeObject<GooglePlayerInfo>(playerInfoJson);
            
                Console.WriteLine("Player Info: " + JsonConvert.SerializeObject(playerInfoJson));
            }
            catch (Exception ex)
            {
                // В зависимости от логики, можно вернуть ошибку или продолжить, если эта информация не критична
                Console.WriteLine("Warning: Failed to retrieve player info: " + ex.Message);
            }

            // Поиск пользователя по PlayerId
            var user = await _db.UserProfiles.FirstOrDefaultAsync(u => u.GooglePlayId == request.GoogleClientId);

            if (user == null)
            {
                user = new UserProfile
                {
                    Id = Guid.CreateVersion7(),
                    Username = playerInfo == null ? GenerateUsername() : playerInfo.DisplayName,
                    Role = UserRole.User,
                    CreatedAt = DateTime.UtcNow,
                    LastEditAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    GooglePlayId = request.GoogleClientId,
                    DebugMode = false
                };
            
                user.DeviceIds ??= new List<string> { };
            
                if (string.IsNullOrWhiteSpace(request.DeviceId) == false && user.DeviceIds.Contains(request.DeviceId) == false)
                {
                    user.DeviceIds.Add(request.DeviceId);
                }

                _db.UserProfiles.Add(user);
            }
            else
            {
                user.DeviceIds ??= new List<string> { };
            
                if (string.IsNullOrWhiteSpace(request.DeviceId) == false && user.DeviceIds.Contains(request.DeviceId) == false)
                {
                    user.DeviceIds.Add(request.DeviceId);
                }
            
                user.LastLoginAt = DateTime.UtcNow;
            }

            if (string.IsNullOrWhiteSpace(request.DeviceId) == false)
            {
                var device = await _db.UserDevices.FirstOrDefaultAsync(x => x.DeviceId == request.DeviceId);

                if (device == null)
                {
                    var userDevice = new UserDevice
                    {
                        Id = Guid.CreateVersion7(),
                        DeviceId = request.DeviceId,
                        UserId = user.Id
                    };
                    userDevice.SetInfo(request.DeviceInfo);
                    await _db.UserDevices.AddAsync(userDevice);
                }
            }

            await _db.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, user.Role.ToString()),
                new("UserId", user.Id.ToString())
            };

            var expirationDate = GetExpirationDate();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT_SECRET"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expirationDate,
                signingCredentials: creds
            );
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(jwt);

            return Ok(new LoginResponse
            {
                Index = user.Index,
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                ExpirationDate = expirationDate,
                JwtToken = jwtToken
            });
        }


        [HttpPost("LinkGooglePlay")]
        [Authorize]
        public async Task<IActionResult> LinkGooglePlay([FromBody] GooglePlayLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.GoogleClientId) || string.IsNullOrWhiteSpace(request.AuthCode))
                return BadRequest("Missing PlayerId or AuthCode");

            // Проверяем AuthCode через Google
            try
            {
                await ExchangeCodeForToken(request.AuthCode);
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid Google Play AuthCode: " + ex.Message);
            }

            // Получаем текущего пользователя
            var userIdValue = Request.Headers[Headers.UID].FirstOrDefault();
            if (string.IsNullOrEmpty(userIdValue))
            {
                userIdValue = User.FindFirst("UserId")?.Value;
            }
            if (!Guid.TryParse(userIdValue, out Guid userId))
                return Unauthorized("Invalid or missing UserId.");

            var user = await _db.UserProfiles.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            // Проверяем, не привязан ли этот PlayerId к другому профилю
            var existingUser = await _db.UserProfiles
                .FirstOrDefaultAsync(u => u.GooglePlayId == request.GoogleClientId && u.Id != user.Id);
            if (existingUser != null)
                return BadRequest("This Google Play account is already linked to another profile.");

            // Связываем PlayerId
            user.GooglePlayId = request.GoogleClientId;

            if (user.Role == UserRole.Guest)
                user.Role = UserRole.User;

            await _db.SaveChangesAsync();

            return Ok("Google Play account linked successfully.");
        }
    }
}