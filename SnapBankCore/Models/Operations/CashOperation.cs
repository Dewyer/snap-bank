using System;
using System.Collections.Generic;
using System.Text;

namespace SnapBank.Core.Models.Operations
{
    public class CashOperation : AuthenticatedOperation
    {
        public string ToIban { get; set; }
        public Currency Money { get; set; }
        public CashOperationType OperationType { get; set; }
    }

    public enum CashOperationType
    {
        Deposit,
        Withdraw
    }
}
