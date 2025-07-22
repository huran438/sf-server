using Google.Apis.AndroidPublisher.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFServer.Shared.Client.Purchases;
using SFServer.API.Data;
using Microsoft.EntityFrameworkCore;
using SFServer.Shared.Server.Purchases;

namespace SFServer.API.Controllers;

[ApiController]
[Route("{projectId:guid}/[controller]")]
[Authorize]
public class PurchasesController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly DatabseContext _db;

    public PurchasesController(IConfiguration config, DatabseContext db)
    {
        _config = config;
        _db = db;
    }

    [Authorize(Roles = "Admin,Developer")]
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(Guid projectId)
    {
        var list = await _db.Products.Where(p => p.ProjectId == projectId).ToListAsync();
        return Ok(list);
    }

    [Authorize(Roles = "Admin,Developer")]
    [HttpGet("products/{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid projectId, Guid id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.ProjectId == projectId);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [Authorize(Roles = "Admin,Developer")]
    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct(Guid projectId, [FromBody] Product product)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _db.Products.AnyAsync(p => p.ProjectId == projectId && p.ProductId == product.ProductId))
            return Conflict("ProductId already exists");

        product.ProjectId = projectId;
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [Authorize(Roles = "Admin,Developer")]
    [HttpPut("products/{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid projectId, Guid id, [FromBody] Product product)
    {
        if (id != product.Id) return BadRequest();
        var existing = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.ProjectId == projectId);
        if (existing == null) return NotFound();

        if (await _db.Products.AnyAsync(p => p.ProjectId == projectId && p.ProductId == product.ProductId && p.Id != id))
            return Conflict("ProductId already exists");

        _db.Entry(existing).CurrentValues.SetValues(product);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin,Developer")]
    [HttpDelete("products/{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid projectId, Guid id)
    {
        var existing = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.ProjectId == projectId);
        if (existing == null) return NotFound();
        _db.Products.Remove(existing);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("player/{userId}/purchases")]
    public async Task<IActionResult> GetPlayerPurchases(Guid projectId, Guid userId)
    {
        var list = await _db.PlayerPurchases
            .Where(p => p.UserId == userId)
            .Include(p => p.Product)
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost("validate-android")]
    public async Task<IActionResult> ValidateAndroid(Guid projectId, [FromBody] AndroidPurchaseValidationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var settings = await _db.ProjectSettings.FirstOrDefaultAsync(s => s.ProjectId == projectId);
        var credJson = settings?.GoogleServiceAccountJson;
        if (string.IsNullOrEmpty(credJson))
            credJson = _config["GOOGLE_SERVICE_ACCOUNT_JSON"];
        if (string.IsNullOrEmpty(credJson))
            return StatusCode(500, "Service account credentials not configured");

        GoogleCredential credential;
        using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(credJson)))
        {
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped(AndroidPublisherService.Scope.Androidpublisher);
        }

        var service = new AndroidPublisherService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "SFServer"
        });

        try
        {
            var result = await service.Purchases.Products
                .Get(request.PackageName, request.ProductId, request.PurchaseToken)
                .ExecuteAsync();

            if (result.PurchaseState == 0)
            {
                var userIdValue = Request.Headers[Headers.UID].FirstOrDefault();
                if (string.IsNullOrEmpty(userIdValue))
                {
                    userIdValue = User.FindFirst("UserId")?.Value;
                }
                if (Guid.TryParse(userIdValue, out var userId))
                {
                    var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == request.ProductId && p.ProjectId == projectId);
                    if (product != null)
                    {
                        _db.PlayerPurchases.Add(new PlayerPurchase
                        {
                            Id = Guid.CreateVersion7(),
                            UserId = userId,
                            ProductId = product.Id,
                            PurchasedAt = DateTime.UtcNow
                        });
                        await _db.SaveChangesAsync();
                    }
                }
            }

            return Ok(new AndroidPurchaseValidationResponse
            {
                PackageName = request.PackageName,
                ProductId = request.ProductId,
                IsValid = result.PurchaseState == 0,
                Acknowledged = result.AcknowledgementState == 1,
                PurchaseState = result.PurchaseState ?? 0,
                ConsumptionState = result.ConsumptionState ?? 0,
                PurchaseTimeMillis = result.PurchaseTimeMillis ?? 0
            });
        }
        catch (Google.GoogleApiException ex)
        {
            return BadRequest(new AndroidPurchaseValidationResponse
            {
                PackageName = request.PackageName,
                ProductId = request.ProductId,
                IsValid = false,
                Error = ex.Message
            });
        }
    }
}
