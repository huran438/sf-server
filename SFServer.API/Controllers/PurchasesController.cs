using Google.Apis.AndroidPublisher.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFServer.Shared.Client.Purchases;
using SFServer.API.Data;
using Microsoft.EntityFrameworkCore;
using SFServer.Shared.Server.Inventory;

namespace SFServer.API.Controllers;

[ApiController]
[Route("[controller]")]
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

    [HttpPost("validate-android")]
    public async Task<IActionResult> ValidateAndroid([FromBody] AndroidPurchaseValidationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var settings = await _db.ServerSettings.FirstOrDefaultAsync();
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
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    var item = await _db.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == request.ProductId);
                    if (item != null)
                    {
                        var existing = await _db.PlayerInventoryItems.FirstOrDefaultAsync(p => p.UserId == userId && p.ItemId == item.Id);
                        if (existing != null)
                            existing.Amount += 1;
                        else
                        {
                            _db.PlayerInventoryItems.Add(new PlayerInventoryItem
                            {
                                Id = Guid.NewGuid(),
                                UserId = userId,
                                ItemId = item.Id,
                                Amount = 1
                            });
                        }
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
