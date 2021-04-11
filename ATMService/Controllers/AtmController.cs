﻿using ATMService.Core.Common;
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

        [HttpPost("/withdraw/{requestedAmount}")]
        public ActionResult Withdraw(int requestedAmount)
        {
            try
            {
                var history = _denomService.Withdraw(requestedAmount);

                return Ok(FormatTransaction(history));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

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
