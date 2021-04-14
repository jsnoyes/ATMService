using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ATMService.Gateway.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GatewayController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _getOverviewUrl;
        private readonly string _getHistoryUrl;
        private readonly string _withdrawUrl;
        private readonly string _restockUrl;
        private readonly string _addHistoryUrl;

        public GatewayController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient();
            _getOverviewUrl = config["urls:getOverview"];
            _getHistoryUrl = config["urls:getHistory"];
            _withdrawUrl = config["urls:withdraw"];
            _restockUrl = config["urls:restock"];
            _addHistoryUrl = config["urls:addHistory"];
        }

        [HttpGet("/overview")]
        public async Task<IActionResult> GetOverview() => await ProxyTo(_getOverviewUrl);

        [HttpGet("/history")]
        public async Task<IActionResult> GetHistory() => await ProxyTo(_getHistoryUrl);

        [HttpPost("/history")]
        public async Task<IActionResult> AddHistory([FromQuery] int amount, [FromQuery] bool isSuccess) => await ProxyPostTo($"{_addHistoryUrl}?amount={amount}&isSuccess={isSuccess}", HttpContext.Request);

        [HttpPost("/withdraw/{requestedAmount}")]
        public async Task<IActionResult> Withdraw(int requestedAmount) => await ProxyPostTo($"{_withdrawUrl}/{requestedAmount}", HttpContext.Request);

        [HttpPost("/restock")]
        public async Task<IActionResult> Restock() => await ProxyPostTo(_restockUrl, HttpContext.Request);

        private async Task<ContentResult> ProxyTo(string url) => Content(await _httpClient.GetStringAsync(url));
        private async Task<ContentResult> ProxyPostTo(string url, HttpRequest request)
        {
            using(var content = new StreamContent(request.Body))
            {
                request.Headers.ToList().ForEach(h => content.Headers.TryAddWithoutValidation(h.Key, h.Value.ToList()));
                var postResult = await _httpClient.PostAsync(url, content);
                var res = await postResult.Content.ReadAsStringAsync();
                return Content(res);
            }
        }
    }
}
