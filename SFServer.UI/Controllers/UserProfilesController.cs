using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SFServer.Shared.Server.UserProfile;
using SFServer.Shared.Server.Wallet;
using SFServer.Shared.Server.Inventory;
using SFServer.UI.Models.UserProfiles;
using SFServer.UI;

namespace SFServer.UI.Controllers
{
    public class UserProfilesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ProjectContext _project;

        public UserProfilesController(IConfiguration configuration, ProjectContext project)
        {
            _configuration = configuration;
            _project = project;
        }

        private HttpClient GetAuthenticatedHttpClient()
        {
            return User.CreateApiClient(_configuration, _project.CurrentProjectId);
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string search = "", string sortColumn = "Id", string sortOrder = "asc")
        {
            try
            {
                using var client = GetAuthenticatedHttpClient();

                if (pageSize <= 0)
                {
                    pageSize = 20;
                }

                // Retrieve profiles using MessagePack.
                var profiles = await client.GetFromMessagePackAsync<List<UserProfile>>("UserProfiles");
                if (profiles == null)
                {
                    TempData["Error"] = "Failed to load user profiles.";
                    profiles = new List<UserProfile>();
                }

                // Filter profiles by search query.
                if (!string.IsNullOrWhiteSpace(search))
                {
                    bool isGuid = Guid.TryParse(search, out Guid searchId);
                    profiles = profiles.Where(p =>
                        p.Username.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (p.Email != null && p.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (isGuid && p.Id == searchId)
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
                int totalProfiles = profiles.Count;
                int totalPages = (int)Math.Ceiling(totalProfiles / (double)pageSize);
                var pagedProfiles = profiles.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                var model = new UserProfilesIndexViewModel
                {
                    Users = pagedProfiles,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalCount = totalProfiles,
                    PageSize = pageSize,
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
                Id = Guid.CreateVersion7(),
                Username = model.Username,
                Email = model.Email,
                Role = model.Role,
                CreatedAt = DateTime.UtcNow
            };


            using var client = GetAuthenticatedHttpClient();

            // Use MessagePack for POST.
            var response = await client.PostMessagePackAsync("UserProfiles", newUser);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", new { projectId = _project.CurrentProjectId });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Failed to add user. {errorContent}");
            return View(model);
        }

        // GET: /UserProfiles/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            using var httpClient = GetAuthenticatedHttpClient();
            UserProfile profile;
            try
            {
                profile = await httpClient.GetFromMessagePackAsync<UserProfile>($"UserProfiles/{id}");
            }
            catch (ApiRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            catch (ApiRequestException ex)
            {
                TempData["Error"] = $"Failed to load user profile: {ex.Message}";
                return RedirectToAction("Index", new { projectId = _project.CurrentProjectId });
            }

            if (profile == null)
            {
                return NotFound();
            }
            
            var viewModel = new EditUserProfileViewModel
            {
                Id = profile.Id,
                Username = profile.Username,
                Email = profile.Email,
                Role = profile.Role,
                GoogleId = profile.GooglePlayId,
                DebugMode = profile.DebugMode
            };

            viewModel.DeviceIds = profile.DeviceIds?.ToArray() ?? Array.Empty<string>();

            viewModel.UserDevices = new UserDevice[viewModel.DeviceIds.Length];

            for (var i = 0; i < viewModel.DeviceIds.Length; i++)
            {
                var deviceId = viewModel.DeviceIds[i];
                var encodedId = Uri.EscapeDataString(deviceId);
                var device = await httpClient.GetFromMessagePackAsync<UserDevice>($"UserProfiles/{id}/device/{encodedId}");
                if (device == null) continue;
                viewModel.UserDevices[i] = device;
            }
            
            var walletItems = await httpClient.GetFromMessagePackAsync<List<WalletItem>>($"Wallet/{profile.Id}");

            if (walletItems != null)
            {
                viewModel.WalletItems = walletItems.Select(w => new WalletItemViewModel
                {
                    Id = w.Id,
                    Currency = w.Currency.Title,
                    Amount = w.Amount
                }).ToList();
            }
            else
            {
                viewModel.WalletItems = new List<WalletItemViewModel> { };
            }

            var playerItems = await httpClient.GetFromMessagePackAsync<List<PlayerInventoryItem>>($"Inventory/player/{profile.Id}/inventory");
            var allItems = await httpClient.GetFromMessagePackAsync<List<InventoryItem>>("Inventory");
            if (playerItems != null && allItems != null)
            {
                viewModel.InventoryItems = playerItems.Join(allItems,
                    p => p.ItemId,
                    i => i.Id,
                    (p, i) => new PlayerInventoryItemViewModel
                    {
                        ItemId = p.ItemId,
                        Amount = p.Amount,
                        Title = i.Title
                    }).ToList();
            }
            else
            {
                viewModel.InventoryItems = new List<PlayerInventoryItemViewModel> { };
            }
            ViewData["AllInventoryItems"] = allItems ?? new List<InventoryItem>();

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

            using var httpClient = GetAuthenticatedHttpClient();
            // Retrieve the existing user using MessagePack.
            UserProfile existingProfile;
            try
            {
                existingProfile = await httpClient.GetFromMessagePackAsync<UserProfile>($"UserProfiles/{id}");
            }
            catch (ApiRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            catch (ApiRequestException ex)
            {
                TempData["Error"] = $"Failed to load user profile: {ex.Message}";
                return RedirectToAction("Index");
            }

            if (existingProfile == null)
            {
                return NotFound();
            }

            // Update fields.
            existingProfile.Username = model.Username;
            existingProfile.Email = model.Email;
            existingProfile.Role = model.Role;
            existingProfile.DebugMode = model.Role is UserRole.Admin or UserRole.Developer && model.DebugMode;


            // Use MessagePack for PUT.
            var response = await httpClient.PutAsMessagePackAsync($"UserProfiles/{id}", existingProfile);
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

            return RedirectToAction("Index", new { projectId = _project.CurrentProjectId });
        }
        
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateWallet(WalletUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid wallet data.";
                // Set the active tab to "wallet"
                TempData["activeTab"] = "wallet";
                return RedirectToAction("Edit", new { id = model.UserId });
            }

            using var client = GetAuthenticatedHttpClient();

            foreach (var item in model.WalletItems)
            {
                var response = await client.PutAsMessagePackAsync($"Wallet/{item.WalletItemId}", new WalletUpdateDto
                {
                    Id = item.WalletItemId,
                    Amount = item.Amount
                });
                
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Failed to update wallet item {item.WalletItemId}: {errorMsg}";
                    // You may choose to stop processing or continue.
                }
            }

            TempData["Success"] = "Wallet updated successfully.";
            // Set the active tab to "wallet"
            TempData["activeTab"] = "wallet";
            return RedirectToAction("Edit", new { projectId = _project.CurrentProjectId, id = model.UserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInventory(InventoryUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid inventory data.";
                TempData["activeTab"] = "inventory";
                return RedirectToAction("Edit", new { projectId = _project.CurrentProjectId, id = model.UserId });
            }

            using var client = GetAuthenticatedHttpClient();

            var items = model.Items
                .Where(i => i.ItemId.HasValue && i.Amount > 0)
                .Select(i => new PlayerInventoryItem { ItemId = i.ItemId!.Value, Amount = i.Amount })
                .ToList();

            await client.PutAsMessagePackAsync($"Inventory/player/{model.UserId}/inventory", items);

            TempData["Success"] = "Inventory updated successfully.";
            TempData["activeTab"] = "inventory";
            return RedirectToAction("Edit", new { projectId = _project.CurrentProjectId, id = model.UserId });
        }
    }
}