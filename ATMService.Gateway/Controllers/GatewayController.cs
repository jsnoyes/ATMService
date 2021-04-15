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

        /// <summary>
        /// Get the available counts of each denomination.
        /// </summary>
        [HttpGet("/overview")]
        public async Task<IActionResult> GetOverview() => await ProxyTo(_getOverviewUrl);

        /// <summary>
        /// Get descriptions of the full history of withdrawals at the ATM.
        /// </summary>
        [HttpGet("/history")]
        public async Task<IActionResult> GetHistory() => await ProxyTo(_getHistoryUrl);

        /// <summary>
        /// Create new history record for ATM withdrawals.
        /// </summary>
        /// <param name="amount">The amount that was requested to be withdrawn.</param>
        /// <param name="isSuccess">Whether the request could be fulfilled.</param>
        /// <returns>The history record that was created.</returns>
        [HttpPost("/history")]
        public async Task<IActionResult> AddHistory([FromQuery] int amount, [FromQuery] bool isSuccess) => await ProxyPostTo($"{_addHistoryUrl}?amount={amount}&isSuccess={isSuccess}", HttpContext.Request);

        /// <summary>
        /// Withdraw the requested amount from the ATM.
        /// </summary>
        /// <param name="requestedAmount">Dollar amount requested from the ATM.</param>
        /// <returns>Description of the action that the ATM took.</returns>
        [HttpPost("/withdraw/{requestedAmount}")]
        public async Task<IActionResult> Withdraw(int requestedAmount) => await ProxyPostTo($"{_withdrawUrl}/{requestedAmount}", HttpContext.Request);


        /// <summary>
        /// Add counts to the stored denominations.
        /// </summary>
        /// <param name="stock">The counts of each denomination to add the to stored counts in the ATM.</param>
        /// <returns>The total count of each denomination existing in the ATM.</returns>
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
