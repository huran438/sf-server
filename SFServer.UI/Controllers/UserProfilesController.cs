using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SFServer.Shared.Models.UserProfile;
using SFServer.Shared.Models.Wallet;
using SFServer.UI.Models.UserProfiles;

namespace SFServer.UI.Controllers
{
    public class UserProfilesController : Controller
    {
        private readonly IConfiguration _configuration;

        public UserProfilesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private HttpClient GetAuthenticatedHttpClient()
        {
            var client = new HttpClient { BaseAddress = new Uri(_configuration["API_BASE_URL"]) };
            var jwtToken = User.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value;
            if (!string.IsNullOrEmpty(jwtToken))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
            }
            else
            {
                Console.WriteLine("JWT token not found in user claims.");
            }

            return client;
        }


        public async Task<IActionResult> Index(int page = 1, string search = "", string sortColumn = "Id", string sortOrder = "asc")
        {
            try
            {
                using var client = GetAuthenticatedHttpClient();
                var profiles = await client.GetFromJsonAsync<List<UserProfile>>("UserProfiles");

                // Filter profiles by search query.
                if (!string.IsNullOrWhiteSpace(search))
                {
                    bool isNumeric = Guid.TryParse(search, out Guid searchId);
                    profiles = profiles.Where(p =>
                        p.Username.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        p.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (isNumeric && p.Id == searchId)
                    ).ToList();
                }

                // Apply sorting based on sortColumn and sortOrder.
                profiles = sortColumn switch
                {
                    "Username" => sortOrder == "asc" ? profiles.OrderBy(p => p.Username).ToList() : profiles.OrderByDescending(p => p.Username).ToList(),
                    "Email" => sortOrder == "asc" ? profiles.OrderBy(p => p.Email).ToList() : profiles.OrderByDescending(p => p.Email).ToList(),
                    "CreatedAt" => sortOrder == "asc" ? profiles.OrderBy(p => p.CreatedAt).ToList() : profiles.OrderByDescending(p => p.CreatedAt).ToList(),
                    "LastEditAt" => sortOrder == "asc" ? profiles.OrderBy(p => p.LastEditAt).ToList() : profiles.OrderByDescending(p => p.LastEditAt).ToList(),
                    "LastLoginAt" => sortOrder == "asc" ? profiles.OrderBy(p => p.LastLoginAt).ToList() : profiles.OrderByDescending(p => p.LastLoginAt).ToList(),
                    "Role" => sortOrder == "asc" ? profiles.OrderBy(p => p.Role).ToList() : profiles.OrderByDescending(p => p.Role).ToList(),
                    _ => profiles
                };

                // Pagination logic.
                int pageSize = 20;
                int totalProfiles = profiles.Count;
                int totalPages = (int)Math.Ceiling(totalProfiles / (double)pageSize);
                var pagedProfiles = profiles.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                // Build the view model.
                var model = new UserProfilesIndexViewModel
                {
                    Users = pagedProfiles,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    SearchQuery = search,
                    SortColumn = sortColumn,
                    SortOrder = sortOrder
                };

                return View(model);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return RedirectToAction("AccessDenied", "Account");
            }
        }


        // GET: /UserProfiles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /UserProfiles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Map view model to UserProfile entity.
            var newUser = new UserProfile
            {
                Username = model.Username,
                Email = model.Email,
                Role = model.Role,
                CreatedAt = DateTime.UtcNow
            };
            
            var hasher = new PasswordHasher<UserProfile>();
            newUser.PasswordHash = hasher.HashPassword(newUser, model.Password);

            using var client = GetAuthenticatedHttpClient();
            var response = await client.PostAsJsonAsync("UserProfiles", newUser);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Failed to add user. {errorContent}");
            return View(model);
        }

        // GET: /UserProfiles/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            using var httpClient = GetAuthenticatedHttpClient();
            var profile = await httpClient.GetFromJsonAsync<UserProfile>($"UserProfiles/{id}");
            if (profile == null)
            {
                return NotFound();
            }


            // Map UserProfile to EditUserProfileViewModel
            var viewModel = new EditUserProfileViewModel
            {
                Id = profile.Id,
                Username = profile.Username,
                Email = profile.Email,
                Role = profile.Role,
                GoogleId = profile.GooglePlayId,
                DebugMode = profile.DebugMode
            };

            // After retrieving the user's profile:
            var userId = profile.Id; // assuming this is the user ID
            var walletItems = await httpClient.GetFromJsonAsync<List<WalletItem>>($"Wallet/{userId}");

            // Map them to your view model.
            viewModel.WalletItems = walletItems.Select(w => new WalletItemViewModel
            {
                Id = w.Id,
                Currency = w.Currency.Title,
                Amount = w.Amount
            }).ToList();

            return View(viewModel);
        }

        // POST: /UserProfiles/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditUserProfileViewModel model)
        {
            if (id != model.Id)
            {
                ModelState.AddModelError("", "ID mismatch.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var _httpClient = GetAuthenticatedHttpClient();
            // Retrieve the existing user (preserving the existing password hash if no new password provided)
            var existingProfile = await _httpClient.GetFromJsonAsync<UserProfile>($"UserProfiles/{id}");
            if (existingProfile == null)
            {
                return NotFound();
            }

            // Update fields
            existingProfile.Username = model.Username;
            existingProfile.Email = model.Email;
            existingProfile.Role = model.Role;
            existingProfile.DebugMode = model.Role is UserRole.Admin or UserRole.Developer && model.DebugMode;

            // Update password if a new one is provided.
            if (!string.IsNullOrWhiteSpace(model.NewPassword) && model.NewPassword == model.ConfirmPassword)
            {
                var hasher = new PasswordHasher<UserProfile>();
                existingProfile.PasswordHash = hasher.HashPassword(existingProfile, model.NewPassword);
            }

            // Send the updated profile to the API (adjust your HTTP client call accordingly)
            var response = await _httpClient.PutAsJsonAsync($"UserProfiles/{id}", existingProfile);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Failed to update user profile. Details: {errorContent}");
                return View(model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            using var httpClient = GetAuthenticatedHttpClient();
            var response = await httpClient.DeleteAsync($"UserProfiles/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to delete user.";
            }
            return RedirectToAction("Index");
        }
    }
}