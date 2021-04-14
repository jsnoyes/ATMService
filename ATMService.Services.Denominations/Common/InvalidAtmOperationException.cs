using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATMService.Services.Denominations.Common
{
    public class InvalidAtmOperationException : InvalidOperationException
    {
        public InvalidAtmOperationException(string message) : base(message)
        {
        }
    }
}
