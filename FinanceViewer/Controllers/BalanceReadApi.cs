using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text;

namespace FinanceViewer.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    
    
    
    public class BalanceReadApi : ControllerBase
    {
        private static HttpClient httpClient = new();
        
        private readonly IConfiguration _configuration;
        
        // NOT using .env because i dont want to bother
        private const string ETH_API_KEY = "2N2STTS9NTDC4972WV4NFXUPXFHMX9X9TI";
        //_configuration["ETH_API_KEY"]; //
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

                return txCount / 1_000_000_000_000_000_000;
            },
            ["SOL"] = async (string address) =>
            {
                var rpcUrl = "https://api.mainnet-beta.solana.com";

                using var client = new HttpClient();

                var payload = new
                {
                    jsonrpc = "2.0",
                    id = 1,
                    method = "getBalance",
                    @params = new object[]
                    {
                        address
                    }
                };

                var json = JsonSerializer.Serialize(payload);

                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(rpcUrl, content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseJson);

                // Result is in lamports
                long lamports = doc.RootElement
                    .GetProperty("result")
                    .GetProperty("value")
                    .GetInt64();

                return lamports / 1_000_000_000m;
            }
            
        };


        [HttpGet("balance")]
        async public Task<ActionResult<object>> GetBalance(string? address, string? currency)
        {
            if (currency != null && address != null && FetchFunctions.ContainsKey(currency.ToUpper()) && FetchFunctions[currency.ToUpper()] != null)
            {
                return new
                {
                    result = await FetchFunctions[currency.ToUpper()](address)
                };
            }

            return BadRequest();
        }

        // public BalanceReadApi(IConfiguration configuration)
        // {
        //     _configuration = configuration;
        // }
    }
}
