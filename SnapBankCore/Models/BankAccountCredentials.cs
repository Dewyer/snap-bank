using System;
using System.Collections.Generic;
using System.Text;

namespace SnapBank.Core.Models
{
    public class BankAccountCredentials
    {
        public string SecretLogin { get; set; }
        public string SecretSalt { get; set; }

        public string LastOperationalToken { get; set; }
        public DateTime TokenExpiration { get; set; }
    }
}
