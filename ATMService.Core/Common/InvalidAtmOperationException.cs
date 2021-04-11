using System;
using System.Collections.Generic;
using System.Text;

namespace ATMService.Core.Common
{
    public class InvalidAtmOperationException : InvalidOperationException
    {
        public InvalidAtmOperationException(string message) : base(message)
        {
        }
    }
}
