using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ATMService.Services.Denominations.ServiceClients
{
    public interface IGatewayServiceClient
    {
        Task<string> AddHistory(int amount, bool isSuccess);
    }

    public class GatewayServiceClient : IGatewayServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _gatewayUrl;
        public GatewayServiceClient(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient();
            _gatewayUrl = config["urls:gateway"];
        }

        public async Task<string> AddHistory(int amount, bool isSuccess)
        {
            var postResult = await _httpClient.PostAsync($"{_gatewayUrl}/history?amount={amount}&isSuccess={isSuccess}", null);
            return await postResult.Content.ReadAsStringAsync();
        }
    }
}
