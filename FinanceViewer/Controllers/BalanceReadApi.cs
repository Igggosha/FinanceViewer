using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace FinanceViewer.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class BalanceReadApi : ControllerBase
    {
        private static HttpClient httpClient = new();
        private delegate Task<int> fetchFunction(string accountId);
        
        private static Dictionary<string, fetchFunction?> FetchFunctions = new()
        {
            ["BTC"] = async (string address) =>
            {
                var json = await httpClient.GetStringAsync($"https://blockstream.info/api/address/{address}");
                using var doc = JsonDocument.Parse(json);

                int txCount = doc.RootElement
                    .GetProperty("chain_stats")
                    .GetProperty("tx_count")
                    .GetInt32();

                return txCount;
            },
            
        };


        [HttpGet("balance")]
        async public Task<ActionResult<int>> GetBalance(string? address, string? currency)
        {
            if (currency != null && address != null && FetchFunctions.ContainsKey(currency) && FetchFunctions[currency] != null)
            {
                return await FetchFunctions[currency](address);
            }

            return BadRequest();
        }
    }
}
