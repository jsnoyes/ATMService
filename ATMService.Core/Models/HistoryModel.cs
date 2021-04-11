using System;
using System.Collections.Generic;
using System.Text;

namespace ATMService.Core.Models
{
    public class HistoryModel
    {
        public int TotalAmount { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime ExecutionTime { get; set; }
    }
}
