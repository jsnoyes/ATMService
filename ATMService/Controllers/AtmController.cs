using ATMService.Core.Common;
using ATMService.Core.Models;
using ATMService.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATMService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AtmController : ControllerBase
    {
        private readonly IDenominationService _denomService;

        public AtmController(IDenominationService denomService)
        {
            _denomService = denomService;
        }

        /// <summary>
        /// Withdraw the requested amount from the ATM.
        /// </summary>
        /// <param name="requestedAmount">Dollar amount requested from the ATM.</param>
        /// <returns>Description of the action that the ATM took.</returns>
        [HttpPost("/withdraw/{requestedAmount}")]
        public ActionResult Withdraw(int requestedAmount)
        {
            try
            {
                if (requestedAmount < 0)
                    throw new InvalidAtmOperationException($"You may not withdraw negative amounts. Requested amount: {requestedAmount}");

                var history = _denomService.Withdraw(requestedAmount);

                return Ok(FormatTransaction(history));
            }
            catch (InvalidAtmOperationException iaoe)
            {
                return BadRequest(iaoe.Message);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get descriptions of the full history of withdrawals at the ATM.
        /// </summary>
        [HttpGet("/history")]
        public ActionResult GetHistories()
        {
            try
            {
                var histories = _denomService.GetHistory()
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

        /// <summary>
        /// Get the available counts of each denomination.
        /// </summary>
        [HttpGet("/overview")]
        public ActionResult GetOverview()
        {
            try
            {
                return Ok(_denomService.GetDenominations());
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Add counts to the stored denominations.
        /// </summary>
        /// <param name="stock">The counts of each denomination to add the to stored counts in the ATM.</param>
        /// <returns>The total count of each denomination existing in the ATM.</returns>
        [HttpPost("/restock")]
        public ActionResult Restock([FromBody] List<DenominationModel> denominations)
        {
            try
            {
                var newDenoms = _denomService.Restock(denominations);
                return Ok(newDenoms);
            }
            catch(InvalidAtmOperationException iaoe)
            {
                return BadRequest(iaoe.Message);
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
    }
}
