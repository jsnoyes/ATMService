using ATMService.Services.Denominations.Common;
using ATMService.Services.Denominations.Models;
using ATMService.Services.Denominations.ServiceClients;
using ATMService.Services.Shared.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATMService.Services.Denominations.Services
{
    public interface IDenominationService
    {
        /// <summary>
        /// Withdraws the requested amount of money from the ATM.
        /// </summary>
        /// <param name="requestedAmount">The total amount of money that the user requested.</param>
        /// <returns>The withdrawal history record.</returns>
        Task<string> Withdraw(int requestedAmount);

        /// <summary>
        /// Add counts to the stored denominations.
        /// </summary>
        /// <param name="stock">The counts of each denomination to add the to stored counts in the ATM.</param>
        /// <returns>The total count of each denomination existing in the ATM.</returns>
        /// <exception cref="InvalidAtmOperationException">Thrown if invalid denominations are passed in.</exception>
        IEnumerable<DenominationModel> Restock(IEnumerable<DenominationModel> stock);

        /// <summary>
        /// Get the available counts of each denomination.
        /// </summary>
        IEnumerable<DenominationModel> GetDenominations();
    }

    public class DenominationService : IDenominationService
    {
        private readonly IJSonDao<DenominationModel> _dao;
        private readonly IGatewayServiceClient _gatewayServiceClient;

        private static readonly object _lock = new object();

        public DenominationService(IJSonDao<DenominationModel> dao, IGatewayServiceClient gatewayServiceClient)
        {
            _dao = dao;
            _gatewayServiceClient = gatewayServiceClient;
        }

        public IEnumerable<DenominationModel> GetDenominations()
        {
            var denoms = _dao.Values;
            return denoms;
        }

        public IEnumerable<DenominationModel> Restock(IEnumerable<DenominationModel> stock)
        {
            lock (_lock)
            {
                var currentDenoms = GetDenominations().ToDictionary(s => s.Denomination);

                foreach (var entry in stock)
                {
                    if (entry.Count < 0)
                    {
                        throw new InvalidAtmOperationException($"Denomination {entry.Denomination} can not be stocked with a negative count.");
                    }

                    if (currentDenoms.TryGetValue(entry.Denomination, out DenominationModel denom))
                    {
                        denom.Count += entry.Count;
                    }
                    else
                    {
                        throw new InvalidAtmOperationException($"Denomination {entry.Denomination} is not a valid value.");
                    }
                }

                var finalDenoms = currentDenoms.Values.ToList();

                _dao.Update(finalDenoms);

                return finalDenoms;
            }
        }

        public async Task<string> Withdraw(int requestedAmount)
        {
            var currentAmountLeft = requestedAmount;
            bool isSuccess = false;

            lock (_lock)
            {
                var availableDenominations = GetDenominations().ToList();
                foreach (var denom in availableDenominations)
                {
                    var numUsed = currentAmountLeft / denom.Denomination; //Intentional integer truncation
                    if (numUsed > denom.Count)
                    {
                        numUsed = denom.Count;
                    }
                    currentAmountLeft -= numUsed * denom.Denomination;
                    denom.Count -= numUsed;
                }

                isSuccess = currentAmountLeft == 0;
                if (isSuccess)
                {
                    _dao.Update(availableDenominations);
                }
            }

            return await _gatewayServiceClient.AddHistory(requestedAmount, isSuccess);
        }
    }
}
