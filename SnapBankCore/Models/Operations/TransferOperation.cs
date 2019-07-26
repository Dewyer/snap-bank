using System;
using System.Collections.Generic;
using System.Text;

namespace SnapBank.Core.Models.Operations
{
    public class TransferOperation : AuthenticatedOperation
    {
        public string ToIban { get; set; }
        public string Message { get; set; }
        public Currency Money { get; set; }
    }
}
