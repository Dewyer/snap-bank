using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SnapBank.Core.Models
{
    public enum TransactionDirection
    {
        Incoming,
        Outgoing
    }

    public enum TransactionType
    {
        CashDeposit,
        CashWithdrawal,
        InterbankTransaction
    }

    public class BankTransaction
    {

        [BsonId]
        public string Id { get; set; }
        public string Message { get; set; }
        public string OtherPartyIban { get; set; }

        [BsonRepresentation(BsonType.String)]
        public TransactionDirection Direction { get; set; }

        [BsonRepresentation(BsonType.String)]
        public TransactionType TransactionType { get; set; }

        public Currency Money { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
