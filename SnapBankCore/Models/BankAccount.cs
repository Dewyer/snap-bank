using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace SnapBank.Core.Models
{
    public class BankAccount
    {
        [BsonId]
        public string Id { get; set; }
        public string HolderName { get; set; }
        public BankAccountCredentials Credentials { get; set; }

        public string Iban { get; set; }

        public List<BankTransaction> TransactionHistory { get; set; }

    }
}
