using System.Text;
using System.Text.Json;

namespace BillingService.Services;

public record DeductStockRequest(int ProductId, int Quantity);

public class InventoryClient(HttpClient httpClient, ILogger<InventoryClient> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<(bool Success, string Error)> DeductStockAsync(int productId, int quantity)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new DeductStockRequest(productId, quantity));
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("api/products/deduct-stock", content);

            if (response.IsSuccessStatusCode)
                return (true, string.Empty);

            var body = await response.Content.ReadAsStringAsync();
            return (false, $"Inventory service error: {body}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to contact inventory service");
            return (false, "Serviço de estoque indisponível. Tente novamente em instantes.");
        }
    }
}