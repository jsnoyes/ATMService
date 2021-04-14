using ATMService.Services.Denominations.Common;
using ATMService.Services.Denominations.Models;
using ATMService.Services.Denominations.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATMService.Services.Denominations.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DenominationController : ControllerBase
    {
        private readonly IDenominationService _denomService;

        public DenominationController(IDenominationService denomService)
        {
            _denomService = denomService;
        }

        /// <summary>
        /// Withdraw the requested amount from the ATM.
        /// </summary>
        /// <param name="requestedAmount">Dollar amount requested from the ATM.</param>
        /// <returns>Description of the action that the ATM took.</returns>
        [HttpPost("/withdraw/{requestedAmount}")]
        public async Task<ActionResult> Withdraw(int requestedAmount)
        {
            try
            {
                if (requestedAmount < 0)
                    throw new InvalidAtmOperationException($"You may not withdraw negative amounts. Requested amount: {requestedAmount}");

                var response = await _denomService.Withdraw(requestedAmount);

                return Ok(response);
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

        [HttpPost("/restock")]
        public ActionResult Restock([FromBody] List<DenominationModel> denominations)
        {
            try
            {
                var newDenoms = _denomService.Restock(denominations);
                return Ok(newDenoms);
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
    }
}
