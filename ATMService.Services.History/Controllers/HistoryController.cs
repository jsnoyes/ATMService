using ATMService.Services.History.Models;
using ATMService.Services.Shared.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace ATMService.Services.History.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        private readonly IJSonDao<HistoryModel> _dao;

        public HistoryController(IJSonDao<HistoryModel> dao)
        {
            _dao = dao;
        }

        [HttpGet("/history")]
        public ActionResult GetHistories()
        {
            try
            {
                var histories = _dao.Values
                    .OrderByDescending(s => s.ExecutionTime)
                    .Select(s => FormatTransaction(s))
                    .ToList();
                return Ok(histories);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("/history")]
        public ActionResult CreateHistory([FromQuery] int amount, [FromQuery] bool isSuccess)
        {
            try
            {
                var history = AddHistory(amount, isSuccess, DateTime.Now);
                return Ok(FormatTransaction(history));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private string FormatTransaction(HistoryModel history)
        {
            return history.IsSuccess
                ? $"Dispensed ${history.TotalAmount}"
                : $"Insufficient Funds";
        }

        private HistoryModel AddHistory(int amount, bool isSuccess, DateTime executionTime)
        {
            var history = new HistoryModel { TotalAmount = amount, IsSuccess = isSuccess, ExecutionTime = executionTime };
            _dao.Add(history);
            return history;
        }
    }
}
