using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATMService.Services.History.Models
{
    public class HistoryModel
    {
        public int TotalAmount { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime ExecutionTime { get; set; }
    }
}
