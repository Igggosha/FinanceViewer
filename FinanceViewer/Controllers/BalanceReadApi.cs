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
        
        // NOT using .env because i dont want to bother
        private const string ETH_API_KEY = "2N2STTS9NTDC4972WV4NFXUPXFHMX9X9TI"; 
        private delegate Task<decimal> fetchFunction(string accountId);
        
        private static Dictionary<string, fetchFunction?> FetchFunctions = new()
        {
            ["BTC"] = async (string address) =>
            {
                var json = await httpClient.GetStringAsync($"https://blockstream.info/api/address/{address}");
                using var doc = JsonDocument.Parse(json);

                decimal txCount = doc.RootElement
                    .GetProperty("chain_stats")
                    .GetProperty("tx_count")
                    .GetDecimal();

                return txCount;
            },
            ["ETH"] = async (string address) =>
            {
                var json = await httpClient.GetStringAsync($"https://api.etherscan.io/v2/api?apikey={ETH_API_KEY}&address={address}&chainid=1&module=account&action=balance");
                using var doc = JsonDocument.Parse(json);

                decimal txCount = decimal.Parse(doc.RootElement
                    .GetProperty("result")
                    .GetString());

                return txCount;
            },
            
        };


        [HttpGet("balance")]
        async public Task<ActionResult<decimal>> GetBalance(string? address, string? currency)
        {
            if (currency != null && address != null && FetchFunctions.ContainsKey(currency.ToUpper()) && FetchFunctions[currency.ToUpper()] != null)
            {
                return await FetchFunctions[currency.ToUpper()](address);
            }

            return BadRequest();
        }
    }
}
