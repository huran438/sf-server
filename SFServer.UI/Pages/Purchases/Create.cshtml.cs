using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SFServer.Shared.Server.Purchases;
using SFServer.UI;

namespace SFServer.UI.Pages.Purchases
{
    public class CreateProductModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly ProjectContext _project;

        [BindProperty(SupportsGet = true)]
        public Guid projectId { get; set; }

        public CreateProductModel(IConfiguration config, ProjectContext project)
        {
            _config = config;
            _project = project;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        private HttpClient GetClient() => User.CreateApiClient(_config, _project.CurrentProjectId);

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            using var http = GetClient();
            var response = await http.PostMessagePackAsync("Purchases/products", Product);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to create product");
                return Page();
            }
            ViewData["ClosePage"] = true;
            return Page();
        }
    }
}
