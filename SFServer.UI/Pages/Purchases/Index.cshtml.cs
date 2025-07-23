using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SFServer.Shared.Server.Purchases;
using SFServer.UI;

namespace SFServer.UI.Pages.Purchases
{
    public class PurchasesPageModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly ProjectContext _project;

        [BindProperty(SupportsGet = true)]
        public Guid projectId { get; set; }

        public PurchasesPageModel(IConfiguration config, ProjectContext project)
        {
            _config = config;
            _project = project;
        }

        public List<Product> Products { get; set; } = new();

        private HttpClient GetClient() => User.CreateApiClient(_config, _project.CurrentProjectId);

        public async Task OnGetAsync()
        {
            using var http = GetClient();
            Products = await http.GetFromMessagePackAsync<List<Product>>("Purchases/products");
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            using var http = GetClient();
            await http.DeleteAsync($"Purchases/products/{id}");
            return RedirectToPage(new { projectId });
        }
    }
}
